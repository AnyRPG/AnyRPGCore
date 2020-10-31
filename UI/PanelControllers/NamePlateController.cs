using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class NamePlateController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField]
        private Image healthSlider = null;

        [SerializeField]
        private GameObject healthBar = null;

        [SerializeField]
        private TextMeshProUGUI characterName = null;

        [SerializeField]
        private TextMeshProUGUI questIndicator = null;

        [SerializeField]
        private GameObject questIndicatorBackground = null;

        [SerializeField]
        private float positionOffset;

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

        [SerializeField]
        private Canvas namePlateCanvas = null;

        [SerializeField]
        private Canvas speechBubbleCanvas = null;

        private INamePlateController unitNamePlateController = null;

        private int healthSliderWidth;

        private bool isPlayerUnitNamePlate = false;

        private bool localComponentsInitialized = false;

        protected bool eventSubscriptionsInitialized = false;

        public Image MyHealthSlider { get => healthSlider; }
        public GameObject MyHealthBar { get => healthBar; }
        public TextMeshProUGUI MyCharacterName { get => characterName; set => characterName = value; }
        public TextMeshProUGUI MyQuestIndicator { get => questIndicator; }
        public GameObject MyQuestIndicatorBackground { get => questIndicatorBackground; set => questIndicatorBackground = value; }
        public Image MyGenericIndicatorImage { get => genericIndicatorImage; set => genericIndicatorImage = value; }
        public NamePlateCanvasController MyNamePlateCanvasController { get => namePlateCanvasController; set => namePlateCanvasController = value; }
        public Canvas MyNamePlateCanvas { get => namePlateCanvas; set => namePlateCanvas = value; }
        public Canvas MySpeechBubbleCanvas { get => speechBubbleCanvas; set => speechBubbleCanvas = value; }

        private void Start() {
            //Debug.Log("NamePlateController.Start(): namePlateUnit: " + (namePlateUnit == null ? "null" : namePlateUnit.MyDisplayName));
            CreateEventSubscriptions();
            HideSpeechBubble();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("NamePlateController.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += CleanupEventSubscriptions;
            //Debug.Log("NamePlateController.CreateEventSubscriptions()");
            SystemEventManager.StartListening("OnReputationChange", HandleReputationChange);
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            //if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
            ProcessPlayerUnitSpawn();
            //}
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnReputationChange", HandleReputationChange);
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= CleanupEventSubscriptions;
            if (unitNamePlateController != null) {
                unitNamePlateController.OnNameChange += SetCharacterName;
            }

            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void ProcessPlayerUnitSpawn() {
            SetFactionColor();
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        public void HandleReputationChange(string eventName, EventParamProperties eventParam) {
            SetFactionColor();
        }


        private void InitializeLocalComponents() {
            //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.InitializeLocalComponents()");
            if (localComponentsInitialized == true) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.InitializeLocalComponents(): already done.  exiting!");

                return;
            }

            if (healthSlider != null) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.InitializeLocalComponents(): healthSlider != null");
            }
            if (healthSlider.GetComponent<LayoutElement>() != null) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.InitializeLocalComponents(): healthSlider.GetComponent<LayoutElement>() != null: " + healthSlider.GetComponent<LayoutElement>().preferredWidth);
            }
            healthSliderWidth = (int)(healthSlider.GetComponent<LayoutElement>().preferredWidth);
            //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.InitializeLocalComponents(): healthSliderWidth: " + healthSliderWidth);
            MyQuestIndicatorBackground.SetActive(false);
            MyGenericIndicatorImage.gameObject.SetActive(false);

            // ensure we update the nameplate color when the player spawns
            SetFactionColor();
            if (MyCharacterName != null) {
                //Debug.Log("starting nameplate on " + MyCharacterName.text);
            } else {
                //Debug.Log("starting nameplate on unknown character");
            }
            CheckForPlayerOwnerShip();
            //Debug.Log("NamePlateController: PlayerUnitSpawned: " + PlayerManager.MyInstance.MyPlayerUnitSpawned);

            localComponentsInitialized = true;
        }

        public void CheckForPlayerOwnerShip() {
            //Debug.Log("NamePlateController.CheckForPlayerOwnerShip()");
            if (PlayerManager.MyInstance.PlayerUnitSpawned && unitNamePlateController.NamePlateUnit.gameObject == PlayerManager.MyInstance.PlayerUnitObject) {
                //Debug.Log("NamePlateController.Start(). Setting Player healthbar to ignore raycast");
                namePlateCanvasGroup.blocksRaycasts = false;
                UIManager.MyInstance.SetLayerRecursive(gameObject, LayerMask.NameToLayer("Ignore Raycast"));
                isPlayerUnitNamePlate = true;
            } else {
                namePlateCanvasGroup.blocksRaycasts = true;
            }

        }

        public void Highlight() {
            healthBar.GetComponent<Image>().color = Color.white;
            namePlateCanvas.sortingOrder = 1;
        }

        public void UnHighlight() {
            healthBar.GetComponent<Image>().color = Color.black;
            namePlateCanvas.sortingOrder = 0;
        }

        private void SetCharacterName() {
            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName()");
            if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerName") == 0 && PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): ShowPlayerName and ShowPlayerFaction are both set to zero, setting charactername.text to string.empty");
                MyCharacterName.text = string.Empty;
                return;
            }

            if (MyCharacterName != null && unitNamePlateController != null) {
                if (MyCharacterName.text != null) {
                    // character names have special coloring. white if no faction, green if character, otherwise normal faction colors
                    if (unitNamePlateController.Faction != null) {
                        //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName);
                        Color textColor;
                        if (UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene != null && UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene.MyUseDefaultFactionColors == true) {
                            if (unitNamePlateController.Faction != null) {
                                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: USING DEFAULT");
                                textColor = unitNamePlateController.Faction.GetFactionColor();
                            } else {
                                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: USING UNIT");
                                textColor = Faction.GetFactionColor(unitNamePlateController.NamePlateUnit);
                            }
                        } else {
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: USING UNIT");
                            textColor = Faction.GetFactionColor(unitNamePlateController.NamePlateUnit);
                        }
                        //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: " + ColorUtility.ToHtmlStringRGB(textColor));


                        string nameString = string.Empty;
                        string factionString = string.Empty;
                        if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerName") == 0) {
                            // nothing for now
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): not showing name");
                        } else {
                            nameString = unitNamePlateController.UnitDisplayName;
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): showing name");
                        }
                        if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): not showing faction");
                        } else {
                            if (unitNamePlateController.SuppressFaction == false) {
                                factionString = "<" + unitNamePlateController.Faction.DisplayName + ">";
                            }
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): showing faction");
                        }
                        if (unitNamePlateController.Title != string.Empty) {
                            factionString = "<" + unitNamePlateController.Title + ">";
                        }
                        string newLineString = string.Empty;
                        if (factionString != string.Empty && nameString != string.Empty) {
                            newLineString = "\n";
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): faction nad name are both not empty");
                        } else {
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): faction or name are empty");
                        }
                        MyCharacterName.text = string.Format("<color=#{0}>{1}{2}{3}</color>", ColorUtility.ToHtmlStringRGB(textColor), nameString, newLineString, factionString);
                    } else {
                        //Debug.Log("NamePlateController.SetCharacterName(): namePlateUnit has no faction!");
                        Color textColor;
                        if (unitNamePlateController.NamePlateUnit.gameObject == PlayerManager.MyInstance.ActiveUnitController.gameObject) {
                            textColor = Color.green;
                        } else {
                            textColor = Color.white;
                        }
                        if (PlayerPrefs.GetInt("ShowPlayerName") == 0 && isPlayerUnitNamePlate) {
                            // nothing
                        } else {
                            MyCharacterName.text = string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(textColor), unitNamePlateController.UnitDisplayName);
                        }
                    }
                } else {
                    //Debug.Log("NamePlateController.SetCharacterName(): text field is null!");
                }
            } else {
                //Debug.Log("NamePlateController.SetCharacterName(): character name is null!");
            }
        }

        public void SetNamePlateUnit(INamePlateUnit namePlateUnit, bool usePositionOffset) {
            //Debug.Log("NamePlateController.SetNamePlateUnit(" + namePlateUnit.UnitDisplayName + ") setting namePlateUnit on nameplate in instanceid" + GetInstanceID().ToString());
            // moved code here from awake since a nameplate always has to be initialized so this method will always be called before anything else
            unitNamePlateController = namePlateUnit.NamePlateController;
            if (usePositionOffset == false) {
                positionOffset = 0f;
            }

            InitializeLocalComponents();

            if (unitNamePlateController.HasHealth()) {
                (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter.CharacterStats.OnResourceAmountChanged += HandleResourceAmountChanged;
                ProcessHealthChanged(unitNamePlateController.MaxHealth(), unitNamePlateController.CurrentHealth());
                if ((unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter != null && (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter.CharacterFactionManager != null) {
                    (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter.CharacterFactionManager.OnReputationChange += HandleReputationChange;
                }
            } else {
                MyHealthBar.SetActive(false);
            }
            unitNamePlateController.NamePlate = this;

            unitNamePlateController.OnNameChange += SetCharacterName;
        }

        /// <summary>
        /// because name plates only show health, filter resource updates to health only
        /// </summary>
        /// <param name="powerResource"></param>
        /// <param name="currentHealth"></param>
        /// <param name="maxHealth"></param>
        public void HandleResourceAmountChanged(PowerResource powerResource, int currentHealth, int maxHealth) {
            //Debug.Log(gameObject.name + ".CharacterUnit.HandleHealthBarNeedsUpdate(" + currentHealth + ", " + maxHealth + ")");
            if (unitNamePlateController.HasHealth()
                && (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter != null
                && (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter.CharacterStats != null
                && (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter.CharacterStats.PrimaryResource == powerResource) {
                ProcessHealthChanged(currentHealth, maxHealth);
            }
        }

        public void HandleReputationChange() {
            SetFactionColor();
        }

        public void CheckForDisabledHealthBar() {
            //Debug.Log(namePlateUnit.UnitDisplayName + ".NamePlateController.CheckForDisableHealthBar()");
            if (unitNamePlateController.HasHealth() && isPlayerUnitNamePlate) {
                //Debug.Log("CheckForDisableHealthBar() THIS IS THE PLAYER UNIT NAMEPLATE.  CHECK IF MAX HEALTH: ");
                if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterStats != null) {
                    if (PlayerManager.MyInstance.MyCharacter.CharacterStats.CurrentPrimaryResource == PlayerManager.MyInstance.MyCharacter.CharacterStats.MaxPrimaryResource && PlayerPrefs.GetInt("HideFullHealthBar") == 1) {
                        DisableHealthBar();
                        return;
                    }
                }
            }
            if (unitNamePlateController.HasHealth()) {
                EnableHealthBar();
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

        void ProcessHealthChanged(int maxHealth, int currentHealth) {
            //Debug.Log("NamePlateController.ProcessHealthChanged()");
            float healthPercent = (float)currentHealth / maxHealth;
            //Debug.Log(MyCharacterName.text + ".NamePlateController.OnHealthChanged(" + maxHealth + ", " + currentHealth + "): healthsliderwidth: " + healthSliderWidth.ToString() + "; healthPercent: " + healthPercent.ToString());
            if (MyHealthSlider == null) {
                //Debug.Log("NamePlateController.OnHealthChanged() MyHealthSlider == null!");
                return;
            }
            LayoutElement layoutElement = MyHealthSlider.GetComponent<LayoutElement>();
            if (layoutElement == null) {
                //Debug.Log("NamePlateController.OnHealthChanged() MyHealthSlider.layoutElement == null!");
                return;
            }
            layoutElement.preferredWidth = healthPercent * healthSliderWidth;
            CheckForDisabledHealthBar();
        }

        private void LateUpdate() {
            if (unitNamePlateController != null && (PlayerManager.MyInstance.PlayerUnitObject != null || UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene != null)) {
                //Debug.Log("Setting the position of the nameplate transform in lateupdate");
                bool renderNamePlate = true;
                //Debug.Log("NamePlateController.LateUpdate(): the position of the character is " + characterUnit.transform.position);
                Camera currentCamera;
                if (UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene != null) {
                    currentCamera = CutsceneCameraController.MyInstance.GetComponent<Camera>();
                } else {
                    currentCamera = CameraManager.MyInstance.MyActiveMainCamera;
                }
                //Debug.Log("NamePlateController.LateUpdate(): namePlateUnit: " + (namePlateUnit as MonoBehaviour).gameObject.name + "; currentcamera: " + (currentCamera == null ? "null" : currentCamera.name));
                Vector3 relativePosition = currentCamera.WorldToViewportPoint(unitNamePlateController.NamePlateTransform.position);
                //Debug.Log("NamePlateController.LateUpdate(): the relative position of the character(" + (namePlateUnit as MonoBehaviour).gameObject.name + ") is " + relativePosition);
                if (!(relativePosition.z >= 0 && (relativePosition.x >= 0 && relativePosition.x <= 1) && (relativePosition.y >= 0 && relativePosition.y <= 1))) {
                    //Debug.Log("outisde viewport, not rendering");
                    renderNamePlate = false;
                }
                if (UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene != null) {
                    //Debug.Log("NamePlateController.LateUpdate(): cutscene: calculating distance from camera");
                    float unitDistance = Mathf.Abs(Vector3.Distance(CutsceneCameraController.MyInstance.gameObject.transform.position, unitNamePlateController.NamePlateTransform.position));
                    if (unitDistance > 40f) {
                        //Debug.Log("NamePlateController.LateUpdate(): cutscene: calculating distance from camera: more than 40f: " + unitDistance);
                        renderNamePlate = false;
                    }
                } else {
                    //Debug.Log("NamePlateController.LateUpdate(): not cutscene: calculating distance from player");
                    if (Mathf.Abs(Vector3.Distance(PlayerManager.MyInstance.PlayerUnitObject.transform.position, unitNamePlateController.NamePlateTransform.position)) > 40f) {
                        renderNamePlate = false;
                    }
                }
                if (renderNamePlate) {
                    //Debug.Log("renderNamePlate");
                    Vector3 usedPosition = currentCamera.WorldToScreenPoint(unitNamePlateController.NamePlateTransform.position + Vector3.up * positionOffset);
                    namePlateContents.position = usedPosition;
                    speechBubbleContents.position = usedPosition;
                    //Debug.Log(characterUnit.gameObject.name + ".distance to player: " + Mathf.Abs(Vector3.Distance(PlayerManager.MyInstance.MyPlayerUnitObject.transform.position, characterUnit.transform.position)));
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
            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor()");
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false && UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene == null) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.SetFactionColor(): player unit not spawned yet and this is not a cutscene");
                return;
            }
            // the last condition was preventing inanimate units from setting their nameplate name color properly
            if (unitNamePlateController == null || PlayerManager.MyInstance.MyCharacter == null) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.SetFactionColor() characterunit or player instance is null. returning!");
                return;
            }
            CheckForPlayerOwnerShip();
            if (unitNamePlateController.HasHealth() == true) {
                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): nameplateUnit has health, setting bar color");
                if (UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene != null && UIManager.MyInstance.MyCutSceneBarController.MyCurrentCutscene.MyUseDefaultFactionColors == true) {
                    if (unitNamePlateController.Faction != null) {
                        //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: USING DEFAULT");
                        MyHealthSlider.color = unitNamePlateController.Faction.GetFactionColor();
                    } else {
                        //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: USING UNIT");
                        MyHealthSlider.color = Faction.GetFactionColor((unitNamePlateController as UnitNamePlateController).UnitController);
                    }
                } else {
                    //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: USING UNIT");
                    MyHealthSlider.color = Faction.GetFactionColor((unitNamePlateController as UnitNamePlateController).UnitController);
                }
                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): nameplateUnit has health, set faction color: " + MyHealthSlider.color);
                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): getting color for faction: " + namePlateUnit.MyFaction.MyName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate + "; name: " + namePlateUnit.MyDisplayName + "; color: " + ColorUtility.ToHtmlStringRGB(MyHealthSlider.color));

            }
            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): setting character name");
            SetCharacterName();
            CheckForDisabledHealthBar();
        }

        private void OnDestroy() {
            //Debug.Log(gameObject.name + ".NamePlateController.OnDestroy()");
            if (unitNamePlateController != null) {
                //Debug.Log(gameObject.name + ".NamePlateController.OnDestroy(): removing onhealthchanged and setting mynameplate to null");
                unitNamePlateController.ResourceBarNeedsUpdate -= ProcessHealthChanged;
                if (unitNamePlateController.HasHealth()) {
                    (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter.CharacterStats.OnResourceAmountChanged -= HandleResourceAmountChanged;
                    (unitNamePlateController as UnitNamePlateController).UnitController.BaseCharacter.CharacterFactionManager.OnReputationChange -= HandleReputationChange;
                }
                unitNamePlateController.NamePlate = null;
            }
            CleanupEventSubscriptions();
        }

        public void OnClick(BaseEventData eventData) {
            //Debug.Log("NamePlateController: OnClick()");
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
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
            if (unitNamePlateController.Interactable != null) {
                unitNamePlateController.Interactable.OnMouseHover();
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (unitNamePlateController.Interactable != null) {
                unitNamePlateController.Interactable.OnMouseOut();
            }
        }

        private void HandleRightClick() {
            //Debug.Log("NamePlateController: HandleRightClick(): " + namePlateUnit.MyDisplayName);
            if (unitNamePlateController != (PlayerManager.MyInstance.MyCharacter.CharacterUnit as INamePlateUnit)) {
                PlayerManager.MyInstance.PlayerController.InterActWithTarget(unitNamePlateController.Interactable);
            }
        }

        private void HandleLeftClick() {
            //Debug.Log("NamePlateController: HandleLeftClick(): " + namePlateUnit.MyDisplayName);
            if (unitNamePlateController != (PlayerManager.MyInstance.MyCharacter.CharacterUnit as INamePlateUnit)) {
                PlayerManager.MyInstance.MyCharacter.UnitController.SetTarget(unitNamePlateController.Interactable);
            }
        }

        public void HideSpeechBubble() {
            if (speechBubbleBackground != null) {
                speechBubbleBackground.SetActive(false);
            }
        }

        public void ShowSpeechBubble() {
            if (speechBubbleBackground != null) {
                speechBubbleBackground.SetActive(true);
            }
        }

        public void SetSpeechText(string newSpeechText) {
            if (speechBubbleText != null && newSpeechText != null) {
                speechBubbleText.text = newSpeechText;
            }
        }

    }

}