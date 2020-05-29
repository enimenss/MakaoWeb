using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Makao
{
    public class MakaoHub : Hub
    {
        public void updateScore(int PlayerId,int Score)
        {
            Clients.All.updateScore(PlayerId,Score);
        }


        public void updateDivider(string Divider,int Cycle)
        {
            Clients.All.updateDivider(Divider,Cycle);
        }
    }
}