using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.RatingTable.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Custom.RatingTable
{
    [OrchardFeature("Custom.RatingTable")]
    public class Migrations : DataMigrationImpl
    {
        private readonly IRepository<RatingRecord> _ratingRepository;
        private readonly IRepository<RatingTablePartRecord> _ratingTablePartRepository;
        private readonly IContentManager _contentManager;

        public Migrations(IRepository<RatingRecord> ratingRepository, IRepository<RatingTablePartRecord> ratingTablePartRepository, IContentManager contentManager)
        {
            _ratingRepository = ratingRepository;
            _ratingTablePartRepository = ratingTablePartRepository;
            _contentManager = contentManager;
        }

        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(RatingTablePartRecord).Name,
                                      table => table
                                                   .ContentPartRecord()
                                                   .Column<string>("Name")
                );

            SchemaBuilder.CreateTable(typeof(RatingRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<int>("Place")
                                                   .Column<int>("PlaceIncrement")
                                                   .Column<string>("Player")
                                                   .Column<double>("Rating")
                                                   .Column<double>("RatingIncrement")
                                                   .Column<string>("RatingTablePartRecord_Id")
                );

            ContentDefinitionManager.AlterPartDefinition(
                typeof(RatingTablePart).Name,
                builder => builder.Attachable()
                );

            return 2;
        }

        private readonly IEnumerable<RatingRecord> _ratings =
            new List<RatingRecord> 
            {
                new RatingRecord {Place = 1, PlaceIncrement = 0, Player = "Julia", Rating = 2088.7, RatingIncrement = 0},
                new RatingRecord {Place = 2, PlaceIncrement = 2, Player = "Semenovich", Rating = 2057.8, RatingIncrement = 0},
                new RatingRecord {Place = 3, PlaceIncrement = 2, Player = "Sting", Rating = 2056.2, RatingIncrement = 7.5},
                new RatingRecord {Place = 4, PlaceIncrement = -1, Player = "Fisher", Rating = 2053.9, RatingIncrement = -10}
            };

        public int UpdateFrom2()
        {
            ContentDefinitionManager.AlterTypeDefinition("RatingTable",
                cfg => cfg
                    .DisplayedAs("Rating Table")
                    .WithPart("TitlePart")
                    .WithPart(typeof(RatingTablePart).Name)
                    .WithPart("CommonPart")
                    .Creatable()
                );

            return 3;
        }

        public int UpdateFrom3()
        {

            if (_ratingRepository == null)
                throw new InvalidOperationException(
                    "Couldn't find content manager repository.");

            var ratingTableContentItem = _contentManager.New("RatingTable");

            var ratingTablePartItem = ratingTableContentItem.As<RatingTablePart>();

            if (_ratingRepository == null)
                throw new InvalidOperationException(
                    "Couldn't find rating repository.");

            foreach (var rating in _ratings)
            {
                rating.RatingTablePartRecord = ratingTablePartItem.Record;
                _ratingRepository.Create(rating);
            }

            _contentManager.Create(ratingTableContentItem);

            return 4;
        }
    }
}