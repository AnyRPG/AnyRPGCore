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

        private const string storyDemoGameScenePath = "ALostSoul/Games/ALostSoulStoryDemo/Scenes/Game/ALostSoulStoryDemoGame/ALostSoulStoryDemoGame.unity";
        private const string characterDemoGameScenePath = "ALostSoul/Games/ALostSoulCharacterDemo/Scenes/Game/ALostSoulCharacterDemoGame/ALostSoulCharacterDemoGame.unity";
        private const string featuresDemoGameScenePath = "AnyRPG/Core/Games/FeaturesDemoGame/Scenes/Game/FeaturesDemoGame/FeaturesDemoGame.unity";
        private const string movementTestGameScenePath = "AnyRPG/Addons/anyrpg-movement-test-game/Scenes/Game/MovementTestGame/MovementTestGame.unity";
        private const string mmoDemoGameScenePath = "AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo/Scenes/AnyMMODemo/AnyMMODemo.unity";

        private const string storyDemoGamePath = "ALostSoul/Games/ALostSoulStoryDemo";
        private const string characterDemoGamePath = "ALostSoul/Games/ALostSoulCharacterDemo";
        private const string featuresDemoGamePath = "AnyRPG/Core/Games/FeaturesDemoGame";
        private const string movementTestGamePath = "AnyRPG/Addons/anyrpg-movement-test-game/Games/MovementTestGame";
        private const string mmoDemoGamePath = "AnyRPG/Addons/anymmo-fishnet/Games/AnyMMODemo";

        private const string storyDemoBuildProfilePath = "ALostSoul/Build Profiles/A Lost Soul Story Demo.asset";
        private const string characterDemoBuildProfilePath = "ALostSoul/Build Profiles/A Lost Soul Character Demo.asset";
        private const string featuresDemoBuildProfilePath = "AnyRPG/Core/Build Profiles/Features Demo Game.asset";
        private const string movementTestGameBuildProfilePath = "AnyRPG/Addons/anyrpg-movement-test-game/Build Profiles/Movement Test Game.asset";
        private const string mmoDemoBuildProfilePath = "AnyRPG/Addons/anymmo-fishnet/Build Profiles/AnyMMO Demo Game.asset";

        private const string coreTemplateContentFolder = "AnyRPG/Core/Content";
        private const string engineTemplateContentFolder = "AnyRPG/Engine/Content";
        private const string umaTemplateContentFolder = "AnyRPG/Addons/anyrpg-uma/Content";
        private const string fishNetTemplateContentFolder = "AnyRPG/Addons/anymmo-fishnet/Content";

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

            string[] mainTabs = { "Start Here", "Install Optional Addons", "Create Your Game", "Included Demo Games", "Support" };

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

            UnityPackageReq fishNetPackage = new UnityPackageReq {
                Name = "FishNet",
                Folder = "Assets/FishNet",
                SearchTerm = "FishNet: Networking Evolved",
                StoreUrl = "https://assetstore.unity.com/packages/package/207815"
            };

            UnityPackageReq umaPackage = new UnityPackageReq {
                Name = "UMA 2",
                Folder = "Assets/UMA",
                SearchTerm = "UMA 2",
                StoreUrl = "https://assetstore.unity.com/packages/package/35611"
            };

            DrawModularAddonPanel(
                "UMA 2 Integration",
                "Adds advanced runtime character customization",
                "anyrpg-uma",
                "https://github.com/AnyRPG/anyrpg-uma",
                "",
                "AnyRPG UMA Addon",
                new List<UnityPackageReq> { umaPackage, },
                new List<AddonReq>(),
                DrawUMAPostInstall
            );

            GUILayout.Space(15);

            DrawModularAddonPanel(
                "FishNet (MMO) Networking",
                "Enables online play using the FishNet library",
                "anymmo-fishnet",
                "https://github.com/AnyRPG/anymmo-fishnet",
                "",
                "AnyMMO FishNet Addon",
                new List<UnityPackageReq> { fishNetPackage },
                new List<AddonReq>()
            );

            GUILayout.Space(15);

            DrawModularAddonPanel(
                "FishNet-UMA Integration",
                "Adds template content with UMA characters configured for use with FishNet",
                "anymmo-fishnet-uma",
                "https://github.com/AnyRPG/anymmo-fishnet-uma",
                "",
                "AnyMMO FishNet UMA Addon",
                new List<UnityPackageReq> {
                    fishNetPackage,
                    umaPackage
                },
                new List<AddonReq> {
                new AddonReq { Name = "AnyRPG UMA Addon", Folder = "anyrpg-uma", GitUrl = "https://github.com/AnyRPG/anyrpg-uma" },
                new AddonReq { Name = "AnyMMO FishNet Addon", Folder = "anymmo-fishnet", GitUrl = "https://github.com/AnyRPG/anymmo-fishnet" },
                }
            );

            GUILayout.Space(15);

            DrawModularAddonPanel(
                "Movement Test Game",
                "A simple one scene game with stairs, ramps, obstacles, and water suitable for testing character controller modifications",
                "anyrpg-movement-test-game",
                "https://github.com/AnyRPG/anyrpg-movement-test-game",
                "",
                "AnyRPG Movement Test Game",
                new List<UnityPackageReq>(),
                new List<AddonReq>()
            );

            GUILayout.Space(15);

            DrawModularAddonPanel(
                "CC0 Fantasy Content Pack",
                "A collection of fantasy assets released under the CC0 license, suitable for use in any project",
                "anyrpg-cc0-fantasy-content-pack",
                "",
                "https://www.anyrpg.org/download/anyrpg-cc0-fantasy-content-pack/",
                "AnyRPG CC0 Fantasy Content Pack",
                new List<UnityPackageReq>(),
                new List<AddonReq>()
            );

            GUILayout.Space(15);

            DrawModularAddonPanel(
                "A Lost Soul Demo Games",
                "Demos of the game, A Lost Soul, created using the anyrpg-cc0-fantasy-content-pack assets",
                "a-lost-soul-demo-games",
                "https://github.com/AnyRPG/a-lost-soul-demo-games",
                "",
                "A Lost Soul Demo Games",
                new List<UnityPackageReq> {
                    umaPackage
                },
                new List<AddonReq> {
                new AddonReq { Name = "AnyRPG UMA Addon", Folder = "anyrpg-uma", GitUrl = "https://github.com/AnyRPG/anyrpg-uma" },
                new AddonReq { Name = "CC0 Fantasy Content Pack", Folder = "anyrpg-cc0-fantasy-content-pack", GitUrl = "", WebUrl = "https://www.anyrpg.org/download/anyrpg-cc0-fantasy-content-pack/" },
                }
            );


        }

        private void DrawModularAddonPanel(string title, string desc, string addonFolder, string gitUrl, string webUrl, string addonLabel, List<UnityPackageReq> unityReqs = null, List<AddonReq> addonReqs = null, Action extraContent = null) {
            GUILayout.BeginVertical(title, "window");
            GUILayout.Space(16);
            EditorGUILayout.LabelField(desc, new GUIStyle(EditorStyles.label) { fontSize = 12, wordWrap = true });
            GUILayout.Space(10);

            bool allReqsMet = true;
            int stepCounter = 1; // Internal counter for labeling steps

            // --- UNITY PACKAGE DEPENDENCIES ---
            if (unityReqs != null) {
                foreach (var req in unityReqs) {
                    bool installed = Directory.Exists(Path.Combine(Application.dataPath, "..", req.Folder));
                    if (!installed) allReqsMet = false;

                    DrawStatusStep($"{stepCounter++}. {req.Name} Unity Package", installed, "Installed",
                        installed ? "Open Package Manager" : "Install Package",
                        () => UnityEditor.PackageManager.UI.Window.Open(req.SearchTerm), req.StoreUrl);
                }
            }

            // --- ANYRPG ADDON DEPENDENCIES ---
            if (addonReqs != null) {
                foreach (var req in addonReqs) {
                    string relPath = Path.Combine("Assets", "AnyRPG", "Addons", req.Folder);
                    bool installed = Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", relPath)));
                    if (!installed) allReqsMet = false;

                    // Draws the dependency with full Manage options
                    DrawFullAddonStep($"{stepCounter++}. {req.Name}", req.Folder, req.GitUrl, req.WebUrl, installed, true);
                }
            }

            // --- THE PRIMARY ADDON ---
            string mainRelPath = Path.Combine("Assets", "AnyRPG", "Addons", addonFolder);
            bool mainInstalled = Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", mainRelPath)));

            DrawFullAddonStep($"{stepCounter++}. {addonLabel}", addonFolder, gitUrl, webUrl, mainInstalled, allReqsMet);

            // --- STEP 3+ (Extra Content) ---
            if (mainInstalled && allReqsMet && extraContent != null) {
                extraContent.Invoke();
            }

            GUILayout.EndVertical();
        }

        private void DrawFullAddonStep(string stepLabel, string folder, string gitUrl, string webUrl, bool isInstalled, bool requirementsMet) {
            string relPath = Path.Combine("Assets", "AnyRPG", "Addons", folder);
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relPath));

            GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawStatusStep(stepLabel, isInstalled, "Installed", "", null, gitUrl != string.Empty ? gitUrl : webUrl, requirementsMet);

            // --- MANAGE SECTION (Thin Outline Box) ---
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Manage", EditorStyles.miniBoldLabel);

            if (!isInstalled) {
                if (gitUrl != "") {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUI.enabled = requirementsMet;
                    // Button text remains original, unaffected by the step number
                    if (GUILayout.Button($"Install {stepLabel.Substring(stepLabel.IndexOf('.') + 2)} (Requires Git)", GUILayout.Height(25))) {
                        InstallAddon(folder, gitUrl);
                    }
                    GUI.enabled = true;
                    DrawTerminalCommand($"git clone {gitUrl} \"{fullPath}\"");
                    GUILayout.EndVertical();
                }
                if (webUrl != "") {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    if (GUILayout.Button("Web Download", GUILayout.Height(25))) {
                        Application.OpenURL(webUrl);
                    }
                    GUILayout.EndVertical();
                }
            } else {
                // UPDATE
                if (gitUrl != "") {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    if (GUILayout.Button("Update Addon (Requires Git)", GUILayout.Height(25))) {
                        UpdateAddon(fullPath, folder);
                    }
                    DrawTerminalCommand($"cd \"{fullPath}\" && git pull");
                    GUILayout.EndVertical();

                    // CHECK UPDATES
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    if (GUILayout.Button("Check for Updates (Requires Git)", GUILayout.Height(25))) {
                        CheckForUpdates(fullPath, folder);
                    }
                    DrawTerminalCommand($"cd \"{fullPath}\" && git fetch && git status -uno");
                    GUILayout.EndVertical();
                }
                if (webUrl != "") {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    if (GUILayout.Button("Web Download", GUILayout.Height(25))) {
                        Application.OpenURL(webUrl);
                    }
                    GUILayout.EndVertical();
                }

                // REMOVE
                GUILayout.BeginVertical(EditorStyles.helpBox);
                if (GUILayout.Button("Remove Addon", GUILayout.Height(25))) {
                    if (EditorUtility.DisplayDialog($"Remove {folder}", $"Are you sure you want to delete the addon folder at {relPath}?", "Delete", "Cancel")) {
                        AssetDatabase.DeleteAsset(relPath);
                        AssetDatabase.Refresh();
                    }
                }
                EditorGUILayout.HelpBox("Removes the addon files from the project.", MessageType.None);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical(); // End Manage Box
            GUILayout.EndVertical(); // End Step Box
        }


        private void DrawTerminalCommand(string command) {
            GUILayout.Space(5);
            // Start with textArea to get the background/border box
            GUIStyle term = new GUIStyle(EditorStyles.textArea);

            // Set our custom colors and wrapping
            term.wordWrap = true;
            term.fontSize = 12;
            term.normal.textColor = Color.white;
            term.normal.background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.05f));
            term.padding = new RectOffset(8, 8, 8, 8);

            // In Unity 6, EditorStyles.textField.font is the most reliable 
            // way to get the internal monospaced font used for code/data input.
            if (EditorStyles.textField.font != null) {
                term.font = EditorStyles.textField.font;
            }

            // 2 lines of text (approx 45 pixels for 12pt font)
            EditorGUILayout.SelectableLabel(command, term, GUILayout.Height(45));
        }

        private void CheckForUpdates(string fullPath, string addonName) {
            EditorUtility.DisplayProgressBar("AnyRPG Addon Checker", $"Checking {addonName} for updates...", 0.5f);
            try {
                // Fetch updates and check the status of local vs remote
                RunGitCommand("fetch", fullPath);
                // We use 'status -uno' to see if we are behind without showing untracked files
                RunGitCommand("status -uno", fullPath);

                EditorUtility.DisplayDialog("Update Check", $"Check complete for {addonName}. Check the Console log to see if 'Your branch is behind'.", "OK");
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private void UpdateAddon(string fullPath, string addonName) {
            EditorUtility.DisplayProgressBar("AnyRPG Addon Updater", $"Updating {addonName}...", 0.5f);
            try {
                // We run "pull" and specify the addon folder as the working directory
                RunGitCommand("pull", fullPath);
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        // Updated to accept an optional working directory (defaults to Assets)
        private static void RunGitCommand(string args, string workingDir = null) {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                FileName = "git",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDir ?? Application.dataPath
            };

            try {
                using (Process process = Process.Start(startInfo)) {
                    if (process == null) throw new Exception("Could not start Git process.");

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0) {
                        UnityEngine.Debug.Log($"[AnyRPG] Git Success: {output}");
                        AssetDatabase.Refresh();
                    } else {
                        // If pull fails because of local changes, this will catch it
                        EditorUtility.DisplayDialog("Git Error", $"Command failed: {error}", "OK");
                        UnityEngine.Debug.LogError($"[AnyRPG] Git Error (Code {process.ExitCode}): {error}");
                    }
                }
            } catch (System.ComponentModel.Win32Exception) {
                EditorUtility.DisplayDialog("Git Not Found", "Git is required. Please install Git and restart Unity.", "OK");
            } catch (Exception ex) {
                EditorUtility.DisplayDialog("Git Error", $"Unexpected error: {ex.Message}", "OK");
            }
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
            DrawCustomInfoBox("For UMA to see all AnyRPG assets, you must remove all existing Global Library Filters.", "console.infoicon");
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
            }
                GUILayout.FlexibleSpace();

                GUI.enabled = enabled;
                if (btnText != "") {
                    if (GUILayout.Button(btnText, GUILayout.MinWidth(120), GUILayout.Height(22))) {
                        // This opens the Package Manager and filters for the package
                        onClick?.Invoke();
                    }
                }
                GUI.enabled = true;

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

            if (!anyDemosFound) EditorGUILayout.HelpBox("No demo games detected.", MessageType.Info);
        }

        private bool DrawStripDemoItem(string title, string folderPath, string profilePath) {
            bool folderExists = Directory.Exists(Path.Combine(Application.dataPath, folderPath));
            bool profileExists = File.Exists(Path.Combine(Application.dataPath, profilePath));

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

            if (System.IO.File.Exists(Path.Combine(Application.dataPath, profilePath))) {
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
                    PerformStripTemplate(engineTemplateContentFolder);
                    PerformStripTemplate(umaTemplateContentFolder);
                    PerformStripTemplate(fishNetTemplateContentFolder);
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Cleanup Complete", "All template content removed.", "OK");
                }
            }
            GUI.backgroundColor = Color.white;
            GUILayout.Space(15);

            bool anyFound = false;
            anyFound |= DrawStripTemplateRow("Core Template Content", coreTemplateContentFolder);
            anyFound |= DrawStripTemplateRow("Engine Template Content", engineTemplateContentFolder);
            anyFound |= DrawStripTemplateRow("UMA Template Content", umaTemplateContentFolder);
            anyFound |= DrawStripTemplateRow("FishNet Template Content", fishNetTemplateContentFolder);

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

    public class UnityPackageReq {
        public string Name;
        public string Folder;           // e.g., "Assets/UMA"
        public string SearchTerm;       // e.g., "UMA 2"
        public string StoreUrl;
    }

    public class AddonReq {
        public string Name = string.Empty;
        public string Folder = string.Empty;           // e.g., "anyrpg-uma"
        public string GitUrl = string.Empty;
        public string WebUrl = string.Empty;
    }

}