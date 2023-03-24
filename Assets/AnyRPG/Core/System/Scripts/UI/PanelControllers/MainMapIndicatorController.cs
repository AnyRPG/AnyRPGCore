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

        private Dictionary<InteractableOptionComponent, GameObject> mainMapLayers = new Dictionary<InteractableOptionComponent, GameObject>();

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
            foreach (InteractableOptionComponent _interactable in interactable.Interactables) {
                //Debug.Log((interactable == null ? "null" : interactable.name) + ".MainMapIndicatorController.SetupMainMap(): checking " + _interactable.ToString());

                // prioritize images - DICTIONARY DOESN'T CURRENTLY SUPPORT BOTH
                if (_interactable.HasMainMapIcon()) {
                    //Debug.Log((interactable == null ? "null" : interactable.name) + ".MainMapIndicatorController.SetupMainMap(): adding icon : " + _interactable.ToString());
                    GameObject go = objectPooler.GetPooledObject(mainMapImageLayerPrefab, contentParent);
                    Image _image = go.GetComponent<Image>();
                    _interactable.SetMiniMapIcon(_image);
                    mainMapLayers.Add(_interactable, go);
                } else if (_interactable.HasMainMapText()) {
                    //Debug.Log((interactable == null ? "null" : interactable.name) + ".MainMapIndicatorController.SetupMainMap(): adding text layer: " + _interactable.ToString());
                    GameObject go = objectPooler.GetPooledObject(mainMapTextLayerPrefab, contentParent);
                    TextMeshProUGUI _text = go.GetComponent<TextMeshProUGUI>();
                    _interactable.SetMiniMapText(_text);
                    mainMapLayers.Add(_interactable, go);
                }
            }
            setupComplete = true;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log($"{gameObject.name}.MainMapIndicatorController.SetInteractable(" + interactable.gameObject.name + "): instance: " + instanceNumber);
            this.interactable = interactable;
            SetupMainMap();
        }

        public void HandleMainMapStatusUpdate(InteractableOptionComponent _interactable) {
            //Debug.Log(_interactable.Interactable.gameObject.name + ".MainMapIndicatorController.HandleMainMapStatusUpdate()");
            if (mainMapLayers.ContainsKey(_interactable) == false ||  mainMapLayers[_interactable] == null) {
                //Debug.Log(_interactable.DisplayName + ".MainMapIndicatorController.HandleMainMapStatusUpdate(): mainMapLayers[_interactable] is null! Exiting");
                return;
            }
            // this only supports one or the other too - prioritizing images
            if (_interactable.HasMainMapIcon()) {
                _interactable.SetMiniMapIcon(mainMapLayers[_interactable].GetComponent<Image>());
            } else if (_interactable.HasMainMapText()) {
                _interactable.SetMiniMapText(mainMapLayers[_interactable].GetComponent<TextMeshProUGUI>());
            }
        }

        public void ResetSettings() {
            foreach (GameObject go in mainMapLayers.Values) {
                objectPooler.ReturnObjectToPool(go);
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