using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Custom.Tournament.Helpers;
using Custom.Tournament.Models;
using Custom.Tournament.Services;
using Custom.Tournament.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using PagedList;

namespace Custom.Tournament.Controllers
{
    [OrchardFeature("Custom.Tournament")]
    [Admin]
    public class TournamentAdminController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthorizer _authorizer;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly ITournamentDataService _tournamentDataService;
        private readonly IRepository<PlayerRecord> _playerRepository;
        private readonly IRepository<TourRecord> _tourRepository;
        private readonly IRepository<RatingRecord> _ratingRepository;
        private readonly IRepository<LeagueRecord> _leagueRepository;
        private readonly IRepository<LeagueInTourRecord> _leagueInTourRepository;
        private readonly IRepository<GroupRecord> _groupRepository;
        private readonly IRepository<GameRecord> _gameRepository;

        private readonly int _pageSize = 10;

        public Localizer T { get; set; }

        public TournamentAdminController(
            IOrchardServices orchardServices, 
            ITournamentDataService tournamentDataService, 
            IRepository<PlayerRecord> playerRepository, 
            IRepository<TourRecord> tourRepository, 
            IRepository<RatingRecord> ratingRepository, 
            IRepository<LeagueRecord> leagueRepository, 
            IRepository<LeagueInTourRecord> leagueInTourRepository,
            IRepository<GroupRecord> groupRepository,
            IRepository<GameRecord> gameRepository)
        {
            _orchardServices = orchardServices;
            _authorizer = orchardServices.Authorizer;
            _contentManager = orchardServices.ContentManager;
            _transactionManager = orchardServices.TransactionManager;
            _tournamentDataService = tournamentDataService;
            _playerRepository = playerRepository;
            _tourRepository = tourRepository;
            _ratingRepository = ratingRepository;
            _leagueRepository = leagueRepository;
            _leagueInTourRepository = leagueInTourRepository;
            _groupRepository = groupRepository;
            _gameRepository = gameRepository;

            T = NullLocalizer.Instance;
        }

        [Themed]
        public ActionResult TournamentDashboard(){

            ViewBag.Players = _playerRepository.Table.ToList();
            ViewBag.Tours = _tourRepository.Table.ToList();

            return View();
        }

        [Themed]
        public ActionResult EditTours(int? page)
        {
            var tours = _tourRepository.Table;

            var pageNumber = (page ?? 1);
            return View(tours.OrderByDescending(x => x.Date).ToPagedList(pageNumber, _pageSize));
        }

        [Themed]
        public ActionResult CreateTour()
        {
            return View("EditTour");
        }

        [Themed]
        [HttpPost]
        public ActionResult CreateTour(TourRecord tour)
        {
            return RedirectToAction("EditTours");
        }

        [Themed]
        public ActionResult EditTour(int tourId = 0)
        {
            TourRecord tour;
            if (tourId != 0)
            {
                tour = _tourRepository.Get(tourId);
            }
            else
            {
                tour = new TourRecord();
            }
            return View(tour);
        }

        [Themed]
        [HttpPost]
        public ActionResult EditTour(TourRecord tour)
        {
            if (tour.Id == 0)
            {
                _tourRepository.Create(tour);
            }
            else
            {
                var tourInDb = _tourRepository.Get(tour.Id);
                tourInDb.Date = tour.Date;
                _tourRepository.Update(tourInDb);
            }

            return RedirectToAction("EditTours");
        }

        [Themed]
        public ActionResult DeleteTour(int tourId)
        {
            TourRecord tour;
            if (tourId != 0)
            {
                tour = _tourRepository.Get(tourId);

                if (tour.LeagueInTourRecords.Count == 0 && tour.RatingRecords.Count == 0)
                {
                    _tourRepository.Delete(tour);
                    _orchardServices.Notifier.Information(T("Tour has been deleted deleted"));
                }
                else
                {
                    _orchardServices.Notifier.Information(T("Tour could not be deleted. Reason: there is another objects in the database related to this tour."));
                }
            }
            else
            {
                _orchardServices.Notifier.Information(T("Tour with specified id doesn't exist in the database"));
            }
            return RedirectToAction("EditTours");
        }

        [Themed]
        public ActionResult EditLeagueInTour(int leagueInTourId = 0, bool setCookie = true)
        {
            LeagueInTourRecord leagueInTour;
            if (leagueInTourId != 0)
            {
                leagueInTour = _leagueInTourRepository.Get(leagueInTourId);
            }
            else
            {
                leagueInTour = new LeagueInTourRecord();
            }

            var model = new LeagueInTourEditorViewModel {LeagueInTourId = leagueInTour.Id};

            model.Groups = _groupRepository.Table.Where(x => x.LeagueInTourRecord.Id == leagueInTourId).ToList();

            var participatedPlayerIds = new List<int>();

            foreach (var group in model.Groups)
            {
                foreach (var game in group.GameRecords)
                {
                    if (!participatedPlayerIds.Contains(game.Player1Record.Id))
                    {
                        participatedPlayerIds.Add(game.Player1Record.Id);
                    }
                    if (!participatedPlayerIds.Contains(game.Player2Record.Id))
                    {
                        participatedPlayerIds.Add(game.Player2Record.Id);
                    }
                }
                
            }

            if (setCookie)
            {
                var oldCookie = Request.Cookies["ParticipatedPlayerIds"];
                if (oldCookie != null)
                {
                    var aCookie = new HttpCookie("ParticipatedPlayerIds");
                    aCookie.Expires = DateTime.Now.AddDays(-1);
                    this.Response.Cookies.Add(aCookie);
                }

                this.Response.SetCookie(new HttpCookie("ParticipatedPlayerIds", string.Join(",", participatedPlayerIds.ToArray())));
            }

            return View(model);
        }

        [Themed]
        public ActionResult AddLeagueInTour(int tourId)
        {
            var leagues = _leagueRepository.Table.ToList();
            return View(new AddLeagueInTourEditorViewModel {Leagues = leagues, TourId = tourId});
        }

        [Themed]
        [HttpPost]
        public ActionResult AddLeagueInTour(AddLeagueInTourEditorViewModel addLeagueInTourEditorViewModel)
        {
            var tour = _tourRepository.Get(addLeagueInTourEditorViewModel.TourId);

            if (tour.LeagueInTourRecords.Any(x => x.LeagueRecord.Id == addLeagueInTourEditorViewModel.SelectedLeagueId))
            {
                _orchardServices.Notifier.Information(T("Selected league already exists in this tour"));
                return RedirectToAction("AddLeagueInTour", new {tourId = addLeagueInTourEditorViewModel.TourId});
            }

            var leagueInDb = _leagueRepository.Get(addLeagueInTourEditorViewModel.SelectedLeagueId);
            var leagueInTour = new LeagueInTourRecord {LeagueRecord = leagueInDb, TourRecord = tour};
            _leagueInTourRepository.Create(leagueInTour);

            _orchardServices.Notifier.Information(T("Selected league has been successfully added to this tour"));

            return RedirectToAction("EditTour", new {tourId = addLeagueInTourEditorViewModel.TourId});
        }

        [Themed]
        public ActionResult DeleteLeagueInTour(int leagueInTourId)
        {
            LeagueInTourRecord leagueInTour = _leagueInTourRepository.Get(leagueInTourId);
            if (leagueInTour.GroupRecords.Count > 0)
            {
                _orchardServices.Notifier.Information(T("League In Tour could not be deleted. Reason: there is another objects in the database related to this league in tour."));
            }
            else
            {
                _leagueInTourRepository.Delete(leagueInTour);
                _orchardServices.Notifier.Information(T("League In Tour has been deleted"));
            }

            return RedirectToAction("EditTour", new { tourId = leagueInTour.TourRecord.Id });
        }

        [Themed]
        public ActionResult AddParticipatedPlayers(int leagueInTourId, int[] selectedPlayers )
        {
            var players = _playerRepository.Table.ToList();

            var prevLastTour = _tourRepository.Table.OrderByDescending(x => x.Date).Skip(1).FirstOrDefault();

            List<RatingRecord> ratings;

            if (prevLastTour != null)
            {
                ratings = _ratingRepository.Table.OrderByDescending(x => x.Rating).Where(x => x.TourRecord.Date == prevLastTour.Date).ToList();
            }
            else
            {
                ratings = new List<RatingRecord>();
                foreach (var player in players)
                {
                    ratings.Add(new RatingRecord{IsInArchive = false, PlayerRecord = player, Rating = 0});
                }
            }

            var notIncludedPlayers = players.Where(x => ratings.All(y => y.PlayerRecord.Id != x.Id)).ToList();

            foreach (var notIncludedPlayer in notIncludedPlayers)
            {
                ratings.Add(new RatingRecord { IsInArchive = false, PlayerRecord = notIncludedPlayer, Rating = 0 });
            }


            return View(new ParticipatedPlayersEditorViewModel { Ratings = ratings, ParticipatedPlayerIds = new List<int>(), LeagueInTourId = leagueInTourId });
        }

        [HttpPost]
        public JsonResult GetPlayersByIds(List<int> playerIds)
        {
            var players = _playerRepository.Table.ToList().Where(x => playerIds.Any(y => y == x.Id)).Select(x => new {x.Id, x.Name});

            return Json(players.ToList());
        }

        [Themed]
        public ActionResult EditGroup(int leagueInTourId, int groupId = 0)
        {
            var groupViewModel = new GroupViewModel();

            groupViewModel.LeagueInTourId = leagueInTourId;
            groupViewModel.GroupId = groupId;
            if (groupId != 0)
            {
                var oldPlayers1 = _gameRepository.Table.Where(x => x.GroupRecord.Id == groupId).Select(x => x.Player1Record).Distinct().ToList();
                var oldPlayers2 = _gameRepository.Table.Where(x => x.GroupRecord.Id == groupId).Select(x => x.Player2Record).Distinct().ToList();

                groupViewModel.OldPlayers = oldPlayers1.Union(oldPlayers2).ToList();
                groupViewModel.OldResults = new List<string>();

                for (var i = 0; i < groupViewModel.OldPlayers.Count; i++)
                {
                    for (var j = i + 1; j < groupViewModel.OldPlayers.Count; j++)
                    {
                        var game = _gameRepository.Table.FirstOrDefault(x => x.GroupRecord.Id == groupId &&
                                                                             ((x.Player1Record.Id == groupViewModel.OldPlayers[i].Id && x.Player2Record.Id == groupViewModel.OldPlayers[j].Id)
                                                                              ||
                                                                              (x.Player1Record.Id == groupViewModel.OldPlayers[j].Id && x.Player2Record.Id == groupViewModel.OldPlayers[i].Id)));

                        var c1 = game.CountOfWonSets1;
                        var c2 = game.CountOfWonSets2;

                        if (game.Player1Record.Id == groupViewModel.OldPlayers[j].Id && game.Player2Record.Id == groupViewModel.OldPlayers[i].Id)
                        {
                            c1 = game.CountOfWonSets2;
                            c2 = game.CountOfWonSets1;
                        }

                        groupViewModel.OldResults.Add(String.Format("{0}:{1}:{2}", c1, c2, game.IsForfeited ? 1 : 0));
                    }
                }

                groupViewModel.GroupName = _groupRepository.Get(groupId).Name;
            }

            groupViewModel.ParticipatedPlayers = _playerRepository.Table.Take(5).ToList();

            return View(groupViewModel);
        }

        [Themed]
        public ActionResult DeleteGroup(int leagueInTourId, int groupId = 0)
        {
            var group = _groupRepository.Get(groupId);

            if (group != null)
            {

                if (group.GameRecords.Count > 0)
                {
                    foreach (var game in group.GameRecords)
                    {
                        _gameRepository.Delete(game);
                    }
                }
                _groupRepository.Delete(group);
                _orchardServices.Notifier.Information(T("Group with all the related games has been deleted"));
            }

            return RedirectToAction("EditLeagueInTour", new { leagueInTourId});
        }

        [HttpPost]
        public ActionResult SaveGroup(int leagueInTourId, int groupId, string groupName, List<int> playerIds, List<string> results)
        {
            var players = new List<PlayerRecord>();

            if (playerIds == null)
            {
                playerIds = new List<int>();
            }

            if (results == null)
            {
                results = new List<string>();
            }

            foreach (var id in playerIds)
            {
                players.Add(_playerRepository.Get(id));
            }

            GroupRecord group;

            if (groupId == 0)
            {
                group = new GroupRecord();
                group.LeagueInTourRecord = _leagueInTourRepository.Get(leagueInTourId);
                _groupRepository.Create(group);
            }
            else
            {
                group = _groupRepository.Get(groupId);
            }
            group.Name = groupName;

            var k = 0;
            var games = new List<GameRecord>();

            for (var i = 0; i < players.Count; i++)
            {
                for (var j = i + 1; j < players.Count; j++)
                {
                    GameRecord game;
                    GameRecord gameInDb = null;
                    

                    if (groupId != 0)
                    {
                        gameInDb = group.GameRecords.FirstOrDefault(x => (x.Player1Record.Id == players[i].Id && x.Player2Record.Id == players[j].Id)
                                                                             ||
                                                                             (x.Player1Record.Id == players[j].Id && x.Player2Record.Id == players[i].Id));
                        game = gameInDb ?? new GameRecord();
                    }
                    else
                    {
                        game = new GameRecord();
                    }

                    game.Player1Record = players[i];
                    game.Player2Record = players[j];

                    var resultArr = results[k++].Split(':');

                    game.CountOfWonSets1 = Int32.Parse(resultArr[0]);
                    game.CountOfWonSets2 = Int32.Parse(resultArr[1]);
                    game.WinnerRecord = game.Player1Record;
                    if (game.CountOfWonSets2 > game.CountOfWonSets1)
                    {
                        game.WinnerRecord = game.Player2Record;
                    }

                    game.IsForfeited = Int32.Parse(resultArr[2]) == 1;

                    game.GroupRecord = group;

                    if (gameInDb == null)
                    {
                        _gameRepository.Create(game);
                    }

                    games.Add(game);
                }
            }

            if (groupId != 0)
            {
                var deletedGames = new List<GameRecord>();
                foreach (var gameInDb in group.GameRecords)
                {
                    if (!games.Any(x => (x.Player1Record.Id == gameInDb.Player1Record.Id && x.Player2Record.Id == gameInDb.Player2Record.Id)
                                       ||
                                       (x.Player1Record.Id == gameInDb.Player2Record.Id && x.Player2Record.Id == gameInDb.Player1Record.Id)))
                    {
                        _gameRepository.Delete(gameInDb);
                        deletedGames.Add(gameInDb);
                    }
                }
                while (deletedGames.Count > 0)
                {
                    group.GameRecords.Remove(deletedGames[0]);
                    deletedGames.Remove(deletedGames[0]);
                }
            }

            return Json(new {});
        }

        [Themed]
        public ActionResult SynchronizeWithRemoteSite()
        {
            var players = _tournamentDataService.GetPlayers();

            foreach (var player in players)
            {
                var playerName = player.Name;
                var playerInDb = _playerRepository.Fetch(x => x.Name == playerName).FirstOrDefault();
                if (playerInDb == null)
                {
                    _playerRepository.Create(player);
                }
            }

            _transactionManager.RequireNew();

            var tours = _tournamentDataService.GetTours();

            foreach (var tour in tours)
            {
                var date = tour.Date;
                var tourInDb = _tourRepository.Fetch(x => x.Date == date).FirstOrDefault();
                if (tourInDb == null)
                {
                    _tourRepository.Create(tour);
                }
            }

            _transactionManager.RequireNew();

            var ratingsTasks = _tournamentDataService.GetRatings();

            foreach (var task in ratingsTasks)
            {
                Task.WaitAll(task);
            }

            var playersInDb = _playerRepository.Table.ToList();
            var ratingsInDb = _ratingRepository.Table.ToList();

            foreach (var ratingsTask in ratingsTasks)
            {
                var ratings = ratingsTask.Result;

                foreach (var rating in ratings)
                {
                    var tourId = rating.TourRecord.Id;
                    var playerInDb = playersInDb.FirstOrDefault(x => x.Name == rating.PlayerRecord.Name);
                    var ratingInDb = ratingsInDb.FirstOrDefault(x => x.TourRecord.Id == tourId && x.PlayerRecord.Id == playerInDb.Id);

                    if (ratingInDb == null)
                    {
                        rating.PlayerRecord = playerInDb;
                        _ratingRepository.Create(rating);
                    }
                }
                _transactionManager.RequireNew();
            }

            _transactionManager.RequireNew();

            var leagueAndTourDtoTasks = _tournamentDataService.GetLeaguesAndLeagueInTours();

            foreach (var task in leagueAndTourDtoTasks)
            {
                Task.WaitAll(task);
            }

            foreach (var leagueAndTourDtoTask in leagueAndTourDtoTasks)
            {
                var leaguesInDb = _leagueRepository.Table.ToList();
                var leagueInToursInDb = _leagueInTourRepository.Table.ToList();

                var leagueAndTourDto = leagueAndTourDtoTask.Result;

                foreach (var league in leagueAndTourDto.LeagueRecords)
                {
                    if (leaguesInDb.All(x => x.Name != league.Name))
                    {
                        _leagueRepository.Create(league);
                    }
                }

                foreach (var leagueInTour in leagueAndTourDto.LeagueInTourRecords)
                {

                    if (!leagueInToursInDb.Any(x => x.LeagueRecord.Name == leagueInTour.LeagueRecord.Name && x.TourRecord.Date == leagueInTour.TourRecord.Date))
                    {
                        if (leagueInTour.LeagueRecord.Id == 0)
                        {
                            leagueInTour.LeagueRecord = leaguesInDb.FirstOrDefault(x => x.Name == leagueInTour.LeagueRecord.Name);
                        }
                        _leagueInTourRepository.Create(leagueInTour);
                    }
                }

                _transactionManager.RequireNew();
            }

            var groupAndGameDtoTasks = _tournamentDataService.GetGroupAndGames();

            foreach (var task in groupAndGameDtoTasks)
            {
                Task.WaitAll(task);
            }

            var leagueInToursInDb2 = _leagueInTourRepository.Table.ToList();
            var playersInDb2 = _playerRepository.Table.ToList();

            foreach (var groupAndGameDtoTask in groupAndGameDtoTasks)
            {
                var groupsInDb = _groupRepository.Table.ToList();
                var gamesInDb = _gameRepository.Table.ToList();
                var groupAndGameDto = groupAndGameDtoTask.Result;

                List<GameRecord> singleGamesInLeagueInTourInDb = null;
                List<GroupRecord> notSingleGroupsInLeagueInTourInDb = null;
                var leagueName = String.Empty;

                foreach (var group in groupAndGameDto.GroupRecords)
                {
                    var g = group;

                    if (singleGamesInLeagueInTourInDb == null || g.LeagueInTourRecord.LeagueRecord.Name != leagueName)
                    {
                        singleGamesInLeagueInTourInDb = gamesInDb.Where(x => x.GroupRecord.LeagueInTourRecord.TourRecord.Date == g.LeagueInTourRecord.TourRecord.Date
                                                                             &&
                                                                             x.GroupRecord.LeagueInTourRecord.LeagueRecord.Name == g.LeagueInTourRecord.LeagueRecord.Name
                                                                             &&
                                                                             x.GroupRecord.Name == GlobalConstants.SingleGroupName).ToList();

                        notSingleGroupsInLeagueInTourInDb = groupsInDb.Where(x => x.LeagueInTourRecord.TourRecord.Date == g.LeagueInTourRecord.TourRecord.Date
                                                                             &&
                                                                             x.LeagueInTourRecord.LeagueRecord.Name == g.LeagueInTourRecord.LeagueRecord.Name
                                                                             &&
                                                                             x.Name != GlobalConstants.SingleGroupName).ToList();

                        leagueName = g.LeagueInTourRecord.LeagueRecord.Name;
                    }

                    if (!CheckIfExistInDb(group, singleGamesInLeagueInTourInDb, notSingleGroupsInLeagueInTourInDb))
                    {
                        group.LeagueInTourRecord = leagueInToursInDb2.FirstOrDefault(x => x.LeagueRecord.Name == group.LeagueInTourRecord.LeagueRecord.Name && x.TourRecord.Date == group.LeagueInTourRecord.TourRecord.Date);
                        var gamesOfGroup = group.GameRecords;
                        group.GameRecords = null;
                        _groupRepository.Create(group);

                        foreach (var game in gamesOfGroup)
                        {
                            game.Player1Record = playersInDb2.FirstOrDefault(x => x.Name == game.Player1Record.Name);
                            game.Player2Record = playersInDb2.FirstOrDefault(x => x.Name == game.Player2Record.Name);
                            game.WinnerRecord = playersInDb2.FirstOrDefault(x => x.Name == game.WinnerRecord.Name);

                            _gameRepository.Create(game);
                        }
                    }
                }

                _transactionManager.RequireNew();
            }

            return RedirectToAction("TournamentDashboard");
        }

        private bool CheckIfExistInDb(GroupRecord group, List<GameRecord> singleGamesInLeagueInTourInDb, List<GroupRecord> notSingleGroupsInLeagueInTourInDb)
        {
            var f = notSingleGroupsInLeagueInTourInDb.Any(x => x.Name == group.Name);

            if (f)
            {
                return true;
            }

            if (group.Name != GlobalConstants.SingleGroupName)
            {
                return false;
            }

            var singleGameForTargetGroup = group.GameRecords.FirstOrDefault();

            f = singleGamesInLeagueInTourInDb.Any(x => (x.Player1Record.Name == singleGameForTargetGroup.Player1Record.Name && x.Player2Record.Name == singleGameForTargetGroup.Player2Record.Name)
                                         ||
                                         (x.Player1Record.Name == singleGameForTargetGroup.Player2Record.Name && x.Player2Record.Name == singleGameForTargetGroup.Player1Record.Name));

            return f;
        }

        [Themed]
        public ActionResult ClearCache()
        {
            _tournamentDataService.ClearCache();
            return RedirectToAction("TournamentDashboard");
        }
    }
}