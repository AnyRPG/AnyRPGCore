using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    [System.Serializable]
    public class PooledObjectConfig {

        [Tooltip("A reference to the GameObject this configuration applies to")]
        [SerializeField]
        private GameObject pooledObject = null;

        [Tooltip("If objects should persist through scene changes, set this to true")]
        [SerializeField]
        private bool persistSceneChange = false;

        [Tooltip("Set this value to true to instantiate objects when the game is loaded")]
        [SerializeField]
        private bool preloadPool = false;

        [Tooltip("The number of objects that should be preloaded")]
        [SerializeField]
        private int preloadCount = 0;

        [Tooltip("The maximum number of objects to instantiate before re-using them. Zero is unlimited.")]
        [SerializeField]
        private int maxObjects = 0;

        public GameObject PooledObject { get => pooledObject; set => pooledObject = value; }
        public bool PersistSceneChange { get => persistSceneChange; set => persistSceneChange = value; }
        public bool PreloadPool { get => preloadPool; set => preloadPool = value; }
        public int PreloadCount { get => preloadCount; set => preloadCount = value; }
        public int MaxObjects { get => maxObjects; set => maxObjects = value; }
    }
}