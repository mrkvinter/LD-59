using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace Code.Game.Scripts.Battle.Items
{
    public class ItemDescription : MonoBehaviour
    {
        public TMP_Text Title;
        public TMP_Text Description;

        public void Show(string title, string description)
        {
            Title.text = title;
            Description.text = description;
            
            gameObject.SetActive(true);
            
            Title.gameObject.SetActive(!title.IsNullOrWhitespace());
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}