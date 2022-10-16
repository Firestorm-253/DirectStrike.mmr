using Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_1.dotNet.DTOs
{
    [Serializable]
    public record PlayerDTO
    {
        public DateTime DateTime { get; set; }

        public long? BattleNetId { get; set; }
        public Dictionary<Region, long> RegionIds { get; set; }

        public string Name { get; set; }
        public string Clan { get; set; }
        public double Score { get; set; }
        public double MMR { get; set; }
        public double Consistency { get; set; }
        public (Commander, int, double)[] CommanderMMRs  { get; set; }

        public PlayerDTO(Player player, DateTime dateTime)
        {
            this.DateTime = dateTime;

            this.BattleNetId = player.BattleNetId;
            this.RegionIds = player.RegionIds;

            this.Name = player.Name;
            this.Clan = player.Clan;
            this.Score = player.Score;
            this.MMR = player.MMR;
            this.Consistency = player.Consistency;
            this.CommanderMMRs = ((Commander, int, double)[])player.CommanderMMRs.Clone();
        }
    }
}
