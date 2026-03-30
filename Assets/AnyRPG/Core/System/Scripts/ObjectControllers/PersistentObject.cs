using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(UUID))]
    public class PersistentObject : AutoConfiguredMonoBehaviour, IPersistentObjectOwner {

        [SerializeField]
        private PersistentObjectComponent persistentObjectComponent = new PersistentObjectComponent();

        private UUID uuid = null;

        // game manager references
        private LevelManagerServer levelManagerServer = null;

        public IUUID UUID { get => uuid; }
        public PersistentObjectComponent PersistentObjectComponent { get => persistentObjectComponent; set => persistentObjectComponent = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            GetComponentReferences();
            persistentObjectComponent.Setup(this, systemGameManager);

            RegisterWithLevelManager();

            // testing : moved here from start() for object pooling.  monitor for breakage
            persistentObjectComponent.Init();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            levelManagerServer = systemGameManager.LevelManagerServer;
        }

        private void RegisterWithLevelManager() {
            if (persistentObjectComponent.SaveOnGameSave == false && persistentObjectComponent.SaveOnLevelUnload == false) {
                return;
            }
            if (levelManagerServer != null) {
                levelManagerServer.RegisterPersistentObject(this);
            }
        }


        public void GetComponentReferences() {
            uuid = GetComponent<UUID>();
        }

        public void PopulatePersistentObjectSaveData(PersistentObjectSaveData persistentObjectSaveData) {
            // nothing to do here.  This object doesn't have any data to save, but the method needs to be here to satisfy the interface
        }

    }

}

