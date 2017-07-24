using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Common.Contract;

namespace LiveCameraSample
{
    enum EmotionType
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
    class RoundEmotion: IRound
    {
        public RoundEmotion(EmotionType emotionType)
        {
            this.targetEmotion = emotionType;
        }

        private EmotionType targetEmotion;
        public MainWindow.AppMode[] GetAppModes()
        {
            MainWindow.AppMode[] modes = {MainWindow.AppMode.Emotions};
            return modes;
        }

        public string GetRoundDescription()
        {
            return $"Try to get the highest '{this.targetEmotion}' score you can!";
        }

        public List<int> ComputeFrameScorePerPlayer(LiveCameraResult apiResult)
        {
            List<int> frameScores = new List<int>();
            int currScore;
            KeyValuePair<string, float> currDominantEmotion;
            foreach (EmotionScores userEmotionScores in apiResult.EmotionScores)
            {
                currDominantEmotion = getDominantEmotion(userEmotionScores);
                currScore = currDominantEmotion.Key == this.targetEmotion.ToString() ? 10 : 0;
                frameScores.Add(currScore);
            }

            return frameScores;
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
