using Code.Game.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.Game.Scripts
{
    public class Instruction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Transform targetSocket;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private float hoverScale = 1.1f;

        private Transform baseParent;
        private Vector3 basePosition;
        private Quaternion baseRotation;
        private Vector3 baseScale;

        private bool isOpened;
        private bool isAnimating;

        private void Awake()
        {
            baseParent = transform.parent;
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
            baseScale = transform.localScale;
        }

        private Transform ResolveTargetSocket()
        {
            if (targetSocket != null) return targetSocket;
            targetSocket = G.Resolve<SceneLinks>().CenterBigItemSocket;
            return targetSocket;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isAnimating || isOpened) return;
            transform.localScale = baseScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isOpened) return;
            transform.localScale = baseScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isAnimating) return;

            if (isOpened) Return().Forget();
            else Open().Forget();
        }

        private async UniTask Open()
        {
            isAnimating = true;
            transform.localScale = baseScale;
            transform.SetParent(ResolveTargetSocket(), true);

            await DOTween.Sequence()
                .Join(transform.DOLocalMove(Vector3.zero, moveDuration))
                .Join(transform.DOLocalRotateQuaternion(Quaternion.identity, moveDuration))
                .SetEase(Ease.OutCubic)
                .ToUniTask();

            isOpened = true;
            isAnimating = false;
        }

        private async UniTask Return()
        {
            isAnimating = true;
            transform.SetParent(baseParent, true);

            await DOTween.Sequence()
                .Join(transform.DOLocalMove(basePosition, moveDuration))
                .Join(transform.DOLocalRotateQuaternion(baseRotation, moveDuration))
                .SetEase(Ease.OutCubic)
                .ToUniTask();

            isOpened = false;
            isAnimating = false;
        }
    }
}