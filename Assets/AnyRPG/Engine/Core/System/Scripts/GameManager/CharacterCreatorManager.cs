using AnyRPG;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCreatorManager : PreviewManager {

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

            while (CharacterPanel.Instance.MyPreviewCameraController == null) {
                yield return null;
            }

            CharacterPanel.Instance.MyPreviewCameraController.InitializeCamera(unitController);
        }


    }
}