using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class UnitPreviewManager : PreviewManager {

        #region Singleton
        private static UnitPreviewManager instance;

        public static UnitPreviewManager Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
        }
        #endregion


        public override UnitProfile GetCloneSource() {
            return UnitSpawnControlPanel.Instance.MySelectedUnitSpawnButton.MyUnitProfile;
        }

        public void HandleOpenWindow() {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");

            cloneSource = GetCloneSource();
            if (cloneSource == null) {
                return;
            }

            OpenWindowCommon();
        }


    }
}