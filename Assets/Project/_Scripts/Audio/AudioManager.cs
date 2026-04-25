using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace Audio
{
    /// <summary>
    /// Handles the game's audio system.
    /// Is a singleton class.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        // ───── Singleton ────────────────────────────────────────────────
        
        public static AudioManager Instance { get; private set; }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        // ───── Serialized properties ────────────────────────────────────────────────
        
        [Header("Pool")]
        [SerializeField] private int _poolSize = 20;
        
        [Space(10.0f)]
        
        [Header("Master Volumes")]
        [SerializeField] [Range(0.0f, 1.0f)] private float _sfxVolume = 1.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _musicVolume = 0.8f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _uiVolume = 1.0f;
        [SerializeField] [Range(0.0f, 1.0f)] private float _voiceVolume = 1.0f;

        // ───── Private properties ────────────────────────────────────────────────
        
        private readonly Queue<AudioSource> _availableSources = new Queue<AudioSource>();
        private readonly List<AudioSource> _activeSources = new List<AudioSource>();
        
        private static ProfilerMarker _playSoundMarker = new ProfilerMarker("AudioManager.Play");

        // ───── Lifecycle methods ────────────────────────────────────────────────
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitialisePool();
        }

        private void Update()
        {
            ReturnFinishedSources();
        }

        // ───── Public methods ────────────────────────────────────────────────
        
        /// <summary>
        /// Plays a sound at a world position (e.g. explosion).
        /// </summary>
        /// <param name="data"></param>
        /// <param name="position"></param>
        public void Play(SoundData data, Vector3 position)
        {
            using (_playSoundMarker.Auto())
            {
                if (data == null || !data.IsValid) { return; }
                
                AudioSource source = GetSource();
                if (source == null) { return; }
                
                ConfigureSource(source, data, position);
                source.transform.position = position;
                source.Play();
                
                _activeSources.Add(source);
            }
        }
        
        /// <summary>
        /// Plays a sound that follows an object (e.g. car engine sounds).
        /// </summary>
        /// <param name="data"></param>
        /// <param name="position"></param>
        /// <param name="parent"></param>
        public void Play(SoundData data, Vector3 position, Transform parent)
        {
            using (_playSoundMarker.Auto())
            {
                if (data == null || !data.IsValid) { return; }
                
                AudioSource source = GetSource();
                if (source == null) { return; }
                
                source.transform.SetParent(parent);
                ConfigureSource(source, data, position);
                source.Play();
                
                _activeSources.Add(source);
            }
        }
        
        /// <summary>
        /// Plays a sound with a specified volume multiplier (e.g. quieter crouch steps).
        /// </summary>
        /// <param name="data"></param>
        /// <param name="position"></param>
        /// <param name="volumeMultiplier"></param>
        public void Play(SoundData data, Vector3 position, float volumeMultiplier)
        {
            using (_playSoundMarker.Auto())
            {
                if (data == null || !data.IsValid) { return; }
                
                AudioSource source = GetSource();
                if (source == null) { return; }
                
                ConfigureSource(source, data, position);
                
                source.volume *= volumeMultiplier;
                source.transform.position = position;
                source.Play();
                
                _activeSources.Add(source);
            }
        }
        
        /// <summary>
        /// Plays a 2D sound (e.g. UI sounds).
        /// </summary>
        /// <param name="data"></param>
        public void Play2D(SoundData data)
        {
            using (_playSoundMarker.Auto())
            {
                if (data == null || !data.IsValid) { return; }
                
                AudioSource source = GetSource();
                if (source == null) { return; }

                source.transform.position = Vector3.zero;
                ConfigureSource(source, data, Vector3.zero);
                source.spatialBlend = 0.0f;
                source.Play();

                _activeSources.Add(source);
            }
        }
        
        public void SetVolume(AudioCategory category, float volume)
        {
            switch (category)
            {
                case AudioCategory.SFX: _sfxVolume = volume; break;
                case AudioCategory.Music: _musicVolume = volume; break;
                case AudioCategory.UI: _uiVolume = volume; break;
                case AudioCategory.Voice: _voiceVolume = volume; break;
            }
        }
        
        public float GetVolume(AudioCategory category)
        {
            return category switch
            {
                AudioCategory.SFX => _sfxVolume,
                AudioCategory.Music => _musicVolume,
                AudioCategory.UI => _uiVolume,
                AudioCategory.Voice => _voiceVolume,
                _ => 1.0f
            };
        }

        // ───── Private methods ────────────────────────────────────────────────
        
        private void InitialisePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                AudioSource source = CreateSource();
                _availableSources.Enqueue(source);
            }
        }
        
        private AudioSource CreateSource()
        {
            GameObject go = new GameObject("AudioSource");
            go.transform.SetParent(transform);
            go.SetActive(false);
            
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }
        
        private AudioSource GetSource()
        {
            AudioSource source;

            if (_availableSources.Count > 0)
            {
                source = _availableSources.Dequeue();
                source.gameObject.SetActive(true);
            }
            else
            {
                // Pool exhausted — create new source
                Debug.LogWarning("[AudioManager] Pool exhausted. Consider increasing pool size.");
                source = CreateSource();
                source.gameObject.SetActive(true);
            }

            return source;
        }
        
        private void ConfigureSource(AudioSource source, SoundData data, Vector3 position)
        {
            float categoryVolume = GetVolume(data.Category);

            source.clip = data.GetClip();
            source.volume = data.GetVolume() * categoryVolume;
            source.pitch = data.GetPitch();
            source.spatialBlend = data.SpatialBlend;
            source.maxDistance = data.MaxDistance;
            source.minDistance = data.MinDistance;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.loop = false;

            source.transform.SetParent(transform);
            source.transform.position = position;
        }
        
        private void ReturnFinishedSources()
        {
            for (int i = _activeSources.Count - 1; i >= 0; i--)
            {
                AudioSource source = _activeSources[i];

                if (source == null || !source.isPlaying)
                {
                    if (source != null)
                    {
                        source.transform.SetParent(transform);
                        source.gameObject.SetActive(false);
                        _availableSources.Enqueue(source);
                    }

                    _activeSources.RemoveAt(i);
                }
            }
        }
    }
}

