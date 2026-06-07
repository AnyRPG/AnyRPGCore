using UnityEngine;

namespace AnyRPG {
    public class SystemKeyBindPanel : WindowPanel {

        //public override event Action<ICloseableWindowContents> OnOpenWindow;

        [SerializeField]
        private GameObject movementKeyParent = null;

        [SerializeField]
        private GameObject actionBarsKeyParent = null;

        [SerializeField]
        private GameObject systemKeyParent = null;

        [SerializeField]
        private GameObject keyBindButtonPrefab = null;

        [SerializeField]
        private UINavigationController navigationMenuController = null;

        [SerializeField]
        private UINavigationController movementPanelController = null;

        [SerializeField]
        private UINavigationController actionBarsPanelController = null;

        [SerializeField]
        private UINavigationController systemPanelController = null;

        [Header("Panels")]
        [Tooltip("The UI Sub-Panel under KEY BINDINGS for MOVEMENT")]
        public GameObject PanelMovement = null;

        [Tooltip("The UI Sub-Panel under KEY BINDINGS for COMBAT")]
        public GameObject PanelCombat = null;

        [Tooltip("The UI Sub-Panel under KEY BINDINGS for GENERAL")]
        public GameObject PanelGeneral = null;

        [Header("Buttons")]
        public HighlightButton movementButton = null;
        public HighlightButton actionBarsButton = null;
        public HighlightButton systemButton = null;

        // game manager references
        KeyBindManager keyBindManager = null;
        ObjectPooler objectPooler = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            InitializeKeys();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            keyBindManager = systemGameManager.KeyBindManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        /*
        public void OnEnable() {
            ToggleMovementPanel();
        }
        */

        private void InitializeKeys() {
            //Debug.Log("KeyBindMenuController.InitializeKeys()");
            foreach (InputActionNode inputActionNode in inputManager.InputActionNodes.Values) {
                Transform nodeParent = null;
                UINavigationController uINavigationController = null;
                if (inputActionNode.KeyBindType == KeyBindType.Action) {
                    nodeParent = actionBarsKeyParent.transform;
                    uINavigationController = actionBarsPanelController;
                } else if (inputActionNode.KeyBindType == KeyBindType.Normal) {
                    nodeParent = movementKeyParent.transform;
                    uINavigationController = movementPanelController;
                } else if (inputActionNode.KeyBindType == KeyBindType.Constant || inputActionNode.KeyBindType == KeyBindType.System) {
                    nodeParent = systemKeyParent.transform;
                    uINavigationController = systemPanelController;
                }
                if (nodeParent != null) {
                    KeyBindSlotScript keyBindSlotScript = objectPooler.GetPooledObject(keyBindButtonPrefab, nodeParent).GetComponent<KeyBindSlotScript>();
                    keyBindSlotScript.Configure(systemGameManager);
                    keyBindSlotScript.Initialize(inputActionNode);
                    inputActionNode.SetSlotScript(keyBindSlotScript);
                    uINavigationController.AddActiveButton(keyBindSlotScript.KeyboardAssignButton);
                }
            }
        }

        public void ResetPanels() {
            // turn off all panels
            PanelMovement.gameObject.SetActive(false);
            PanelCombat.gameObject.SetActive(false);
            PanelGeneral.gameObject.SetActive(false);

        }

        /*
        public void ResetButtons() {
            movementButton.DeSelect();
            actionBarsButton.DeSelect();
            systemButton.DeSelect();
        }
        */

        public void ToggleMovementPanel() {
            //Debug.Log("SystemKeyBindPanelController.ToggleMovementPanel()");
            ResetPanels();
            PanelMovement.gameObject.SetActive(true);

            navigationMenuController.UnHightlightButtonBackgrounds(movementButton);

            movementButton.HighlightBackground();
            movementButton.Select();
        }

        public void ToggleActionBarsPanel() {
            ResetPanels();
            PanelCombat.gameObject.SetActive(true);

            navigationMenuController.UnHightlightButtonBackgrounds(actionBarsButton);

            actionBarsButton.HighlightBackground();
            actionBarsButton.Select();
        }

        public void ToggleSystemPanel() {
            ResetPanels();
            PanelGeneral.gameObject.SetActive(true);

            navigationMenuController.UnHightlightButtonBackgrounds(systemButton);

            systemButton.HighlightBackground();
            systemButton.Select();
        }

        public void ResetToDefaults() {
            inputManager.ResetToDefault();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("SystemKeyBindPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            
            ToggleMovementPanel();
            navigationMenuController.UnFocus();
        }


    }

}