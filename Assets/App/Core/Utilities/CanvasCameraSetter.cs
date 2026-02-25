using UnityEngine;

namespace Miyo.Core
{
    public class CanvasCameraSetter : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }
    }
}
