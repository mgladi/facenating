using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameSystem;

namespace LiveCameraSample
{
    public interface IRound
    {
        string GetRoundDescription();
        List<int> ComputeFrameScorePerPlayer(LiveCameraResult result);
    }
}
