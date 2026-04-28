using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// A robust pool-based audio manager providing crossfaded music, ambient looping tracks, 
    /// and spatial 2D/3D sound effects powered by EventBus and ScriptableObject configurations.
    /// </summary>
    public class AudioManager_UMFOSS : MonoSingletonGeneric<AudioManager_UMFOSS>
    {
        public AudioConfig_UMFOSS audioConfig;

        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 10;

        public float MasterVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float AmbientVolume { get; private set; }

        private List<SFXSourceData> sfxPool;
        private AudioSource musicSource;
        private AudioSource ambientSource;

        private Coroutine musicCrossfadeCoroutine;
        private MusicEntry currentMusicEntry;
        private AmbientEntry currentAmbientEntry;

        private bool isMuted = false;
        private float preMuteMasterVolume;
        private float preMuteSFXVolume;
        private float preMuteMusicVolume;
        private float preMuteAmbientVolume;

        // Internal class for pool state tracking
        private class SFXSourceData
        {
            public AudioSource Source;
            public float BaseVolume;
            public float StartTime;
            public float BusyUntilTime;

            public bool IsIdle => Time.time >= BusyUntilTime && !Source.isPlaying;
        }

        protected override void Awake()
        {
            base.Awake();

            if (audioConfig == null)
            {
                Debug.LogError("[AudioManager] AudioConfig is not assigned in the inspector!");
            }

            LoadVolumes();

            // Initialize SFX Pool
            sfxPool = new List<SFXSourceData>(sfxPoolSize);
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject go = new GameObject($"SFXSource_{i}");
                go.transform.SetParent(transform);
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Add(new SFXSourceData { Source = source });
            }

            // Initialize Music Source
            GameObject musicGo = new GameObject("MusicSource");
            musicGo.transform.SetParent(transform);
            musicSource = musicGo.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;

            // Initialize Ambient Source
            GameObject ambientGo = new GameObject("AmbientSource");
            ambientGo.transform.SetParent(transform);
            ambientSource = ambientGo.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlaySFXEvent>(OnPlaySFX);
            EventBus.Subscribe<PlayMusicEvent>(OnPlayMusic);
            EventBus.Subscribe<StopMusicEvent>(OnStopMusic);
            EventBus.Subscribe<SetVolumeEvent>(OnSetVolume);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlaySFXEvent>(OnPlaySFX);
            EventBus.Unsubscribe<PlayMusicEvent>(OnPlayMusic);
            EventBus.Unsubscribe<StopMusicEvent>(OnStopMusic);
            EventBus.Unsubscribe<SetVolumeEvent>(OnSetVolume);
        }

        private void LoadVolumes()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
            AmbientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.5f);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            // The logic for applying specific volumes to sources 
            // is handled in Play methods via calculations.
        }

        #region SFX API
        public void PlaySFX(string key, Vector3 position)
        {
            SFXEntry entry = FindSFXEntry(key);
            if (entry == null) return;

            PlaySFX(entry, position);
        }

        public void PlaySFX(SFXEntry entry, Vector3 position)
        {
            SFXSourceData data = GetAvailableSFXSource();
            if (data == null) return;

            data.Source.transform.position = position;
            data.Source.clip = entry.clip;
            data.Source.spatialBlend = entry.spatialBlend;
            data.Source.pitch = entry.GetRandomPitch();
            data.Source.volume = entry.volume * SFXVolume * MasterVolume;
            
            data.StartTime = Time.time;
            data.BusyUntilTime = Time.time + entry.clip.length;
            data.BaseVolume = entry.volume;

            data.Source.Play();
        }

        public void StopAllSFX()
        {
            foreach (var data in sfxPool)
            {
                data.Source.Stop();
                data.BusyUntilTime = 0;
            }
        }

        private SFXSourceData GetAvailableSFXSource()
        {
            // 1. First look for idle sources
            foreach (var data in sfxPool)
            {
                if (data.IsIdle) return data;
            }

            // 2. If pool exhausted, steal the oldest source
            SFXSourceData oldest = sfxPool[0];
            foreach (var data in sfxPool)
            {
                if (data.StartTime < oldest.StartTime) oldest = data;
            }

            oldest.Source.Stop();
            return oldest;
        }
        #endregion

        #region Music API
        public void PlayMusic(string key, bool fadeIn = true)
        {
            MusicEntry newEntry = FindMusicEntry(key);
            if (newEntry == null) return;

            if (currentMusicEntry != null && currentMusicEntry.key == key && musicSource.isPlaying)
                return;

            if (musicCrossfadeCoroutine != null)
                StopCoroutine(musicCrossfadeCoroutine);

            musicCrossfadeCoroutine = StartCoroutine(MusicCrossfadeCoroutine(newEntry, fadeIn));
            currentMusicEntry = newEntry;

            EventBus.Publish(new MusicStartedEvent { key = newEntry.key });
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (musicSource.isPlaying)
            {
                if (fadeOut)
                {
                    if (musicCrossfadeCoroutine != null) StopCoroutine(musicCrossfadeCoroutine);
                    musicCrossfadeCoroutine = StartCoroutine(MusicFadeOutCoroutine(1.5f));
                }
                else
                {
                    musicSource.Stop();
                }
                
                string lastKey = currentMusicEntry?.key ?? "";
                currentMusicEntry = null;
                EventBus.Publish(new MusicStoppedEvent { key = lastKey });
            }
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }

        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        private IEnumerator MusicCrossfadeCoroutine(MusicEntry newEntry, bool fadeIn)
        {
            float duration = fadeIn ? 1.5f : 0.1f;
            float startVolume = musicSource.isPlaying ? musicSource.volume : 0;
            float elapsed = 0;

            // Fade Out previous
            if (startVolume > 0)
            {
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
                    yield return null;
                }
            }

            // Update Clip
            musicSource.clip = newEntry.clip;
            musicSource.Play();

            // Fade In next
            elapsed = 0;
            float targetVolume = newEntry.volume * MusicVolume * MasterVolume;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0, targetVolume, elapsed / duration);
                yield return null;
            }
            
            musicSource.volume = targetVolume;
            musicCrossfadeCoroutine = null;
        }

        private IEnumerator MusicFadeOutCoroutine(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = 0;
            musicCrossfadeCoroutine = null;
        }
        #endregion

        #region Ambient API
        public void PlayAmbient(string key)
        {
            AmbientEntry entry = FindAmbientEntry(key);
            if (entry == null) return;

            if (currentAmbientEntry != null && currentAmbientEntry.key == key && ambientSource.isPlaying)
                return;

            ambientSource.clip = entry.clip;
            ambientSource.volume = entry.volume * AmbientVolume * MasterVolume;
            ambientSource.Play();
            currentAmbientEntry = entry;
        }

        public void StopAmbient()
        {
            ambientSource.Stop();
            currentAmbientEntry = null;
        }
        #endregion

        #region Volume Control
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            ApplyVolumes();
            PublishVolumeChanged(AudioCategory.Master, MasterVolume);
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
            ApplyVolumes();
            PublishVolumeChanged(AudioCategory.SFX, SFXVolume);
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            if (currentMusicEntry != null)
                musicSource.volume = currentMusicEntry.volume * MusicVolume * MasterVolume;
            PublishVolumeChanged(AudioCategory.Music, MusicVolume);
        }

        public void SetAmbientVolume(float volume)
        {
            AmbientVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("AmbientVolume", AmbientVolume);
            if (currentAmbientEntry != null)
                ambientSource.volume = currentAmbientEntry.volume * AmbientVolume * MasterVolume;
            PublishVolumeChanged(AudioCategory.Ambient, AmbientVolume);
        }

        private void PublishVolumeChanged(AudioCategory category, float volume)
        {
            EventBus.Publish(new VolumeChangedEvent { category = category, volume = volume });
        }

        public void MuteAll(bool mute)
        {
            if (mute && !isMuted)
            {
                preMuteMasterVolume = MasterVolume;
                SetMasterVolume(0);
                isMuted = true;
                EventBus.Publish(new AllAudioMutedEvent());
            }
            else if (!mute && isMuted)
            {
                SetMasterVolume(preMuteMasterVolume);
                isMuted = false;
                EventBus.Publish(new AllAudioUnmutedEvent());
            }
        }

        public void UnmuteAll() => MuteAll(false);

        public void PauseAll(bool pause)
        {
            if (pause) AudioListener.pause = true;
            else AudioListener.pause = false;
        }

        public void ResumeAll() => PauseAll(false);
        #endregion

        #region Event Handlers
        private void OnPlaySFX(PlaySFXEvent e)
        {
            PlaySFX(e.key, e.position);
        }

        private void OnPlayMusic(PlayMusicEvent e)
        {
            PlayMusic(e.key, e.fadeIn);
        }

        private void OnStopMusic(StopMusicEvent e)
        {
            StopMusic(e.fadeOut);
        }

        private void OnSetVolume(SetVolumeEvent e)
        {
            switch (e.category)
            {
                case AudioCategory.Master: SetMasterVolume(e.volume); break;
                case AudioCategory.SFX: SetSFXVolume(e.volume); break;
                case AudioCategory.Music: SetMusicVolume(e.volume); break;
                case AudioCategory.Ambient: SetAmbientVolume(e.volume); break;
            }
        }
        #endregion

        #region Helpers
        private SFXEntry FindSFXEntry(string key)
        {
            if (audioConfig == null) return null;
            return audioConfig.sfxEntries.Find(e => e.key == key);
        }

        private MusicEntry FindMusicEntry(string key)
        {
            if (audioConfig == null) return null;
            return audioConfig.musicEntries.Find(e => e.key == key);
        }

        private AmbientEntry FindAmbientEntry(string key)
        {
            if (audioConfig == null) return null;
            return audioConfig.ambientEntries.Find(e => e.key == key);
        }
        #endregion
    }

    // Local Event Definitions
    public struct MusicStartedEvent { public string key; }
    public struct MusicStoppedEvent { public string key; }
    public struct AllAudioMutedEvent { }
    public struct AllAudioUnmutedEvent { }
}
