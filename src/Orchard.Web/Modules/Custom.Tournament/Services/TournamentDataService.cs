using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CsQuery;
using Custom.Tournament.Helpers;
using Custom.Tournament.Models;
using Custom.Tournament.Models.Dto;
using Orchard;
using Orchard.Data;

namespace Custom.Tournament.Services
{
    public interface ITournamentDataService : IDependency
    {
        IEnumerable<RatingRecord> GetData();

        IEnumerable<PlayerRecord> GetPlayers();

        IEnumerable<TourRecord> GetTours();

        IEnumerable<Task<IEnumerable<RatingRecord>>> GetRatings();

        Task<IEnumerable<RatingRecord>> GetRatingsByTour(TourRecord tour);

        IEnumerable<Task<LeagueAndTourDto>> GetLeaguesAndLeagueInTours();

        Task<LeagueAndTourDto> GetLeaguesAndLeagueInToursByTour(TourRecord tour);

        IEnumerable<Task<GroupAndGameDto>> GetGroupAndGames();

        void ClearCache();
    }

    public class TournamentDataService : ITournamentDataService
    {
        private readonly IRepository<PlayerRecord> _playerRepository;
        private readonly IRepository<TourRecord> _tourRepository;
        private readonly IRepository<LeagueRecord> _leagueRepository;
        private readonly IRepository<LeagueInTourRecord> _leagueInTourRepository;

        private CacheHelper _cacheHelper;
        private readonly string _groupPagesDirectoryName = "Groups";
        private readonly string _ratingPagesDirectoryName = "Ratings";
        private readonly string _groupPageNameFormat = "group-{0}.html";
        private readonly string _ratingPageNameFormat = "rating-{0}.html";


        public TournamentDataService(IRepository<PlayerRecord> playerRepository, IRepository<TourRecord> tourRepository, IRepository<LeagueRecord> leagueRepository, IRepository<LeagueInTourRecord> leagueInTourRepository)
        {
            _playerRepository = playerRepository;
            _tourRepository = tourRepository;
            _leagueRepository = leagueRepository;
            _leagueInTourRepository = leagueInTourRepository;

            _cacheHelper = new CacheHelper(HttpContext.Current.Server.MapPath(@"~/") + @"/App_Data/TableTennisSiteData");
        }

        public IEnumerable<RatingRecord> GetData()
        {
            var req = HttpWebRequest.Create("http://tabletennis.by");
            req.Method = "GET";

            string source;
            using (var reader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                source = reader.ReadToEnd();
            }

            File.WriteAllText(HttpContext.Current.Server.MapPath(@"~/") + @"/App_Data/TableTennisSiteData/tabletennis-html.html", source);

            CQ html = source;

            var cq = html["div[id='tour'] > table > tbody > tr[class!='league']"];

            var ratingRecords = new List<RatingRecord>();

            //foreach (var el in cq.Elements)
            //{
            //    var tdPlace = ((CQ) el.InnerHTML)["td.place"].FirstElement();
            //    var place = Int32.Parse(tdPlace.InnerText);

            //    var tdPlaceInc = ((CQ)el.InnerHTML)["td.place-i"].FirstElement();
            //    var placeInc = 0;
            //    if (!String.IsNullOrEmpty(tdPlaceInc.InnerHTML.Trim('\n')))
            //    {
            //        var spanPlaceInc = ((CQ) tdPlaceInc.InnerHTML)["span"].FirstElement();
            //        placeInc = Int32.Parse(spanPlaceInc.InnerText);
            //        if (spanPlaceInc.Attributes["class"] == "diff decr")
            //        {
            //            placeInc = -placeInc;
            //        }
            //    }

            //    var aPlayer = ((CQ)el.InnerHTML)["td.player > a"].FirstElement();
            //    var player = aPlayer.InnerText;

            //    var tdRating = ((CQ)el.InnerHTML)["td.rating"].FirstElement();
            //    var rating = Double.Parse(tdRating.InnerText, CultureInfo.InvariantCulture);

            //    var tdRatingInc = ((CQ)el.InnerHTML)["td.rating-i"].FirstElement();
            //    var ratingInc = 0.0;
            //    if (!String.IsNullOrEmpty(tdRatingInc.InnerHTML.Trim('\n')))
            //    {
            //        var spanRatingInc = ((CQ)tdRatingInc.InnerHTML)["span"].FirstElement();
            //        ratingInc = Double.Parse(spanRatingInc.InnerText, CultureInfo.InvariantCulture);
            //        if (spanRatingInc.Attributes["class"] == "diff decr")
            //        {
            //            ratingInc = -ratingInc;
            //        }
            //    }

            //    ratingRecords.Add(new RatingRecord {Place = place, PlaceIncrement = placeInc, Player = player, Rating = rating, RatingIncrement = ratingInc});
            //}

            return ratingRecords;
        }

        public IEnumerable<PlayerRecord> GetPlayers()
        {
            var req = HttpWebRequest.Create("http://tabletennis.by/players");
            req.Method = "GET";

            string source;
            using (var reader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                source = reader.ReadToEnd();
            }

            //File.WriteAllText(HttpContext.Current.Server.MapPath(@"~/") + @"/App_Data/TableTennisSiteData/tabletennis-players.html", source);

            CQ html = source;

            var cq = html["dl.tabs > dd.tab-content:first > ul.players-list-wide > li:not([class])"];

            var players = new List<PlayerRecord>();
            foreach (var el in cq.Elements)
            {
                var aPlayer = ((CQ)el.InnerHTML)["a[href]"].FirstElement();
                var player = aPlayer.InnerText;
                players.Add(new PlayerRecord { Name = player });
            }

            return players;
        }

        public IEnumerable<TourRecord> GetTours()
        {
            var req = HttpWebRequest.Create("http://tabletennis.by/tours");
            req.Method = "GET";

            string source;
            using (var reader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                source = reader.ReadToEnd();
            }

            File.WriteAllText(HttpContext.Current.Server.MapPath(@"~/") + @"/App_Data/TableTennisSiteData/tabletennis-tours.html", source);

            CQ html = source;

            var cq = html["table#tours > tbody > tr"];

            var tours = new List<TourRecord>();
            foreach (var el in cq.Elements)
            {
                var tdTour = ((CQ)el.InnerHTML)["td.date"].FirstElement();
                var strDate = tdTour["data-order-by"];

                var date = DateTime.Parse(strDate);

                tours.Add(new TourRecord { Date = date });
            }

            return tours;
        }

        public IEnumerable<Task<IEnumerable<RatingRecord>>> GetRatings()
        {
            var ratingRecordsTasks = new List<Task<IEnumerable<RatingRecord>>>();

            foreach (var tour in _tourRepository.Table.ToList())
            {
                while (ratingRecordsTasks.Count(x => x.IsCompleted == false) >= 10)
                {
                    Thread.Sleep(200);
                }
                var tourRatingRecordsTask = GetRatingsByTour(tour);
                ratingRecordsTasks.Add(tourRatingRecordsTask);
            }

            return ratingRecordsTasks;
        }

        public async Task<IEnumerable<RatingRecord>> GetRatingsByTour(TourRecord tour)
        {
            var strDate = tour.Date.ToString("yyyy-MM-dd");

            bool f = false;

            string source = String.Empty;

            var cacheFileName = GetFileName(_ratingPageNameFormat, strDate);

            if (!_cacheHelper.ExistInCache(_ratingPagesDirectoryName, cacheFileName))
            {
                do
                {
                    try
                    {
                        var req = HttpWebRequest.Create(String.Format("http://tabletennis.by/tours/{0}", strDate));
                        req.Method = "GET";

                        var response = await req.GetResponseAsync().ConfigureAwait(false);

                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            source = reader.ReadToEnd();

                            if (!_cacheHelper.ExistInCache(_ratingPagesDirectoryName, cacheFileName))
                            {
                                _cacheHelper.SaveDataToCache(_ratingPagesDirectoryName, cacheFileName, source);
                            }

                            f = true;
                        }
                    }
                    catch (WebException)
                    {}
                    catch (Exception)
                    {}
                } while (f == false);
            }
            else
            {
                source = _cacheHelper.GetCachedData(_ratingPagesDirectoryName, cacheFileName);
            }

            CQ html = source;

            var cq = html["div[id='tour'] > table > tbody > tr[class!='league']"];

            var ratingRecords = new List<RatingRecord>();
            var lastPlace = 0;

            foreach (var el in cq.Elements)
            {
                var tdPlace = ((CQ)el.InnerHTML)["td.place"].FirstElement();
                var place = Int32.Parse(tdPlace.InnerText);

                var aPlayer = ((CQ)el.InnerHTML)["td.player > a"].FirstElement();
                var player = aPlayer.InnerText;

                var tdRating = ((CQ)el.InnerHTML)["td.rating"].FirstElement();
                var rating = Double.Parse(tdRating.InnerText, CultureInfo.InvariantCulture);

                var isInArchive = el.HasClass("arch");

                ratingRecords.Add(new RatingRecord { TourRecord = tour, PlayerRecord = new PlayerRecord { Name = player }, Place = place, Rating = rating , IsInArchive = isInArchive});
            }

            if (ratingRecords.Count > 0)
            {
                lastPlace = ratingRecords.Last().Place;
            }

            var cqZero = html["div.nil-ratings-list > a[href]"];

            foreach (var el in cqZero.Elements)
            {
                var player = el.InnerText;

                ratingRecords.Add(new RatingRecord { TourRecord = tour, PlayerRecord = new PlayerRecord { Name = player }, Place = ++lastPlace, Rating = 0 });
            }

            return ratingRecords;
        }

        public IEnumerable<Task<LeagueAndTourDto>> GetLeaguesAndLeagueInTours()
        {
            var leagueAndTourDtoTasks = new List<Task<LeagueAndTourDto>>();

            foreach (var tour in _tourRepository.Table.ToList())
            {
                while (leagueAndTourDtoTasks.Count(x => x.IsCompleted == false) >= 10)
                {
                    Thread.Sleep(200);
                }
                var leagueAndTourDtoTask = GetLeaguesAndLeagueInToursByTour(tour);
                leagueAndTourDtoTasks.Add(leagueAndTourDtoTask);
            }

            return leagueAndTourDtoTasks;
        }

        public async Task<LeagueAndTourDto> GetLeaguesAndLeagueInToursByTour(TourRecord tour)
        {
            var strDate = tour.Date.ToString("yyyy-MM-dd");

            bool f = false;

            string source = String.Empty;

            var cacheFileName = GetFileName(_groupPageNameFormat, strDate);

            if (!_cacheHelper.ExistInCache(_groupPagesDirectoryName, cacheFileName))
            {
                do
                {
                    try
                    {
                        var req = HttpWebRequest.Create(String.Format("http://tabletennis.by/tours/{0}/groups", strDate));
                        req.Method = "GET";

                        var response = await req.GetResponseAsync().ConfigureAwait(false);

                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            source = reader.ReadToEnd();

                            if (!_cacheHelper.ExistInCache(_groupPagesDirectoryName, cacheFileName))
                            {
                                _cacheHelper.SaveDataToCache(_groupPagesDirectoryName, cacheFileName, source);
                            }

                            f = true;
                        }
                    }
                    catch (WebException)
                    {}
                    catch (Exception)
                    {}
                } while (f == false);
            }
            else
            {
                source = _cacheHelper.GetCachedData(_groupPagesDirectoryName, cacheFileName);
            }

            CQ html = source;

            var cq = html["dl[class='tabs groups-tabs'] > dt[data-tab]"];

            var leagueRecords = new List<LeagueRecord>();
            var leagueInTourRecords = new List<LeagueInTourRecord>();

            foreach (var el in cq.Elements)
            {
                var dtLeagueName = HttpUtility.HtmlDecode(el.InnerText);

                var leagueRecord = new LeagueRecord { Name = dtLeagueName };
                leagueRecords.Add(leagueRecord);
                leagueInTourRecords.Add(new LeagueInTourRecord { LeagueRecord = leagueRecord, TourRecord = tour });
            }

            return new LeagueAndTourDto { LeagueRecords = leagueRecords, LeagueInTourRecords = leagueInTourRecords };
        }

        public IEnumerable<Task<GroupAndGameDto>> GetGroupAndGames()
        {
            var groupAndGameDtoTasks = new List<Task<GroupAndGameDto>>();

            foreach (var tour in _tourRepository.Table.ToList())
            {
                while (groupAndGameDtoTasks.Count(x => x.IsCompleted == false) >= 10)
                {
                    Thread.Sleep(200);
                }

                var groupsAndGamesByTourTask = GetGroupsAndGamesByTour(tour);
                groupAndGameDtoTasks.Add(groupsAndGamesByTourTask);
            }

            //var groupsAndGamesByTour = GetGroupsAndGamesByTour(_tourRepository.Table.ToList().Last());
            //groupAndGameDtos.Add(groupsAndGamesByTour);

            return groupAndGameDtoTasks;
        }

        public void ClearCache()
        {
            _cacheHelper.ClearCache(_ratingPagesDirectoryName);
            _cacheHelper.ClearCache(_groupPagesDirectoryName);    
        }

        private async Task<GroupAndGameDto> GetGroupsAndGamesByTourAsync(TourRecord tour)
        {
            return await GetGroupsAndGamesByTour(tour).ConfigureAwait(false);
        }

        private async Task<GroupAndGameDto> GetGroupsAndGamesByTour(TourRecord tour)
        {
            var strDate = tour.Date.ToString("yyyy-MM-dd");

            bool f = false;

            string source = String.Empty;

            var cacheFileName = GetFileName(_groupPageNameFormat, strDate);

            if (!_cacheHelper.ExistInCache(_groupPagesDirectoryName, cacheFileName))
            {
                do
                {
                    try
                    {
                        var req = HttpWebRequest.Create(String.Format("http://tabletennis.by/tours/{0}/groups", strDate));
                        req.Method = "GET";

                        var response = await req.GetResponseAsync().ConfigureAwait(false);

                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            source = reader.ReadToEnd();

                            if (!_cacheHelper.ExistInCache(_groupPagesDirectoryName, cacheFileName))
                            {
                                _cacheHelper.SaveDataToCache(_groupPagesDirectoryName, cacheFileName, source);
                            }

                            f = true;
                        }
                    }
                    catch (WebException)
                    {}
                    catch (Exception)
                    {}
                } 
                while (f == false);
            }
            else
            {
                source = _cacheHelper.GetCachedData(_groupPagesDirectoryName, cacheFileName);
            }

            return await Task.Run(() => ParseGroupsPage(source, tour)).ConfigureAwait(false);
        }

        private GroupAndGameDto ParseGroupsPage(string source, TourRecord tour)
        {
            CQ html = source;

            var cq = html["dl[class='tabs groups-tabs'] > dt[data-tab]"];

            //var leagueRecords = new List<LeagueRecord>();
            //var leagueInTourRecords = new List<LeagueInTourRecord>();

            var allGroupsInTour = new List<GroupRecord>();
            var allGamesInTour = new List<GameRecord>();

            foreach (var el in cq.Elements)
            {
                var dtLeagueName = HttpUtility.HtmlDecode(el.InnerText);

                var leagueRecord = new LeagueRecord { Name = dtLeagueName };
                //leagueRecords.Add(leagueRecord);
                var leagueInTourRecord = new LeagueInTourRecord { LeagueRecord = leagueRecord, TourRecord = tour };
                //leagueInTourRecords.Add(new LeagueInTourRecord { LeagueRecord = leagueRecord, TourRecord = tour });
                var allGamesInLeagueInTour = new List<GameRecord>();

                var ddEl = el.NextElementSibling;
                var cqTables = ((CQ)ddEl.InnerHTML)["table[class='group']"];

                foreach (var cqTable in cqTables.Elements)
                {
                    GroupRecord group;
                    List<GameRecord> games;
                    ParseGroupTable(cqTable, out group, out games);
                    group.LeagueInTourRecord = leagueInTourRecord;
                    allGamesInLeagueInTour.AddRange(games);

                    allGroupsInTour.Add(group);

                }

                List<GroupRecord> singleGroups;
                List<GameRecord> singleGames;
                ParseSingleGames(ddEl, out singleGroups, out singleGames);
                singleGroups.ForEach(x => x.LeagueInTourRecord = leagueInTourRecord);

                FilterDuplicates(allGamesInLeagueInTour);

                allGamesInTour.AddRange(allGamesInLeagueInTour);
                allGamesInTour.AddRange(singleGames);
                allGroupsInTour.AddRange(singleGroups);
            }

            return new GroupAndGameDto { GroupRecords = allGroupsInTour, GameRecords = allGamesInTour };
        }

        private void ParseSingleGames(IDomElement el, out List<GroupRecord> groups, out List<GameRecord> games)
        {
            groups = new List<GroupRecord>();
            games = new List<GameRecord>();

            var divGames = ((CQ)el.InnerHTML)["div.game"];

            foreach (var divGame in divGames.Elements)
            {
                var group = new GroupRecord { Name = GlobalConstants.SingleGroupName };
                var game = new GameRecord();
                var player1 = new PlayerRecord();
                var player2 = new PlayerRecord();
                player1.Name = ((CQ)divGame.InnerHTML)["span[class='g-w'] > a"].Elements.FirstOrDefault().InnerText;
                player2.Name = ((CQ)divGame.InnerHTML)["span[class='g-l'] > a"].Elements.FirstOrDefault().InnerText;

                game.Player1Record = player1;
                game.Player2Record = player2;
                game.WinnerRecord = player1;
                game.CountOfWonSets1 = Int32.Parse(((CQ)divGame.InnerHTML)["span[class='g-w-p']"].Elements.FirstOrDefault().InnerText);
                game.CountOfWonSets2 = Int32.Parse(((CQ)divGame.InnerHTML)["span[class='g-l-p']"].Elements.FirstOrDefault().InnerText);
                game.GroupRecord = group;
                game.IsForfeited = false;

                group.GameRecords = new List<GameRecord> {game};
                groups.Add(group);
                games.Add(game);
            }
        }

        private void ParseGroupTable(IDomElement el, out GroupRecord group, out List<GameRecord> games)
        {
            group = new GroupRecord();
            group.GameRecords = new List<GameRecord>();
            games = new List<GameRecord>();
            group.Name = HttpUtility.HtmlDecode(((CQ)el.InnerHTML)["div.caption"].Elements.FirstOrDefault().InnerText.Trim('\n'));

            var aPlayers = ((CQ)el.InnerHTML)["tbody > tr > td.player > a[href]"];
            var players = new List<PlayerRecord>();

            foreach (var aPlayer in aPlayers.Elements)
            {
                players.Add(new PlayerRecord { Name = aPlayer.InnerText });
            }

            var tdResults = ((CQ)el.InnerHTML)["tbody > tr > td[class^='game rs']"];

            for (var i = 0; i < players.Count - 1; i++)
            {
                for (var j = i + 1; j < players.Count; j++)
                {
                    var ind = i * (players.Count - 1) + j - 1;
                    var tdResult = tdResults.Elements.ElementAt(ind);

                    var strResult = tdResult.InnerText;

                    var strCounts = strResult.Split(':');
                    var countOfWonSets1 = Int32.Parse(strCounts[0]);
                    var countOfWonSets2 = Int32.Parse(strCounts[1]);

                    var winner = players[i];
                    if (countOfWonSets1 < countOfWonSets2)
                    {
                        winner = players[j];
                    }

                    var isForfeited = ((CQ)tdResult.InnerHTML)["sup"].Elements.Count() > 0;

                    var game = new GameRecord
                    {
                        CountOfWonSets1 = countOfWonSets1,
                        CountOfWonSets2 = countOfWonSets2,
                        GroupRecord = group,
                        Player1Record = players[i],
                        Player2Record = players[j],
                        WinnerRecord = winner,
                        IsForfeited = isForfeited
                    };

                    group.GameRecords.Add(game);
                    games.Add(game);
                }
            }
        }

        private void FilterDuplicates(List<GameRecord> games)
        {
            var gamesToRemove = new List<GameRecord>();
            foreach (var game in games)
            {
                if (gamesToRemove.Contains(game))
                {
                    continue;
                }

                GameRecord g = game;
                Func<GameRecord, bool> pred = x =>
                                              (x.Player1Record.Name == g.Player1Record.Name && x.Player2Record.Name == g.Player2Record.Name)
                                              ||
                                              (x.Player2Record.Name == g.Player1Record.Name && x.Player1Record.Name == g.Player2Record.Name);

                if (games.Count(pred) > 1)
                {
                    var lastGame = games.LastOrDefault(pred);
                    gamesToRemove.Add(lastGame);
                }
            }

            foreach (var gameToRemove in gamesToRemove)
            {
                games.Remove(gameToRemove);
            }
        }

        private string GetFileName(string format, string arg)
        {
            return String.Format(format, arg);
        }
    }

}