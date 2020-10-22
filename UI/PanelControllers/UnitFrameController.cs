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

        [Tooltip("If false, a single snapshot is taken of the unit, instead of a real-time video in the preview image")]
        [SerializeField]
        private bool realTimeCamera = false;

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

        private UnitNamePlateController unitNamePlateController = null;
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

        public UnitNamePlateController UnitNamePlateController { get => unitNamePlateController; set => unitNamePlateController = value; }

        //public GameObject FollowGameObject { get => followGameObject; set => followGameObject = value; }


        public override void Awake() {
            //Debug.Log(gameObject.name + ": UnitFrameController.Awake()");
            InitializeController();
            previewCamera.enabled = false;
        }

        protected void Start() {
            //Debug.Log(gameObject.name + ".UnitFrameController.Start()");
            InitializeController();
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

        private void Update() {
            if (!targetInitialized) {
                //Debug.Log("UnitFrameController.Update(). Not initialized yet.  Exiting.");
                return;
            }
            if (followTransform == null) {
                //Debug.Log(gameObject.name + "UnitFrameController.Update(). Follow transform is null.  Exiting.");
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
            if (unitNamePlateController == null) {
                return;
            }
            InitializeStats();
            InitializePosition();
            gameObject.SetActive(true);
            if (isActiveAndEnabled) {
                GetFollowTarget();
            } else {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.  Will run StartCoroutien() on enable instead.");
            }
        }

        public void InitializePosition() {
            //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition()");
            if (unitNamePlateController.UnitFrameCameraPositionOffset != null) {
                cameraPositionOffset = unitNamePlateController.UnitFrameCameraPositionOffset;
            } else {
                cameraPositionOffset = cameraPositionOffsetDefault;
            }
            if (unitNamePlateController.UnitFrameCameraLookOffset != null) {
                cameraLookOffset = unitNamePlateController.UnitFrameCameraLookOffset;
            } else {
                cameraLookOffset = cameraLookOffsetDefault;
            }
        }

        public void SetTarget(UnitNamePlateController unitNamePlateController) {
            //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(" + target.name + ")");

            // prevent old target from still sending us updates while we are focused on a new target
            ClearTarget(false);

            if (!isActiveAndEnabled) {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(" + target.name + "): controller is not active and enabled.  Activating");
                gameObject.SetActive(true);
            }

            InitializeController();
            this.unitNamePlateController = unitNamePlateController;

            CalculateResourceColors();
            if (unitNamePlateController.HasHealth()) {
                castBarController.SetTarget(unitNamePlateController);
                statusEffectPanelController.SetTarget(unitNamePlateController.UnitController.BaseCharacter.CharacterUnit);
            }

            if (isActiveAndEnabled) {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(" + target.name + "):  WE ARE NOW ACTIVE AND ENABLED");
                TargetInitialization();
            } else {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.  Will run TargetInitialization() on enable instead.");
            }
            if (realTimeCamera == true) {
                previewCamera.enabled = true;
            } else {
                //previewCamera.Render();
                StartCoroutine(WaitForCamera());
            }
        }

        public void CalculateResourceColors() {
            if (unitNamePlateController != null && unitNamePlateController.UnitController.BaseCharacter != null && unitNamePlateController.UnitController.BaseCharacter.CharacterStats != null) {
                if (unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList.Count > 0) {
                    primaryPowerResource = unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList[0];
                    powerResourceColor1 = unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList[0].DisplayColor;
                } else {
                    primaryPowerResource = null;
                }
                if (unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList.Count > 1) {
                    secondaryPowerResource = unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList[1];
                    powerResourceColor2 = unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList[1].DisplayColor;
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
            yield return null;
            previewCamera.Render();
        }

        public void ClearTarget(bool closeWindowOnClear = true) {
            //Debug.Log(gameObject.name + ".UnitFrameController.ClearTarget()");

            if (unitNamePlateController != null && unitNamePlateController.UnitController != null) {
                unitNamePlateController.UnitController.BaseCharacter.CharacterStats.OnResourceAmountChanged -= HandleResourceAmountChanged;
                unitNamePlateController.UnitController.BaseCharacter.OnNameChange -= HandleNameChange;
                unitNamePlateController.UnitController.BaseCharacter.OnClassChange -= HandleClassChange;
                unitNamePlateController.UnitController.BaseCharacter.CharacterStats.OnLevelChanged -= HandleLevelChanged;
                unitNamePlateController.UnitController.BaseCharacter.CharacterStats.OnReviveComplete -= HandleReviveComplete;
                unitNamePlateController.UnitController.BaseCharacter.CharacterFactionManager.OnReputationChange -= HandleReputationChange;
            }
            unitNamePlateController = null;
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
            unitNameText.text = unitNamePlateController.UnitDisplayName;

            if (!unitNamePlateController.HasPrimaryResource()) {
                ClearPrimaryResourceBar();
            }

            if (!unitNamePlateController.HasSecondaryResource()) {
                ClearSecondaryResourceBar();
            }

            if (unitNamePlateController.UnitController == null || unitNamePlateController.UnitController.BaseCharacter == null || unitNamePlateController.UnitController.BaseCharacter.CharacterStats == null) {
                //Debug.Log("UnitFrameController: followGameObject(" + followGameObject.name + ") does not have a BaseCharacter component");
                return;
            }

            // set initial resource values in character display
            int counter = 0;
            if (unitNamePlateController.UnitController.BaseCharacter.CharacterStats != null) {
                foreach (PowerResource _powerResource in unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList) {
                    HandleResourceAmountChanged(_powerResource, (int)unitNamePlateController.UnitController.BaseCharacter.CharacterStats.GetPowerResourceMaxAmount(_powerResource), (int)unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceDictionary[_powerResource].currentValue);
                    counter++;
                    if (counter > 1) {
                        break;
                    }
                }
            }

            // allow the character to send us events whenever the hp, mana, or cast time has changed so we can update the windows that display those values
            unitNamePlateController.UnitController.BaseCharacter.CharacterStats.OnResourceAmountChanged += HandleResourceAmountChanged;
            unitNamePlateController.UnitController.BaseCharacter.OnNameChange += HandleNameChange;
            unitNamePlateController.UnitController.BaseCharacter.CharacterStats.OnLevelChanged += HandleLevelChanged;
            unitNamePlateController.UnitController.BaseCharacter.CharacterStats.OnReviveComplete += HandleReviveComplete;
            unitNamePlateController.UnitController.BaseCharacter.OnClassChange += HandleClassChange;

            if (unitNamePlateController.UnitController.BaseCharacter.CharacterFactionManager != null) {
                unitNamePlateController.UnitController.BaseCharacter.CharacterFactionManager.OnReputationChange += HandleReputationChange;
            } else {
                Debug.LogError("UnitFrameController.InitializeStats(): baseCharacter: " + unitNamePlateController.UnitController.BaseCharacter.name + " has no CharacterFactionManager");
            }


            HandleLevelChanged(unitNamePlateController.Level);
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
            Transform targetBone = unitNamePlateController.UnitController.transform;
            string unitFrameTarget = unitNamePlateController.UnitController.NamePlateController.UnitFrameTarget;
            //Debug.Log("Unit Frame: Searching for target: " + unitFrameTarget);
            if (unitFrameTarget != string.Empty) {
                if (unitNamePlateController.UnitController.gameObject != null) {
                    targetBone = unitNamePlateController.UnitController.gameObject.transform.FindChildByRecursive(unitFrameTarget);
                    if (targetBone == null) {
                        Debug.LogWarning(gameObject.name + ".UnitFrameController.GetFollowTarget(): Could not find targetBone: " + unitFrameTarget);
                    }
                }
            }
            this.followTransform = targetBone;
            targetInitialized = true;
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

            if (displayedCurrentResource <= 0) {
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
            if (unitNamePlateController.UnitController != null && unitNamePlateController.UnitController.BaseCharacter != null && unitNamePlateController.UnitController.BaseCharacter.CharacterStats != null) {
                int counter = 0;
                bool updateBar = false;
                foreach (PowerResource _powerResource in unitNamePlateController.UnitController.BaseCharacter.CharacterStats.PowerResourceList) {
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
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                return;
            }
            if (unitNamePlateController == null) {
                return;
            }
            reputationColor = Faction.GetFactionColor(unitNamePlateController.UnitController);
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
    }

}