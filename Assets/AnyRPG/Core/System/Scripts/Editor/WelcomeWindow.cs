using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace AnyRPG {

    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow {

        public const string installedVersion = "1.0";

        private const string storyDemoGameScenePath = "/ALostSoul/Games/ALostSoulStoryDemo/Scenes/Game/ALostSoulStoryDemoGame/ALostSoulStoryDemoGame.unity";
        private const string characterDemoGameScenePath = "/ALostSoul/Games/ALostSoulCharacterDemo/Scenes/Game/ALostSoulCharacterDemoGame/ALostSoulCharacterDemoGame.unity";
        private const string contentDemoGameScenePath = "/AnyRPG/Engine/Games/ContentDemo/Scenes/Game/ContentDemoGame/ContentDemoGame.unity";
        private const string featuresDemoGameScenePath = "/AnyRPG/Core/Games/FeaturesDemoGame/Scenes/Game/FeaturesDemoGame/FeaturesDemoGame.unity";
        private const string zeroConfigModeGameScenePath = "/AnyRPG/Core/Games/EmptyGame/Scenes/EmptyGame/EmptyGame.unity";
        private const string umaDemoGameScenePath = "/AnyRPG/Addons/anyrpg-uma/Games/UMADemoGame/Scenes/Game/UMADemoGame/UMADemoGame.unity";
        private const string mmoDemoGameScenePath = "/AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemo/AnyMMODemo.unity";

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
            get { return PlayerPrefs.GetInt("DisplayWelcomeWindow") == 1 ? true : false; }
            set {
                if (value != (PlayerPrefs.GetInt("DisplayWelcomeWindow") == 1 ? true : false)) {
                    PlayerPrefs.SetInt("DisplayWelcomeWindow", value == true ? 1 : 0);
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

            GUILayout.Label("MENU", EditorStyles.boldLabel);
            GUILayout.Space(5);

            string[] tabs = { "Start Here", "Install Optional Addons", "Create Your Game", "Included Demo Games", "Support" };

            // SelectionGrid creates large, clickable buttons for your tabs
            selectedTab = GUILayout.SelectionGrid(selectedTab, tabs, 1, GUILayout.Height(tabs.Length * 45));

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
            if (GUILayout.Button("Install Optional Addons", GUILayout.Height(35))) {
                selectedTab = 1; // Index for "Install Optional Addons"
            }
            GUILayout.EndVertical();

            GUILayout.Space(15);

            // 3. CREATE YOUR GAME
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("3. Create Your Game", EditorStyles.boldLabel);
            DrawCustomInfoBox("Use the New Game Wizard to automate the creation of your Game Manager and scene structure.", "console.infoicon");
            if (GUILayout.Button("Go to New Game Wizard", GUILayout.Height(35))) {
                selectedTab = 2; // Index for "Create Your Game"
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

            // Updated 12pt Infographic Header
            DrawCustomInfoBox("These addons require their base Unity packages to be installed first.", "console.infoicon");

            GUILayout.Space(10);

            // UMA PANEL
            /*
            DrawTwoStepAddonPanel(
                "UMA 2 Integration",
                "Adds advanced character customization and runtime mesh combining.",
                "Assets/UMA",
                "https://assetstore.unity.com/packages/package/35611",
                "anyrpg-uma",
                "https://github.com/AnyRPG/anyrpg-uma",
                "UMA 2",
                "AnyRPG UMA Addon",
                "UMA 2"
            );
            */
            
            // UMA 2 PANEL (With Step 3)
            DrawTwoStepAddonPanel(
                "UMA 2 Integration",
                "Adds advanced character customization and runtime mesh combining.",
                "Assets/UMA",
                "https://assetstore.unity.com/packages/package/35611",
                "anyrpg-uma",
                "https://github.com/AnyRPG/anyrpg-uma",
                "UMA 2",
                "AnyRPG UMA Addon",
                "UMA 2",
                DrawUMAPostInstall // Passing the Step 3 method here
            );

            GUILayout.Space(15);

            // FISHNET PANEL
            DrawTwoStepAddonPanel(
                "FishNet (MMO) Networking",
                "Enables multiplayer capabilities using the FishNet library.",
                "Assets/FishNet",
                "https://assetstore.unity.com/packages/package/207815",
                "anymmo-fishnet",
                "https://github.com/AnyRPG/anymmo-fishnet",
                "FishNet",
                "AnyMMO FishNet Addon",
                "FishNet: Networking Evolved"
            );
        }

        private void DrawAddonDescription(string desc) {
            GUIStyle bodyStyle = new GUIStyle(EditorStyles.label) { fontSize = 12, wordWrap = true };
            EditorGUILayout.LabelField(desc, bodyStyle);
            GUILayout.Space(10);
        }

        private void DrawTerminalCommand(string command) {
            GUILayout.Space(10);
            GUIStyle term = new GUIStyle(EditorStyles.textArea) {
                wordWrap = true,
                normal = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.05f)) }
            };
            term.font = Font.CreateDynamicFontFromOSFont(new string[] { "Courier New", "monospace" }, 12);
            EditorGUILayout.SelectableLabel(command, term, GUILayout.Height(60));
        }

        /*
        private void DrawTwoStepAddonPanel(string title, string desc, string baseFolder, string storeUrl, string addonFolder, string gitUrl, string packageShortName, string addonString, string packageFullName) {
            GUILayout.BeginVertical(title, "window");
            GUILayout.Space(16);

            GUIStyle bodyStyle = new GUIStyle(EditorStyles.label) { fontSize = 12, wordWrap = true };
            EditorGUILayout.LabelField(desc, bodyStyle);
            GUILayout.Space(10);

            // --- STEP 1: BASE PACKAGE ---
            bool hasBase = Directory.Exists(Path.Combine(Application.dataPath, "..", baseFolder));
            DrawStatusStep($"1. {packageShortName} Unity Package", hasBase, "Installed", "Open Package Manager", () => UnityEditor.PackageManager.UI.Window.Open(packageFullName), storeUrl);

            GUILayout.Space(5);

            // --- STEP 2: ANYRPG ADDON ---
            string relPath = Path.Combine("Assets", "AnyRPG", "Addons", addonFolder);
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relPath));
            bool hasAddon = Directory.Exists(fullPath);

            DrawStatusStep($"2. {addonString}", hasAddon, "Installed", "Clone (Requires Git)", () => InstallAddon(addonFolder, gitUrl), gitUrl, hasBase);

            // Manual Command Block
            if (!hasAddon) {
                GUILayout.Space(10);
                GUIStyle term = new GUIStyle(EditorStyles.textArea) {
                    wordWrap = true,
                    normal = { textColor = Color.white, background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.05f)) }
                };
                term.font = Font.CreateDynamicFontFromOSFont(new string[] { "Courier New", "monospace" }, 12);
                EditorGUILayout.SelectableLabel($"git clone {gitUrl} \"{fullPath}\"", term, GUILayout.Height(60));
            }
            GUILayout.EndVertical();
        }
        */

        private void DrawTwoStepAddonPanel(string title, string desc, string baseFolder, string storeUrl, string addonFolder, string gitUrl, string packageString, string addonString, string pmSearchTerm, Action extraContent = null) {
            GUILayout.BeginVertical(title, "window");
            GUILayout.Space(16);

            EditorGUILayout.LabelField(desc, new GUIStyle(EditorStyles.label) { fontSize = 12, wordWrap = true });
            GUILayout.Space(10);

            // --- STEP 1: BASE PACKAGE ---
            bool hasBase = Directory.Exists(Path.Combine(Application.dataPath, "..", baseFolder));
            DrawStatusStep($"1. {packageString} Unity Package", hasBase, "Installed", "Install Package", () => UnityEditor.PackageManager.UI.Window.Open(pmSearchTerm), storeUrl);

            GUILayout.Space(5);

            // --- STEP 2: ANYRPG ADDON ---
            string relPath = Path.Combine("Assets", "AnyRPG", "Addons", addonFolder);
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relPath));
            bool hasAddon = Directory.Exists(fullPath);
            DrawStatusStep($"2. {addonString}", hasAddon, "Installed", "Clone Addon (Requires Git)", () => InstallAddon(addonFolder, gitUrl), gitUrl, hasBase);

            if (!hasAddon) {
                DrawTerminalCommand($"git clone {gitUrl} \"{fullPath}\"");
            }

            // --- STEP 3: OPTIONAL EXTRA CONTENT ---
            if (hasBase && hasAddon && extraContent != null) {
                GUILayout.Space(5);
                extraContent.Invoke();
            }

            GUILayout.EndVertical();
        }

        private void DrawUMAPostInstall() {
            // Parent container for the entire Step 3 section
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // The main section label is now INSIDE the helpBox
            EditorGUILayout.LabelField("3. Post Install Configuration", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Step A Sub-Box
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Step A: Remove Library Filters", EditorStyles.boldLabel);
            DrawCustomInfoBox("For AnyRPG to see all UMA assets, you must remove all existing Global Library Filters.", "console.infoicon");
            if (GUILayout.Button("Open Global Library Filters", GUILayout.Height(25))) {
                EditorApplication.ExecuteMenuItem("UMA/Global Library Filters");
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);

            // Step B Sub-Box
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Step B: Rebuild Global Library", EditorStyles.boldLabel);
            DrawCustomInfoBox("After removing filters, rebuild the library via the UMA Welcome Window.", "console.infoicon");
            if (GUILayout.Button("Open UMA Welcome Window", GUILayout.Height(25))) {
                EditorApplication.ExecuteMenuItem("UMA/Welcome to UMA");
            }
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }


        private void DrawStatusStep(string label, bool installed, string okText, string btnText, Action onClick, string url, bool enabled = true) {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            // Row 1: Title
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            // Row 2: Status Line
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Status:", GUILayout.Width(50));
            if (installed) {
                GUI.color = Color.green;
                GUILayout.Label("\u2714 " + okText);
                GUI.color = Color.white;
            } else {
                GUI.color = new Color(1f, 0.4f, 0.4f);
                GUILayout.Label("\u2718 Missing", GUILayout.Width(70));
                GUI.color = Color.white;

                GUILayout.FlexibleSpace();

                GUI.enabled = enabled;
                // Changing "Open Store" to "Install Package" logic
                if (GUILayout.Button(btnText, GUILayout.MinWidth(120), GUILayout.Height(22))) {
                    // This opens the Package Manager and filters for the package
                    onClick?.Invoke();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            // Row 3: Manual URL Link
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("URL:", GUILayout.Width(40));
            GUIStyle linkStyle = new GUIStyle(EditorStyles.label) {
                normal = { textColor = new Color(0.3f, 0.6f, 1f) },
                fontStyle = FontStyle.Italic,
                fontSize = 12
            };
            if (GUILayout.Button(url, linkStyle)) Application.OpenURL(url);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void InstallAddon(string folderName, string repoUrl) {
            string targetPath = Path.Combine(Application.dataPath, "AnyRPG", "Addons", folderName);

            // Ensure the Addons directory exists first
            string addonsRoot = Path.Combine(Application.dataPath, "AnyRPG", "Addons");
            if (!Directory.Exists(addonsRoot)) Directory.CreateDirectory(addonsRoot);

            EditorUtility.DisplayProgressBar("AnyRPG Addon Installer", $"Cloning {folderName} from GitHub...", 0.5f);
            try {
                RunGitCommand($"clone {repoUrl} \"{targetPath}\"");
            } finally {
                // Always clear the progress bar, even if it fails
                EditorUtility.ClearProgressBar();
            }
        }

        private static void RunGitCommand(string args) {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = "git",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Application.dataPath // Run from Assets folder
            };

            try {
                using (Process process = Process.Start(startInfo)) {
                    if (process == null) throw new Exception("Could not start Git process.");

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0) {
                        UnityEngine.Debug.Log($"[AnyRPG] Git Success: {output}");
                        // Force Unity to see the new files immediately
                        AssetDatabase.Refresh();
                    } else {
                        EditorUtility.DisplayDialog("Git Error", $"Clone failed: {error}", "OK");
                        UnityEngine.Debug.LogError($"[AnyRPG] Git Error (Code {process.ExitCode}): {error}");
                    }
                }
            } catch (System.ComponentModel.Win32Exception) {
                UnityEngine.Debug.LogError("[AnyRPG] Git not found! Please ensure Git is installed and added to your system PATH.");
                EditorUtility.DisplayDialog("Git Not Found", "Git is required to download addons. Please install Git and restart Unity.", "OK");
            } catch (Exception ex) {
                EditorUtility.DisplayDialog("Git Error", $"Git not found or command failed: {ex.Message}", "OK");
                UnityEngine.Debug.LogError($"[AnyRPG] Unexpected Error: {ex.Message}");
            }
        }

        private void DrawDemosTab() {
            EditorGUILayout.LabelField("INCLUDED DEMO GAMES", EditorStyles.boldLabel);
            GUILayout.Space(10);

            DrawDemoButton(
                    "A Lost Soul Story Demo Game",
                    storyDemoGameScenePath,
                    "Assets/Settings/Build Profiles/A Lost Soul Story Demo.asset",
                    "The first 2 chapters of the game, 'A Lost Soul', re-created using the open source assets included in AnyRPG"
                );

            DrawDemoButton(
                   "A Lost Soul Character Demo Game",
                   characterDemoGameScenePath,
                   "Assets/Settings/Build Profiles/A Lost Soul Character Demo.asset",
                   "Explore the game world of A Lost Soul by starting as any character model and faction included in the game"
               );

            DrawDemoButton(
                "Content Demo Game",
                contentDemoGameScenePath,
                "Assets/Settings/Build Profiles/Content Demo Game.asset",
                "A simple demo of all 3d and audio content including\n -Clothing\n -Characters\n -Buildings\n -Props\n -Weapons"
                );

            DrawDemoButton(
                "Features Demo Game",
                featuresDemoGameScenePath,
                "Assets/Settings/Build Profiles/Features Demo Game.asset",
                "A simple 2 level game that provides examples of the most common features and interactables included in AnyRPG for quick reference when implementing them in your own game"
                );

            DrawDemoButton(
                "Empty (Zero Config Mode) Game",
                zeroConfigModeGameScenePath,
                "Assets/Settings/Build Profiles/Empty Game.asset",
                "A bare bones single scene with no main menu that demonstrates how to use AnyRPG in Zero Config (Controller Only) mode by including an unconfigured GameManager into any scene"
                );

            DrawDemoButton(
                "UMA Demo Game",
                umaDemoGameScenePath,
                "Assets/Settings/Build Profiles/UMA Demo Game.asset",
                "A copy of the Features Demo Game using UMA characters."
                );

            DrawDemoButton(
                "AnyMMO FishNet Demo Game",
                mmoDemoGameScenePath,
                "Assets/Settings/Build Profiles/AnyMMO Demo Game.asset",
                "A copy of the Features Demo Game designed for play over the network with multiple players."
                );

        }

        private void DrawDemoButton(string title, string scenePath, string profilePath, string description) {
            if (!System.IO.File.Exists(Application.dataPath + scenePath)) return;

            GUILayout.BeginVertical("box");
            if (GUILayout.Button(title, GUILayout.Height(30))) {
                var buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(profilePath);
                if (buildProfile != null) {
                    BuildProfile.SetActiveBuildProfile(buildProfile);
                }
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets" + scenePath);
            }

            // This reproduces the "HelpBox" background and border style exactly
            GUILayout.BeginVertical("helpBox");

            GUIStyle helpBoxTextStyle = new GUIStyle(EditorStyles.label);
            helpBoxTextStyle.wordWrap = true;
            helpBoxTextStyle.fontSize = 12; // Your requested +2 points
                                            // Keep standard label colors so it looks native
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
            if (GUILayout.Button("Launch New Game Wizard", GUILayout.Height(40))) {
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






    }
}