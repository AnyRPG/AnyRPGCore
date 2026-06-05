using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace AnyRPG {

    public class InputManager : ConfiguredMonoBehaviour {

        private Mouse mouse = null;
        private Gamepad gamepad = null;
        private Dictionary<string, InputActionNode> inputActionNodes = new Dictionary<string, InputActionNode>();

        [SerializeField]
        private InputActionAsset inputActions;

        private InputAction forwardAction;
        private InputAction backAction;
        private InputAction strafeLeftAction;
        private InputAction strafeRightAction;
        private InputAction turnLeftAction;
        private InputAction turnRightAction;
        private InputAction jumpAction;
        private InputAction crouchAction;
        private InputAction rollAction;
        private InputAction toggleRunAction;
        private InputAction toggleAutoRunAction;
        private InputAction toggleStrafeAction;
        private InputAction toggleMouseLookAction;
        private InputAction acceptAction;
        private InputAction cancelAllAction;
        private InputAction mainMenuAction;
        private InputAction beginChatCommandAction;
        private InputAction gamepadButtonAAction;
        private InputAction gamepadButtonBAction;
        private InputAction gamepadButtonXAction;
        private InputAction gamepadButtonYAction;
        private InputAction gamepadButtonLeftShoulderAction;
        private InputAction gamepadButtonRightShoulderAction;
        private InputAction gamepadButtonSelectAction;
        private InputAction gamepadButtonStartAction;
        private InputAction gamepadButtonLeftStickAction;
        private InputAction gamepadButtonRightStickAction;
        private InputAction questLogAction;
        private InputAction characterPanelAction;
        private InputAction currencyPanelAction;
        private InputAction abilityBookAction;
        private InputAction skillBookAction;
        private InputAction achievementBookAction;
        private InputAction reputationBookAction;
        private InputAction inventoryAction;
        private InputAction socialAction;
        private InputAction mainMapAction;
        private InputAction nextTargetAction;
        private InputAction unequipAllAction;
        private InputAction hideUIAction;
        private InputAction actionButton1Action;
        private InputAction actionButton2Action;
        private InputAction actionButton3Action;
        private InputAction actionButton4Action;
        private InputAction actionButton5Action;
        private InputAction actionButton6Action;
        private InputAction actionButton7Action;
        private InputAction actionButton8Action;
        private InputAction actionButton9Action;
        private InputAction actionButton10Action;
        private InputAction actionButton11Action;
        private InputAction actionButton12Action;
        private InputAction actionButton13Action;
        private InputAction actionButton14Action;
        private InputAction actionButton15Action;
        private InputAction actionButton16Action;
        private InputAction actionButton17Action;
        private InputAction actionButton18Action;
        private InputAction actionButton19Action;
        private InputAction actionButton20Action;
        private InputAction actionButton21Action;
        private InputAction actionButton22Action;
        private InputAction actionButton23Action;
        private InputAction actionButton24Action;
        private InputAction actionButton25Action;
        private InputAction actionButton26Action;
        private InputAction actionButton27Action;
        private InputAction actionButton28Action;
        private InputAction actionButton29Action;
        private InputAction actionButton30Action;
        private InputAction actionButton31Action;
        private InputAction actionButton32Action;
        private InputAction actionButton33Action;
        private InputAction actionButton34Action;
        private InputAction actionButton35Action;
        private InputAction actionButton36Action;
        private InputAction actionButton37Action;
        private InputAction actionButton38Action;
        private InputAction actionButton39Action;
        private InputAction actionButton40Action;
        private InputAction actionButton41Action;
        private InputAction actionButton42Action;
        private InputAction actionButton43Action;
        private InputAction actionButton44Action;
        private InputAction actionButton45Action;
        private InputAction actionButton46Action;
        private InputAction actionButton47Action;
        private InputAction actionButton48Action;
        private InputAction actionButton49Action;
        private InputAction actionButton50Action;
        private InputAction actionButton51Action;
        private InputAction actionButton52Action;
        private InputAction actionButton53Action;
        private InputAction actionButton54Action;
        private InputAction actionButton55Action;
        private InputAction actionButton56Action;
        private InputAction actionButton57Action;
        private InputAction actionButton58Action;
        private InputAction actionButton59Action;
        private InputAction actionButton60Action;
        private InputAction actionButton61Action;
        private InputAction actionButton62Action;
        private InputAction actionButton63Action;
        private InputAction actionButton64Action;
        private InputAction actionButton65Action;
        private InputAction actionButton66Action;
        private InputAction actionButton67Action;
        private InputAction actionButton68Action;
        private InputAction actionButton69Action;
        private InputAction actionButton70Action;
        private InputAction actionButton71Action;
        private InputAction actionButton72Action;
        private InputAction actionButton73Action;
        private InputAction actionButton74Action;
        private InputAction actionButton75Action;
        private InputAction actionButton76Action;
        private InputAction actionButton77Action;
        private InputAction actionButton78Action;
        private InputAction actionButton79Action;
        private InputAction actionButton80Action;
        private InputAction actionButton81Action;
        private InputAction actionButton82Action;
        private InputAction actionButton83Action;
        private InputAction actionButton84Action;

        public bool rightMouseButtonDown = false;
        public bool middleMouseButtonDown = false;
        public bool rightMouseButtonUp = false;
        public bool middleMouseButtonUp = false;
        //public bool rightMouseButtonStayDown = false;
        public bool rightMouseButtonClicked = false;
        public bool middleMouseButtonClicked = false;
        public bool rightMouseButtonClickedOverUI = false;
        public bool middleMouseButtonClickedOverUI = false;
        public Vector3 rightMouseButtonDownPosition = Vector3.zero;
        public Vector3 rightMouseButtonUpPosition = Vector3.zero;
        public Vector3 middleMouseButtonDownPosition = Vector3.zero;
        public Vector3 middleMouseButtonUpPosition = Vector3.zero;
        public bool leftMouseButtonDown = false;
        public bool leftMouseButtonUp = false;
        //public bool leftMouseButtonStayDown = false;
        public bool leftMouseButtonClicked = false;
        public bool leftMouseButtonClickedOverUI = false;
        public Vector3 leftMouseButtonDownPosition = Vector3.zero;
        public Vector3 leftMouseButtonUpPosition = Vector3.zero;
        public bool mouseScrolled = false;
        public float mouseScrollDeltaY = 0f;
        public float mouseDeltaX = 0f;
        public float mouseDeltaY = 0f;
        public Vector3 mousePosition = Vector3.zero;

        public float dPadHorizontal = 0f;
        public float dPadVertical = 0f;
        public bool dPadDown = false;
        public bool dPadDownPressed = false;
        public bool dPadUp = false;
        public bool dPadUpPressed = false;
        public bool dPadLeft = false;
        public bool dPadLeftPressed = false;
        public bool dPadRight = false;
        public bool dPadRightPressed = false;

        public float leftTriggerAxis = 0f;
        public float rightTriggerAxis = 0f;
        public bool leftTriggerDown = false;
        public bool leftTriggerUp = false;
        public bool leftTriggerPressed = false;
        public bool rightTriggerDown = false;
        public bool rightTriggerUp = false;
        public bool rightTriggerPressed = false;

        public float rightAnalogVertical = 0f;
        public float rightAnalogHorizontal = 0f;
        public float leftAnalogVertical = 0f;
        public float leftAnalogHorizontal = 0f;

        private int lastRegisteredFrame = 0;

        // game manager references
        private KeyBindManager keyBindManager = null;
        private UIManager uIManager = null;
        private NameplateManager namePlateManager = null;
        private LevelManagerClient levelManagerClient = null;

        public InputAction ForwardAction { get => forwardAction; }
        public InputAction BackAction { get => backAction; }
        public InputAction StrafeLeftAction { get => strafeLeftAction; }
        public InputAction StrafeRightAction { get => strafeRightAction; }
        public InputAction TurnLeftAction { get => turnLeftAction; }
        public InputAction TurnRightAction { get => turnRightAction; }
        public InputAction JumpAction { get => jumpAction; }
        public InputAction CrouchAction { get => crouchAction; }
        public InputAction RollAction { get => rollAction; }
        public InputAction ToggleRunAction { get => toggleRunAction; }
        public InputAction ToggleAutoRunAction { get => toggleAutoRunAction; }
        public InputAction ToggleStrafeAction { get => toggleStrafeAction; }
        public InputAction ToggleMouseLookAction { get => toggleMouseLookAction; }
        public InputAction AcceptAction { get => acceptAction; }
        public InputAction CancelAllAction { get => cancelAllAction; }
        public InputAction MainMenuAction { get => mainMenuAction; }
        public InputAction BeginChatCommandAction { get => beginChatCommandAction; }
        public InputAction GamepadButtonAAction { get => gamepadButtonAAction; }
        public InputAction GamepadButtonBAction { get => gamepadButtonBAction; }
        public InputAction GamepadButtonXAction { get => gamepadButtonXAction; }
        public InputAction GamepadButtonYAction { get => gamepadButtonYAction; }
        public InputAction GamepadButtonLeftShoulderAction { get => gamepadButtonLeftShoulderAction; }
        public InputAction GamepadButtonRightShoulderAction { get => gamepadButtonRightShoulderAction; }
        public InputAction GamepadButtonSelectAction { get => gamepadButtonSelectAction; }
        public InputAction GamepadButtonStartAction { get => gamepadButtonStartAction; }
        public InputAction GamepadButtonLeftStickAction { get => gamepadButtonLeftStickAction; }
        public InputAction GamepadButtonRightStickAction { get => gamepadButtonRightStickAction; }
        public InputAction QuestLogAction { get => questLogAction; }
        public InputAction CharacterPanelAction { get => characterPanelAction; }
        public InputAction CurrencyPanelAction { get => currencyPanelAction; }
        public InputAction AbilityBookAction { get => abilityBookAction; }
        public InputAction SkillBookAction { get => skillBookAction; }
        public InputAction AchievementBookAction { get => achievementBookAction; }
        public InputAction ReputationBookAction { get => reputationBookAction; }
        public InputAction InventoryAction { get => inventoryAction; }
        public InputAction SocialAction { get => socialAction; }
        public InputAction MainMapAction { get => mainMapAction; }
        public InputAction NextTargetAction { get => nextTargetAction; }
        public InputAction UnequipAllAction { get => unequipAllAction; }
        public InputAction HideUIAction { get => hideUIAction; }
        public InputAction ActionButton1Action { get => actionButton1Action; }
        public InputAction ActionButton2Action { get => actionButton2Action; }
        public InputAction ActionButton3Action { get => actionButton3Action; }
        public InputAction ActionButton4Action { get => actionButton4Action; }
        public InputAction ActionButton5Action { get => actionButton5Action; }
        public InputAction ActionButton6Action { get => actionButton6Action; }
        public InputAction ActionButton7Action { get => actionButton7Action; }
        public InputAction ActionButton8Action { get => actionButton8Action; }
        public InputAction ActionButton9Action { get => actionButton9Action; }
        public InputAction ActionButton10Action { get => actionButton10Action; }
        public InputAction ActionButton11Action { get => actionButton11Action; }
        public InputAction ActionButton12Action { get => actionButton12Action; }
        public InputAction ActionButton13Action { get => actionButton13Action; }
        public InputAction ActionButton14Action { get => actionButton14Action; }
        public InputAction ActionButton15Action { get => actionButton15Action; }
        public InputAction ActionButton16Action { get => actionButton16Action; }
        public InputAction ActionButton17Action { get => actionButton17Action; }
        public InputAction ActionButton18Action { get => actionButton18Action; }
        public InputAction ActionButton19Action { get => actionButton19Action; }
        public InputAction ActionButton20Action { get => actionButton20Action; }
        public InputAction ActionButton21Action { get => actionButton21Action; }
        public InputAction ActionButton22Action { get => actionButton22Action; }
        public InputAction ActionButton23Action { get => actionButton23Action; }
        public InputAction ActionButton24Action { get => actionButton24Action; }
        public InputAction ActionButton25Action { get => actionButton25Action; }
        public InputAction ActionButton26Action { get => actionButton26Action; }
        public InputAction ActionButton27Action { get => actionButton27Action; }
        public InputAction ActionButton28Action { get => actionButton28Action; }
        public InputAction ActionButton29Action { get => actionButton29Action; }
        public InputAction ActionButton30Action { get => actionButton30Action; }
        public InputAction ActionButton31Action { get => actionButton31Action; }
        public InputAction ActionButton32Action { get => actionButton32Action; }
        public InputAction ActionButton33Action { get => actionButton33Action; }
        public InputAction ActionButton34Action { get => actionButton34Action; }
        public InputAction ActionButton35Action { get => actionButton35Action; }
        public InputAction ActionButton36Action { get => actionButton36Action; }
        public InputAction ActionButton37Action { get => actionButton37Action; }
        public InputAction ActionButton38Action { get => actionButton38Action; }
        public InputAction ActionButton39Action { get => actionButton39Action; }
        public InputAction ActionButton40Action { get => actionButton40Action; }
        public InputAction ActionButton41Action { get => actionButton41Action; }
        public InputAction ActionButton42Action { get => actionButton42Action; }
        public InputAction ActionButton43Action { get => actionButton43Action; }
        public InputAction ActionButton44Action { get => actionButton44Action; }
        public InputAction ActionButton45Action { get => actionButton45Action; }
        public InputAction ActionButton46Action { get => actionButton46Action; }
        public InputAction ActionButton47Action { get => actionButton47Action; }
        public InputAction ActionButton48Action { get => actionButton48Action; }
        public InputAction ActionButton49Action { get => actionButton49Action; }
        public InputAction ActionButton50Action { get => actionButton50Action; }
        public InputAction ActionButton51Action { get => actionButton51Action; }
        public InputAction ActionButton52Action { get => actionButton52Action; }
        public InputAction ActionButton53Action { get => actionButton53Action; }
        public InputAction ActionButton54Action { get => actionButton54Action; }
        public InputAction ActionButton55Action { get => actionButton55Action; }
        public InputAction ActionButton56Action { get => actionButton56Action; }
        public InputAction ActionButton57Action { get => actionButton57Action; }
        public InputAction ActionButton58Action { get => actionButton58Action; }
        public InputAction ActionButton59Action { get => actionButton59Action; }
        public InputAction ActionButton60Action { get => actionButton60Action; }
        public InputAction ActionButton61Action { get => actionButton61Action; }
        public InputAction ActionButton62Action { get => actionButton62Action; }
        public InputAction ActionButton63Action { get => actionButton63Action; }
        public InputAction ActionButton64Action { get => actionButton64Action; }
        public InputAction ActionButton65Action { get => actionButton65Action; }
        public InputAction ActionButton66Action { get => actionButton66Action; }
        public InputAction ActionButton67Action { get => actionButton67Action; }
        public InputAction ActionButton68Action { get => actionButton68Action; }
        public InputAction ActionButton69Action { get => actionButton69Action; }
        public InputAction ActionButton70Action { get => actionButton70Action; }
        public InputAction ActionButton71Action { get => actionButton71Action; }
        public InputAction ActionButton72Action { get => actionButton72Action; }
        public InputAction ActionButton73Action { get => actionButton73Action; }
        public InputAction ActionButton74Action { get => actionButton74Action; }
        public InputAction ActionButton75Action { get => actionButton75Action; }
        public InputAction ActionButton76Action { get => actionButton76Action; }
        public InputAction ActionButton77Action { get => actionButton77Action; }
        public InputAction ActionButton78Action { get => actionButton78Action; }
        public InputAction ActionButton79Action { get => actionButton79Action; }
        public InputAction ActionButton80Action { get => actionButton80Action; }
        public InputAction ActionButton81Action { get => actionButton81Action; }
        public InputAction ActionButton82Action { get => actionButton82Action; }
        public InputAction ActionButton83Action { get => actionButton83Action; }
        public InputAction ActionButton84Action { get => actionButton84Action; }
        public Dictionary<string, InputActionNode> InputActionNodes { get => inputActionNodes; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            levelManagerClient.OnLevelLoad += HandleLevelLoad;
            InitializeKeys();
            LoadPlayerKeyControls();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            keyBindManager = systemGameManager.KeyBindManager;
            uIManager = systemGameManager.UIManager;
            namePlateManager = uIManager.NameplateManager;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        private void LoadPlayerKeyControls() {
            Debug.Log("InputManager.LoadPlayerKeyControls()");

            if (PlayerPrefs.HasKey("InputActionOverrides")) {
                inputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString("InputActionOverrides"));
                
                // loop through input action nodes and call their ProcessUpdateAction() method to update their display strings based on the new overrides we just loaded
                foreach (InputActionNode inputActionNode in inputActionNodes.Values) {
                    inputActionNode.ProcessUpdateAction();
                }
            }
        }

        public void SavePlayerKeyControls() {
            Debug.Log("InputManager.SavePlayerKeyControls()");

            PlayerPrefs.SetString("InputActionOverrides", inputActions.SaveBindingOverridesAsJson());
        }

        public void UpdateInputAction(string actionName, bool control, bool shift, string nativePath) {
            Debug.Log($"InputManager.UpdateInputAction(actionName: {actionName}, nativePath: {nativePath}, Ctrl: {control}, Shift: {shift})");

            if (inputActionNodes.TryGetValue(actionName, out InputActionNode inputActionNode)) {
                inputActionNode.UpdateInputAction(control, shift, nativePath);
                SavePlayerKeyControls();
            } else {
                Debug.LogError("InputManager.UpdateInputAction() - No InputActionNode found for actionName: " + actionName);
            }
        }

        public void UnbindInputAction(string actionName, bool control, bool shift, string nativePath) {
            Debug.Log($"InputManager.UnbindInputAction() called for action '{actionName}' with nativePath '{nativePath}' (Ctrl: {control}, Shift: {shift})");

            if (string.IsNullOrEmpty(nativePath)) return;

            foreach (InputActionNode inputActionNode in inputActionNodes.Values) {
                InputAction action = inputActionNode.InputAction;
                if (action == null) continue;

                // Loop through all bindings attached to this action
                for (int i = 0; i < action.bindings.Count; i++) {
                    InputBinding binding = action.bindings[i];

                    // Only look for keyboard paths
                    string activePath = !string.IsNullOrEmpty(binding.overridePath) ? binding.overridePath : binding.path;
                    if (activePath.StartsWith("<Keyboard>") && activePath == nativePath) {

                        bool hasCtrlModifier = false;
                        bool hasShiftModifier = false;
                        int compositeStartIndex = -1;

                        // --- MODIFIER MATCH CHECK ---
                        // If this binding is part of a composite (like a modifier binding), we need to validate its siblings
                        if (binding.isPartOfComposite) {
                            // Find the header index of this composite group
                            int currentIndex = i;
                            while (currentIndex >= 0) {
                                if (action.bindings[currentIndex].isComposite) {
                                    compositeStartIndex = currentIndex;
                                    break;
                                }
                                currentIndex--;
                            }

                            // Scan all parts of this specific composite group to check for active modifiers
                            if (compositeStartIndex != -1) {
                                int scanIndex = compositeStartIndex + 1;
                                while (scanIndex < action.bindings.Count && action.bindings[scanIndex].isPartOfComposite) {
                                    InputBinding partBinding = action.bindings[scanIndex];
                                    string partPath = !string.IsNullOrEmpty(partBinding.overridePath) ? partBinding.overridePath : partBinding.path;

                                    // Check if this specific part of the composite is a Ctrl or Shift key
                                    if (partPath.Contains("ctrl") || partPath.Contains("control")) hasCtrlModifier = true;
                                    if (partPath.Contains("shift")) hasShiftModifier = true;

                                    scanIndex++;
                                }
                            }
                        }

                        // If the physical binding modifiers match the parameters passed into the method, proceed
                        if (hasCtrlModifier == control && hasShiftModifier == shift) {

                            // If it was part of a composite, it's safest to reset the entire composite group override
                            if (binding.isPartOfComposite && compositeStartIndex != -1) {
                                action.RemoveBindingOverride(compositeStartIndex);
                            } else {
                                // Otherwise, it's a standalone key, just remove this specific row override
                                action.RemoveBindingOverride(i);
                            }

                            Debug.Log($"Successfully unbound keyboard path '{nativePath}' (Ctrl: {control}, Shift: {shift}) from '{action.name}'. Gamepad settings left untouched.");
                            inputActionNode.ProcessUpdateAction();
                            return;
                        }
                    }
                }
            }
        }

        private InputAction InitializeAction(string actionName, string bindKeyLabel, KeyBindType keyBindType) {
            
            var actionMap = inputActions.FindActionMap(actionName);
            if (actionMap == null) {
                return null;
            }

            // Find the specific Action inside that map
            InputAction inputAction = actionMap.FindAction(actionName);

            // You must explicitly enable the map, or polling will always return false
            actionMap.Enable();
            InputActionNode inputActionNode = new InputActionNode(actionName, inputAction, bindKeyLabel, keyBindType);
            inputActionNodes.Add(actionName, inputActionNode);

            return inputAction;
        }

        private void InitializeKeys() {
            //Debug.Log("KeyBindManager.InitializeKeys()");
            if (inputActions == null) {
                Debug.LogError("InputManager.InitializeKeys() - No input actions asset assigned");
                return;
            }

            forwardAction = InitializeAction("FORWARD", "Forward", KeyBindType.Normal);
            backAction = InitializeAction("BACK", "Backward", KeyBindType.Normal);
            strafeLeftAction = InitializeAction("STRAFELEFT", "Strafe Left", KeyBindType.Normal);
            strafeRightAction = InitializeAction("STRAFERIGHT", "Strafe Right", KeyBindType.Normal);
            turnLeftAction = InitializeAction("TURNLEFT", "Turn Left", KeyBindType.Normal);
            turnRightAction = InitializeAction("TURNRIGHT", "Turn Right", KeyBindType.Normal);
            jumpAction = InitializeAction("JUMP", "Jump", KeyBindType.Normal);
            crouchAction = InitializeAction("CROUCH", "Crouch", KeyBindType.Normal);
            rollAction = InitializeAction("ROLL", "Roll", KeyBindType.Normal);
            toggleRunAction = InitializeAction("TOGGLERUN", "Toggle Run", KeyBindType.Normal);
            toggleAutoRunAction = InitializeAction("TOGGLEAUTORUN", "Toggle Autorun", KeyBindType.Normal);
            toggleStrafeAction = InitializeAction("TOGGLESTRAFE", "Toggle Strafe", KeyBindType.Normal);
            toggleMouseLookAction = InitializeAction("TOGGLEMOUSELOOK", "Toggle Mouse Look", KeyBindType.Normal);

            acceptAction = InitializeAction("ACCEPT", "Accept", KeyBindType.Constant);
            //cancelAction = InitializeAction("CANCEL", "Cancel", KeyBindType.Constant);
            cancelAllAction = InitializeAction("CANCELALL", "Cancel All", KeyBindType.Constant);
            mainMenuAction = InitializeAction("MAINMENU", "Main Menu", KeyBindType.Constant);
            beginChatCommandAction = InitializeAction("BEGINCHATCOMMAND", "Chat Command", KeyBindType.Constant);
            gamepadButtonAAction = InitializeAction("GAMEPADBUTTONA", "Gamepad Button A", KeyBindType.Hidden);
            gamepadButtonBAction = InitializeAction("GAMEPADBUTTONB", "Gamepad Button B", KeyBindType.Hidden);
            gamepadButtonXAction = InitializeAction("GAMEPADBUTTONX", "Gamepad Button X", KeyBindType.Hidden);
            gamepadButtonYAction = InitializeAction("GAMEPADBUTTONY", "Gamepad Button Y", KeyBindType.Hidden);
            gamepadButtonLeftShoulderAction = InitializeAction("GAMEPADBUTTONLEFTSHOULDER", "Gamepad Button Left Shoulder", KeyBindType.Hidden);
            gamepadButtonRightShoulderAction = InitializeAction("GAMEPADBUTTONRIGHTSHOULDER", "Gamepad Button Right Shoulder", KeyBindType.Hidden);
            gamepadButtonSelectAction = InitializeAction("GAMEPADBUTTONSELECT", "Gamepad Button Select", KeyBindType.Hidden);
            gamepadButtonStartAction = InitializeAction("GAMEPADBUTTONSTART", "Gamepad Button Start", KeyBindType.Hidden);
            gamepadButtonLeftStickAction = InitializeAction("GAMEPADBUTTONLEFTSTICK", "Gamepad Button Left Stick", KeyBindType.Hidden);
            gamepadButtonRightStickAction = InitializeAction("GAMEPADBUTTONRIGHTSTICK", "Gamepad Button Right Stick", KeyBindType.Hidden);

            questLogAction = InitializeAction("QUESTLOG", "Quest Log", KeyBindType.System);
            characterPanelAction = InitializeAction("CHARACTERPANEL", "Character Panel", KeyBindType.System);
            currencyPanelAction = InitializeAction("CURRENCYPANEL", "Currency Panel", KeyBindType.System);
            abilityBookAction = InitializeAction("ABILITYBOOK", "Ability Book", KeyBindType.System);
            skillBookAction = InitializeAction("SKILLBOOK", "Skill Book", KeyBindType.System);
            achievementBookAction = InitializeAction("ACHIEVEMENTBOOK", "Achievement Book", KeyBindType.System);
            reputationBookAction = InitializeAction("REPUTATIONBOOK", "Reputation Book", KeyBindType.System);
            inventoryAction = InitializeAction("INVENTORY", "Inventory", KeyBindType.System);
            socialAction = InitializeAction("SOCIAL", "Social", KeyBindType.System);
            mainMapAction = InitializeAction("MAINMAP", "Map", KeyBindType.System);
            nextTargetAction = InitializeAction("NEXTTARGET", "Next Target", KeyBindType.System);
            unequipAllAction = InitializeAction("UNEQUIPALL", "Unequip All", KeyBindType.System);
            hideUIAction = InitializeAction("HIDEUI", "Hide UI", KeyBindType.System);

            actionButton1Action = InitializeAction("ACTIONBUTTON1", "Action Button 1", KeyBindType.Action);
            actionButton2Action = InitializeAction("ACTIONBUTTON2", "Action Button 2", KeyBindType.Action);
            actionButton3Action = InitializeAction("ACTIONBUTTON3", "Action Button 3", KeyBindType.Action);
            actionButton4Action = InitializeAction("ACTIONBUTTON4", "Action Button 4", KeyBindType.Action);
            actionButton5Action = InitializeAction("ACTIONBUTTON5", "Action Button 5", KeyBindType.Action);
            actionButton6Action = InitializeAction("ACTIONBUTTON6", "Action Button 6", KeyBindType.Action);
            actionButton7Action = InitializeAction("ACTIONBUTTON7", "Action Button 7", KeyBindType.Action);
            actionButton8Action = InitializeAction("ACTIONBUTTON8", "Action Button 8", KeyBindType.Action);
            actionButton9Action = InitializeAction("ACTIONBUTTON9", "Action Button 9", KeyBindType.Action);
            actionButton10Action = InitializeAction("ACTIONBUTTON10", "Action Button 10", KeyBindType.Action);
            actionButton11Action = InitializeAction("ACTIONBUTTON11", "Action Button 11", KeyBindType.Action);
            actionButton12Action = InitializeAction("ACTIONBUTTON12", "Action Button 12", KeyBindType.Action);
            actionButton13Action = InitializeAction("ACTIONBUTTON13", "Action Button 13", KeyBindType.Action);
            actionButton14Action = InitializeAction("ACTIONBUTTON14", "Action Button 14", KeyBindType.Action);
            actionButton15Action = InitializeAction("ACTIONBUTTON15", "Action Button 15", KeyBindType.Action);
            actionButton16Action = InitializeAction("ACTIONBUTTON16", "Action Button 16", KeyBindType.Action);
            actionButton17Action = InitializeAction("ACTIONBUTTON17", "Action Button 17", KeyBindType.Action);
            actionButton18Action = InitializeAction("ACTIONBUTTON18", "Action Button 18", KeyBindType.Action);
            actionButton19Action = InitializeAction("ACTIONBUTTON19", "Action Button 19", KeyBindType.Action);
            actionButton20Action = InitializeAction("ACTIONBUTTON20", "Action Button 20", KeyBindType.Action);
            actionButton21Action = InitializeAction("ACTIONBUTTON21", "Action Button 21", KeyBindType.Action);
            actionButton22Action = InitializeAction("ACTIONBUTTON22", "Action Button 22", KeyBindType.Action);
            actionButton23Action = InitializeAction("ACTIONBUTTON23", "Action Button 23", KeyBindType.Action);
            actionButton24Action = InitializeAction("ACTIONBUTTON24", "Action Button 24", KeyBindType.Action);
            actionButton25Action = InitializeAction("ACTIONBUTTON25", "Action Button 25", KeyBindType.Action);
            actionButton26Action = InitializeAction("ACTIONBUTTON26", "Action Button 26", KeyBindType.Action);
            actionButton27Action = InitializeAction("ACTIONBUTTON27", "Action Button 27", KeyBindType.Action);
            actionButton28Action = InitializeAction("ACTIONBUTTON28", "Action Button 28", KeyBindType.Action);
            actionButton29Action = InitializeAction("ACTIONBUTTON29", "Action Button 29", KeyBindType.Action);
            actionButton30Action = InitializeAction("ACTIONBUTTON30", "Action Button 30", KeyBindType.Action);
            actionButton31Action = InitializeAction("ACTIONBUTTON31", "Action Button 31", KeyBindType.Action);
            actionButton32Action = InitializeAction("ACTIONBUTTON32", "Action Button 32", KeyBindType.Action);
            actionButton33Action = InitializeAction("ACTIONBUTTON33", "Action Button 33", KeyBindType.Action);
            actionButton34Action = InitializeAction("ACTIONBUTTON34", "Action Button 34", KeyBindType.Action);
            actionButton35Action = InitializeAction("ACTIONBUTTON35", "Action Button 35", KeyBindType.Action);
            actionButton36Action = InitializeAction("ACTIONBUTTON36", "Action Button 36", KeyBindType.Action);
            actionButton37Action = InitializeAction("ACTIONBUTTON37", "Action Button 37", KeyBindType.Action);
            actionButton38Action = InitializeAction("ACTIONBUTTON38", "Action Button 38", KeyBindType.Action);
            actionButton39Action = InitializeAction("ACTIONBUTTON39", "Action Button 39", KeyBindType.Action);
            actionButton40Action = InitializeAction("ACTIONBUTTON40", "Action Button 40", KeyBindType.Action);
            actionButton41Action = InitializeAction("ACTIONBUTTON41", "Action Button 41", KeyBindType.Action);
            actionButton42Action = InitializeAction("ACTIONBUTTON42", "Action Button 42", KeyBindType.Action);
            actionButton43Action = InitializeAction("ACTIONBUTTON43", "Action Button 43", KeyBindType.Action);
            actionButton44Action = InitializeAction("ACTIONBUTTON44", "Action Button 44", KeyBindType.Action);
            actionButton45Action = InitializeAction("ACTIONBUTTON45", "Action Button 45", KeyBindType.Action);
            actionButton46Action = InitializeAction("ACTIONBUTTON46", "Action Button 46", KeyBindType.Action);
            actionButton47Action = InitializeAction("ACTIONBUTTON47", "Action Button 47", KeyBindType.Action);
            actionButton48Action = InitializeAction("ACTIONBUTTON48", "Action Button 48", KeyBindType.Action);

            actionButton49Action = InitializeAction("ACTIONBUTTON49", "Action Button 49", KeyBindType.Action);
            actionButton50Action = InitializeAction("ACTIONBUTTON50", "Action Button 50", KeyBindType.Action);
            actionButton51Action = InitializeAction("ACTIONBUTTON51", "Action Button 51", KeyBindType.Action);
            actionButton52Action = InitializeAction("ACTIONBUTTON52", "Action Button 52", KeyBindType.Action);
            actionButton53Action = InitializeAction("ACTIONBUTTON53", "Action Button 53", KeyBindType.Action);
            actionButton54Action = InitializeAction("ACTIONBUTTON54", "Action Button 54", KeyBindType.Action);
            actionButton55Action = InitializeAction("ACTIONBUTTON55", "Action Button 55", KeyBindType.Action);
            actionButton56Action = InitializeAction("ACTIONBUTTON56", "Action Button 56", KeyBindType.Action);
            actionButton57Action = InitializeAction("ACTIONBUTTON57", "Action Button 57", KeyBindType.Action);
            actionButton58Action = InitializeAction("ACTIONBUTTON58", "Action Button 58", KeyBindType.Action);
            actionButton59Action = InitializeAction("ACTIONBUTTON59", "Action Button 59", KeyBindType.Action);
            actionButton60Action = InitializeAction("ACTIONBUTTON60", "Action Button 60", KeyBindType.Action);

            actionButton61Action = InitializeAction("ACTIONBUTTON61", "Action Button 61", KeyBindType.Action);
            actionButton62Action = InitializeAction("ACTIONBUTTON62", "Action Button 62", KeyBindType.Action);
            actionButton63Action = InitializeAction("ACTIONBUTTON63", "Action Button 63", KeyBindType.Action);
            actionButton64Action = InitializeAction("ACTIONBUTTON64", "Action Button 64", KeyBindType.Action);
            actionButton65Action = InitializeAction("ACTIONBUTTON65", "Action Button 65", KeyBindType.Action);
            actionButton66Action = InitializeAction("ACTIONBUTTON66", "Action Button 66", KeyBindType.Action);
            actionButton67Action = InitializeAction("ACTIONBUTTON67", "Action Button 67", KeyBindType.Action);
            actionButton68Action = InitializeAction("ACTIONBUTTON68", "Action Button 68", KeyBindType.Action);
            actionButton69Action = InitializeAction("ACTIONBUTTON69", "Action Button 69", KeyBindType.Action);
            actionButton70Action = InitializeAction("ACTIONBUTTON70", "Action Button 70", KeyBindType.Action);
            actionButton71Action = InitializeAction("ACTIONBUTTON71", "Action Button 71", KeyBindType.Action);
            actionButton72Action = InitializeAction("ACTIONBUTTON72", "Action Button 72", KeyBindType.Action);
            actionButton73Action = InitializeAction("ACTIONBUTTON73", "Action Button 73", KeyBindType.Action);
            actionButton74Action = InitializeAction("ACTIONBUTTON74", "Action Button 74", KeyBindType.Action);
            actionButton75Action = InitializeAction("ACTIONBUTTON75", "Action Button 75", KeyBindType.Action);
            actionButton76Action = InitializeAction("ACTIONBUTTON76", "Action Button 76", KeyBindType.Action);
            actionButton77Action = InitializeAction("ACTIONBUTTON77", "Action Button 77", KeyBindType.Action);
            actionButton78Action = InitializeAction("ACTIONBUTTON78", "Action Button 78", KeyBindType.Action);
            actionButton79Action = InitializeAction("ACTIONBUTTON79", "Action Button 79", KeyBindType.Action);
            actionButton80Action = InitializeAction("ACTIONBUTTON80", "Action Button 80", KeyBindType.Action);
            actionButton81Action = InitializeAction("ACTIONBUTTON81", "Action Button 81", KeyBindType.Action);
            actionButton82Action = InitializeAction("ACTIONBUTTON82", "Action Button 82", KeyBindType.Action);
            actionButton83Action = InitializeAction("ACTIONBUTTON83", "Action Button 83", KeyBindType.Action);
            actionButton84Action = InitializeAction("ACTIONBUTTON84", "Action Button 84", KeyBindType.Action);
        }

        public void HandleLevelLoad() {
            ResetAllInput();
        }

        private void ResetAllInput() {
            //Debug.Log("InputManager.ResetAllInput()");
            rightMouseButtonClicked = false;
            rightMouseButtonClickedOverUI = false;
            rightMouseButtonDown = false;
            rightMouseButtonUp = false;
            leftMouseButtonClicked = false;
            leftMouseButtonClickedOverUI = false;
            leftMouseButtonDown = false;
            leftMouseButtonUp = false;
            middleMouseButtonClicked = false;
            middleMouseButtonClickedOverUI = false;
            middleMouseButtonDown = false;
            middleMouseButtonUp = false;
            mouseScrolled = false;
            mouseScrollDeltaY = 0f;
            mouseDeltaX = 0f;
            mouseDeltaY = 0f;

            rightAnalogVertical = 0f;
            rightAnalogHorizontal = 0f;
            leftAnalogVertical = 0f;
            leftAnalogHorizontal = 0f;

            dPadHorizontal = 0f;
            dPadVertical = 0f;
            dPadDown = false;
            dPadDownPressed = false;
            dPadUp = false;
            dPadUpPressed = false;
            dPadLeft = false;
            dPadLeftPressed = false;
            dPadRight = false;
            dPadRightPressed = false;

            leftTriggerAxis = 0f;
            rightTriggerAxis = 0f;
            leftTriggerDown = false;
            leftTriggerUp = false;
            leftTriggerPressed = false;
            rightTriggerDown = false;
            rightTriggerUp = false;
            rightTriggerPressed = false;

            ResetInputActionNodes();
        }

        public void ResetInputActionNodes() {
            foreach (InputActionNode inputActionNode in inputActionNodes.Values) {
                inputActionNode.UnRegisterKeyPress(true);
                inputActionNode.UnRegisterKeyHeld();
                inputActionNode.UnRegisterKeyUp();
            }
        }

        public void RegisterInput() {
            if (keyBindManager.BindName != string.Empty) {
                // we are binding a key.  discard all input
                //Debug.Log("Key Binding in progress.  returning.");
                foreach (InputActionNode inputActionNode in inputActionNodes.Values) {
                    inputActionNode.UnRegisterKeyPress(true);
                    inputActionNode.UnRegisterKeyHeld();
                    inputActionNode.UnRegisterKeyUp();
                }
                return;
            }


            RegisterMouseActions();
            RegisterGamepadActions();
            RegisterKeyPresses();

        }

        private void RegisterGamepadActions() {
            gamepad = Gamepad.current;
            if (gamepad == null) return; // Safeguard if no gamepad is connected
            RegisterDPadAxis();
            RegisterTriggerAxis();
            RegisterAnalog();
        }

        private void RegisterAnalog() {
            rightAnalogVertical = gamepad.rightStick.y.ReadValue();
            rightAnalogHorizontal = gamepad.rightStick.x.ReadValue();
            leftAnalogVertical = gamepad.leftStick.y.ReadValue();
            leftAnalogHorizontal = gamepad.leftStick.x.ReadValue();

        }

        private void RegisterTriggerAxis() {

            leftTriggerAxis = gamepad.leftTrigger.ReadValue();
            rightTriggerAxis = gamepad.rightTrigger.ReadValue();

            //Debug.Log("leftTriggerAxis: " + leftTriggerAxis + "; rightTriggerAxis: " + rightTriggerAxis);

            //leftTriggerDown = false;
            leftTriggerUp = false;
            leftTriggerPressed = false;
            //rightTriggerDown = false;
            rightTriggerUp = false;
            rightTriggerPressed = false;

            if (leftTriggerAxis == 1 && leftTriggerDown == false) {
                leftTriggerDown = true;
            }
            if (leftTriggerAxis == 0 && leftTriggerDown == true) {
                leftTriggerDown = false;
                leftTriggerUp = true;
                leftTriggerPressed = true;
            }

            if (rightTriggerAxis == 1 && rightTriggerDown == false) {
                rightTriggerDown = true;
            }
            if (rightTriggerAxis == 0 && rightTriggerDown == true) {
                rightTriggerDown = false;
                rightTriggerUp = true;
                rightTriggerPressed = true;
            }


        }

        private void RegisterDPadAxis() {
            dPadHorizontal = gamepad.dpad.x.ReadValue();
            dPadVertical = gamepad.dpad.y.ReadValue();
            dPadDownPressed = false;
            dPadUpPressed = false;
            dPadLeftPressed = false;
            dPadRightPressed = false;
            if (dPadDown == false) {
                dPadDown = (dPadVertical < 0f);
                if (dPadDown) {
                    //Debug.Log("dPadDownPressed");
                    dPadDownPressed = true;
                    //gamePadModeActive = true;
                }
            } else if (dPadVertical >= 0f) {
                dPadDown = false;
            }
            if (dPadUp == false) {
                dPadUp = (dPadVertical > 0f);
                if (dPadUp) {
                    //Debug.Log("dPadUpPressed");
                    dPadUpPressed = true;
                    //gamePadModeActive = true;
                }
            } else if (dPadVertical <= 0f) {
                dPadUp = false;
            }
            if (dPadLeft == false) {
                dPadLeft = (dPadHorizontal < 0f);
                if (dPadLeft) {
                    //Debug.Log("dPadLeftPressed");
                    dPadLeftPressed = true;
                    //gamePadModeActive = true;
                }
            } else if (dPadHorizontal >= 0f) {
                dPadLeft = false;
            }
            if (dPadRight == false) {
                dPadRight = (dPadHorizontal > 0f);
                if (dPadRight) {
                    //Debug.Log("dPadRightPressed");
                    dPadRightPressed = true;
                    //gamePadModeActive = true;
                }
            } else if (dPadHorizontal <= 0f) {
                dPadRight = false;
            }
        }

        public void RegisterKeyPresses() {
            if (lastRegisteredFrame >= Time.frameCount) {
                return;
            }
            lastRegisteredFrame = Time.frameCount;

            foreach (InputActionNode inputActionNode in inputActionNodes.Values) {
                InputAction action = inputActionNode.InputAction;

                if (action == null || !action.enabled) {
                    continue;
                }

                // --- 1. Corrected Modern Action Polling API ---
                // Extension methods use uppercase letters, and held states use the IsPressed() method
                bool keyDown = action.WasPressedThisFrame();
                bool keyHeld = action.IsPressed();
                bool keyUp = action.WasReleasedThisFrame();

                // --- 2. Execute AnyRPG Action Registrations ---
                if (keyDown) {
                    inputActionNode.RegisterKeyPress();
                } else {
                    inputActionNode.UnRegisterKeyPress();
                }

                if (keyHeld) {
                    inputActionNode.RegisterKeyHeld();
                } else {
                    inputActionNode.UnRegisterKeyHeld();
                }

                if (keyUp) {
                    inputActionNode.RegisterKeyUp();
                } else {
                    inputActionNode.UnRegisterKeyUp();
                }
            }
        }

        public bool KeyBindWasPressedOrHeld(string actionName) {
            if (inputActionNodes.ContainsKey(actionName) &&
                (inputActionNodes[actionName].KeyPressed == true || inputActionNodes[actionName].KeyHeld == true)) {
                return true;
            }
            return false;
        }

        public bool KeyBindWasPressed(string actionName) {
            if (inputActionNodes.ContainsKey(actionName) && inputActionNodes[actionName].KeyPressed == true) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Detect clicks and drags
        /// </summary>
        private void RegisterMouseActions() {
            leftMouseButtonClicked = false;
            rightMouseButtonClicked = false;
            middleMouseButtonClicked = false;
            leftMouseButtonUp = false;
            rightMouseButtonUp = false;
            middleMouseButtonUp = false;
            mouseScrolled = false;

            mouse = Mouse.current;
            if (mouse == null) return; // Safeguard if no mouse is connected

            // track left mouse button up and down events to determine difference in click vs drag
            if (mouse.leftButton.wasReleasedThisFrame) {
                if (leftMouseButtonDown) {
                    leftMouseButtonUpPosition = mouse.position.ReadValue();
                    leftMouseButtonUp = true;
                    //Debug.Log($"down mouse position: {leftMouseButtonDownPosition.ToString()} up mouse position: {leftMouseButtonUpPosition.ToString()}");
                    if (leftMouseButtonUpPosition == leftMouseButtonDownPosition) {
                        leftMouseButtonClicked = true;
                    }
                    leftMouseButtonDown = false;
                    leftMouseButtonClickedOverUI = false;
                }
            } else {
                leftMouseButtonUp = false;
            }

            // track right mouse button up and down events to determine difference in click vs drag
            if (mouse.rightButton.wasReleasedThisFrame) {
                if (rightMouseButtonDown) {
                    rightMouseButtonUpPosition = mouse.position.ReadValue();
                    rightMouseButtonUp = true;
                    //Debug.Log($"down mouse position: {rightMouseButtonDownPosition.ToString()} up mouse position: {rightMouseButtonUpPosition.ToString()}");
                    if (rightMouseButtonUpPosition == rightMouseButtonDownPosition) {
                        rightMouseButtonClicked = true;
                    }
                    rightMouseButtonDown = false;
                    rightMouseButtonClickedOverUI = false;
                }
            } else {
                rightMouseButtonUp = false;
            }


            // track middle mouse button up and down events to determine difference in click vs drag
            if (mouse.middleButton.wasReleasedThisFrame && middleMouseButtonDown) {
                middleMouseButtonUpPosition = mouse.position.ReadValue();
                middleMouseButtonUp = true;
                //Debug.Log($"down mouse position: {rightMouseButtonDownPosition.ToString()} up mouse position: {rightMouseButtonUpPosition.ToString()}");
                if (middleMouseButtonUpPosition == middleMouseButtonDownPosition) {
                    middleMouseButtonClicked = true;
                }
                middleMouseButtonDown = false;
                middleMouseButtonClickedOverUI = false;
            } else {
                middleMouseButtonUp = false;
            }


            // return if the mouse is off the screen, after detecting mouse up events (in case we started a pan while on screen)
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            if (!screenRect.Contains(mousePosition))
                return;

            if (mouse.rightButton.wasPressedThisFrame) {
                rightMouseButtonDown = true;
                rightMouseButtonDownPosition = mouse.position.ReadValue();
                // IGNORE NAMEPLATES FOR THE PURPOSE OF CAMERA MOVEMENT
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNameplate() : true)) {
                    rightMouseButtonClickedOverUI = true;
                }
            }

            // track left mouse button up and down events to determine difference in click vs drag
            if (mouse.leftButton.wasPressedThisFrame) {
                leftMouseButtonDown = true;
                leftMouseButtonDownPosition = mouse.position.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNameplate() : true)) {
                    leftMouseButtonClickedOverUI = true;
                }
            }

            if (mouse.middleButton.wasPressedThisFrame) {
                middleMouseButtonDown = true;
                middleMouseButtonDownPosition = mouse.position.ReadValue();
                if (EventSystem.current.IsPointerOverGameObject() && (namePlateManager != null ? !namePlateManager.MouseOverNameplate() : true)) {
                    middleMouseButtonClickedOverUI = true;
                }
            }

            mouseScrollDeltaY = mouse != null ? mouse.scroll.y.ReadValue() / 120f : 0f;
            mouseDeltaX = mouse != null ? mouse.delta.x.ReadValue() : 0f;
            mouseDeltaY = mouse != null ? mouse.delta.y.ReadValue() : 0f;
            mousePosition = mouse != null ? mouse.position.ReadValue() : Vector3.zero;

            if (mouseScrollDeltaY != 0f) {
                //Debug.Log($"Mouse scrolled: {mouseScrollDeltaY}");
                mouseScrolled = true;
            }

        }
    }

}