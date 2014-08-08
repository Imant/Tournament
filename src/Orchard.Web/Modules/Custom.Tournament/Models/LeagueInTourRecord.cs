using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Custom.Tournament.Models
{
    [OrchardFeature("Custom.Tournament")]
    public class LeagueInTourRecord
    {
        public virtual int Id { get; set; }

        public virtual LeagueRecord LeagueRecord { get; set; }

        public virtual TourRecord TourRecord { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<GroupRecord> GroupRecords { get; set; }
    }
}