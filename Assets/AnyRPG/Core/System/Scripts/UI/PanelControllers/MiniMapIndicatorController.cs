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

        private Dictionary<InteractableOptionComponent, GameObject> miniMapLayers = new Dictionary<InteractableOptionComponent, GameObject>();

        private Interactable interactable = null;

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
            foreach (InteractableOptionComponent _interactable in interactable.Interactables) {
                // prioritize images - DICTIONARY DOESN'T CURRENTLY SUPPORT BOTH
                if (_interactable.HasMiniMapIcon()) {
                    //else if (_interactable.HasMiniMapIcon()) {
                    // do both now!
                    GameObject go = objectPooler.GetPooledObject(miniMapImageLayerPrefab, contentParent);
                    Image _image = go.GetComponent<Image>();
                    _interactable.SetMiniMapIcon(_image);
                    miniMapLayers.Add(_interactable, go);
                } else if (_interactable.HasMiniMapText()) {
                    GameObject go = objectPooler.GetPooledObject(miniMapTextLayerPrefab, contentParent);
                    TextMeshProUGUI _text = go.GetComponent<TextMeshProUGUI>();
                    _interactable.SetMiniMapText(_text);
                    miniMapLayers.Add(_interactable, go);
                }
            }
            setupComplete = true;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log(gameObject.name + ".MiniMapIndicatorController.SetInteractable(" + interactable.gameObject.name + "): instance: " + instanceNumber);
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
                _interactable.SetMiniMapIcon(miniMapLayers[_interactable].GetComponent<Image>());
            } else if (_interactable.HasMiniMapText()) {
                _interactable.SetMiniMapText(miniMapLayers[_interactable].GetComponent<TextMeshProUGUI>());
            }
        }

        public void ResetSettings() {
            foreach (GameObject go in miniMapLayers.Values) {
                objectPooler.ReturnObjectToPool(go);
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