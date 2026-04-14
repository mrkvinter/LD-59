using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace KvinterGames
{
    public class AudioService : MonoBehaviour
    {
        private const int MAX_SOUNDS = 20;

        [SerializeField] private Sound[] sounds;
        [SerializeField] private MixerGroup[] mixerGroups;

        private readonly List<AudioSource> soundAudioSources = new();
        private int currentSourceIndex;
        private readonly Dictionary<string, Sound> soundDictionary = new();

        protected void Awake()
        {
            foreach (var sound in sounds)
            {
                soundDictionary[sound.Name] = sound;
            }

            for (int i = 0; i < MAX_SOUNDS; i++)
            {
                var go = new GameObject($"AudioSource{i}");
                go.transform.SetParent(transform);
                var audioSource = go.AddComponent<AudioSource>();
                soundAudioSources.Add(audioSource);
            }
        }

        public AudioSound PlaySound(AudioClip clip, float pitchRandomness = 0, float volume = 1,
            bool fadeOut = false, float pitch = 1, string mixerGroup = "default")
        {
            var mixer = Array.Find(mixerGroups, m => m.Name == mixerGroup).Group;

            var audioSource = GetFreeAudioSource();
            audioSource.volume = volume;
            audioSource.pitch = pitch + Random.Range(-pitchRandomness, pitchRandomness);
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = mixer;
            audioSource.PlayOneShot(clip);
            if (fadeOut)
            {
                audioSource.DOFade(0, 1)
                    .SetDelay(clip.length - 1);
            }

            return new AudioSound(audioSource, clip, mixer);
        }

        public AudioSound PlaySound(string soundName, float pitchRandomness = 0, float volume = 1,
            bool fadeOut = false, float pitch = 1, bool fadeIn = false, string mixerGroup = "default")
        {
            if (soundDictionary.TryGetValue(soundName, out var soundInfo))
            {
                var mixer = Array.Find(mixerGroups, m => m.Name == mixerGroup).Group;

                var audioSource = GetFreeAudioSource();
                audioSource.volume = volume;
                audioSource.pitch = pitch + Random.Range(-pitchRandomness, pitchRandomness);
                audioSource.outputAudioMixerGroup = mixer;
                var clipIndex = Random.Range(0, soundInfo.Clips.Length);
                var clip = soundInfo.Clips[clipIndex];
                audioSource.clip = clip;
                audioSource.PlayOneShot(clip);
                if (fadeOut)
                {
                    audioSource.DOFade(0, 1)
                        .SetDelay(clip.length - 1);
                }
                if (fadeIn)
                {
                    audioSource.volume = 0;
                    audioSource.DOFade(volume, 1);
                }
                return new AudioSound(audioSource, clip, mixer);
            }
            else
            {
                Debug.LogError($"Sound with name {soundName} not found");
                return null;
            }
        }

        private AudioSource GetFreeAudioSource()
        {
            var audioSource = soundAudioSources[currentSourceIndex];
            currentSourceIndex = (currentSourceIndex + 1) % soundAudioSources.Count;
            return audioSource;
        }

        public LoopSound PlayLoop(AudioClip clip, string mixerGroup = "default")
        {
            var mixer = Array.Find(mixerGroups, m => m.Name == mixerGroup).Group;
            var audioSource = new GameObject("LoopAudioSource").AddComponent<AudioSource>();
            audioSource.transform.SetParent(transform);
            return new LoopSound(audioSource, clip, mixer);
        }
        
        public LoopSound PlayLoop(string soundName, string mixerGroup = "default")
        {
            if (soundDictionary.TryGetValue(soundName, out var soundInfo))
            {
                var mixer = Array.Find(mixerGroups, m => m.Name == mixerGroup).Group;
                var audioSource = new GameObject("LoopAudioSource").AddComponent<AudioSource>();
                audioSource.transform.SetParent(transform);
                var clipIndex = Random.Range(0, soundInfo.Clips.Length);
                var clip = soundInfo.Clips[clipIndex];
                return new LoopSound(audioSource, clip, mixer);
            }
            else
            {
                Debug.LogError($"Sound with name {soundName} not found");
                return null;
            }
        }
        
        public abstract class BaseAudioSound
        {
            public AudioSource AudioSource { get; }
            protected AudioMixerGroup MixerGroup { get; set; }

            protected BaseAudioSound(AudioSource audioSource, AudioMixerGroup mixerGroup)
            {
                AudioSource = audioSource;
                MixerGroup = mixerGroup;
            }
        }
        public class LoopSound : BaseAudioSound, IDisposable
        {
            public LoopSound(AudioSource audioSource, AudioClip clip, AudioMixerGroup audioMixerGroup) : base(audioSource, audioMixerGroup)
            {
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.outputAudioMixerGroup = audioMixerGroup;
                audioSource.Play();
            }

            public void Stop(float fadeOutTime = 0)
            {
                if (fadeOutTime > 0)
                {
                    AudioSource.DOFade(0, fadeOutTime)
                        .OnComplete(Dispose);
                }
                else
                {
                    Dispose();
                }
            }
            
            public void Pause()
            {
                AudioSource.Pause();
            }
            
            public void Resume()
            {
                AudioSource.UnPause();
            }

            public void Dispose()
            {
                AudioSource.Stop();
                AudioSource.clip = null;
                AudioSource.loop = false;
            }
        }
        
        public class AudioSound : BaseAudioSound
        {
            public AudioClip Clip { get; }
            public AudioSound(AudioSource audioSource, AudioClip clip, AudioMixerGroup audioMixerGroup) : base(audioSource, audioMixerGroup)
            {
                Clip = clip;
            }
        }
    }

    [Serializable]
    public struct Sound
    {
        public string Name;
        public AudioClip[] Clips;
    }
    
    [Serializable]
    public struct MixerGroup
    {
        public string Name;
        public AudioMixerGroup Group;
    }
}