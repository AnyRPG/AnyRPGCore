using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class UnitPreviewManager : PreviewManager {

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