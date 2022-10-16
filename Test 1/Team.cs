using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_1
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

        public bool IsEqual(Team team)
        {
            foreach (Player player in this.Players) {
                if (!team.Players.Any(x => x.ID == player.ID)) {
                    return false;
                }
            }
            return true;
        }

        public void ResetRatings()
        {
            this.MMR = Program.STARTING_MMR;
        }

        
        public static Team GetTeamByPlayerId(List<Player> players, List<Team> teams, int playerId_1, int playerId_2, int playerId_3)
        {
            var teamsFound = GetTeamsByPlayersIds(players, teams, new int[] { playerId_1, playerId_2, playerId_3 });
            if (teamsFound.Length == 0) {
                throw new Exception("ERROR: Team creation failed!");
            } else if (teamsFound.Length != 1) {
                throw new Exception("More than one team with the same players!");
            }

            return teamsFound.First();
        }
        public static Team[] GetTeamByPlayerId(List<Player> players, List<Team> teams, int playerId_1, int playerId_2)
        {
            var teamsFound = GetTeamsByPlayersIds(players, teams, new int[] { playerId_1, playerId_2 });
            if (teamsFound.Length == 0) {
                throw new Exception("ERROR: Team creation failed!");
            } else if (teamsFound.Length != 1) {
                throw new Exception("More than one team with the same players!");
            }

            return teamsFound;
        }
        public static Team[] GetTeamByPlayerId(List<Player> players, List<Team> teams, int playerId)
        {
            var teamsFound = GetTeamsByPlayersIds(players, teams, new int[] { playerId });
            if (teamsFound.Length == 0) {
                throw new Exception("ERROR: Team creation failed!");
            } else if (teamsFound.Length != 1) {
                throw new Exception("More than one team with the same players!");
            }

            return teamsFound;
        }
        private static Team[] GetTeamsByPlayersIds(List<Player> players, List<Team> teams, int[] playersIds)
        {
            Team[] teamsFound = teams.FindAll(t => t.Players.Count(x => playersIds.Contains(x.ID)) == playersIds.Length).ToArray();
            if (teamsFound.Any()) {
                return teamsFound;
            }

            if (playersIds.Length == 3) {
                Team team = new Team(new Player[] {
                    Player.GetPlayerById(players, playersIds[0]),
                    Player.GetPlayerById(players, playersIds[1]),
                    Player.GetPlayerById(players, playersIds[2]) });
                teams.Add(team);
                return new Team[] { team };
            }

            throw new Exception("Team doesn't exist and can't be created with less than 3 Players!");
        }
    }
}
