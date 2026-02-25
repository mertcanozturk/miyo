using System;
using UnityEngine;

namespace Miyo.Games
{
    [CreateAssetMenu(menuName = "Miyo/Games/Balloon Pop Config")]
    public class BalloonConfig : ScriptableObject
    {
        [Header("Balon Tipleri")]
        public BalloonTypeDefinition[] balloonTypes;

        [Header("Zorluk")]
        public float baseSpawnInterval = 1.2f;
        public float minSpawnInterval = 0.45f;
        public Vector2 spawnCountRange = new(1, 3);
        [Range(0.9f, 1f)]
        public float difficultyFactor = 0.985f;

        [Header("Tempo Bump")]
        public float tempoBumpEverySeconds = 30f;
        public int tempoBumpStreakCount = 15;
        [Range(0f, 1f)]
        public float tempoBumpSpeedMult = 0.8f; // interval bu katsayıyla çarpılır (0.8 = %20 hızlanma)
        public float tempoBumpDuration = 5f;

        [Header("Hedef Değişimi")]
        public int popsPerTarget = 5; // kaç doğru pop'tan sonra hedef tipi değişir

        [Header("Balon Hızı")]
        public float minBalloonSpeed = 1.5f; // başlangıç hızı
        public float maxBalloonSpeed = 4f;   // maksimum hız (zorluk arttıkça yaklaşılır)

        [Header("Karıştırıcı")]
        [Range(0f, 1f)]
        public float minNonTargetRatio = 0.35f;  // oyun başından itibaren hedef-dışı balon oranı (minimum)
        [Range(0f, 1f)]
        public float maxNonTargetRatio = 0.6f;   // zorluk ilerledikçe ulaşılan hedef-dışı balon oranı (maksimum)
    }

    [Serializable]
    public class BalloonTypeDefinition
    {
        public string typeId;        // "circle", "star", "heart" ...
        public string displayName;   // HUD hedef göstergesinde kullanılır ("Daire", "Yıldız")
        public GameObject prefab;    // 3D balon prefabı (hem spawn'da hem hedef göstergesinde kullanılır)
    }
}
