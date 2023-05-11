using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class MiniMapIndicatorController : ConfiguredMonoBehaviour {

        [SerializeField]
        private GameObject miniMapTextLayerPrefab = null;

        [SerializeField]
        private GameObject miniMapImageLayerPrefab = null;

        [SerializeField]
        private Transform contentParent = null;

        private Dictionary<InteractableOptionComponent, MiniMapIndicatorLayerController> miniMapLayers = new Dictionary<InteractableOptionComponent, MiniMapIndicatorLayerController>();

        private Interactable interactable = null;
        private InteractableOptionComponent highestPriorityInteractable = null;
        private int highestPriorityValue = 0;

        private bool setupComplete = false;

        // game manager references

        private ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
        }

        public void SetupMiniMap() {
            //Debug.Log(transform.parent.gameObject.name + ".MiniMapIndicatorController.SetupMiniMap(): interactable: " + (interactable == null ? "null" : interactable.name));

            if (setupComplete == true) {
                return;
            }
            if (interactable == null) {
                return;
            }
            foreach (InteractableOptionComponent interactableOptionComponent in interactable.Interactables) {
                // prioritize images - DICTIONARY DOESN'T CURRENTLY SUPPORT BOTH
                if (interactableOptionComponent.HasMiniMapIcon()) {
                    GameObject go = objectPooler.GetPooledObject(miniMapImageLayerPrefab, contentParent);
                    MiniMapIndicatorLayerController miniMapIndicatorLayerController = go.GetComponent<MiniMapIndicatorLayerController>();
                    miniMapIndicatorLayerController.Setup(interactableOptionComponent);
                    miniMapLayers.Add(interactableOptionComponent, miniMapIndicatorLayerController);
                } else if (interactableOptionComponent.HasMiniMapText()) {
                    GameObject go = objectPooler.GetPooledObject(miniMapTextLayerPrefab, contentParent);
                    MiniMapIndicatorLayerController miniMapIndicatorLayerController = go.GetComponent<MiniMapIndicatorLayerController>();
                    miniMapIndicatorLayerController.Setup(interactableOptionComponent);
                    miniMapLayers.Add(interactableOptionComponent, miniMapIndicatorLayerController);
                }
            }
            EnableHighestPriorityLayer();
            setupComplete = true;
        }

        private void EnableHighestPriorityLayer() {
            highestPriorityValue = 0;
            highestPriorityInteractable = null;
            foreach (KeyValuePair<InteractableOptionComponent, MiniMapIndicatorLayerController> keyValuePair in miniMapLayers) {
                if (keyValuePair.Value.IsActive == false) {
                    continue;
                }
                if (keyValuePair.Key.PriorityValue > highestPriorityValue || highestPriorityInteractable == null) {
                    highestPriorityValue = keyValuePair.Key.PriorityValue;
                    highestPriorityInteractable = keyValuePair.Key;
                }
            }

            if (highestPriorityInteractable == null) {
                return;
            }
            foreach (KeyValuePair<InteractableOptionComponent, MiniMapIndicatorLayerController> keyValuePair in miniMapLayers) {
                if (keyValuePair.Key == highestPriorityInteractable) {
                    keyValuePair.Value.gameObject.SetActive(true);
                } else {
                    keyValuePair.Value.gameObject.SetActive(false);
                }
            }
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log($"{gameObject.name}.MiniMapIndicatorController.SetInteractable(" + interactable.gameObject.name + "): instance: " + instanceNumber);
            this.interactable = interactable;
            SetupMiniMap();
        }

        public void HandleMiniMapStatusUpdate(InteractableOptionComponent _interactable) {
            //Debug.Log(_interactable.Interactable.gameObject.name + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate()");
            if (miniMapLayers.ContainsKey(_interactable) == false || miniMapLayers[_interactable] == null) {
                //Debug.Log(_interactable.DisplayName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate(): miniMapLayers[_interactable] is null! Exiting");
                return;
            }
            // this only supports one or the other too - prioritizing images
            if (_interactable.HasMiniMapIcon()) {
                miniMapLayers[_interactable].ConfigureDisplay();
            } else if (_interactable.HasMiniMapText()) {
                miniMapLayers[_interactable].ConfigureDisplay();
            }

            EnableHighestPriorityLayer();
        }

        public void ResetSettings() {
            foreach (MiniMapIndicatorLayerController miniMapIndicatorLayerController in miniMapLayers.Values) {
                objectPooler.ReturnObjectToPool(miniMapIndicatorLayerController.gameObject);
            }
            miniMapLayers.Clear();
            interactable = null;
            setupComplete = false;
        }

        /*
        public void OnPointerEnter(BaseEventData eventData) {
            if (characterUnit.MyInteractable != null) {
                characterUnit.MyInteractable.OnMouseEnter();
            }
        }

        public void OnPointerExit(BaseEventData eventData) {
            if (characterUnit.MyInteractable != null) {
                characterUnit.MyInteractable.OnMouseExit();
            }
        }
        */

    }

}