using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitFrameController : DraggableWindow {

        [Header("Unit Name")]

        [SerializeField]
        private TextMeshProUGUI unitNameText = null;

        [SerializeField]
        private TextMeshProUGUI unitLevelText = null;

        [SerializeField]
        private Image unitNameBackground = null;

        [Header("Resources")]

        [FormerlySerializedAs("healthSlider")]
        [SerializeField]
        private Image primaryResourceSlider = null;

        [FormerlySerializedAs("healthText")]
        [SerializeField]
        private TextMeshProUGUI primaryResourceText = null;

        [FormerlySerializedAs("manaSlider")]
        [SerializeField]
        private Image secondaryResourceSlider = null;

        [FormerlySerializedAs("manaText")]
        [SerializeField]
        private TextMeshProUGUI secondaryResourceText = null;

        [Header("Cast Bar")]

        [SerializeField]
        private CastBarController castBarController = null;

        [Header("Unit Preview")]

        // the next 2 things need to be updated to focus on the right character
        [SerializeField]
        private Transform cameraTransform = null;

        // replaces cameraTransform;
        [SerializeField]
        private Camera previewCamera = null;

        [SerializeField]
        private Texture portraitTexture = null;

        [SerializeField]
        private RawImage portraitImage = null;

        [SerializeField]
        private Vector3 cameraLookOffsetDefault = new Vector3(0, 1.6f, 0);

        [SerializeField]
        private Vector3 cameraPositionOffsetDefault = new Vector3(0, 1.6f, 0.66f);

        private Vector3 cameraLookOffset = Vector3.zero;

        private Vector3 cameraPositionOffset = Vector3.zero;

        private float originalPrimaryResourceSliderWidth = 0f;
        private float originalSecondaryResourceSliderWidth = 0f;

        //private GameObject followGameObject = null;

        private BaseNamePlateController namePlateController = null;
        //private CharacterUnit characterUnit = null;

        [Header("Status Effects")]

        [SerializeField]
        private StatusEffectPanelController statusEffectPanelController = null;

        private Transform followTransform = null;

        private PowerResource primaryPowerResource = null;
        private PowerResource secondaryPowerResource = null;

        private LayoutElement primaryResourceSliderLayout = null;
        private LayoutElement secondaryResourceSliderLayout = null;

        private Color powerResourceColor1 = Color.green;
        private Color powerResourceColor2 = Color.blue;

        private bool controllerInitialized = false;
        private bool targetInitialized = false;

        Color reputationColor;

        public BaseNamePlateController UnitNamePlateController { get => namePlateController; set => namePlateController = value; }

        //public GameObject FollowGameObject { get => followGameObject; set => followGameObject = value; }


        public override void Awake() {
            //Debug.Log(gameObject.name + ": UnitFrameController.Awake()");
            InitializeController();
            previewCamera.enabled = false;
        }

        protected void Start() {
            //Debug.Log(gameObject.name + ".UnitFrameController.Start()");
            //InitializeController();
            if (!targetInitialized) {
                this.gameObject.SetActive(false);
            }
            if (statusEffectPanelController != null) {
                statusEffectPanelController.MyEffectLimit = 7;
            }
        }

        public void InitializeController() {
            //Debug.Log(gameObject.name + ".UnitFrameController.InitializeController()");
            if (controllerInitialized) {
                return;
            }
            portraitImage.texture = portraitTexture;
            if (primaryResourceSliderLayout == null) {
                primaryResourceSliderLayout = primaryResourceSlider.GetComponent<LayoutElement>();
            }
            if (secondaryResourceSliderLayout == null) {
                secondaryResourceSliderLayout = secondaryResourceSlider.GetComponent<LayoutElement>();
            }
            originalPrimaryResourceSliderWidth = primaryResourceSliderLayout.preferredWidth;
            originalSecondaryResourceSliderWidth = secondaryResourceSliderLayout.preferredWidth;
            DeActivateCastBar();
            controllerInitialized = true;
            //Debug.Log(gameObject.name + ": UnitFrameController.Awake() originalHealthSliderWidth: " + originalHealthSliderWidth);
        }

        private void LateUpdate() {
            if (SystemConfigurationManager.MyInstance.RealTimeUnitFrameCamera) {
                UpdateCameraPosition();
            }
        }

        private void UpdateCameraPosition() {
            if (!targetInitialized || UnitNamePlateController?.NamePlateUnit?.CameraTargetReady == false) {
                //Debug.Log("UnitFrameController.Update(). Not initialized yet.  Exiting.");
                return;
            }
            if (UnitNamePlateController?.NamePlateUnit?.CameraTargetReady == true && followTransform == null) {
                //Debug.Log(gameObject.name + "UnitFrameController.Update(). Follow transform is null. possibly dead unit despawned. Exiting.");
                ClearTarget();
                return;
            }

            if (cameraTransform != null) {
                //Vector3 wantedPosition = followTransform.TransformPoint(0, offsetY, offsetZ);
                //Vector3 wantedLookPosition = followTransform.TransformPoint(0, offsetY, 0);
                Vector3 wantedPosition = followTransform.TransformPoint(cameraPositionOffset);
                Vector3 wantedLookPosition = followTransform.TransformPoint(cameraLookOffset);
                cameraTransform.position = wantedPosition;
                cameraTransform.LookAt(wantedLookPosition);

            } else {
            }
        }

        private void TargetInitialization() {
            if (namePlateController == null) {
                return;
            }
            InitializeStats();
            InitializePosition();
            gameObject.SetActive(true);
            targetInitialized = true;
            if (isActiveAndEnabled) {
                if (namePlateController.NamePlateUnit.CameraTargetReady) {
                    HandleTargetReady();
                }// else {
                // testing subscribe no matter what in case unit appearance changes
                    SubscribeToTargetReady();
                //}
            } else {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.  Will run StartCoroutien() on enable instead.");
            }
        }

        public void SubscribeToTargetReady() {
            namePlateController.NamePlateUnit.OnCameraTargetReady += HandleTargetReady;
        }

        public void UnsubscribeFromTargetReady() {
            if (namePlateController?.NamePlateUnit != null) {
                namePlateController.NamePlateUnit.OnCameraTargetReady -= HandleTargetReady;
            }
        }

        public void HandleTargetReady() {
            //UnsubscribeFromTargetReady();
            GetFollowTarget();
            UpdateCameraPosition();
            StartCoroutine(WaitForCamera());
        }

        public void InitializePosition() {
            //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition()");
            if (namePlateController.UnitFrameCameraPositionOffset != null) {
                cameraPositionOffset = namePlateController.UnitFrameCameraPositionOffset;
            } else {
                cameraPositionOffset = cameraPositionOffsetDefault;
            }
            if (namePlateController.UnitFrameCameraLookOffset != null) {
                cameraLookOffset = namePlateController.UnitFrameCameraLookOffset;
            } else {
                cameraLookOffset = cameraLookOffsetDefault;
            }
        }

        public void SetTarget(BaseNamePlateController namePlateController) {
            //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget()");

            // prevent old target from still sending us updates while we are focused on a new target
            ClearTarget(false);

            if (!isActiveAndEnabled) {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(" + target.name + "): controller is not active and enabled.  Activating");
                gameObject.SetActive(true);
            }

            InitializeController();
            this.namePlateController = namePlateController;

            CalculateResourceColors();
            if (namePlateController.Interactable.CharacterUnit != null) {
                castBarController.SetTarget(namePlateController as UnitNamePlateController);
                statusEffectPanelController.SetTarget((namePlateController as UnitNamePlateController).UnitController);
            }

            if (isActiveAndEnabled) {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(" + target.name + "):  WE ARE NOW ACTIVE AND ENABLED");
                TargetInitialization();
            } else {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.  Will run TargetInitialization() on enable instead.");
            }
            if (SystemConfigurationManager.MyInstance.RealTimeUnitFrameCamera == true) {
                previewCamera.enabled = true;
            } else {
                //previewCamera.Render();
                StartCoroutine(WaitForCamera());
            }
        }

        public void CalculateResourceColors() {
            if (namePlateController != null) {
                if (namePlateController.PowerResourceList.Count > 0) {
                    primaryPowerResource = namePlateController.PowerResourceList[0];
                    powerResourceColor1 = namePlateController.PowerResourceList[0].DisplayColor;
                } else {
                    primaryPowerResource = null;
                }
                if (namePlateController.PowerResourceList.Count > 1) {
                    secondaryPowerResource = namePlateController.PowerResourceList[1];
                    powerResourceColor2 = namePlateController.PowerResourceList[1].DisplayColor;
                } else {
                    secondaryPowerResource = null;
                }
            }

            if (primaryResourceSlider.color != powerResourceColor1) {
                primaryResourceSlider.color = powerResourceColor1;
            }
            if (secondaryResourceSlider.color != powerResourceColor2) {
                secondaryResourceSlider.color = powerResourceColor2;
            }


        }

        private IEnumerator WaitForCamera() {
            //yield return new WaitForEndOfFrame();
            yield return null;
            previewCamera.Render();
        }

        public void ClearTarget(bool closeWindowOnClear = true) {
            //Debug.Log(gameObject.name + ".UnitFrameController.ClearTarget()");
            UnsubscribeFromTargetReady();

            if (namePlateController != null && namePlateController.Interactable.CharacterUnit != null) {
                (namePlateController as UnitNamePlateController).UnitController.OnResourceAmountChanged -= HandleResourceAmountChanged;
                (namePlateController as UnitNamePlateController).UnitController.OnNameChange -= HandleNameChange;
                (namePlateController as UnitNamePlateController).UnitController.OnClassChange -= HandleClassChange;
                (namePlateController as UnitNamePlateController).UnitController.OnLevelChanged -= HandleLevelChanged;
                (namePlateController as UnitNamePlateController).UnitController.OnReviveComplete -= HandleReviveComplete;
                (namePlateController as UnitNamePlateController).UnitController.OnReputationChange -= HandleReputationChange;
            }
            namePlateController = null;
            targetInitialized = false;
            castBarController.ClearTarget();
            statusEffectPanelController.ClearTarget();
            primaryPowerResource = null;
            secondaryPowerResource = null;
            if (closeWindowOnClear) {
                gameObject.SetActive(false);
            }
            previewCamera.enabled = false;
        }

        public void HandleReviveComplete() {
            HandleReputationChange();
        }

        private void InitializeStats() {
            //Debug.Log(gameObject.name + ".UnitFrameController.InitializeStats()");

            HandleReputationChange();
            //Debug.Log("Charcter name is " + baseCharacter.MyCharacterName);
            unitNameText.text = namePlateController.UnitDisplayName;

            if (!namePlateController.HasPrimaryResource()) {
                ClearPrimaryResourceBar();
            }

            if (!namePlateController.HasSecondaryResource()) {
                ClearSecondaryResourceBar();
            }

            // set initial resource values in character display
            int counter = 0;
                foreach (PowerResource _powerResource in namePlateController.PowerResourceList) {
                    HandleResourceAmountChanged(_powerResource, (int)namePlateController.GetPowerResourceMaxAmount(_powerResource), (int)namePlateController.GetPowerResourceAmount(_powerResource));
                    counter++;
                    if (counter > 1) {
                        break;
                    }
                }

            // allow the character to send us events whenever the hp, mana, or cast time has changed so we can update the windows that display those values
            if ((namePlateController as UnitNamePlateController) is UnitNamePlateController) {
                (namePlateController as UnitNamePlateController).UnitController.OnResourceAmountChanged += HandleResourceAmountChanged;
                (namePlateController as UnitNamePlateController).UnitController.OnNameChange += HandleNameChange;
                (namePlateController as UnitNamePlateController).UnitController.OnLevelChanged += HandleLevelChanged;
                (namePlateController as UnitNamePlateController).UnitController.OnReviveComplete += HandleReviveComplete;
                (namePlateController as UnitNamePlateController).UnitController.OnClassChange += HandleClassChange;
                (namePlateController as UnitNamePlateController).UnitController.OnReputationChange += HandleReputationChange;
            }

            HandleLevelChanged(namePlateController.Level);
        }

        public void HandleClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
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
            //Debug.Log(gameObject.name + ".UnitFrameController.WaitForFollowTarget()");
            Transform targetBone = namePlateController.NamePlateUnit.transform;
            string unitFrameTarget = namePlateController.UnitFrameTarget;
            //Debug.Log("Unit Frame: Searching for target: " + unitFrameTarget);
            if (unitFrameTarget != string.Empty) {
                if (namePlateController.NamePlateUnit.gameObject != null) {
                    targetBone = namePlateController.NamePlateUnit.gameObject.transform.FindChildByRecursive(unitFrameTarget);
                    if (targetBone == null) {
                        Debug.LogWarning(gameObject.name + ".UnitFrameController.GetFollowTarget(): Could not find targetBone: " + unitFrameTarget);
                    }
                }
            }
            this.followTransform = targetBone;
        }

        private void DeActivateCastBar() {
            castBarController.ClearTarget();
        }

        public void HandlePrimaryResourceAmountChanged(int maxResourceAmount, int currentResourceAmount) {
            // Debug.Log(gameObject.name + ".UnitFrameController.HandlePrimaryResourceAmountChanged(" + maxResourceAmount + ", " + currentResourceAmount + ")");

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

            if (displayedCurrentResource <= 0 && namePlateController.HasHealth() == true) {
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

        public void HandleLevelChanged(int _level) {
            CalculateResourceColors();
            unitLevelText.text = _level.ToString();
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterStats != null) {
                unitLevelText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level, _level);
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
            //Debug.Log(gameObject.name + ".UnitFrameController.HandleResourceAmountChanged()");
            if (namePlateController != null) {
                int counter = 0;
                bool updateBar = false;
                foreach (PowerResource _powerResource in namePlateController.PowerResourceList) {
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

        public void HandleReputationChange() {
            if (PlayerManager.MyInstance == null || PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                return;
            }
            if (namePlateController == null) {
                return;
            }
            reputationColor = Faction.GetFactionColor(namePlateController.NamePlateUnit);
            //Color tmp = Faction.GetFactionColor(baseCharacter.MyFaction);
            reputationColor.a = 0.5f;
            unitNameBackground.color = reputationColor;

        }

        public override void OnEnable() {
            //Debug.Log(gameObject.name + ".UnitFrameController.OnEnable()");
            base.OnEnable();
            // just in case something was targetted before the canvas became active
            TargetInitialization();
        }

        public override void OnDisable() {
            //Debug.Log(gameObject.name + ".UnitFrameController.OnDisable()");
            base.OnDisable();
            UnsubscribeFromTargetReady();
        }
    }

}