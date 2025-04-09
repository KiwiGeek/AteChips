using System.Collections.Generic;

namespace AteChips.Core.Video;

public interface IVideoCard
{
    IEnumerable<VideoOutputSignal> GetOutputs();
}