using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class MainMapIndicatorController : MonoBehaviour {

        [SerializeField]
        private GameObject mainMapTextLayerPrefab = null;

        [SerializeField]
        private GameObject mainMapImageLayerPrefab = null;

        [SerializeField]
        private Transform contentParent = null;

        private Interactable interactable = null;

        private RectTransform rectTransform = null;

        private Vector2 uiOffset = Vector2.zero;

        private Dictionary<InteractableOptionComponent, GameObject> mainMapLayers = new Dictionary<InteractableOptionComponent, GameObject>();

        private bool setupComplete = false;

        private void Awake() {
            //Debug.Log("MainMapIndicatorController.Awake()");
            rectTransform = GetComponent<RectTransform>();
            //canvas.worldCamera = CameraManager.MyInstance.MainMapCamera;
            //canvas.planeDistance = 1f;
            uiOffset = new Vector2((float)rectTransform.sizeDelta.x / 2f, (float)rectTransform.sizeDelta.y / 2f);
            //Debug.Log("MainMapIndicatorController.Awake(): rectTransform.sizeDelta: " + rectTransform.sizeDelta + "; uiOffset" + uiOffset);
        }

        public void SetupMainMap() {
            //Debug.Log(transform.parent.gameObject.name + ".MainMapIndicatorController.SetupMainMap(): interactable: " + (interactable == null ? "null" : interactable.name));
            if (setupComplete == true) {
                return;
            }
            if (interactable == null) {
                return;
            }
            foreach (InteractableOptionComponent _interactable in interactable.Interactables) {
                // prioritize images - DICTIONARY DOESN'T CURRENTLY SUPPORT BOTH
                if (_interactable.HasMainMapIcon()) {
                    //else if (_interactable.HasMainMapIcon()) {
                    // do both now!
                    GameObject go = ObjectPooler.MyInstance.GetPooledObject(mainMapImageLayerPrefab, contentParent);
                    Image _image = go.GetComponent<Image>();
                    _interactable.SetMiniMapIcon(_image);
                    mainMapLayers.Add(_interactable, go);
                } else if (_interactable.HasMainMapText()) {
                    GameObject go = ObjectPooler.MyInstance.GetPooledObject(mainMapTextLayerPrefab, contentParent);
                    TextMeshProUGUI _text = go.GetComponent<TextMeshProUGUI>();
                    _interactable.SetMiniMapText(_text);
                    mainMapLayers.Add(_interactable, go);
                }
            }
            setupComplete = true;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log(gameObject.name + ".MainMapIndicatorController.SetInteractable(" + interactable.gameObject.name + "): instance: " + instanceNumber);
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
            //if (_interactable.GetCurrentOptionCount() > 0) {
            if (_interactable.HasMainMapIcon()) {
                //Debug.Log(_interactable.DisplayName + ".MainMapIndicatorController.HandleMainMapStatusUpdate() : hasicon");
                _interactable.SetMiniMapIcon(mainMapLayers[_interactable].GetComponent<Image>());
            } else if (_interactable.HasMainMapText()) {
                //Debug.Log(_interactable.MyName + ".MainMapIndicatorController.HandleMainMapStatusUpdate() : hastext");
                _interactable.SetMiniMapText(mainMapLayers[_interactable].GetComponent<TextMeshProUGUI>());
            }
            //}
        }

        public void CleanupLayers() {
            foreach (GameObject go in mainMapLayers.Values) {
                ObjectPooler.MyInstance.ReturnObjectToPool(go);
            }
            mainMapLayers.Clear();
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