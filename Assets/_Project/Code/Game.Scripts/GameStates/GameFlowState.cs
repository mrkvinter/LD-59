using Code.Game.Core;
using Code.Game.Scripts.Battle;
using Code.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core;
using UnityEngine;

namespace Code.Game.Scripts.GameStates
{
    public class GameFlowState
    {
        private BattleState battleState;
        private SceneLinks sceneLinks;
        private float mainLightIntensity;

        public GameFlowState()
        {
            battleState = new BattleState();
        }

        public void Initialize()
        {
            battleState.OnEnter();
            
            sceneLinks = G.Resolve<SceneLinks>();
            sceneLinks.RestartButton.onClick
                .AddListener(G.Resolve<IGameDirector>().RestartGame);
            
            mainLightIntensity = sceneLinks.MainLight.intensity;
            sceneLinks.MainLight.intensity = 0;
            sceneLinks.MainLight.gameObject.SetActive(true);
            sceneLinks.DialoguePanel.gameObject.SetActive(false);
            FirstEvent().Forget();
        }

        public async UniTask FirstEvent()
        {
            sceneLinks.FirstPersonEvent.gameObject.SetActive(true);
            sceneLinks.Table.transform.position = sceneLinks.PointTable_1.transform.position;
            if (sceneLinks.FastMode)
            {
                sceneLinks.Table.transform.position = sceneLinks.PointTable_2.transform.position;
                sceneLinks.MainLight.intensity = mainLightIntensity;
                sceneLinks.HandStatefulObject.SetState("Hidden");
            }
            else
            {
                await sceneLinks.Table.transform.DOMove(sceneLinks.PointTable_2.transform.position, 8f)
                    .SetEase(Ease.Linear);
                sceneLinks.MainLight.DOIntensity(mainLightIntensity, 2f);
                await sceneLinks.HandStatefulObject.SetStateAsync("Hidden");
            }

            sceneLinks.DialoguePanel.gameObject.SetActive(true);
            await sceneLinks.DialoguePanel.ShowDialogueAsync(
                Dialogue.Create().Clear().SetSpeaker(null)
                    .Text("О, смотрю у нас новенький вылез.").Delay(Dialogue.LongDelay).Clear()
                    .Text("Если хочешь тут выжить, то придется тебе раздобыть Сигнальный Огни.").Delay(Dialogue.LongDelay).Clear()
                    .Text("Они дают свет и отпугивают разных тварей вокруг").Delay(Dialogue.LongDelay).Clear()
                    .Text("У меня как раз есть несколько, можем сыграть.").Delay(Dialogue.LongDelay).Clear()
                    .Text("Но помни, играем до последнего, а кто поиграл, сдохнет в темноте.").Delay(Dialogue.LongDelay).Clear()
                    .Text("Ха-ха-ха.").Delay(Dialogue.MediumDelay)
                );
        }
    }
}