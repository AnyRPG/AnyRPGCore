using AnyRPG;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PetPreviewManager : PreviewManager {

        public override UnitProfile GetCloneSource() {
            return PetSpawnControlPanel.Instance.MySelectedPetSpawnButton.MyUnitProfile;
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