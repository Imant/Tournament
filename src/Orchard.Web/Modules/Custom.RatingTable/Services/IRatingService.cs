using System.Collections.Generic;
using Custom.RatingTable.Models;
using Orchard;

namespace Custom.RatingTable.Services
{
    public interface IRatingService : IDependency
    {
        IList<RatingRecord> GetRatingsForPart(int ratingTablePartRecordId);
    }
}