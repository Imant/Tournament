using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;

namespace Custom.Tournament.Models
{
    [OrchardFeature("Custom.Tournament")]
    public class GameRecord
    {
        public virtual int Id { get; set; }

        public virtual GroupRecord GroupRecord { get; set; }

        public virtual PlayerRecord Player1Record { get; set; }

        public virtual PlayerRecord Player2Record { get; set; }

        public virtual int CountOfWonSets1 { get; set; }

        public virtual int CountOfWonSets2 { get; set; }

        public virtual PlayerRecord WinnerRecord { get; set; }

        public virtual bool IsForfeited { get; set; }
    }
}