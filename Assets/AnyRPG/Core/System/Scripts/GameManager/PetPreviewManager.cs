using AnyRPG;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class PetPreviewManager : PreviewManager {

        /*
        public override UnitProfile GetCloneSource() {
            return PetSpawnControlPanel.Instance.MySelectedPetSpawnButton.MyUnitProfile;
        }
        */

        public void HandleOpenWindow(PetSpawnControlPanel petSpawnControlPanel) {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow()");

            //cloneSource = GetCloneSource();
            unitProfile = petSpawnControlPanel.SelectedPetSpawnButton.MyUnitProfile;
            if (unitProfile == null) {
                return;
            }

            SpawnUnit();
        }

    }
}