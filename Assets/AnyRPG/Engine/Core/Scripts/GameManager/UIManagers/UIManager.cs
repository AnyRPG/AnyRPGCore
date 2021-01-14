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
        private GameObject inventoryCanvas = null;

        [SerializeField]
        private GameObject miniMapCanvasParent = null;

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

        // is a window currently being dragged.  used to suppres camera turn and pan
        private bool dragInProgress = false;

        protected bool eventSubscriptionsInitialized = false;

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
        public GameObject MouseOverTarget { get => mouseOverTarget; set => mouseOverTarget = value; }
        public GameObject MouseOverWindow { get => mouseOverWindow; set => mouseOverWindow = value; }
        public GameObject MyToolTip { get => toolTip; set => toolTip = value; }
        public CutSceneBarController MyCutSceneBarController { get => cutSceneBarController; set => cutSceneBarController = value; }
        public GameObject MyPlayerInterfaceCanvas { get => playerInterface; set => playerInterface = value; }
        public GameObject MyPopupWindowContainer { get => popupWindowContainer; set => popupWindowContainer = value; }
        public GameObject MyPopupPanelContainer { get => popupPanelContainer; set => popupPanelContainer = value; }
        public GameObject MyCombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }
        public bool MyDragInProgress { get => dragInProgress; set => dragInProgress = value; }
        public GameObject MyCutSceneBarsCanvas { get => cutSceneBarsCanvas; set => cutSceneBarsCanvas = value; }
        public GameObject MyInventoryCanvas { get => inventoryCanvas; set => inventoryCanvas = value; }
        public CurrencyBarController MyToolTipCurrencyBarController { get => toolTipCurrencyBarController; set => toolTipCurrencyBarController = value; }

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
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
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
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandleMainCamera);
                SystemEventManager.MyInstance.OnBeforePlayerConnectionSpawn -= HandleBeforePlayerConnectionSpawn;
                SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= HandlePlayerConnectionDespawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
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
            if (PopupWindowManager.MyInstance != null) {
                PopupWindowManager.MyInstance.CloseAllWindows();
            }

            if (inGameUI.activeSelf == true) {
                inGameUI.SetActive(false);
            }
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandleMainCamera);
        }

        public void ActivateInGameUI() {
            //Debug.Log("UIManager.ActivateInGameUI()");
            DeactivateLoadingUI();
            inGameUI.SetActive(true);
            if (CameraManager.MyInstance != null) {
                CameraManager.MyInstance.DisableCutsceneCamera();
            }
            if (!PlayerManager.MyInstance.PlayerUnitSpawned) {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandleMainCamera);
            } else {
                InitializeMainCamera();
            }
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
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
            MyActionBarManager.UpdateVisuals();

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

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("UIManager.HandleCharacterDespawn()");
            //SystemEventManager.MyInstance.OnAbilityListChanged -= HandleAbilityListChanged;
            DeInitializeMiniMapTarget();
            statusEffectPanelController.ClearTarget();
            focusUnitFrameController.ClearTarget();
            floatingCastBarController.ClearTarget();
            playerUnitFrameController.ClearTarget();
            DeactivatePlayerUI();
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
            if (MyToolTipCurrencyBarController != null) {
                MyToolTipCurrencyBarController.ClearCurrencyAmounts();
                if (describable is Item && showSellPrice != string.Empty) {
                    //Debug.Log("UIManager.ShowToolTipCommon(" + (describable == null ? "null" : describable.MyName) + "): describable is item");
                    KeyValuePair<Currency, int> sellAmount = (describable as Item).MySellPrice;
                    if (sellAmount.Value == 0 || sellAmount.Key == null) {
                        //Debug.Log("UIManager.ShowToolTipCommon(" + (describable == null ? "null" : describable.MyName) + ")");
                        // don't print a s sell price on things that cannot be sold
                        return;
                    }
                    MyToolTipCurrencyBarController.UpdateCurrencyAmount(sellAmount.Key, sellAmount.Value, showSellPrice);
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
            if (PopupWindowManager.MyInstance.bankWindow.CloseableWindowContents != null) {
                PopupWindowManager.MyInstance.bankWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            }

        }

        public void UpdatePopupWindowOpacity() {
            //Debug.Log("UIManager.UpdatePopupWindowOpacity()");
            int opacityLevel = (int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255);
            PopupWindowManager.MyInstance.abilityBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.achievementListWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.reputationBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.skillBookWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.skillTrainerWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.musicPlayerWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.characterPanelWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.craftingWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.currencyListWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.interactionWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.lootWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.mainMapWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.questGiverWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.questLogWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.vendorWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
            PopupWindowManager.MyInstance.dialogWindow.CloseableWindowContents.SetBackGroundColor(new Color32(0, 0, 0, (byte)opacityLevel));
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