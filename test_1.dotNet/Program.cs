using Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using test_1.dotNet.DTOs;

namespace test_1.dotNet
{
    public class Program
    {
        public static int MAX_MONTHS_FOR_MMR = 6;
        public static int MAX_DAYS_FOR_MMR = (int)DateTime.UtcNow.Subtract(DateTime.UtcNow.AddMonths(-MAX_MONTHS_FOR_MMR)).TotalDays;
        public static double STARTING_MMR = 100;

        public static double CONSISTENCY_IMPACT = 0.50;
        public static double CONSISTENCY_CHANGE = 0.15;
        public static double SCORE_WIN_LOOSE_VALUE = 1;

        //Important! don't change multiplier without changing clip aswell!!!
        public static double MMR_ADAPTION_MULTIPLIER = 64;
        public static double MAX_MMR_DIFFERENCE_FOR_IMPACT = MMR_ADAPTION_MULTIPLIER * 12.5; // called "clip"

        public static int MIN_AMOUNT_GAMES_PER_TEAM = 10;
        public static int MIN_AMOUNT_MMR_GAMES_PER_PLAYER = 200;

        List<Player> players = new List<Player>();
        List<Team> teams = new List<Team>();
        List<Game> games = new List<Game>();

        public void Run()
        {
            LoadGamesFromReplayFolder(@"Replays", true);

            while (true) {
                SetRatings();

                var validatedMMR_players = GetValidatedMMR_Players();
                var playersMMR = validatedMMR_players.OrderByDescending(x => x.MMR).ToArray();
                var teamsMMR = teams.OrderByDescending(x => x.MMR).ToArray();
                var playersplayers_score = players.OrderByDescending(x => x.Score).ToArray();
                var allPlayersMMR = this.players.OrderByDescending(x => x.MMR).ToArray();

                //PrintRatings();
            }
        }

        public void Run(out PlayerDTO[][] validatedPlayersDTOs)
        {
            this.LoadGamesFromReplayFolder(@"Replays", true);
            this.SetRatings();

            validatedPlayersDTOs = GetValidatedPlayersDTOs();
        }

        public PlayerDTO[][] GetValidatedPlayersDTOs()
        {
            var validatedPlayers = GetValidatedMMR_Players();
            PlayerDTO[][] validatedPlayersDTOs = new PlayerDTO[validatedPlayers.Count][];

            for (int i = 0; i < validatedPlayers.Count; i++) {
                validatedPlayersDTOs[i] = validatedPlayers[i].DTOs_overTime;
            }
            return validatedPlayersDTOs;
        }

        public List<Player> GetValidatedMMR_Players()
        {
            List<Player> validated = new List<Player>();
            foreach (Player player in players) {
                if (player.ValidSingleMMRGames >= MIN_AMOUNT_MMR_GAMES_PER_PLAYER) {
                    validated.Add(player);
                }
            }
            return validated;
        }

        public void PrintRatings()
        {
            string strg_players_mmr = string.Empty;
            string strg_players_score = string.Empty;
            string strg_teams_mmr = string.Empty;

            var validatedMMR_players = GetValidatedMMR_Players();
            var players_mmr = validatedMMR_players.OrderByDescending(x => x.MMR).ToArray();
            var players_score = players.OrderByDescending(x => x.Score).ToArray();
            var teams_mmr = teams.OrderByDescending(x => x.MMR).ToArray();

            int counter = 0;
            for (int i = 0; i < validatedMMR_players.Count; i++) {
                strg_players_mmr += string.Format("Total: {0} | Count: {1} => {2}\n",
                    (int)players_mmr[i].MMR, players_mmr[i].ValidSingleMMRGames, players_mmr[i].Name);

                strg_players_score += string.Format("players_score: {0} | Count: {1} => {2}\n",
                    (int)players_score[i].Score, players_score[i].TotalGames, players_score[i].Name);

                counter++;
                if (counter % 100 == 0) {
                    Console.Clear();
                    Console.WriteLine("Print");
                    Console.WriteLine("{0} / {1}", i, players.Count);
                }
            }

            counter = 0;
            for (int i = 0; i < teams.Count; i++) {
                strg_teams_mmr += string.Format("MMR: {0} | Count: {1} => ({2} | {3} | {4})\n",
                    (int)teams_mmr[i].MMR, teams_mmr[i].Games.Count, teams_mmr[i].Players[0].Name, teams_mmr[i].Players[1].Name, teams_mmr[i].Players[2].Name);

                counter++;
                if (counter % 100 == 0) {
                    Console.Clear();
                    Console.WriteLine("Print");
                    Console.WriteLine("{0} / {1}", players.Count, players.Count);
                    Console.WriteLine("{0} / {1}", i, teams.Count);
                }
            }


            File.WriteAllText("Results\\players_players_mmr.txt", strg_players_mmr);
            File.WriteAllText("Results\\players_players_score.txt", strg_players_score);
            File.WriteAllText("Results\\teams_mmr.txt", strg_teams_mmr);
        }

        public void LoadGamesFromReplayFolder(string folder = @"Replays", bool console = true)
        {
            string[] uploaders = Directory.GetFiles(folder, "*.ds-replays");
            int uploaderCounter = 0;

            foreach (string uploader in uploaders) {
                uploaderCounter++;
                string uploaderName = uploader.Split('\\').Last().Split('.').First();

                var timeBefore = DateTime.Now;
                int gamesAmountBefore = this.games.Count;
                int teamsAmountBefore = this.teams.Count;
                int playerAmountBefore = this.players.Count;

                List<SerializableGame> deserializedGames = Decoder.DeserializeReplays(uploader);
                for (int i = 0; i < deserializedGames.Count; i++) {
                    Game game = LoadGameFromDeserializedGame(deserializedGames[i]);

                    if ((DateTime.Now - timeBefore).TotalSeconds > 0.5) {
                        if (console) {
                            Console.Clear();
                            Console.WriteLine("{0} ({1} / {2})", uploaderName, uploaderCounter, uploaders.Length);
                            Console.WriteLine("{0} / {1}", i, deserializedGames.Count);

                            Console.WriteLine("\n{0} games (+{1})", this.games.Count, (this.games.Count - gamesAmountBefore));
                            Console.WriteLine("{0} teams (+{1})", this.teams.Count, (this.teams.Count - teamsAmountBefore));
                            Console.WriteLine("{0} players (+{1})", this.players.Count, (this.players.Count - playerAmountBefore));
                            Console.WriteLine("{0}ms", (int)(DateTime.Now - timeBefore).TotalMilliseconds);
                        }

                        timeBefore = DateTime.Now;
                        gamesAmountBefore = this.games.Count;
                        teamsAmountBefore = this.teams.Count;
                        playerAmountBefore = this.players.Count;
                    }

                    if (game != null) {
                        this.AddGameAtCorrectIndex(game);
                    }
                }

                //foreach (Player player in this.players) {

                //}
            }
        }

        private void AddGameAtCorrectIndex(Game game)
        {
            for (int i = this.games.Count - 1; i >= 0; i--) {
                if (game.Equals(this.games[i])) {
                    check_doubleGamesCount++;
                    return;
                }

                if (game.DateTime.CompareTo(this.games[i].DateTime) > 0) {
                    this.games.Insert(i + 1, game);
                    return;
                }
            }

            this.games.Insert(0, game);
        }

        public void SetRatings()
        {
            foreach (Player player in this.players) {
                player.ResetRatings();
            }
            foreach (Team team in this.teams) {
                team.ResetRatings();
            }

            foreach (Game game in this.games) {
                game.AddToRatings();
            }
        }

        private int check_doubleGamesCount = 0;
        private int check_doublePlayerCount = 0;
        private int check_none3v3CommandersCount = 0;
        private int check_DateTimeCount = 0;

        private Game LoadGameFromDeserializedGame(SerializableGame deserializedGame)
        {
            if (deserializedGame.Players.Any(_ => _.CurrentName.Equals("Firestorm") && _.Id.Item1.HasValue)) {
            }
            if (deserializedGame.Players.Any(_ => _.CurrentName.Equals("Dmivladi") && _.Id.Item1.HasValue)) {
            }

            if (deserializedGame.DateTime.Date == DateTime.UtcNow.Date) {
            }

            if (deserializedGame.Players.Count != 6 || deserializedGame.GameMode != "GameModeCommanders") {
                check_none3v3CommandersCount++;
                return null;
            }

            if ((DateTime.Now - deserializedGame.DateTime).TotalDays > MAX_DAYS_FOR_MMR) {
                check_DateTimeCount++;
                return null;
            }

            List<Player> players__team_1 = new List<Player>();
            List<Player> players__team_2 = new List<Player>();

            List<string> commanders__team_1 = new List<string>();
            List<string> commanders__team_2 = new List<string>();

            List<bool> leavers__team_1 = new List<bool>();
            List<bool> leavers__team_2 = new List<bool>();
            
            foreach (SerializablePlayer deserializedPlayer in deserializedGame.Players.OrderBy(_ => _.InGamePosition)) {
                long? battleNetId = deserializedPlayer.Id.Item1;
                Dictionary<Region, long> regionIds = deserializedPlayer.Id.Item2;
                
                Player player = Player.GetPlayerById(players, battleNetId, regionIds, deserializedPlayer.CurrentName, deserializedPlayer.CurrentClan);

                if (deserializedPlayer.Team == 1) {
                    players__team_1.Add(player);
                    commanders__team_1.Add(deserializedPlayer.Commander);
                    leavers__team_1.Add(deserializedPlayer.IsLeaver);
                } else if (deserializedPlayer.Team == 2) {
                    players__team_2.Add(player);
                    commanders__team_2.Add(deserializedPlayer.Commander);
                    leavers__team_2.Add(deserializedPlayer.IsLeaver);
                } else {
                    throw new Exception();
                }
            }

            if (deserializedGame.HasLeavers) {
            }

            Team team_1 = Team.GetTeamByPlayersIds(this.players, this.teams,
                players__team_1.Select(_ => _.BattleNetId).ToArray(), players__team_1.Select(_ => _.RegionIds).ToArray());
            Team team_2 = Team.GetTeamByPlayersIds(this.players, this.teams,
                players__team_2.Select(_ => _.BattleNetId).ToArray(), players__team_2.Select(_ => _.RegionIds).ToArray());

            if (deserializedGame.Players.Select(_ => _.Commander).Count(x => Commanders.AllCommanderNames.Any(y => x.Equals(y))) < 6) {
                return null;
            }

            if (deserializedGame.HasLeavers) {

            }

            return new Game(deserializedGame.DateTime, deserializedGame.WinnerTeam,
                team_1, team_2,
                players__team_1, players__team_2,
                commanders__team_1.Select(Commanders.Get).ToArray(), commanders__team_2.Select(Commanders.Get).ToArray(),
                leavers__team_1.ToArray(), leavers__team_2.ToArray());
        }

        private (int[], string, string)[] GetDoubleAccounts()
        {
            return new (int[], string, string)[] {
                (new int[] { 1781823, 8598649 }, "Basilic", ""),
                (new int[] { 8429108, 7128994 }, "InsaneSmoker", "ClanHs"),
                (new int[] { 11519644, 10071777 }, "Shame", "ClanHs"),
                (new int[] { 8509078, 12564643 }, "Firestorm", "TPROCT"),
                (new int[] { 10033229, 12423164 }, "Dmivladi", "TPROCT"),
                (new int[] { 10975778, 9718696 }, "Heyrandoms", "ClanHs"),
                (new int[] { 5536342, 20825889 }, "TITAN","DSRTN"),
                (new int[] { 1240799, 11024704 }, "VeNaRiS", "ClanHs"),
                (new int[] { 9068543, 11292844 }, "Yanusson", "DSoP"),
                (new int[] { 7737962, 10260644 }, "Skippy", "TPROCT"),
            };
        }
    }
}
