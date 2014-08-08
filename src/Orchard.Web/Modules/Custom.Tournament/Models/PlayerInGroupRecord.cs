using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;

namespace Custom.Tournament.Models
{
    [OrchardFeature("Custom.Tournament")]
    public class PlayerInGroupRecord
    {
        public virtual int Id { get; set; }

        public virtual PlayerRecord PlayerRecord { get; set; }

        public virtual GroupRecord GroupRecord { get; set; }

        public virtual int CountOfWonGames { get; set; }

        public virtual int CountOfLostGames { get; set; }

        public virtual int PlaceInGroup { get; set; }
    }
}