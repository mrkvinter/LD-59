using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Game.Scripts.Battle
{
    public class HealthPanel : MonoBehaviour
    {
        [SerializeField] private List<GameObject> healthIcons;
        
        public void SetHealthCount(int healthCount)
        {
            for (int i = 0; i < healthIcons.Count; i++)
            {
                healthIcons[i].SetActive(i < healthCount);
            }
        }

        [Button]
        private void Populate()
        {
            healthIcons.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                healthIcons.Add(transform.GetChild(i).gameObject);
            }
        }
    }
}