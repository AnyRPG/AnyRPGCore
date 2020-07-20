using AnyRPG;
using System.Collections;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCreatorManager : PreviewManager {

        #region Singleton
        private static CharacterCreatorManager instance;

        public static CharacterCreatorManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CharacterCreatorManager>();
                }

                return instance;
            }
        }

        #endregion


        /*
        public override GameObject GetCloneSource() {
            return unitProfile.UnitPrefab;
        }
        */

        public void HandleOpenWindow(UnitProfile unitProfile) {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");

            if (unitProfile == null) {
                Debug.Log("CharacterCreatorManager.HandleOpenWindow(): unitProfile is null");
                return;
            }
            cloneSource = unitProfile.UnitPrefab;
            if (cloneSource == null) {
                return;
            }

            OpenWindowCommon();
        }

        public IEnumerator WaitForCamera() {
            //Debug.Log("CharacterCreatorManager.WaitForCamera();");

            while (CharacterPanel.MyInstance.MyPreviewCameraController == null) {
                yield return null;
            }
            //Debug.Log("WaitForCamera(): got camera");

            CharacterPanel.MyInstance.MyPreviewCameraController.InitializeCamera(previewUnit.transform);
            //targetInitialized = true;

        }


    }
}