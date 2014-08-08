using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Custom.Tournament.Models;

namespace Custom.Tournament.Helpers
{
    public class DataHelper
    {
        public static RatingRecord GetCurrentRating(PlayerRecord player)
        {
            if (player.RatingRecords != null && player.RatingRecords.Count > 0)
            {
                return player.RatingRecords.LastOrDefault();
            }
            return null;
        }
    }
}