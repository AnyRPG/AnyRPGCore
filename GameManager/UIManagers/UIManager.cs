using AnyRPG;
using System;
using System.Collections.Generic;
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
        private GameObject inGameUI;

        [SerializeField]
        private GameObject playerUI;

        [SerializeField]
        private GameObject systemMenuUI;

        [SerializeField]
        private GameObject loadingCanvas;

        [SerializeField]
        private GameObject inventoryCanvas;

        [SerializeField]
        private GameObject miniMapCanvasParent;

        [SerializeField]
        private GameObject cutSceneBarsCanvas;

        [SerializeField]
        private GameObject bottomPanel;

        [SerializeField]
        private GameObject sidePanel;

        [SerializeField]
        private GameObject mouseOverWindow;

        [SerializeField]
        private GameObject playerInterface;

        [SerializeField]
        private GameObject popupWindowContainer;

        [SerializeField]
        private GameObject popupPanelContainer;

        [SerializeField]
        private GameObject combatTextCanvas;

        [SerializeField]
        private UnitFrameController playerUnitFrameController;

        [SerializeField]
        private UnitFrameController focusUnitFrameController;

        [SerializeField]
        private MiniMapController miniMapController;

        [SerializeField]
        private CutSceneBarController cutSceneBarController;

        [SerializeField]
        private XPBarController xpBarController;

        [SerializeField]
        private StatusEffectPanelController statusEffectPanelController;

        [SerializeField]
        private CastBarController floatingCastBarController;

        [SerializeField]
        private CloseableWindow questTrackerWindow;

        [SerializeField]
        private CloseableWindow combatLogWindow;

        [SerializeField]
        private ActionBarManager actionBarManager;

        [SerializeField]
        private GameObject toolTip;

        private Text toolTipText;

        [SerializeField]
        private RectTransform tooltipRect;

        // objects in the mouseover window
        private Text mouseOverText;
        private GameObject mouseOverTarget;

        // is a window currently being dragged.  used to suppres camera turn and pan
        private bool dragInProgress = false;

        protected bool startHasRun = false;
        protected bool eventReferencesInitialized = false;

        public StatusEffectPanelController MyStatusEffectPanelController { get => statusEffectPanelController; }
        public UnitFrameController MyFocusUnitFrameController { get => focusUnitFrameController; }
        public ActionBarManager MyActionBarManager { get => actionBarManager; set => actionBarManager = value; }
        public GameObject MyMiniMapCanvasParent { get => miniMapCanvasParent; }
        public UnitFrameController MyPlayerUnitFrameController { get => playerUnitFrameController; set => playerUnitFrameController = value; }
        public CloseableWindow MyQuestTrackerWindow { get => questTrackerWindow; set => questTrackerWindow = value; }
        public CloseableWindow MyCombatLogWindow { get => combatLogWindow; set => combatLogWindow = value; }
        public CastBarController MyFloatingCastBarController { get => floatingCastBarController; set => floatingCastBarController = value; }
        public MiniMapController MyMiniMapController { get => miniMapController; set => miniMapController = value; }
        public XPBarController MyXPBarController { get => xpBarController; set => xpBarController = value; }
        public GameObject MyBottomPanel { get => bottomPanel; set => bottomPanel = value; }
        public GameObject MySidePanel { get => sidePanel; set => sidePanel = value; }
        public GameObject MyMouseOverTarget { get => mouseOverTarget; set => mouseOverTarget = value; }
        public GameObject MyMouseOverWindow { get => mouseOverWindow; set => mouseOverWindow = value; }
        public GameObject MyToolTip { get => toolTip; set => toolTip = value; }
        public CutSceneBarController MyCutSceneBarController { get => cutSceneBarController; set => cutSceneBarController = value; }
        public GameObject MyPlayerInterfaceCanvas { get => playerInterface; set => playerInterface = value; }
        public GameObject MyPopupWindowContainer { get => popupWindowContainer; set => popupWindowContainer = value; }
        public GameObject MyPopupPanelContainer { get => popupPanelContainer; set => popupPanelContainer = value; }
        public GameObject MyCombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }
        public bool MyDragInProgress { get => dragInProgress; set => dragInProgress = value; }
        public GameObject MyCutSceneBarsCanvas { get => cutSceneBarsCanvas; set => cutSceneBarsCanvas = value; }
        public GameObject MyInventoryCanvas { get => inventoryCanvas; set => inventoryCanvas = value; }

        public void PerformSetupActivities() {
            //Debug.Log("UIManager.PerformSetupActivities()");
            // deactivate all UIs
            DeactivateInGameUI();
            DeactivateLoadingUI();
            DeactivatePlayerUI();

            // disable things that track characters
            playerUnitFrameController.ClearTarget();
            focusUnitFrameController.ClearTarget();
            miniMapController.ClearTarget();
            startHasRun = true;
            //Debug.Log("UIManager subscribing to characterspawn");
            CreateEventReferences();

            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                HandlePlayerUnitSpawn();
            }
            toolTipText = toolTip.GetComponentInChildren<Text>();

            // get references to all the items in the mouseover window we will need to update
            mouseOverText = mouseOverWindow.transform.GetComponentInChildren<Text>();

            DeActivateMouseOverWindow();
        }

        public void CheckMissingConfiguration() {
            if (playerInterface == null) {
                Debug.LogError("UIManager.CheckMissingConfiguration(): playerInterface not set.  Check inspector for missing value!");
            }
        }

        private void CreateEventReferences() {
            //Debug.Log("PlayerManager.CreateEventReferences()");
            if (eventReferencesInitialized || !startHasRun) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            SystemEventManager.MyInstance.OnPlayerConnectionSpawn += HandlePlayerConnectionSpawn;
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn += HandlePlayerConnectionDespawn;
            eventReferencesInitialized = true;
        }

        private void CleanupEventReferences() {
            //Debug.Log("PlayerManager.CleanupEventReferences()");
            if (!eventReferencesInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= InitializeMainCamera;
                SystemEventManager.MyInstance.OnPlayerConnectionSpawn -= HandlePlayerConnectionSpawn;
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= HandlePlayerConnectionDespawn;
            }
            eventReferencesInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventReferences();
        }

        void Update() {

            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
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
            if (PopupWindowManager.MyInstance != null) {
                PopupWindowManager.MyInstance.CloseAllWindows();
            }

            if (inGameUI.activeSelf == true) {
                inGameUI.SetActive(false);
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= InitializeMainCamera;
        }

        public void ActivateInGameUI() {
            //Debug.Log("UIManager.ActivateInGameUI()");
            DeactivateLoadingUI();
            inGameUI.SetActive(true);
            if (AnyRPGCutsceneCameraController.MyInstance != null) {
                AnyRPGCutsceneCameraController.MyInstance.gameObject.SetActive(false);
            }
            if (!PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                SystemEventManager.MyInstance.OnPlayerUnitSpawn += InitializeMainCamera;
            } else {
                InitializeMainCamera();
            }
        }

        public void InitializeMainCamera() {
            CameraManager.MyInstance.MyMainCameraController.InitializeCamera(PlayerManager.MyInstance.MyPlayerUnitObject.transform);
        }

        public void DeactivatePlayerUI() {
            playerUI.SetActive(false);
            HideToolTip();
        }

        public void ActivatePlayerUI() {
            //Debug.Log("UIManager.ActivatePlayerUI()");
            playerUI.SetActive(true);
            MyPlayerInterfaceCanvas.SetActive(true);
            MyPopupWindowContainer.SetActive(true);
            MyPopupPanelContainer.SetActive(true);
            MyCombatTextCanvas.SetActive(true);
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

        public void HandlePlayerConnectionSpawn() {
            Debug.Log("UIManager.HandlePlayerConnectionSpawn()");

            // allow the player ability manager to send us events so we can redraw the ability list when it changes
            SystemEventManager.MyInstance.OnAbilityListChanged += HandleAbilityListChanged;
        }

        public void HandlePlayerConnectionDespawn() {
            Debug.Log("UIManager.HandlePlayerConnectionDespawn()");
            SystemEventManager.MyInstance.OnAbilityListChanged -= HandleAbilityListChanged;
        }

        public void HandlePlayerUnitSpawn() {
            Debug.Log("UIManager.HandlePlayerUnitSpawn()");
            ActivatePlayerUI();

            // allow the player ability manager to send us events so we can redraw the ability list when it changes
            //SystemEventManager.MyInstance.OnAbilityListChanged += HandleAbilityListChanged;

            // enable things that track the character
            // initialize unit frame
            playerUnitFrameController.SetTarget(PlayerManager.MyInstance.MyPlayerUnitObject);
            floatingCastBarController.SetTarget(PlayerManager.MyInstance.MyPlayerUnitObject);

            // intialize mini map
            InitializeMiniMapTarget(PlayerManager.MyInstance.MyPlayerUnitObject);
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("UIManager.HandleCharacterDespawn()");
            //SystemEventManager.MyInstance.OnAbilityListChanged -= HandleAbilityListChanged;
            DeInitializeMiniMapTarget();
            DeactivateUnitFrames();
            DeactivatePlayerUI();
        }

        public void InitializeMiniMapTarget(GameObject target) {
            miniMapController.SetTarget(target);
        }

        public void DeInitializeMiniMapTarget() {
            miniMapController.ClearTarget();
        }

        public void HandleAbilityListChanged(string newAbility) {
            Debug.Log("UIManager.HandleAbilityListChanged(" + (newAbility == null ? "null" : newAbility) + ")");
            // loop through ability bars and try to add ability
            if (actionBarManager != null) {
                if (SystemAbilityManager.MyInstance.GetResource(newAbility).MyAutoAddToBars == true) {
                    if (!actionBarManager.AddNewAbility(newAbility)) {
                        Debug.Log("UIManager.HandleAbilityListChanged(): All Ability Bars were full.  unable to add " + newAbility);
                    }
                } else {
                    Debug.Log("UIManager.HandleAbilityListChanged(): " + newAbility + ".autoaddtobars = false");
                }
            } else {
                Debug.Log("UIManager.HandleAbilityListChanged(): " + newAbility + ". actionbarmanager is null");
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
                clickable.MyStackSizeText.text = count.ToString();
                clickable.MyStackSizeText.color = Color.white;
                //clickable.MyIcon.color = Color.white;
            } else {
                ClearStackCount(clickable);
            }
        }

        public void ClearStackCount(IClickable clickable) {
            //Debug.Log("UIManager.ClearStackCount(" + clickable.ToString() + ")");
            clickable.MyStackSizeText.color = new Color(0, 0, 0, 0);
            //clickable.MyIcon.color = Color.white;
        }

        public void ShowToolTip(Vector3 position, IDescribable description) {
            //Debug.Log("UIManager.ShowToolTip(): " + Input.mousePosition);
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
            ShowToolTip(new Vector2(pivotX, pivotY), position, description);
        }

        /// <summary>
        /// Show the tooltip
        /// </summary>
        public void ShowToolTip(Vector2 pivot, Vector3 position, IDescribable description) {
            //Debug.Log("UIManager.ShowToolTip()");
            tooltipRect.pivot = pivot;
            toolTip.SetActive(true);
            toolTip.transform.position = position;
            toolTipText.text = description.GetDescription();
        }

        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public void HideToolTip() {
            toolTip.SetActive(false);
        }

        public void RefreshTooltip(IDescribable description) {
            //Debug.Log("UIManager.RefreshTooltip()");
            if (description != null && toolTipText != null && toolTipText.text != null) {
                toolTipText.text = description.GetDescription();
            } else {
                HideToolTip();
            }
        }

        public void DeactivateUnitFrames() {
            //Debug.Log("UIManager.DeactivateUnitFrames()");
            focusUnitFrameController.ClearTarget();
            floatingCastBarController.ClearTarget();
            playerUnitFrameController.ClearTarget();
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
            UpdateActionBarOpacity();
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
            if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                if (actionBarManager.MyActionBarControllers[1].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[1].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar2") == 1) {
                if (!actionBarManager.MyActionBarControllers[1].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[1].gameObject.SetActive(true);
                }
            }

            if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                if (actionBarManager.MyActionBarControllers[2].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[2].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar3") == 1) {
                if (!actionBarManager.MyActionBarControllers[2].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[2].gameObject.SetActive(true);
                }
            }

            if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                if (actionBarManager.MyActionBarControllers[3].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[3].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar4") == 1) {
                if (!actionBarManager.MyActionBarControllers[3].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[3].gameObject.SetActive(true);
                }
            }
            if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                if (actionBarManager.MyActionBarControllers[4].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[4].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar5") == 1) {
                if (!actionBarManager.MyActionBarControllers[4].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[4].gameObject.SetActive(true);
                }
            }
            if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                if (actionBarManager.MyActionBarControllers[5].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[5].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar6") == 1) {
                if (!actionBarManager.MyActionBarControllers[5].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[5].gameObject.SetActive(true);
                }
            }
            if (PlayerPrefs.GetInt("UseActionBar7") == 0) {
                if (actionBarManager.MyActionBarControllers[6].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[6].gameObject.SetActive(false);
                }
            } else if (PlayerPrefs.GetInt("UseActionBar7") == 1) {
                if (!actionBarManager.MyActionBarControllers[6].gameObject.activeSelf) {
                    actionBarManager.MyActionBarControllers[6].gameObject.SetActive(true);
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
            MyQuestTrackerWindow.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void UpdateCombatLogOpacity() {
            int opacityLevel = (int)(PlayerPrefs.GetFloat("CombatLogOpacity") * 255);
            MyCombatLogWindow.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void UpdateActionBarOpacity() {
            int opacityLevel = (int)(PlayerPrefs.GetFloat("ActionBarOpacity") * 255);
            foreach (ActionBarController actionBarController in actionBarManager.MyActionBarControllers) {
                actionBarController.SetBackGroundColor(new Color32(0, 0, 0, 0));
                foreach (ActionButton actionButton in actionBarController.MyActionButtons) {
                    actionButton.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
                }
            }
        }

        public void UpdateInventoryOpacity() {
            //Debug.Log("UIManager.UpdateInventoryOpacity()");

            int opacityLevel = (int)(PlayerPrefs.GetFloat("InventoryOpacity") * 255);
            int slotOpacityLevel = (int)(PlayerPrefs.GetFloat("InventorySlotOpacity") * 255);
            foreach (BagNode bagNode in InventoryManager.MyInstance.MyBagNodes) {
                //Debug.Log("UIManager.UpdateInventoryOpacity(): found bagNode");
                if (bagNode.MyBagPanel != null) {
                    //Debug.Log("UIManager.UpdateInventoryOpacity(): found bagNode and bagpanel is not null!");
                    bagNode.MyBagPanel.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
                    bagNode.MyBagPanel.SetSlotColor();
                }
                if (bagNode.MyBagButton != null) {
                    bagNode.MyBagButton.SetBackGroundColor();
                }
            }
            if (PopupWindowManager.MyInstance.bankWindow.MyCloseableWindowContents != null) {
                PopupWindowManager.MyInstance.bankWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            }

        }

        public void UpdatePopupWindowOpacity() {
            //Debug.Log("UIManager.UpdatePopupWindowOpacity()");
            int opacityLevel = (int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255);
            PopupWindowManager.MyInstance.abilityBookWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.achievementListWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.reputationBookWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.skillBookWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.characterPanelWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.craftingWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.currencyListWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.interactionWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.lootWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.mainMapWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.questGiverWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.questLogWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.vendorWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void UpdateSystemMenuOpacity() {
            int opacityLevel = (int)(PlayerPrefs.GetFloat("SystemMenuOpacity") * 255);
            SystemWindowManager.MyInstance.mainMenuWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.nameChangeWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.deleteGameMenuWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.characterCreatorWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.exitMenuWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.inGameMainMenuWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.keyBindConfirmWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.playMenuWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.settingsMenuWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            SystemWindowManager.MyInstance.playerOptionsMenuWindow.MyCloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
        }

        public void SetLayerRecursive(GameObject objectName, int newLayer) {
            // set the preview unit layer to the PlayerPreview layer so the preview camera can see it and all other cameras will ignore it
            objectName.layer = 12;
            foreach (Transform childTransform in objectName.gameObject.GetComponentsInChildren<Transform>(true)) {
                childTransform.gameObject.layer = newLayer;
            }

        }
    }

}