using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_1.dotNet
{
    public class MatchupCombination
    {
        private double team_1_value = 1;
        private double team_2_value = 1;

        public Commander[] Commanders__team_1 { get; set; }
        public Commander[] Commanders__team_2 { get; set; }

        public MatchupCombination(Commander[] commanders__team_1, Commander[] commanders__team_2)
        {
            this.Commanders__team_1 = commanders__team_1;
            this.Commanders__team_2 = commanders__team_2;

            //ToDo
            //this.team_1_value = ;
            //this.team_2_value = ;
        }

        public double GetValueInRespectiveTo(int team)
        {
            if (team == 1) {
                return this.team_1_value;
            } else if (team == 2) {
                return this.team_2_value;
            }
            throw new Exception();
        }
        public int GetCommanderIndex(int team, int playerIndex)
        {
            if (team == 1) {
                return (int)this.Commanders__team_1[playerIndex];
            } else if (team == 2) {
                return (int)this.Commanders__team_2[playerIndex];
            }
            throw new Exception();
        }
    }
}
