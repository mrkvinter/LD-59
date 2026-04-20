using Code.Game.Scripts.Battle;
using Code.Game.Scripts.Battle.Items;
using Code.UI;
using Game.Utilities;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
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

        public HealthPanel PlayerWinStones;
        public HealthPanel EnemyWinStones;

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
        public ItemHolder EnemyItemHolder;

        [Header("Volumes")] 
        public Volume PillsVolume;

        [Header("VCams")]
        public CinemachineVirtualCameraBase VC_LookAtEnemy;
        public CinemachineVirtualCameraBase VC_LookAtTable;
        public CinemachineVirtualCameraBase VC_LookAtEnemyCard;
        public CinemachineVirtualCameraBase VC_LookAtFlaresCard;
        public CinemachineVirtualCameraBase VC_LookAtInstruction;
        // public CinemachineVirtualCameraBase VC_LookAtEnemyStones;
        public CinemachineVirtualCameraBase VC_LookAtPlayerStones;

        [Header("Start Games")]
        public CanvasGroup BlackScreen;
        public CanvasGroup KvinterGames;
        public CanvasGroup Alarm;
        public CanvasGroup ThanksForPlaying;
        
        [Header("Events")] public bool FastMode;
        public GameObject InputBlocker;
        public StatefulObject HandStatefulObject;
        public Transform PointTable_1;
        public Transform PointTable_2;
        public Transform Table;
        public Light MainLight;
        public DialoguePanel DialoguePanel;

        [Header("First_Person_Event")]
        public AudioSource Person1AudioSource;
        public AudioSource Person2AudioSource;
        public AudioSource Person3AudioSource;

        public GameObject FirstPersonEvent;
        public GameObject SecondPersonEvent;
        public GameObject ThirdPersonEvent;
        
        public AudioListener AudioListener;

        public void SwitchAudio()
        {
            AudioListener.enabled = !AudioListener.enabled;
        }
    }
}