using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveCameraSample
{
    interface IRound
    {
        MainWindow.AppMode[] GetAppModes();
        string GetRoundDescription();
        List<int> ComputeFrameScorePerPlayer(LiveCameraResult result);
    }
}
