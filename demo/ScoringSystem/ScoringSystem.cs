using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoringSystem
{
    public class ScoringSystem
    {
        public int[] PlayerScores
        {
            get;
            private set;
        }

        public ScoringSystem(int numOfPlayers)
        {
            if(numOfPlayers < 1 || numOfPlayers > 4)
            {
                throw new Exception("Bad number of player input");
            }
            PlayerScores = new int[numOfPlayers];
            foreach(int player in PlayerScores)
            {
                PlayerScores[player] = 0;
            }
        }

        public void AddRoundScore(int[] PlayerRoundScores)
        {
            if(PlayerRoundScores == null || PlayerRoundScores.Length != PlayerScores.Length)
            {
                throw new Exception("Bad player round score inpute");
            }

            foreach(int player in PlayerScores)
            {
                PlayerScores[player] += PlayerRoundScores[player];
            }
        }


    }
}
