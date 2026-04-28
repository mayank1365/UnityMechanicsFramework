using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "UMF/Audio/Audio Config")]
    public class AudioConfig_UMFOSS : ScriptableObject
    {
        [Header("SFX Mappings")]
        public List<SFXEntry> sfxEntries;

        [Header("Music Mappings")]
        public List<MusicEntry> musicEntries;

        [Header("Ambient Mappings")]
        public List<AmbientEntry> ambientEntries;
    }

    [Serializable]
    public class SFXEntry
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
        [Range(0f, 0.5f)] public float pitchRandomness = 0.1f;

        public float GetRandomPitch() => 1f + UnityEngine.Random.Range(-pitchRandomness, pitchRandomness);
    }

    [Serializable]
    public class MusicEntry
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Serializable]
    public class AmbientEntry
    {
        public string key;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }
}
