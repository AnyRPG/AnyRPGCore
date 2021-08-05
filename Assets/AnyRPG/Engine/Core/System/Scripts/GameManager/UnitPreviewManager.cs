using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class UnitPreviewManager : PreviewManager {

        /*
        public override UnitProfile GetCloneSource() {
            return UnitSpawnControlPanel.Instance.SelectedUnitSpawnButton.MyUnitProfile;
        }
        */

        public void HandleOpenWindow(UnitSpawnControlPanel unitSpawnControlPanel) {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");

            //cloneSource = GetCloneSource();
            cloneSource = unitSpawnControlPanel.SelectedUnitSpawnButton.UnitProfile;
            if (cloneSource == null) {
                return;
            }

            OpenWindowCommon();
        }


    }
}