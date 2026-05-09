using UnityEngine;

public static class ProceduralAudio
{
    private const int SampleRate = 44100;

    public static AudioClip CreateClick()
    {
        return CreateTone(0.06f, 1200f, 28f, 0.3f);
    }

    public static AudioClip CreatePopupOpen()
    {
        return CreateSweep(0.18f, 400f, 800f, 8f, 0.28f);
    }

    public static AudioClip CreatePopupClose()
    {
        return CreateSweep(0.16f, 800f, 380f, 9f, 0.26f);
    }

    public static AudioClip CreateMirrorRotate()
    {
        return CreateSweep(0.14f, 600f, 950f, 12f, 0.32f);
    }

    public static AudioClip CreateBatteryCharge()
    {
        return CreateChord(new float[] { 523f, 659f, 784f }, 0.3f, 4f, 0.32f);
    }

    public static AudioClip CreateEnergyStarCollect()
    {
        return CreateBell(880f, 0.4f, 0.3f);
    }

    public static AudioClip CreateWin()
    {
        return CreateChord(new float[] { 523f, 659f, 784f, 1047f }, 0.6f, 2.5f, 0.35f);
    }

    private static AudioClip CreateTone(float duration, float frequency, float decay, float volume)
    {
        int sampleCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * decay);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
        }
        var clip = AudioClip.Create("ProcTone", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateSweep(float duration, float startFreq, float endFreq, float decay, float volume)
    {
        int sampleCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float k = t / duration;
            float freq = Mathf.Lerp(startFreq, endFreq, k);
            phase += 2f * Mathf.PI * freq / SampleRate;
            float envelope = Mathf.Exp(-t * decay);
            samples[i] = Mathf.Sin(phase) * envelope * volume;
        }
        var clip = AudioClip.Create("ProcSweep", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateChord(float[] freqs, float duration, float decay, float volume)
    {
        int sampleCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float invCount = 1f / freqs.Length;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * decay);
            float v = 0f;
            for (int f = 0; f < freqs.Length; f++)
            {
                v += Mathf.Sin(2f * Mathf.PI * freqs[f] * t);
            }
            samples[i] = v * invCount * envelope * volume;
        }
        var clip = AudioClip.Create("ProcChord", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateBell(float fundamental, float duration, float volume)
    {
        int sampleCount = Mathf.RoundToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] harmonics = new float[] { 1f, 2f, 3f, 4.2f };
        float[] amps = new float[] { 1f, 0.5f, 0.3f, 0.2f };
        float ampSum = 2f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-t * 3.5f);
            float v = 0f;
            for (int h = 0; h < harmonics.Length; h++)
            {
                v += Mathf.Sin(2f * Mathf.PI * fundamental * harmonics[h] * t) * amps[h];
            }
            samples[i] = v / ampSum * envelope * volume;
        }
        var clip = AudioClip.Create("ProcBell", sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
