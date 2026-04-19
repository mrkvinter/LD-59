using System.Threading;
using Code.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Code.Game.Scripts.GameStates
{
    public class DialoguePrinter
    {
        private readonly DialoguePanel panel;

        public string SpeakerName { get; set; }

        public DialoguePrinter(DialoguePanel panel)
        {
            this.panel = panel;
        }

        public UniTask PrintDialogue(Dialogue dialogue, CancellationToken token = default)
        {
            return panel.ShowDialogueAsync(dialogue, token);
        }

        public UniTask PrintByLine(params string[] lines) => PrintByLine(CancellationToken.None, lines);

        public async UniTask PrintByLine(CancellationToken token, params string[] lines)
        {
            foreach (var line in lines)
            {
                if (token.IsCancellationRequested) return;

                var dialogue = Dialogue.Create().SetSpeaker(SpeakerName).Clear().Text(line);
                var showing = true;
                panel.ShowDialogueAsync(dialogue, token)
                    .ContinueWith(() => showing = false)
                    .Forget();

                await UniTask.NextFrame(token);

                while (showing)
                {
                    if (WasClicked())
                        panel.Skip();
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                await UniTask.WaitUntil(WasClicked, cancellationToken: token);
            }
        }

        public void Clear() => panel.Clear();

        private static bool WasClicked()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) return true;

            var gamepad = Gamepad.current;
            if (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame) return true;

            return false;
        }
    }
}