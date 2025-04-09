using AteChips.Shared.Video;
using System.Collections.Generic;

namespace AteChips.Core.Shared.Interfaces;

public interface IVideoCard
{
    IEnumerable<VideoOutputSignal> GetOutputs();
}