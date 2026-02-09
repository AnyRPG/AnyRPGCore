using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitFramePanelBase : NavigableInterfaceElement, IPointerClickHandler {

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

        // the next 2 things need to be updated to focus on the right character
        [SerializeField]
        protected Transform cameraTransform = null;

        // replaces cameraTransform;
        [SerializeField]
        protected Camera previewCamera = null;

        [SerializeField]
        protected Image leaderIcon = null;

        [SerializeField]
        protected Texture portraitTexture = null;

        [SerializeField]
        protected Image portraitImage = null;

        [SerializeField]
        protected RawImage portraitSnapshotImage = null;

        [SerializeField]
        protected Vector3 cameraLookOffsetDefault = new Vector3(0, 1.6f, 0);

        [SerializeField]
        protected Vector3 cameraPositionOffsetDefault = new Vector3(0, 1.6f, 0.66f);

        protected Vector3 cameraLookOffset = Vector3.zero;

        protected Vector3 cameraPositionOffset = Vector3.zero;

        protected float originalPrimaryResourceSliderWidth = 0f;
        protected float originalSecondaryResourceSliderWidth = 0f;

        //protected BaseNamePlateController namePlateController = null;
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

        protected Coroutine waitForCameraCoroutine = null;

        // avoid GC by using global variables for these
        protected Vector3 wantedPosition = Vector3.zero;
        protected Vector3 wantedLookPosition = Vector3.zero;

        // track the camera wait start frame to ensure the current camera wait routine is still valid
        //private int lastWaitFrame = 0;

        // game manager references
        protected PlayerManager playerManager = null;
        protected ContextMenuService contextMenuService = null;
        protected CharacterGroupServiceClient characterGroupServiceClient = null;

        //public BaseNamePlateController UnitNamePlateController { get => namePlateController; set => namePlateController = value; }
        public UnitController UnitController { get => unitController; set => unitController = value; }

        //public GameObject FollowGameObject { get => followGameObject; set => followGameObject = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"{gameObject.name}: UnitFrameController.Awake()");
            base.Configure(systemGameManager);

            InitializeController();
            statusEffectPanelController.Configure(systemGameManager);
            if (previewCamera != null) {
                previewCamera.enabled = false;
            }
            if (!targetInitialized) {
                this.gameObject.SetActive(false);
            }
            if (statusEffectPanelController != null) {
                statusEffectPanelController.EffectLimit = 7;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            contextMenuService = systemGameManager.ContextMenuService;
            characterGroupServiceClient = systemGameManager.CharacterGroupServiceClient;
        }

        public void InitializeController() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.InitializeController()");
            if (controllerInitialized) {
                return;
            }
            portraitSnapshotImage.texture = portraitTexture;
            originalPrimaryResourceSliderWidth = primaryResourceSliderLayout.preferredWidth;
            originalSecondaryResourceSliderWidth = secondaryResourceSliderLayout.preferredWidth;
            DeactivateCastBar();
            controllerInitialized = true;
            //Debug.Log($"{gameObject.name}: UnitFrameController.Awake() originalHealthSliderWidth: " + originalHealthSliderWidth);
        }

        private void LateUpdate() {
            if (systemConfigurationManager.UIConfiguration.RealTimeUnitFrameCamera) {
                UpdateCameraPosition();
            }
        }

        private void UpdateCameraPosition() {
            if (!targetInitialized || unitController.CameraTargetReady == false) {
                //Debug.Log("UnitFrameController.Update(). Not initialized yet.  Exiting.");
                return;
            }
            if (unitController.CameraTargetReady == true && followTransform == null) {
                //Debug.Log($"{gameObject.name}UnitFrameController.Update(). Follow transform is null. possibly dead unit despawned. Exiting.");
                ClearTarget();
                return;
            }

            if (cameraTransform != null) {
                //Vector3 wantedPosition = followTransform.TransformPoint(0, offsetY, offsetZ);
                //Vector3 wantedLookPosition = followTransform.TransformPoint(0, offsetY, 0);

                //Vector3 wantedPosition = followTransform.TransformPoint(cameraPositionOffset);
                wantedPosition = unitController.transform.TransformPoint(unitController.transform.InverseTransformPoint(followTransform.position) + cameraPositionOffset);
                //Vector3 wantedLookPosition = followTransform.TransformPoint(cameraLookOffset);
                wantedLookPosition = unitController.transform.TransformPoint(unitController.transform.InverseTransformPoint(followTransform.position) + cameraLookOffset);
                cameraTransform.position = wantedPosition;
                cameraTransform.LookAt(wantedLookPosition);

            } else {
            }
        }

        private void TargetInitialization() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.TargetInitialization() instanceId: {GetInstanceID()}");

            if (unitController == null) {
                return;
            }
            InitializeStats();
            InitializePosition();
            gameObject.SetActive(true);
            targetInitialized = true;
            if (isActiveAndEnabled == false) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.TargetInitialization(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.");
                partialTargetInitialization = true;
                return;
            }
            partialTargetInitialization = false;
            unitController.ConfigureUnitFrame(this, previewCamera != null);

        }

        public void ConfigurePortrait(Sprite icon) {
            portraitSnapshotImage.gameObject.SetActive(false);
            portraitImage.gameObject.SetActive(true);

            portraitImage.sprite = icon;
        }

        public void ConfigureSnapshotPortrait() {
            portraitImage.gameObject.SetActive(false);
            portraitSnapshotImage.gameObject.SetActive(true);
            if (unitController.CameraTargetReady) {
                HandleTargetReady();
            }// else {
             // testing subscribe no matter what in case unit appearance changes
            SubscribeToTargetReady();
            //}
        }

        public void SubscribeToTargetReady() {
            unitController.OnCameraTargetReady += HandleTargetReady;
            subscribedToTargetReady = true;
        }

        public void UnsubscribeFromTargetReady() {
            if (subscribedToTargetReady == false) {
                return;
            }

            if (unitController != null) {
                unitController.OnCameraTargetReady -= HandleTargetReady;
            }

            subscribedToTargetReady = false;
        }

        public void HandleTargetReady() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.HandleTargetReady()");

            //UnsubscribeFromTargetReady();
            GetFollowTarget();
            UpdateCameraPosition();
            //lastWaitFrame++;
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.HandleTargetReady() " + namePlateController.Interactable.GetInstanceID() + "; frame : " + lastWaitFrame);
            //if (waitForCameraCoroutine == null) {
            //waitForCameraCoroutine = StartCoroutine(WaitForCamera(lastWaitFrame));
            //}
            waitForCameraCoroutine = StartCoroutine(WaitForCamera());
            //namePlateController?.NamePlateUnit.RequestSnapshot();
        }

        public void InitializePosition() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.InitializePosition()");
            if (unitController != null) {
                cameraPositionOffset = unitController.NamePlateController.UnitFrameCameraPositionOffset;
            } else {
                cameraPositionOffset = cameraPositionOffsetDefault;
            }
            if (unitController.NamePlateController.UnitFrameCameraLookOffset != null) {
                cameraLookOffset = unitController.NamePlateController.UnitFrameCameraLookOffset;
            } else {
                cameraLookOffset = cameraLookOffsetDefault;
            }
        }

        public virtual void SetTarget(UnitController unitController) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.SetTarget({unitController.gameObject.name})");

            // prevent old target from still sending us updates while we are focused on a new target
            ClearTarget(false);

            if (!isActiveAndEnabled) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.SetTarget(" + target.name + "): controller is not active and enabled.  Activating");
                gameObject.SetActive(true);
            }

            InitializeController();
            this.unitController = unitController;

            CalculateResourceColors();
            UpdateLeaderIcon();
            if (unitController != null) {
                castBarController.SetTarget(unitController);
                statusEffectPanelController.SetTarget(unitController);
                if (unitController != playerManager.UnitController) {
                    systemEventManager.OnReputationChange += HandleReputationChange;
                }
            }

            if (isActiveAndEnabled) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.SetTarget(" + target.name + "):  WE ARE NOW ACTIVE AND ENABLED");
                TargetInitialization();
            } else {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.SetTarget(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.  Will run TargetInitialization() on enable instead.");
            }
            if (systemConfigurationManager.UIConfiguration.RealTimeUnitFrameCamera == true && previewCamera != null) {
                previewCamera.enabled = true;
            }/* else {
            // this code disabled because it is handled by TargetInitialization(), which results in an extra render request here
                //previewCamera.Render();
                lastWaitFrame++;
                //if (waitForCameraCoroutine == null) {
                    waitForCameraCoroutine = StartCoroutine(WaitForCamera(lastWaitFrame));
                //}
            }*/
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

            if (unitController?.NamePlateController != null) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.CalculateResourceColors(): found NamePlateController on unitController");
                if (unitController.NamePlateController.PowerResourceList.Count > 0) {
                    primaryPowerResource = unitController.NamePlateController.PowerResourceList[0];
                    powerResourceColor1 = unitController.NamePlateController.PowerResourceList[0].DisplayColor;
                } else {
                    primaryPowerResource = null;
                }
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.CalculateResourceColors(): {primaryPowerResource?.ResourceName}");
                if (unitController.NamePlateController.PowerResourceList.Count > 1) {
                    secondaryPowerResource = unitController.NamePlateController.PowerResourceList[1];
                    powerResourceColor2 = unitController.NamePlateController.PowerResourceList[1].DisplayColor;
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

        private IEnumerator WaitForCamera() {
            //private IEnumerator WaitForCamera(int frameNumber) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): " + namePlateController.Interactable.GetInstanceID() + "; frame: " + frameNumber);
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): " + namePlateController.Interactable.GetInstanceID());
            //yield return new WaitForEndOfFrame();
            yield return null;
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): about to render " + namePlateController.Interactable.GetInstanceID() + "; initial frame: " + frameNumber + "; current frame: " + lastWaitFrame);
            //if (lastWaitFrame != frameNumber) {
            if (unitController.IsBuilding() == true) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): a new wait was started. initial frame: " + frameNumber +  "; current wait: " + lastWaitFrame);
            } else {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForCamera(): rendering");
                UpdateCameraPosition();
                previewCamera.Render();
                waitForCameraCoroutine = null;
                //namePlateController?.Interactable.ClearSnapshotRequest();
            }
        }

        public void ClearTarget(bool closeWindowOnClear = true) {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.ClearTarget({closeWindowOnClear})");

            if (waitForCameraCoroutine != null) {
                StopCoroutine(waitForCameraCoroutine);
            }
            UnsubscribeFromTargetReady();

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
            if (previewCamera != null) {
                previewCamera.enabled = false;
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

            HandleReputationChange(playerManager.UnitController);
            //Debug.Log("Charcter name is " + baseCharacter.MyCharacterName);
            unitNameText.text = unitController.DisplayName;

            if (!unitController.NamePlateController.HasPrimaryResource()) {
                ClearPrimaryResourceBar();
            }

            if (!unitController.NamePlateController.HasSecondaryResource()) {
                ClearSecondaryResourceBar();
            }

            // set initial resource values in character display
            int counter = 0;
            foreach (PowerResource _powerResource in unitController.NamePlateController.PowerResourceList) {
                //Debug.Log($"{gameObject.name}.UnitFramePanelBase.InitializeStats(): Initializing resource: {_powerResource.ResourceName}");
                HandleResourceAmountChanged(_powerResource, (int)unitController.NamePlateController.GetPowerResourceMaxAmount(_powerResource), (int)unitController.NamePlateController.GetPowerResourceAmount(_powerResource));
                counter++;
                if (counter > 1) {
                    break;
                }
            }

            CreateSubscriptions();

            HandleLevelChanged(unitController.NamePlateController.Level);
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

        private void GetFollowTarget() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.WaitForFollowTarget()");
            Transform targetBone = unitController.NamePlateController.NamePlateUnit.transform;
            string unitFrameTarget = unitController.NamePlateController.UnitFrameTarget;
            //Debug.Log("Unit Frame: Searching for target: " + unitFrameTarget);
            if (unitFrameTarget != string.Empty) {
                if (unitController.gameObject != null) {
                    targetBone = unitController.transform.FindChildByRecursive(unitFrameTarget);
                    if (targetBone == null) {
                        Debug.LogWarning($"{gameObject.name}.UnitFramePanelBase.GetFollowTarget(): Could not find targetBone: {unitFrameTarget}");
                    }
                }
            }
            this.followTransform = targetBone;
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

            if (displayedCurrentResource <= 0 && unitController.NamePlateController.HasHealth() == true) {
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
            if (playerManager.UnitController?.CharacterStats != null) {
                unitLevelText.color = LevelEquations.GetTargetColor(playerManager.UnitController.CharacterStats.Level, _level);
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
            if (unitController?.NamePlateController != null) {
                int counter = 0;
                bool updateBar = false;
                foreach (PowerResource _powerResource in unitController.NamePlateController.PowerResourceList) {
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

            if (playerManager == null || playerManager.PlayerUnitSpawned == false) {
                return;
            }
            reputationColor = Faction.GetFactionColor(playerManager, unitController);
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

        public void OnDisable() {
            //Debug.Log($"{gameObject.name}.UnitFramePanelBase.OnDisable(): {GetInstanceID()}");

            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            UnsubscribeFromTargetReady();
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

            contextMenuService.ShowContextMenu(unitController, mousePosition);
        }

        protected virtual void HandleLeftClick(Vector2 mousePosition) {
            //Debug.Log($"UnitFrameController.HandleLeftClick({mousePosition})");
            // nothing here, overridden in derived classes
        }

    }
}