using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_1
{
    [DebuggerDisplay("Score: {(int)Score} | MMR: (Total: {(int)MMR} | Team: {(int)TeamMMR} | Single: {(int)SingleMMR} => {Name})")]
    public class Player
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double Score { get; set; }

        private double _consistency = 0;
        public double Consistency {
            get => this._consistency;
            set {
                this._consistency = value;
                if (this._consistency > 1) {
                    this._consistency = 1;
                } else if (this._consistency < 0) {
                    this._consistency = 0;
                }
            }
        }

        private double _teamMMR = 0;
        public double TeamMMR {
            //get {
            //    if (this.ValidTeamMMRGames >= Program.MIN_AMOUNT_MMR_GAMES_PER_PLAYER) {
            //        return this._teamMMR;
            //    }
            //    return Program.STARTING_MMR;
            //}
            get => this._teamMMR;
            set => this._teamMMR = value;
        }
        private double _singleMMR = 0;
        public double SingleMMR {
            //get {
            //    if (this.ValidSingleMMRGames >= Program.MIN_AMOUNT_MMR_GAMES_PER_PLAYER) {
            //        return this._singleMMR;
            //    }
            //    return Program.STARTING_MMR;
            //}
            get => this._singleMMR;
            set => this._singleMMR = value;
        }
        public double MMR => (this.SingleMMR + this.TeamMMR) / 2;

        public int ValidTeamMMRGames { get; set; }
        public int ValidSingleMMRGames { get; set; }
        public int TeamGames => this.Teams.Sum(x => x.Active ? x.Games.Count : 0);
        public int TotalGames => this.Teams.Sum(x => x.Games.Count);

        public List<Team> Teams { get; set; }

        public Player(int id, string name)
        {
            this.ID = id;
            this.Name = name;
            this.TeamMMR = Program.STARTING_MMR;
            this.SingleMMR = Program.STARTING_MMR;

            this.Teams = new List<Team>();
        }

        public void AddTeam(Team team)
        {
            if (this.Teams.Any(x => x.IsEqual(team))) {
                return;
            }

            this.Teams.Add(team);
        }

        public void ResetRatings(bool score=true, bool singleMMR=true, bool teamMMR=true, bool singleMMRgames=true, bool teamMMRgames = true)
        {
            this.Score = score ? 0 : this.Score;
            this.SingleMMR = score ? Program.STARTING_MMR : this.SingleMMR;
            this.TeamMMR = score ? Program.STARTING_MMR : this.TeamMMR;
            this.ValidSingleMMRGames = score ? 0 : this.ValidSingleMMRGames;
            this.ValidTeamMMRGames = score ? 0 : this.ValidTeamMMRGames;
        }


        public static Player GetPlayerById(List<Player> players, int id, string? name=null)
        {
            Player player = players.Find(x => x.ID == id);
            if (player == null) {
                if (true) {

                }

                player = new Player(id, name);
                players.Add(player);
            }
            return player;
        }
        public static Player GetPlayerByName(List<Player> players, string name)
        {
            Player player = players.Find(x => x.Name == name);
            if (player == null) {
                player = new Player(players.Count, name);
                players.Add(player);
            }
            return player;
        }
    }
}
