using Microsoft.Scripting.Utils;
using Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test_1.dotNet.DTOs;
using static IronPython.Modules._ast;

namespace test_1.dotNet
{
    [DebuggerDisplay("Score: {(int)Score} | MMR: {(int)MMR} | ({bestCommander.Item1} ({bestCommander.Item2}) = {(int)bestCommander.Item3}) => <{Clan}> {Name}")]
    public class Player
    {
        public long? BattleNetId { get; set; }
        public Dictionary<Region, long> RegionIds { get; set; }

        public string Name { get; set; }
        public DateTime NameClan_date { get; set; }
        public string Clan { get; set; }
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

        public (Commander, int, double)[] CommanderMMRs { get; set; } = new (Commander, int, double)[Commanders.Amount];
        private (Commander, int, double) bestCommander => this.CommanderMMRs.OrderByDescending(_ => _.Item3).First();
        public double GetCommanderMMR(MatchupCombination matchup, int team, int playerIndex)
            => this.CommanderMMRs[matchup.GetCommanderIndex(team, playerIndex)].Item3;

        private double _singleMMR = 0;
        public double MMR {
            //get {
            //    if (this.ValidSingleMMRGames >= Program.MIN_AMOUNT_MMR_GAMES_PER_PLAYER) {
            //        return this._singleMMR;
            //    }
            //    return Program.STARTING_MMR;
            //}
            get => this._singleMMR;
            set => this._singleMMR = value;
        }

        public int ValidSingleMMRGames { get; set; }
        public int TeamGames => this.Teams.Sum(x => x.Active ? x.Games.Count : 0);
        public int TotalGames => this.Teams.Sum(x => x.Games.Count);

        public List<Team> Teams { get; set; }

        public GameDTO[] GameDTOs => this.Teams.SelectMany(t => t.Games.Select(g => g.DTO)).OrderBy(_ => _.DateTime).ToArray();
        public PlayerDTO[] DTOs_overTime {
            get {
                return this.GameDTOs.Select((GameDTO g) => {
                    if (this.BattleNetId.HasValue) {
                        var found = g.Players__Team_1.Where(p => p.BattleNetId.HasValue ? p.BattleNetId.Equals(this.BattleNetId) : false);

                        if (!found.Any()) {
                            found = g.Players__Team_2.Where(p => p.BattleNetId.HasValue ? p.BattleNetId.Equals(this.BattleNetId) : false);
                        }

                        return found.First();
                    } else {
                        var found = g.Players__Team_1.Where(p => p.RegionIds.Any(x => this.RegionIds.Any(y => x.Equals(y))));

                        if (!found.Any()) {
                            found = g.Players__Team_2.Where(p => p.RegionIds.Any(x => this.RegionIds.Any(y => x.Equals(y))));
                        }

                        return found.First();
                    }
                }).ToArray();
            }
        }

        public Player(long? battleNetId, Dictionary<Region, long> regionIds, string name, string clan, DateTime nameClan_date)
        {
            this.BattleNetId = battleNetId;
            this.RegionIds = regionIds;

            this.Name = name;
            this.Clan = ((clan != null) ? clan : string.Empty);
            this.NameClan_date = nameClan_date;
            this.MMR = Program.STARTING_MMR;

            this.Teams = new List<Team>();
        }

        public PlayerDTO CreateDTO(DateTime dateTime)
        {
            return new PlayerDTO(this, dateTime);
        }

        public void AddTeam(Team team)
        {
            if (this.Teams.Any(x => x.Equals(team))) {
                return;
            }

            this.Teams.Add(team);
        }

        public void ResetRatings(bool score=true, bool singleMMR=true, bool singleMMRgames=true)
        {
            this.Consistency = 0.5;
            this.Score = score ? 0 : this.Score;

            if (singleMMR) {
                this.MMR = Program.STARTING_MMR;

                for (int i = 0; i < this.CommanderMMRs.Length; i++) {
                    this.CommanderMMRs[i] = ((Commander)i, 0, Program.STARTING_MMR);
                }
            }

            this.ValidSingleMMRGames = singleMMRgames ? 0 : this.ValidSingleMMRGames;
        }

        public void LinkTo(Player mainAccount)
        {
            foreach (Team team in this.Teams) {
                for (int i = 0; i < team.Players.Length; i++) {
                    if (team.Players[i].Equals(this)) {
                        team.Players[i] = mainAccount;
                    }
                }

                foreach (Game game in team.Games) {
                    for (int i = 0; i < team.Players.Length; i++) {
                        if (game.Players__Team_1[i].Equals(this)) {
                            game.Players__Team_1[i] = mainAccount;
                        }
                        if (game.Players__Team_2[i].Equals(this)) {
                            game.Players__Team_2[i] = mainAccount;
                        }
                    }

                    game.DTO = game.CreateDTO();
                }

                mainAccount.AddTeam(team);
            }

            this.CommanderMMRs = null;
            this.RegionIds = null;
            this.Teams = null;
        }



        public static Player GetPlayerById(List<Player> players, long? battleNetId, Dictionary<Region, long> regionIds, string? newName = null, string? newClan = null)
        {
            Player match = null;

            if (battleNetId.HasValue) {
                match = GetPlayerByBattleNetId(players, battleNetId.Value, regionIds, newName, newClan);

                Player[] regionPlayers = GetPlayersByRegionIds(players, regionIds).Where(_ => !_.Equals(match)).ToArray();
                foreach (Player regionPlayer in regionPlayers) {
                    regionPlayer.LinkTo(match);
                    players.Remove(regionPlayer);
                }
            } else {
                match = GetPlayerByRegionIds(players, regionIds, newName, newClan);
            }

            return match;
        }
        private static Player GetPlayerByBattleNetId(List<Player> players, long battleNetId, Dictionary<Region, long>? regionIds, string? newName = null, string? newClan = null)
        {
            Player match = players.Find(_ => _.BattleNetId.Equals(battleNetId));

            if (match == null) {
                if (regionIds == null) {
                    throw new Exception();
                }

                match = new Player(battleNetId, regionIds, newName, newClan, DateTime.UtcNow.AddYears(-20));
                players.Add(match);
            } else {
                if (newName != null) {
                    match.Name = newName;
                }
                if (newClan != null) {
                    match.Clan = newClan;
                }
            }

            return match;
        }

        private static Player[] GetPlayersByRegionIds(List<Player> players, Dictionary<Region, long> regionIds)
        {
            return players.FindAll(p => p.RegionIds.Any(x => regionIds.Any(y => y.Equals(x)))).ToArray();
        }
        private static Player GetPlayerByRegionIds(List<Player> players, Dictionary<Region, long> regionIds, string? newName = null, string? newClan=null)
        {
            Player match = players.Find(p => p.RegionIds.Any(x => regionIds.Any(y => y.Equals(x))));
            if (match == null) {
                match = new Player(null, regionIds, newName, newClan, DateTime.UtcNow.AddYears(-20));
                players.Add(match);
            } else {
                if (newName != null) {
                    match.Name = newName;
                }
                if (newClan != null) {
                    match.Clan = newClan;
                }

                foreach (KeyValuePair<Region, long> regionId in regionIds) {
                    if (!match.RegionIds.ContainsKey(regionId.Key)) {
                        match.RegionIds.Add(regionId.Key, regionId.Value);
                    }
                }
            }
            return match;
        }
    }
}
