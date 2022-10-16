using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_1.dotNet.DTOs
{
    public record GameDTO
    {
        public DateTime DateTime { get; }
        public int WinnerTeam { get; }

        public PlayerDTO[] Players__Team_1 { get; }
        public PlayerDTO[] Players__Team_2 { get; }

        public bool[] Leavers__team_1 { get; set; }
        public bool[] Leavers__team_2 { get; set; }

        public double TeamMMR_Change__Team_1 { get; }
        public double TeamMMR_Change__Team_2 { get; }

        public double[] Commander_Change__Team_1 { get; } = new double[3];
        public double[] Commander_Change__Team_2 { get; } = new double[3];

        public double[] PlayerMMR_Changes__Team_1 { get; } = new double[3];
        public double[] PlayerMMR_Changes__Team_2 { get; } = new double[3];

        public double[] Score_Changes__Team_1 { get; } = new double[3];
        public double[] Score_Changes__Team_2 { get; } = new double[3];

        public double[] Consistency_Changes__Team_1 { get; } = new double[3];
        public double[] Consistency_Changes__Team_2 { get; } = new double[3];

        public GameDTO(Game game)
        {
            this.DateTime = game.DateTime;
            this.WinnerTeam = game.WinnerTeam;

            this.Players__Team_1 = game.Players__Team_1.Select(x => x.CreateDTO(game.DateTime)).ToArray();
            this.Players__Team_2 = game.Players__Team_2.Select(x => x.CreateDTO(game.DateTime)).ToArray();

            this.Leavers__team_1 = (bool[])game.Leavers__team_1.Clone();
            this.Leavers__team_2 = (bool[])game.Leavers__team_2.Clone();

            this.TeamMMR_Change__Team_1 = game.TeamMMR_Change__Team_1;
            this.TeamMMR_Change__Team_2 = game.TeamMMR_Change__Team_2;

            this.Commander_Change__Team_1 = (double[])game.CommanderMMR_Changes__Team_1.Clone();
            this.Commander_Change__Team_2 = (double[])game.CommanderMMR_Changes__Team_2.Clone();

            this.PlayerMMR_Changes__Team_1 = (double[])game.PlayerMMR_Changes__Team_1.Clone();
            this.PlayerMMR_Changes__Team_2 = (double[])game.PlayerMMR_Changes__Team_2.Clone();

            this.Score_Changes__Team_1 = (double[])game.Score_Changes__Team_1.Clone();
            this.Score_Changes__Team_2 = (double[])game.Score_Changes__Team_2.Clone();

            this.Consistency_Changes__Team_1 = (double[])game.Consistency_Changes__Team_1.Clone();
            this.Consistency_Changes__Team_2 = (double[])game.Consistency_Changes__Team_2.Clone();
        }
    }
}
