using AnyRPG;
using System.Collections;
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


        public void HandleOpenWindow(UnitProfile unitProfile) {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");

            if (unitProfile == null) {
                Debug.Log("CharacterCreatorManager.HandleOpenWindow(): unitProfile is null");
                return;
            }
            cloneSource = unitProfile;
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

            CharacterPanel.MyInstance.MyPreviewCameraController.InitializeCamera(unitController);
        }


    }
}