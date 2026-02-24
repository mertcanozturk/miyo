using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.Games
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MatchPuzzleCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text _letterText;
        [SerializeField] private Image _spriteDisplay; // opsiyonel — sadece image kartı prefab'ında bağlanır
        [SerializeField] private Button _button;
        [SerializeField] private Image _cardBack;
        [SerializeField] private Image _cardFront;

        private CanvasGroup _canvasGroup;
        private string _matchKey;
        private bool _isOpen;
        private bool _isMatched;
        private int _index;

        public string MatchKey => _matchKey;
        public string Letter => _matchKey; // geriye dönük uyumluluk alias'ı
        public bool IsOpen => _isOpen;
        public bool IsMatched => _isMatched;
        public int Index => _index;
        public Button Button => _button;
        public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();
        public RectTransform RectTransform => (RectTransform)transform;
        public Image CardBack => _cardBack;
        public Image CardFront => _cardFront;

        public void Initialize(int index, MatchPuzzleCardContent content)
        {
            _index = index;
            _matchKey = content.MatchKey;
            _isOpen = false;
            _isMatched = false;

            if (content.Kind == MatchPuzzleCardContent.ContentKind.Text)
            {
                if (_letterText != null)
                {
                    _letterText.text = content.Text;
                    _letterText.alpha = 0f;
                    _letterText.gameObject.SetActive(true);
                }
                if (_spriteDisplay != null) _spriteDisplay.gameObject.SetActive(false);
            }
            else
            {
                if (_letterText != null)
                {
                    _letterText.text = string.Empty;
                    _letterText.alpha = 0f;
                    _letterText.gameObject.SetActive(false);
                }
                if (_spriteDisplay != null)
                {
                    _spriteDisplay.sprite = content.SpriteRef;
                    _spriteDisplay.gameObject.SetActive(false);
                }
            }

            if (_cardBack) _cardBack.gameObject.SetActive(true);
            if (_cardFront) _cardFront.gameObject.SetActive(false);
            CanvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
        }

        /// <summary>Geriye dönük uyumluluk: metin kartları için eski çağrı biçimi.</summary>
        public void Initialize(int index, string letter)
            => Initialize(index, MatchPuzzleCardContent.FromText(letter, letter));

        public void SetOpen(bool open)
        {
            _isOpen = open;
            if (_letterText != null) _letterText.alpha = open ? 1f : 0f;
            if (_spriteDisplay != null) _spriteDisplay.gameObject.SetActive(open);
            if (_cardBack) _cardBack.gameObject.SetActive(!open);
            if (_cardFront) _cardFront.gameObject.SetActive(open);
        }

        public void SetMatched()
        {
            _isMatched = true;
            _isOpen = true;
            _button.interactable = false;
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable && !_isMatched;
        }
    }
}
