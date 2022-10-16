using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test_1.dotNet.DTOs;

namespace test_1.dotNet
{
    public class Game
    {
        public GameDTO DTO { get; set; }

        public DateTime DateTime { get; set; }
        public int WinnerTeam { get; set; }

        public Team Team_1 { get; set; }
        public Team Team_2 { get; set; }

        public List<Player> Players__Team_1 { get; set; }
        public List<Player> Players__Team_2 { get; set; }

        public bool[] Leavers__team_1 { get; set; }
        public bool[] Leavers__team_2 { get; set; }

        public double TeamMMR_Change__Team_1 { get; private set; }
        public double TeamMMR_Change__Team_2 { get; private set; }

        public double[] CommanderMMR_Changes__Team_1 { get; private set; } = new double[3];
        public double[] CommanderMMR_Changes__Team_2 { get; private set; } = new double[3];

        public double[] PlayerMMR_Changes__Team_1 { get; private set; } = new double[3];
        public double[] PlayerMMR_Changes__Team_2 { get; private set; } = new double[3];

        public double[] Score_Changes__Team_1 { get; private set; } = new double[3];
        public double[] Score_Changes__Team_2 { get; private set; } = new double[3];

        public double[] Consistency_Changes__Team_1 { get; private set; } = new double[3];
        public double[] Consistency_Changes__Team_2 { get; private set; } = new double[3];
        
        public MatchupCombination MatchupCombination { get; set; }

        public Game(DateTime dateTime, int winnerTeam,
            Team team_1, Team team_2,
            List<Player> players__Team_1, List<Player> players__Team_2,
            Commander[] commanders__team_1, Commander[] commanders__team_2,
            bool[] leavers__team_1, bool[] leavers__team_2)
        {
            this.DateTime = dateTime;
            this.WinnerTeam = winnerTeam;

            this.Team_1 = team_1;
            this.Team_2 = team_2;

            this.Players__Team_1 = players__Team_1;
            this.Players__Team_2 = players__Team_2;

            this.Leavers__team_1 = leavers__team_1;
            this.Leavers__team_2 = leavers__team_2;

            this.MatchupCombination = new MatchupCombination(commanders__team_1, commanders__team_2);

            this.Team_1.AddGame(this);
            this.Team_2.AddGame(this);
        }

        public GameDTO CreateDTO()
        {
            return new GameDTO(this);
        }

        public override bool Equals(object? obj)
        {
            Game comparison = obj as Game;
            if (comparison == null) {
                throw new Exception("ERROR: Not a valid game!");
                //return false;
            }

            if (this.DateTime.Date != comparison.DateTime.Date) {
                return false;
            }
            if (((int)this.DateTime.TimeOfDay.TotalMinutes) != ((int)comparison.DateTime.TimeOfDay.TotalMinutes)) {
                return false;
            }
            if (this.WinnerTeam != comparison.WinnerTeam) {
                return false;
            }
            if (this.Team_1 != comparison.Team_1) {
                return false;
            }
            if (this.Team_2 != comparison.Team_2) {
                return false;
            }

            return true;
        }

        private static double GetCorrected_revConsistency(double raw_revConsistency)
        {
            // formula correct???
            return ((1 - Program.CONSISTENCY_IMPACT) + (Program.CONSISTENCY_IMPACT * raw_revConsistency));
        }

        public void AddToRatings()
        {
            double mcv__team_1 = this.MatchupCombination.GetValueInRespectiveTo(1);
            double mcv__team_2 = this.MatchupCombination.GetValueInRespectiveTo(2);

            double playerMMR_sum__team_1 = this.Players__Team_1.Sum(x => x.MMR);
            double playerMMR_sum__team_2 = this.Players__Team_2.Sum(x => x.MMR);
            double playerMMR_sum__team_1_v2 = this.Players__Team_1.Sum(_ => GetCorrected_revConsistency(1 - _.Consistency) * _.MMR);
            double playerMMR_sum__team_2_v2 = this.Players__Team_2.Sum(_ => GetCorrected_revConsistency(1 - _.Consistency) * _.MMR);

            double commanderMMR_sum__team_1 = this.Players__Team_1.Sum(x => x.GetCommanderMMR(this.MatchupCombination, 1, this.Players__Team_1.IndexOf(x)));
            double commanderMMR_sum__team_2 = this.Players__Team_2.Sum(x => x.GetCommanderMMR(this.MatchupCombination, 2, this.Players__Team_2.IndexOf(x)));
            double commanderMMR_sum__team_1_v2 = this.Players__Team_1.Sum(_ => GetCorrected_revConsistency(1 - _.Consistency) * _.GetCommanderMMR(this.MatchupCombination, 1, this.Players__Team_1.IndexOf(_)));
            double commanderMMR_sum__team_2_v2 = this.Players__Team_2.Sum(_ => GetCorrected_revConsistency(1 - _.Consistency) * _.GetCommanderMMR(this.MatchupCombination, 2, this.Players__Team_2.IndexOf(_)));

            double score_sum__team_1 = this.Players__Team_1.Sum(x => x.Score);
            double score_sum__team_2 = this.Players__Team_2.Sum(x => x.Score);


            double teamElo__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (this.Team_2.MMR - this.Team_1.MMR)));
            double teamElo__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (this.Team_1.MMR - this.Team_2.MMR)));

            double playersElo__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (playerMMR_sum__team_2 - playerMMR_sum__team_1)));
            double playersElo__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (playerMMR_sum__team_1 - playerMMR_sum__team_2)));

            double commandersElo__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (commanderMMR_sum__team_2 - commanderMMR_sum__team_1)));
            double commandersElo__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (commanderMMR_sum__team_1 - commanderMMR_sum__team_2)));
            
            double scoreElo__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((score_sum__team_2 - score_sum__team_1) / 3)));
            double scoreElo__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((score_sum__team_1 - score_sum__team_2) / 3)));


            double[] playersImpacts__team_1 = new double[3];
            double[] playersImpacts__team_2 = new double[3];

            double[] commandersImpacts__team_1 = new double[3];
            double[] commandersImpacts__team_2 = new double[3];
            
            if (this.DateTime.Date == DateTime.Today.AddDays(-1)) {
            }


            for (int i = 0; i < 3; i++) {

                //----------------------------------------------------------

                //team_1__playersELOs[i] = (2.0 / 3.0) * 0.5;
                //team_2__playersELOs[i] = (2.0 / 3.0) * 0.5;

                //----------------------------------------------------------

                //double factor_playerToTeamMates__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT *
                //    (((playerMMR_sum__team_1 - this.Players__Team_1[i].MMR) / 2) - this.Players__Team_1[i].MMR)));
                //double factor_playerToTeamMates__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT *
                //    (((playerMMR_sum__team_2 - this.Players__Team_2[i].MMR) / 2) - this.Players__Team_2[i].MMR)));

                //double factor_commanderToTeamMatesCommander__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT *
                //    (((commanderMMR_sum__team_1 - this.Players__Team_1[i].GetCommanderMMR(this.MatchupCombination, 1, i)) / 2) - this.Players__Team_1[i].GetCommanderMMR(this.MatchupCombination, 1, i))));
                //double factor_commanderToTeamMatesCommander__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT *
                //    (((commanderMMR_sum__team_2 - this.Players__Team_2[i].GetCommanderMMR(this.MatchupCombination, 2, i)) / 2) - this.Players__Team_2[i].GetCommanderMMR(this.MatchupCombination, 2, i))));


                double factor_playerToTeamMates__team_1 = ((playerMMR_sum__team_1 == 0) ? (1.0 / 3) : (this.Players__Team_1[i].MMR) / playerMMR_sum__team_1);
                double factor_playerToTeamMates__team_2 = ((playerMMR_sum__team_2 == 0) ? (1.0 / 3) : (this.Players__Team_2[i].MMR) / playerMMR_sum__team_2);

                double factor_playerToTeamMates__team_1_v2 = ((playerMMR_sum__team_1_v2 == 0) ? (1.0 / 3) : (GetCorrected_revConsistency(1 - this.Players__Team_1[i].Consistency) * this.Players__Team_1[i].MMR) / playerMMR_sum__team_1_v2);
                double factor_playerToTeamMates__team_2_v2 = ((playerMMR_sum__team_2_v2 == 0) ? (1.0 / 3) : (GetCorrected_revConsistency(1 - this.Players__Team_2[i].Consistency) * this.Players__Team_2[i].MMR) / playerMMR_sum__team_2_v2);


                double factor_commanderToTeamMatesCommander__team_1 = ((commanderMMR_sum__team_1 == 0) ? (1.0 / 3) : (this.Players__Team_1[i].GetCommanderMMR(this.MatchupCombination, 1, i) / commanderMMR_sum__team_1));
                double factor_commanderToTeamMatesCommander__team_2 = ((commanderMMR_sum__team_2 == 0) ? (1.0 / 3) : (this.Players__Team_2[i].GetCommanderMMR(this.MatchupCombination, 2, i) / commanderMMR_sum__team_2));

                double factor_commanderToTeamMatesCommander__team_1_v2 = ((commanderMMR_sum__team_1_v2 == 0) ? (1.0 / 3) : (GetCorrected_revConsistency(1 - this.Players__Team_1[i].Consistency) * this.Players__Team_1[i].GetCommanderMMR(this.MatchupCombination, 1, i) / commanderMMR_sum__team_1_v2));
                double factor_commanderToTeamMatesCommander__team_2_v2 = ((commanderMMR_sum__team_2_v2 == 0) ? (1.0 / 3) : (GetCorrected_revConsistency(1 - this.Players__Team_2[i].Consistency) * this.Players__Team_2[i].GetCommanderMMR(this.MatchupCombination, 2, i) / commanderMMR_sum__team_2_v2));


                //double rev_consistencyImpact__team_1 = (rev_consistency_sum__team_1) == 0 ? (1.0 / 3) : ((1 - GetCorrected_revConsistency(this.Players__Team_1[i].Consistency)) / (rev_consistency_sum__team_1));
                //double rev_consistencyImpact__team_2 = (rev_consistency_sum__team_2) == 0 ? (1.0 / 3) : ((1 - GetCorrected_revConsistency(this.Players__Team_2[i].Consistency)) / (rev_consistency_sum__team_2));

                //rev_consistencyImpacts__team_1[i] = rev_consistencyImpact__team_1;
                //rev_consistencyImpacts__team_2[i] = rev_consistencyImpact__team_2;

                playersImpacts__team_1[i] = factor_playerToTeamMates__team_1 * GetCorrected_revConsistency(1 - this.Players__Team_1[i].Consistency);     //(0.50 * factor_playerToTeamMates__team_1) + (0.50 * factor_playerToEnemy__team_1);
                playersImpacts__team_2[i] = factor_playerToTeamMates__team_2 * GetCorrected_revConsistency(1 - this.Players__Team_2[i].Consistency);     //(0.50 * factor_playerToTeamMates__team_2) + (0.50 * factor_playerToEnemy__team_2);

                commandersImpacts__team_1[i] = factor_commanderToTeamMatesCommander__team_1 * GetCorrected_revConsistency(1 - this.Players__Team_1[i].Consistency);     //(0.50 * factor_playerToTeamMates__team_1) + (0.50 * factor_playerToEnemy__team_1);
                commandersImpacts__team_2[i] = factor_commanderToTeamMatesCommander__team_2 * GetCorrected_revConsistency(1 - this.Players__Team_2[i].Consistency);     //(0.50 * factor_playerToTeamMates__team_2) + (0.50 * factor_playerToEnemy__team_2);

                //----------------------------------------------------------
            }

            double teamMMR_Change__team_1 = 0;
            double teamMMR_Change__team_2 = 0;
            double[] commanderMMR_Changes__team_1 = new double[3];
            double[] commanderMMR_Changes__team_2 = new double[3];
            double[] playerMMR_Changes__team_1 = new double[3];
            double[] playerMMR_Changes__team_2 = new double[3];
            double[] score_Changes__team_1 = new double[3];
            double[] score_Changes__team_2 = new double[3];
            double[] consistency_Changes__team_1 = new double[3];
            double[] consistency_Changes__team_2 = new double[3];
            
            if (this.WinnerTeam == 1) {
                SetWinnerChanges(mcv__team_1, scoreElo__team_1, teamElo__team_1, playersElo__team_1, commandersElo__team_1, playersImpacts__team_1, commandersImpacts__team_1,
                    out teamMMR_Change__team_1, playerMMR_Changes__team_1, score_Changes__team_1, consistency_Changes__team_1, commanderMMR_Changes__team_1);

                SetLooserChanges(mcv__team_2, scoreElo__team_2, teamElo__team_2, playersElo__team_2, commandersElo__team_2, playersImpacts__team_2, commandersImpacts__team_2,
                    out teamMMR_Change__team_2, playerMMR_Changes__team_2, score_Changes__team_2, consistency_Changes__team_2, commanderMMR_Changes__team_2);
            } else {
                SetWinnerChanges(mcv__team_2, scoreElo__team_2, teamElo__team_2, playersElo__team_2, commandersElo__team_2, playersImpacts__team_2, commandersImpacts__team_2,
                    out teamMMR_Change__team_2, playerMMR_Changes__team_2, score_Changes__team_2, consistency_Changes__team_2, commanderMMR_Changes__team_2);

                SetLooserChanges(mcv__team_1, scoreElo__team_1, teamElo__team_1, playersElo__team_1, commandersElo__team_1, playersImpacts__team_1, commandersImpacts__team_1,
                    out teamMMR_Change__team_1, playerMMR_Changes__team_1, score_Changes__team_1, consistency_Changes__team_1, commanderMMR_Changes__team_1);
            }

            this.TeamMMR_Change__Team_1 = teamMMR_Change__team_1;
            this.TeamMMR_Change__Team_2 = teamMMR_Change__team_2;
            this.CommanderMMR_Changes__Team_1 = commanderMMR_Changes__team_1;
            this.CommanderMMR_Changes__Team_2 = commanderMMR_Changes__team_2;
            this.PlayerMMR_Changes__Team_1 = playerMMR_Changes__team_1;
            this.PlayerMMR_Changes__Team_2 = playerMMR_Changes__team_2;
            this.Score_Changes__Team_1 = score_Changes__team_1;
            this.Score_Changes__Team_2 = score_Changes__team_2;
            this.Consistency_Changes__Team_1 = consistency_Changes__team_1;
            this.Consistency_Changes__Team_2 = consistency_Changes__team_2;

            FixMMR_Equality(this.CommanderMMR_Changes__Team_1, this.CommanderMMR_Changes__Team_2);
            FixMMR_Equality(this.PlayerMMR_Changes__Team_1, this.PlayerMMR_Changes__Team_2);


            if (PlayerMMR_Changes__Team_1.Sum(_ => Math.Abs(_)) != PlayerMMR_Changes__Team_2.Sum(_ => Math.Abs(_))) {
            }



            this.SetRatings();
            this.DTO = this.CreateDTO();

            if (biggestMMR_gain != null) {
                if (Math.Max(this.CommanderMMR_Changes__Team_1.Max(), this.CommanderMMR_Changes__Team_2.Max()) > Math.Max(biggestMMR_gain.CommanderMMR_Changes__Team_1.Max(), biggestMMR_gain.CommanderMMR_Changes__Team_2.Max())) {
                    biggestMMR_gain = this;
                }
            } else {
                biggestMMR_gain = this;
            }

            if (biggestMMR_drop != null) {
                if (Math.Min(this.CommanderMMR_Changes__Team_1.Min(), this.CommanderMMR_Changes__Team_2.Min()) < Math.Min(biggestMMR_drop.CommanderMMR_Changes__Team_1.Min(), biggestMMR_drop.CommanderMMR_Changes__Team_2.Min())) {
                    biggestMMR_drop = this;
                }
            } else {
                biggestMMR_drop = this;
            }
        }

        public static Game biggestMMR_gain = null;
        public static Game biggestMMR_drop = null;

        private void SetWinnerChanges(
            in double mcv, in double scoreElo, in double teamElo, in double playersElo, in double commandersElo, in double[] playersImpacts, in double[] commandersImpacts,
            out double teamMMR_Change, double[] singleMMR_changes, double[] scoreChanges, double[] consistency_Changes, double[] commanderMMR_Changes)
        {
            teamMMR_Change = (Program.MMR_ADAPTION_MULTIPLIER * mcv * (1.0 - teamElo));

            for (int i = 0; i < 3; i++) {
                scoreChanges[i] = Program.SCORE_WIN_LOOSE_VALUE * (1 + (mcv * (1 - scoreElo) * (playersImpacts[i] * commandersImpacts[i]) / 2));
                
                singleMMR_changes[i] = Program.MMR_ADAPTION_MULTIPLIER * mcv * (1 - playersElo) * playersImpacts[i];
                commanderMMR_Changes[i] = Program.MMR_ADAPTION_MULTIPLIER * mcv * (1 - commandersElo) * commandersImpacts[i];

                consistency_Changes[i] = Program.CONSISTENCY_CHANGE * 2 * (((playersElo + commandersElo) / 2) - 0.50);
            }
        }
        private void SetLooserChanges(
            in double mcv, in double scoreElo, in double teamElo, in double playersElo, in double commandersElo, in double[] playersImpacts, in double[] commandersImpacts,
            out double teamMMR_Change, double[] singleMMR_changes, double[] scoreChanges, double[] consistency_Changes, double[] commanderMMR_Changes)
        {
            teamMMR_Change = -(Program.MMR_ADAPTION_MULTIPLIER * mcv * teamElo);

            for (int i = 0; i < 3; i++) {
                scoreChanges[i] = -(Program.SCORE_WIN_LOOSE_VALUE * (1 + (mcv * scoreElo * (playersImpacts[i] * commandersImpacts[i]) / 2)));

                singleMMR_changes[i] = -(Program.MMR_ADAPTION_MULTIPLIER * mcv * playersElo * playersImpacts[i]);
                commanderMMR_Changes[i] = -(Program.MMR_ADAPTION_MULTIPLIER * mcv * commandersElo * commandersImpacts[i]);

                consistency_Changes[i] = Program.CONSISTENCY_CHANGE * 2 * (0.50 - ((playersElo + commandersElo) / 2));
            }
        }

        private void FixMMR_Equality(double[] mmr_Changes__Team_1, double[] mmr_Changes__Team_2)
        {
            double abs_sumMMRchanges__team_1 = Math.Abs(mmr_Changes__Team_1.Sum());
            double abs_sumMMRchanges__team_2 = Math.Abs(mmr_Changes__Team_2.Sum());

            for (int i = 0; i < 3; i++) {
                mmr_Changes__Team_1[i] = mmr_Changes__Team_1[i] *
                    ((abs_sumMMRchanges__team_1 + abs_sumMMRchanges__team_2) / (abs_sumMMRchanges__team_1 * 2));
                mmr_Changes__Team_2[i] = mmr_Changes__Team_2[i] *
                    ((abs_sumMMRchanges__team_2 + abs_sumMMRchanges__team_1) / (abs_sumMMRchanges__team_2 * 2));
            }
        }

        private void FixMMR_Leavers(double[] mmr_Changes__Team_1, double[] mmr_Changes__Team_2)
        {

        }

        private void SetRatings()
        {
            for (int i = 0; i < 3; i++) {
                this.Players__Team_1[i].Score += this.Score_Changes__Team_1[i];
                this.Players__Team_2[i].Score += this.Score_Changes__Team_2[i];

                this.Players__Team_1[i].Consistency += this.Consistency_Changes__Team_1[i];
                this.Players__Team_2[i].Consistency += this.Consistency_Changes__Team_2[i];
            }

            if ((DateTime.UtcNow - this.DateTime).TotalDays >= Program.MAX_DAYS_FOR_MMR) {
                return;
            }

            this.Team_1.MMR += this.TeamMMR_Change__Team_1;
            this.Team_2.MMR += this.TeamMMR_Change__Team_2;

            for (int i = 0; i < 3; i++) {
                Player pTeam_1 = this.Players__Team_1[i];
                Player pTeam_2 = this.Players__Team_2[i];

                int pTeam_1_commIndex = this.MatchupCombination.GetCommanderIndex(1, i);
                int pTeam_2_commIndex = this.MatchupCombination.GetCommanderIndex(2, i);

                this.Players__Team_1[i].MMR += this.PlayerMMR_Changes__Team_1[i];
                this.Players__Team_2[i].MMR += this.PlayerMMR_Changes__Team_2[i];
                //this.Players__Team_1[i].MMR += this.CommanderMMR_Changes__Team_1[i];
                //this.Players__Team_2[i].MMR += this.CommanderMMR_Changes__Team_2[i];

                pTeam_1.CommanderMMRs[pTeam_1_commIndex].Item3 += this.CommanderMMR_Changes__Team_1[i];
                pTeam_2.CommanderMMRs[pTeam_2_commIndex].Item3 += this.CommanderMMR_Changes__Team_2[i];
                pTeam_1.CommanderMMRs[pTeam_1_commIndex].Item2 ++;
                pTeam_2.CommanderMMRs[pTeam_2_commIndex].Item2 ++;

                pTeam_1.ValidSingleMMRGames++;
                pTeam_2.ValidSingleMMRGames++;

                if (pTeam_1.MMR < 0) {
                    pTeam_1.MMR = 0;
                }
                if (pTeam_2.MMR < 0) {
                    pTeam_2.MMR = 0;
                }
            }

            if (this.Team_1.MMR < 0) {
                this.Team_1.MMR = 0;
            }
            if (this.Team_2.MMR < 0) {
                this.Team_2.MMR = 0;
            }
        }
    }
}