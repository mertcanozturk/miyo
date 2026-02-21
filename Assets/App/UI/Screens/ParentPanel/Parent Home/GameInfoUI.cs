using Miyo.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Miyo.UI
{
    public class GameInfoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _gameNameText;
        [SerializeField] private UICollection<CategoryUI> _categoryElements;
        [SerializeField] private Image _gameIcon;
        [SerializeField] private Image[] _starsIcons;
        [SerializeField] private string playTimeFormat = "{0}m {1}s";
        [SerializeField] private TMP_Text _playTimeText;
        public void Prepare(GameDefinition game, int playTimeMinutes, int starsCount)
        {
            if (game != null)
            {
                _gameNameText.text = game.GameName;
                _categoryElements.Count = 1;
                _categoryElements[0].SetCategory(game.Category);
                _gameIcon.sprite = game.GameIcon;
            }
            else
            {
                _gameNameText.text = "—";
                _categoryElements.Count = 0;
                _gameIcon.sprite = null;
            }

            for (int i = 0; i < _starsIcons.Length; i++)
                _starsIcons[i].gameObject.SetActive(i < starsCount);

            _playTimeText.text = string.Format(playTimeFormat, playTimeMinutes / 60, playTimeMinutes % 60);
        }
    }
}
