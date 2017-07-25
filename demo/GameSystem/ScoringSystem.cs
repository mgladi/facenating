using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSystem
{
    public class ScoringSystem
    {
        public int[] PlayersScore
        {
            get;
            private set;
        }

        public ScoringSystem(int numOfPlayers)
        {
            if(numOfPlayers < 1 || numOfPlayers > 4)
            {
                throw new Exception("Bad number of Players");
            }

            PlayersScore = new int[numOfPlayers];

            foreach(var player in PlayersScore)
            {
                PlayersScore[player] = 0;
            }
        }

        public void AddRoundScore(int[] playerRoundScore)
        {
            if(playerRoundScore == null || playerRoundScore.Length != PlayersScore.Length)
            {
                throw new Exception("Bad score input");
            }

            foreach(var player in playerRoundScore)
            {
                PlayersScore[player] += playerRoundScore[player];
            }
        }
    }
}
