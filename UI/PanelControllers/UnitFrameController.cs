using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitFrameController : DraggableWindow {

        // objects in the player stats window

        [SerializeField]
        private TextMeshProUGUI unitNameText = null;

        [SerializeField]
        private TextMeshProUGUI unitLevelText = null;

        [SerializeField]
        private Image unitNameBackground = null;

        [SerializeField]
        private Image healthSlider = null;

        [SerializeField]
        private TextMeshProUGUI healthText = null;

        [SerializeField]
        private Image manaSlider = null;

        [SerializeField]
        private TextMeshProUGUI manaText = null;

        [SerializeField]
        private CastBarController castBarController = null;

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

        private float originalHealthSliderWidth = 0f;
        private float originalManaSliderWidth = 0f;

        [SerializeField]
        private GameObject followGameObject = null;

        [SerializeField]
        private StatusEffectPanelController statusEffectPanelController = null;

        [SerializeField]
        private bool realTimeCamera = false;

        private Transform followTransform = null;

        private bool controllerInitialized = false;
        private bool targetInitialized = false;

        public GameObject MyFollowGameObject { get => followGameObject; set => followGameObject = value; }


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
            originalHealthSliderWidth = healthSlider.GetComponent<LayoutElement>().preferredWidth;
            originalManaSliderWidth = manaSlider.GetComponent<LayoutElement>().preferredWidth;
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
            if (followGameObject == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".UnitFrameController.TargetInitialization() at beginning isactive: " + isActiveAndEnabled);
            InitializeStats();
            InitializePosition();
            //Debug.Log(gameObject.name + ".UnitFrameController.TargetInitialization() before setactive isactive: " + isActiveAndEnabled);
            gameObject.SetActive(true);
            //Debug.Log(gameObject.name + ".UnitFrameController.TargetInitialization() after setactve isactive: " + isActiveAndEnabled);
            if (isActiveAndEnabled) {
                GetFollowTarget();
            } else {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(): Unit Frame Not active after activate command.  Likely gameobject under inactive canvas.  Will run StartCoroutien() on enable instead.");
            }
        }

        public void InitializePosition() {
            //Debug.Log(gameObject.name + ".UnitFrameController.InitializePosition()");
            if (followGameObject.GetComponent<INamePlateUnit>().MyUnitFrameCameraPositionOffset != null) {
                cameraPositionOffset = followGameObject.GetComponent<INamePlateUnit>().MyUnitFrameCameraPositionOffset;
            } else {
                cameraPositionOffset = cameraPositionOffsetDefault;
            }
            if (followGameObject.GetComponent<INamePlateUnit>().MyUnitFrameCameraLookOffset != null) {
                cameraLookOffset = followGameObject.GetComponent<INamePlateUnit>().MyUnitFrameCameraLookOffset;
            } else {
                cameraLookOffset = cameraLookOffsetDefault;
            }
        }

        public void SetTarget(GameObject target) {
            //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(" + target.name + ")");

            // prevent old target from still sending us updates while we are focused on a new target
            ClearTarget(false);

            if (!isActiveAndEnabled) {
                //Debug.Log(gameObject.name + ".UnitFrameController.SetTarget(" + target.name + "): controller is not active and enabled.  Activating");
                gameObject.SetActive(true);
            }

            InitializeController();
            followGameObject = target;
            INamePlateUnit namePlateUnit = followGameObject.GetComponent<INamePlateUnit>();
            if (namePlateUnit.HasHealth()) {
                castBarController.SetTarget(target);
                statusEffectPanelController.SetTarget(namePlateUnit as CharacterUnit);
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

        private IEnumerator WaitForCamera() {
            yield return null;
            previewCamera.Render();
        }

        public void ClearTarget(bool closeWindowOnClear = true) {
            //Debug.Log(gameObject.name + ".UnitFrameController.ClearTarget()");
            if (followGameObject != null) {

                INamePlateUnit namePlateUnit = followGameObject.GetComponent<INamePlateUnit>();
                if (namePlateUnit is CharacterUnit) {
                    (namePlateUnit as CharacterUnit).MyCharacter.MyCharacterStats.OnHealthChanged -= OnHealthChanged;
                    (namePlateUnit as CharacterUnit).MyCharacter.MyCharacterStats.OnManaChanged -= OnManaChanged;
                    (namePlateUnit as CharacterUnit).MyCharacter.MyCharacterStats.OnLevelChanged -= OnLevelChanged;
                    (namePlateUnit as CharacterUnit).MyCharacter.MyCharacterFactionManager.OnReputationChange -= HandleReputationChange;
                }
            }
            followGameObject = null;
            targetInitialized = false;
            castBarController.ClearTarget();
            statusEffectPanelController.ClearTarget();
            if (closeWindowOnClear) {
                gameObject.SetActive(false);
            }
            previewCamera.enabled = false;
        }

        private void InitializeStats() {
            //Debug.Log(gameObject.name + ".UnitFrameController.InitializeStats()");

            INamePlateUnit namePlateUnit = followGameObject.GetComponent<INamePlateUnit>();

            if (namePlateUnit == null) {
                return;
            }

            HandleReputationChange();
            //Debug.Log("Charcter name is " + baseCharacter.MyCharacterName);
            unitNameText.text = namePlateUnit.MyDisplayName;

            if (namePlateUnit.HasHealth()) {
                BaseCharacter baseCharacter = followGameObject.GetComponent<CharacterUnit>().MyCharacter;
                if (baseCharacter.MyCharacterStats == null) {
                    //Debug.Log("UnitFrameController: followGameObject(" + followGameObject.name + ") does not have a BaseCharacter component");
                    return;
                }

                // set initial hp and mana values in character display
                OnHealthChanged(baseCharacter.MyCharacterStats.MyMaxHealth, baseCharacter.MyCharacterStats.currentHealth);
                OnManaChanged(baseCharacter.MyCharacterStats.MyMaxMana, baseCharacter.MyCharacterStats.currentMana);
                OnLevelChanged(baseCharacter.MyCharacterStats.MyLevel);

                // allow the character to send us events whenever the hp, mana, or cast time has changed so we can update the windows that display those values
                baseCharacter.MyCharacterStats.OnHealthChanged += OnHealthChanged;
                baseCharacter.MyCharacterStats.OnManaChanged += OnManaChanged;
                baseCharacter.MyCharacterStats.OnLevelChanged += OnLevelChanged;
                if (baseCharacter.MyCharacterFactionManager != null) {
                    baseCharacter.MyCharacterFactionManager.OnReputationChange += HandleReputationChange;
                } else {
                    Debug.LogError("UnitFrameController.InitializeStats(): baseCharacter: " + baseCharacter.name + " has no CharacterFactionManager");
                }

            } else {
                // manually set everything to 1 if this is an inanimate unit
                OnHealthChanged(1, 1);
                OnManaChanged(1, 1);
                OnLevelChanged(1);
            }
        }

        private void GetFollowTarget() {
            //Debug.Log(gameObject.name + ".UnitFrameController.WaitForFollowTarget()");
            Transform targetBone = followGameObject.transform;
            string unitFrameTarget = followGameObject.GetComponent<INamePlateUnit>().MyUnitFrameTarget;
            //Debug.Log("Unit Frame: Searching for target: " + unitFrameTarget);
            if (unitFrameTarget != string.Empty) {
                if (followGameObject != null) {
                    targetBone = followGameObject.transform.FindChildByRecursive(unitFrameTarget);
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

        public void OnHealthChanged(int maxHealth, int currentHealth) {

            // prevent division by zero
            int displayedMaxHealth = maxHealth;
            int displayedCurrentHealth = currentHealth;
            if (displayedMaxHealth == 0) {
                displayedMaxHealth = 1;
                displayedCurrentHealth = 1;
            }

            float healthPercent = (float)displayedCurrentHealth / displayedMaxHealth;
            //Debug.Log("UnitFrameController: setting healthSlider width to " + (healthPercent * originalHealthSliderWidth).ToString());

            // code for an actual image, not currently used
            //playerHPSlider.fillAmount = healthPercent;

            // code for the default image
            if (healthSlider != null) {
                healthSlider.GetComponent<LayoutElement>().preferredWidth = healthPercent * originalHealthSliderWidth;
            }
            if (healthText != null) {
                healthText.text = string.Format("{0} / {1} ({2}%)", displayedCurrentHealth, displayedMaxHealth, (healthPercent * 100).ToString("F0"));
            }
        }

        public void OnLevelChanged(int _level) {
            unitLevelText.text = _level.ToString();
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
                unitLevelText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, _level);
            }
        }

        public void OnManaChanged(int maxMana, int currentMana) {
            //Debug.Log("Updating mana bar");
            float manaPercent = (float)currentMana / maxMana;
            //Debug.Log("UnitFrameController: setting manaSlider width to " + (manaPercent * originalManaSliderWidth).ToString());

            // code for an actual image, not currently used
            //playerManaSlider.fillAmount = manaPercent;

            // code for the default image
            if (manaSlider != null) {
                manaSlider.GetComponent<LayoutElement>().preferredWidth = manaPercent * originalManaSliderWidth;
            }

            if (manaText != null) {
                manaText.text = string.Format("{0} / {1} ({2}%)", currentMana, maxMana, (manaPercent * 100).ToString("F0"));
            }
        }

        public void HandleReputationChange() {
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
                return;
            }
            if (followGameObject == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".UnitFrameController.OnReputationChange(): " + followGameObject.name);
            INamePlateUnit namePlateUnit = followGameObject.GetComponent<INamePlateUnit>();

            if (namePlateUnit == null) {
                return;
            }
            Color tmp = Faction.GetFactionColor(namePlateUnit);
            //Color tmp = Faction.GetFactionColor(baseCharacter.MyFaction);
            tmp.a = 0.5f;
            unitNameBackground.color = tmp;

        }

        public override void OnEnable() {
            //Debug.Log(gameObject.name + ".UnitFrameController.OnEnable()");
            base.OnEnable();
            // just in case something was targetted before the canvas became active
            TargetInitialization();
        }
    }

}