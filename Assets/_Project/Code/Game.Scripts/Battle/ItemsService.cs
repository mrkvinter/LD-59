using Code.Game.Core;
using Code.Game.Scripts.Battle.Items;
using RG.DefinitionSystem.Core;
using UnityEngine;

namespace Code.Game.Scripts.Battle
{
    public class ItemsService
    {
        private readonly SceneLinks sceneLinks = G.Resolve<SceneLinks>();

        public Item CreateItem(DefRef<ItemDef> itemDef)
        {
            var item = ItemFactory.Create(itemDef, sceneLinks.ItemHolder.transform);
            if (!sceneLinks.ItemHolder.TryAdd(item.View, out var socket))
            {
                Debug.LogError("No free sockets");
                Object.Destroy(item.View.gameObject);
                return null;
            }

            item.View.transform.SetParent(socket, false);
            return item;
        }

        public void Release(Item item)
        {
            if (item?.View == null) return;

            item.View.HideDescription();
            sceneLinks.ItemHolder.Release(item.View);

            Object.Destroy(item.View.gameObject);
            item.SetView(null);
        }
    }
}