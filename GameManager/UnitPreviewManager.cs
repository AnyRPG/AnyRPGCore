using AnyRPG;
using System.Collections;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;


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


        public override GameObject GetCloneSource() {
            return UnitSpawnControlPanel.MyInstance.MySelectedUnitSpawnButton.MyUnitProfile.UnitPrefab;
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