using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AnyRPG {
    public class NamePlateController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField]
        private Image healthSlider;

        [SerializeField]
        private GameObject healthBar;

        [SerializeField]
        private Text characterName;

        [SerializeField]
        private Text questIndicator;

        [SerializeField]
        private GameObject questIndicatorBackground;

        [SerializeField]
        private float positionOffset;

        [SerializeField]
        private Image genericIndicatorImage;

        [SerializeField]
        private GameObject speechBubbleBackground;

        [SerializeField]
        private Text speechBubbleText;

        private INamePlateUnit namePlateUnit;

        private int healthSliderWidth;

        private CanvasGroup canvasGroup;
        private bool isPlayerUnitNamePlate = false;

        private bool localComponentsInitialized = false;

        protected bool eventSubscriptionsInitialized = false;

        public Image MyHealthSlider { get => healthSlider; }
        public GameObject MyHealthBar { get => healthBar; }
        public Text MyCharacterName { get => characterName; set => characterName = value; }
        public Text MyQuestIndicator { get => questIndicator; }
        public GameObject MyQuestIndicatorBackground { get => questIndicatorBackground; set => questIndicatorBackground = value; }
        public Image MyGenericIndicatorImage { get => genericIndicatorImage; set => genericIndicatorImage = value; }

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
            SystemEventManager.MyInstance.OnReputationChange += SetFactionColor;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += SetFactionColor;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                SetFactionColor();
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= SetFactionColor;
            SystemEventManager.MyInstance.OnReputationChange -= SetFactionColor;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= CleanupEventSubscriptions;
            SystemEventManager.MyInstance.OnPlayerNameChanged -= SetCharacterName;

            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }


        private void InitializeLocalComponents() {
            //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.InitializeLocalComponents()");
            if (localComponentsInitialized == true) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.InitializeLocalComponents(): already done.  exiting!");
                return;
            }

            canvasGroup = GetComponent<CanvasGroup>();
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
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned && ((namePlateUnit as CharacterUnit) == PlayerManager.MyInstance.MyCharacter.MyCharacterUnit)) {
                //Debug.Log("NamePlateController.Start(). Setting Player healthbar to ignore raycast");
                canvasGroup.blocksRaycasts = false;
                UIManager.MyInstance.SetLayerRecursive(gameObject, LayerMask.NameToLayer("Ignore Raycast"));
                isPlayerUnitNamePlate = true;
                SystemEventManager.MyInstance.OnPlayerNameChanged += SetCharacterName;
            } else {
                canvasGroup.blocksRaycasts = true;
            }

        }

        public void Highlight() {
            healthBar.GetComponent<Image>().color = Color.white;
            transform.SetAsLastSibling();
        }

        public void UnHighlight() {
            healthBar.GetComponent<Image>().color = Color.black;
        }

        private void SetCharacterName() {
            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName()");
            if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerName") == 0 && PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): ShowPlayerName and ShowPlayerFaction are both set to zero, setting charactername.text to string.empty");
                MyCharacterName.text = string.Empty;
                return;
            }

            if (MyCharacterName != null && namePlateUnit != null) {
                if (MyCharacterName.text != null) {
                    // character names have special coloring. white if no faction, green if character, otherwise normal faction colors
                    if (namePlateUnit.MyFaction != null) {
                        //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): getting color for faction: " + namePlateUnit.MyFactionName + " isplayerUnitNamePlate: " + isPlayerUnitNamePlate);
                        Color textColor = Faction.GetFactionColor(namePlateUnit);
                        string nameString = string.Empty;
                        string factionString = string.Empty;
                        if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerName") == 0) {
                            // nothing for now
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): not showing name");
                        } else {
                            nameString = namePlateUnit.MyDisplayName;
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): showing name");
                        }
                        if (isPlayerUnitNamePlate && PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): not showing faction");
                        } else {
                            factionString = "<" + namePlateUnit.MyFaction.MyName + ">";
                            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetCharacterName(): showing faction");
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
                        if ((namePlateUnit as MonoBehaviour).gameObject == PlayerManager.MyInstance.MyPlayerUnitObject) {
                            textColor = Color.green;
                        } else {
                            textColor = Color.white;
                        }
                        if (PlayerPrefs.GetInt("ShowPlayerName") == 0 && isPlayerUnitNamePlate) {
                            // nothing
                        } else {
                            MyCharacterName.text = string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(textColor), namePlateUnit.MyDisplayName);
                        }
                    }
                } else {
                    //Debug.Log("NamePlateController.SetCharacterName(): text field is null!");
                }
            } else {
                //Debug.Log("NamePlateController.SetCharacterName(): character name is null!");
            }
        }

        public void SetNamePlateUnit(INamePlateUnit namePlateUnit) {
            //Debug.Log("NamePlateController.SetNamePlateUnit(" + namePlateUnit.MyDisplayName + ") setting namePlateUnit on nameplate in instanceid" + GetInstanceID().ToString());
            // moved code here from awake since a nameplate always has to be initialized so this method will always be called before anything else
            this.namePlateUnit = namePlateUnit;

            InitializeLocalComponents();

            if (namePlateUnit.HasHealth()) {
                namePlateUnit.HealthBarNeedsUpdate += OnHealthChanged;
                OnHealthChanged(namePlateUnit.MaxHealth(), namePlateUnit.CurrentHealth());
                if (namePlateUnit is CharacterUnit) {
                    if ((namePlateUnit as CharacterUnit).MyBaseCharacter != null) {
                        if ((namePlateUnit as CharacterUnit).MyBaseCharacter.MyCharacterFactionManager != null) {
                            (namePlateUnit as CharacterUnit).MyBaseCharacter.MyCharacterFactionManager.OnReputationChange += HandleReputationChange;
                        } else {
                            Debug.Log("NamePlateController.SetNamePlateUnit(" + namePlateUnit.MyDisplayName + ") nameplate unit has no character faction manager!");
                        }
                    } else {
                        Debug.Log("NamePlateController.SetNamePlateUnit(" + namePlateUnit.MyDisplayName + ") nameplate unit has no base Character!");
                    }
                } else {
                    Debug.Log("NamePlateController.SetNamePlateUnit(" + namePlateUnit.MyDisplayName + ") nameplate unit is not characterUnit!");
                }
            } else {
                MyHealthBar.SetActive(false);
            }
            namePlateUnit.MyNamePlate = this;
        }

        public void HandleReputationChange() {
            SetFactionColor();
        }

        public void CheckForDisabledHealthBar() {
            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.CheckForDisableHealthBar()");
            if (namePlateUnit.HasHealth() && isPlayerUnitNamePlate) {
                //Debug.Log("CheckForDisableHealthBar() THIS IS THE PLAYER UNIT NAMEPLATE.  CHECK IF MAX HEALTH: ");
                if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterStats != null) {
                    if (PlayerManager.MyInstance.MyCharacter.MyCharacterStats.currentHealth == PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyMaxHealth && PlayerPrefs.GetInt("HideFullHealthBar") == 1) {
                        DisableHealthBar();
                        return;
                    }
                }
            }
            if (namePlateUnit.HasHealth()) {
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

        void OnHealthChanged(int maxHealth, int currentHealth) {
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
            if (namePlateUnit != null && (PlayerManager.MyInstance.MyPlayerUnitObject != null || LevelManager.MyInstance.GetActiveSceneNode().MyIsCutScene)) {
                //Debug.Log("Setting the position of the nameplate transform in lateupdate");
                bool renderNamePlate = true;
                //Debug.Log("NamePlateController.LateUpdate(): the position of the character is " + characterUnit.transform.position);
                Camera currentCamera;
                if (LevelManager.MyInstance.GetActiveSceneNode().MyIsCutScene) {
                    currentCamera = AnyRPGCutsceneCameraController.MyInstance.GetComponent<Camera>();
                } else {
                    currentCamera = CameraManager.MyInstance.MyMainCamera;
                }
                Vector3 relativePosition = currentCamera.WorldToViewportPoint(namePlateUnit.MyNamePlateTransform.position);
                //Debug.Log("NamePlateController.LateUpdate(): the relative position of the character(" + (namePlateUnit as MonoBehaviour).gameObject.name + ") is " + relativePosition);
                if (!(relativePosition.z >= 0 && (relativePosition.x >= 0 && relativePosition.x <= 1) && (relativePosition.y >= 0 && relativePosition.y <= 1))) {
                    //Debug.Log("outisde viewport, not rendering");
                    renderNamePlate = false;
                }
                if (LevelManager.MyInstance.GetActiveSceneNode().MyIsCutScene) {
                    //Debug.Log("NamePlateController.LateUpdate(): cutscene: calculating distance from camera");
                    float unitDistance = Mathf.Abs(Vector3.Distance(AnyRPGCutsceneCameraController.MyInstance.gameObject.transform.position, namePlateUnit.MyNamePlateTransform.position));
                    if (unitDistance > 40f) {
                        //Debug.Log("NamePlateController.LateUpdate(): cutscene: calculating distance from camera: more than 40f: " + unitDistance);
                        renderNamePlate = false;
                    }
                } else {
                    //Debug.Log("NamePlateController.LateUpdate(): not cutscene: calculating distance from player");
                    if (Mathf.Abs(Vector3.Distance(PlayerManager.MyInstance.MyPlayerUnitObject.transform.position, namePlateUnit.MyNamePlateTransform.position)) > 40f) {
                        renderNamePlate = false;
                    }
                }
                if (renderNamePlate) {
                    //Debug.Log("renderNamePlate");
                    transform.position = currentCamera.WorldToScreenPoint(namePlateUnit.MyNamePlateTransform.position + Vector3.up * positionOffset);
                    //Debug.Log(characterUnit.gameObject.name + ".distance to player: " + Mathf.Abs(Vector3.Distance(PlayerManager.MyInstance.MyPlayerUnitObject.transform.position, characterUnit.transform.position)));
                    canvasGroup.alpha = 1;
                    if (isPlayerUnitNamePlate) {
                        canvasGroup.blocksRaycasts = false;
                    } else {
                        canvasGroup.blocksRaycasts = true;
                    }
                } else {
                    //Debug.Log("DONOTrenderNamePlate");
                    canvasGroup.alpha = 0;
                    canvasGroup.blocksRaycasts = false;
                }
                //transform.position = Camera.main.WorldToScreenPoint(characterStats.transform.position);
            } else {
                //Debug.Log("charcterstats is null in nameplate on lateupdate in instanceid " + GetInstanceID().ToString());
            }
        }

        private void SetFactionColor() {
            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor()");
            SceneNode activeSceneNode = LevelManager.MyInstance.GetActiveSceneNode();
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false && activeSceneNode != null && !activeSceneNode.MyIsCutScene) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.SetFactionColor(): player unit not spawned yet and this is not a cutscene");
                return;
            }
            // the last condition was preventing inanimate units from setting their nameplate name color properly
            if (namePlateUnit == null || PlayerManager.MyInstance.MyCharacter == null) {
                //Debug.Log(namePlateUnit.MyDisplayName + "NamePlateController.SetFactionColor() characterunit or player instance is null. returning!");
                return;
            }
            CheckForPlayerOwnerShip();
            if (namePlateUnit.HasHealth() == true) {
                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): nameplateUnit has health, setting bar color");
                MyHealthSlider.color = Faction.GetFactionColor(namePlateUnit);
                //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): nameplateUnit has health, set faction color: " + MyHealthSlider.color);
            }
            //Debug.Log(namePlateUnit.MyDisplayName + ".NamePlateController.SetFactionColor(): setting character name");
            SetCharacterName();
            CheckForDisabledHealthBar();
        }

        private void OnDestroy() {
            //Debug.Log(gameObject.name + ".NamePlateController.OnDestroy()");
            if (namePlateUnit != null) {
                //Debug.Log(gameObject.name + ".NamePlateController.OnDestroy(): removing onhealthchanged and setting mynameplate to null");
                namePlateUnit.HealthBarNeedsUpdate -= OnHealthChanged;
                if (namePlateUnit.HasHealth()) {
                    (namePlateUnit as CharacterUnit).MyBaseCharacter.MyCharacterFactionManager.OnReputationChange -= HandleReputationChange;
                }
                namePlateUnit.MyNamePlate = null;
            }
            CleanupEventSubscriptions();
        }

        public void OnClick(BaseEventData eventData) {
            //Debug.Log("NamePlateController: OnClick()");
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
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
            if (namePlateUnit.MyInteractable != null) {
                namePlateUnit.MyInteractable.OnMouseHover();
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (namePlateUnit.MyInteractable != null) {
                namePlateUnit.MyInteractable.OnMouseOut();
            }
        }

        private void HandleRightClick() {
            //Debug.Log("NamePlateController: HandleRightClick(): " + namePlateUnit.MyDisplayName);
            if (namePlateUnit != (PlayerManager.MyInstance.MyCharacter.MyCharacterUnit as INamePlateUnit)) {
                (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).InterActWithTarget(namePlateUnit.MyInteractable, (namePlateUnit as MonoBehaviour).gameObject);
            }
        }

        private void HandleLeftClick() {
            //Debug.Log("NamePlateController: HandleLeftClick(): " + namePlateUnit.MyDisplayName);
            if (namePlateUnit != (PlayerManager.MyInstance.MyCharacter.MyCharacterUnit as INamePlateUnit)) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterController.SetTarget((namePlateUnit as MonoBehaviour).gameObject);
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