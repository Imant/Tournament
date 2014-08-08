using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.Tournament.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Custom.Tournament
{
    [OrchardFeature("Custom.Tournament")]
    public class TournamentMigrations : DataMigrationImpl
    {
        private readonly IRepository<RatingRecord> _ratingRepository;
        private readonly IContentManager _contentManager;

        public TournamentMigrations(IRepository<RatingRecord> ratingRepository, IContentManager contentManager)
        {
            _ratingRepository = ratingRepository;
            _contentManager = contentManager;
        }

        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(PlayerRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<string>("Name")
                );

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateTable(typeof(TourRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<DateTime>("Date")
                );

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.CreateTable(typeof(RatingRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<double>("Rating")
                                                   .Column<int>("Place")
                                                   .Column<bool>("IsInArchive")
                                                   .Column<int>("TourRecord_Id")
                                                   .Column<int>("PlayerRecord_Id")
                );

            return 4;
        }

        public int UpdateFrom4(){
            SchemaBuilder.ExecuteSql(String.Format("alter table {0}{1} alter column [Name] nvarchar(255) COLLATE Cyrillic_General_CS_AS NULL", SchemaBuilder.FormatPrefix(SchemaBuilder.FeaturePrefix), typeof(PlayerRecord).Name));

            return 5;
        }

        public int UpdateFrom5()
        {
            SchemaBuilder.AlterTable(typeof(PlayerRecord).Name,
                                      act => act.CreateIndex("IDX_Name", "Name")
                );

            SchemaBuilder.AlterTable(typeof(TourRecord).Name,
                                      act => act.CreateIndex("IDX_Date", "Date")
                );

            SchemaBuilder.AlterTable(typeof(RatingRecord).Name,
                                      act => act.CreateIndex("IDX_TourRecord_Id", "TourRecord_Id")
                );

            SchemaBuilder.AlterTable(typeof(RatingRecord).Name,
                                      act => act.CreateIndex("IDX_PlayerRecord_Id", "PlayerRecord_Id")
                );

            return 6;
        }

        public int UpdateFrom6()
        {
            SchemaBuilder.CreateTable(typeof(LeagueRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<string>("Name")
                );

            SchemaBuilder.CreateTable(typeof(LeagueInTourRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<int>("LeagueRecord_Id")
                                                   .Column<int>("TourRecord_Id")
                );

            SchemaBuilder.AlterTable(typeof(LeagueRecord).Name,
                                      act => act.CreateIndex("IDX_Name", "Name")
                );

            SchemaBuilder.AlterTable(typeof(LeagueInTourRecord).Name,
                                      act => act.CreateIndex("IDX_LeagueRecord_Id", "LeagueRecord_Id")
                );

            SchemaBuilder.AlterTable(typeof(LeagueInTourRecord).Name,
                                      act => act.CreateIndex("IDX_TourRecord_Id", "TourRecord_Id")
                );

            return 7;
        }

        public int UpdateFrom7()
        {
            SchemaBuilder.CreateTable(typeof(GroupRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<string>("Name")
                                                   .Column<int>("LeagueInTourRecord_Id")
                );

            SchemaBuilder.AlterTable(typeof (GroupRecord).Name,
                                     act => act.CreateIndex("IDX_LeagueInTourRecord_Id", "LeagueInTourRecord_Id")
                );



            SchemaBuilder.CreateTable(typeof(GameRecord).Name,
                                      table => table
                                                   .Column<int>("Id", column => column.PrimaryKey().Identity())
                                                   .Column<int>("GroupRecord_Id")
                                                   .Column<int>("Player1Record_Id")
                                                   .Column<int>("Player2Record_Id")
                                                   .Column<int>("WinnerRecord_Id")
                                                   .Column<int>("CountOfWonSets1")
                                                   .Column<int>("CountOfWonSets2")
                                                   .Column<bool>("IsForeited")
                );

            SchemaBuilder.AlterTable(typeof(GameRecord).Name,
                                      act => act.CreateIndex("IDX_GroupRecord_Id", "GroupRecord_Id")
                );

            SchemaBuilder.AlterTable(typeof(GameRecord).Name,
                                      act => act.CreateIndex("IDX_Player1Record_Id", "Player1Record_Id")
                );

            SchemaBuilder.AlterTable(typeof(GameRecord).Name,
                                      act => act.CreateIndex("IDX_Player2Record_Id", "Player2Record_Id")
                );

            SchemaBuilder.AlterTable(typeof(GameRecord).Name,
                                      act => act.CreateIndex("IDX_WinnerRecord_Id", "WinnerRecord_Id")
                );

            return 8;
        }

        public int UpdateFrom8()
        {
            SchemaBuilder.CreateForeignKey("FK_Player1Record_Id", typeof (GameRecord).Name, new[] {"Player1Record_Id"}, typeof (PlayerRecord).Name, new[] {"Id"});
            SchemaBuilder.CreateForeignKey("FK_Player2Record_Id", typeof(GameRecord).Name, new[] { "Player2Record_Id" }, typeof(PlayerRecord).Name, new[] { "Id" });
            SchemaBuilder.CreateForeignKey("FK_WinnerRecord_Id", typeof(GameRecord).Name, new[] { "WinnerRecord_Id" }, typeof(PlayerRecord).Name, new[] { "Id" });

            return 9;
        }

        public int UpdateFrom9()
        {
            SchemaBuilder.AlterTable(typeof(GameRecord).Name,
                                      act => act.DropColumn("IsForeited")
                );

            SchemaBuilder.AlterTable(typeof(GameRecord).Name,
                                      act => act.AddColumn<bool>("IsForfeited")
                );

            return 10;
        }

        public int UpdateFrom10()
        {
            ContentDefinitionManager.AlterPartDefinition(typeof(RatingTablePart).Name,
                                                         builder => builder.Attachable());

            return 11;
        }

        public int UpdateFrom11()
        {
            ContentDefinitionManager.AlterTypeDefinition("RatingTable",
                                                         cfg => cfg
                                                                    .WithPart(typeof(RatingTablePart).Name)
                                                                    .Creatable()
                );

            return 12;
        }
    }
}