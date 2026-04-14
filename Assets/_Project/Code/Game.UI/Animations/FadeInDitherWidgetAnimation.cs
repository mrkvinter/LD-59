using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Game.UI.Animations
{
    public class FadeInDitherWidgetAnimation : BaseWidgetAnimation
    {
        [SerializeField] private float duration;
        [SerializeField] private Vector2 offset;
        [SerializeField] private Material ditherMaterial;
        [SerializeField] private Material[] preparedMaterials;

        private Vector2 startPosition;
        private Material ditherMaterialInstance;
        private List<Material> fontMaterials;
        private static readonly int DitherIntensity = Shader.PropertyToID("_DitherIntensity");


        protected override void OnInitialize()
        {
            startPosition = Layout.RectTransform.anchoredPosition;
            ditherMaterialInstance = new Material(ditherMaterial);
            fontMaterials = new List<Material>();
            fontMaterials.AddRange(preparedMaterials);
            foreach (var preparedMaterial in preparedMaterials)
            {
                preparedMaterial.SetFloat(DitherIntensity, 1);
            }

            var tmpTexts = GetComponentsInChildren<TMP_Text>();
            foreach (var text in tmpTexts)
            {
                text.fontMaterial = new Material(text.fontMaterial);
                fontMaterials.Add(text.fontMaterial);
            }
            var texts = GetComponentsInChildren<UnityEngine.UI.Text>();
            foreach (var text in texts)
            {
                text.material = new Material(text.material);
                fontMaterials.Add(text.material);
            }

            var images = GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (var image in images)
            {
                image.material = ditherMaterialInstance;
            }
            
            var sprites = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                sprite.material = ditherMaterialInstance;
            }
        }

        protected override void PlayOpen(bool instant, Action callback)
        {
            if (instant)
            {
                ditherMaterialInstance.SetFloat(DitherIntensity, 1);
                SetDitherIntensity(1);
                Layout.RectTransform.anchoredPosition = startPosition + offset;
                callback.Invoke();
                return;
            }

            DOTween.To(() => ditherMaterialInstance.GetFloat(DitherIntensity),
                SetDitherIntensity, 1, duration);
            Layout.RectTransform.DOAnchorPos(startPosition + offset, duration)
                .OnComplete(callback.Invoke);
        }

        protected override void PlayClose(bool instant, Action callback)
        {
            if (instant)
            {
                ditherMaterialInstance.SetFloat(DitherIntensity, 0);
                SetDitherIntensity(0);
                Layout.RectTransform.anchoredPosition = startPosition;
                callback.Invoke();
                return;
            }

            DOTween.To(() => ditherMaterialInstance.GetFloat(DitherIntensity),
                SetDitherIntensity, 0, duration);
            Layout.RectTransform.DOAnchorPos(startPosition, duration)
                .OnComplete(callback.Invoke);
        }

        private void SetDitherIntensity(float value)
        {
            ditherMaterialInstance.SetFloat(DitherIntensity, value);
            foreach (var fontMaterial in fontMaterials)
            {
                fontMaterial.SetFloat(DitherIntensity, value);
            }
        }
    }
}