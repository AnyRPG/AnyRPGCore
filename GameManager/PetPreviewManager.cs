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


        public override GameObject GetCloneSource() {
            return PetSpawnControlPanel.MyInstance.MySelectedPetSpawnButton.MyUnitProfile.UnitPrefab;
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