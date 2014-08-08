using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Custom.Tournament.Models;

namespace Custom.Tournament.ViewModels
{
    public class AddLeagueInTourEditorViewModel
    {
        public List<LeagueRecord> Leagues { get; set; }

        public int TourId { get; set; }

        public int SelectedLeagueId { get; set; }
    }
}