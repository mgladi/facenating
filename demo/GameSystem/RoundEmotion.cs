using System;
using System.Collections.Generic;
using System.Linq;
using GameSystem;
using Microsoft.ProjectOxford.Common.Contract;
using System.Windows.Media.Imaging;

namespace LiveCameraSample
{
    public enum EmotionType
    {
        Anger,
        Contempt,
        Disgust,
        Fear,
        Happiness,
        Neutral,
        Sadness,
        Surprise,
    }

    public class RoundEmotion: IRound
    {
        public RoundEmotion() : this(null, null){}

        public RoundEmotion(EmotionType emotionType) : this(emotionType, null){}

        public RoundEmotion(EmotionType? emotionType, double? targetScore)
        {
            this.targetEmotion = emotionType ?? getRandomEmotion();
            this.targetScore = targetScore ?? getRandomScore();
        }

        public EmotionType targetEmotion { get; private set; }
        public double targetScore { get; private set; }
        private const double Delta = 0.3;

        public string GetRoundDescription()
        {
            return $"Try to get {this.targetScore} \n'{this.targetEmotion}' score!";
        }
        public string GetRoundTarget()
        {
            return $"{this.targetEmotion}: {this.targetScore}";
        }

        public Dictionary<Guid, int> ComputeFrameScorePerPlayer(LiveCameraResult apiResult)
        {
            
            var scoresDictionary = new Dictionary<Guid, int>();

            if (apiResult.Identities != null && apiResult.Identities.Count > 0)
            {
                KeyValuePair<string, float> currDominantEmotion;
                Guid personId;

                foreach (var item in apiResult.Identities)
                {
                    personId = item.Key;
                    currDominantEmotion = getDominantEmotion(apiResult.Identities[personId].FaceAttributes.Emotion);
                    double delta = Math.Abs(currDominantEmotion.Value - this.targetScore);
                    if (currDominantEmotion.Key == this.targetEmotion.ToString() &&
                        delta <= Delta)
                    {
                        scoresDictionary[personId] = 10 * (int)Math.Round(1 + 10*(Delta - delta), 1);
                    }
                    else
                    {
                        scoresDictionary[personId] = 0;
                    }
                }
            }

            return scoresDictionary;
        }

        private double getRandomScore()
        {
            Random random = new Random();
            return 0.5 + (random.Next(5) / 10.0); // random in [0.5, 0.9]
        }

        private EmotionType getRandomEmotion()
        {
            Random random = new Random();
            Array values = Enum.GetValues(typeof(EmotionType));
            return (EmotionType) values.GetValue(random.Next(values.Length));
        }

        public static KeyValuePair<string, float> getDominantEmotion(EmotionScores scores)
        {
            var scoreList = scores.ToRankedList();
            KeyValuePair<string, float> maxPair = scoreList.First();
            foreach (var score in scoreList)
            {
                if (score.Value > maxPair.Value)
                {
                    maxPair = score;
                }
            }
            return maxPair;
        }

        public BitmapImage GetRoundTemplateImage()
        {
            switch (this.targetEmotion)
            {
                case EmotionType.Anger:
                    return ImageProvider.AngryRound;
                case EmotionType.Contempt:
                    return ImageProvider.ContemptRound;
                case EmotionType.Disgust:
                    return ImageProvider.DisgussedRound;
                case EmotionType.Fear:
                    return ImageProvider.FearRound;
                case EmotionType.Happiness:
                    return ImageProvider.HappyRound;
                case EmotionType.Neutral:
                    return ImageProvider.NeutralRound;
                case EmotionType.Sadness:
                    return ImageProvider.SadRound;
                case EmotionType.Surprise:
                    return ImageProvider.SuprisedRound;
                default:
                    break;
            }
            return null;
        }

        public string GetRoundImageText()
        {
            return this.targetScore.ToString();
        }
    }
}
