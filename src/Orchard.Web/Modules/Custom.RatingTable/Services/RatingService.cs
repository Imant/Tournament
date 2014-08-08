using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.RatingTable.Models;
using Orchard;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Services;

namespace Custom.RatingTable.Services
{
    public class RatingService : IRatingService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IClock _clock;
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<RatingRecord> _ratingRepository;

        public RatingService(IOrchardServices orchardServices,
                             IClock clock,
                             IEncryptionService encryptionService,
                             IRepository<RatingRecord> ratingRepository){
            _orchardServices = orchardServices;
            _clock = clock;
            _encryptionService = encryptionService;
            _ratingRepository = ratingRepository;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IList<RatingRecord> GetRatingsForPart(int ratingTablePartRecordId){
            return _ratingRepository.Table.Where(x => x.RatingTablePartRecord.Id == ratingTablePartRecordId).ToList();
        }
    }
}