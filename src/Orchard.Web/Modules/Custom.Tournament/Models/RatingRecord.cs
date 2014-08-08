using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Extensions;

namespace Custom.Tournament.Models
{
    [OrchardFeature("Custom.Tournament")]
    public class RatingRecord
    {
        public virtual int Id { get; set; }

        public virtual TourRecord TourRecord { get; set; }

        public virtual PlayerRecord PlayerRecord { get; set; }

        public virtual int Place { get; set; }

        public virtual double Rating { get; set; }

        public virtual bool IsInArchive { get; set; }
    }
}