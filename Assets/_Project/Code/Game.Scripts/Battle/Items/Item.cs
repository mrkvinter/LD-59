using System;
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
        public string Description => ItemDef.Description;
        public bool IsSelectable { get; set; }

        public void SetView(ItemView view)
        {
            View = view;
            if (View != null) View.Item = this;
        }
        
        public virtual UniTask OnUse(BattleState battleState) => UniTask.CompletedTask;

        protected async UniTask MoveToCenter(SceneLinks SceneLinks)
        {
            View.transform.parent = SceneLinks.CenterSocket;
            View.transform.DOLocalRotate(Vector3.zero, 0.25f);
            await View.transform.DOLocalMove(Vector3.zero, 0.25f);
        }

        protected async UniTask MoveDown()
        {
            await View.transform.DOLocalMoveY(-1, 0.5f);
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