using Code.Game.Core;
using Code.Game.Scripts.Battle.Items;
using RG.DefinitionSystem.Core;
using UnityEngine;

namespace Code.Game.Scripts.Battle
{
    public class ItemsService
    {
        private readonly SceneLinks sceneLinks = G.Resolve<SceneLinks>();

        private ItemView[] sockets;

        public ItemsService()
        {
            sockets = new ItemView[sceneLinks.ItemSockets.Length];
        }
        public Item CreateItem(DefRef<ItemDef> itemDef)
        {
            var freeSocketIndex = GetFreeSocketIndex();
            if (freeSocketIndex == -1)
            {
                Debug.LogError("No free sockets");
                return null;
            }
            
            var freeSocket = sceneLinks.ItemSockets[freeSocketIndex];
            var item = ItemFactory.Create(itemDef, freeSocket);
            sockets[freeSocketIndex] = item.View;
            return item;
        }
        
        private int GetFreeSocketIndex()
        {
            for (var i = 0; i < sockets.Length; i++)
            {
                if (sockets[i] == null) return i;
            }

            return -1;
        }

        public void Release(Item item)
        {
            if (item?.View == null) return;

            item.View.HideDescription();
            for (var i = 0; i < sockets.Length; i++)
            {
                if (sockets[i] != item.View) continue;
                sockets[i] = null;
                break;
            }

            Object.Destroy(item.View.gameObject);
            item.SetView(null);
        }
    }
}