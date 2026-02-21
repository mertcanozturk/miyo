using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
namespace Miyo.UI
{
    public class BarGraphValue : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private Image _fillImage;
        private EventTrigger _eventTrigger;

        void Awake(){
            _valueText.gameObject.SetActive(false);
            SetupEventTrigger();
        }

        private void SetupEventTrigger()
        {
            if (_button == null) return;

            _eventTrigger = _button.gameObject.GetComponent<EventTrigger>();
            if (_eventTrigger == null)
            {
                _eventTrigger = _button.gameObject.AddComponent<EventTrigger>();
            }

            // OnPointerDown event'i ekle
            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
            _eventTrigger.triggers.Add(pointerDownEntry);

            // OnPointerUp event'i ekle
            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
            _eventTrigger.triggers.Add(pointerUpEntry);

            // OnPointerExit event'i ekle (butonun dışına çıkıldığında da gizle)
            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
            pointerExitEntry.eventID = EventTriggerType.PointerExit;
            pointerExitEntry.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
            _eventTrigger.triggers.Add(pointerExitEntry);
        }

        public void SetValue(string value)
        {
            if (_valueText != null)
                _valueText.text = value;
        }

        public void SetFillAmount(float amount)
        {
            if (_fillImage != null)
            {
                _fillImage.fillAmount = amount;
                // Eğer text görünürse pozisyonu güncelle
                if (_valueText != null && _valueText.gameObject.activeSelf)
                {
                    UpdateTextPosition();
                }
            }
        }

        private void OnPointerDown(PointerEventData eventData)
        {
            if (_valueText != null)
            {
                UpdateTextPosition();
                _valueText.gameObject.SetActive(true);
            }
        }

        private void UpdateTextPosition()
        {
            if (_fillImage == null || _valueText == null) return;

            RectTransform fillRect = _fillImage.rectTransform;
            RectTransform textRect = _valueText.rectTransform;

            // fillImage'in fillAmount'una göre pozisyonu hesapla
            float fillAmount = _fillImage.fillAmount;
            
            // fillImage'in gerçek rect'ini al
            Rect fillRectBounds = fillRect.rect;
            
            // fillImage'in fillMethod'una göre dolma noktasını hesapla (local space'de)
            Vector3 fillPointLocal = Vector3.zero;
            
            if (_fillImage.fillMethod == Image.FillMethod.Vertical)
            {
                // Vertical fill için
                if (_fillImage.fillOrigin == 0) // Bottom
                {
                    // Alt taraftan başlayarak yukarı doğru doldur
                    fillPointLocal = new Vector3(
                        fillRectBounds.center.x,
                        fillRectBounds.yMin + (fillRectBounds.height * fillAmount),
                        0f
                    );
                }
                else // Top
                {
                    // Üst taraftan başlayarak aşağı doğru doldur
                    fillPointLocal = new Vector3(
                        fillRectBounds.center.x,
                        fillRectBounds.yMax - (fillRectBounds.height * fillAmount),
                        0f
                    );
                }
            }
            else if (_fillImage.fillMethod == Image.FillMethod.Horizontal)
            {
                // Horizontal fill için
                if (_fillImage.fillOrigin == 0) // Left
                {
                    // Sol taraftan başlayarak sağa doğru doldur
                    fillPointLocal = new Vector3(
                        fillRectBounds.xMin + (fillRectBounds.width * fillAmount),
                        fillRectBounds.center.y,
                        0f
                    );
                }
                else // Right
                {
                    // Sağ taraftan başlayarak sola doğru doldur
                    fillPointLocal = new Vector3(
                        fillRectBounds.xMax - (fillRectBounds.width * fillAmount),
                        fillRectBounds.center.y,
                        0f
                    );
                }
            }
            
            // Local pozisyonu world pozisyona çevir
            Vector3 fillPointWorld = fillRect.TransformPoint(fillPointLocal);
            
            // World pozisyonu text'in parent'ının local pozisyonuna çevir
            Vector3 textPointLocal = textRect.parent != null 
                ? textRect.parent.InverseTransformPoint(fillPointWorld)
                : fillPointWorld;
            
            textRect.localPosition = textPointLocal;
        }

        private void OnPointerUp(PointerEventData eventData)
        {
            if (_valueText != null)
                _valueText.gameObject.SetActive(false);
        }

        private void OnPointerExit(PointerEventData eventData)
        {
            if (_valueText != null)
                _valueText.gameObject.SetActive(false);
        }
    }
}
