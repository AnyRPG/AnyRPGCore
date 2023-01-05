using AnyRPG;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCreatorManager : PreviewManager {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        public void ConfirmAction() {
            OnConfirmAction();
        }

        public void EndInteraction() {
            OnEndInteraction();
        }

        public void HandleOpenWindow(UnitProfile unitProfile) {
            //Debug.Log("CharacterCreatorManager.HandleOpenWindow(" + (unitProfile == null ? "null" : unitProfile.DisplayName) + ")");

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

    }
}