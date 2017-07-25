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
        public RoundEmotion(EmotionType emotionType, double targetScore)
        {
            this.targetEmotion = emotionType;
            this.targetScore = targetScore;
        }

        private EmotionType targetEmotion;
        private double targetScore;

        public string GetRoundDescription()
        {
            return $"Try to get the highest '{this.targetEmotion}' score you can!";
        }

        public Dictionary<Guid, int> ComputeFrameScorePerPlayer(LiveCameraResult apiResult)
        {
            List<int> frameScores = new List<int>();
            int currScore;
            KeyValuePair<string, float> currDominantEmotion;
            foreach (EmotionScores userEmotionScores in apiResult.EmotionScores)
            {
                currScore = 0;
                currDominantEmotion = getDominantEmotion(userEmotionScores);
                if (currDominantEmotion.Key == this.targetEmotion.ToString() &&
                    Math.Abs(currDominantEmotion.Value - this.targetScore) <= 0.1)
                {
                   currScore = 10;
                }
                frameScores.Add(currScore);
            }

            return new Dictionary<Guid, int>(); //TODO
        }

        private KeyValuePair<string, float> getDominantEmotion(EmotionScores scores)
        {
            return scores.ToRankedList().Max();
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
