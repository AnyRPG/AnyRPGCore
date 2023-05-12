using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class MainMapIndicatorController : ConfiguredMonoBehaviour {

        [SerializeField]
        private GameObject mainMapTextLayerPrefab = null;

        [SerializeField]
        private GameObject mainMapImageLayerPrefab = null;

        [SerializeField]
        private Transform contentParent = null;

        private Dictionary<InteractableOptionComponent, MiniMapIndicatorLayerController> mainMapLayers = new Dictionary<InteractableOptionComponent, MiniMapIndicatorLayerController>();

        private Interactable interactable = null;

        private bool setupComplete = false;

        // game manager references

        private ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;
        }

        public void SetupMainMap() {
            //Debug.Log((interactable == null ? "null" : interactable.name) + ".MainMapIndicatorController.SetupMainMap()");
            if (setupComplete == true) {
                return;
            }
            if (interactable == null) {
                return;
            }
            foreach (InteractableOptionComponent interactableOptionComponent in interactable.Interactables) {
                //Debug.Log((interactable == null ? "null" : interactable.name) + ".MainMapIndicatorController.SetupMainMap(): checking " + _interactable.ToString());

                // prioritize images - DICTIONARY DOESN'T CURRENTLY SUPPORT BOTH
                if (interactableOptionComponent.HasMainMapIcon()) {
                    //Debug.Log((interactable == null ? "null" : interactable.name) + ".MainMapIndicatorController.SetupMainMap(): adding icon : " + _interactable.ToString());
                    GameObject go = objectPooler.GetPooledObject(mainMapImageLayerPrefab, contentParent);
                    MiniMapIndicatorLayerController miniMapIndicatorLayerController = go.GetComponent<MiniMapIndicatorLayerController>();
                    miniMapIndicatorLayerController.Setup(interactableOptionComponent);
                    mainMapLayers.Add(interactableOptionComponent, miniMapIndicatorLayerController);
                } else if (interactableOptionComponent.HasMainMapText()) {
                    //Debug.Log((interactable == null ? "null" : interactable.name) + ".MainMapIndicatorController.SetupMainMap(): adding text layer: " + _interactable.ToString());
                    GameObject go = objectPooler.GetPooledObject(mainMapTextLayerPrefab, contentParent);
                    MiniMapIndicatorLayerController miniMapIndicatorLayerController = go.GetComponent<MiniMapIndicatorLayerController>();
                    miniMapIndicatorLayerController.Setup(interactableOptionComponent);
                    mainMapLayers.Add(interactableOptionComponent, miniMapIndicatorLayerController);
                }
            }
            setupComplete = true;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log($"{gameObject.name}.MainMapIndicatorController.SetInteractable(" + interactable.gameObject.name + "): instance: " + instanceNumber);
            this.interactable = interactable;
            SetupMainMap();
        }

        public void HandleMainMapStatusUpdate(InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log(_interactable.Interactable.gameObject.name + ".MainMapIndicatorController.HandleMainMapStatusUpdate()");
            if (mainMapLayers.ContainsKey(interactableOptionComponent) == false ||  mainMapLayers[interactableOptionComponent] == null) {
                //Debug.Log(_interactable.DisplayName + ".MainMapIndicatorController.HandleMainMapStatusUpdate(): mainMapLayers[_interactable] is null! Exiting");
                return;
            }
            // this only supports one or the other too - prioritizing images
            if (interactableOptionComponent.HasMainMapIcon()) {
                mainMapLayers[interactableOptionComponent].ConfigureDisplay();
            } else if (interactableOptionComponent.HasMainMapText()) {
                mainMapLayers[interactableOptionComponent].ConfigureDisplay();
            }
        }

        public void ResetSettings() {
            foreach (MiniMapIndicatorLayerController miniMapIndicatorLayerController in mainMapLayers.Values) {
                objectPooler.ReturnObjectToPool(miniMapIndicatorLayerController.gameObject);
            }
            mainMapLayers.Clear();
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