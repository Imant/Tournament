using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.Tournament.Models;

namespace Custom.Tournament.ViewModels
{
    public class GroupViewModel
    {
        public int LeagueInTourId { get; set; }

        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public List<PlayerRecord> OldPlayers { get; set; }

        public List<PlayerRecord> ParticipatedPlayers { get; set; }

        public List<string> OldResults { get; set; } 
    }
}