using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace LiveCameraSample
{
    public static class ImageProvider
    {
        public static BitmapImage HappyRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_happy.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }

        public static BitmapImage SadRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_sad.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
        public static BitmapImage AngryRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_angry.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
        public static BitmapImage DisgussedRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_disgust.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
        public static BitmapImage NeutralRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_neutral.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
        public static BitmapImage SuprisedRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_suprised.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
        public static BitmapImage FearRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_fear.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
        public static BitmapImage AgeRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_happy.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }

        public static BitmapImage CatchRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_happy.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
    }
}
