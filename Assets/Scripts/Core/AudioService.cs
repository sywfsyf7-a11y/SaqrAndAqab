using System;
using UnityEngine;

namespace SaqrAndAqab.Core
{
    // SFX + music. Missing audio files become small procedural clips so the game is never silent.
    public static class AudioService
    {
        const int SampleRate = 44100;
        const int MaxSources = 12;

        static GameObject _root;
        static readonly System.Collections.Generic.List<AudioSource> _sources = new System.Collections.Generic.List<AudioSource>();
        static AudioSource _musicSource;
        static readonly System.Collections.Generic.Dictionary<string, AudioClip> _clips = new System.Collections.Generic.Dictionary<string, AudioClip>();

        static float MusicVolume
        {
            get { return SaveService.Data != null ? Mathf.Clamp01(SaveService.Data.musicVolume) : 1f; }
        }

        static float SfxVolume
        {
            get { return SaveService.Data != null ? Mathf.Clamp01(SaveService.Data.sfxVolume) : 1f; }
        }

        public static void Play(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (SfxVolume <= 0f) return;

            EnsureRoot();

            var clip = GetClip(id);
            if (clip == null) return;

            var source = GetSource();
            if (source == null) return;

            source.clip = clip;
            source.loop = false;
            source.volume = SfxVolume;
            source.Play();
        }

        public static void PlayMusic(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            EnsureRoot();
            if (_musicSource == null)
            {
                var go = new GameObject("Music Source");
                go.transform.SetParent(_root.transform, false);
                _musicSource = go.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            var clip = GetMusicClip(name);
            if (clip == null)
            {
                if (_musicSource.isPlaying) _musicSource.Stop();
                _musicSource.clip = null;
                return;
            }

            if (_musicSource.clip == clip && _musicSource.isPlaying) return;

            _musicSource.clip = clip;
            _musicSource.volume = MusicVolume;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        public static void StopMusic()
        {
            if (_musicSource != null && _musicSource.isPlaying) _musicSource.Stop();
        }

        public static void ApplyVolumes()
        {
            if (_musicSource != null) _musicSource.volume = MusicVolume;

            for (int i = 0; i < _sources.Count; i++)
            {
                if (_sources[i] != null) _sources[i].volume = SfxVolume;
            }
        }

        static void EnsureRoot()
        {
            if (_root != null) return;

            _root = new GameObject("Audio Service");
            if (Application.isPlaying) UnityEngine.Object.DontDestroyOnLoad(_root);
        }

        static AudioSource GetSource()
        {
            for (int i = 0; i < _sources.Count; i++)
            {
                var source = _sources[i];
                if (source != null && !source.isPlaying) return source;
            }

            if (_sources.Count >= MaxSources) return null;

            var go = new GameObject("SFX Source");
            go.transform.SetParent(_root.transform, false);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.volume = SfxVolume;
            _sources.Add(src);
            return src;
        }

        static AudioClip GetClip(string id)
        {
            AudioClip clip;
            if (_clips.TryGetValue(id, out clip) && clip != null) return clip;

            clip = Resources.Load<AudioClip>("Audio/" + id);
            if (clip == null) clip = GenerateClip(id);

            if (clip != null) _clips[id] = clip;
            return clip;
        }

        static AudioClip GetMusicClip(string name)
        {
            return Resources.Load<AudioClip>("Audio/music_" + name);
        }

        static AudioClip GenerateClip(string id)
        {
            // Short procedural bursts: sine with a quick decay, with per-id frequency.
            int samples = SampleRate / 6; // ~0.16 s
            float freq = FrequencyFor(id);

            var clip = AudioClip.Create("sfx_" + id, samples, 1, SampleRate, false);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float envelope = Mathf.Exp(-t * 18f);
                float value = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;

                // Add a little noise for impact sounds.
                if (id == "hit" || id == "boss_slam" || id == "lose")
                {
                    value += (UnityEngine.Random.value * 2f - 1f) * envelope * 0.25f;
                }

                data[i] = Mathf.Clamp(value, -1f, 1f) * 0.6f;
            }

            clip.SetData(data, 0);
            return clip;
        }

        static float FrequencyFor(string id)
        {
            switch (id)
            {
                case "hit": return 220f;
                case "slash": return 660f;
                case "arrow": return 440f;
                case "coin": return 880f;
                case "summon": return 330f;
                case "heal": return 520f;
                case "boss_slam": return 110f;
                case "win": return 740f;
                case "lose": return 180f;
                case "ui_click": return 1000f;
                default: return 440f;
            }
        }
    }
}
