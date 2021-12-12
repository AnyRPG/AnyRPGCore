using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class UIManager : ConfiguredMonoBehaviour {

        [Header("UI Managers")]

        [SerializeField]
        private ActionBarManager actionBarManager = null;

        [SerializeField]
        private CombatTextManager combatTextManager = null;

        [SerializeField]
        private MessageFeedManager messageFeedManager = null;

        [SerializeField]
        private NamePlateManager namePlateManager = null;

        [SerializeField]
        private MapManager mapManager = null;

        [SerializeField]
        private MainMapManager mainMapManager = null;

        [SerializeField]
        private MiniMapManager miniMapManager = null;

        [SerializeField]
        private OnScreenKeyboardManager onScreenKeyboardManager = null;

        [Header("UI Elements")]

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
        private DraggableWindow bottomPanel = null;

        [SerializeField]
        private DraggableWindow sidePanel = null;

        [SerializeField]
        private DraggableWindow mouseOverWindow = null;

        [SerializeField]
        private GameObject playerInterface = null;

        [SerializeField]
        private GameObject popupWindowContainer = null;

        [SerializeField]
        private GameObject popupPanelContainer = null;

        [SerializeField]
        private GameObject combatTextCanvas = null;

        [SerializeField]
        private CutSceneBarController cutSceneBarController = null;

        [SerializeField]
        private InteractionTooltipController interactionTooltipController = null;

        [SerializeField]
        private TooltipController tooltipController = null;

        /*
        [SerializeField]
        private GameObject toolTip = null;

        private TextMeshProUGUI toolTipText = null;

        [SerializeField]
        private CurrencyBarController toolTipCurrencyBarController = null;

        [SerializeField]
        private RectTransform tooltipRect = null;
        */

        [SerializeField]
        private HandScript handScript = null;

        [Header("Player Interface Elements")]

        [SerializeField]
        private CloseableWindow playerUnitFrameWindow = null;

        [SerializeField]
        private UnitFrameController playerUnitFrameController = null;

        [SerializeField]
        private CloseableWindow focusUnitFrameWindow = null;

        [SerializeField]
        private UnitFrameController focusUnitFrameController = null;

        [SerializeField]
        private CloseableWindow statusEffectWindow = null;

        [SerializeField]
        private MiniMapController miniMapController = null;

        [SerializeField]
        private CloseableWindow miniMapWindow = null;

        [SerializeField]
        private CloseableWindow questTrackerWindow = null;

        [SerializeField]
        private CloseableWindow floatingCastBarWindow = null;

        [SerializeField]
        private CastBarController floatingCastBarController = null;

        [SerializeField]
        private CloseableWindow gamepadWindow = null;

        [SerializeField]
        private CloseableWindow xpBarWindow = null;

        [SerializeField]
        private XPBarController xpBarController = null;

        [SerializeField]
        private CloseableWindow combatLogWindow = null;


        [Header("Popup Windows")]

        public PagedWindow abilityBookWindow;
        public PagedWindow skillBookWindow;
        public PagedWindow reputationBookWindow;
        public PagedWindow currencyListWindow;
        public PagedWindow achievementListWindow;
        public CloseableWindow characterPanelWindow;
        public PagedWindow lootWindow;
        public PagedWindow vendorWindow;
        //public CloseableWindow chestWindow;
        public CloseableWindow bankWindow;
        public CloseableWindow inventoryWindow;
        public CloseableWindow questLogWindow;
        public CloseableWindow questGiverWindow;
        public CloseableWindow skillTrainerWindow;
        public CloseableWindow musicPlayerWindow;
        public CloseableWindow interactionWindow;
        public CloseableWindow craftingWindow;
        public CloseableWindow mainMapWindow;
        public CloseableWindow dialogWindow;
        public CloseableWindow factionChangeWindow;
        public CloseableWindow classChangeWindow;
        public CloseableWindow specializationChangeWindow;
        public CloseableWindow assignToActionBarsWindow;

        [Header("System Windows")]

        public CloseableWindow mainMenuWindow;
        public CloseableWindow inGameMainMenuWindow;
        public CloseableWindow gamepadMainMenuWindow;
        public CloseableWindow keyBindConfirmWindow;
        public CloseableWindow playerOptionsMenuWindow;
        public CloseableWindow characterCreatorWindow;
        public CloseableWindow unitSpawnWindow;
        public CloseableWindow petSpawnWindow;
        public CloseableWindow playMenuWindow;
        public CloseableWindow settingsMenuWindow;
        public CloseableWindow creditsWindow;
        public CloseableWindow exitMenuWindow;
        public CloseableWindow deleteGameMenuWindow;
        public CloseableWindow copyGameMenuWindow;
        public CloseableWindow loadGameWindow;
        public CloseableWindow newGameWindow;
        public CloseableWindow confirmDestroyMenuWindow;
        public CloseableWindow confirmCancelCutsceneMenuWindow;
        public CloseableWindow confirmSellItemMenuWindow;
        public CloseableWindow nameChangeWindow;
        public CloseableWindow exitToMainMenuWindow;
        public CloseableWindow confirmNewGameMenuWindow;
        public CloseableWindow onScreenKeyboardWindow;

        [Header("Navigable Interface Elements")]

        [SerializeField]
        private List<NavigableInterfaceElement> navigableInterfaceElements = new List<NavigableInterfaceElement>();

        private List<NavigableInterfaceElement> activeNavigableInterfaceElements = new List<NavigableInterfaceElement>();

        // objects in the mouseover window
        protected TextMeshProUGUI mouseOverText;
        protected GameObject mouseOverTarget;

        private int ignoreChangeLayer = 0;

        // keep track of window positions at startup in case of need to reset
        private Dictionary<string, float> defaultWindowPositions = new Dictionary<string, float>();

        // is a window currently being dragged.  used to suppres camera turn and pan
        private bool dragInProgress = false;

        private bool hadMoveable = false;

        protected bool eventSubscriptionsInitialized = false;

        /*
        // ui opacity defaults
        private float defaultInventoryOpacity = 0.5f;
        private float defaultActionBarOpacity = 0.5f;
        private float defaultQuestTrackerOpacity = 0.3f;
        private float defaultPopupWindowOpacity = 0.8f;
        private float defaultPagedButtonsOpacity = 0.8f;
        private float defaultInventorySlotOpacity = 0.5f;
        private float defaultSystemMenuOpacity = 0.8f;
        private float defaultCombatLogOpacity = 0.8f;

        // ui element visibility defaults
        private int defaultUseQuestTracker = 1;
        private int defaultUseActionBar2 = 1;
        private int defaultUseActionBar3 = 1;
        private int defaultUseActionBar4 = 1;
        private int defaultUseActionBar5 = 1;
        private int defaultUseActionBar6 = 1;
        private int defaultUseActionBar7 = 1;
        private int defaultUseFocusUnitFrameButton = 1;
        private int defaultUsePlayerUnitFrameButton = 1;
        private int defaultUseFloatingCastBarButton = 1;
        private int defaultUseMiniMapButton = 1;
        private int defaultUseExperienceBarButton = 1;
        private int defaultUseFloatingCombatTextButton = 1;
        private int defaultUseMessageFeedButton = 1;
        private int defaultUseStatusEffectBarButton = 1;
        private int defaultLockUIButton = 1;
        private int defaultUseCombatLogButton = 1;
        private int defaultShowPlayerNameButton = 1;
        private int defaultShowPlayerFactionButton = 1;
        private int defaultHideFullHealthBarButton = 1;
        */

        // game manager references
        private PlayerManager playerManager = null;
        private KeyBindManager keyBindManager = null;
        private InputManager inputManager = null;
        private CameraManager cameraManager = null;
        private InventoryManager inventoryManager = null;
        private SystemEventManager systemEventManager = null;
        private ControlsManager controlsManager = null;

        public CloseableWindow StatusEffectWindow { get => statusEffectWindow; }
        public UnitFrameController FocusUnitFrameController { get => focusUnitFrameController; }
        public ActionBarManager ActionBarManager { get => actionBarManager; set => actionBarManager = value; }
        public UnitFrameController PlayerUnitFrameController { get => playerUnitFrameController; set => playerUnitFrameController = value; }
        public CloseableWindow QuestTrackerWindow { get => questTrackerWindow; set => questTrackerWindow = value; }
        public CloseableWindow CombatLogWindow { get => combatLogWindow; set => combatLogWindow = value; }
        public CastBarController FloatingCastBarController { get => floatingCastBarController; set => floatingCastBarController = value; }
        public MiniMapController MiniMapController { get => miniMapController; set => miniMapController = value; }
        public XPBarController XPBarController { get => xpBarController; set => xpBarController = value; }
        public DraggableWindow BottomPanel { get => bottomPanel; set => bottomPanel = value; }
        public DraggableWindow SidePanel { get => sidePanel; set => sidePanel = value; }
        public GameObject MouseOverTarget { get => mouseOverTarget; set => mouseOverTarget = value; }
        public DraggableWindow MouseOverWindow { get => mouseOverWindow; set => mouseOverWindow = value; }
        public CutSceneBarController CutSceneBarController { get => cutSceneBarController; set => cutSceneBarController = value; }
        public GameObject PlayerInterfaceCanvas { get => playerInterface; set => playerInterface = value; }
        public GameObject PopupWindowContainer { get => popupWindowContainer; set => popupWindowContainer = value; }
        public GameObject PopupPanelContainer { get => popupPanelContainer; set => popupPanelContainer = value; }
        public GameObject CombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }
        public bool DragInProgress { get => dragInProgress; set => dragInProgress = value; }
        public GameObject CutSceneBarsCanvas { get => cutSceneBarsCanvas; set => cutSceneBarsCanvas = value; }
        public GameObject PlayerUI { get => playerUI; }
        public Dictionary<string, float> DefaultWindowPositions { get => defaultWindowPositions; }
        public CombatTextManager CombatTextManager { get => combatTextManager; set => combatTextManager = value; }
        public MessageFeedManager MessageFeedManager { get => messageFeedManager; set => messageFeedManager = value; }
        public NamePlateManager NamePlateManager { get => namePlateManager; set => namePlateManager = value; }
        public MainMapManager MainMapManager { get => mainMapManager; set => mainMapManager = value; }
        public MiniMapManager MiniMapManager { get => miniMapManager; set => miniMapManager = value; }
        public OnScreenKeyboardManager OnScreenKeyboardManager { get => onScreenKeyboardManager; set => onScreenKeyboardManager = value; }
        public HandScript HandScript { get => handScript; set => handScript = value; }
        public MapManager MapManager { get => mapManager; set => mapManager = value; }
        public int IgnoreChangeLayer { get => ignoreChangeLayer; }
        public CloseableWindow MiniMapWindow { get => miniMapWindow; set => miniMapWindow = value; }
        public List<NavigableInterfaceElement> NavigableInterfaceElements { get => activeNavigableInterfaceElements; set => activeNavigableInterfaceElements = value; }
        public CloseableWindow PlayerUnitFrameWindow { get => playerUnitFrameWindow; }
        public CloseableWindow FocusUnitFrameWindow { get => focusUnitFrameWindow; }
        public CloseableWindow FloatingCastBarWindow { get => floatingCastBarWindow; }
        public CloseableWindow XPBarWindow { get => xpBarWindow; }
        public CloseableWindow GamepadWindow { get => gamepadWindow; set => gamepadWindow = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            // initialize ui managers
            actionBarManager.Configure(systemGameManager);
            combatTextManager.Configure(systemGameManager);
            messageFeedManager.Configure(systemGameManager);
            namePlateManager.Configure(systemGameManager);
            mapManager.Configure(systemGameManager);
            mainMapManager.Configure(systemGameManager);
            miniMapManager.Configure(systemGameManager);
            onScreenKeyboardManager.Configure(systemGameManager);


            // initialize ui elements
            cutSceneBarController.Configure(systemGameManager);
            playerUnitFrameWindow.Configure(systemGameManager);
            focusUnitFrameWindow.Configure(systemGameManager);
            statusEffectWindow.Configure(systemGameManager);
            miniMapWindow.Configure(systemGameManager);
            floatingCastBarWindow.Configure(systemGameManager);
            gamepadWindow.Configure(systemGameManager);
            xpBarWindow.Configure(systemGameManager);
            bottomPanel.Configure(systemGameManager);
            sidePanel.Configure(systemGameManager);
            mouseOverWindow.Configure(systemGameManager);
            questTrackerWindow.Configure(systemGameManager);
            combatLogWindow.Configure(systemGameManager);
            tooltipController.Configure(systemGameManager);
            interactionTooltipController.Configure(systemGameManager);
            handScript.Configure(systemGameManager);

            // initialize popup windows
            abilityBookWindow.Configure(systemGameManager);
            skillBookWindow.Configure(systemGameManager);
            reputationBookWindow.Configure(systemGameManager);
            currencyListWindow.Configure(systemGameManager);
            achievementListWindow.Configure(systemGameManager);
            characterPanelWindow.Configure(systemGameManager);
            lootWindow.Configure(systemGameManager);
            vendorWindow.Configure(systemGameManager);
            //chestWindow.Configure(systemGameManager);
            bankWindow.Configure(systemGameManager);
            inventoryWindow.Configure(systemGameManager);
            questLogWindow.Configure(systemGameManager);
            questGiverWindow.Configure(systemGameManager);
            skillTrainerWindow.Configure(systemGameManager);
            musicPlayerWindow.Configure(systemGameManager);
            interactionWindow.Configure(systemGameManager);
            craftingWindow.Configure(systemGameManager);
            mainMapWindow.Configure(systemGameManager);
            dialogWindow.Configure(systemGameManager);
            factionChangeWindow.Configure(systemGameManager);
            classChangeWindow.Configure(systemGameManager);
            specializationChangeWindow.Configure(systemGameManager);
            assignToActionBarsWindow.Configure(systemGameManager);

            // initialize system windows
            mainMenuWindow.Configure(systemGameManager);
            inGameMainMenuWindow.Configure(systemGameManager);
            gamepadMainMenuWindow.Configure(systemGameManager);
            keyBindConfirmWindow.Configure(systemGameManager);
            playerOptionsMenuWindow.Configure(systemGameManager);
            characterCreatorWindow.Configure(systemGameManager);
            unitSpawnWindow.Configure(systemGameManager);
            petSpawnWindow.Configure(systemGameManager);
            playMenuWindow.Configure(systemGameManager);
            creditsWindow.Configure(systemGameManager);
            exitMenuWindow.Configure(systemGameManager);
            deleteGameMenuWindow.Configure(systemGameManager);
            copyGameMenuWindow.Configure(systemGameManager);
            loadGameWindow.Configure(systemGameManager);
            newGameWindow.Configure(systemGameManager);
            confirmDestroyMenuWindow.Configure(systemGameManager);
            confirmCancelCutsceneMenuWindow.Configure(systemGameManager);
            confirmSellItemMenuWindow.Configure(systemGameManager);
            nameChangeWindow.Configure(systemGameManager);
            exitToMainMenuWindow.Configure(systemGameManager);
            confirmNewGameMenuWindow.Configure(systemGameManager);
            onScreenKeyboardWindow.Configure(systemGameManager);

            // setting menu must go last because it checks all other windows opacity
            // which requires them to have configured their panels first
            settingsMenuWindow.Configure(systemGameManager);

            CreateEventSubscriptions();

            ignoreChangeLayer = LayerMask.NameToLayer("Equipment");

            //SetUIDefaults();

            settingsMenuWindow.Init();

            activeNavigableInterfaceElements.AddRange(navigableInterfaceElements);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            keyBindManager = systemGameManager.KeyBindManager;
            inputManager = systemGameManager.InputManager;
            cameraManager = systemGameManager.CameraManager;
            inventoryManager = systemGameManager.InventoryManager;
            systemEventManager = systemGameManager.SystemEventManager;
            controlsManager = systemGameManager.ControlsManager;
        }

        public void PerformSetupActivities() {
            //Debug.Log("UIManager.PerformSetupActivities()");

            // activate in game UI to get default positions
            ActivateInGameUI();

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

            if (playerManager.PlayerUnitSpawned) {
                ProcessPlayerUnitSpawn();
            }

            // get references to all the items in the mouseover window we will need to update
            mouseOverText = mouseOverWindow.transform.GetComponentInChildren<TextMeshProUGUI>();

            DeActivateMouseOverWindow();

        }

        /*
        void Start() {
            inventoryManager.Close();
        }
        */

        private void GetDefaultWindowPositions() {
            //Debug.Log("Savemanager.GetDefaultWindowPositions()");
            defaultWindowPositions.Add("AbilityBookWindowX", abilityBookWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("AbilityBookWindowY", abilityBookWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("SkillBookWindowX", skillBookWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("SkillBookWindowY", skillBookWindow.RectTransform.anchoredPosition.y);

            //Debug.Log("abilityBookWindowX: " + abilityBookWindowX + "; abilityBookWindowY: " + abilityBookWindowY);
            defaultWindowPositions.Add("ReputationBookWindowX", reputationBookWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("ReputationBookWindowY", reputationBookWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("CurrencyListWindowX", currencyListWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("CurrencyListWindowY", currencyListWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("CharacterPanelWindowX", characterPanelWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("CharacterPanelWindowY", characterPanelWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("LootWindowX", lootWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("LootWindowY", lootWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("VendorWindowX", vendorWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("VendorWindowY", vendorWindow.RectTransform.anchoredPosition.y);
            //defaultWindowPositions.Add("ChestWindowX", chestWindow.RectTransform.anchoredPosition.x);
            //defaultWindowPositions.Add("ChestWindowY", chestWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("BankWindowX", bankWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("BankWindowY", bankWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("InventoryWindowX", inventoryWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("InventoryWindowY", inventoryWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("QuestLogWindowX", questLogWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("QuestLogWindowY", questLogWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("AchievementListWindowX", achievementListWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("AchievementListWindowY", achievementListWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("QuestGiverWindowX", questGiverWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("QuestGiverWindowY", questGiverWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("SkillTrainerWindowX", skillTrainerWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("SkillTrainerWindowY", skillTrainerWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("MusicPlayerWindowX", musicPlayerWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("MusicPlayerWindowY", musicPlayerWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("InteractionWindowX", interactionWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("InteractionWindowY", interactionWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("CraftingWindowX", craftingWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("CraftingWindowY", craftingWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("MainMapWindowX", mainMapWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("MainMapWindowY", mainMapWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("DialogWindowX", dialogWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("DialogWindowY", dialogWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("QuestTrackerWindowX", QuestTrackerWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("QuestTrackerWindowY", QuestTrackerWindow.RectTransform.anchoredPosition.y);
            defaultWindowPositions.Add("CombatLogWindowX", CombatLogWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("CombatLogWindowY", CombatLogWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("MessageFeedManagerX", MessageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("MessageFeedManagerY", MessageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition.y);

            //Debug.Log("Saving FloatingCastBarController: " + MyFloatingCastBarController.transform.position.x + "; " + MyFloatingCastBarController.transform.position.y);
            defaultWindowPositions.Add("FloatingCastBarControllerX", FloatingCastBarWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("FloatingCastBarControllerY", FloatingCastBarWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("StatusEffectPanelControllerX", StatusEffectWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("StatusEffectPanelControllerY", StatusEffectWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("PlayerUnitFrameControllerX", playerUnitFrameWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("PlayerUnitFrameControllerY", playerUnitFrameWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("FocusUnitFrameControllerX", focusUnitFrameWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("FocusUnitFrameControllerY", focusUnitFrameWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("MiniMapControllerX", MiniMapWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("MiniMapControllerY", MiniMapWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("XPBarControllerX", XPBarWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("XPBarControllerY", XPBarWindow.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("BottomPanelX", BottomPanel.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("BottomPanelY", BottomPanel.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("SidePanelX", SidePanel.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("SidePanelY", SidePanel.RectTransform.anchoredPosition.y);

            defaultWindowPositions.Add("MouseOverWindowX", MouseOverWindow.RectTransform.anchoredPosition.x);
            defaultWindowPositions.Add("MouseOverWindowY", MouseOverWindow.RectTransform.anchoredPosition.y);
        }

        public void LoadDefaultWindowPositions() {
            //Debug.Log("UIManager.LoadDefaultWindowPositions()");

            // popup windowws
            abilityBookWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["AbilityBookWindowX"], defaultWindowPositions["AbilityBookWindowY"], 0);
            skillBookWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["SkillBookWindowX"], defaultWindowPositions["SkillBookWindowY"], 0);
            reputationBookWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["ReputationBookWindowX"], defaultWindowPositions["ReputationBookWindowY"], 0);
            currencyListWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["CurrencyListWindowX"], defaultWindowPositions["CurrencyListWindowY"], 0);
            characterPanelWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["CharacterPanelWindowX"], defaultWindowPositions["CharacterPanelWindowY"], 0);
            lootWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["LootWindowX"], defaultWindowPositions["LootWindowY"], 0);
            vendorWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["VendorWindowX"], defaultWindowPositions["VendorWindowY"], 0);
            //chestWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["ChestWindowX"], defaultWindowPositions["ChestWindowY"], 0);
            bankWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["BankWindowX"], defaultWindowPositions["BankWindowY"], 0);
            inventoryWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["InventoryWindowX"], defaultWindowPositions["InventoryWindowY"], 0);
            questLogWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["QuestLogWindowX"], defaultWindowPositions["QuestLogWindowY"], 0);
            achievementListWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["AchievementListWindowX"], defaultWindowPositions["AchievementListWindowY"], 0);
            questGiverWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["QuestGiverWindowX"], defaultWindowPositions["QuestGiverWindowY"], 0);
            skillTrainerWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["SkillTrainerWindowX"], defaultWindowPositions["SkillTrainerWindowY"], 0);
            musicPlayerWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["MusicPlayerWindowX"], defaultWindowPositions["MusicPlayerWindowY"], 0);
            interactionWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["InteractionWindowX"], defaultWindowPositions["InteractionWindowY"], 0);
            craftingWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["CraftingWindowX"], defaultWindowPositions["CraftingWindowY"], 0);
            mainMapWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["MainMapWindowX"], defaultWindowPositions["MainMapWindowY"], 0);
            dialogWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["DialogWindowX"], defaultWindowPositions["DialogWindowY"], 0);

            // ui elements
            QuestTrackerWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["QuestTrackerWindowX"], defaultWindowPositions["QuestTrackerWindowY"], 0);
            CombatLogWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["CombatLogWindowX"], defaultWindowPositions["CombatLogWindowY"], 0);
            MessageFeedManager.MessageFeedWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["MessageFeedManagerX"], defaultWindowPositions["MessageFeedManagerY"], 0);
            FloatingCastBarWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["FloatingCastBarControllerX"], defaultWindowPositions["FloatingCastBarControllerY"], 0);
            StatusEffectWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["StatusEffectPanelControllerX"], defaultWindowPositions["StatusEffectPanelControllerY"], 0);
            playerUnitFrameWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["PlayerUnitFrameControllerX"], defaultWindowPositions["PlayerUnitFrameControllerY"], 0);
            focusUnitFrameWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["FocusUnitFrameControllerX"], defaultWindowPositions["FocusUnitFrameControllerY"], 0);
            MiniMapWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["MiniMapControllerX"], defaultWindowPositions["MiniMapControllerY"], 0);
            XPBarWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["XPBarControllerX"], defaultWindowPositions["XPBarControllerY"], 0);
            BottomPanel.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["BottomPanelX"], defaultWindowPositions["BottomPanelY"], 0);
            SidePanel.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["SidePanelX"], defaultWindowPositions["SidePanelY"], 0);
            MouseOverWindow.RectTransform.anchoredPosition = new Vector3(defaultWindowPositions["MouseOverWindowX"], defaultWindowPositions["MouseOverWindowY"], 0);

        }

        public void CheckMissingConfiguration() {
            if (playerInterface == null) {
                Debug.LogError("UIManager.CheckMissingConfiguration(): playerInterface not set.  Check inspector for missing value!");
            }
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("UIManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            SystemEventManager.StartListening("OnBeforePlayerConnectionSpawn", HandleBeforePlayerConnectionSpawn);
            SystemEventManager.StartListening("OnPlayerConnectionSpawn", HandlePlayerConnectionSpawn);
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            Debug.Log("UIManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnLevelLoad", HandleLevelLoad);
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            //SystemEventManager.StopListening("OnPlayerUnitSpawn", HandleMainCamera);
            SystemEventManager.StopListening("OnBeforePlayerConnectionSpawn", HandleBeforePlayerConnectionSpawn);
            SystemEventManager.StopListening("OnPlayerConnectionSpawn", HandlePlayerConnectionSpawn);
            SystemEventManager.StopListening("OnPlayerConnectionDespawn", HandlePlayerConnectionDespawn);
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

        public void ShowInteractionTooltip(Interactable interactable) {
            if (controlsManager.GamePadModeActive == false) {
                return;
            }
            interactionTooltipController.ShowInteractionTooltip(interactable);
        }

        public void ShowToolTip(Vector3 position, IDescribable describable) {
            tooltipController.ShowToolTip(position, describable, string.Empty);
        }

        public void ShowToolTip(Vector2 pivot, Vector3 position, IDescribable describable) {
            tooltipController.ShowToolTip(pivot, position, describable);
        }

        public void ShowGamepadTooltip(RectTransform paneltransform, Transform buttonTransform, IDescribable describable, string sellPriceString) {
            tooltipController.ShowGamepadTooltip(paneltransform, buttonTransform, describable, sellPriceString);
        }

        public void RefreshTooltip(IDescribable describable) {
            tooltipController.RefreshTooltip(describable);
        }

        public void RefreshGamepadToolTip() {
            tooltipController.RefreshGamepadToolTip();
        }

        public void HideToolTip() {
            tooltipController.HideToolTip();
        }


        public void ProcessInput() {

            if (handScript.Moveable != null) {
                hadMoveable = true;
            } else {
                hadMoveable = false;
            }

            // don't hide windows while binding keys
            if (keyBindManager.BindName == string.Empty && playerManager.PlayerUnitSpawned != false) {

                // ui element keys pressed
                if (inputManager.KeyBindWasPressed("HIDEUI")) {
                    if (playerUI.gameObject.activeSelf) {
                        playerUI.SetActive(false);
                    } else {
                        playerUI.SetActive(true);
                    }
                }

                // popup window keys pressed
                if (inputManager.KeyBindWasPressed("INVENTORY")) {
                    inventoryWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("ABILITYBOOK")) {
                    abilityBookWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("SKILLBOOK")) {
                    skillBookWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("ACHIEVEMENTBOOK")) {
                    achievementListWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("REPUTATIONBOOK")) {
                    reputationBookWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("CURRENCYPANEL")) {
                    currencyListWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("CHARACTERPANEL")) {
                    characterPanelWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("QUESTLOG")) {
                    questLogWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("MAINMAP")) {
                    //Debug.Log("mainmap was pressed");
                    mainMapWindow.ToggleOpenClose();
                }

                if (playerUI.activeInHierarchy == true) {
                    handScript.ProcessInput();
                }
            }

            if (inputManager.KeyBindWasPressed("CANCELALL") && hadMoveable == false) {
                CloseAllPopupWindows();
            }

            // old system window manager code below : monitor for breakage

            if (mainMenuWindow.enabled == false && settingsMenuWindow.enabled == false) {
                return;
            }

            if (inputManager.KeyBindWasPressed("CANCELALL")) {
                settingsMenuWindow.CloseWindow();
                creditsWindow.CloseWindow();
                exitMenuWindow.CloseWindow();
                playMenuWindow.CloseWindow();
                deleteGameMenuWindow.CloseWindow();
                copyGameMenuWindow.CloseWindow();
                confirmDestroyMenuWindow.CloseWindow();
                confirmSellItemMenuWindow.CloseWindow();
                inGameMainMenuWindow.CloseWindow();
                gamepadMainMenuWindow.CloseWindow();
                petSpawnWindow.CloseWindow();

                // do not allow accidentally closing this while dead
                if (playerManager.PlayerUnitSpawned == true && playerManager.MyCharacter.CharacterStats.IsAlive != false) {
                    playerOptionsMenuWindow.CloseWindow();
                }
            }

            if (inputManager.KeyBindWasPressed("MAINMENU")) {
                if (controlsManager.GamePadModeActive == true) {
                    gamepadMainMenuWindow.ToggleOpenClose();
                } else {
                    inGameMainMenuWindow.ToggleOpenClose();
                }
            }

        }

        public void CloseAllPopupWindows() {
            //Debug.Log("CloseAllWindows()");
            abilityBookWindow.CloseWindow();
            skillBookWindow.CloseWindow();
            achievementListWindow.CloseWindow();
            reputationBookWindow.CloseWindow();
            currencyListWindow.CloseWindow();
            characterPanelWindow.CloseWindow();
            lootWindow.CloseWindow();
            vendorWindow.CloseWindow();
            //chestWindow.CloseWindow();
            bankWindow.CloseWindow();
            inventoryWindow.CloseWindow();
            questLogWindow.CloseWindow();
            questGiverWindow.CloseWindow();
            skillTrainerWindow.CloseWindow();
            musicPlayerWindow.CloseWindow();
            interactionWindow.CloseWindow();
            craftingWindow.CloseWindow();
            mainMapWindow.CloseWindow();
            dialogWindow.CloseWindow();
            factionChangeWindow.CloseWindow();
            classChangeWindow.CloseWindow();
            specializationChangeWindow.CloseWindow();
            assignToActionBarsWindow.CloseWindow();
        }

        public void CloseAllSystemWindows() {
            //Debug.Log("SystemWindowManager.CloseAllWindows()");
            mainMenuWindow.CloseWindow();
            inGameMainMenuWindow.CloseWindow();
            gamepadMainMenuWindow.CloseWindow();
            settingsMenuWindow.CloseWindow();
            creditsWindow.CloseWindow();
            exitMenuWindow.CloseWindow();
            playMenuWindow.CloseWindow();
            deleteGameMenuWindow.CloseWindow();
            copyGameMenuWindow.CloseWindow();
            confirmDestroyMenuWindow.CloseWindow();
            confirmSellItemMenuWindow.CloseWindow();
        }

        public void DeactivateInGameUI() {
            //Debug.Log("UIManager.DeactivateInGameUI()");
            CloseAllPopupWindows();

            if (inGameUI.activeSelf == true) {
                inGameUI.SetActive(false);
            }
            //SystemEventManager.StopListening("OnPlayerUnitSpawn", HandleMainCamera);
            dragInProgress = false;
        }

        public void ActivateInGameUI() {
            //Debug.Log("UIManager.ActivateInGameUI()");
            DeactivateLoadingUI();
            inGameUI.SetActive(true);
            //Debug.Break();
            //return;
            if (cameraManager != null) {
                cameraManager.DisableCutsceneCamera();
            }
            /*
            if (!playerManager.PlayerUnitSpawned) {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandleMainCamera);
            } else {
                InitializeMainCamera();
            }
            */
            dragInProgress = false;
        }

        
        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".UIManager.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }
        /*
        public void HandleMainCamera(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessMainCamera();
        }


        public void ProcessMainCamera() {
            InitializeMainCamera();
        }

        public void InitializeMainCamera() {
            cameraManager.MainCameraController.InitializeCamera(playerManager.ActiveUnitController.transform);
        }
        */

        public void DeactivatePlayerUI() {
            //Debug.Log("UIManager.DeactivatePlayerUI()");
            playerUI.SetActive(false);
            HideToolTip();
            HideInteractionToolTip();
        }

        public void ActivatePlayerUI() {
            //Debug.Log("UIManager.ActivatePlayerUI()");
            playerUI.SetActive(true);

            // canvases need to be activated because a cutscene could have deactivated them
            PlayerInterfaceCanvas.SetActive(true);
            PopupWindowContainer.SetActive(true);
            PopupPanelContainer.SetActive(true);
            CombatTextCanvas.SetActive(true);
        }

        public void InitializePlayerUI() {
            //Debug.Log("UIManager.InitializePlayerUI()");
            playerUI.SetActive(true);
            PlayerInterfaceCanvas.SetActive(true);
            PopupWindowContainer.SetActive(true);
            PopupPanelContainer.SetActive(true);
            CombatTextCanvas.SetActive(true);

            statusEffectWindow.OpenWindow();
            miniMapWindow.OpenWindow();
            questTrackerWindow.OpenWindow();
            messageFeedManager.MessageFeedWindow.OpenWindow();
            //actionBarManager.ActivateCorrectActionBars();
            // open gamepad window even though gamepad mode may not be active
            // so that the window position can be retrieved
            gamepadWindow.OpenWindow();
            xpBarWindow.OpenWindow();
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
            CloseAllSystemWindows();
        }

        public void ActivateSystemMenuUI() {
            //Debug.Log("UIManager.ActivateSystemMenuUI()");
            DeactivateLoadingUI();
            systemMenuUI.SetActive(true);
        }

        public void HandleBeforePlayerConnectionSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("UIManager.HandleBeforePlayerConnectionSpawn()");

            // allow the player ability manager to send us events so we can redraw the ability list when it changes
            systemEventManager.OnAbilityListChanged += HandleAbilityListChanged;
        }

        public void HandlePlayerConnectionSpawn(string eventName, EventParamProperties eventParamProperties) {
            SetupDeathPopup();
        }

        public void HandlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("UIManager.HandlePlayerConnectionDespawn()");
            RemoveDeathPopup();

            systemEventManager.OnAbilityListChanged -= HandleAbilityListChanged;
        }

        public void PlayerDeathHandler(CharacterStats characterStats) {
            //Debug.Log("PopupWindowManager.PlayerDeathHandler()");
            StartCoroutine(PerformDeathWindowDelay());
        }

        public IEnumerator PerformDeathWindowDelay() {
            float timeCount = 0f;
            while (timeCount < 2f) {
                yield return null;
                timeCount += Time.deltaTime;
            }
            playerOptionsMenuWindow.OpenWindow();
        }

        public void SetupDeathPopup() {
            //Debug.Log("PopupWindowmanager.SetupDeathPopup()");
            playerManager.MyCharacter.CharacterStats.OnDie += PlayerDeathHandler;
        }

        public void RemoveDeathPopup() {
            //Debug.Log("PopupWindowmanager.RemoveDeathPopup()");
            playerManager.MyCharacter.CharacterStats.OnDie -= PlayerDeathHandler;
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("UIManager.HandlePlayerUnitSpawn()");
            ActivatePlayerUI();

            // some visuals can be dependent on zone restrictions so visuals should be updated
            ActionBarManager.UpdateVisuals();

            // enable things that track the character
            // initialize unit frame
            playerUnitFrameController.SetTarget(playerManager.ActiveUnitController.NamePlateController);
            floatingCastBarController.SetTarget(playerManager.ActiveUnitController.NamePlateController as UnitNamePlateController);
            (statusEffectWindow.CloseableWindowContents as StatusEffectWindowPanel).SetTarget(playerManager.ActiveUnitController);

            // intialize mini map
            InitializeMiniMapTarget(playerManager.ActiveUnitController.gameObject);
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("UIManager.HandleCharacterDespawn()");
            DeInitializeMiniMapTarget();
            (statusEffectWindow.CloseableWindowContents as StatusEffectWindowPanel).ClearTarget();
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
            //Debug.Log("UIManager.HandleAbilityListChanged(" + (newAbility == null ? "null" : newAbility.DisplayName) + ")");
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
            mouseOverWindow.gameObject.SetActive(true);
            mouseOverText.text = newFocus.transform.name;
        }

        public void DeActivateMouseOverWindow() {
            mouseOverWindow.gameObject.SetActive(false);
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
            SetItemBackground(item, backgroundImage, defaultColor, item.ItemQuality);
        }

        /// <summary>
        /// set the background image for an item based on the item quality settings
        /// </summary>
        /// <param name="item"></param>
        /// <param name="backgroundImage"></param>
        /// <param name="defaultColor"></param>
        public void SetItemBackground(Item item, Image backgroundImage, Color defaultColor, ItemQuality itemQuality) {
            //Debug.Log("UIManager.SetItemBackground(" + item.DisplayName + ")");
            Color finalColor;
            if (itemQuality != null) {
                if (itemQuality.IconBackgroundImage != null) {
                    if (item.IconBackgroundImage != null) {
                        backgroundImage.sprite = item.IconBackgroundImage;
                    } else {
                        backgroundImage.sprite = itemQuality.IconBackgroundImage;
                    }
                    if (itemQuality.TintBackgroundImage == true) {
                        finalColor = itemQuality.QualityColor;
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

        public void HideInteractionToolTip() {
            //Debug.Log("UIManager.HideToolTip()");
            interactionTooltipController.HideInteractionTooltip();
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

        /// <summary>
        /// show or hide ui elements and adjust opacity according to current settings
        /// </summary>
        /// <param name="closeAfterUpdate"></param>
        public void CheckUISettings(bool closeAfterUpdate = false) {
            //Debug.Log("UIManager.CheckUISettings()");

            // player interaface settings
            InitializePlayerUI();

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

        public void ToggleGamepadMode() {
            //Debug.Log("UIManager.ToggleGamepadMode()");
            UpdateActionBars();
            UpdateLockUI();
        }

        public void UpdateActionBars() {
            //Debug.Log("UIManager.UpdateActionBars()");

            actionBarManager.ActivateCorrectActionBars();
            
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
                if (statusEffectWindow.gameObject.activeSelf) {
                    Debug.Log("Disabling status effect window");
                    statusEffectWindow.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseStatusEffectBar") == 1) {
                if (!statusEffectWindow.gameObject.activeSelf) {
                    Debug.Log("Enabling status effect window");
                    statusEffectWindow.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateFloatingCombatText() {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                if (CombatTextManager.CombatTextCanvas.gameObject.activeSelf) {
                    CombatTextManager.CombatTextCanvas.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseFloatingCastBar") == 1) {
                if (!CombatTextManager.CombatTextCanvas.gameObject.activeSelf) {
                    CombatTextManager.CombatTextCanvas.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateMessageFeed() {
            if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
                if (MessageFeedManager.MessageFeedWindow.gameObject.activeSelf) {
                    MessageFeedManager.MessageFeedWindow.gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseMessageFeed") == 1) {
                if (!MessageFeedManager.MessageFeedWindow.gameObject.activeSelf) {
                    MessageFeedManager.MessageFeedWindow.gameObject.SetActive(true);
                }
            }
        }

        public void UpdateLockUI() {
            //Debug.Log("UIManager.UpdateLockUI()");
            playerUnitFrameWindow.LockUI();
            focusUnitFrameWindow.LockUI();
            statusEffectWindow.LockUI();
            miniMapWindow.LockUI();
            MessageFeedManager.LockUI();
            floatingCastBarWindow.LockUI();
            xpBarWindow.LockUI();
            if (!controlsManager.GamePadModeActive) {
                bottomPanel.LockUI();
                sidePanel.LockUI();
                mouseOverWindow.LockUI();
            }
            if (PlayerPrefs.HasKey("LockUI")) {
                //Debug.Log("UIManager.UpdateLockUI(): playerprefs has key LockUI");
                if (PlayerPrefs.GetInt("LockUI") == 0) {
                    //Debug.Log("UIManager.UpdateLockUI(): playerprefs has key LockUI and it IS 0");
                    if (controlsManager.GamePadModeActive == false) {
                        mouseOverWindow.gameObject.SetActive(true);
                    }
                    AddNavigableInterfaceElement(playerUnitFrameController);
                    focusUnitFrameWindow.OpenWindow();
                    AddNavigableInterfaceElement(focusUnitFrameController);
                    AddNavigableInterfaceElement(messageFeedManager.MessageFeedWindow.CloseableWindowContents as NavigableInterfaceElement);
                    floatingCastBarController.gameObject.SetActive(true);
                    AddNavigableInterfaceElement(floatingCastBarWindow.CloseableWindowContents as FloatingCastbarPanel);
                    AddNavigableInterfaceElement(xpBarController);
                } else {
                    //Debug.Log("UIManager.UpdateLockUI(): playerprefs has key LockUI and it IS NOT 0");
                    if (controlsManager.GamePadModeActive == false) {
                        mouseOverWindow.gameObject.SetActive(false);
                    }
                    RemoveNavigableInterfaceElement(playerUnitFrameController);
                    RemoveNavigableInterfaceElement(focusUnitFrameController);
                    if (focusUnitFrameController.UnitNamePlateController == null) {
                        focusUnitFrameController.ClearTarget();
                    }
                    RemoveNavigableInterfaceElement(messageFeedManager.MessageFeedWindow.CloseableWindowContents as NavigableInterfaceElement);
                    floatingCastBarController.gameObject.SetActive(false);
                    RemoveNavigableInterfaceElement(floatingCastBarWindow.CloseableWindowContents as FloatingCastbarPanel);
                    RemoveNavigableInterfaceElement(xpBarController);
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
            foreach (ActionBarController actionBarController in actionBarManager.GamepadActionBarControllers) {
                actionBarController.SetBackGroundColor(new Color32(0, 0, 0, 0));
                foreach (ActionButton actionButton in actionBarController.ActionButtons) {
                    actionButton.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
                }
            }

        }

        public void UpdateInventoryOpacity() {
            //Debug.Log("UIManager.UpdateInventoryOpacity()");

            int opacityLevel = (int)(PlayerPrefs.GetFloat("InventoryOpacity") * 255);
            //int slotOpacityLevel = (int)(PlayerPrefs.GetFloat("InventorySlotOpacity") * 255);
            inventoryManager.SetSlotBackgroundColor();
            if (bankWindow.CloseableWindowContents != null) {
                bankWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            }
            if (inventoryWindow.CloseableWindowContents != null) {
                inventoryWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            }

        }

        public void UpdatePopupWindowOpacity() {
            //Debug.Log("UIManager.UpdatePopupWindowOpacity()");
            int opacityLevel = (int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255);
            abilityBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            achievementListWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            reputationBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            skillBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            skillTrainerWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            musicPlayerWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            characterPanelWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            craftingWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            currencyListWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            interactionWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            lootWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            mainMapWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            dialogWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            questGiverWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            questLogWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            vendorWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void UpdateSystemMenuOpacity() {
            int opacityLevel = (int)(PlayerPrefs.GetFloat("SystemMenuOpacity") * 255);
            mainMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            nameChangeWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            deleteGameMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            characterCreatorWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            exitMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            inGameMainMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            gamepadMainMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            keyBindConfirmWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            playMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            settingsMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            playerOptionsMenuWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            onScreenKeyboardWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void SetLayerRecursive(GameObject objectName, int newLayer) {
            // set the preview unit layer to the PlayerPreview layer so the preview camera can see it and all other cameras will ignore it
            objectName.layer = newLayer;
            foreach (Transform childTransform in objectName.gameObject.GetComponentsInChildren<Transform>(true)) {
                if (childTransform.gameObject.layer != ignoreChangeLayer) {
                    childTransform.gameObject.layer = newLayer;
                }
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

        /*
        public void SetUIDefaults() {
            if (!PlayerPrefs.HasKey("InventoryOpacity")) {
                PlayerPrefs.SetFloat("InventoryOpacity", defaultInventoryOpacity);
            }
            if (!PlayerPrefs.HasKey("InventorySlotOpacity")) {
                PlayerPrefs.SetFloat("InventorySlotOpacity", defaultInventorySlotOpacity);
            }
            if (!PlayerPrefs.HasKey("ActionBarOpacity")) {
                PlayerPrefs.SetFloat("ActionBarOpacity", defaultActionBarOpacity);
            }
            if (!PlayerPrefs.HasKey("QuestTrackerOpacity")) {
                PlayerPrefs.SetFloat("QuestTrackerOpacity", defaultQuestTrackerOpacity);
            }
            if (!PlayerPrefs.HasKey("CombatLogOpacity")) {
                PlayerPrefs.SetFloat("CombatLogOpacity", defaultCombatLogOpacity);
            }
            if (!PlayerPrefs.HasKey("PopupWindowOpacity")) {
                PlayerPrefs.SetFloat("PopupWindowOpacity", defaultPopupWindowOpacity);
            }
            if (!PlayerPrefs.HasKey("PagedButtonsOpacity")) {
                PlayerPrefs.SetFloat("PagedButtonsOpacity", defaultPagedButtonsOpacity);
            }
            if (!PlayerPrefs.HasKey("SystemMenuOpacity")) {
                PlayerPrefs.SetFloat("SystemMenuOpacity", defaultSystemMenuOpacity);
            }
            if (!PlayerPrefs.HasKey("UseQuestTracker")) {
                PlayerPrefs.SetInt("UseQuestTracker", defaultUseQuestTracker);
            }
            if (!PlayerPrefs.HasKey("UseActionBar2")) {
                PlayerPrefs.SetInt("UseActionBar2", defaultUseActionBar2);
            }
            if (!PlayerPrefs.HasKey("UseActionBar3")) {
                PlayerPrefs.SetInt("UseActionBar3", defaultUseActionBar3);
            }
            if (!PlayerPrefs.HasKey("UseActionBar4")) {
                PlayerPrefs.SetInt("UseActionBar4", defaultUseActionBar4);
            }
            if (!PlayerPrefs.HasKey("UseActionBar5")) {
                PlayerPrefs.SetInt("UseActionBar5", defaultUseActionBar5);
            }
            if (!PlayerPrefs.HasKey("UseActionBar6")) {
                PlayerPrefs.SetInt("UseActionBar6", defaultUseActionBar6);
            }
            if (!PlayerPrefs.HasKey("UseActionBar7")) {
                PlayerPrefs.SetInt("UseActionBar7", defaultUseActionBar7);
            }
            if (!PlayerPrefs.HasKey("UseFocusUnitFrame")) {
                PlayerPrefs.SetInt("UseFocusUnitFrame", defaultUseFocusUnitFrameButton);
            }
            if (!PlayerPrefs.HasKey("UsePlayerUnitFrame")) {
                PlayerPrefs.SetInt("UsePlayerUnitFrame", defaultUsePlayerUnitFrameButton);
            }
            if (!PlayerPrefs.HasKey("UseFloatingCastBar")) {
                PlayerPrefs.SetInt("UseFloatingCastBar", defaultUseFloatingCastBarButton);
            }
            if (!PlayerPrefs.HasKey("UseMiniMap")) {
                PlayerPrefs.SetInt("UseMiniMap", defaultUseMiniMapButton);
            }
            if (!PlayerPrefs.HasKey("UseExperienceBar")) {
                PlayerPrefs.SetInt("UseExperienceBar", defaultUseExperienceBarButton);
            }
            if (!PlayerPrefs.HasKey("UseFloatingCombatText")) {
                PlayerPrefs.SetInt("UseFloatingCombatText", defaultUseFloatingCombatTextButton);
            }
            if (!PlayerPrefs.HasKey("UseMessageFeed")) {
                PlayerPrefs.SetInt("UseMessageFeed", defaultUseMessageFeedButton);
            }
            if (!PlayerPrefs.HasKey("UseStatusEffectBar")) {
                PlayerPrefs.SetInt("UseStatusEffectBar", defaultUseStatusEffectBarButton);
            }
            if (!PlayerPrefs.HasKey("UseCombatLog")) {
                PlayerPrefs.SetInt("UseCombatLog", defaultUseCombatLogButton);
            }
            if (!PlayerPrefs.HasKey("LockUI")) {
                PlayerPrefs.SetInt("LockUI", defaultLockUIButton);
            }
            if (!PlayerPrefs.HasKey("ShowPlayerName")) {
                PlayerPrefs.SetInt("ShowPlayerName", defaultShowPlayerNameButton);
            }
            if (!PlayerPrefs.HasKey("ShowPlayerFaction")) {
                PlayerPrefs.SetInt("ShowPlayerFaction", defaultShowPlayerFactionButton);
            }
            if (!PlayerPrefs.HasKey("HideFullHealthBar")) {
                PlayerPrefs.SetInt("HideFullHealthBar", defaultHideFullHealthBarButton);
            }
        }
        */

        public void AddNavigableInterfaceElement(NavigableInterfaceElement navigableInterfaceElement) {
            if (activeNavigableInterfaceElements.Contains(navigableInterfaceElement) == false) {
                activeNavigableInterfaceElements.Add(navigableInterfaceElement);
            }
        }

        public void RemoveNavigableInterfaceElement(NavigableInterfaceElement navigableInterfaceElement) {
            if (activeNavigableInterfaceElements.Contains(navigableInterfaceElement)) {
                activeNavigableInterfaceElements.Remove(navigableInterfaceElement);
            }
        }
    }

}