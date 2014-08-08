using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Custom.Tournament.Models.Dto
{
    public class GroupAndGameDto
    {
        public List<GroupRecord> GroupRecords { get; set; }

        public List<GameRecord> GameRecords { get; set; }
    }
}