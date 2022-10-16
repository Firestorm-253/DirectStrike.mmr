using Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_1.dotNet
{
    [DebuggerDisplay("MMR: {(int)MMR} | Count: {Games.Count} => ({Players[0].Name} | {Players[1].Name} | {Players[2].Name})")]
    public class Team
    {
        public double MMR { get; set; }
        public bool Active => this.Games.Count >= Program.MIN_AMOUNT_GAMES_PER_TEAM;

        public List<Game> Games { get; set; }
        public Player[] Players { get; set; }

        public Team(Player[] players)
        {
            this.Players = players;
            
            this.MMR = Program.STARTING_MMR;
            this.Games = new List<Game>();

            foreach (Player player in players) {
                player.AddTeam(this);
            }
        }

        public void AddGame(Game game)
        {
            if (this.Games.Contains(game)) {
                return;
            }

            this.Games.Add(game);
        }

        //public override bool Equals(object? obj)
        //{
        //    Team comparison = obj as Team;
        //    if (comparison == null) {
        //        throw new Exception("ERROR: Not a valid team!");
        //        //return false;
        //    }
        //}

        public void ResetRatings()
        {
            this.MMR = Program.STARTING_MMR;
        }

        private void LinkTo(Team mainTeam)
        {
            for (int p = 0; p < this.Players.Length; p++) {
                for (int t = 0; t < this.Players[p].Teams.Count; t++) {
                    if (this.Players[p].Teams[t].Equals(this)) {
                        this.Players[p].Teams[t] = mainTeam;
                    }
                }
            }

            foreach (Game game in this.Games) {
                if (game.Team_1.Equals(this)) {
                    game.Team_1 = mainTeam;
                }
                if (game.Team_2.Equals(this)) {
                    game.Team_2 = mainTeam;
                }

                game.DTO = game.CreateDTO();
            }
            mainTeam.Games.AddRange(this.Games);
        }



        public static bool Exists(List<Team> teams, long?[] battleNetIds, Dictionary<Region, long>[] regionsIds, out Team[] teamsFound)
        {
            List<Team> result = new List<Team>();

            foreach (Team team in teams) {
                bool same = true;

                for (int i = 0; i < battleNetIds.Length; i++) {
                    if (battleNetIds[i].HasValue) {
                        if (!team.Players.Any(p => p.BattleNetId.Equals(battleNetIds[i]))) {
                            same = false;
                        }
                    } else {
                        if (!team.Players.Any(p => p.RegionIds.Any(x => regionsIds[i].Any(y => y.Equals(x))))) {
                            same = false;
                        }
                    }
                }

                if (same) {
                    result.Add(team);
                }
            }

            teamsFound = result.ToArray();
            return teamsFound.Any();
        }
        public static bool Exists(List<Team> teams, long?[] battleNetIds, Dictionary<Region, long>[] regionsIds, out Team teamFound)
        {
            bool didFind = Team.Exists(teams, battleNetIds, regionsIds, out Team[] teamsFound);
            
            if (!didFind) {
                teamFound = null;
                return false;
            } else if (teamsFound.Length != 1) {
                foreach (Team team in teamsFound) {
                    if (team.Equals(teamsFound[0])) {
                        continue;
                    }

                    team.LinkTo(teamsFound[0]);
                    teams.Remove(team);
                }
                teamFound = teamsFound[0];
                return true;
            } else {
                teamFound = teamsFound[0];
                return true;
            }
        }

        public static Team GetTeamByPlayersIds(List<Player> players, List<Team> teams, long?[] battleNetIds, Dictionary<Region, long>[] regionsIds)
        {
            if (Team.Exists(teams, battleNetIds, regionsIds, out Team teamFound)) {
                return teamFound;
            }
            if (battleNetIds.Length.Equals(3) && regionsIds.Length.Equals(3)) {
                Team team = new Team(new Player[] {
                    Player.GetPlayerById(players, battleNetIds[0], regionsIds[0]),
                    Player.GetPlayerById(players, battleNetIds[1], regionsIds[1]),
                    Player.GetPlayerById(players, battleNetIds[2], regionsIds[2])
                });
                teams.Add(team);
                return team;
            }
            return null;
        }
    }
}
