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

        private void OnEnable() {
            GetComponentReferences();
            persistentObjectComponent.Setup(this);

            // testing : moved here from start() for object pooling.  monitor for breakage
            persistentObjectComponent.Init();
        }

        /*
        void Start() {
            persistentObjectComponent.Init();
        }
        */

        public void GetComponentReferences() {
            uuid = GetComponent<UUID>();
        }

        private void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            persistentObjectComponent.Cleanup();
        }

    }

}

