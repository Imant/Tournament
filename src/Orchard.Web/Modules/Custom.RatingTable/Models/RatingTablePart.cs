using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Custom.RatingTable.Models
{
    [OrchardFeature("Custom.RatingTable")]
    public class RatingTablePart : ContentPart<RatingTablePartRecord>
    {
        //public IList<RatingRecord> RatingRecords
        //{
        //    get
        //    {
        //        return Record.RatingRecords;
        //    }
        //    set
        //    {
        //        Record.RatingRecords = value;
        //    }
        //}

        private readonly LazyField<IList<RatingRecord>> _ratings = new LazyField<IList<RatingRecord>>();
        internal LazyField<IList<RatingRecord>> RatingsField { get { return _ratings; } }
        public IList<RatingRecord> RatingRecords{
            get { return Record.RatingRecords; }
        }
        //{
        //    get { return _ratings.Value; }
        //    set { _ratings.Value = value; }
        //}

        public string Name 
        {
            get { return Record.Name; }
            set { Record.Name = value; }
        }
    }

    [OrchardFeature("Custom.RatingTable")]
    public class RatingTablePartRecord : ContentPartRecord
    {
        public RatingTablePartRecord()
        {
            RatingRecords = new List<RatingRecord>();
        }

        [CascadeAllDeleteOrphan]
        public virtual IList<RatingRecord> RatingRecords { get; set; }

        public virtual string Name { get; set; }
    }
}