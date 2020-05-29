using Makao.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Makao.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult NewGame()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewGame(NewGameViewModel Model)
        {
            int gameId = 0;
            using (var data = new MakaoEntities())
            {
                var game = new Game { NumOfPlayers = Model.NumOfPlayers, StartDate = DateTime.Now };
                data.Games.Add(game);
                data.SaveChanges();
                gameId = game.Id;
            }
           return RedirectToAction("Players", "Home", new { Id = gameId });
        }


        public ActionResult Players(int Id)
        {
            Game game = new Game();
            using (var data = new MakaoEntities())
            {
                game = (from g in data.Games where g.Id == Id select g).FirstOrDefault();
            }

            PlayersViewModel model = new PlayersViewModel();
            model.PlayerNames = new List<PlayerName>();
            for(int i=0; i < game.NumOfPlayers; i++)
            {
                model.PlayerNames.Add(new PlayerName());
            }
            model.Id = Id;

             return View(model);
          }

        [HttpPost]
        public ActionResult Players(PlayersViewModel Model)
        {
            int i = 1;
            using (var data = new MakaoEntities())
            {
                foreach (var row in Model.PlayerNames)
                {
                    var player = new Player { Name = row.Name, GameId = Model.Id, Position = i };
                    if (i == 1)
                    {
                        Game game = (from g in data.Games where g.Id ==Model.Id select g).FirstOrDefault();
                        game.Divider = player.Name;
                    }
                    data.Players.Add(player);
                    data.SaveChanges();
                    var score = new Score() { Cycle =null, GameId = Model.Id, PlayerId = player.Id, InsertDate = DateTime.Now, Score1 = 0 };
                    data.Scores.Add(score);
                    i++;
                }
                data.SaveChanges();
            }
            return RedirectToAction("GameStats","Home",new { Id = Model.Id });
        }


        public ActionResult GameStats(int Id)
        {
            List<GameStatsModel> model = new List<GameStatsModel>();

            Game game = new Game();

            int cycle = 0;

            using(var data =new MakaoEntities())
            {
                game = (from g in data.Games where g.Id==Id select g).FirstOrDefault();
                model = (from p in data.Players
                         join s in data.Scores on p.Id equals s.PlayerId
                         where s.GameId == Id
                         group new { s, p } by new { s.GameId, s.PlayerId,p.Position, p.Name } into g
                         orderby g.Key.Position
                         select new GameStatsModel
                         {
                             PlayerId = g.Key.PlayerId,
                             PlayerName = g.Key.Name,
                             Scores=g.Sum(x=>x.s.Score1),
                             Position=g.Key.Position
                         }
                       ).ToList();

                cycle = (from s in data.Scores where s.GameId == Id select s.Cycle).Max().GetValueOrDefault(1);
            }

            ViewBag.GameId = Id;
            string Divider = game.Divider;
            if (string.IsNullOrEmpty(Divider))
            {
                Divider = model.Where(x => x.Position == 1).Select(y => y.PlayerName).FirstOrDefault();
            }

            ViewBag.Divider = Divider;

            ViewBag.Cycle = cycle;

            return View(model);
        }


        [HttpPost]
        public ActionResult EndRound(int GameId)
        {
            string Status = "OK";

            string Divider = string.Empty;

            List<Player> players = new List<Player>();

            Game game = new Game();

            using (var data = new MakaoEntities())
            {
                game = (from g in data.Games where g.Id == GameId select g).FirstOrDefault();
                players = (from p in data.Players
                           where p.GameId == GameId
                           select p
                       ).ToList();


                int currentDealerPosition;
                Divider = game.Divider;
                if (string.IsNullOrEmpty(Divider))
                {
                    Divider = players.Where(x => x.Position == 1).Select(y => y.Name).FirstOrDefault();
                }
                else
                {
                    currentDealerPosition = players.Where(x => x.Name == Divider).Select(y => y.Position.GetValueOrDefault(1)).FirstOrDefault() % game.NumOfPlayers.GetValueOrDefault(0) + 1;
                    Divider = players.Where(x => x.Position == currentDealerPosition).Select(y => y.Name).FirstOrDefault();
                }

                game.Divider = Divider;
                data.SaveChanges();

            }

            return this.Json(new
            {
                Status = Status,
                Data=Divider

            });
        }



        [HttpPost]
        public ActionResult SubmitScore(int GameId,int PlayerScore,int PlayerId,int Cycle)
        {
            string Status = "OK";

            Score model = new Score { GameId = GameId, PlayerId = PlayerId, Score1 = PlayerScore,Cycle=Cycle,InsertDate=DateTime.Now };



            using (var data = new MakaoEntities())
            {
                var playerScore = (from p in data.Scores where p.GameId == GameId && p.PlayerId == PlayerId select p).Sum(x => x.Score1).GetValueOrDefault();
                if (playerScore+model.Score1 > 1000)
                {
                    Game game = (from g in data.Games where g.Id ==GameId select g).FirstOrDefault();
                    var playerName = (from p in data.Players where p.GameId == GameId && p.Id == PlayerId select p.Name).FirstOrDefault();
                    game.Loser =playerName;
                    game.EndDate = DateTime.Now;
                    data.SaveChanges();
                }
                data.Scores.Add(model);
                data.SaveChanges();
            }

                return this.Json(new
            {
                Status = Status,
            });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}