using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Custom.Tournament.Models;
using Custom.Tournament.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;

namespace Custom.Tournament.Controllers
{
    [OrchardFeature("Custom.Tournament")]
    public class TournamentController : Controller
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

        public TournamentController(
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

        public ActionResult GetRatingRecords()
        {
            var lastTour = _tourRepository.Table.OrderByDescending(x => x.Date).FirstOrDefault();
            if (lastTour != null)
            {
                var ratingRecords = _ratingRepository.Table.Where(x => x.TourRecord.Id == lastTour.Id).OrderBy(x => x.Place).Select(x => new { x.Id, x.Place, x.PlayerRecord.Name, x.Rating, x.IsInArchive }).ToList();
                return Json(new { ratingRecords }, JsonRequestBehavior.AllowGet);
            }

            return Json(new {}, JsonRequestBehavior.AllowGet);
        }
    }
}