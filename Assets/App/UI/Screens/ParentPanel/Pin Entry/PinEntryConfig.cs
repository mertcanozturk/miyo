using UnityEngine;

namespace Miyo.UI.Screens
{
    /// <summary>
    /// PIN girişi ekranının davranışını kontrol eden yapılandırma.
    /// Assets/App/Configurations/ altında bir asset örneği oluşturun:
    ///   Right-click → Create → Miyo → Pin Entry Config
    /// </summary>
    [CreateAssetMenu(menuName = "Miyo/Pin Entry Config", fileName = "PinEntryConfig")]
    public class PinEntryConfig : ScriptableObject
    {
        [field: SerializeField, Min(1)]
        [field: Tooltip("PIN kaç haneli olacak (varsayılan: 6)")]
        public int PinLength { get; private set; } = 6;

        [field: SerializeField, Min(1)]
        [field: Tooltip("Kaç yanlış denemeden sonra bekleme süresi başlar")]
        public int MaxWrongAttempts { get; private set; } = 3;

        [field: SerializeField]
        [field: Tooltip(
            "Ardışık kilitlenme turlarında uygulanacak bekleme süreleri (saniye). " +
            "Son eleman tekrar kullanılır. Örnek: [30, 120, 300]")]
        public int[] LockoutDurationsSeconds { get; private set; } = { 30, 120, 300 };

        /// <summary>
        /// Belirtilen kilitlenme turuna karşılık gelen bekleme süresini döner.
        /// round ≥ dizi uzunluğu ise son eleman kullanılır.
        /// </summary>
        public int GetLockoutDuration(int round)
        {
            int index = Mathf.Clamp(round, 0, LockoutDurationsSeconds.Length - 1);
            return LockoutDurationsSeconds[index];
        }
    }
}
