using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_1
{
    public class Game
    {
        public DateTime DateTime { get; set; }
        public int WinnerIndex { get; set; }

        public Team Team_1 { get; set; }
        public Team Team_2 { get; set; }

        public double TeamMMR_Change__Team_1 { get; private set; }
        public double TeamMMR_Change__Team_2 { get; private set; }

        public double[] SingleMMR_Changes__Team_1 { get; private set; } = new double[3];
        public double[] SingleMMR_Changes__Team_2 { get; private set; } = new double[3];

        public double[] Score_Changes__Team_1 { get; private set; } = new double[3];
        public double[] Score_Changes__Team_2 { get; private set; } = new double[3];

        public double[] Consistency_Changes__Team_1 { get; private set; } = new double[3];
        public double[] Consistency_Changes__Team_2 { get; private set; } = new double[3];
        
        public MatchupCombination MatchupCombination { get; set; }

        public Game(DateTime dateTime, int winnerIndex, Team team_1, Team team_2, object[] commanders__team_1, object[] commanders__team_2)
        {
            this.DateTime = dateTime;
            this.WinnerIndex = winnerIndex;

            this.Team_1 = team_1;
            this.Team_2 = team_2;
            
            this.MatchupCombination = new MatchupCombination(commanders__team_1, commanders__team_2);

            this.Team_1.AddGame(this);
            this.Team_2.AddGame(this);
        }

        public void AddToRatings()
        {
            double mcv__team_1 = this.MatchupCombination.GetValueInRespectiveTo(0);
            double mcv__team_2 = this.MatchupCombination.GetValueInRespectiveTo(1);

            double MMR_sum__team_1 = Team_1.Players.Sum(x => x.MMR);
            double MMR_sum__team_2 = Team_2.Players.Sum(x => x.MMR);
            double score_sum__team_1 = Team_1.Players.Sum(x => x.Score);
            double score_sum__team_2 = Team_2.Players.Sum(x => x.Score);

            double teamElo__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (this.Team_2.MMR - this.Team_1.MMR)));
            double teamElo__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (this.Team_1.MMR - this.Team_2.MMR)));

            double playersElo__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (MMR_sum__team_2 - MMR_sum__team_1)));
            double playersElo__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (MMR_sum__team_1 - MMR_sum__team_2)));

            double scoreElo__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((score_sum__team_2 - score_sum__team_1) / 3)));
            double scoreElo__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((score_sum__team_1 - score_sum__team_2) / 3)));

            double[] playersImpacts__team_1 = new double[3];
            double[] playersImpacts__team_2 = new double[3];

            if (this.DateTime.Date == DateTime.Today) {
            }

            for (int i = 0; i < 3; i++) {

                //----------------------------------------------------------

                //team_1__playersELOs[i] = 0.5;
                //team_2__playersELOs[i] = 0.5;

                //----------------------------------------------------------

                double factor_playerToTeamMates__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (((MMR_sum__team_1 - Team_1.Players[i].SingleMMR) / 2) - Team_1.Players[i].SingleMMR)));
                double factor_playerToTeamMates__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (((MMR_sum__team_2 - Team_2.Players[i].SingleMMR) / 2) - Team_2.Players[i].SingleMMR)));

                //alreadyIncluded in "singleElo__team"
                //double factor_playerToEnemy__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((MMR_sum__team_2 / 3) - Team_1.Players[i].SingleMMR)));
                //double factor_playerToEnemy__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((MMR_sum__team_1 / 3) - Team_2.Players[i].SingleMMR)));

                playersImpacts__team_1[i] = (2.0 / 3.0) * factor_playerToTeamMates__team_1 * (0.5 + (0.5 * (1 - Team_1.Players[i].Consistency)));     //(0.50 * factor_playerToTeamMates__team_1) + (0.50 * factor_playerToEnemy__team_1);
                playersImpacts__team_2[i] = (2.0 / 3.0) * factor_playerToTeamMates__team_2 * (0.5 + (0.5 * (1 - Team_2.Players[i].Consistency)));     //(0.50 * factor_playerToTeamMates__team_2) + (0.50 * factor_playerToEnemy__team_2);

                //----------------------------------------------------------
            }

            double teamMMR_Change__team_1 = 0;
            double teamMMR_Change__team_2 = 0;
            double[] singleMMR_Changes__team_1 = new double[3];
            double[] singleMMR_Changes__team_2 = new double[3];
            double[] score_Changes__team_1 = new double[3];
            double[] score_Changes__team_2 = new double[3];
            double[] consistency_Changes__team_1 = new double[3];
            double[] consistency_Changes__team_2 = new double[3];
            
            if (this.WinnerIndex == 0) {
                SetWinnerChanges(mcv__team_1, scoreElo__team_1, teamElo__team_1, playersElo__team_1, playersImpacts__team_1,
                    out teamMMR_Change__team_1, singleMMR_Changes__team_1, score_Changes__team_1, consistency_Changes__team_1);

                SetLooserChanges(mcv__team_2, scoreElo__team_2, teamElo__team_2, playersElo__team_2, playersImpacts__team_2,
                    out teamMMR_Change__team_2, singleMMR_Changes__team_2, score_Changes__team_2, consistency_Changes__team_2);
            } else {
                SetWinnerChanges(mcv__team_2, scoreElo__team_2, teamElo__team_2, playersElo__team_2, playersImpacts__team_2,
                    out teamMMR_Change__team_2, singleMMR_Changes__team_2, score_Changes__team_2, consistency_Changes__team_2);

                SetLooserChanges(mcv__team_1, scoreElo__team_1, teamElo__team_1, playersElo__team_1, playersImpacts__team_1,
                    out teamMMR_Change__team_1, singleMMR_Changes__team_1, score_Changes__team_1, consistency_Changes__team_1);
            }

            this.TeamMMR_Change__Team_1 = teamMMR_Change__team_1;
            this.TeamMMR_Change__Team_2 = teamMMR_Change__team_2;
            this.SingleMMR_Changes__Team_1 = singleMMR_Changes__team_1;
            this.SingleMMR_Changes__Team_2 = singleMMR_Changes__team_2;
            this.Score_Changes__Team_1 = score_Changes__team_1;
            this.Score_Changes__Team_2 = score_Changes__team_2;
            this.Consistency_Changes__Team_1 = consistency_Changes__team_1;
            this.Consistency_Changes__Team_2 = consistency_Changes__team_2;

            this.SetRatings();
        }

        private void SetWinnerChanges(
            in double mcv, in double scoreElo, in double teamElo, in double playersElo, in double[] playersImpacts,
            out double teamMMR_Change, double[] singleMMR_changes, double[] scoreChanges, double[] consistency_Changes)
        {
            teamMMR_Change = (Program.MMR_ADAPTION_MULTIPLIER * mcv * (1.0 - teamElo));

            if (playersElo != 0.5) {
            }

            for (int i = 0; i < 3; i++) {
                scoreChanges[i] = scoreWinLooseValue * (1 + (mcv * (1 - scoreElo) * playersImpacts[i]));
                
                singleMMR_changes[i] = Program.MMR_ADAPTION_MULTIPLIER * mcv * (1 - playersElo) * playersImpacts[i];
                consistency_Changes[i] = consistencyMultiplier * (playersElo - 0.50);
            }
        }
        private void SetLooserChanges(in double mcv, in double scoreElo,
            in double teamElo, in double playersElo, in double[] playersImpacts,
            out double teamMMR_Change, double[] singleMMR_changes, double[] scoreChanges, double[] consistency_Changes)
        {
            teamMMR_Change = -(Program.MMR_ADAPTION_MULTIPLIER * mcv * teamElo);

            for (int i = 0; i < 3; i++) {
                scoreChanges[i] = -(scoreWinLooseValue * (1 + (mcv * scoreElo * playersImpacts[i])));

                singleMMR_changes[i] = -(Program.MMR_ADAPTION_MULTIPLIER * mcv * playersElo * playersImpacts[i]);
                consistency_Changes[i] = consistencyMultiplier * (playersElo - 0.50);
            }
        }

        const double consistencyMultiplier = 0.01;
        const double scoreWinLooseValue = 1;

        private void SetRatings()
        {
            for (int i = 0; i < 3; i++) {
                this.Team_1.Players[i].Score += this.Score_Changes__Team_1[i];
                this.Team_2.Players[i].Score += this.Score_Changes__Team_2[i];

                this.Team_1.Players[i].Consistency += this.Consistency_Changes__Team_1[i];
                this.Team_2.Players[i].Consistency += this.Consistency_Changes__Team_2[i];
            }

            if ((DateTime.UtcNow - this.DateTime).TotalDays >= Program.MAX_DAYS_FOR_MMR) {
                return;
            }

            this.Team_1.MMR += this.TeamMMR_Change__Team_1;
            this.Team_2.MMR += this.TeamMMR_Change__Team_2;

            for (int i = 0; i < 3; i++) {
                Player pTeam_1 = this.Team_1.Players[i];
                Player pTeam_2 = this.Team_2.Players[i];

                if (this.Team_1.Games.Count >= Program.MIN_AMOUNT_GAMES_PER_TEAM) {
                    pTeam_1.TeamMMR = ((pTeam_1.ValidTeamMMRGames * pTeam_1.TeamMMR) + this.Team_1.MMR) / (pTeam_1.ValidTeamMMRGames + 1);
                    pTeam_1.ValidTeamMMRGames++;
                }
                if (this.Team_2.Games.Count >= Program.MIN_AMOUNT_GAMES_PER_TEAM) {
                    pTeam_2.TeamMMR = ((pTeam_2.ValidTeamMMRGames * pTeam_2.TeamMMR) + this.Team_2.MMR) / (pTeam_2.ValidTeamMMRGames + 1);
                    pTeam_2.ValidTeamMMRGames++;
                }

                this.Team_1.Players[i].SingleMMR += this.SingleMMR_Changes__Team_1[i];
                this.Team_2.Players[i].SingleMMR += this.SingleMMR_Changes__Team_2[i];

                pTeam_1.ValidSingleMMRGames++;
                pTeam_2.ValidSingleMMRGames++;

                if (pTeam_1.SingleMMR < 0) {
                    pTeam_1.SingleMMR = 0;
                }
                if (pTeam_2.SingleMMR < 0) {
                    pTeam_2.SingleMMR = 0;
                }

                if (pTeam_1.TeamMMR < 0) {
                    pTeam_1.TeamMMR = 0;
                }
                if (pTeam_2.TeamMMR < 0) {
                    pTeam_2.TeamMMR = 0;
                }
            }


            if (this.Team_1.MMR < 0) {
                this.Team_1.MMR = 0;
            }
            if (this.Team_2.MMR < 0) {
                this.Team_2.MMR = 0;
            }
        }

        //private double[] GetMaximumsOfWinMMR_change_Formula()
        //{
        //    double[] winMMR_changes = GetWinMMR_changes(this.WinnerIndex, this.Team_1.Players, this.Team_2.Players);

        //}

        //private double NewtonMethod(double startValue)
        //{

        //}

        //public static double[] GetWinMMR_changes(int teamIndex, Player[] team_1, Player[] team_2)
        //{
        //    double MMR_sum__team_1 = team_1.Sum(x => x.MMR);
        //    double MMR_sum__team_2 = team_2.Sum(x => x.MMR);

        //    double[] winMMR_changes = new double[3];

        //    for (int i = 0; i < 3; i++) {
        //        double elo_teamToEnemy__team_1;
        //        double elo_teamToEnemy__team_2;

        //        double factor_playerToTeamMates__team_1;
        //        double factor_playerToTeamMates__team_2;

        //        double factor_playerToEnemy__team_1;
        //        double factor_playerToEnemy__team_2;

        //        double value;

        //        if (teamIndex == 0) {
        //            elo_teamToEnemy__team_1 = (1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((MMR_sum__team_2 - MMR_sum__team_1) / 3))));
        //            factor_playerToTeamMates__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (((MMR_sum__team_1 - team_1[i].MMR) / 2) - team_1[i].MMR)));
        //            factor_playerToEnemy__team_1 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((MMR_sum__team_2 / 3) - team_1[i].MMR)));

        //            value = (1 - elo_teamToEnemy__team_1) * (0.50 * factor_playerToTeamMates__team_1) + (0.50 * factor_playerToEnemy__team_1);
        //        } else {
        //            elo_teamToEnemy__team_2 = (1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((MMR_sum__team_1 - MMR_sum__team_2) / 3))));
        //            factor_playerToTeamMates__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * (((MMR_sum__team_2 - team_2[i].MMR) / 2) - team_1[i].MMR)));
        //            factor_playerToEnemy__team_2 = 1.0 / (1 + Math.Pow(10, 2 / Program.MAX_MMR_DIFFERENCE_FOR_IMPACT * ((MMR_sum__team_1 / 3) - team_2[i].MMR)));

        //            value = (1 - elo_teamToEnemy__team_2) * (0.50 * factor_playerToTeamMates__team_2) + (0.50 * factor_playerToEnemy__team_2);
        //        }

        //        winMMR_changes[i] = value;
        //    }

        //    return winMMR_changes;
        //}
    }
}







/*
 

                this.TeamMMR_Change__Team_1 = (Program.MMR_ADAPTION_MULTIPLIER *
                    (1.0 - teamElo__team_1) * mcv__team_1);
                this.TeamMMR_Change__Team_2 = (Program.MMR_ADAPTION_MULTIPLIER *
                    teamElo__team_2 * -mcv__team_1);

                for (int i = 0; i < 3; i++) {
                    this.SingleMMR_Changes__Team_1[i] = (Program.MMR_ADAPTION_MULTIPLIER *
                        team_1__playersImpacts[i] * mcv__team_1);
                    this.SingleMMR_Changes__Team_2[i] = (Program.MMR_ADAPTION_MULTIPLIER *
                        (1 - team_2__playersImpacts[i]) * -mcv__team_1);

                    this.Score_Changes__Team_1[i] = scoreWinLooseValue + ((1 - scoreElo__team_1) * mcv__team_1);
                    this.Score_Changes__Team_2[i] = -scoreWinLooseValue + ((1 - scoreElo__team_2) * -mcv__team_1);
                }
 */