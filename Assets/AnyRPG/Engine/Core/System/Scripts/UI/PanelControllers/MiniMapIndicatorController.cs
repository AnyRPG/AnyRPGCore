using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class MiniMapIndicatorController : MonoBehaviour {

        [SerializeField]
        private GameObject miniMapTextLayerPrefab = null;

        [SerializeField]
        private GameObject miniMapImageLayerPrefab = null;

        [SerializeField]
        private Transform contentParent = null;

        private Interactable interactable = null;

        private Canvas canvas = null;

        private RectTransform rectTransform = null;

        private Vector2 uiOffset = Vector2.zero;

        private Dictionary<InteractableOptionComponent, GameObject> miniMapLayers = new Dictionary<InteractableOptionComponent, GameObject>();

        private bool setupComplete = false;

        private void Awake() {
            //Debug.Log("MiniMapIndicatorController.Awake()");
            canvas = GetComponent<Canvas>();
            rectTransform = GetComponent<RectTransform>();
            //canvas.worldCamera = CameraManager.MyInstance.MiniMapCamera;
            //canvas.planeDistance = 1f;
            uiOffset = new Vector2((float)rectTransform.sizeDelta.x / 2f, (float)rectTransform.sizeDelta.y / 2f);
            //Debug.Log("MiniMapIndicatorController.Awake(): rectTransform.sizeDelta: " + rectTransform.sizeDelta + "; uiOffset" + uiOffset);
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("MiniMapIndicatorController.CleanupEventSubscriptions()");
            foreach (InteractableOptionComponent _interactable in interactable.Interactables) {
                if (_interactable.HasMiniMapIcon() || _interactable.HasMiniMapText()) {
                    _interactable.MiniMapStatusUpdateHandler -= HandleMiniMapStatusUpdate;
                }
            }
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void SetupMiniMap() {
            //Debug.Log(transform.parent.gameObject.name + ".MiniMapIndicatorController.SetupMiniMap(): interactable: " + (interactable == null ? "null" : interactable.name));
            if (setupComplete == true) {
                return;
            }
            if (interactable == null) {
                //Debug.Log(".MiniMapIndicatorController.Start(): interactable is null");
                return;
            }
            foreach (InteractableOptionComponent _interactable in interactable.Interactables) {
                // prioritize images - DICTIONARY DOESN'T CURRENTLY SUPPORT BOTH
                if (_interactable.HasMiniMapIcon()) {
                    //else if (_interactable.HasMiniMapIcon()) {
                    // do both now!
                    //Debug.Log(interactable.MyName + ".MiniMapIndicatorController.Start(): interactable has minimapicon");
                    GameObject go = Instantiate(miniMapImageLayerPrefab, contentParent);
                    Image _image = go.GetComponent<Image>();
                    _interactable.SetMiniMapIcon(_image);
                    miniMapLayers.Add(_interactable, go);
                } else if (_interactable.HasMiniMapText()) {
                    //Debug.Log(transform.parent.gameObject.name + ".MiniMapIndicatorController.Start(): interactable has minimaptext");
                    GameObject go = Instantiate(miniMapTextLayerPrefab, contentParent);
                    TextMeshProUGUI _text = go.GetComponent<TextMeshProUGUI>();
                    _interactable.SetMiniMapText(_text);
                    miniMapLayers.Add(_interactable, go);
                }
                if (_interactable.HasMiniMapIcon() || _interactable.HasMiniMapText()) {
                    //Debug.Log(interactable.name + ".MiniMapIndicatorController.SetupMiniMap(): adding minimap status handler");
                    _interactable.MiniMapStatusUpdateHandler += HandleMiniMapStatusUpdate;
                } else {
                    //Debug.Log(interactable.name + ".MiniMapIndicatorController.SetupMiniMap(): unit had no icon or text, not setting up status handler");
                }
            }
            setupComplete = true;
        }

        public void SetInteractable(Interactable interactable) {
            //Debug.Log(gameObject.name + ".MiniMapIndicatorController.SetInteractable(" + interactable.gameObject.name + "): instance: " + instanceNumber);
            this.interactable = interactable;
            interactable.OnInteractableDestroy += HandleInteractableDestroy;
            SetupMiniMap();
        }

        public void HandleInteractableDestroy() {
            MiniMapController.MyInstance.RemoveIndicator(interactable);
        }

        // testing : not in use.  updated directly by minimap controller instead
        /*
        public void UpdatePosition() {
            //Debug.Log("MiniMapIndicatorController.LateUpdate(): interactable: " + (interactable == null ? "null" : (interactable.MyName == string.Empty ? interactable.name : interactable.MyName)) );
            if (setupComplete == false) {
                //Debug.Log("MiniMapIndicatorController.LateUpdate(): namePlateUnit: " + (interactable == null ? "null" : interactable.MyName) + ": setup has not completed yet!");
                return;
            }

            Vector2 viewportPosition = CameraManager.MyInstance.MiniMapCamera.WorldToViewportPoint(interactable.gameObject.transform.position);
            Vector2 proportionalPosition = new Vector2(viewportPosition.x * rectTransform.sizeDelta.x, viewportPosition.y * rectTransform.sizeDelta.y);
            //Debug.Log(interactable.gameObject.name + ".MiniMapIndicatorController.LateUpdate(). interactable position: " + interactable.gameObject.transform.position + "; viewportPosition: " + viewportPosition + "; proportionalPosition: " + proportionalPosition);
            contentParent.localPosition = proportionalPosition - uiOffset;
        }
        */

        public void HandleMiniMapStatusUpdate(InteractableOptionComponent _interactable) {
            //Debug.Log(_interactable.Interactable.gameObject.name + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate()");
            if (miniMapLayers[_interactable] == null) {
                //Debug.Log(_interactable.DisplayName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate(): miniMapLayers[_interactable] is null! Exiting");
                return;
            }
            // this only supports one or the other too - prioritizing images
            //if (_interactable.GetCurrentOptionCount() > 0) {
            if (_interactable.HasMiniMapIcon()) {
                //Debug.Log(_interactable.DisplayName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate() : hasicon");
                _interactable.SetMiniMapIcon(miniMapLayers[_interactable].GetComponent<Image>());
            } else if (_interactable.HasMiniMapText()) {
                //Debug.Log(_interactable.MyName + ".MiniMapIndicatorController.HandleMiniMapStatusUpdate() : hastext");
                _interactable.SetMiniMapText(miniMapLayers[_interactable].GetComponent<TextMeshProUGUI>());
            }
            //}
        }

        private void OnDestroy() {
            //Debug.Log(gameObject.name + ".MiniMapIndicatorController.OnDestroy(): interactable: " + interactable.gameObject.name);
            if (interactable != null) {
                interactable.OnInteractableDestroy -= HandleInteractableDestroy;
            }
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