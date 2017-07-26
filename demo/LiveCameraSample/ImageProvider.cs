using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LiveCameraSample
{
    public static class ImageProvider
    {
        public static BitmapImage HappyRound { get
            {
                string fullPath = Path.GetFullPath("Data/round_happy.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }

        public static BitmapImage SadRound
        {
            get
            {
                string fullPath = Path.GetFullPath("Data/sad_round_template.png");
                return new BitmapImage(new Uri(fullPath));
            }
        }
    }
}
