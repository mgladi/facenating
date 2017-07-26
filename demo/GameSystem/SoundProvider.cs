using System;
using System.IO;
using System.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiveCameraSample
{
    public static class SoundProvider
    {
        public static MediaPlayer PrepareYourself
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/PrepareYourself.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer Countdown
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/321.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer Round1
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/Round1.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer Round2
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/Round2.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer Round3
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/Round3.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer Round4
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/Round4.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer FinalRound
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/FinalRound.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer TheWinner
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/TheWinner.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer KillHim
        {
            get
            {
                MediaPlayer sound = new MediaPlayer();
                string fullPath = Path.GetFullPath("Vocals/KillHim.mp3");
                sound.Open(new Uri(fullPath));
                return sound;
            }
        }

        public static MediaPlayer Round(int roundNumber)
        {
            switch (roundNumber)
            {
                case 1:
                    return SoundProvider.Round1;
                case 2:
                    return SoundProvider.Round2;
                case 3:
                    return SoundProvider.Round3;
                case 4:
                    return SoundProvider.Round4;
                case 5:
                    return SoundProvider.FinalRound;
            }
            return null;
        }
    }
}
