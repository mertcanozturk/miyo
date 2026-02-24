using System;
using Miyo.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Miyo.UI
{
    public class GameAppView : MonoBehaviour
    {
        public Button PlayButton;
        public TMP_Text GameNameText;
        public Image GameIcon;
        
        public void SetGame(GameDefinition game, Action<GameDefinition> PlayButtonClicked)
        {
            GameNameText.text = game.GameName;
            GameIcon.sprite = game.GameIcon;
            PlayButton.onClick.RemoveAllListeners();
            PlayButton.onClick.AddListener(() => PlayButtonClicked(game));
        }
    }
}
