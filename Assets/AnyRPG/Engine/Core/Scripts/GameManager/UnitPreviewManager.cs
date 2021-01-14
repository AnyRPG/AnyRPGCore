using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class UnitPreviewManager : PreviewManager {

        #region Singleton
        private static UnitPreviewManager instance;

        public static UnitPreviewManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<UnitPreviewManager>();
                }

                return instance;
            }
        }

        #endregion


        public override UnitProfile GetCloneSource() {
            return UnitSpawnControlPanel.MyInstance.MySelectedUnitSpawnButton.MyUnitProfile;
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