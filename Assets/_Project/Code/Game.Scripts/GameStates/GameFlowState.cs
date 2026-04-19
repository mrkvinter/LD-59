using System;
using System.Collections.Generic;
using System.Linq;
using Code.Game.Core;
using Code.Game.Scripts.Battle;
using Code.Game.Scripts.Battle.Items;
using Code.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core;
using RG.DefinitionSystem.Core;
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

        private List<DefRef<SignDef>> GetTutorialPlayerDeck()
        {
            var baseDeck = new List<DefRef<SignDef>>();
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Rock, 3));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Papper, 0));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Scissors, 1));

            return baseDeck.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private List<DefRef<SignDef>> GetTutorialEnemyDeck()
        {
            var baseDeck = new List<DefRef<SignDef>>();
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Rock, 0));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Papper, 1));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Scissors, 3));

            return baseDeck.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private List<DefRef<SignDef>> GetBaseDeck()
        {
            var baseDeck = new List<DefRef<SignDef>>();
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Rock, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Papper, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Scissors, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Goat, 1));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.F, 1));

            return baseDeck.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private List<DefRef<SignDef>> GetSecondDeck()
        {
            var baseDeck = new List<DefRef<SignDef>>();
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Rock, 1));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Papper, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Scissors, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.Goat, 2));
            baseDeck.AddRange(Enumerable.Repeat(SignDefType.F, 3));

            return baseDeck.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private List<DefRef<ItemDef>> baseItems => new()
        {
            ItemDefType.Knife,
            ItemDefType.Pills,
            ItemDefType.Pills,
            ItemDefType.SpareSignalFlare,
            ItemDefType.Whetstone,
            ItemDefType.FortuneCookie,
        };

        public void Initialize()
        {
            sceneLinks = G.Resolve<SceneLinks>();
            sceneLinks.RestartButton.onClick
                .AddListener(G.Resolve<IGameDirector>().RestartGame);

            mainLightIntensity = sceneLinks.MainLight.intensity;
            sceneLinks.MainLight.intensity = 0;
            sceneLinks.MainLight.gameObject.SetActive(true);
            sceneLinks.DialoguePanel.gameObject.SetActive(false);


            G.AudioService.PlayLoop("rumble_loop");
            FirstEvent().Forget();
        }

        public async UniTask GameOverAsync()
        {
        }

        public async UniTask FirstEvent()
        {
            var dPrinter = new DialoguePrinter(sceneLinks.DialoguePanel);

            battleState.OnGameEnd += OnBattleEnd;

            var enemyPlayer = new Battle.Player(2, GetTutorialEnemyDeck());
            var player = new Battle.Player(2, GetTutorialPlayerDeck());
            battleState.ItemsPerRound = 0;
            battleState.CardsPerRound = 4;
            battleState.StartBattle(player, enemyPlayer, baseItems);

            sceneLinks.InputBlocker.gameObject.SetActive(true);
            sceneLinks.VC_LookAtEnemy.Priority.Enabled = true;
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
            if (!sceneLinks.FastMode)
            {
                await dPrinter.PrintByLine(
                    "О, смотрю у нас новенький вылез.",
                    "Если хочешь тут выжить, то придется тебе раздобыть Сигнальный Огни.",
                    "Они дают свет и отпугивают разных тварей вокруг",
                    "У меня как раз есть несколько, можем сыграть.",
                    "Но помни, играем до последнего, а кто поиграл, сдохнет в темноте.",
                    "Ха-ха-ха."
                );
            }

            dPrinter.Clear();

            if (!sceneLinks.FastMode)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

                await dPrinter.PrintByLine(
                    "Играть будем в камень ножница бумага коза фак",
                    "Слышал о такой? Она тут популярна",
                    "Играем до трех побед");

                sceneLinks.VC_LookAtEnemy.Priority.Enabled = false;
                sceneLinks.VC_LookAtInstruction.Priority.Enabled = true;
                await dPrinter.PrintByLine("Правила можешь глянуть тут");

                sceneLinks.VC_LookAtInstruction.Priority.Enabled = false;
                sceneLinks.VC_LookAtFlaresCard.Priority.Enabled = true;
                await dPrinter.PrintByLine(
                    "Тут мои сигнальные огни",
                    "Когда у кого-то не останется огней, тот проиграл"
                );

                sceneLinks.VC_LookAtFlaresCard.Priority.Enabled = false;
                sceneLinks.VC_LookAtEnemy.Priority.Enabled = true;
            }

            await dPrinter.PrintDialogue(Dialogue.Create().Clear().Text("Все понял? Можем начинать.")
                .Delay(Dialogue.LongDelay).Clear());
            sceneLinks.DialoguePanel.gameObject.SetActive(false);

            sceneLinks.VC_LookAtEnemy.Priority.Enabled = false;
            sceneLinks.InputBlocker.gameObject.SetActive(false);

            var tcs = new UniTaskCompletionSource();
            battleState.NotStartNextRound = true;
            battleState.OnRoundEnd += OnAllCardsUsed;
            await tcs.Task;

            void OnAllCardsUsed()

            {
                battleState.OnRoundEnd -= OnAllCardsUsed;
                tcs.TrySetResult();
            }

            sceneLinks.InputBlocker.gameObject.SetActive(true);
            sceneLinks.DialoguePanel.gameObject.SetActive(true);

            sceneLinks.VC_LookAtPlayerStones.Priority.Enabled = true;
            await dPrinter.PrintByLine(
                "Ты набрал больше камней, этот раунд за тобой"
            );


            player.ReplaceBag(GetBaseDeck());
            enemyPlayer.ReplaceBag(GetBaseDeck());
            battleState.ItemsPerRound = 2;
            battleState.CardsPerRound = 5;
            battleState.NotStartNextRound = false;

            sceneLinks.VC_LookAtPlayerStones.Priority.Enabled = false;
            battleState.StartNewRound();

            await dPrinter.PrintByLine(
                "Давай немного усложним игру",
                "Теперь каждый раунд будут выдаваться предметы",
                "Так же я добавил пару карт, чтобы сделать игру интереснее",
                "Глянь в инструкции, чтобы понять, как они работают."
            );

            dPrinter.Clear();
            sceneLinks.DialoguePanel.gameObject.SetActive(false);
            sceneLinks.InputBlocker.gameObject.SetActive(false);


            void OnBattleEnd(bool playerWon) => UniTask.Create(async () =>
            {
                battleState.OnGameEnd -= OnBattleEnd;

                if (playerWon)
                {
                    sceneLinks.InputBlocker.gameObject.SetActive(true);
                    sceneLinks.DialoguePanel.gameObject.SetActive(true);
                    await dPrinter.PrintByLine("Черт, не может быть");
                    sceneLinks.InputBlocker.gameObject.SetActive(true);
                    sceneLinks.DialoguePanel.gameObject.SetActive(true);

                    sceneLinks.FirstPersonEvent.gameObject.SetActive(false);

                    battleState.OnExit();

                    SecondEvent().Forget();
                }
                else
                {
                    sceneLinks.InputBlocker.gameObject.SetActive(true);
                    sceneLinks.DialoguePanel.gameObject.SetActive(true);
                    await dPrinter.PrintByLine("У тебя не было шансов");
                    G.Resolve<IGameDirector>().RestartGame();
                }
            });
        }

        public async UniTask SecondEvent()
        {
            var dPrinter = new DialoguePrinter(sceneLinks.DialoguePanel);

            battleState.OnGameEnd += OnBattleEnd;

            var enemyPlayer = new Battle.Player(4, GetSecondDeck());
            var player = new Battle.Player(3, GetBaseDeck());
            battleState.ItemsPerRound = 2;
            battleState.CardsPerRound = 5;
            battleState.StartBattle(player, enemyPlayer, baseItems);

            sceneLinks.SecondPersonEvent.gameObject.SetActive(true);
            sceneLinks.InputBlocker.gameObject.SetActive(true);
            sceneLinks.VC_LookAtEnemy.Priority.Enabled = true;

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

            await dPrinter.PrintByLine("Oh… you want to play with me? Um… okay, let’s try...");
            sceneLinks.VC_LookAtEnemy.Priority.Enabled = false;
            sceneLinks.InputBlocker.gameObject.SetActive(false);
            sceneLinks.DialoguePanel.gameObject.SetActive(false);


            void OnBattleEnd(bool playerWon) => UniTask.Create(async () =>
            {
                battleState.OnGameEnd -= OnBattleEnd;

                if (playerWon)
                {
                    sceneLinks.InputBlocker.gameObject.SetActive(true);
                    sceneLinks.DialoguePanel.gameObject.SetActive(true);
                    await dPrinter.PrintByLine("I knew this would happen sooner or later...");
                    sceneLinks.InputBlocker.gameObject.SetActive(true);
                    sceneLinks.DialoguePanel.gameObject.SetActive(true);

                    ThirdEvent().Forget();
                }
                else
                {
                    sceneLinks.InputBlocker.gameObject.SetActive(true);
                    sceneLinks.DialoguePanel.gameObject.SetActive(true);
                    await dPrinter.PrintByLine("Oh my gosh… I didn’t mean to. I’m so sorry...");
                    G.Resolve<IGameDirector>().RestartGame();
                }
            });
        }

        public async UniTask ThirdEvent()
        {
        }
    }
}