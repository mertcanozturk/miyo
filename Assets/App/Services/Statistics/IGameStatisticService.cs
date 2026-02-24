using Cysharp.Threading.Tasks;
using Miyo.Data;

namespace Miyo.Services.Statistics
{
    public interface IGameStatisticService
    {
        UniTask<GameStatistic[]> GetStatisticsAsync();
        UniTask<GameStatistic[]> GetStatisticsForChildAsync(string childId);
    }
}
