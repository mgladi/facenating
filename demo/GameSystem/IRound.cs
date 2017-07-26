using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSystem;
using System.Windows.Media.Imaging;

namespace LiveCameraSample
{
    public interface IRound
    {
        string GetRoundDescription();
        string GetRoundTarget();
        BitmapImage GetRoundTemplateImage();
        BitmapImage GetRoundIndicator();
        string GetRoundImageText();
        Dictionary<Guid, int> ComputeFrameScorePerPlayer(LiveCameraResult result);
    }
}
