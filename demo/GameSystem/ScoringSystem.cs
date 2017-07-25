using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSystem
{
    public class ScoringSystem
    {
        public Dictionary<Guid, int> TotalScore
        {
            get;
            private set;
        }

        public Dictionary<Guid, int> CurrentRoundScore
        {
            get;
            private set;
        }

        public ScoringSystem()
        {
            TotalScore= new Dictionary<Guid,int>();
            CurrentRoundScore = new Dictionary<Guid, int>();
        }


        public void AddToCurrentRound(Dictionary<Guid, int> playerRoundScore)
        {
            foreach (var item in playerRoundScore)
            {
                if (!CurrentRoundScore.ContainsKey(item.Key))
                {

                    CurrentRoundScore[item.Key] = 0;
                }
                CurrentRoundScore[item.Key] += playerRoundScore[item.Key];
            }
        }

        public void CreateNewRound()
        {
            foreach (var item in CurrentRoundScore)
            {
                if (!TotalScore.ContainsKey(item.Key))
                {

                    TotalScore[item.Key] = 0;
                }
                TotalScore[item.Key] += CurrentRoundScore[item.Key];
            }
            CurrentRoundScore = new Dictionary<Guid, int>();
        }
    }
}
