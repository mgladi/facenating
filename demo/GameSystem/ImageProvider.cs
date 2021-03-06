﻿using System;
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

        public static BitmapImage HappyIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/happy_indicator.png");
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

        public static BitmapImage SadIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/sad_indicator.png");
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


        public static BitmapImage AngryIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/angry_indicator.png");
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


        public static BitmapImage DisgustIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/disgust_indicator.png");
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


        public static BitmapImage NeurtalIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/neutral_indicator.png");
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


        public static BitmapImage SuprisedIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/surprise_indicator.png");
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

        public static BitmapImage FearIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/fear_indicator.png");
                return new BitmapImage(new Uri(fullPath));

            }
        }

        public static BitmapImage AgeRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_age.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }

        public static BitmapImage Instructions
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/insructions.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }


        public static BitmapImage GameOver
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/game_over.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }


        public static BitmapImage RoundOver
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_over.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }

        public static BitmapImage AgeIndicator
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/old_indicator.png");
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

        public static BitmapImage WaitingForPlayers
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/start_game.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }

        public static BitmapImage EndRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/round_over.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
    }
}
