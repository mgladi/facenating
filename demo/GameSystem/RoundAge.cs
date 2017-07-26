using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GameSystem;
using Microsoft.ProjectOxford.Common.Contract;

namespace LiveCameraSample
{
    public class RoundAge: IRound
    {
        public RoundAge()
        {
            this.agesSum = new Dictionary<Guid, double>();
            this.agesCount = new Dictionary<Guid, double>();
            this.agesAverage = new Dictionary<Guid, double>();
        }

        private Dictionary<Guid, double> agesSum;
        private Dictionary<Guid, double> agesCount;
        private Dictionary<Guid, double> agesAverage;

        public string GetRoundDescription()
        {
            return "Try to look as old as you can!";
        }
        public string GetRoundTarget()
        {
            return "Oldest";
        }

        public BitmapImage GetRoundTemplateImage()
        {
            return ImageProvider.AgeRound;
        }

        public string GetRoundImageText()
        {
            return "";
        }

        public Dictionary<Guid, int> ComputeFrameScorePerPlayer(LiveCameraResult apiResult)
        {
            var scoresDictionary = new Dictionary<Guid, int>();

            if (apiResult.Identities != null && apiResult.Identities.Count > 0)
            {
                Guid personId;
                double personAverageAge;
                double deltaFromAverage;
                double age;
                foreach (var item in apiResult.Identities)
                {
                    personId = item.Key;

                    if (!agesCount.ContainsKey(personId))
                    {
                        agesCount[personId] = 0;
                        agesSum[personId] = 0.0;
                        agesAverage[personId] = 0.0;
                    }

                    age = apiResult.Identities[personId].FaceAttributes.Age;
                    personAverageAge = this.agesAverage[personId];
                    deltaFromAverage = age - personAverageAge;
                    if (deltaFromAverage > 0 && personAverageAge > 0)
                    {
                        scoresDictionary[item.Key] = 10 * (int)Math.Round(deltaFromAverage / 2);
                    }

                    this.agesCount[personId]++;
                    this.agesSum[personId] += age;
                    this.agesAverage[personId] = this.agesSum[personId]/this.agesCount[personId];
                }
            }

            return scoresDictionary;
        }

        public BitmapImage GetRoundIndicator()
        {
            return ImageProvider.AgeIndicator;
        }
    }
}
