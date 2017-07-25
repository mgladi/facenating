using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Vision;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
using VideoFrameAnalyzer;
//using LiveCameraSample;
//using LiveCameraSample.Properties;

namespace GameSystem
{
    //public enum RoundType { Age, Emotion, Catch}
    public class Round
    {
        public TimeSpan RoundTimeSpan = new TimeSpan(0, 0, 30);
        private IRoundType roundType;

        public Round(int roundNum)
        {
            roundType = new EmotionRound();
            RoundEndTime = DateTime.Now + RoundTimeSpan;
            Console.WriteLine("Begin Round " + roundNum);
        }

        public DateTime RoundEndTime
        {
            get;
            private set;
        }

        public void AddResults(LiveCameraResult result)
        {

        }
    }
}
