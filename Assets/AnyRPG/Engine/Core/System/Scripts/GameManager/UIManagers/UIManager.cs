using AnyRPG;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class UIManager : MonoBehaviour {

        #region Singleton
        private static UIManager instance;

        public static UIManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<UIManager>();
                }

                return instance;
            }
        }

        #endregion

        [SerializeField]
        private GameObject inGameUI = null;

        [SerializeField]
        private GameObject playerUI = null;

        [SerializeField]
        private GameObject systemMenuUI = null;

        [SerializeField]
        private GameObject loadingCanvas = null;

        [SerializeField]
        private GameObject cutSceneBarsCanvas = null;

        [SerializeField]
        private GameObject bottomPanel = null;

        [SerializeField]
        private GameObject sidePanel = null;

        [SerializeField]
        private GameObject mouseOverWindow = null;

        [SerializeField]
        private GameObject playerInterface = null;

        [SerializeField]
        private GameObject popupWindowContainer = null;

        [SerializeField]
        private GameObject popupPanelContainer = null;

        [SerializeField]
        private GameObject combatTextCanvas = null;

        [SerializeField]
        private UnitFrameController playerUnitFrameController = null;

        [SerializeField]
        private UnitFrameController focusUnitFrameController = null;

        [SerializeField]
        private MiniMapController miniMapController = null;

        [SerializeField]
        private CutSceneBarController cutSceneBarController = null;

        [SerializeField]
        private XPBarController xpBarController = null;

        [SerializeField]
        private StatusEffectPanelController statusEffectPanelController = null;

        [SerializeField]
        private CastBarController floatingCastBarController = null;

        [SerializeField]
        private CloseableWindow questTrackerWindow = null;

        [SerializeField]
        private CloseableWindow combatLogWindow = null;

        [SerializeField]
        private ActionBarManager actionBarManager = null;

        [SerializeField]
        private GameObject toolTip = null;

        private TextMeshProUGUI toolTipText = null;

        [SerializeField]
        private CurrencyBarController toolTipCurrencyBarController = null;

        [SerializeField]
        private RectTransform tooltipRect = null;

        // objects in the mouseover window
        private TextMeshProUGUI mouseOverText;
        private GameObject mouseOverTarget;

        // keep track of window positions at startup in case of need to reset
        private Dictionary<string, float> defaultWindowPositions = new Dictionary<string, float>();

        // is a window currently being dragged.  used to suppres camera turn and pan
        private bool dragInProgress = false;

        protected bool eventSubscriptionsInitialized = false;

        public StatusEffectPanelController StatusEffectPanelController { get => statusEffectPanelController; }
        public UnitFrameController FocusUnitFrameController { get => focusUnitFrameController; }
        public ActionBarManager ActionBarManager { get => actionBarManager; set => actionBarManager = value; }
        public UnitFrameController PlayerUnitFrameController { get => playerUnitFrameController; set => playerUnitFrameController = value; }
        public CloseableWindow QuestTrackerWindow { get => questTrackerWindow; set => questTrackerWindow = value; }
        public CloseableWindow CombatLogWindow { get => combatLogWindow; set => combatLogWindow = value; }
        public CastBarController FloatingCastBarController { get => floatingCastBarController; set => floatingCastBarController = value; }
        public MiniMapController MiniMapController { get => miniMapController; set => miniMapController = value; }
        public XPBarController XPBarController { get => xpBarController; set => xpBarController = value; }
        public GameObject BottomPanel { get => bottomPanel; set => bottomPanel = value; }
        public GameObject SidePanel { get => sidePanel; set => sidePanel = value; }
        public GameObject MouseOverTarget { get => mouseOverTarget; set => mouseOverTarget = value; }
        public GameObject MouseOverWindow { get => mouseOverWindow; set => mouseOverWindow = value; }
        public GameObject ToolTip { get => toolTip; set => toolTip = value; }
        public CutSceneBarController CutSceneBarController { get => cutSceneBarController; set => cutSceneBarController = value; }
        public GameObject PlayerInterfaceCanvas { get => playerInterface; set => playerInterface = value; }
        public GameObject PopupWindowContainer { get => popupWindowContainer; set => popupWindowContainer = value; }
        public GameObject PopupPanelContainer { get => popupPanelContainer; set => popupPanelContainer = value; }
        public GameObject CombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }
        public bool DragInProgress { get => dragInProgress; set => dragInProgress = value; }
        public GameObject CutSceneBarsCanvas { get => cutSceneBarsCanvas; set => cutSceneBarsCanvas = value; }
        public CurrencyBarController ToolTipCurrencyBarController { get => toolTipCurrencyBarController; set => toolTipCurrencyBarController = value; }
        public GameObject PlayerUI { get => playerUI; }
        public Dictionary<string, float> DefaultWindowPositions { get => defaultWindowPositions; }

        public void PerformSetupActivities() {
            //Debug.Log("UIManager.PerformSetupActivities()");

            // activate in game UI to get default positions
            ActivateInGameUI();
            //return;
            // system menu needs to be activated so that the UI settings check can adjust its opacity
            ActivateSystemMenuUI();

            // this call will activate the player UI
            CheckUISettings(false);

            GetDefaultWindowPositions();

            // deactivate all UIs
            DeactivateInGameUI();
            DeactivateLoadingUI();
            DeactivatePlayerUI();
            DeactivateSystemMenuUI();

            // disable things that track characters
            playerUnitFrameController.ClearTarget();
            focusUnitFrameController.ClearTarget();
            miniMapController.ClearTarget();
            //Debug.Log("UIManager subscribing to characterspawn");
            CreateEventSubscriptions();

            if (PlayerManager.MyInstance.PlayerUnitSpawned) {
                ProcessPlayerUnitSpawn();
            }
            toolTipText = toolTip.GetComponentInChildren<TextMeshProUGUI>();

            // get references to all the items in the mouseover window we will need to update
            mouseOverText = mouseOverWindow.transform.GetComponentInChildren<TextMeshProUGUI>();

            DeActivateMouseOverWindow();
        }

        private void GetDefaultWindowPositions() {
            //Debug.Log("Savemanager.GetDefaultWindowPositions()");
            defaultWindowPositions.Add("AbilityBookWindowX", PopupWindowManager.Instance.abilityBookWindow.transform.position.x);
            defaultWindowPositions.Add("AbilityBookWindowY", PopupWindowManager.Instance.abilityBookWindow.transform.position.y);

            defaultWindowPositions.Add("SkillBookWindowX", PopupWindowManager.Instance.skillBookWindow.transform.position.x);
            defaultWindowPositions.Add("SkillBookWindowY", PopupWindowManager.Instance.skillBookWindow.transform.position.y);

            //Debug.Log("abilityBookWindowX: " + abilityBookWindowX + "; abilityBookWindowY: " + abilityBookWindowY);
            defaultWindowPositions.Add("ReputationBookWindowX", PopupWindowManager.Instance.reputationBookWindow.transform.position.x);
            defaultWindowPositions.Add("ReputationBookWindowY", PopupWindowManager.Instance.reputationBookWindow.transform.position.y);
            defaultWindowPositions.Add("CurrencyListWindowX", PopupWindowManager.Instance.currencyListWindow.transform.position.x);
            defaultWindowPositions.Add("CurrencyListWindowY", PopupWindowManager.Instance.currencyListWindow.transform.position.y);

            defaultWindowPositions.Add("CharacterPanelWindowX", PopupWindowManager.Instance.characterPanelWindow.transform.position.x);
            defaultWindowPositions.Add("CharacterPanelWindowY", PopupWindowManager.Instance.characterPanelWindow.transform.position.y);
            defaultWindowPositions.Add("LootWindowX", PopupWindowManager.Instance.lootWindow.transform.position.x);
            defaultWindowPositions.Add("LootWindowY", PopupWindowManager.Instance.lootWindow.transform.position.y);
            defaultWindowPositions.Add("VendorWindowX", PopupWindowManager.Instance.vendorWindow.transform.position.x);
            defaultWindowPositions.Add("VendorWindowY", PopupWindowManager.Instance.vendorWindow.transform.position.y);
            defaultWindowPositions.Add("ChestWindowX", PopupWindowManager.Instance.chestWindow.transform.position.x);
            defaultWindowPositions.Add("ChestWindowY", PopupWindowManager.Instance.chestWindow.transform.position.y);
            defaultWindowPositions.Add("BankWindowX", PopupWindowManager.Instance.bankWindow.transform.position.x);
            defaultWindowPositions.Add("BankWindowY", PopupWindowManager.Instance.bankWindow.transform.position.y);
            defaultWindowPositions.Add("QuestLogWindowX", PopupWindowManager.Instance.questLogWindow.transform.position.x);
            defaultWindowPositions.Add("QuestLogWindowY", PopupWindowManager.Instance.questLogWindow.transform.position.y);
            defaultWindowPositions.Add("AchievementListWindowX", PopupWindowManager.Instance.achievementListWindow.transform.position.x);
            defaultWindowPositions.Add("AchievementListWindowY", PopupWindowManager.Instance.achievementListWindow.transform.position.y);
            defaultWindowPositions.Add("QuestGiverWindowX", PopupWindowManager.Instance.questGiverWindow.transform.position.x);
            defaultWindowPositions.Add("QuestGiverWindowY", PopupWindowManager.Instance.questGiverWindow.transform.position.y);
            defaultWindowPositions.Add("SkillTrainerWindowX", PopupWindowManager.Instance.skillTrainerWindow.transform.position.x);
            defaultWindowPositions.Add("SkillTrainerWindowY", PopupWindowManager.Instance.skillTrainerWindow.transform.position.y);
            defaultWindowPositions.Add("InteractionWindowX", PopupWindowManager.Instance.interactionWindow.transform.position.x);
            defaultWindowPositions.Add("InteractionWindowY", PopupWindowManager.Instance.interactionWindow.transform.position.y);
            defaultWindowPositions.Add("CraftingWindowX", PopupWindowManager.Instance.craftingWindow.transform.position.x);
            defaultWindowPositions.Add("CraftingWindowY", PopupWindowManager.Instance.craftingWindow.transform.position.y);
            defaultWindowPositions.Add("MainMapWindowX", PopupWindowManager.Instance.mainMapWindow.transform.position.x);
            defaultWindowPositions.Add("MainMapWindowY", PopupWindowManager.Instance.mainMapWindow.transform.position.y);
            defaultWindowPositions.Add("QuestTrackerWindowX", QuestTrackerWindow.transform.position.x);
            defaultWindowPositions.Add("QuestTrackerWindowY", QuestTrackerWindow.transform.position.y);
            defaultWindowPositions.Add("CombatLogWindowX", CombatLogWindow.transform.position.x);
            defaultWindowPositions.Add("CombatLogWindowY", CombatLogWindow.transform.position.y);

            defaultWindowPositions.Add("MessageFeedManagerX", MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position.x);
            defaultWindowPositions.Add("MessageFeedManagerY", MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position.y);

            //Debug.Log("Saving FloatingCastBarController: " + MyFloatingCastBarController.transform.position.x + "; " + MyFloatingCastBarController.transform.position.y);
            defaultWindowPositions.Add("FloatingCastBarControllerX", FloatingCastBarController.transform.position.x);
            defaultWindowPositions.Add("FloatingCastBarControllerY", FloatingCastBarController.transform.position.y);

            defaultWindowPositions.Add("StatusEffectPanelControllerX", StatusEffectPanelController.transform.position.x);
            defaultWindowPositions.Add("StatusEffectPanelControllerY", StatusEffectPanelController.transform.position.y);

            defaultWindowPositions.Add("PlayerUnitFrameControllerX", PlayerUnitFrameController.transform.position.x);
            defaultWindowPositions.Add("PlayerUnitFrameControllerY", PlayerUnitFrameController.transform.position.y);

            defaultWindowPositions.Add("FocusUnitFrameControllerX", FocusUnitFrameController.transform.position.x);
            defaultWindowPositions.Add("FocusUnitFrameControllerY", FocusUnitFrameController.transform.position.y);

            defaultWindowPositions.Add("MiniMapControllerX", MiniMapController.transform.position.x);
            defaultWindowPositions.Add("MiniMapControllerY", MiniMapController.transform.position.y);

            defaultWindowPositions.Add("XPBarControllerX", XPBarController.transform.position.x);
            defaultWindowPositions.Add("XPBarControllerY", XPBarController.transform.position.y);

            defaultWindowPositions.Add("BottomPanelX", BottomPanel.transform.position.x);
            defaultWindowPositions.Add("BottomPanelY", BottomPanel.transform.position.y);

            defaultWindowPositions.Add("SidePanelX", SidePanel.transform.position.x);
            defaultWindowPositions.Add("SidePanelY", SidePanel.transform.position.y);

            defaultWindowPositions.Add("MouseOverWindowX", MouseOverWindow.transform.position.x);
            defaultWindowPositions.Add("MouseOverWindowY", MouseOverWindow.transform.position.y);
        }

        public void LoadDefaultWindowPositions() {
            //Debug.Log("UIManager.LoadDefaultWindowPositions()");

            PopupWindowManager.Instance.abilityBookWindow.transform.position = new Vector3(defaultWindowPositions["AbilityBookWindowX"], defaultWindowPositions["AbilityBookWindowY"], 0);
            PopupWindowManager.Instance.skillBookWindow.transform.position = new Vector3(defaultWindowPositions["SkillBookWindowX"], defaultWindowPositions["SkillBookWindowY"], 0);
            PopupWindowManager.Instance.reputationBookWindow.transform.position = new Vector3(defaultWindowPositions["ReputationBookWindowX"], defaultWindowPositions["ReputationBookWindowY"], 0);
            PopupWindowManager.Instance.currencyListWindow.transform.position = new Vector3(defaultWindowPositions["CurrencyListWindowX"], defaultWindowPositions["CurrencyListWindowY"], 0);
            PopupWindowManager.Instance.characterPanelWindow.transform.position = new Vector3(defaultWindowPositions["CharacterPanelWindowX"], defaultWindowPositions["CharacterPanelWindowY"], 0);
            PopupWindowManager.Instance.lootWindow.transform.position = new Vector3(defaultWindowPositions["LootWindowX"], defaultWindowPositions["LootWindowY"], 0);
            PopupWindowManager.Instance.vendorWindow.transform.position = new Vector3(defaultWindowPositions["VendorWindowX"], defaultWindowPositions["VendorWindowY"], 0);
            PopupWindowManager.Instance.chestWindow.transform.position = new Vector3(defaultWindowPositions["ChestWindowX"], defaultWindowPositions["ChestWindowY"], 0);
            PopupWindowManager.Instance.bankWindow.transform.position = new Vector3(defaultWindowPositions["BankWindowX"], defaultWindowPositions["BankWindowY"], 0);
            PopupWindowManager.Instance.questLogWindow.transform.position = new Vector3(defaultWindowPositions["QuestLogWindowX"], defaultWindowPositions["QuestLogWindowY"], 0);
            PopupWindowManager.Instance.achievementListWindow.transform.position = new Vector3(defaultWindowPositions["AchievementListWindowX"], defaultWindowPositions["AchievementListWindowY"], 0);
            PopupWindowManager.Instance.questGiverWindow.transform.position = new Vector3(defaultWindowPositions["QuestGiverWindowX"], defaultWindowPositions["QuestGiverWindowY"], 0);
            PopupWindowManager.Instance.skillTrainerWindow.transform.position = new Vector3(defaultWindowPositions["SkillTrainerWindowX"], defaultWindowPositions["SkillTrainerWindowY"], 0);
            PopupWindowManager.Instance.interactionWindow.transform.position = new Vector3(defaultWindowPositions["InteractionWindowX"], defaultWindowPositions["InteractionWindowY"], 0);
            PopupWindowManager.Instance.craftingWindow.transform.position = new Vector3(defaultWindowPositions["CraftingWindowX"], defaultWindowPositions["CraftingWindowY"], 0);
            PopupWindowManager.Instance.mainMapWindow.transform.position = new Vector3(defaultWindowPositions["MainMapWindowX"], defaultWindowPositions["MainMapWindowY"], 0);
            QuestTrackerWindow.transform.position = new Vector3(defaultWindowPositions["QuestTrackerWindowX"], defaultWindowPositions["QuestTrackerWindowY"], 0);
            CombatLogWindow.transform.position = new Vector3(defaultWindowPositions["CombatLogWindowX"], defaultWindowPositions["CombatLogWindowY"], 0);
            MessageFeedManager.MyInstance.MessageFeedGameObject.transform.position = new Vector3(defaultWindowPositions["MessageFeedManagerX"], defaultWindowPositions["MessageFeedManagerY"], 0);
            FloatingCastBarController.transform.position = new Vector3(defaultWindowPositions["FloatingCastBarControllerX"], defaultWindowPositions["FloatingCastBarControllerY"], 0);
            StatusEffectPanelController.transform.position = new Vector3(defaultWindowPositions["StatusEffectPanelControllerX"], defaultWindowPositions["StatusEffectPanelControllerY"], 0);
            PlayerUnitFrameController.transform.position = new Vector3(defaultWindowPositions["PlayerUnitFrameControllerX"], defaultWindowPositions["PlayerUnitFrameControllerY"], 0);
            FocusUnitFrameController.transform.position = new Vector3(defaultWindowPositions["FocusUnitFrameControllerX"], defaultWindowPositions["FocusUnitFrameControllerY"], 0);
            MiniMapController.transform.position = new Vector3(defaultWindowPositions["MiniMapControllerX"], defaultWindowPositions["MiniMapControllerY"], 0);
            XPBarController.transform.position = new Vector3(defaultWindowPositions["XPBarControllerX"], defaultWindowPositions["XPBarControllerY"], 0);
            BottomPanel.transform.position = new Vector3(defaultWindowPositions["BottomPanelX"], defaultWindowPositions["BottomPanelY"], 0);
            SidePanel.transform.position = new Vector3(defaultWindowPositions["SidePanelX"], defaultWindowPositions["SidePanelY"], 0);
            MouseOverWindow.transform.position = new Vector3(defaultWindowPositions["MouseOverWindowX"], defaultWindowPositions["MouseOverWindowY"], 0);
        }

        public void CheckMissingConfiguration() {
            if (playerInterface == null) {
                Debug.LogError("UIManager.CheckMissingConfiguration(): playerInterface not set.  Check inspector for missing value!");
            }
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            SystemEventManager.MyInstance.OnBeforePlayerConnectionSpawn += HandleBeforePlayerConnectionSpawn;
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn += HandlePlayerConnectionDespawn;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandleMainCamera);
                SystemEventManager.MyInstance.OnBeforePlayerConnectionSpawn -= HandleBeforePlayerConnectionSpawn;
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= HandlePlayerConnectionDespawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            dragInProgress = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        void Update() {

            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                // if there is no player, these windows shouldn't be open
                return;
            }
            // don't hide windows while binding keys
            if (KeyBindManager.MyInstance.MyBindName == string.Empty) {
                if (InputManager.MyInstance.KeyBindWasPressed("HIDEUI")) {
                    if (playerUI.gameObject.activeSelf) {
                        playerUI.SetActive(false);
                    } else {
                        playerUI.SetActive(true);
                    }
                }
            }
        }

        public void DeactivateInGameUI() {
            //Debug.Log("UIManager.DeactivateInGameUI()");
            if (PopupWindowManager.Instance != null) {
                PopupWindowManager.Instance.CloseAllWindows();
            }

            if (inGameUI.activeSelf == true) {
                inGameUI.SetActive(false);
            }
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandleMainCamera);
            dragInProgress = false;
        }

        public void ActivateInGameUI() {
            //Debug.Log("UIManager.ActivateInGameUI()");
            DeactivateLoadingUI();
            inGameUI.SetActive(true);
            //Debug.Break();
            //return;
            if (CameraManager.MyInstance != null) {
                CameraManager.MyInstance.DisableCutsceneCamera();
            }
            if (!PlayerManager.MyInstance.PlayerUnitSpawned) {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandleMainCamera);
            } else {
                InitializeMainCamera();
            }
            dragInProgress = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".UIManager.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void HandleMainCamera(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessMainCamera();
        }


        public void ProcessMainCamera() {
            InitializeMainCamera();
        }

        public void InitializeMainCamera() {
            CameraManager.MyInstance.MainCameraController.InitializeCamera(PlayerManager.MyInstance.ActiveUnitController.transform);
        }

        public void DeactivatePlayerUI() {
            //Debug.Log("UIManager.DeactivatePlayerUI()");
            playerUI.SetActive(false);
            HideToolTip();
        }

        public void ActivatePlayerUI() {
            //Debug.Log("UIManager.ActivatePlayerUI()");
            playerUI.SetActive(true);
            PlayerInterfaceCanvas.SetActive(true);
            PopupWindowContainer.SetActive(true);
            PopupPanelContainer.SetActive(true);
            CombatTextCanvas.SetActive(true);
            questTrackerWindow.OpenWindow();
            combatLogWindow.OpenWindow();
            UpdateLockUI();
        }

        public void DeactivateLoadingUI() {
            //Debug.Log("UIManager.DeactivateLoadingUI()");
            loadingCanvas.SetActive(false);
        }

        public void ActivateLoadingUI() {
            //Debug.Log("UIManager.ActivateLoadingUI()");
            DeactivateInGameUI();
            DeactivateSystemMenuUI();
            loadingCanvas.SetActive(true);
        }

        public void DeactivateSystemMenuUI() {
            //Debug.Log("UIManager.DeactivateSystemMenuUI()");
            systemMenuUI.SetActive(false);
            SystemWindowManager.MyInstance.CloseAllWindows();
        }

        public void ActivateSystemMenuUI() {
            //Debug.Log("UIManager.ActivateSystemMenuUI()");
            DeactivateLoadingUI();
            systemMenuUI.SetActive(true);
        }

        public void HandleBeforePlayerConnectionSpawn() {
            //Debug.Log("UIManager.HandleBeforePlayerConnectionSpawn()");

            // allow the player ability manager to send us events so we can redraw the ability list when it changes
            SystemEventManager.MyInstance.OnAbilityListChanged += HandleAbilityListChanged;
        }

        public void HandlePlayerConnectionDespawn() {
            //Debug.Log("UIManager.HandlePlayerConnectionDespawn()");
            SystemEventManager.MyInstance.OnAbilityListChanged -= HandleAbilityListChanged;
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("UIManager.HandlePlayerUnitSpawn()");
            ActivatePlayerUI();

            // some visuals can be dependent on zone restrictions so visuals should be updated
            ActionBarManager.UpdateVisuals();

            // allow the player ability manager to send us events so we can redraw the ability list when it changes
            //SystemEventManager.MyInstance.OnAbilityListChanged += HandleAbilityListChanged;

            // enable things that track the character
            // initialize unit frame
            playerUnitFrameController.SetTarget(PlayerManager.MyInstance.ActiveUnitController.NamePlateController);
            floatingCastBarController.SetTarget(PlayerManager.MyInstance.ActiveUnitController.NamePlateController as UnitNamePlateController);
            statusEffectPanelController.SetTarget(PlayerManager.MyInstance.ActiveUnitController);

            // intialize mini map
            InitializeMiniMapTarget(PlayerManager.MyInstance.ActiveUnitController.gameObject);
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("UIManager.HandleCharacterDespawn()");
            //SystemEventManager.MyInstance.OnAbilityListChanged -= HandleAbilityListChanged;
            DeInitializeMiniMapTarget();
            statusEffectPanelController.ClearTarget();
            focusUnitFrameController.ClearTarget();
            floatingCastBarController.ClearTarget();
            playerUnitFrameController.ClearTarget();
            //DeactivatePlayerUI();
        }

        public void InitializeMiniMapTarget(GameObject target) {
            miniMapController.SetTarget(target);
        }

        public void DeInitializeMiniMapTarget() {
            miniMapController.ClearTarget();
        }

        public void HandleAbilityListChanged(BaseAbility newAbility) {
            //Debug.Log("UIManager.HandleAbilityListChanged(" + (newAbility == null ? "null" : newAbility.MyName) + ")");
            // loop through ability bars and try to add ability
            if (actionBarManager != null) {
                if (newAbility.AutoAddToBars == true) {
                    if (!actionBarManager.AddNewAbility(newAbility)) {
                        //Debug.Log("UIManager.HandleAbilityListChanged(): All Ability Bars were full.  unable to add " + newAbility);
                    }
                } else {
                    //Debug.Log("UIManager.HandleAbilityListChanged(): " + newAbility + ".autoaddtobars = false");
                }
            } else {
                //Debug.Log("UIManager.HandleAbilityListChanged(): " + newAbility + ". actionbarmanager is null");
            }
        }

        public void ActivateMouseOverWindow(GameObject newFocus) {
            mouseOverTarget = newFocus;
            mouseOverWindow.SetActive(true);
            mouseOverText.text = newFocus.transform.name;
        }

        public void DeActivateMouseOverWindow() {
            mouseOverWindow.SetActive(false);
        }

        public void UpdateStackSize(IClickable clickable, int count, bool alwaysDisplayCount = false) {
            //Debug.Log("UpdateStackSize(" + count + ", " + alwaysDisplayCount + ")");
            if (count > 1 || alwaysDisplayCount == true) {
                if (clickable.StackSizeText.text != count.ToString()) {
                    clickable.StackSizeText.text = count.ToString();
                }
                if (clickable.StackSizeText.color != Color.white) {
                    clickable.StackSizeText.color = Color.white;
                }
                //clickable.MyIcon.color = Color.white;
            } else {
                ClearStackCount(clickable);
            }
        }

        public void ClearStackCount(IClickable clickable) {
            //Debug.Log("UIManager.ClearStackCount(" + clickable.ToString() + ")");
            if (clickable.StackSizeText.color != new Color(0, 0, 0, 0)) {
                clickable.StackSizeText.color = new Color(0, 0, 0, 0);
            }
            //clickable.MyIcon.color = Color.white;
        }

        /// <summary>
        /// set the background image for an item based on the item quality settings
        /// </summary>
        /// <param name="item"></param>
        /// <param name="backgroundImage"></param>
        /// <param name="defaultColor"></param>
        public void SetItemBackground(Item item, Image backgroundImage, Color defaultColor) {
            //Debug.Log("UIManager.SetItemBackground(" + item.DisplayName + ")");
            Color finalColor;
            if (item.ItemQuality != null) {
                if (item.ItemQuality.IconBackgroundImage != null) {
                    if (item.IconBackgroundImage != null) {
                        backgroundImage.sprite = item.IconBackgroundImage;
                    } else {
                        backgroundImage.sprite = item.ItemQuality.IconBackgroundImage;
                    }
                    if (item.ItemQuality.TintBackgroundImage == true) {
                        finalColor = item.ItemQuality.MyQualityColor;
                    } else {
                        finalColor = Color.white;
                    }
                } else {
                    finalColor = defaultColor;
                }
            } else {
                finalColor = defaultColor;
            }
            if (backgroundImage != null) {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image is not null, setting color: " + finalColor);
                backgroundImage.color = finalColor;
            } else {
                //Debug.Log(gameObject.name + ".WindowContentController.SetBackGroundColor(): background image IS NULL!");
            }

        }

        public void ShowToolTip(Vector3 position, IDescribable describable) {
            ShowToolTip(position, describable, string.Empty);
        }

        public void ShowToolTip(Vector3 position, IDescribable describable, string showSellPrice) {
            //Debug.Log("UIManager.ShowToolTip(): Input.MousePosition: " + Input.mousePosition + "; description: " + (describable == null ? "null" : describable.MyName));
            if (describable == null) {
                HideToolTip();
                return;
            }
            int pivotX;
            int pivotY;
            if (Input.mousePosition.x < (Screen.width / 2)) {
                pivotX = 0;
            } else {
                pivotX = 1;
            }
            if (Input.mousePosition.y < (Screen.height / 2)) {
                pivotY = 0;
            } else {
                pivotY = 1;
            }
            ShowToolTip(new Vector2(pivotX, pivotY), position, describable, showSellPrice);
        }

        public void ShowToolTip(Vector2 pivot, Vector3 position, IDescribable describable) {
            ShowToolTip(pivot, position, describable, string.Empty);
        }

        /// <summary>
        /// Show the tooltip
        /// </summary>
        public void ShowToolTip(Vector2 pivot, Vector3 position, IDescribable describable, string showSellPrice) {
            //Debug.Log("UIManager.ShowToolTip(" + pivot + ", " + position + ", " + (describable == null ? "null" : describable.DisplayName) + ", " + showSellPrice + ")");
            if (describable == null) {
                HideToolTip();
                return;
            }
            tooltipRect.pivot = pivot;
            toolTip.SetActive(true);

            toolTip.transform.position = position;
            ShowToolTipCommon(describable, showSellPrice);
            //toolTipText.text = description.GetDescription();

            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            float topPoint = tooltipRect.rect.yMax + position.y;
            float bottomPoint = tooltipRect.rect.yMin + position.y;
            //Debug.Log("screen height : " + Screen.height + "; position: " + position + "; top: " + tooltipRect.rect.yMax + "; bottom: " + tooltipRect.rect.yMin);

            // move up if too low
            if (bottomPoint < 0f) {
                toolTip.transform.position = new Vector3(toolTip.transform.position.x, (toolTip.transform.position.y - bottomPoint) + 20, toolTip.transform.position.z);
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            }

            // move down if too high
            if (topPoint > Screen.height) {
                toolTip.transform.position = new Vector3(toolTip.transform.position.x, toolTip.transform.position.y - ((topPoint - Screen.height) + 20), toolTip.transform.position.z);
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
            }


        }

        public void ShowToolTipCommon(IDescribable describable, string showSellPrice) {
            //Debug.Log("UIManager.ShowToolTipCommon(" + (describable == null ? "null" : describable.DisplayName) + ", " + showSellPrice + ")");
            if (describable == null) {
                HideToolTip();
                return;
            }

            // show new price
            toolTipText.text = describable.GetDescription();
            if (ToolTipCurrencyBarController != null) {
                ToolTipCurrencyBarController.ClearCurrencyAmounts();
                if (describable is Item && showSellPrice != string.Empty) {
                    //Debug.Log("UIManager.ShowToolTipCommon(" + (describable == null ? "null" : describable.MyName) + "): describable is item");
                    KeyValuePair<Currency, int> sellAmount = (describable as Item).MySellPrice;
                    if (sellAmount.Value == 0 || sellAmount.Key == null) {
                        //Debug.Log("UIManager.ShowToolTipCommon(" + (describable == null ? "null" : describable.MyName) + ")");
                        // don't print a s sell price on things that cannot be sold
                        return;
                    }
                    ToolTipCurrencyBarController.UpdateCurrencyAmount(sellAmount.Key, sellAmount.Value, showSellPrice);
                    //currencyAmountController.MyAmountText.text = "Vendor Price: " + sellAmount;
                }
            }

        }

        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public void HideToolTip() {
            //Debug.Log("UIManager.HideToolTip()");
            toolTip.SetActive(false);
        }
        public void RefreshTooltip(IDescribable describable) {
            RefreshTooltip(describable, string.Empty);
        }
        public void RefreshTooltip(IDescribable describable, string showSellPrice) {
            //Debug.Log("UIManager.RefreshTooltip(" + describable.MyName + ")");
            if (describable != null && toolTipText != null && toolTipText.text != null) {
                ShowToolTipCommon(describable, showSellPrice);
                //toolTipText.text = description.GetDescription();
            } else {
                HideToolTip();
            }
        }

        public void CheckQuestTrackerSettings() {
            if (PlayerPrefs.GetInt("UseQuestTracker") == 0) {
                if (questTrackerWindow.IsOpen) {
                    questTrackerWindow.CloseWindow();
                }
            } else if (PlayerPrefs.GetInt("UseQuestTracker") == 1) {
                if (!questTrackerWindow.IsOpen) {
                    questTrackerWindow.OpenWindow();
                }
            }
        }

        public void CheckCombatLogSettings() {
            if (PlayerPrefs.GetInt("UseCombatLog") == 0) {
                if (combatLogWindow.IsOpen) {
                    combatLogWindow.CloseWindow();
                }
            } else if (PlayerPrefs.GetInt("UseCombatLog") == 1) {
                if (!combatLogWindow.IsOpen) {
                    combatLogWindow.OpenWindow();
                }
            }
        }


        public void CheckUISettings(bool closeAfterUpdate = false) {
            //Debug.Log("UIManager.CheckUISettings()");

            // player interaface settings
            ActivatePlayerUI();

            CheckQuestTrackerSettings();
            CheckCombatLogSettings();
            UpdateActionBars();
            UpdateQuestTrackerOpacity();
            UpdateInventoryOpacity();
            UpdatePopupWindowOpacity();
            UpdateCombatLogOpacity();
            if (closeAfterUpdate) {
                DeactivatePlayerUI();
            }

            // system interface settings
            UpdateSystemMenuOpacity();
        }

        public void UpdateActionBars() {
            //Debug.Log("UIManager.UpdateActionBars()");
            if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                if (actionBarManager.ActionBarControllers[1].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[1].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar2") == 1) {
                if (!actionBarManager.ActionBarControllers[1].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[1].gameObject.SetActive(true);
                }
            }

            if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                if (actionBarManager.ActionBarControllers[2].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[2].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar3") == 1) {
                if (!actionBarManager.ActionBarControllers[2].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[2].gameObject.SetActive(true);
                }
            }

            if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                if (actionBarManager.ActionBarControllers[3].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[3].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar4") == 1) {
                if (!actionBarManager.ActionBarControllers[3].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[3].gameObject.SetActive(true);
                }
            }
            if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                if (actionBarManager.ActionBarControllers[4].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[4].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar5") == 1) {
                if (!actionBarManager.ActionBarControllers[4].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[4].gameObject.SetActive(true);
                }
            }
            if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                if (actionBarManager.ActionBarControllers[5].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[5].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar6") == 1) {
                if (!actionBarManager.ActionBarControllers[5].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[5].gameObject.SetActive(true);
                }
            }
            if (PlayerPrefs.GetInt("UseActionBar7") == 0) {
                if (actionBarManager.ActionBarControllers[6].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[6].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar7") == 1) {
                if (!actionBarManager.ActionBarControllers[6].gameObject.activeSelf) {
                    actionBarManager.ActionBarControllers[6].gameObject.SetActive(true);
                }
            }
            UpdateActionBarOpacity();
        }

        public void UpdateExperienceBar() {
            if (PlayerPrefs.GetInt("UseExperienceBar") == 0) {
                if (xpBarController.gameObject.activeSelf) {
                    xpBarController.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseExperienceBar") == 1) {
                if (!xpBarController.gameObject.activeSelf) {
                    xpBarController.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateMiniMap() {
            //Debug.Log("UIManager.UpdateMiniMap()");
            if (PlayerPrefs.GetInt("UseMiniMap") == 0) {
                //Debug.Log("UIManager.UpdateMiniMap(): use minimap is false");
                if (miniMapController.gameObject.activeSelf) {
                    //Debug.Log("UIManager.UpdateMiniMap(): use minimap is false and gameobject is active, setting active to false");
                    miniMapController.gameObject.SetActive(false);
                    //                miniMapController.ClearTarget();
                }
            } else if (PlayerPrefs.GetInt("UseMiniMap") == 1) {
                //Debug.Log("UIManager.UpdateMiniMap(): use minimap is true");
                if (!miniMapController.gameObject.activeSelf) {
                    //Debug.Log("UIManager.UpdateMiniMap(): use minimap is true and gameobject is not active, setting active to true");
                    miniMapController.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateFocusUnitFrame() {
            if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 0) {
                if (focusUnitFrameController.gameObject.activeSelf) {
                    focusUnitFrameController.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 1) {
                if (!focusUnitFrameController.gameObject.activeSelf) {
                    focusUnitFrameController.gameObject.SetActive(true);
                }
            }
        }

        public void UpdatePlayerUnitFrame() {
            if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 0) {
                if (playerUnitFrameController.gameObject.activeSelf) {
                    playerUnitFrameController.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 1) {
                if (!playerUnitFrameController.gameObject.activeSelf) {
                    playerUnitFrameController.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateFloatingCastBar() {
            if (PlayerPrefs.GetInt("UseFloatingCastBar") == 0) {
                if (floatingCastBarController.gameObject.activeSelf) {
                    floatingCastBarController.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseFloatingCastBar") == 1) {
                if (!floatingCastBarController.gameObject.activeSelf) {
                    floatingCastBarController.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateStatusEffectBar() {
            if (PlayerPrefs.GetInt("UseStatusEffectBar") == 0) {
                if (statusEffectPanelController.gameObject.activeSelf) {
                    statusEffectPanelController.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseStatusEffectBar") == 1) {
                if (!statusEffectPanelController.gameObject.activeSelf) {
                    statusEffectPanelController.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateFloatingCombatText() {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                if (CombatTextManager.MyInstance.MyCombatTextCanvas.gameObject.activeSelf) {
                    CombatTextManager.MyInstance.MyCombatTextCanvas.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseFloatingCastBar") == 1) {
                if (!CombatTextManager.MyInstance.MyCombatTextCanvas.gameObject.activeSelf) {
                    CombatTextManager.MyInstance.MyCombatTextCanvas.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateMessageFeed() {
            if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
                if (MessageFeedManager.MyInstance.MessageFeedGameObject.activeSelf) {
                    MessageFeedManager.MyInstance.MessageFeedGameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseMessageFeed") == 1) {
                if (!MessageFeedManager.MyInstance.MessageFeedGameObject.activeSelf) {
                    MessageFeedManager.MyInstance.MessageFeedGameObject.SetActive(true);
                }
            }
        }

        public void UpdateLockUI() {
            //Debug.Log("UIManager.UpdateLockUI()");
            MessageFeedManager.MyInstance.LockUI();
            floatingCastBarController.LockUI();
            statusEffectPanelController.LockUI();
            playerUnitFrameController.LockUI();
            focusUnitFrameController.LockUI();
            miniMapController.LockUI();
            xpBarController.LockUI();
            bottomPanel.GetComponent<DraggableWindow>().LockUI();
            sidePanel.GetComponent<DraggableWindow>().LockUI();
            mouseOverWindow.GetComponent<DraggableWindow>().LockUI();
            if (PlayerPrefs.HasKey("LockUI")) {
                //Debug.Log("UIManager.UpdateLockUI(): playerprefs has key LockUI");
                if (PlayerPrefs.GetInt("LockUI") == 0) {
                    //Debug.Log("UIManager.UpdateLockUI(): playerprefs has key LockUI and it IS 0");
                    mouseOverWindow.gameObject.SetActive(true);
                    floatingCastBarController.gameObject.SetActive(true);
                } else {
                    //Debug.Log("UIManager.UpdateLockUI(): playerprefs has key LockUI and it IS NOT 0");
                    mouseOverWindow.gameObject.SetActive(false);
                    floatingCastBarController.gameObject.SetActive(false);
                }
            }
        }

        public void UpdateQuestTrackerOpacity() {
            int opacityLevel = (int)(PlayerPrefs.GetFloat("QuestTrackerOpacity") * 255);
            QuestTrackerWindow.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void UpdateCombatLogOpacity() {
            int opacityLevel = (int)(PlayerPrefs.GetFloat("CombatLogOpacity") * 255);
            CombatLogWindow.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void UpdateActionBarOpacity() {
            //Debug.Log("UIManager.UpdateActionBarOpacity()");
            int opacityLevel = (int)(PlayerPrefs.GetFloat("ActionBarOpacity") * 255);
            foreach (ActionBarController actionBarController in actionBarManager.ActionBarControllers) {
                actionBarController.SetBackGroundColor(new Color32(0, 0, 0, 0));
                foreach (ActionButton actionButton in actionBarController.ActionButtons) {
                    actionButton.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
                }
            }
        }

        public void UpdateInventoryOpacity() {
            //Debug.Log("UIManager.UpdateInventoryOpacity()");

            int opacityLevel = (int)(PlayerPrefs.GetFloat("InventoryOpacity") * 255);
            int slotOpacityLevel = (int)(PlayerPrefs.GetFloat("InventorySlotOpacity") * 255);
            foreach (BagNode bagNode in InventoryManager.Instance.BagNodes) {
                //Debug.Log("UIManager.UpdateInventoryOpacity(): found bagNode");
                if (bagNode.BagPanel != null) {
                    //Debug.Log("UIManager.UpdateInventoryOpacity(): found bagNode and bagpanel is not null!");
                    bagNode.BagPanel.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
                    bagNode.BagPanel.SetSlotColor();
                }
                if (bagNode.BagButton != null) {
                    bagNode.BagButton.SetBackGroundColor();
                }
            }
            if (PopupWindowManager.Instance.bankWindow.CloseableWindowContents != null) {
                PopupWindowManager.Instance.bankWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            }

        }

        public void UpdatePopupWindowOpacity() {
            //Debug.Log("UIManager.UpdatePopupWindowOpacity()");
            int opacityLevel = (int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255);
            PopupWindowManager.Instance.abilityBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.achievementListWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.reputationBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.skillBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.skillTrainerWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.musicPlayerWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.characterPanelWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.craftingWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.currencyListWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.interactionWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.lootWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.mainMapWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.questGiverWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.questLogWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.vendorWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.Instance.dialogWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void UpdateSystemMenuOpacity() {
            int opacityLevel = (int)(PlayerPrefs.GetFloat("SystemMenuOpacity") * 255);
            SystemWindowManager.MyInstance.mainMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.nameChangeWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.characterCreatorWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.exitMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.inGameMainMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.keyBindConfirmWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.playMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.settingsMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.playerOptionsMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void SetLayerRecursive(GameObject objectName, int newLayer) {
            // set the preview unit layer to the PlayerPreview layer so the preview camera can see it and all other cameras will ignore it
            objectName.layer = newLayer;
            foreach (Transform childTransform in objectName.gameObject.GetComponentsInChildren<Transform>(true)) {
                childTransform.gameObject.layer = newLayer;
            }

        }

        public static bool MouseInRect(RectTransform rectTransform) {
            Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
            //Debug.Log(gameObject.name + ".MouseInRect(): local Mouse Position: " + localMousePosition + "; rectTransform.rect: " + rectTransform.rect);
            if (rectTransform.rect.Contains(localMousePosition)) {
                return true;
            }
            return false;
        }
    }

}