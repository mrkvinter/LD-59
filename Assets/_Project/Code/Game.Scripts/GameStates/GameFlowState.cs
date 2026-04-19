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

        private List<string> lorePhrases = new()
        {
            "Сколько лет уже "
        };

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
            ItemDefType.Knife,
            ItemDefType.Knife,
            ItemDefType.Pills,
            ItemDefType.Pills,
            ItemDefType.SpareSignalFlare,
            ItemDefType.FortuneCookie,
            ItemDefType.FortuneCookie,
            ItemDefType.BrokenGlass,

            // ItemDefType.Whetstone,
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

            var rumble = G.AudioService.PlayLoop("rumble_loop");
            rumble.AudioSource.volume = 0.6f;

            FirstEvent().Forget();
        }

        public async UniTask GameOverAsync()
        {
        }

        public async UniTask FirstEvent()
        {
            sceneLinks.BlackScreen.alpha = 1;

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
            if (!sceneLinks.FastMode)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
                await sceneLinks.KvinterGames.DOFade(1, 0.5f);
                await UniTask.Delay(TimeSpan.FromSeconds(2f));
                await sceneLinks.KvinterGames.DOFade(0, 0.5f);
                await sceneLinks.Alarm.DOFade(1, 0.5f);
                await UniTask.Delay(TimeSpan.FromSeconds(2));
                await sceneLinks.Alarm.DOFade(0, 0.5f);
                await sceneLinks.BlackScreen.DOFade(0, 4f);
            }
            else
            {
                sceneLinks.BlackScreen.alpha = 0;
            }

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
                    "Oh, looks like we've got a newcomer here.",

                    "If you want to survive here, you'll have to get your hands on some Signal Flares.",

                    "They give off light and scare away all kinds of creatures around here.",

                    "I happen to have a few; we can play.",

                    "But remember, we play until the very end, and whoever loses will die in the dark.",
                    
                    "Ha-ha-ha."
                );
            }

            dPrinter.Clear();

            if (!sceneLinks.FastMode)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

                await dPrinter.PrintByLine(
                    "We're going to play Rock, Paper, Scissors, Goat, F@#k.",
                    "Ever heard of it? It's popular around here.",
                    "Your goal: to get Signal Flares.",
                    "3 stones = 1 Signal Flare. Either you or I will get them.",
                    "But here's the thing: every time you lose, I'll take them from you.",
                    "Don’t let your guard down. I can also determine the winner based on the number of stones."
                    );

                sceneLinks.VC_LookAtEnemy.Priority.Enabled = false;
                sceneLinks.VC_LookAtInstruction.Priority.Enabled = true;
                await dPrinter.PrintByLine("You can check the rules here");

                sceneLinks.VC_LookAtInstruction.Priority.Enabled = false;
                sceneLinks.VC_LookAtFlaresCard.Priority.Enabled = true;
                await dPrinter.PrintByLine(
                    "Here are my signal flares",
                    "When one of us runs out of flares, they lose"
                );

                sceneLinks.VC_LookAtFlaresCard.Priority.Enabled = false;
                sceneLinks.VC_LookAtEnemy.Priority.Enabled = true;
            }

            await dPrinter.PrintDialogue(Dialogue.Create().Clear().Text("Got it? Let’s get started.")
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
                "You have more stones, this round is yours"
            );


            player.ReplaceBag(GetBaseDeck());
            enemyPlayer.ReplaceBag(GetBaseDeck());
            battleState.ItemsPerRound = 3;
            battleState.CardsPerRound = 5;
            battleState.NotStartNextRound = false;

            sceneLinks.VC_LookAtPlayerStones.Priority.Enabled = false;
            battleState.StartNewRound();

            await dPrinter.PrintByLine(
                "Let’s make the game a little more complicated.",
                "Now, items will be handed out each round.",
                "I’ve also added a couple of cards to make the game more interesting.",
                "Check the instructions to see how they work."
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
                    sceneLinks.VC_LookAtEnemy.Priority.Enabled = true;
                    await dPrinter.PrintByLine("Damn, that can't be.");
                    dPrinter.Clear();

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
                    await dPrinter.PrintByLine("You had no chance.");
                    G.Resolve<IGameDirector>().RestartGame();
                }
            });
        }

        public async UniTask SecondEvent()
        {
            var dPrinter = new DialoguePrinter(sceneLinks.DialoguePanel);

            battleState.OnGameEnd += OnBattleEnd;
            
            sceneLinks.MainLight.intensity = 0;
            await sceneLinks.HandStatefulObject.SetStateAsync("Default");

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
            var dPrinter = new DialoguePrinter(sceneLinks.DialoguePanel);

            battleState.OnGameEnd += OnBattleEnd;

            sceneLinks.MainLight.intensity = 0;
            await sceneLinks.HandStatefulObject.SetStateAsync("Default");

            var enemyPlayer = new Battle.Player(5, GetSecondDeck());
            var player = new Battle.Player(3, GetBaseDeck());
            battleState.ItemsPerRound = 2;
            battleState.CardsPerRound = 5;
            battleState.StartBattle(player, enemyPlayer, baseItems);

            sceneLinks.ThirdPersonEvent.gameObject.SetActive(true);
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

            sceneLinks.DialoguePanel.gameObject.SetActive(true);
            await dPrinter.PrintByLine("Go ahead, surprise me. Though I doubt you'll manage it.");
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
                    sceneLinks.VC_LookAtEnemy.Priority.Enabled = true;
                    await dPrinter.PrintByLine(
                        "No way, that's my game!"
                    );

                    battleState.OnExit();

                    await GameOverAsync();
                }
                else
                {
                    sceneLinks.InputBlocker.gameObject.SetActive(true);
                    sceneLinks.DialoguePanel.gameObject.SetActive(true);
                    await dPrinter.PrintByLine("What was that all about? Seriously?");
                    G.Resolve<IGameDirector>().RestartGame();
                }
            });
        }
    }
}