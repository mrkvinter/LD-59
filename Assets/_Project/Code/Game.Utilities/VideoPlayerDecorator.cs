using System;
using UnityEngine.Video;

namespace Game.Utilities
{
    public class VideoPlayerDecorator
    {
        private readonly VideoPlayer videoPlayer;

        public event Action<VideoPlayer> OnEnd;
        public event Action<VideoPlayer, string> OnError;

        public VideoPlayerDecorator(VideoPlayer videoPlayer)
        {
            this.videoPlayer = videoPlayer;
        }

        public void Start()
        {
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnLoopPointReached;
            videoPlayer.errorReceived += OnErrorReceived;
        }
        
        public void Stop()
        {
            videoPlayer.Stop();
            videoPlayer.loopPointReached -= OnLoopPointReached;
            videoPlayer.errorReceived -= OnErrorReceived;

            OnEnd?.Invoke(videoPlayer);
        }
        
        private void OnLoopPointReached(VideoPlayer source)
        {
            Stop();
        }
        
        private void OnErrorReceived(VideoPlayer source, string message)
        {
            OnError?.Invoke(source, message);
        }
    }
}