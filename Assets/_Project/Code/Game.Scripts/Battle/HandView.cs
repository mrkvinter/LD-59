using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RG.DefinitionSystem.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.Scripts.Battle
{
    public class HandView : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private Image handImage;
        [SerializeField] private Sprite[] signs;
        
        public async UniTask PlayShakeAnimation()
        {
            await DOTween.Sequence()
                .Append(root.DOLocalRotate(new Vector3(0, 0, 15), 0.2f))
                .Append(root.DOLocalRotate(new Vector3(0, 0, -20), 0.2f))
                .SetLoops(3)
                .ToUniTask();
            
            await root.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
        }
        
        public void SetSign(Sign sign)
        {
            handImage.gameObject.SetActive(true);
            var signDef = DefManager.GetDefMap<SignDef>().DefinitionsEntries
                .FirstOrDefault(e => e.Sign == sign);
            if (signDef == null) 
            {
                handImage.gameObject.SetActive(false);
                return;
            }
            handImage.sprite = signDef.Sprite;
        }
        
        public void SetVisible(bool visible)
        {
            root.gameObject.SetActive(visible);
        }
    }
}