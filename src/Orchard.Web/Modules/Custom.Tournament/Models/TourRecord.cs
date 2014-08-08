using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Custom.Tournament.Models
{
    [OrchardFeature("Custom.Tournament")]
    public class TourRecord
    {
        public virtual int Id { get; set; }

        public virtual DateTime Date { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<RatingRecord> RatingRecords { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<LeagueInTourRecord> LeagueInTourRecords { get; set; }
    }
}