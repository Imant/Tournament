using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Custom.Tournament.Models.Dto
{
    public class LeagueAndTourDto
    {
        public List<LeagueInTourRecord> LeagueInTourRecords { get; set; }

        public List<LeagueRecord> LeagueRecords { get; set; }
    }
}