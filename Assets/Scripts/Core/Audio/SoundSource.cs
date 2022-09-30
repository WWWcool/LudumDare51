using System;
using System.Collections.Generic;
using Hellmade;
using UnityEngine;
using Utils;
using Zenject;
using Random = UnityEngine.Random;

namespace Core.Audio
{
    [CreateAssetMenu(fileName = "SoundSource", menuName = "Repositories/SoundSource")]
    public class SoundSource : ScriptableObject
    {
        public List<AudioClip> variants = new List<AudioClip>();
        public FloatRange pitchRange;
        public float baseVolume;
        public Hellmade.Audio.AudioType audioType;
        public bool loop;
        public bool persist;
        public float fadeIn;
        public float fadeOut;
        public float min3DDistance = 100f;

        private EazySoundManager _soundManager;

        private Func<int, Hellmade.Audio>[] _audioGetters;
        private Func<AudioClip, float, bool, Transform, int>[] _audioInitializers;

        [Inject]
        public void Construct(EazySoundManager soundManager)
        {
            _soundManager = soundManager;
            InitAudioGetters();
            InitAudioInitializers();
        }

        private void InitAudioInitializers()
        {
            _audioInitializers = new Func<AudioClip, float, bool, Transform, int>[]
            {
                (clip, volume, aLoop, transform) => _soundManager.PrepareMusic(clip, baseVolume, loop, transform),
                (clip, volume, aLoop, transform) => _soundManager.PrepareSound(clip, baseVolume, loop, transform),
                (clip, volume, aLoop, transform) => _soundManager.PrepareUISound(clip, baseVolume, loop, transform)
            };
        }

        private void InitAudioGetters()
        {
            _audioGetters = new Func<int, Hellmade.Audio>[]
            {
                id => _soundManager.GetMusicAudio(id),
                id => _soundManager.GetSoundAudio(id),
                id => _soundManager.GetUISoundAudio(id)
            };
        }

        public void Play(Transform transform = null)
        {
            if (variants.Count == 0) return;
            var clip = variants[Random.Range(0, variants.Count)];
            var pitch = Random.Range(pitchRange.min, pitchRange.max);
            var soundId = _audioInitializers[(int) audioType].Invoke(clip, baseVolume, loop, transform);
            var audio = _audioGetters[(int) audioType].Invoke(soundId);
            if (audioType != Hellmade.Audio.AudioType.Music) audio.Pitch = pitch;
            audio.FadeInSeconds = fadeIn;
            audio.FadeOutSeconds = fadeOut;
            audio.Min3DDistance = min3DDistance;
            audio.Play();
        }
    }
}