using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class DraggableWindow : ConfiguredMonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler {

        [Header("Draggable Window")]

        [SerializeField]
        protected Transform moveableTransform = null;

        [Tooltip("This may be different than the moveable transform.  It is used for saving and restoring the window position")]
        [SerializeField]
        protected RectTransform rectTransform = null;

        [SerializeField]
        protected string dragString = string.Empty;

        [SerializeField]
        protected string gamepadDragString = string.Empty;

        [SerializeField]
        protected TextMeshProUGUI dragText = null;

        [SerializeField]
        protected bool alwaysDraggable = false;

        [SerializeField]
        private bool neverDraggable = false;

        protected Vector2 startMousePosition, startWindowPosition;

        protected bool uiLocked = true;

        // game manager references
        protected UIManager uIManager = null;
        protected SaveManager saveManager = null;
        protected ControlsManager controlsManager = null;

        public RectTransform RectTransform { get => rectTransform; }
        public bool UiLocked { get => uiLocked; }
        public bool NeverDraggable { get => neverDraggable;  }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            if (moveableTransform == null) {
                //Debug.Log(gameObject.name + "DraggableWindow.Awake(): moveableTransform was null, setting to self");
                moveableTransform = transform;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
            controlsManager = systemGameManager.ControlsManager;
        }



        public void OnBeginDrag(PointerEventData eventData) {
            //Debug.Log("FramedWindow.OnBeginDrag()");
            if (neverDraggable) {
                return;
            }
            startMousePosition = eventData.position;
            startWindowPosition = moveableTransform.position;
            uIManager.DragInProgress = true;
        }

        public void OnDrag(PointerEventData eventData) {
            //Debug.Log("DraggableWindow.OnDrag()");
            if (neverDraggable) {
                return;
            }
            if (uiLocked == true && alwaysDraggable == false) {
                return;
            }
            moveableTransform.position = (eventData.position - startMousePosition) + startWindowPosition;
        }

        public virtual void LockUI() {
            //Debug.Log(gameObject.name + ".DraggableWindow.LockUI()");
            if (!PlayerPrefs.HasKey("LockUI")) {
                return;
            }
            if (neverDraggable) {
                return;
            }
            if (PlayerPrefs.GetInt("LockUI") == 0) {
                //Debug.Log("DraggableWindow.LockUI(): UI is unlocked");
                uiLocked = false;
                if (dragString != null && dragString != string.Empty && dragText != null) {
                    dragText.gameObject.SetActive(true);
                    dragText.raycastTarget = true;
                    if (controlsManager.GamePadModeActive == true) {
                        dragText.text = gamepadDragString;
                    } else {
                        dragText.text = dragString;
                    }
                }
            } else {
                uiLocked = true;
                if (dragText != null) {
                    dragText.text = "";
                    dragText.raycastTarget = false;
                    dragText.gameObject.SetActive(false);
                }
            }
        }

        public virtual void OnEnable() {
            LockUI();
        }

        public virtual void OnDisable() {
            // overwrite me
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (neverDraggable) {
                return;
            }
            uIManager.DragInProgress = false;
            saveManager.SaveWindowPositions();
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (neverDraggable) {
                return;
            }
            uIManager.DragInProgress = true;
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (neverDraggable) {
                return;
            }
            uIManager.DragInProgress = false;
        }

        public void LeftAnalog(float inputHorizontal, float inputVertical) {
            if (neverDraggable) {
                return;
            }
            if (uiLocked == true && alwaysDraggable == false) {
                return;
            }
            moveableTransform.position += new Vector3(inputHorizontal, inputVertical, 0f);
            saveManager.SaveWindowPositions();
            uIManager.RefreshGamepadToolTip();
        }
    }

}