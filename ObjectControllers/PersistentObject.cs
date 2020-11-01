using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(UUID))]
    public class PersistentObject : MonoBehaviour, IPersistentObjectOwner {

        [SerializeField]
        private PersistentObjectComponent persistentObjectComponent = new PersistentObjectComponent();

        private UUID uuid = null;

        public UUID UUID { get => uuid; set => uuid = value; }
        public PersistentObjectComponent PersistentObjectComponent { get => persistentObjectComponent; set => persistentObjectComponent = value; }

        private void Awake() {
            GetComponentReferences();
            persistentObjectComponent.Setup(this);
        }

        // Start is called before the first frame update
        void Start() {
            persistentObjectComponent.Init();
        }

        public void GetComponentReferences() {
            uuid = GetComponent<UUID>();
        }

        private void OnDisable() {
            persistentObjectComponent.Cleanup();
        }

    }

}

