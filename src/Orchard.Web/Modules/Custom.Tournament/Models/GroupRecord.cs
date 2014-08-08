using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Custom.Tournament.Models
{
    [OrchardFeature("Custom.Tournament")]
    public class GroupRecord
    {
        public virtual int Id { get; set; }

        public virtual string Name { get; set; }

        public virtual LeagueInTourRecord LeagueInTourRecord { get; set; }

        //[CascadeAllDeleteOrphan]
        //public virtual IList<PlayerInGroupRecord> PlayerInGroupRecords { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<GameRecord> GameRecords { get; set; }
    }
}