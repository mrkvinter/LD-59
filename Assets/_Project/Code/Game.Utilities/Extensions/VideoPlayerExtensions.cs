using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

namespace Game.Utilities.Extensions
{
    public static class VideoPlayerExtensions
    {
        public static async UniTask PlayAsync(this VideoPlayerDecorator videoPlayer)
        {
            var tcs = new UniTaskCompletionSource();
            videoPlayer.OnEnd += OnLoopPointReached;
            videoPlayer.OnError += OnErrorReceived;
            videoPlayer.Start();
            await tcs.Task;
            videoPlayer.OnEnd -= OnLoopPointReached;
            videoPlayer.OnError -= OnErrorReceived;
            return;

            void OnLoopPointReached(VideoPlayer vp)
            {
                tcs.TrySetResult();
            }
            
            void OnErrorReceived(VideoPlayer source, string message)
            {
                Debug.LogError($"VideoPlayer error: {message}");
                videoPlayer.Stop();
            }
        }
    }
}