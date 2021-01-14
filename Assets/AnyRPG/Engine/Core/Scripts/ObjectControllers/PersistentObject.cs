using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(UUID))]
    public class PersistentObject : MonoBehaviour, IPersistentObjectOwner {

        [SerializeField]
        private PersistentObjectComponent persistentObjectComponent = new PersistentObjectComponent();

        private UUID uuid = null;

        public IUUID UUID { get => uuid; }
        public PersistentObjectComponent PersistentObjectComponent { get => persistentObjectComponent; set => persistentObjectComponent = value; }

        private void Awake() {
            GetComponentReferences();
            persistentObjectComponent.Setup(this);
        }

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

