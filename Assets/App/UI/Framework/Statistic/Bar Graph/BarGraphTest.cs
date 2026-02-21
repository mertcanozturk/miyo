using System.Collections.Generic;
using UnityEngine;

namespace Miyo.UI
{
    public class BarGraphTest : MonoBehaviour
    {
        [SerializeField] private BarGraph _barGraph;

        private void Start()
        {
            if (_barGraph == null)
                _barGraph = GetComponent<BarGraph>();

            if (_barGraph != null)
                SetWeeklyTestData();
        }

        /// <summary>
        /// 1 haftalık örnek veri: Pzt, Salı, Çarş, Perş, Cum, Cmt, Pazar + süre değerleri (dakika).
        /// Value = dakika; fillAmount için kullanılır. ValueDisplay = "Xs Ydk" formatında gösterim.
        /// </summary>
        private void SetWeeklyTestData()
        {
            var entries = new List<BarGraphEntry>
            {
                new BarGraphEntry { Value = 170f,  ValueDisplay = "2s 50dk",  Label = "Pzt" },   // 2 saat 50 dk = 170 dk
                new BarGraphEntry { Value = 120f,  ValueDisplay = "2s 0dk",   Label = "Salı" },
                new BarGraphEntry { Value = 90f,   ValueDisplay = "1s 30dk",   Label = "Çarş" },
                new BarGraphEntry { Value = 200f, ValueDisplay = "3s 20dk",  Label = "Perş" },
                new BarGraphEntry { Value = 45f,   ValueDisplay = "0s 45dk",   Label = "Cum" },
                new BarGraphEntry { Value = 180f,  ValueDisplay = "3s 0dk",   Label = "Cmt" },
                new BarGraphEntry { Value = 60f,   ValueDisplay = "1s 0dk",   Label = "Paz" }
            };

            // maxValue = 0 → veriden otomatik hesaplanır (en büyük = 200)
            _barGraph.SetData(entries);
        }
    }
}
