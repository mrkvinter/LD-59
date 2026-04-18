using Code.Game.Scripts.Battle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.Scripts
{
    public class SceneLinks : MonoBehaviour
    {
        public Canvas CanvasMain;
        public Canvas CanvasGame;

        public GameObject GameUI;
        public GameObject Hands;

        public CardView CardPrefab;
        public RectTransform CardsParent;
        
        public HandView LeftHandView;
        public HandView RightHandView;
        
        public HealthPanel EnemyHealthPanel;
        public HealthPanel PlayerHealthPanel;

        public TMP_Text EnemyStateText;

        public Button RestartButton;

        [Header("Titles")]
        public GameObject WinTitle;
        public GameObject LoseTitle;
        public GameObject DrawTitle;
    }
}