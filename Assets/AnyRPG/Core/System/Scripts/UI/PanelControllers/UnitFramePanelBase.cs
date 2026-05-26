using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitFramePanelBase : NavigableInterfaceElement, IPointerClickHandler, IContextMenuTarget {

        [Header("Unit Name")]

        [SerializeField]
        protected TextMeshProUGUI unitNameText = null;

        [SerializeField]
        protected Image unitNameBackground = null;

        [SerializeField]
        protected TextMeshProUGUI unitLevelText = null;

        [Header("Resources")]

        [SerializeField]
        protected LayoutElement primaryResourceSliderLayout = null;

        [SerializeField]
        protected LayoutElement secondaryResourceSliderLayout = null;


        [FormerlySerializedAs("healthSlider")]
        [SerializeField]
        protected Image primaryResourceSlider = null;

        [FormerlySerializedAs("healthText")]
        [SerializeField]
        protected TextMeshProUGUI primaryResourceText = null;

        [FormerlySerializedAs("manaSlider")]
        [SerializeField]
        protected Image secondaryResourceSlider = null;

        [FormerlySerializedAs("manaText")]
        [SerializeField]
        protected TextMeshProUGUI secondaryResourceText = null;

        [Header("Cast Bar")]

        [SerializeField]
        protected CastBarController castBarController = null;

        [Header("Unit Preview")]

        [SerializeField]
        protected Image leaderIcon = null;

        [SerializeField]
        protected Texture portraitTexture = null;

        [SerializeField]
        protected Image portraitImage = null;

        protected float originalPrimaryResourceSliderWidth = 0f;
        protected float originalSecondaryResourceSliderWidth = 0f;

        protected UnitController unitController = null;

        [Header("Status Effects")]

        [SerializeField]
        protected StatusEffectPanelController statusEffectPanelController = null;

        protected Transform followTransform = null;

        protected PowerResource primaryPowerResource = null;
        protected PowerResource secondaryPowerResource = null;

        protected Color powerResourceColor1 = Color.green;
        protected Color powerResourceColor2 = Color.blue;

        protected bool controllerInitialized = false;
        protected bool targetInitialized = false;
        protected bool partialTargetInitialization = false;
        protected bool subscribedToTargetReady = false;

        protected Color reputationColor;

        // game manager references
        protected PlayerManagerClient playerManagerClient = null;
        protected ContextMenuService contextMenuService = null;
        protected CharacterGroupServiceClient characterGroupServiceClient = null;
        protected InspectCharacterService inspectCharacterService = null;
        protected TradeServiceClient tradeServiceClient = null;

        //public BaseNameplateController UnitNameplateController { get => namePlateController; set => namePlateController = value; }
        public UnitController UnitController { get => unitController; set => unitController = value; }

        //public GameObject FollowGameObject { get => followGameObject; set => followGameObject = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"{gameObject.name}: UnitFrameController.Awake()");
            base.Configure(systemGameManager);

            InitializeController();
            statusEffectPanelController.Configure(systemGameManager);
            if (!targetInitialized) {
                this.gameObject.SetActive(false);
            }
            if (statusEffectPanelController != null) {
                statusEffectPanelController.EffectLimit = 7;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
            contextMenuService = systemGameManager.ContextMenuService;
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
            inspectCharacterService = systemGameManager.InspectCharacterService;
            tradeServiceClient = systemGameManager.TradeServiceClient;
        }

        public void InitializeController() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.InitializeController()");
            if (controllerInitialized) {
                return;
            }
            ProcessInitializeController();
            controllerInitialized = true;
            //Debug.Log($"{gameObject.name}: UnitFrameController.Awake() originalHealthSliderWidth: " + originalHealthSliderWidth);
        }

        protected virtual void ProcessInitializeController() {
            originalPrimaryResourceSliderWidth = primaryResourceSliderLayout.preferredWidth;
            originalSecondaryResourceSliderWidth = secondaryResourceSliderLayout.preferredWidth;
            DeactivateCastBar();
        }

        private void TargetInitialization() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.TargetInitialization() instanceId: {GetInstanceID()}");

            if (unitController == null) {
                return;
            }
            InitializeStats();
            gameObject.SetActive(true);
            targetInitialized = true;
            if (isActiveAndEnabled == false) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.TargetInitialization(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.");
                partialTargetInitialization = true;
                return;
            }
            partialTargetInitialization = false;
            PostTargetInitialization();
        }

        protected virtual void PostTargetInitialization() {
            // overridden in derived classes, if necessary
        }

        public virtual void ConfigurePortrait(Sprite icon) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.ConfigurePortrait()");

            portraitImage.gameObject.SetActive(true);

            portraitImage.sprite = icon;
        }

        public virtual void SetTarget(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.SetTarget({unitController.gameObject.name})");

            // prevent old target from still sending us updates while we are focused on a new target
            ClearTarget(false);

            if (!isActiveAndEnabled) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.SetTarget(" + target.name + "): controller is not active and enabled.  Activating");
                gameObject.SetActive(true);
            }

            this.unitController = unitController;

            CalculateResourceColors();
            UpdateLeaderIcon();
            if (unitController != null) {
                castBarController.SetTarget(unitController);
                statusEffectPanelController.SetTarget(unitController);
                if (unitController != playerManagerClient.UnitController) {
                    systemEventManager.OnReputationChange += HandleReputationChange;
                }
            }

            if (isActiveAndEnabled) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.SetTarget(" + target.name + "):  WE ARE NOW ACTIVE AND ENABLED");
                TargetInitialization();
            }
        }

        public void UpdateLeaderIcon() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.UpdateLeaderIcon()");

            if (leaderIcon == null || unitController == null) {
                // no icon or no target, hide leader icon
                leaderIcon.gameObject.SetActive(false);
                return;
            }

            if (characterGroupServiceClient.CurrentCharacterGroup == null) {
                // no group, hide icon
                leaderIcon.gameObject.SetActive(false);
                return;
            }

            if (characterGroupServiceClient.CurrentCharacterGroup.leaderPlayerCharacterId == unitController.CharacterId) {
                // this unit is the leader, show icon
                leaderIcon.gameObject.SetActive(true);
            } else {
                // not the leader, hide icon
                leaderIcon.gameObject.SetActive(false);
            }
        }

        public void CalculateResourceColors() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.CalculateResourceColors()");

            if (unitController?.NameplateController != null) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.CalculateResourceColors(): found NameplateController on unitController");
                if (unitController.NameplateController.PowerResourceList.Count > 0) {
                    primaryPowerResource = unitController.NameplateController.PowerResourceList[0];
                    powerResourceColor1 = unitController.NameplateController.PowerResourceList[0].DisplayColor;
                } else {
                    primaryPowerResource = null;
                }
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.CalculateResourceColors(): {primaryPowerResource?.ResourceName}");
                if (unitController.NameplateController.PowerResourceList.Count > 1) {
                    secondaryPowerResource = unitController.NameplateController.PowerResourceList[1];
                    powerResourceColor2 = unitController.NameplateController.PowerResourceList[1].DisplayColor;
                } else {
                    secondaryPowerResource = null;
                }
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.CalculateResourceColors(): {secondaryPowerResource?.ResourceName}");
            }

            if (primaryResourceSlider.color != powerResourceColor1) {
                primaryResourceSlider.color = powerResourceColor1;
            }
            if (secondaryResourceSlider.color != powerResourceColor2) {
                secondaryResourceSlider.color = powerResourceColor2;
            }


        }

        public virtual void ClearTarget(bool closeWindowOnClear = true) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.ClearTarget({closeWindowOnClear})");

            if (unitController != null) {
                ClearSubscriptions();
            }
            systemEventManager.OnReputationChange -= HandleReputationChange;
            unitController = null;
            targetInitialized = false;
            leaderIcon.gameObject.SetActive(false);
            castBarController.ClearTarget();
            statusEffectPanelController.ClearTarget();

            primaryPowerResource = null;
            secondaryPowerResource = null;
            if (closeWindowOnClear) {
                gameObject.SetActive(false);
            }
        }

        public virtual void CreateSubscriptions() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.CreateSubscriptions()");

            // allow the character to send us events whenever the hp, mana, or cast time has changed so we can update the windows that display those values
            unitController.UnitEventController.OnResourceAmountChanged += HandleResourceAmountChanged;
            unitController.UnitEventController.OnNameChange += HandleNameChange;
            unitController.UnitEventController.OnClassChange += HandleClassChange;
            unitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            unitController.UnitEventController.OnReviveComplete += HandleReviveComplete;
            unitController.UnitEventController.OnReputationChange += HandleReputationChange;
        }

        public virtual void ClearSubscriptions() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.ClearSubscriptions() instanceId: {GetInstanceID()}");

            if (unitController == null) {
                return;
            }

            unitController.UnitEventController.OnResourceAmountChanged -= HandleResourceAmountChanged;
            unitController.UnitEventController.OnNameChange -= HandleNameChange;
            unitController.UnitEventController.OnClassChange -= HandleClassChange;
            unitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;
            unitController.UnitEventController.OnReviveComplete -= HandleReviveComplete;
            unitController.UnitEventController.OnReputationChange -= HandleReputationChange;
        }

        public void HandleReviveComplete(UnitController sourceUnitController) {
            HandleReputationChange(sourceUnitController);
        }

        private void InitializeStats() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.InitializeStats()");

            HandleReputationChange(playerManagerClient.UnitController);
            //Debug.Log("Charcter name is " + baseCharacter.MyCharacterName);
            unitNameText.text = unitController.DisplayName;

            if (!unitController.NameplateController.HasPrimaryResource()) {
                ClearPrimaryResourceBar();
            }

            if (!unitController.NameplateController.HasSecondaryResource()) {
                ClearSecondaryResourceBar();
            }

            // set initial resource values in character display
            int counter = 0;
            foreach (PowerResource _powerResource in unitController.NameplateController.PowerResourceList) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.InitializeStats(): Initializing resource: {_powerResource.ResourceName}");
                HandleResourceAmountChanged(_powerResource, (int)unitController.NameplateController.GetPowerResourceMaxAmount(_powerResource), (int)unitController.NameplateController.GetPowerResourceAmount(_powerResource));
                counter++;
                if (counter > 1) {
                    break;
                }
            }

            CreateSubscriptions();

            HandleLevelChanged(unitController.NameplateController.Level);
        }

        public void HandleClassChange(UnitController sourceUnitController, CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.HandleClassChange({sourceUnitController.gameObject.name}, {newCharacterClass}, {oldCharacterClass})");
            CalculateResourceColors();
        }

        public void ClearPrimaryResourceBar() {

            if (primaryResourceSliderLayout != null) {
                primaryResourceSliderLayout.preferredWidth = 0;
            }
            if (primaryResourceText != null) {
                primaryResourceText.text = string.Empty;
            }
        }

        public void ClearSecondaryResourceBar() {

            if (secondaryResourceSliderLayout != null) {
                secondaryResourceSliderLayout.preferredWidth = 0;
            }
            if (secondaryResourceText != null) {
                secondaryResourceText.text = string.Empty;
            }
        }

        public void ClearResourceBars() {

            ClearPrimaryResourceBar();
            ClearSecondaryResourceBar();

        }

        private void DeactivateCastBar() {
            castBarController.ClearTarget();
        }

        public void HandlePrimaryResourceAmountChanged(int maxResourceAmount, int currentResourceAmount) {
            // Debug.Log(gameObject.name + ".UnitFramePanelBase.HandlePrimaryResourceAmountChanged(" + maxResourceAmount + ", " + currentResourceAmount + ")");

            // prevent division by zero
            int displayedMaxResource = maxResourceAmount;
            int displayedCurrentResource = currentResourceAmount;
            if (displayedMaxResource == 0) {
                displayedMaxResource = 1;
                displayedCurrentResource = 1;
            }

            float resourcePercent = (float)displayedCurrentResource / displayedMaxResource;

            // code for an actual image, not currently used
            //playerHPSlider.fillAmount = healthPercent;

            // code for the default image
            if (primaryResourceSliderLayout != null) {
                primaryResourceSliderLayout.preferredWidth = resourcePercent * originalPrimaryResourceSliderWidth;
            }

            if (primaryResourceText != null) {
                string percentText = string.Empty;
                if (resourcePercent != 0f) {
                    percentText = (resourcePercent * 100).ToString("F0");
                }
                primaryResourceText.text = string.Format("{0} / {1} ({2}%)", displayedCurrentResource, displayedMaxResource, percentText);
            }

            if (displayedCurrentResource <= 0 && unitController.NameplateController.HasHealth() == true) {
                Color tmp = Color.gray;
                //Color tmp = Faction.GetFactionColor(baseCharacter.MyFaction);
                tmp.a = 0.5f;
                unitNameBackground.color = tmp;
            } else {
                if (unitNameBackground.color != reputationColor) {
                    unitNameBackground.color = reputationColor;
                }
            }
        }

        public void HandleSecondaryResourceAmountChanged(int maxResourceAmount, int currentResourceAmount) {

            // prevent division by zero
            int displayedMaxResource = maxResourceAmount;
            int displayedCurrentResource = currentResourceAmount;
            if (displayedMaxResource == 0) {
                displayedMaxResource = 1;
                displayedCurrentResource = 1;
            }

            float resourcePercent = (float)displayedCurrentResource / displayedMaxResource;

            // code for an actual image, not currently used
            //playerManaSlider.fillAmount = manaPercent;

            // code for the default image
            if (secondaryResourceSliderLayout != null) {
                secondaryResourceSliderLayout.preferredWidth = resourcePercent * originalSecondaryResourceSliderWidth;
            }

            if (secondaryResourceText != null) {
                string percentText = string.Empty;
                if (maxResourceAmount > 0) {
                    percentText = " (" + (resourcePercent * 100).ToString("F0") + "%)";
                }
                secondaryResourceText.text = string.Format("{0} / {1}{2}", currentResourceAmount, maxResourceAmount, percentText);
            }
        }

        public virtual void HandleLevelChanged(int _level) {
            CalculateResourceColors();
            if (unitLevelText == null) {
                return;
            }
            unitLevelText.text = _level.ToString();
            if (playerManagerClient.UnitController?.CharacterStats != null) {
                unitLevelText.color = LevelEquations.GetTargetColor(playerManagerClient.UnitController.CharacterStats.Level, _level);
            }
        }

        public void HandleNameChange(string newName) {
            unitNameText.text = newName;
        }

        /// <summary>
        /// accept a resource amount changed message
        /// </summary>
        /// <param name="maxResourceAmount"></param>
        /// <param name="currentResourceAmount"></param>
        public void HandleResourceAmountChanged(PowerResource powerResource, int maxResourceAmount, int currentResourceAmount) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.HandleResourceAmountChanged({powerResource.ResourceName}, {maxResourceAmount}, {currentResourceAmount})");
            if (unitController?.NameplateController != null) {
                int counter = 0;
                bool updateBar = false;
                foreach (PowerResource _powerResource in unitController.NameplateController.PowerResourceList) {
                    if (powerResource == _powerResource) {
                        updateBar = true;
                        break;
                    }
                    counter++;
                    if (counter > 1) {
                        break;
                    }
                }

                if (updateBar) {
                    if (counter == 0) {
                        HandlePrimaryResourceAmountChanged(maxResourceAmount, currentResourceAmount);
                    }
                    if (counter == 1) {
                        HandleSecondaryResourceAmountChanged(maxResourceAmount, currentResourceAmount);
                    }
                }

            }

        }

        public void HandleReputationChange(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.HandleReputationChange({sourceUnitController.gameObject.name}) instanceId: {GetInstanceID()}");

            if (playerManagerClient == null || playerManagerClient.PlayerUnitSpawned == false) {
                return;
            }
            reputationColor = Faction.GetFactionColor(playerManagerClient, unitController);
            //Color tmp = Faction.GetFactionColor(baseCharacter.Faction);
            reputationColor.a = 0.5f;
            unitNameBackground.color = reputationColor;

        }

        public void OnEnable() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.OnEnable() instanceId: {GetInstanceID()}");

            // just in case something was targetted before the canvas became active
            if (partialTargetInitialization && unitController != null) {
                TargetInitialization();
            }
        }

        public void OnPointerClick(PointerEventData pointerEventData) {
            //Debug.Log("UnitFrameController.OnPointerClick()");

            if (unitController == null) {
                return;
            }

            if (pointerEventData.button == PointerEventData.InputButton.Right) {
                HandleRightClick(pointerEventData.position);
            } else if (pointerEventData.button == PointerEventData.InputButton.Left) {
                HandleLeftClick(pointerEventData.position);
            }
        }

        private void HandleRightClick(Vector2 mousePosition) {
            //Debug.Log($"UnitFrameController.HandleRightClick({mousePosition})");

            contextMenuService.ShowContextMenu(this, mousePosition);
        }

        protected virtual void HandleLeftClick(Vector2 mousePosition) {
            //Debug.Log($"UnitFrameController.HandleLeftClick({mousePosition})");
            // nothing here, overridden in derived classes
        }

        public void SetupContextMenu(ContextMenuPanel contextMenuPanel) {
            SetupInspectButton(contextMenuPanel);
            SetupInviteButton(contextMenuPanel);
            SetupKickButton(contextMenuPanel);
            SetupDisbandButton(contextMenuPanel);
            SetupLeaveButton(contextMenuPanel);
            SetupPromoteButton(contextMenuPanel);
            SetupTradeButton(contextMenuPanel);
            SetupMessageButton(contextMenuPanel);
        }

        private void SetupInspectButton(ContextMenuPanel contextMenuPanel) {
            if (unitController == null) {
                return;
            }
            if (unitController == playerManagerClient.UnitController) {
                // inspect button should not be available if the target is the player
                return;
            }

            contextMenuPanel.EnableInspectButton(true);
        }

        private void SetupTradeButton(ContextMenuPanel contextMenuPanel) {
            if (unitController == null) {
                return;
            }
            if (unitController == playerManagerClient.UnitController) {
                // cannot trade with ourselves
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is player, disabling invite button");
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only trade in network mode
                //Debug.Log("ContextMenuPanel.SetupMessageButton() game mode is local, disabling invite button");
                return;
            }

            if (unitController.UnitControllerMode != UnitControllerMode.Player) {
                // can only trade players
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is not player, disabling invite button");
                return;
            }

            if (Faction.RelationWith(unitController, playerManagerClient.UnitController) < 0) {
                // can only trade neutral or better
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target faction relationship is negative, disabling invite button");
                return;
            }

            // all checks passed.  this character can be traded with
            contextMenuPanel.EnableTradeButton(true);
        }

        private void SetupMessageButton(ContextMenuPanel contextMenuPanel) {
            if (unitController == null) {
                return;
            }

            if (unitController == playerManagerClient.UnitController) {
                // cannot message ourselves
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is player, disabling invite button");
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only message in network mode
                //Debug.Log("ContextMenuPanel.SetupMessageButton() game mode is local, disabling invite button");
                return;
            }

            if (unitController.UnitControllerMode != UnitControllerMode.Player) {
                // can only message players
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target is not player, disabling invite button");
                return;
            }

            if (Faction.RelationWith(unitController, playerManagerClient.UnitController) < 0) {
                // can only message neutral or better
                //Debug.Log("ContextMenuPanel.SetupMessageButton() target faction relationship is negative, disabling invite button");
                return;
            }

            if (systemConfigurationManager.PrivateMessageChatCommand == string.Empty) {
                // system must have a message command to use
                return;
            }

            // all checks passed.  this character can be messaged
            contextMenuPanel.EnableMessageButton(true);
        }

        private void SetupInviteButton(ContextMenuPanel contextMenuPanel) {
                if (unitController == null) {
                    return;
            }
            if (unitController == playerManagerClient.UnitController) {
                // invite button should not be available if the target is the player
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target is player, disabling invite button");
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                //Debug.Log("ContextMenuPanel.SetupInviteButton() game mode is local, disabling invite button");
                return;
            }

            if (unitController.UnitControllerMode != UnitControllerMode.Player) {
                // can only invite players
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target is not player, disabling invite button");
                return;
            }

            if (Faction.RelationWith(unitController, playerManagerClient.UnitController) < 0) {
                // can only invite neutral or better
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target faction relationship is negative, disabling invite button");
                return;
            }

            if (unitController.CharacterGroupManager.IsInGroup()) {
                // target is already in a group
                //Debug.Log("ContextMenuPanel.SetupInviteButton() target is already in a group, disabling invite button");
                return;
            }

            // all checks passed.  this character can be invited
            contextMenuPanel.EnableInviteButton(true);
        }

        private void SetupPromoteButton(ContextMenuPanel contextMenuPanel) {
            if (unitController == null) {
                return;
            }

            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                return;
            }

            // check if the player is the leader
            if (characterGroup.leaderPlayerCharacterId != playerManagerClient.UnitController.CharacterId) {
                // player is not the leader
                return;
            }

            // check that the target is not the player
            if (unitController == playerManagerClient.UnitController) {
                // cannot promote yourself
                return;
            }

            // check if the target is in the group
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(unitController.CharacterId) == false) {
                // target is not in the group
                return;
            }

            // all checks passed.  this character can be promoted
            contextMenuPanel.EnablePromoteButton(true);
        }

        private void SetupKickButton(ContextMenuPanel contextMenuPanel) {
            if (unitController == null) {
                return;
            }
            if (unitController == playerManagerClient.UnitController) {
                // invite button should not be available if the target is the player
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                return;
            }

            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                return;
            }

            // check if the player is the leader
            if (characterGroup.leaderPlayerCharacterId != playerManagerClient.UnitController.CharacterId) {
                // player is not the leader
                return;
            }

            // check if the target is in the group
            if (characterGroup.MemberList[UnitControllerMode.Player].ContainsKey(unitController.CharacterId) == false) {
                // target is not in the group
                return;
            }
            // all checks passed.  this character can be kicked
            contextMenuPanel.EnableKickButton(true);
        }

        private void SetupDisbandButton(ContextMenuPanel contextMenuPanel) {
            if (unitController == null) {
                return;
            }
            if (unitController != playerManagerClient.UnitController) {
                // only the leader can disband themselves
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                return;
            }

            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                return;
            }

            // check if the player is the leader
            if (characterGroup.leaderPlayerCharacterId != playerManagerClient.UnitController.CharacterId) {
                // player is not the leader
                return;
            }

            // all checks passed.  this character can be kicked
            contextMenuPanel.EnableDisbandButton(true);
        }

        private void SetupLeaveButton(ContextMenuPanel contextMenuPanel) {
            if (unitController == null) {
                return;
            }
            if (unitController != playerManagerClient.UnitController) {
                // only the leader can leave themselves
                return;
            }

            if (systemGameManager.GameMode == GameMode.Local) {
                // can only invite in network mode
                return;
            }

            // check if the player is in a group
            CharacterGroup characterGroup = characterGroupServiceClient.CurrentCharacterGroup;
            if (characterGroup == null) {
                // player is not in a group
                return;
            }

            // all checks passed.  this character can be kicked
            contextMenuPanel.EnableLeaveButton(true);
        }

        public void BeginPrivateMessage() {
            //Debug.Log($"ContextMenuService.BeginPrivateMessage()");

            if (unitController == null) {
                return;
            }
            contextMenuService.BeginPrivateMessage(unitController);
        }


        public void PerformContextMenuAction(string actionName) {
            switch (actionName) {
                case "Inspect":
                    inspectCharacterService.SetTargetUnitController(unitController);
                    break;
                case "Trade":
                    tradeServiceClient.RequestBeginTrade(unitController.CharacterId);
                    break;
                case "Invite":
                    characterGroupServiceClient.RequestInviteCharacterToGroup(unitController.CharacterId);
                    break;
                case "Kick":
                    characterGroupServiceClient.RequestRemoveCharacterFromGroup(unitController.CharacterId);
                    break;
                case "Disband":
                    characterGroupServiceClient.RequestDisbandGroup();
                    break;
                case "Leave":
                    characterGroupServiceClient.RequestLeaveGroup();
                    break;
                case "Promote":
                    characterGroupServiceClient.RequestPromoteCharacterToLeader(unitController.CharacterId);
                    break;
                case "Message":
                    BeginPrivateMessage();
                    break;
                default:
                    break;
            }
        }
    }
}