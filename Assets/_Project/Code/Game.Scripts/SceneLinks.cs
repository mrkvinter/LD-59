using Code.Game.Scripts.Battle;
using Code.Game.Scripts.Battle.Items;
using Code.UI;
using Game.Utilities;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
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
        public CardHolder PlayerCardsParent;
        public CardHolder EnemyCardsParent;
        
        public HandView LeftHandView;
        public HandView RightHandView;
        
        public HealthPanel EnemyHealthPanel;
        public HealthPanel PlayerHealthPanel;

        public TMP_Text EnemyStateText;

        public Button RestartButton;
        public ItemDescription ItemDescription;

        [Header("Titles")]
        public GameObject WinTitle;
        public GameObject LoseTitle;
        public GameObject DrawTitle;

        [Header("Items")]
        public Transform CenterSocket;
        public Transform CenterBigItemSocket;
        public ItemHolder ItemHolder;

        [Header("Volumes")] 
        public Volume PillsVolume;

        [Header("VCams")]
        public CinemachineVirtualCameraBase VC_LookAtEnemy;
        public CinemachineVirtualCameraBase VC_LookAtTable;
        public CinemachineVirtualCameraBase VC_LookAtEnemyCard;

        [Header("Events")] public bool FastMode;
        public StatefulObject HandStatefulObject;
        public Transform PointTable_1;
        public Transform PointTable_2;
        public Transform Table;
        public Light MainLight;
        public DialoguePanel DialoguePanel;

        [Header("First_Person_Event")]
        public GameObject FirstPersonEvent;
    }
}