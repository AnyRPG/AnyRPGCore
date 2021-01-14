using AnyRPG;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PetPreviewManager : PreviewManager {

        #region Singleton
        private static PetPreviewManager instance;

        public static PetPreviewManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<PetPreviewManager>();
                }

                return instance;
            }
        }

        #endregion


        public override UnitProfile GetCloneSource() {
            return PetSpawnControlPanel.MyInstance.MySelectedPetSpawnButton.MyUnitProfile;
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