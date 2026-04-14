using UnityEngine;

namespace Game.Utilities
{
    public abstract class BaseImageAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private float frameRate = 0.1f;
        [SerializeField] private bool playOnAwake = true;
        [SerializeField] private AnimationType animationType = AnimationType.Loop;
        
        private bool isPlaying;
        private int currentFrame;
        private float timer;

        public void Play()
        {
            isPlaying = true;
            timer = 0;
            currentFrame = 0;
            SetSprite(sprites[currentFrame]);
        }
        
        public void Pause()
        {
            isPlaying = false;
        }
        
        public void Stop()
        {
            isPlaying = false;
            currentFrame = 0;
            SetSprite(sprites[currentFrame]);
        }

        private void Awake()
        {
            if (playOnAwake)
                Play();
        }

        private void Update()
        {
            if (!isPlaying || timer >= frameRate && animationType == AnimationType.Once)
                return;

            timer += Time.deltaTime;
            if (timer >= frameRate)
            {
                timer = 0;
                currentFrame++;
                if (currentFrame >= sprites.Length)
                {
                    switch (animationType)
                    {
                        case AnimationType.Loop:
                            currentFrame = 0;
                            break;
                        case AnimationType.Once:
                            isPlaying = false;
                            return;
                    }
                }
                
                if (currentFrame < sprites.Length)
                    SetSprite(sprites[currentFrame]);
            }
        }

        protected abstract void SetSprite(Sprite sprite);

        private enum AnimationType
        {
            Loop,
            Once
        }

        public void SetSprites(Sprite[] newSprites)
        {
            sprites = newSprites;
        }
    }
}