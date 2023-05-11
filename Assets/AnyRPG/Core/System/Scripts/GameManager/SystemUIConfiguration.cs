using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class SystemUIConfiguration {

        [Header("MINI MAP")]
        /*
        [Tooltip("If the the minimap texture for a scene cannot be found, what type of map display should be used")]
        [SerializeField]
        private MiniMapFallBackMode miniMapFallBackMode = MiniMapFallBackMode.Empty;
        */

        [Tooltip("When a minimap texture for a scene cannot be found, how many pixels per meter should be used when taking an automatic snapshot.  A higher number results in better image quality, but also higher memory usage.")]
        [SerializeField]
        private int autoPixelsPerMeter = 10;

        [Tooltip("The icon to show on the mini map to represent the player.")]
        [SerializeField]
        private Sprite playerMiniMapIcon = null;

        [Tooltip("The icon to show on the mini map to represent all NPC and PC characters that are not the current player.")]
        [SerializeField]
        private Sprite characterMiniMapIcon = null;

        [Header("UNIT FRAMES")]

        [Tooltip("Using a real time camera will reduce performance.")]
        [SerializeField]
        private bool realTimeUnitFrameCamera = false;

        [Header("COLORS")]

        [Tooltip("Default UI color for static elements that have no additional transparency applied to them.")]
        [SerializeField]
        private Color defaultUIColor = new Color32(254, 174, 2, 39);

        [Tooltip("Default UI color for background of UI sliders.")]
        [SerializeField]
        private Color defaultUIFillColor = new Color32(219, 152, 0, 255);

        [Tooltip("Default UI color for outline image, when the mouse is hovering over an image.")]
        [SerializeField]
        private Color highlightOutlineColor = new Color32(254, 174, 2, 102);

        [Tooltip("Default UI color for background highlight image, when a UI element has been clicked on and is the active image from a group of images.")]
        [SerializeField]
        private Color highlightImageColor = new Color32(254, 174, 2, 39);

        [Tooltip("Default UI color for the button image on highlight buttons.")]
        [SerializeField]
        private Color highlightButtonColor = new Color32(254, 174, 2, 255);

        [Tooltip("The normal color for button UI elements.")]
        [SerializeField]
        private Color buttonNormalColor = new Color32(163, 163, 163, 82);

        [Tooltip("The highlight color for button UI elements.")]
        [SerializeField]
        private Color buttonHighlightedColor = new Color32(165, 165, 165, 166);

        [Tooltip("The pressed color for button UI elements.")]
        [SerializeField]
        private Color buttonPressedColor = new Color32(120, 120, 120, 71);

        [Tooltip("The selected color for button UI elements.")]
        [SerializeField]
        private Color buttonSelectedColor = new Color32(165, 165, 165, 166);

        [Tooltip("The disabled color for button UI elements.")]
        [SerializeField]
        private Color buttonDisabledColor = new Color32(82, 82, 82, 17);

        [Header("IMAGES")]

        [Tooltip("The image to use for the frame of UI panel elements.")]
        [SerializeField]
        private Sprite defaultUIPanelFrame;

        [Tooltip("The faction icon to show on the load game screen when the player has no faction.")]
        [SerializeField]
        private Sprite defaultFactionIcon;

        [Header("SYSTEM BAR")]

        [Tooltip("The main menu icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarMainMenu;

        [Tooltip("The ability book icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarAbilityBook;

        [Tooltip("The character icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarCharacter;

        [Tooltip("The quest log icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarQuestLog;

        [Tooltip("The map icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarMap;

        [Tooltip("The skills icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarSkills;

        [Tooltip("The reputations icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarReputations;

        [Tooltip("The currencies icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarCurrencies;

        [Tooltip("The achievements icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarAchievements;

        [Tooltip("The inventory icon to show on the UI system bar.")]
        [SerializeField]
        private Sprite systemBarInventory;

        [Header("UI Elements")]

        [Tooltip("Whether or not to display the quest tracker.")]
        [SerializeField]
        private UIElementUsage useQuestTracker = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the quest tracker by default if user choice is selected.")]
        [SerializeField]
        private bool useQuestTrackerDefault = true;

        [Tooltip("Whether or not to display the system bar.")]
        [SerializeField]
        private UIElementUsage useSystemBar = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the system bar by default if user choice is selected.")]
        [SerializeField]
        private bool useSystemBarDefault = true;

        [Tooltip("Whether or not to display action bar 1.")]
        [SerializeField]
        private UIElementUsage useActionBar1 = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display action bar 1 by default if user choice is selected.")]
        [SerializeField]
        private bool useActionBar1Default = true;

        [Tooltip("Whether or not to display action bar 2.")]
        [SerializeField]
        private UIElementUsage useActionBar2 = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display action bar 2 by default if user choice is selected.")]
        [SerializeField]
        private bool useActionBar2Default = true;

        [Tooltip("Whether or not to display action bar 3.")]
        [SerializeField]
        private UIElementUsage useActionBar3 = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display action bar 3 by default if user choice is selected.")]
        [SerializeField]
        private bool useActionBar3Default = true;

        [Tooltip("Whether or not to display action bar 4.")]
        [SerializeField]
        private UIElementUsage useActionBar4 = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display action bar 4 by default if user choice is selected.")]
        [SerializeField]
        private bool useActionBar4Default = true;

        [Tooltip("Whether or not to display action bar 5.")]
        [SerializeField]
        private UIElementUsage useActionBar5 = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display action bar 5 by default if user choice is selected.")]
        [SerializeField]
        private bool useActionBar5Default = true;

        [Tooltip("Whether or not to display action bar 6.")]
        [SerializeField]
        private UIElementUsage useActionBar6 = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display action bar 6 by default if user choice is selected.")]
        [SerializeField]
        private bool useActionBar6Default = true;

        [Tooltip("Whether or not to display action bar 7.")]
        [SerializeField]
        private UIElementUsage useActionBar7 = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display action bar 7 by default if user choice is selected.")]
        [SerializeField]
        private bool useActionBar7Default = true;

        [Tooltip("Whether or not to display the player unit frame.")]
        [SerializeField]
        private UIElementUsage usePlayerUnitFrame = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the player unit frame by default if user choice is selected.")]
        [SerializeField]
        private bool usePlayerUnitFrameDefault = true;

        [Tooltip("Whether or not to display the target unit frame.")]
        [SerializeField]
        private UIElementUsage useTargetUnitFrame = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the target unit frame by default if user choice is selected.")]
        [SerializeField]
        private bool useTargetUnitFrameDefault = true;

        [Tooltip("Whether or not to display the floating cast bar.")]
        [SerializeField]
        private UIElementUsage useFloatingCastBar = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the floating cast bar by default if user choice is selected.")]
        [SerializeField]
        private bool useFloatingCastBarDefault = true;

        [Tooltip("Whether or not to display the mini map.")]
        [SerializeField]
        private UIElementUsage useMiniMap = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the mini map by default if user choice is selected.")]
        [SerializeField]
        private bool useMiniMapDefault = true;

        [Tooltip("Whether or not to display the experience bar.")]
        [SerializeField]
        private UIElementUsage useExperienceBar = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the experience bar by default if user choice is selected.")]
        [SerializeField]
        private bool useExperienceBarDefault = true;

        [Tooltip("Whether or not to display the floating combat text.")]
        [SerializeField]
        private UIElementUsage useFloatingCombatText = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the floating combat text by default if user choice is selected.")]
        [SerializeField]
        private bool useFloatingCombatTextDefault = true;

        [Tooltip("Whether or not to display the message feed.")]
        [SerializeField]
        private UIElementUsage useMessageFeed = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the message feed by default if user choice is selected.")]
        [SerializeField]
        private bool useMessageFeedDefault = true;

        [Tooltip("Whether or not to display the status effect bar.")]
        [SerializeField]
        private UIElementUsage useStatusEffectBar = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the status effect bar by default if user choice is selected.")]
        [SerializeField]
        private bool useStatusEffectBarDefault = true;

        [Tooltip("Whether or not to display the combat log.")]
        [SerializeField]
        private UIElementUsage useCombatLog = UIElementUsage.UserChoice;

        [Tooltip("Whether or not to display the combat log by default if user choice is selected.")]
        [SerializeField]
        private bool useCombatLogDefault = true;


        public Sprite SystemBarMainMenu { get => systemBarMainMenu; set => systemBarMainMenu = value; }
        public Sprite SystemBarAbilityBook { get => systemBarAbilityBook; set => systemBarAbilityBook = value; }
        public Sprite SystemBarCharacter { get => systemBarCharacter; set => systemBarCharacter = value; }
        public Sprite SystemBarQuestLog { get => systemBarQuestLog; set => systemBarQuestLog = value; }
        public Sprite SystemBarMap { get => systemBarMap; set => systemBarMap = value; }
        public Sprite SystemBarSkills { get => systemBarSkills; set => systemBarSkills = value; }
        public Sprite SystemBarReputations { get => systemBarReputations; set => systemBarReputations = value; }
        public Sprite SystemBarCurrencies { get => systemBarCurrencies; set => systemBarCurrencies = value; }
        public Sprite SystemBarAchievements { get => systemBarAchievements; set => systemBarAchievements = value; }
        public Sprite SystemBarInventory { get => systemBarInventory; set => systemBarInventory = value; }
        public Color DefaultUIColor { get => defaultUIColor; set => defaultUIColor = value; }
        public Color DefaultUIFillColor { get => defaultUIFillColor; set => defaultUIFillColor = value; }
        public Sprite DefaultFactionIcon { get => defaultFactionIcon; set => defaultFactionIcon = value; }

        public Sprite PlayerMiniMapIcon { get => playerMiniMapIcon; set => playerMiniMapIcon = value; }
        public Sprite CharacterMiniMapIcon { get => characterMiniMapIcon; set => characterMiniMapIcon = value; }
        public bool RealTimeUnitFrameCamera { get => realTimeUnitFrameCamera; set => realTimeUnitFrameCamera = value; }

        public int AutoPixelsPerMeter { get => autoPixelsPerMeter; set => autoPixelsPerMeter = value; }

        public Color ButtonNormalColor { get => buttonNormalColor; set => buttonNormalColor = value; }
        public Color ButtonHighlightedColor { get => buttonHighlightedColor; set => buttonHighlightedColor = value; }
        public Color ButtonPressedColor { get => buttonPressedColor; set => buttonPressedColor = value; }
        public Color ButtonSelectedColor { get => buttonSelectedColor; set => buttonSelectedColor = value; }
        public Color ButtonDisabledColor { get => buttonDisabledColor; set => buttonDisabledColor = value; }
        public Color HighlightOutlineColor { get => highlightOutlineColor; set => highlightOutlineColor = value; }
        public Color HighlightImageColor { get => highlightImageColor; set => highlightImageColor = value; }
        public Color HighlightButtonColor { get => highlightButtonColor; set => highlightButtonColor = value; }
        public UIElementUsage UseQuestTracker { get => useQuestTracker; set => useQuestTracker = value; }
        public UIElementUsage UseSystemBar { get => useSystemBar; set => useSystemBar = value; }
        public UIElementUsage UseActionBar1 { get => useActionBar1; set => useActionBar1 = value; }
        public UIElementUsage UseActionBar2 { get => useActionBar2; set => useActionBar2 = value; }
        public UIElementUsage UseActionBar3 { get => useActionBar3; set => useActionBar3 = value; }
        public UIElementUsage UseActionBar4 { get => useActionBar4; set => useActionBar4 = value; }
        public UIElementUsage UseActionBar5 { get => useActionBar5; set => useActionBar5 = value; }
        public UIElementUsage UseActionBar6 { get => useActionBar6; set => useActionBar6 = value; }
        public UIElementUsage UseActionBar7 { get => useActionBar7; set => useActionBar7 = value; }
        public UIElementUsage UsePlayerUnitFrame { get => usePlayerUnitFrame; set => usePlayerUnitFrame = value; }
        public UIElementUsage UseTargetUnitFrame { get => useTargetUnitFrame; set => useTargetUnitFrame = value; }
        public UIElementUsage UseFloatingCastBar { get => useFloatingCastBar; set => useFloatingCastBar = value; }
        public UIElementUsage UseMiniMap { get => useMiniMap; set => useMiniMap = value; }
        public UIElementUsage UseExperienceBar { get => useExperienceBar; set => useExperienceBar = value; }
        public UIElementUsage UseFloatingCombatText { get => useFloatingCombatText; set => useFloatingCombatText = value; }
        public UIElementUsage UseMessageFeed { get => useMessageFeed; set => useMessageFeed = value; }
        public UIElementUsage UseStatusEffectBar { get => useStatusEffectBar; set => useStatusEffectBar = value; }
        public UIElementUsage UseCombatLog { get => useCombatLog; set => useCombatLog = value; }
        public bool UseQuestTrackerDefault { get => useQuestTrackerDefault; set => useQuestTrackerDefault = value; }
        public bool UseSystemBarDefault { get => useSystemBarDefault; set => useSystemBarDefault = value; }
        public bool UseActionBar1Default { get => useActionBar1Default; set => useActionBar1Default = value; }
        public bool UseActionBar2Default { get => useActionBar2Default; set => useActionBar2Default = value; }
        public bool UseActionBar3Default { get => useActionBar3Default; set => useActionBar3Default = value; }
        public bool UseActionBar4Default { get => useActionBar4Default; set => useActionBar4Default = value; }
        public bool UseActionBar5Default { get => useActionBar5Default; set => useActionBar5Default = value; }
        public bool UseActionBar6Default { get => useActionBar6Default; set => useActionBar6Default = value; }
        public bool UseActionBar7Default { get => useActionBar7Default; set => useActionBar7Default = value; }
        public bool UsePlayerUnitFrameDefault { get => usePlayerUnitFrameDefault; set => usePlayerUnitFrameDefault = value; }
        public bool UseTargetUnitFrameDefault { get => useTargetUnitFrameDefault; set => useTargetUnitFrameDefault = value; }
        public bool UseFloatingCastBarDefault { get => useFloatingCastBarDefault; set => useFloatingCastBarDefault = value; }
        public bool UseMiniMapDefault { get => useMiniMapDefault; set => useMiniMapDefault = value; }
        public bool UseExperienceBarDefault { get => useExperienceBarDefault; set => useExperienceBarDefault = value; }
        public bool UseFloatingCombatTextDefault { get => useFloatingCombatTextDefault; set => useFloatingCombatTextDefault = value; }
        public bool UseMessageFeedDefault { get => useMessageFeedDefault; set => useMessageFeedDefault = value; }
        public bool UseStatusEffectBarDefault { get => useStatusEffectBarDefault; set => useStatusEffectBarDefault = value; }
        public bool UseCombatLogDefault { get => useCombatLogDefault; set => useCombatLogDefault = value; }
    }

    public enum UIElementUsage { Never, UserChoice, Always }

}