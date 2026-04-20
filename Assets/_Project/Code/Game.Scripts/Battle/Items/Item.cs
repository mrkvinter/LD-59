using System;
using Code.Game.Core;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Code.Game.Scripts.Battle.Items
{
    public abstract class Item
    {
        public ItemView View { get; private set; }
        public ItemDef ItemDef { get; set; }
        public virtual IAffectGame GetAffectGame() => null;
        public string Name => ItemDef.Name;
        public string Description => ItemDef.Description + 
                                     (BlockedForRound ? $"\n <color=red>Can't be used this round</color>" : "");
        public bool IsSelectable { get; set; }
        public bool BlockedForRound { get; set; } = false;
        
        public virtual bool IsBigItem => false;

        public void SetView(ItemView view)
        {
            View = view;
            if (View != null) View.Item = this;
        }
        
        public virtual UniTask OnUse(BattleState battleState, Player player) => UniTask.CompletedTask;

        protected async UniTask MoveToCenter(SceneLinks SceneLinks)
        {
            G.AudioService.PlaySound("use", 0.1f);
            View.transform.parent = IsBigItem ? SceneLinks.CenterBigItemSocket : SceneLinks.CenterSocket;
            View.transform.DOLocalRotateQuaternion(ItemDef.Rotation, 0.25f);
            await View.transform.DOLocalMove(Vector3.zero, 0.25f);
        }

        protected async UniTask MoveDown()
        {
            G.AudioService.PlaySound("use", 0.1f);
            await View.transform.DOLocalMoveY(-1, 0.5f);
        }

        protected void BlockSameItemsWithinRound(BattleState battleState, Player player)
        {
            player.Items.ForEach(e => e.BlockedForRound = e.ItemDef.Id == ItemDef.Id);
        }
    }

    public interface IAffectGame
    {
    }
    
    public interface IAffectWinner : IAffectGame
    {
        public virtual Winner AffectWinner(Winner winner) => winner;
    }
    
    public interface IAffectEnemySign : IAffectGame
    {
        public bool IsSignAvailable(Card card);
    }
}