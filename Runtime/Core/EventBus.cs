using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// A simple, decoupled event bus system using the Observer pattern.
    /// Allows mechanics to communicate without direct references.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> subscribers = new Dictionary<Type, List<Delegate>>();

        /// <summary>Subscribe a handler to an event type.</summary>
        public static void Subscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            if (!subscribers.ContainsKey(eventType))
            {
                subscribers[eventType] = new List<Delegate>();
            }
            subscribers[eventType].Add(handler);
        }

        /// <summary>Unsubscribe a handler from an event type.</summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            if (subscribers.ContainsKey(eventType))
            {
                subscribers[eventType].Remove(handler);
            }
        }

        /// <summary>Publish an event to all subscribers of its type.</summary>
        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);
            if (subscribers.ContainsKey(eventType))
            {
                // Create a copy of the list to avoid issues if handlers subscribe/unsubscribe during iteration
                foreach (var handler in new List<Delegate>(subscribers[eventType]))
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
            }
        }

        /// <summary>Removes every subscriber. Useful for tests and full resets.</summary>
        public static void ClearAll() => subscribers.Clear();
    }

    // --- Core Audio Events ---

    public enum AudioCategory { Master, SFX, Music, Ambient }

    public struct PlaySFXEvent
    {
        public string key;
        public Vector3 position;
    }

    public struct PlayMusicEvent
    {
        public string key;
        public bool fadeIn;
    }

    public struct StopMusicEvent
    {
        public bool fadeOut;
    }

    public struct SetVolumeEvent
    {
        public AudioCategory category;
        public float volume;
    }

    public struct VolumeChangedEvent
    {
        public AudioCategory category;
        public float volume;
    }
}
