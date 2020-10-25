using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(UUID))]
    public class PersistentObject : MonoBehaviour, IPersistentObjectOwner {

        [SerializeField]
        private PersistentObjectComponent persistentObjectComponent = new PersistentObjectComponent();

        [Tooltip("If true, this object will save it's position when switching from one scene to another (including the main menu).  It will not save if the game is quit directly from the main menu.")]
        [SerializeField]
        private bool saveOnLevelUnload = false;

        [Tooltip("If true, this object will save it's position when the player saves the game.")]
        [SerializeField]
        private bool saveOnGameSave = false;

        private UUID uuid = null;

        public UUID UUID { get => uuid; set => uuid = value; }
        public PersistentObjectComponent PersistentObjectComponent { get => persistentObjectComponent; set => persistentObjectComponent = value; }

        private void Awake() {
            GetComponentReferences();
            persistentObjectComponent.Initialize(this);
        }

        // Start is called before the first frame update
        void Start() {
            persistentObjectComponent.Start();
        }

        public void GetComponentReferences() {
            uuid = GetComponent<UUID>();
        }

        private void OnDisable() {
            persistentObjectComponent.OnDisable();
        }

    }

}

