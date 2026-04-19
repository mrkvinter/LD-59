using System.Collections.Generic;
using UnityEngine;

namespace Code.Game.Scripts.Battle
{
    public class CardHolder : MonoBehaviour
    {
        [SerializeField] private float width = 3f;
        [SerializeField] private float maxAngleY = 15f;

        private readonly List<CardView> cards = new();

        public void Add(CardView card)
        {
            card.transform.SetParent(transform, false);
            cards.Add(card);
            Arrange();
        }

        public void Remove(CardView card)
        {
            if (cards.Remove(card))
            {
                Arrange();
            }
        }

        private void Arrange()
        {
            var count = cards.Count;
            if (count == 0) return;

            for (int i = 0; i < count; i++)
            {
                var t = count == 1 ? 0.5f : (float)i / (count - 1);
                var x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);
                var angle = Mathf.Lerp(-maxAngleY, maxAngleY, t);

                var card = cards[i];
                card.transform.localPosition = new Vector3(x, 0f, 0f);
                card.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            }
        }
    }
}