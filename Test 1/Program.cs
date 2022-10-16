using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_1
{
    public class Program
    {
        public static int MAX_DAYS_FOR_MMR = 180;

        public const double MAX_SINGLE_MULTIPLIER_FOR_LOW_GAME_AMOUNT = 1.10;
        public const double MMR_ADAPTION_MULTIPLIER = 32.0; //changes seem dangerous

        public const double STARTING_MMR = 1000;
        public const double MAX_MMR_DIFFERENCE_FOR_IMPACT = 400; /* called "clip"
            +200 clip -> +1000 maxMMR (+otherPlayers)
            400 clip = 2000 maxMMR above otherPlayers */

        public static int MIN_AMOUNT_GAMES_PER_TEAM = 10;
        public static int MIN_AMOUNT_MMR_GAMES_PER_PLAYER = 200;

        List<Player> players = new List<Player>();
        List<Team> teams = new List<Team>();
        List<Game> games = new List<Game>();

        public void Run()
        {
            //SortAndDeleteReplays(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Pizzaman\Account 1");

            string folder = @"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New";
            //LoadGamesFromReplayFolder(folder);
            LoadGamesFromReplayFolder(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Firestorm");
            //LoadGamesFromReplayFolder(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Dmivladi");
            //LoadGamesFromReplayFolder(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Basilic");
            //LoadGamesFromReplayFolder(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Venaris");
            //LoadGamesFromReplayFolder(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Reponix");
            //LoadGamesFromReplayFolder(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Heyrandom");
            //LoadGamesFromReplayFolder(@"C:\Users\Zehnder\Desktop\GameDatas\StarCraft II\Direct Strike\RankingSystem\Replays\New\Pizzaman");




            //while (true) {
            //    SetRatings(1);

            //    var validatedMMR_players = ValidatePlayers();
            //    var playersMMR_total = validatedMMR_players.OrderByDescending(x => x.MMR).ToArray();
            //    var playersMMR_team = validatedMMR_players.OrderByDescending(x => x.TeamMMR).ToArray();
            //    var playersMMR_single = validatedMMR_players.OrderByDescending(x => x.SingleMMR).ToArray();
            //    var teamsMMR = teams.OrderByDescending(x => x.MMR).ToArray();
            //    var playersplayers_score = players.OrderByDescending(x => x.Score).ToArray();

            //    //Print();
            //}

            SetRatings(1);
            Print();
        }

        private List<Player> ValidatePlayers()
        {
            List<Player> validated = new List<Player>();
            foreach (Player player in players) {
                if (player.ValidSingleMMRGames >= MIN_AMOUNT_MMR_GAMES_PER_PLAYER) {
                    validated.Add(player);
                }
            }
            return validated;
        }

        private void Print()
        {
            string strg_players_total_mmr = string.Empty;
            string strg_players_team_mmr = string.Empty;
            string strg_players_single_mmr = string.Empty;
            string strg_players_score = string.Empty;
            string strg_teams_mmr = string.Empty;

            var validatedMMR_players = ValidatePlayers();
            var players_total_mmr = validatedMMR_players.OrderByDescending(x => x.MMR).ToArray();
            var players_team_mmr = validatedMMR_players.OrderByDescending(x => x.TeamMMR).ToArray();
            var players_single_mmr = validatedMMR_players.OrderByDescending(x => x.SingleMMR).ToArray();
            var players_score = players.OrderByDescending(x => x.Score).ToArray();
            var teams_mmr = teams.OrderByDescending(x => x.MMR).ToArray();

            int counter = 0;
            for (int i = 0; i < validatedMMR_players.Count; i++) {
                strg_players_total_mmr += string.Format("Total: {0} | Count: {1} => {2}\n",
                    (int)players_total_mmr[i].MMR, players_total_mmr[i].ValidSingleMMRGames, players_total_mmr[i].Name);

                strg_players_team_mmr += string.Format("Team: {0} | Count: {1} => {2}\n",
                    (int)players_team_mmr[i].TeamMMR, players_team_mmr[i].ValidTeamMMRGames, players_team_mmr[i].Name);

                strg_players_single_mmr += string.Format("Single: {0} | Count: {1} => {2}\n",
                    (int)players_single_mmr[i].SingleMMR, players_single_mmr[i].ValidSingleMMRGames, players_single_mmr[i].Name);

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


            File.WriteAllText("Results\\players_players_total_mmr.txt", strg_players_total_mmr);
            File.WriteAllText("Results\\players_players_team_mmr.txt", strg_players_team_mmr);
            File.WriteAllText("Results\\players_players_single_mmr.txt", strg_players_single_mmr);
            File.WriteAllText("Results\\players_players_score.txt", strg_players_score);
            File.WriteAllText("Results\\teams_mmr.txt", strg_teams_mmr);
        }

        private void DeleteReplay(string file)
        {
            File.Delete(file);
            if (File.Exists(file)) {
            }
        }

        private void SortAndDeleteReplays(string folder)
        {
            string strg =
                "C:\\Users\\Zehnder\\Desktop\\GameDatas\\StarCraft II\\Direct Strike\\RankingSystem\\Replays\\New\\Pizzaman\\Account 1\\Direct Strike TE (71).SC2Replay"
                ;
            //DeleteReplay(strg);

            string[] files = Directory.GetFiles(folder).ToArray();

            int counter = 1 - 1;
            for (int i = counter; i < files.Length; i++) {
                string file = files[i];
                Nmpq.Sc2.Replays.Sc2Replay replay = Nmpq.Sc2.Replays.ReplayParser.ParseReplay(file);

                if ((replay.MapName != "Direct Strike" && replay.MapName != "Direct Strike TE")) {
                    DeleteReplay(file);
                }
                counter++;
            }
        }

        private void LoadGamesFromReplayFolder(string folder)
        {
            string[] uploaders = Directory.GetDirectories(folder);
            int totalAmount = Directory.GetFiles(folder, "*", SearchOption.AllDirectories).Length;
            int totalCounter = 0;
            int uploaderCounter = 0;

            foreach (string uploader in uploaders) {
                uploaderCounter = 0;
                string[] files = Directory.GetFiles(uploader, "*", SearchOption.AllDirectories);

                foreach (string file in files) {
                    string[] splittedFile = file.Split('\\');
                    string fileExtension = splittedFile.Last().Split('.').Last();
                    string playerFolder = splittedFile[splittedFile.Length - 3];

                    if (fileExtension != "SC2Replay") {
                        continue;
                    }
                    Game game = LoadGameFromReplay(file);
                    if (game != null) {
                        games.Add(game);
                    }
                    totalCounter++;
                    uploaderCounter++;

                    if (totalCounter % 100 == 0) {
                        Console.Clear();
                        Console.WriteLine("{0}\n{1} / {2}", playerFolder, uploaderCounter, files.Length);
                        Console.WriteLine("\nTotal:\n{0} / {1}", totalCounter, totalAmount);
                    }

                    //games = games.OrderBy(x => x.DateTime).ToList();
                    //SetRatings();
                    //var playersMMR_single = players.OrderByDescending(x => x.SingleMMR).ToArray();
                }
            }

            games = games.OrderBy(x => x.DateTime).ToList();
        }

        private void SetRatings(int repetition = 1)
        {
            foreach (Player player in this.players) {
                player.ResetRatings();
            }
            foreach (Team team in this.teams) {
                team.ResetRatings();
            }

            for (int r = 0; r < repetition; r++) {
                foreach (Player player in this.players) {
                    player.ResetRatings(score:true, singleMMRgames: true, teamMMRgames: true);
                }

                foreach (Game game in this.games) {
                    game.AddToRatings();
                }
            }
        }

        private int doublePlayerCount = 0;
        private int invalidTeamCount = 0;

        private Game LoadGameFromReplay(string file)
        {
            var replay = Nmpq.Sc2.Replays.ReplayParser.ParseReplay(file);
            if ((replay.MapName != "Direct Strike" && replay.MapName != "Direct Strike TE")) {
                DeleteReplay(file);
                return null;
            }

            if (replay.Players.Length != 6) {
                return null;
            }
            CombineNames(replay.Players);

            int winner = -1;

            List<Player> team_1__players = new List<Player>();
            List<Player> team_2__players = new List<Player>();

            for (int i = 0; i < 6; i++) {
                //if (replay.Players[i].Team == 1) {
                //    team_1__players.Add(Player.GetPlayerByName(players, replay.Players[i].Name));
                //} else {
                //    team_2__players.Add(Player.GetPlayerByName(players, replay.Players[i].Name));
                //}

                //if (i % 2 == 0) {
                //    team_1__players.Add(Player.GetPlayerByName(players, replay.Players[i].Name));
                //} else {
                //    team_2__players.Add(Player.GetPlayerByName(players, replay.Players[i].Name));
                //}

                if (replay.Players[i].Team != (i % 2) + 1) {
                }
            }

            if (team_1__players.Count != team_2__players.Count) {
                invalidTeamCount++;
                return null;
            }

            for (int i = 0; i < 3; i++) {
                if (replay.Players.Count(x => (x.Name == team_1__players[i].Name)) > 1) {
                    doublePlayerCount++;
                    return null;
                }
                if (replay.Players.Count(x => (x.Name == team_2__players[i].Name)) > 1) {
                    doublePlayerCount++;
                    return null;
                }

                if (replay.Players[i].Won) {
                    winner = 0;
                } else if (replay.Players[i + 3].Won) {
                    winner = 1;
                }
            }

            if (winner == -1) {
                return null;
            }

            var doubleCheck = games.Where(x => x.WinnerIndex == winner && Math.Abs(x.DateTime.Subtract(replay.DatePlayed).TotalMinutes) < 1);
            if (doubleCheck.Any()) {
                bool same = true;
                foreach (var item in doubleCheck) {
                    for (int i = 0; i < 3; i++) {
                        if (!item.Team_1.Players.Any(x => x.Name == replay.Players[i].Name)) {
                            same = false;
                            break;
                        }
                        if (!item.Team_2.Players.Any(x => x.Name == replay.Players[i + 3].Name)) {
                            same = false;
                            break;
                        }
                    }

                    if (!same) {
                        break;
                    }
                }

                if (same) {
                    return null;
                }
            }
            
            Team team_1 = Team.GetTeamByPlayerId(this.players, this.teams, team_1__players[0].ID, team_1__players[1].ID, team_1__players[2].ID);
            Team team_2 = Team.GetTeamByPlayerId(this.players, this.teams, team_2__players[0].ID, team_2__players[1].ID, team_2__players[2].ID);

            return new Game(replay.DatePlayed, winner, team_1, team_2, null, null);
        }

        private static void CombineNames(Nmpq.Sc2.Replays.Sc2ReplayPlayer[] players)
        {
            foreach (var player in players) {
                if (player.Name == "LGBTQIAA") {
                    player.Name = "heyrandoms";
                } else if (player.Name == "ironlionzion") {
                    player.Name = "IronLionZion";
                } else if(player.Name == "DejaVu") {
                    player.Name = "shame";
                } else if (player.Name == "EchoMemori" || player.Name == "MemoriAeni" || player.Name == "Aranairen") {
                    player.Name = "Amemiya";
                }
            }
        }
    }
}
