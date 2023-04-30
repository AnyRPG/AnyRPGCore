using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class NamePlateController : ConfiguredMonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField]
        private GameObject healthBar = null;

        [SerializeField]
        private Image healthBarOutline = null;

        [SerializeField]
        private LayoutElement healthBarBackground = null;

        [SerializeField]
        private Image healthSlider = null;

        [SerializeField]
        private LayoutElement sliderLayoutElement = null;

        [SerializeField]
        private TextMeshProUGUI characterName = null;

        [SerializeField]
        private TextMeshProUGUI questIndicator = null;

        [SerializeField]
        private GameObject questIndicatorBackground = null;

        [SerializeField]
        private float positionOffset = 2.3f;

        [SerializeField]
        private Image genericIndicatorImage = null;

        [SerializeField]
        private GameObject speechBubbleBackground = null;

        [SerializeField]
        private TextMeshProUGUI speechBubbleText = null;

        [SerializeField]
        private NamePlateCanvasController namePlateCanvasController = null;

        [SerializeField]
        private CanvasGroup namePlateCanvasGroup = null;

        [SerializeField]
        private CanvasGroup speechBubbleCanvasGroup = null;

        [SerializeField]
        private Transform namePlateContents = null;

        [SerializeField]
        private Transform speechBubbleContents = null;

        private BaseNamePlateController unitNamePlateController = null;

        private bool isPlayerUnitNamePlate = false;

        private bool localComponentsInitialized = false;

        protected bool eventSubscriptionsInitialized = false;
        protected bool playerEventSubscriptionsInitialized = false;

        public Image HealthSlider { get => healthSlider; }
        public GameObject HealthBar { get => healthBar; }
        public TextMeshProUGUI CharacterName { get => characterName; set => characterName = value; }
        public TextMeshProUGUI QuestIndicator { get => questIndicator; }
        public GameObject QuestIndicatorBackground { get => questIndicatorBackground; set => questIndicatorBackground = value; }
        public Image GenericIndicatorImage { get => genericIndicatorImage; set => genericIndicatorImage = value; }
        public NamePlateCanvasController NamePlateCanvasController { get => namePlateCanvasController; set => namePlateCanvasController = value; }

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private CameraManager cameraManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            cameraManager = systemGameManager.CameraManager;
        }

        private void OnEnable() {
            //Debug.Log("NamePlateController.OnEnable()");
            /*
            CreateEventSubscriptions();
            HideSpeechBubble();
            */
        }

        private void CreateEventSubscriptions() {
            //Debug.Log($"{unitNamePlateController.UnitDisplayName}.NamePlateController.CreateEventSubscriptions()");

            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            //if (playerManager.MyPlayerUnitSpawned) {
            ProcessPlayerUnitSpawn();
            //}
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log($"{unitNamePlateController.UnitDisplayName}.NamePlateController.CleanupEventSubscriptions()");

            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);

            eventSubscriptionsInitialized = false;
        }

        private void CreatePlayerEventSubscriptions() {
            if (playerEventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnReputationChange", HandleReputationChange);
            playerEventSubscriptionsInitialized = true;
        }

        private void CleanupPlayerEventSubscriptions() {
            if (!playerEventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnReputationChange", HandleReputationChange);
            playerEventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{unitNamePlateController.UnitDisplayName}.NamePlateController.HandlePlayerUnitDespawn()");

            CleanupPlayerEventSubscriptions();
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{unitNamePlateController.UnitDisplayName}.NamePlateController.HandlePlayerUnitSpawn()");

            ProcessPlayerUnitSpawn();
        }


        public void ProcessPlayerUnitSpawn() {
            CreatePlayerEventSubscriptions();
            SetFactionColor();
        }

        public void HandleReputationChange(string eventName, EventParamProperties eventParam) {
            //Debug.Log($"{unitNamePlateController.UnitDisplayName}.NamePlateController.HandleReputationChange()");

            SetFactionColor();
        }

        private void InitializeLocalComponents() {
            //Debug.Log(unitNamePlateController.UnitDisplayName + "NamePlateController.InitializeLocalComponents()");
            if (localComponentsInitialized == true) {
                //Debug.Log(namePlateUnit.DisplayName + "NamePlateController.InitializeLocalComponents(): already done.  exiting!");

                return;
            }

            //Debug.Log(namePlateUnit.DisplayName + "NamePlateController.InitializeLocalComponents(): healthSliderWidth: " + healthSliderWidth);
            QuestIndicatorBackground.SetActive(false);
            GenericIndicatorImage.gameObject.SetActive(false);

            // ensure we update the nameplate color when the player spawns
            SetFactionColor();
            namePlateCanvasGroup.blocksRaycasts = true;
            //Debug.Log("NamePlateController: PlayerUnitSpawned: " + playerManager.MyPlayerUnitSpawned);

            localComponentsInitialized = true;
        }

        public void SetPlayerOwnerShip() {
            //Debug.Log("NamePlateController.SetPlayerOwnerShip()");
            namePlateCanvasGroup.blocksRaycasts = false;
            uIManager.SetLayerRecursive(gameObject, LayerMask.NameToLayer("Ignore Raycast"));
            isPlayerUnitNamePlate = true;
            CheckForDisabledHealthBar();
        }

        public void Highlight() {
            healthBarOutline.color = Color.white;
            transform.SetAsLastSibling();
        }

        public void UnHighlight(bool setAsFirstSibling = true) {
            healthBarOutline.color = Color.black;
            if (setAsFirstSibling) {
                transform.SetAsFirstSibling();
            }
        }

        private void SetCharacterName() {
            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName()");
            if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerName") == 0 && PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): ShowPlayerName and ShowPlayerFaction are both set to zero, setting charactername.text to string.empty");
                CharacterName.text = string.Empty;
                return;
            }

            if (CharacterName != null && unitNamePlateController != null) {
                if (CharacterName.text != null) {
                    // character names have special coloring. white if no faction, green if character, otherwise normal faction colors
                    if (unitNamePlateController.Faction != null) {
                        //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName);
                        Color textColor;
                        if (uIManager.CutSceneBarController.CurrentCutscene != null && uIManager.CutSceneBarController.CurrentCutscene.UseDefaultFactionColors == true) {
                            if (unitNamePlateController.Faction != null) {
                                //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: USING DEFAULT");
                                textColor = unitNamePlateController.Faction.GetFactionColor();
                            } else {
                                //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: USING UNIT");
                                textColor = Faction.GetFactionColor(playerManager, unitNamePlateController.NamePlateUnit);
                            }
                        } else {
                            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: USING UNIT");
                            textColor = Faction.GetFactionColor(playerManager, unitNamePlateController.NamePlateUnit);
                        }
                        //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: " + ColorUtility.ToHtmlStringRGB(textColor));


                        string nameString = string.Empty;
                        string factionString = string.Empty;
                        if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerName") == 0) {
                            // nothing for now
                            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): not showing name");
                        } else {
                            nameString = unitNamePlateController.UnitDisplayName;
                            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): showing name");
                        }
                        if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): not showing faction");
                        } else {
                            if (unitNamePlateController.SuppressFaction == false) {
                                factionString = "<" + unitNamePlateController.Faction.DisplayName + ">";
                            }
                            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): showing faction");
                        }
                        if (unitNamePlateController.Title != string.Empty) {
                            factionString = "<" + unitNamePlateController.Title + ">";
                        }
                        string newLineString = string.Empty;
                        if (factionString != string.Empty && nameString != string.Empty) {
                            newLineString = "\n";
                            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): faction nad name are both not empty");
                        } else {
                            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetCharacterName(): faction or name are empty");
                        }
                        CharacterName.text = string.Format("<color=#{0}>{1}{2}{3}</color>", ColorUtility.ToHtmlStringRGB(textColor), nameString, newLineString, factionString);
                    } else {
                        //Debug.Log("NamePlateController.SetCharacterName(): namePlateUnit has no faction!");
                        Color textColor;
                        if (playerManager.UnitController != null && unitNamePlateController.NamePlateUnit.gameObject == playerManager.UnitController.gameObject) {
                            textColor = Color.green;
                        } else {
                            textColor = Color.white;
                        }
                        if (PlayerPrefs.GetInt("ShowPlayerName") == 0 && isPlayerUnitNamePlate) {
                            // nothing
                        } else {
                            CharacterName.text = string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(textColor), unitNamePlateController.UnitDisplayName);
                        }
                    }
                } else {
                    //Debug.Log("NamePlateController.SetCharacterName(): text field is null!");
                }
            } else {
                //Debug.Log("NamePlateController.SetCharacterName(): character name is null!");
            }
        }

        public void SetNamePlateUnit(NamePlateUnit namePlateUnit, bool usePositionOffset) {
            //Debug.Log("NamePlateController.SetNamePlateUnit(" + namePlateUnit.DisplayName + ") setting namePlateUnit on nameplate in instanceid" + GetInstanceID().ToString());
            // moved code here from awake since a nameplate always has to be initialized so this method will always be called before anything else
            CreateEventSubscriptions();
            HideSpeechBubble();

            unitNamePlateController = namePlateUnit.NamePlateController;
            if (usePositionOffset == false) {
                positionOffset = 0f;
            }

            InitializeLocalComponents();

            // testing - does this prevent nameplate blinks on bottom of screen as units spawn
            UpdatePosition();
        }

        public void HandleTitleChange(string newTitle) {
            SetCharacterName();
        }

        public void HandleNameChange(string newName) {
            SetCharacterName();
        }

        /// <summary>
        /// because name plates only show health, filter resource updates to health only
        /// </summary>
        /// <param name="powerResource"></param>
        /// <param name="currentHealth"></param>
        /// <param name="maxHealth"></param>
        public void HandleResourceAmountChanged(PowerResource powerResource, int currentHealth, int maxHealth) {
            //Debug.Log(unitNamePlateController.NamePlateUnit.gameObject.name + ".CharacterUnit.HandleResourceAmountChanged(" + currentHealth + ", " + maxHealth + ")");
            if (unitNamePlateController.HasHealth()
                && (unitNamePlateController as UnitNamePlateController).UnitController.CharacterUnit.BaseCharacter != null
                && (unitNamePlateController as UnitNamePlateController).UnitController.CharacterUnit.BaseCharacter.CharacterStats != null
                && (unitNamePlateController as UnitNamePlateController).UnitController.CharacterUnit.BaseCharacter.CharacterStats.PrimaryResource == powerResource) {
                ProcessHealthChanged(currentHealth, maxHealth);
            }
        }

        public void HandleReputationChange() {
            SetFactionColor();
        }

        public void CheckForDisabledHealthBar() {
            //Debug.Log(unitNamePlateController.UnitDisplayName + ".NamePlateController.CheckForDisableHealthBar()");
            if (unitNamePlateController.HasHealth() && isPlayerUnitNamePlate) {
                //Debug.Log("CheckForDisableHealthBar() THIS IS THE PLAYER UNIT NAMEPLATE.  CHECK IF MAX HEALTH: ");
                if (playerManager != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterStats != null) {
                    //Debug.Log("CheckForDisableHealthBar() THIS IS THE PLAYER UNIT NAMEPLATE.  ABOUT TO CHECK PRIMARY RESOURCE: hidebar: " + PlayerPrefs.GetInt("HideFullHealthBar") + " current: " + playerManager.MyCharacter.CharacterStats.CurrentPrimaryResource + "; max: " + playerManager.MyCharacter.CharacterStats.MaxPrimaryResource);
                    if (playerManager.MyCharacter.CharacterStats.CurrentPrimaryResource == playerManager.MyCharacter.CharacterStats.MaxPrimaryResource && PlayerPrefs.GetInt("HideFullHealthBar") == 1) {
                        DisableHealthBar();
                        return;
                    }
                } else {

                }
            }
            if (unitNamePlateController.HasHealth()) {
                EnableHealthBar();
            } else {
                DisableHealthBar();
            }
        }

        public void DisableHealthBar() {
            //Debug.Log(MyCharacterName.text + ".NamePlateController.DisableHealthBar()");
            if (healthBar.activeSelf) {
                healthBar.SetActive(false);
            }
        }

        public void EnableHealthBar() {
            //Debug.Log(MyCharacterName.text + ".NamePlateController.EnableHealthBar()");
            if (!healthBar.activeSelf) {
                healthBar.SetActive(true);
            }
        }

        public void ProcessHealthChanged(int maxHealth, int currentHealth) {
            //Debug.Log(unitNamePlateController.NamePlateUnit.gameObject.name + ".NamePlateController.ProcessHealthChanged(" + maxHealth + ", " + currentHealth + ")");
            float healthPercent = (float)currentHealth / maxHealth;
            //Debug.Log(MyCharacterName.text + ".NamePlateController.OnHealthChanged(" + maxHealth + ", " + currentHealth + "): healthsliderwidth: " + healthSliderWidth.ToString() + "; healthPercent: " + healthPercent.ToString());
            if (HealthSlider == null) {
                //Debug.Log("NamePlateController.OnHealthChanged() MyHealthSlider == null!");
                return;
            }
            if (sliderLayoutElement == null) {
                //Debug.Log("NamePlateController.OnHealthChanged() MyHealthSlider.layoutElement == null!");
                return;
            }
            sliderLayoutElement.preferredWidth = healthPercent * healthBarBackground.preferredWidth;
            CheckForDisabledHealthBar();
        }

        //private void Update() {
        //private void LateUpdate() {
        public void UpdatePosition() {
            //Debug.Log("NamePlateController.UpdatePosition(): frame " + Time.frameCount + "; " + unitNamePlateController.UnitDisplayName);
            if (unitNamePlateController != null
                && (playerManager.UnitController != null || uIManager.CutSceneBarController.CurrentCutscene != null)) {
                //Debug.Log("Setting the position of the nameplate transform in lateupdate");
                bool renderNamePlate = true;
                //Debug.Log("NamePlateController.LateUpdate(): the position of the character is " + characterUnit.transform.position);
                Camera currentCamera;
                if (uIManager.CutSceneBarController.CurrentCutscene != null) {
                    if (cameraManager.CurrentCutsceneCameraController == null) {
                        return;
                    }
                    currentCamera = cameraManager.CurrentCutsceneCameraController.Camera;
                } else {
                    currentCamera = cameraManager.ActiveMainCamera;
                }
                if (currentCamera == null) {
                    return;
                }
                //Debug.Log("NamePlateController.LateUpdate(): namePlateUnit: " + (namePlateUnit as MonoBehaviour).gameObject.name + "; currentcamera: " + (currentCamera == null ? "null" : currentCamera.name));
                Vector3 relativePosition = currentCamera.WorldToViewportPoint(unitNamePlateController.NamePlateTransform.position);
                //Debug.Log("NamePlateController.LateUpdate(): the relative position of the character(" + (namePlateUnit as MonoBehaviour).gameObject.name + ") is " + relativePosition);
                if (!(relativePosition.z >= 0 && (relativePosition.x >= 0 && relativePosition.x <= 1) && (relativePosition.y >= 0 && relativePosition.y <= 1))) {
                    //Debug.Log("outisde viewport, not rendering");
                    renderNamePlate = false;
                }
                if (uIManager.CutSceneBarController.CurrentCutscene != null) {
                    //Debug.Log("NamePlateController.LateUpdate(): cutscene: calculating distance from camera");
                    float unitDistance = Mathf.Abs(Vector3.Distance(cameraManager.CurrentCutsceneCameraController.gameObject.transform.position, unitNamePlateController.NamePlateTransform.position));
                    if (unitDistance > 40f) {
                        //Debug.Log("NamePlateController.LateUpdate(): cutscene: calculating distance from camera: more than 40f: " + unitDistance);
                        renderNamePlate = false;
                    }
                } else {
                    //Debug.Log("NamePlateController.LateUpdate(): not cutscene: calculating distance from player");
                    if (playerManager.ActiveUnitController == null
                        || Mathf.Abs(Vector3.Distance(playerManager.ActiveUnitController.transform.position, unitNamePlateController.NamePlateTransform.position)) > 40f) {
                        renderNamePlate = false;
                    }
                }
                if (renderNamePlate) {
                    //Debug.Log("renderNamePlate");
                    //Vector3 usedPosition = currentCamera.WorldToScreenPoint(worldPosition);
                    Vector3 usedPosition = currentCamera.WorldToScreenPoint(unitNamePlateController.NamePlateTransform.position + (Vector3.up * positionOffset));
                    namePlateContents.position = usedPosition;
                    speechBubbleContents.position = usedPosition;
                    //Debug.Log(characterUnit.gameObject.name + ".distance to player: " + Mathf.Abs(Vector3.Distance(playerManager.MyPlayerUnitObject.transform.position, characterUnit.transform.position)));
                    namePlateCanvasGroup.alpha = 1;
                    speechBubbleCanvasGroup.alpha = 1;
                    if (isPlayerUnitNamePlate) {
                        namePlateCanvasGroup.blocksRaycasts = false;
                    } else {
                        namePlateCanvasGroup.blocksRaycasts = true;
                    }
                } else {
                    //Debug.Log("DONOTrenderNamePlate");
                    speechBubbleCanvasGroup.alpha = 0;
                    namePlateCanvasGroup.alpha = 0;
                    namePlateCanvasGroup.blocksRaycasts = false;
                }
            } else {
                //Debug.Log("charcterstats is null in nameplate on lateupdate in instanceid " + GetInstanceID().ToString());
            }
        }

        private void SetFactionColor() {
            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor()");
            if (playerManager.PlayerUnitSpawned == false && uIManager.CutSceneBarController.CurrentCutscene == null) {
                //Debug.Log(namePlateUnit.DisplayName + "NamePlateController.SetFactionColor(): player unit not spawned yet and this is not a cutscene");
                return;
            }
            // the last condition was preventing inanimate units from setting their nameplate name color properly
            if (unitNamePlateController == null || playerManager.MyCharacter == null) {
                //Debug.Log(namePlateUnit.DisplayName + "NamePlateController.SetFactionColor() characterunit or player instance is null. returning!");
                return;
            }
            //CheckForPlayerOwnerShip();
            if (unitNamePlateController.HasHealth() == true) {
                //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor(): nameplateUnit has health, setting bar color");
                if (uIManager.CutSceneBarController.CurrentCutscene != null && uIManager.CutSceneBarController.CurrentCutscene.UseDefaultFactionColors == true) {
                    if (unitNamePlateController.Faction != null) {
                        //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: USING DEFAULT");
                        HealthSlider.color = unitNamePlateController.Faction.GetFactionColor();
                    } else {
                        //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: USING UNIT");
                        HealthSlider.color = Faction.GetFactionColor(playerManager, (unitNamePlateController as UnitNamePlateController).UnitController);
                    }
                } else {
                    //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: USING UNIT");
                    HealthSlider.color = Faction.GetFactionColor(playerManager, (unitNamePlateController as UnitNamePlateController).UnitController);
                }
                //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor(): nameplateUnit has health, set faction color: " + MyHealthSlider.color);
                //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.DisplayName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.DisplayName + "; color: " + ColorUtility.ToHtmlStringRGB(MyHealthSlider.color));

            }
            //Debug.Log(namePlateUnit.DisplayName + ".NamePlateController.SetFactionColor(): setting character name");
            SetCharacterName();
            CheckForDisabledHealthBar();
        }

        public void OnClick(BaseEventData eventData) {
            //Debug.Log("NamePlateController: OnClick()");
            if (playerManager.PlayerUnitSpawned == false) {
                return;
            }

            PointerEventData pointerEventData = eventData as PointerEventData;
            if (pointerEventData.button == PointerEventData.InputButton.Left) {
                HandleLeftClick();
            }
            if (pointerEventData.button == PointerEventData.InputButton.Right) {
                HandleRightClick();
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (unitNamePlateController?.Interactable != playerManager?.UnitController?.gameObject) {
                uIManager.NamePlateManager.AddMouseOver(this);
                if (unitNamePlateController.Interactable != null) {
                    unitNamePlateController.Interactable.IsMouseOverNameplate = true;
                    unitNamePlateController.Interactable.OnMouseIn();
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            ProcessPointerExit();
        }

        public void ProcessPointerExit() {
            if (unitNamePlateController?.Interactable != playerManager?.UnitController?.gameObject) {
                uIManager.NamePlateManager.RemoveMouseOver(this);
                if (unitNamePlateController?.Interactable != null && unitNamePlateController.Interactable.IsMouseOverNameplate == true) {
                    unitNamePlateController.Interactable.IsMouseOverNameplate = false;
                    unitNamePlateController.Interactable.OnMouseOut();
                }
            }
        }

        private void HandleRightClick() {
            //Debug.Log("NamePlateController: HandleRightClick()");

            if (playerManager.UnitController == null) {
                return;
            }
            if (unitNamePlateController.Interactable.gameObject != playerManager.UnitController.gameObject && unitNamePlateController.Interactable.IsTrigger == false) {
                playerManager.PlayerController.InterActWithTarget(unitNamePlateController.Interactable);
            }
        }

        private void HandleLeftClick() {
            //Debug.Log("NamePlateController: HandleLeftClick(): " + namePlateUnit.DisplayName);
            if (playerManager.UnitController == null) {
                return;
            }
            if (unitNamePlateController.Interactable.gameObject != (playerManager.UnitController.gameObject)) {
                playerManager.UnitController.SetTarget(unitNamePlateController.Interactable);
            }
        }

        public void HideSpeechBubble() {
            if (speechBubbleBackground != null) {
                speechBubbleBackground.SetActive(false);
            }
        }

        public void ShowSpeechBubble() {
            //Debug.Log(unitNamePlateController.UnitDisplayName + ".NamePlateController: ShowSpeechBubble()");
            if (speechBubbleBackground != null) {
                speechBubbleBackground.SetActive(true);
            }
        }

        public void SetSpeechText(string newSpeechText) {
            if (speechBubbleText != null && newSpeechText != null) {
                speechBubbleText.text = newSpeechText;
            }
        }

        // name plates can be disabled by hiding the ui with the '.' button, so this should only be done when sending them to the pool
        //public void OnDisable() {
        public void OnSendObjectToPool() {
            //Debug.Log($"{unitNamePlateController.UnitDisplayName}.NamePlateController.OnSendObjectToPool()");

            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            ProcessPointerExit();
            CleanupEventSubscriptions();
            CleanupPlayerEventSubscriptions();

            // this could have been disabled while it was still the focus so it needs to be unhighlighted just in case
            UnHighlight(false);

            // reset settings
            unitNamePlateController = null;
            isPlayerUnitNamePlate = false;
            localComponentsInitialized = false;
            eventSubscriptionsInitialized = false;
        }

    }

}