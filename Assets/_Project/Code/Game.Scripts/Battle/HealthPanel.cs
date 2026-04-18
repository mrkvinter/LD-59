using UnityEngine;

namespace Code.Game.Scripts.Battle
{
    public class HealthPanel : MonoBehaviour
    {
        [SerializeField] private GameObject[] healthIcons;
        
        public void SetHealthCount(int healthCount)
        {
            for (int i = 0; i < healthIcons.Length; i++)
            {
                healthIcons[i].SetActive(i < healthCount);
            }
        }
    }
}