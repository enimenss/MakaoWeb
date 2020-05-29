using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Makao.Models
{
    public class GameStatsModel
    {

        public string PlayerName { get; set; }
        public int? PlayerId { get; set; }
        public int? Scores { get; set; }

        public int? Position { get; set; }
    }
}