using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Schema;
using GameSystem;
using Microsoft.ProjectOxford.Common.Contract;

namespace LiveCameraSample
{
    public class RoundCatch: IRound
    {
        public RoundCatch(Guid leader, BitmapImage leaderImage)
        {
            this.leader = leader;
            this.leaderImage = leaderImage;
        }

        private Guid leader;
        private BitmapImage leaderImage;
        private const double Delta = 0.3;

        public string GetRoundDescription()
        {
            return "Try to mimic the emotion \n of the leader!";
        }
        public string GetRoundTarget()
        {
            return "Mimic";
        }

        public BitmapImage GetRoundTemplateImage()
        {
            // insert leader image here
            return ImageProvider.CatchRound;
        }

        public string GetRoundImageText()
        {
            return "";
        }

        public Dictionary<Guid, int> ComputeFrameScorePerPlayer(LiveCameraResult apiResult)
        {
            var scoresDictionary = new Dictionary<Guid, int>();

            if (apiResult.Identities != null && apiResult.Identities.Count > 0 && apiResult.Identities.ContainsKey(leader))
            {
                KeyValuePair<string, float> currDominantEmotion;
                Guid personId;
                KeyValuePair<string, float> leaderEmotion;
                int mimicPlayersCount = 0;

                leaderEmotion = RoundEmotion.getDominantEmotion(apiResult.Identities[leader].FaceAttributes.Emotion);

                foreach (var item in apiResult.Identities)
                {
                    personId = item.Key;
                    if (personId == leader)
                    {
                        continue;
                    }

                    currDominantEmotion = RoundEmotion.getDominantEmotion(apiResult.Identities[personId].FaceAttributes.Emotion);
                    double delta = Math.Abs(currDominantEmotion.Value - leaderEmotion.Value);
                    if (currDominantEmotion.Key == leaderEmotion.Key &&
                        delta <= Delta)
                    {
                        scoresDictionary[personId] = 10 * (int)Math.Round(1 + 10 * (Delta - delta), 1);
                        mimicPlayersCount++;
                    }
                    else
                    {
                        scoresDictionary[personId] = 0;
                    }
                }

                int totalPlayersCount = apiResult.Identities.Count;

                // handle leader scoring
                scoresDictionary[leader] = (totalPlayersCount - mimicPlayersCount) * 10;



            }

            return scoresDictionary;
        }

    }
}
