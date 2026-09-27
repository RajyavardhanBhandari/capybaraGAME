using CapybaraGame.Core;
using UnityEngine;

namespace CapybaraGame.Audio
{
    public enum SfxType
    {
        Button,
        Placement,
        Removal,
        Invalid,
        LifeLost,
        Completion,
        Reward,
        PerfectCompletion,
        Unlock,
        NextLevel
    }

    public sealed class AudioService : MonoBehaviour
    {
        private AudioSource sfxSource;
        private AudioSource musicSource;
        private readonly AudioClip[] generated = new AudioClip[10];
        private bool enabledState = true;

        public void Initialize(LocalSave save)
        {
            enabledState = save == null || save.sound;
            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            musicSource.playOnAwake = false;
            GenerateClips();
        }

        public void SetEnabled(bool enabled) => enabledState = enabled;

        public void Play(SfxType type)
        {
            if (!enabledState || sfxSource == null) return;
            int index = (int)type;
            if (index < 0 || index >= generated.Length || generated[index] == null) return;
            sfxSource.PlayOneShot(generated[index], type == SfxType.Completion || type == SfxType.PerfectCompletion ? 0.7f : 0.45f);
        }

        private void GenerateClips()
        {
            generated[(int)SfxType.Button] = Tone("Button", 620f, 0.055f);
            generated[(int)SfxType.Placement] = Tone("Placement", 740f, 0.075f);
            generated[(int)SfxType.Removal] = Tone("Removal", 420f, 0.065f);
            generated[(int)SfxType.Invalid] = Tone("Invalid", 180f, 0.09f);
            generated[(int)SfxType.LifeLost] = Tone("LifeLost", 140f, 0.12f);
            generated[(int)SfxType.Completion] = Chord("Completion", 520f, 660f, 0.28f);
            generated[(int)SfxType.Reward] = Chord("Reward", 660f, 880f, 0.18f);
            generated[(int)SfxType.PerfectCompletion] = Chord("PerfectCompletion", 660f, 880f, 0.32f);
            generated[(int)SfxType.Unlock] = Chord("Unlock", 520f, 780f, 0.22f);
            generated[(int)SfxType.NextLevel] = Tone("NextLevel", 700f, 0.08f);
        }

        private static AudioClip Tone(string name, float frequency, float duration)
        {
            const int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Clamp01(1f - i / (float)samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.25f;
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip Chord(string name, float a, float b, float duration)
        {
            const int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Clamp01(1f - i / (float)samples);
                data[i] = (Mathf.Sin(2f * Mathf.PI * a * t) + Mathf.Sin(2f * Mathf.PI * b * t)) * 0.11f * envelope;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}

namespace CapybaraGame.Platform
{
    public enum HapticType
    {
        Tiny,
        Light,
        Medium,
        Strong,
        Celebration
    }

    public sealed class HapticService
    {
        private bool enabledState = true;

        public void Initialize(LocalSave save) => enabledState = save == null || save.haptics;
        public void SetEnabled(bool enabled) => enabledState = enabled;

        public void Play(HapticType type)
        {
            if (!enabledState || !Application.isMobilePlatform) return;
            Handheld.Vibrate();
        }
    }
}