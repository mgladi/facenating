using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSystem;
using Microsoft.ProjectOxford.Common.Contract;

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

        public Dictionary<Guid, int> ComputeFrameScorePerPlayer(LiveCameraResult apiResult)
        {
            
            var scoresDictionary = new Dictionary<Guid, int>();

            if (apiResult.Identities != null)
            {
                KeyValuePair<string, float> currDominantEmotion;

                foreach (var item in apiResult.Identities)
                {
                    Guid personId = item.Key;
                    currDominantEmotion = getDominantEmotion(apiResult.Identities[personId].FaceAttributes.Emotion);
                    if (currDominantEmotion.Key == this.targetEmotion.ToString() &&
                        Math.Abs(currDominantEmotion.Value - this.targetScore) <= Delta)
                    {
                        scoresDictionary[personId] = 10;
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
            return 0.4 + (random.Next(7) / 10.0); // random in [0.4, 1.0]
        }

        private EmotionType getRandomEmotion()
        {
            Random random = new Random();
            Array values = Enum.GetValues(typeof(EmotionType));
            return (EmotionType) values.GetValue(random.Next(values.Length));
        }

        private KeyValuePair<string, float> getDominantEmotion(EmotionScores scores)
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

        private float getRelevantEmotionScoreFromScores(EmotionScores scores, EmotionType emotionType)
        {
            switch (emotionType)
            {
                case EmotionType.Anger:
                    return scores.Anger;
                case EmotionType.Contempt:
                    return scores.Contempt;
                case EmotionType.Disgust:
                    return scores.Disgust;
                case EmotionType.Fear:
                    return scores.Fear;
                case EmotionType.Happiness:
                    return scores.Happiness;
                case EmotionType.Neutral:
                    return scores.Neutral;
                case EmotionType.Sadness:
                    return scores.Sadness;
                case EmotionType.Surprise:
                    return scores.Surprise;
            }
            return 0;
        }
    }
}
