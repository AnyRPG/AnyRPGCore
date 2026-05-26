using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class HandScript : ConfiguredMonoBehaviour {

        //public IMoveable Moveable { get; set; }
        public IMoveableOwner MoveableOwner { get; set; }

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        [SerializeField]
        private RectTransform rectTransform = null;

        [SerializeField]
        private Image backgroundImage = null;

        [SerializeField]
        private Image icon = null;

        // game manager references
        private UIManager uIManager = null;
        private ActionBarManager actionBarManager = null;
        private InputManager inputManager = null;
        private PlayerManagerClient playerManagerClient = null;
        private ControlsManager controlsManager = null;


        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            uIManager = systemGameManager.UIManager;
            actionBarManager = uIManager.ActionBarManager;
            inputManager = systemGameManager.InputManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
            controlsManager = systemGameManager.ControlsManager;
        }

        public void SetPosition(Vector3 position) {
            transform.position = position;
        }

        // called once per frame
        public void ProcessInput() {
            //Debug.Log("HandScript.ProcessInput()");

            if (controlsManager.GamepadModeActive == false) {
                transform.position = Input.mousePosition + offset;
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && MoveableOwner?.Moveable != null) {
                    if (MoveableOwner.Moveable is InstantiatedItem) {
                        uIManager.confirmDestroyMenuWindow.OpenWindow();
                    } else if (MoveableOwner.Moveable is Ability) {
                        // DROP ABILITY SAFELY
                        if (actionBarManager.FromButton != null) {
                            //actionBarManager.FromButton.ClearUseable();
                            actionBarManager.RequestClearMouseUseable(actionBarManager.FromButton.ActionButtonIndex);
                        }
                        CompleteMove();
                    }
                }
            }
            if (inputManager.KeyBindWasPressed("CANCELALL")) {
                CancelMove();
            }
        }

        public void TakeMoveable(IMoveableOwner moveableOwner) {
            //Debug.Log($"HandScript.TakeMoveable({moveable.DisplayName})");

            this.MoveableOwner = moveableOwner;

            moveableOwner.Moveable.AssignToHandScript(backgroundImage);

            icon.sprite = moveableOwner.Moveable.Icon;
            icon.color = Color.white;


            if (controlsManager.GamepadModeActive == true) {
                rectTransform.pivot = new Vector2(0, 1);
            } else {
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

        }

        public void CancelMove() {
            //Debug.Log("HandScript.CancelMove()");

            if (MoveableOwner == null) {
                return;
            }
            MoveableOwner.CancelHandscriptMove();
            ClearMoveable();
        }

        public void CompleteMove() {
            //Debug.Log("HandScript.CompleteMove()");

            ClearMoveable();
        }

        private void ClearMoveable() {
            //Debug.Log("HandScript.ClearMoveable()");

            playerManagerClient.UnitController.CharacterInventoryManager.FromSlot = null;
            actionBarManager.FromButton = null;
            MoveableOwner = null;

            // clear background image
            backgroundImage.color = new Color32(0, 0, 0, 0);
            backgroundImage.sprite = null;

            // clear icon
            icon.sprite = null;
            icon.color = new Color(0, 0, 0, 0);

            /*
            if (controlsManager.GamePadModeActive == true) {
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            */
        }

        public void DeleteItem() {
            //Debug.Log("HandScript.DeleteItem()");
            if (MoveableOwner?.Moveable is InstantiatedItem) {
                playerManagerClient.UnitController.CharacterInventoryManager.RequestDeleteItem((InstantiatedItem)MoveableOwner.Moveable);
            }
            CancelMove();
        }
    }

}