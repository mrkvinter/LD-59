using RG.DefinitionSystem.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Game.Scripts.Battle.Items
{
    public static class ItemFactory
    {
        public static Item Create(DefRef<ItemDef> defRef, Transform root) => Create(defRef.Unwrap(), root);

        public static Item Create(ItemDef def, Transform root)
        {
            var item = CreateItemImplement(def);
            item.ItemDef = def;
            var view = Object.Instantiate(def.Prefab, root).GetComponent<ItemView>();
            view.transform.localPosition = Vector3.zero;
            view.transform.localRotation = Quaternion.identity;
            
            
            item.SetView(view);
            return item;
        }

        private static Item CreateItemImplement(ItemDef def)
        {
            if (def.Id == ItemDefType.BrokenGlass) return new BrokenGlasses();
            if (def.Id == ItemDefType.Knife) return new Knife();
            if (def.Id == ItemDefType.Pills) return new Pills();
            if (def.Id == ItemDefType.FortuneCookie) return new FortuneCookie();
            if (def.Id == ItemDefType.Whetstone) return new Whetstone();
            if (def.Id == ItemDefType.SpareSignalFlare) return new SpareSignalFlare();
            
            return null;
        }
        
    }
}