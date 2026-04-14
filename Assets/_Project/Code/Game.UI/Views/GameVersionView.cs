using System;
using Game.Utilities;
using TMPro;
using UnityEngine;

namespace Game.UI.Views
{
    public class GameVersionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text versionText;

        private void Awake()
        {
            versionText.text = GameVersion.Full;
        }
    }
}