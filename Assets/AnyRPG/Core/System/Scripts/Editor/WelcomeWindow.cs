using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace AnyRPG {

    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow {

        public const string installedVersion = "1.0";

        private const string storyDemoGameScenePath = "AnyRPG/Addons/a-lost-soul-demo-games/Games/ALostSoulStoryDemo/Scenes/Game/ALostSoulStoryDemoGame/ALostSoulStoryDemoGame.unity";
        private const string characterDemoGameScenePath = "AnyRPG/Addons/a-lost-soul-demo-games/Games/ALostSoulCharacterDemo/Scenes/Game/ALostSoulCharacterDemoGame/ALostSoulCharacterDemoGame.unity";
        private const string featuresDemoGameScenePath = "AnyRPG/Core/Games/FeaturesDemoGame/Scenes/Game/FeaturesDemoGame/FeaturesDemoGame.unity";
        private const string movementTestGameScenePath = "AnyRPG/Addons/anyrpg-movement-test-game/Scenes/Game/MovementTestGame/MovementTestGame.unity";
        private const string mmoDemoGameScenePath = "AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemo/AnyMMODemo.unity";

        private const string storyDemoGamePath = "AnyRPG/Addons/a-lost-soul-demo-games/Games/ALostSoulStoryDemo";
        private const string characterDemoGamePath = "AnyRPG/Addons/a-lost-soul-demo-games/Games/ALostSoulCharacterDemo";
        private const string featuresDemoGamePath = "AnyRPG/Core/Games/FeaturesDemoGame";
        private const string movementTestGamePath = "AnyRPG/Addons/anyrpg-movement-test-game/Games/MovementTestGame";
        private const string mmoDemoGamePath = "AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo";
        private const string featuresDemoGameUMAExtensionsPath = "AnyRPG/Addons/anyrpg-uma/Games/FeaturesDemoGameExtensions";
        private const string mmoDemoGameUMAExtensionsPath = "AnyRPG/Addons/anymmo-fishnet-uma/Games/AnyMMODemoExtensions";

        private const string storyDemoBuildProfilePath = "AnyRPG/Addons/a-lost-soul-demo-games/Build Profiles/A Lost Soul Story Demo.asset";
        private const string characterDemoBuildProfilePath = "AnyRPG/Addons/a-lost-soul-demo-games/Build Profiles/A Lost Soul Character Demo.asset";
        private const string featuresDemoBuildProfilePath = "AnyRPG/Core/Build Profiles/Features Demo Game.asset";
        private const string movementTestGameBuildProfilePath = "AnyRPG/Addons/anyrpg-movement-test-game/Build Profiles/Movement Test Game.asset";
        private const string mmoDemoBuildProfilePath = "AnyRPG/Addons/anymmo-fishnet/Build Profiles/AnyMMO Demo Game.asset";

        private const string coreTemplateContentFolder = "AnyRPG/Core/Content";
        private const string cc0FantasyContentPackContentFolder = "AnyRPG/Addons/anyrpg-cc0-fantasy-content-pack/Content";
        private const string umaTemplateContentFolder = "AnyRPG/Addons/anyrpg-uma/Content";
        private const string fishNetTemplateContentFolder = "AnyRPG/Addons/anymmo-fishnet/Content";
        private const string fishNetUMATemplateContentFolder = "AnyRPG/Addons/anymmo-fishnet-uma/Content";

        public static Texture2D welcomeBanner = null;

        public static Vector2 scrollPosition;

        private int selectedTab = 0;
        private Vector2 scrollPos;

        // Layout Constants
        private const float SidebarWidth = 200f;

        //GUISkin skin;
        private const int windowMinWidth = 800;
        private const int windowMinHeight = 600;
        private const int windowInitialWidth = 900;
        private const int windowInitialHeight = 700;

        public static bool DisplayWelcomeWindow {
            get { return EditorPrefs.GetInt("AnyRPG_DisplayWelcome") == 1 ? true : false; }
            set {
                if (value != (EditorPrefs.GetInt("AnyRPG_DisplayWelcome") == 1 ? true : false)) {
                    EditorPrefs.SetInt("AnyRPG_DisplayWelcome", value == true ? 1 : 0);
                }
            }
        }

        [MenuItem("Tools/AnyRPG/Welcome Window", false, 0)]
        public static void Open() {
            // Changing 'true' to 'false' allows the window to be docked 
            // and usually survives recompiles with its size intact much better.
            WelcomeWindow window = GetWindow<WelcomeWindow>(false, "Welcome to AnyRPG", true);
            window.position = new Rect(100, 100, windowInitialWidth, windowInitialHeight); // Set a generous initial size
            window.Show();
        }

        public void OnEnable() {
            titleContent = new GUIContent("Welcome To AnyRPG");

            // Set a minimum size to prevent the UI from breaking
            minSize = new Vector2(windowMinWidth, windowMinHeight);

            // DO NOT set maxSize equal to minSize if you want to drag the corner.
            // If you want it to stay wide after recompile, let maxSize be large.
            maxSize = new Vector2(4000, 2000);

            InitStyle();
        }


        void InitStyle() {
            welcomeBanner = (Texture2D)Resources.Load("AnyRPGBanner", typeof(Texture2D));
        }

        private void OnGUI() {
            DrawHeader();

            // This Horizontal block acts as the "Body" of the window
            GUILayout.BeginHorizontal();

            // 1. NAVIGATION (Now on the Left)
            DrawSidebar();

            // 2. MAIN CONTENT (Now on the Right)
            DrawContentArea();

            GUILayout.EndHorizontal();

            DrawBottom();
        }

        private void DrawHeader() {
            if (welcomeBanner != null) {
                // 1. Get the current window width
                float windowWidth = position.width;
                float bannerHeight = welcomeBanner.height;

                // 2. Create a background rect that spans the full width
                Rect headerBackgroundRect = GUILayoutUtility.GetRect(windowWidth, bannerHeight);
                EditorGUI.DrawRect(headerBackgroundRect, Color.black);

                // 3. Calculate the centered position for the image
                float xPosition = (windowWidth - welcomeBanner.width) / 2f;
                Rect bannerRect = new Rect(xPosition, headerBackgroundRect.y, welcomeBanner.width, bannerHeight);

                // 4. Draw the banner centered
                GUI.DrawTexture(bannerRect, welcomeBanner);
            } else {
                // Fallback if banner is missing
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.FlexibleSpace();
                GUILayout.Label("AnyRPG", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private void DrawSidebar() {
            // Left-aligned sidebar with a distinct "box" look
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(SidebarWidth), GUILayout.ExpandHeight(true));

            // --- MAIN MENU ---
            GUILayout.Label("MENU", EditorStyles.boldLabel);
            GUILayout.Space(5);

            string[] mainTabs = { "Start Here", "Install Optional Addons", "Create Your Game", "Play Demo Games", "Support" };

            // Logic: If selectedTab is 0-4, show it in this grid. Otherwise, show nothing selected (-1).
            int newMainTab = GUILayout.SelectionGrid(selectedTab < 5 ? selectedTab : -1, mainTabs, 1, GUILayout.Height(mainTabs.Length * 45));

            // If user clicked a button in this first grid, update the global index
            if (newMainTab != -1) selectedTab = newMainTab;

            GUILayout.Space(20);

            // --- BUILD PREPARATION ---
            GUIStyle buildHeader = new GUIStyle(EditorStyles.boldLabel);
            //buildHeader.normal.textColor = new Color(0.4f, 0.7f, 1f); // Professional Blue
            GUILayout.Label("BUILD PREPARATION", buildHeader);
            GUILayout.Space(5);

            string[] buildTabs = { "Strip Demo Games", "Strip Template Resources" };

            // Logic: This grid handles indices 5 and 6. 
            // We pass (selectedTab - 5) to the grid so it sees 0 or 1.
            int secondaryTab = GUILayout.SelectionGrid(selectedTab >= 5 ? selectedTab - 5 : -1, buildTabs, 1, GUILayout.Height(buildTabs.Length * 45));

            // If user clicked a button in this second grid, offset it by 5 to update the global index
            if (secondaryTab != -1) {
                selectedTab = secondaryTab + 5;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }


        private void DrawContentArea() {
            // Scrollview handles pages that grow vertically
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // Add padding inside the content area
            GUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(15, 15, 15, 15) });

            switch (selectedTab) {
                case 0: DrawStartHereTab(); break;
                case 1: DrawAddonsTab(); break;
                case 2: DrawCreateYourGameTab(); break;
                case 3: DrawDemosTab(); break;
                case 4: DrawSupportTab(); break;
                case 5: DrawStripDemoGamesTab(); break;
                case 6: DrawStripTemplateResourcesTab(); break;
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private void DrawStartHereTab() {
            EditorGUILayout.LabelField("WELCOME TO ANYRPG", EditorStyles.boldLabel);

            GUIStyle welcomeStyle = new GUIStyle(EditorStyles.label) {
                wordWrap = true,
                fontSize = 12,
                padding = new RectOffset(0, 0, 0, 10)
            };
            EditorGUILayout.LabelField("Thank you for choosing AnyRPG. Follow the steps below to set up your project.", welcomeStyle);

            GUILayout.Space(5);

            // 1. PREREQUISITES
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("1. Prerequisites", EditorStyles.boldLabel);

            if (IsTextMeshProInstalled()) {
                DrawCustomInfoBox("TextMesh Pro Essentials are installed and ready.", "console.infoicon");
            } else {
                DrawCustomInfoBox("AnyRPG requires TextMesh Pro Essentials. Please import them to continue.", "console.warnicon");
                if (GUILayout.Button("Import TMP Essentials", GUILayout.Height(35))) {
                    EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
                }
            }
            GUILayout.EndVertical();

            GUILayout.Space(15);

            // 2. OPTIONAL ADDONS
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("2. Optional Addons", EditorStyles.boldLabel);
            DrawCustomInfoBox("Extend AnyRPG functionality with specialized features like UMA character customization or FishNet networking.", "console.infoicon");
            if (GUILayout.Button("Open Addon Manager", GUILayout.Height(35))) {
                EditorApplication.ExecuteMenuItem("Tools/AnyRPG/Addon Manager");
            }

            GUILayout.EndVertical();

            GUILayout.Space(15);

            // 3. CREATE YOUR GAME
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("3. Create Your Game", EditorStyles.boldLabel);
            DrawCustomInfoBox("Use the New Game Wizard to automate the creation of your Game Manager and scene structure.", "console.infoicon");
            if (GUILayout.Button("Open New Game Wizard", GUILayout.Height(35))) {
                NewGameWizard.CreateWizard();
            }

            GUILayout.EndVertical();
        }

        private bool IsTextMeshProInstalled() {
            // 1. Check for the physical "TMP Settings" asset (The "Essentials" signal)
            string settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            bool hasSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(settingsPath) != null;

            // 2. Check for the core Package in the assembly (The "Engine" signal)
            bool hasPackage = System.Type.GetType("TMPro.TMP_Settings, Unity.TextMeshPro") != null;

            return hasSettings && hasPackage;
        }
        
        private void DrawAddonsTab() {
            EditorGUILayout.LabelField("INSTALL OPTIONAL ADDONS", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 1. Context / Info Panel with the Large Infographic
            GUILayout.BeginVertical("box");
            DrawCustomInfoBox("Extend AnyRPG functionality with specialized features like UMA character customization or FishNet networking.", "console.infoicon");
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // 2. Action Panel
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Open Addon Manager", GUILayout.Height(40))) {
                EditorApplication.ExecuteMenuItem("Tools/AnyRPG/Addon Manager");
            }

            GUILayout.Space(10);

            // 3. Path Info with Large Infographic
            DrawCustomInfoBox("You can also find this at: Tools -> AnyRPG -> Addon Manager", "console.infoicon");

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Space(10);

        }

        private void DrawDemosTab() {
            EditorGUILayout.LabelField("PLAY DEMO GAMES", EditorStyles.boldLabel);
            GUILayout.Space(10);

            DrawDemoButton(
                    "A Lost Soul Story Demo Game",
                    storyDemoGameScenePath,
                    storyDemoBuildProfilePath,
                    "The first 2 chapters of the game, 'A Lost Soul', re-created using the open source assets included in AnyRPG"
                );

            DrawDemoButton(
                   "A Lost Soul Character Demo Game",
                   characterDemoGameScenePath,
                   characterDemoBuildProfilePath,
                   "Explore the game world of A Lost Soul by starting as any character model and faction included in the game"
               );

            DrawDemoButton(
                "Features Demo Game",
                featuresDemoGameScenePath,
                featuresDemoBuildProfilePath,
                "A simple 2 level game that provides examples of the most common features and interactables included in AnyRPG for quick reference when implementing them in your own game"
                );

            DrawDemoButton(
                "AnyMMO FishNet Demo Game",
                mmoDemoGameScenePath,
                mmoDemoBuildProfilePath,
                "A copy of the Features Demo Game designed for play over the network with multiple players."
                );

            DrawDemoButton(
                "Movement Test Game",
                movementTestGameScenePath,
                movementTestGameBuildProfilePath,
                "A simple one scene game with stairs, ramps, obstacles, and water suitable for testing character controller modifications"
                );


        }

        private void DrawDemoButton(string title, string scenePath, string profilePath, string description) {
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, scenePath))) return;

            GUILayout.BeginVertical("box");
            if (GUILayout.Button(title, GUILayout.Height(30))) {
                string fullPath = "Assets/" + profilePath;

                var buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(fullPath);

                if (buildProfile != null) {
                    BuildProfile.SetActiveBuildProfile(buildProfile);
                } else {
                    UnityEngine.Debug.LogError($"[AnyRPG] BuildProfile not found at: {fullPath}");
                }
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/" + scenePath);
            }

            // This reproduces the "HelpBox" background and border style exactly
            GUILayout.BeginVertical("helpBox");

            GUIStyle helpBoxTextStyle = new GUIStyle(EditorStyles.label);
            helpBoxTextStyle.wordWrap = true;
            helpBoxTextStyle.fontSize = 12; 
            helpBoxTextStyle.normal.textColor = EditorStyles.label.normal.textColor;

            EditorGUILayout.LabelField(description, helpBoxTextStyle);

            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void DrawBottom() {
            GUILayout.BeginHorizontal("box");

            DisplayWelcomeWindow = GUILayout.Toggle(DisplayWelcomeWindow, "Display this window at startup");
            GUILayout.FlexibleSpace();
            GUILayout.Label($"AnyRPG Version {installedVersion}", EditorStyles.miniLabel);

            GUILayout.EndHorizontal();
        }

        public void DrawCreateYourGameTab() {
            EditorGUILayout.LabelField("CREATE YOUR GAME", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 1. Context / Info Panel with the Large Infographic
            GUILayout.BeginVertical("box");
            DrawCustomInfoBox("While you can set up folders and prefabs manually, the New Game Wizard automates the creation of your Game Manager and scene structure.", "console.infoicon");
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // 2. Action Panel
            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Open New Game Wizard", GUILayout.Height(40))) {
                NewGameWizard.CreateWizard();
            }

            GUILayout.Space(10);

            // 3. Path Info with Large Infographic
            DrawCustomInfoBox("You can also find this at: Tools -> AnyRPG -> Wizard -> New Game Wizard", "console.infoicon");

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
        }

        // Helper with Larger Icon (Original HelpBox Size)
        private void DrawCustomInfoBox(string message, string iconName) {
            GUILayout.BeginVertical("helpBox");
            GUILayout.BeginHorizontal();

            // Draw the Icon (Increased to 32x32 to match original HelpBox scale)
            GUIContent icon = EditorGUIUtility.IconContent(iconName);
            Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(32, 32));
            // Added a small margin to the icon so it's not touching the border
            GUILayout.Label(icon, GUILayout.Width(40), GUILayout.Height(40));
            EditorGUIUtility.SetIconSize(oldIconSize);

            // Draw the Text (12pt)
            GUIStyle infoStyle = new GUIStyle(EditorStyles.label);
            infoStyle.wordWrap = true;
            infoStyle.fontSize = 12;
            // Padding adjusted to vertically center the 12pt text against the 32pt icon
            infoStyle.padding = new RectOffset(5, 0, 10, 0);

            EditorGUILayout.LabelField(message, infoStyle);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawSupportTab() {
            EditorGUILayout.LabelField("SUPPORT", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Official Website (The missing first item)
            DrawSupportPanel("Official Website", "https://anyrpg.org", "Download the latest AnyRPG Unity packages and playable games.");

            DrawSupportPanel("Documentation", "https://docs.anyrpg.org", "Full documentation of every feature included in AnyRPG.");

            DrawSupportPanel("YouTube Tutorials", "https://www.youtube.com/channel/UC-SiqAyRXR6eijPggFhFG2g", "Development live streams and tutorial screencasts.");

            DrawSupportPanel("Discord Community", "https://discord.gg/huSAuqk", "Get live help, share your work, and see real-time updates.");

            DrawSupportPanel("Source Code on GitHub", "https://github.com/AnyRPG", "Report bugs and test features under development.");

            DrawSupportPanel("Project Roadmap", "https://trello.com/w/anyrpg", "Suggest new features and see the project status.");
        }

        private void DrawSupportPanel(string title, string url, string description) {
            GUILayout.BeginVertical("box");

            if (GUILayout.Button(title, GUILayout.Height(30))) {
                Application.OpenURL(url);
            }

            // Nested helpBox style for the description to match the Demo buttons
            GUILayout.BeginVertical("helpBox");

            GUIStyle helpBoxTextStyle = new GUIStyle(EditorStyles.label);
            helpBoxTextStyle.wordWrap = true;
            helpBoxTextStyle.fontSize = 12; // Standard +2 points
            helpBoxTextStyle.normal.textColor = EditorStyles.label.normal.textColor;
            helpBoxTextStyle.padding = new RectOffset(5, 5, 2, 5);

            EditorGUILayout.LabelField(description, helpBoxTextStyle);

            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.Space(8);
        }

        private void DrawStripDemoGamesTab() {
            EditorGUILayout.LabelField("STRIP DEMO GAMES", EditorStyles.boldLabel);
            DrawCustomInfoBox("Remove demo content to reduce project size. This will delete the game's source folder and its Build Profile asset.", "console.warnicon");
            GUILayout.Space(10);

            // --- STRIP ALL BUTTON ---
            GUI.backgroundColor = new Color(1f, 0.8f, 0.8f);
            if (GUILayout.Button("STRIP ALL DETECTED DEMO GAMES", GUILayout.Height(35))) {
                if (EditorUtility.DisplayDialog("Strip All Demo Games", "Delete every detected demo game and build profile?", "Delete All", "Cancel")) {
                    PerformStripDemo(storyDemoGamePath, storyDemoBuildProfilePath);
                    PerformStripDemo(characterDemoGamePath, characterDemoBuildProfilePath);
                    PerformStripDemo(featuresDemoGamePath, featuresDemoBuildProfilePath);
                    PerformStripDemo(movementTestGamePath, movementTestGameBuildProfilePath);
                    PerformStripDemo(mmoDemoGamePath, mmoDemoBuildProfilePath);
                    PerformStripDemo(featuresDemoGameUMAExtensionsPath, ""); // No build profile for this one
                    PerformStripDemo(mmoDemoGameUMAExtensionsPath, ""); // No build profile for this one
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Cleanup Complete", "All detected demo games removed.", "OK");
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(15);

            bool anyDemosFound = false;
            anyDemosFound |= DrawStripDemoItem("A Lost Soul Story Demo", storyDemoGamePath, storyDemoBuildProfilePath);
            anyDemosFound |= DrawStripDemoItem("A Lost Soul Character Demo", characterDemoGamePath, characterDemoBuildProfilePath);
            anyDemosFound |= DrawStripDemoItem("Features Demo Game", featuresDemoGamePath, featuresDemoBuildProfilePath);
            anyDemosFound |= DrawStripDemoItem("Movement Test Game", movementTestGamePath, movementTestGameBuildProfilePath);
            anyDemosFound |= DrawStripDemoItem("AnyMMO FishNet Demo Game", mmoDemoGamePath, mmoDemoBuildProfilePath);
            anyDemosFound |= DrawStripDemoItem("Features Demo Game UMA Extensions", featuresDemoGameUMAExtensionsPath, ""); // No build profile for this one
            anyDemosFound |= DrawStripDemoItem("AnyMMO FishNet Demo Game UMA Extensions", mmoDemoGameUMAExtensionsPath, ""); // No build profile for this one

            if (!anyDemosFound) EditorGUILayout.HelpBox("No demo games detected.", MessageType.Info);
        }

        private bool DrawStripDemoItem(string title, string folderPath, string profilePath) {
            bool folderExists = Directory.Exists(Path.Combine(Application.dataPath, folderPath));
            bool profileExists = !string.IsNullOrEmpty(profilePath) && File.Exists(Path.Combine(Application.dataPath, profilePath));

            if (!folderExists && !profileExists) return false;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Resources detected.", EditorStyles.miniLabel);
            GUI.color = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Strip Assets", GUILayout.Width(100))) {
                if (EditorUtility.DisplayDialog("Strip", $"Delete assets for {title}?", "Delete", "Cancel")) {
                    PerformStripDemo(folderPath, profilePath);
                    AssetDatabase.Refresh();
                }
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return true;
        }

        private void PerformStripDemo(string folderPath, string profilePath) {
            // Ensure the paths start with "Assets/"
            string assetFolder = "Assets/" + folderPath;
            string assetProfile = "Assets/" + profilePath;

            // DeleteAsset handles the file/folder AND the .meta file automatically
            if (AssetDatabase.IsValidFolder(assetFolder)) {
                AssetDatabase.DeleteAsset(assetFolder);
            }

            if (!string.IsNullOrEmpty(profilePath) && System.IO.File.Exists(Path.Combine(Application.dataPath, profilePath))) {
                AssetDatabase.DeleteAsset(assetProfile);
            }

            // Force Unity to acknowledge the files are gone immediately
            AssetDatabase.Refresh();
        }

        private void DrawStripTemplateResourcesTab() {
            EditorGUILayout.LabelField("STRIP TEMPLATE RESOURCES", EditorStyles.boldLabel);
            DrawCustomInfoBox("Remove template assets (Packages, Prefabs, Resources).", "console.warnicon");
            GUILayout.Space(10);

            GUI.backgroundColor = new Color(1f, 0.8f, 0.8f);
            if (GUILayout.Button("STRIP ALL DETECTED TEMPLATE CONTENT", GUILayout.Height(35))) {
                if (EditorUtility.DisplayDialog("Strip All", "Remove all template subfolders?", "Delete All", "Cancel")) {
                    PerformStripTemplate(coreTemplateContentFolder);
                    PerformStripTemplate(cc0FantasyContentPackContentFolder);
                    PerformStripTemplate(umaTemplateContentFolder);
                    PerformStripTemplate(fishNetTemplateContentFolder);
                    PerformStripTemplate(fishNetUMATemplateContentFolder);
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Cleanup Complete", "All template content removed.", "OK");
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(15);

            bool anyFound = false;
            anyFound |= DrawStripTemplateRow("Core Template Content", coreTemplateContentFolder);
            anyFound |= DrawStripTemplateRow("CC0 Fantasy Content Pack Template Content", cc0FantasyContentPackContentFolder);
            anyFound |= DrawStripTemplateRow("UMA Template Content", umaTemplateContentFolder);
            anyFound |= DrawStripTemplateRow("FishNet Template Content", fishNetTemplateContentFolder);
            anyFound |= DrawStripTemplateRow("FishNet UMA Template Content", fishNetUMATemplateContentFolder);

            if (!anyFound) EditorGUILayout.HelpBox("No template resources detected.", MessageType.Info);
        }

        private bool DrawStripTemplateRow(string title, string parentPath) {
            string[] subs = { "TemplatePackages", "TemplatePrefabs", "TemplateResources" };
            bool exists = false;
            foreach (var s in subs) if (Directory.Exists(Path.Combine(Application.dataPath, parentPath, s))) exists = true;

            if (!exists) return false;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Subfolders detected.", EditorStyles.miniLabel);
            GUI.color = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Strip Templates", GUILayout.Width(110))) {
                if (EditorUtility.DisplayDialog("Strip", $"Delete template subfolders in {parentPath}?", "Delete", "Cancel")) {
                    PerformStripTemplate(parentPath);
                    AssetDatabase.Refresh();
                }
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return true;
        }

        private void PerformStripTemplate(string parentPath) {
            string[] subFolders = { "TemplatePackages", "TemplatePrefabs", "TemplateResources" };
            foreach (var subFolder in subFolders) {
                string relPath = Path.Combine("Assets", parentPath, subFolder);

                // DeleteAsset removes both the folder and the .meta file
                if (AssetDatabase.IsValidFolder(relPath)) {
                    AssetDatabase.DeleteAsset(relPath);
                }
            }
            AssetDatabase.Refresh();
        }

    }

}