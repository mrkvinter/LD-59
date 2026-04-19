using System.Collections.Generic;
using System.Linq;
using Code.Game.Scripts.Battle.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.Game.Scripts.Battle
{
    public class ItemHolder : MonoBehaviour
    {
        [SerializeField] private List<Transform> sockets;

        private ItemView[] occupants;

        private ItemView[] Occupants => occupants ??= new ItemView[sockets.Count];

        public IReadOnlyList<ItemView> OccupantsList => Occupants.Where(e => e != null).ToList();

        public bool TryAdd(ItemView view, out Transform socket)
        {
            for (var i = 0; i < Occupants.Length; i++)
            {
                if (Occupants[i] != null) continue;
                Occupants[i] = view;
                socket = sockets[i];
                return true;
            }

            socket = null;
            return false;
        }

        public void Release(ItemView view)
        {
            for (var i = 0; i < Occupants.Length; i++)
            {
                if (Occupants[i] != view) continue;
                Occupants[i] = null;
                return;
            }
        }

        [Button("Populate")]
        private void Populate()
        {
            sockets.Clear();
            for (var i = 0; i < transform.childCount; i++)
            {
                sockets.Add(transform.GetChild(i));
            }
        }
    }
}