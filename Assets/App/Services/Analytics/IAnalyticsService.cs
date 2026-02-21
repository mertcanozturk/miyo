using Miyo.Data;

namespace Miyo.Services.Analytics
{
    public interface IAnalyticsService
    {
        /// <summary>
        /// Belirtilen çocuk için dummy istatistik verisi döner.
        /// childId seed olarak kullanılır, böylece her çocuk için farklı veri üretilir.
        /// </summary>
        GameStatistic[] GetDummyGameStatistics(string childId, GameDefinition[] availableGames = null);
    }
}
