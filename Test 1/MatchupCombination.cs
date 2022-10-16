using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_1
{
    public class MatchupCombination
    {
        private double team_1_value = 1;
        private double team_2_value = 1;

        public MatchupCombination(object[] commanders__team_1, object[] commanders__team_2)
        {
            //ToDo
            //this.team_1_value = ;
            //this.team_2_value = ;
        }

        public double GetValueInRespectiveTo(int teamIndex)
        {
            if (teamIndex == 0) {
                return this.team_1_value;
            } else {
                return this.team_2_value;
            }
        }
    }
}
