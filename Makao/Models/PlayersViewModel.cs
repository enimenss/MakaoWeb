using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Makao.Models
{
    public class PlayersViewModel
    {
       public int Id { get; set; }
       public List<PlayerName> PlayerNames { get; set; }
    }
}