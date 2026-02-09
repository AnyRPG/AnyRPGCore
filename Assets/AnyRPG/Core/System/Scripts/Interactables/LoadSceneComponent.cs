using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadSceneComponent : PortalComponent {

        public LoadSceneProps LoadSceneProps { get => interactableOptionProps as LoadSceneProps; }

        public LoadSceneComponent(Interactable interactable, LoadSceneProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{interactable.gameObject.name}.LoadSceneComponent.Interact({sourceUnitController.gameObject.name}, {componentIndex})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            //levelManager.LoadLevel(LoadSceneProps.SceneName);
            // give until the end of the frame so that client interactions can finish before the scene changes
            interactable.StartCoroutine(LoadSceneDelay(LoadSceneProps.SceneName, sourceUnitController));
            //playerManagerServer.LoadScene(LoadSceneProps.SceneName, sourceUnitController);
            return true;
        }

        public IEnumerator LoadSceneDelay(string sceneName, UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.LoadSceneComponent.LoadSceneDelay()");

            yield return new WaitForEndOfFrame();
            //levelManager.LoadLevel(sceneName);
            playerManagerServer.LoadScene(sceneName, sourceUnitController);
        }

    }
}