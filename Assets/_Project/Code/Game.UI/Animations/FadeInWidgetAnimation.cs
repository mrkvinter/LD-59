using System;
using DG.Tweening;
using Game.UI.Base;
using UnityEngine;

namespace Game.UI.Animations
{
    public class FadeInWidgetAnimation : BaseWidgetAnimation
    {
        [SerializeField] private float duration;
        [SerializeField] private Vector2 offset;

        protected override void PlayOpen(bool instant, Action callback)
        {
            if (instant)
            {
                Layout.CanvasGroup.alpha = 1;
                Layout.RectTransform.anchoredPosition += offset;
                callback.Invoke();
                return;
            }

            Layout.CanvasGroup.DOFade(1, duration);
            Layout.RectTransform.DOAnchorPos(Layout.RectTransform.anchoredPosition + offset, duration)
                .OnComplete(callback.Invoke);
        }

        protected override void PlayClose(bool instant, Action callback)
        {
            if (instant)
            {
                Layout.CanvasGroup.alpha = 0;
                Layout.RectTransform.anchoredPosition -= offset;
                callback.Invoke();
                return;
            }

            Layout.CanvasGroup.DOFade(0, duration);
            Layout.RectTransform.DOAnchorPos(Layout.RectTransform.anchoredPosition - offset, duration)
                .OnComplete(callback.Invoke);
        }
    }
}