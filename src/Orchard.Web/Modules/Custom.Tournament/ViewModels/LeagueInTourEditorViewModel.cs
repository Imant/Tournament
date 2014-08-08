using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.Tournament.Models;

namespace Custom.Tournament.ViewModels
{
    public class LeagueInTourEditorViewModel
    {
        public int LeagueInTourId { get; set; }

        public List<PlayerRecord> ParticipatedPlayers { get; set; }

        public List<GroupRecord> Groups { get; set; }
    }
}