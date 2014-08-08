using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.Tournament.Helpers;
using Custom.Tournament.Models;

namespace Custom.Tournament.ViewModels
{
    public class ParticipatedPlayersEditorViewModel
    {
        public List<int> ParticipatedPlayerIds { get; set; }

        public List<RatingRecord> Ratings
        {
            get; set;
        }

        public int LeagueInTourId { get; set; }
    }
}