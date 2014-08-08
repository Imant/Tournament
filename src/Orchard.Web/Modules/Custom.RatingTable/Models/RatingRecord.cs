using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Custom.RatingTable.Models
{
    [OrchardFeature("Custom.RatingTable")]
    public class RatingRecord
    {
        public virtual int Id { get; set; }

        public virtual int Place { get; set; }

        public virtual int PlaceIncrement { get; set; }

        public virtual string Player { get; set; }

        public virtual double Rating { get; set; }

        public virtual double RatingIncrement { get; set; }

        public virtual RatingTablePartRecord RatingTablePartRecord { get; set; }
    }
}