using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace AnyRPG {

    [InitializeOnLoad]
    public class AddonManager : EditorWindow {

        public const string installedVersion = "1.0";

        public static Texture2D welcomeBanner = null;

        public static Vector2 scrollPosition;

        //GUISkin skin;
        private const int windowMinWidth = 800;
        private const int windowMinHeight = 600;
        private const int windowInitialWidth = 900;
        private const int windowInitialHeight = 700;

        // --- State Variables ---
        private List<AddonData> addonsList = new List<AddonData>();
        private string[] addonMenuNames;
        private int selectedAddonIndex = 0;
        private Vector2 sidebarScrollPos;
        private Vector2 contentScrollPos;
        private const float SidebarWidth = 250f;

        [MenuItem("Tools/AnyRPG/Addon Manager", false, 0)]
        public static void Open() {
            // Changing 'true' to 'false' allows the window to be docked 
            // and usually survives recompiles with its size intact much better.
            AddonManager window = GetWindow<AddonManager>(false, "Addon Manager", true);
            window.position = new Rect(100, 100, windowInitialWidth, windowInitialHeight); // Set a generous initial size
            window.Show();
        }

        public void OnEnable() {
            titleContent = new GUIContent("AnyRPG Addon Manager");

            // Set a minimum size to prevent the UI from breaking
            minSize = new Vector2(windowMinWidth, windowMinHeight);

            // DO NOT set maxSize equal to minSize if you want to drag the corner.
            // If you want it to stay wide after recompile, let maxSize be large.
            maxSize = new Vector2(4000, 2000);

            InitStyle();
            InitializeAddonsData();
        }


        void InitStyle() {
            welcomeBanner = (Texture2D)Resources.Load("AnyRPGBanner", typeof(Texture2D));
        }

        private void InitializeAddonsData() {
            var fishNetPackage = new UnityPackageReq { Name = "FishNet", Folder = "Assets/FishNet", SearchTerm = "FishNet: Networking Evolved", StoreUrl = "https://assetstore.unity.com/packages/package/207815" };
            var umaPackage = new UnityPackageReq { Name = "UMA 2", Folder = "Assets/UMA", SearchTerm = "UMA 2", StoreUrl = "https://assetstore.unity.com/packages/package/35611" };

            // Match the Folder string explicitly to the target addon's Folder property for tab linking
            var umaDependency = new AddonReq { Name = "AnyRPG UMA Addon", Folder = "anyrpg-uma" };
            var fishNetDependency = new AddonReq { Name = "AnyMMO FishNet Addon", Folder = "anymmo-fishnet" };
            var cc0Dependency = new AddonReq { Name = "CC0 Fantasy Content Pack", Folder = "anyrpg-cc0-fantasy-content-pack" };

            addonsList = new List<AddonData>
            {
            new AddonData {
                Name = "UMA 2 Integration",
                Description = "Adds advanced runtime character customization.",
                Folder = "anyrpg-uma",
                GitUrl = "https://github.com/AnyRPG/anyrpg-uma",
                PackageName = "AnyRPG UMA Addon",
                UnityPackages = new List<UnityPackageReq> { umaPackage },
                PostInstallAction = DrawUMAPostInstall
            },
            new AddonData {
                Name = "AnyMMO (FishNet)",
                Description = "Enables online play using the FishNet library.",
                Folder = "anymmo-fishnet",
                GitUrl = "https://github.com/AnyRPG/anymmo-fishnet",
                PackageName = "AnyMMO FishNet Addon",
                UnityPackages = new List<UnityPackageReq> { fishNetPackage }
            },
            new AddonData {
                Name = "FishNet-UMA Integration",
                Description = "Adds template content with UMA characters configured for use with FishNet.",
                Folder = "anymmo-fishnet-uma",
                GitUrl = "https://github.com/AnyRPG/anymmo-fishnet-uma",
                PackageName = "AnyMMO FishNet UMA Addon",
                UnityPackages = new List<UnityPackageReq> { fishNetPackage, umaPackage },
                AddonDependencies = new List<AddonReq> { umaDependency, fishNetDependency }
            },
            new AddonData {
                Name = "Movement Test Game",
                Description = "A simple one scene game with stairs, ramps, obstacles, and water suitable for testing character controller modifications.",
                Folder = "anyrpg-movement-test-game",
                GitUrl = "https://github.com/AnyRPG/anyrpg-movement-test-game",
                PackageName = "AnyRPG Movement Test Game"
            },
            new AddonData {
                Name = "CC0 Fantasy Content Pack",
                Description = "A collection of fantasy assets released under the CC0 license, suitable for use in any project.",
                Folder = "anyrpg-cc0-fantasy-content-pack",
                WebUrl = "https://www.anyrpg.org/download/anyrpg-cc0-fantasy-content-pack/",
                PackageName = "AnyRPG CC0 Fantasy Content Pack"
            },
            new AddonData {
                Name = "A Lost Soul Demo Games",
                Description = "Demos of the game, A Lost Soul, created using the anyrpg-cc0-fantasy-content-pack assets.",
                Folder = "a-lost-soul-demo-games",
                GitUrl = "https://github.com/AnyRPG/a-lost-soul-demo-games",
                PackageName = "A Lost Soul Demo Games",
                UnityPackages = new List<UnityPackageReq> { umaPackage },
                AddonDependencies = new List<AddonReq> { umaDependency, cc0Dependency }
            }
        };

            addonMenuNames = new string[addonsList.Count];
            for (int i = 0; i < addonsList.Count; i++) {
                addonMenuNames[i] = addonsList[i].Name;
            }
        }

        private void NavigateToAddon(string targetFolder) {
            for (int i = 0; i < addonsList.Count; i++) {
                if (addonsList[i].Folder == targetFolder) {
                    selectedAddonIndex = i;
                    GUI.FocusControl(null);
                    Repaint();
                    return;
                }
            }
            UnityEngine.Debug.LogWarning($"[Addons Manager] Could not find an addon tab matching folder destination: {targetFolder}");
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
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(SidebarWidth), GUILayout.ExpandHeight(true));
            GUILayout.Label("AVAILABLE ADDONS", EditorStyles.boldLabel);
            GUILayout.Space(5);

            sidebarScrollPos = GUILayout.BeginScrollView(sidebarScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            int newSelection = GUILayout.SelectionGrid(selectedAddonIndex, addonMenuNames, 1, GUILayout.Height(addonMenuNames.Length * 42));
            if (newSelection != selectedAddonIndex) {
                selectedAddonIndex = newSelection;
                GUI.FocusControl(null);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawContentArea() {
            contentScrollPos = GUILayout.BeginScrollView(contentScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            // GUIStyle.none removes the old gray window/box packaging outline
            GUILayout.BeginVertical(GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();

            if (addonsList == null || addonsList.Count == 0 || selectedAddonIndex >= addonsList.Count) {
                EditorGUILayout.LabelField("No addons found.");
                EndContentLayout();
                return;
            }

            AddonData activeAddon = addonsList[selectedAddonIndex];

            // Execute layout using your sequential multi-step rules
            DrawModularAddonPanel(
                activeAddon.Name,
                activeAddon.Description,
                activeAddon.Folder,
                activeAddon.GitUrl,
                activeAddon.WebUrl,
                activeAddon.PackageName,
                activeAddon.UnityPackages,
                activeAddon.AddonDependencies,
                activeAddon.PostInstallAction
            );

            EndContentLayout();
        }

        private void EndContentLayout() {
            GUILayout.EndVertical();
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        // --- REFACTORED MODULAR PANEL (Respecting your styles & execution requirements) ---
        private void DrawModularAddonPanel(string title, string desc, string addonFolder, string gitUrl, string webUrl, string addonLabel, List<UnityPackageReq> unityReqs = null, List<AddonReq> addonReqs = null, Action extraContent = null) {
            GUILayout.BeginVertical(); // Removed "window" style background string wrapper completely

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 16
            };

            // Draw your label using the new style
            EditorGUILayout.LabelField(title, titleStyle);

            GUILayout.Space(10);

            // Custom Requirement: Description is beautifully relocated into the top-level Info box style
            DrawCustomInfoBox(desc, "console.infoicon");
            GUILayout.Space(15);

            bool allReqsMet = true;

            // --- UNITY PACKAGE DEPENDENCIES ---
            if (unityReqs != null && unityReqs.Count > 0) {
                EditorGUILayout.LabelField("REQUIRED UNITY PACKAGES", EditorStyles.boldLabel);
                GUILayout.Space(5);

                foreach (var req in unityReqs) {
                    bool installed = Directory.Exists(Path.Combine(Application.dataPath, "..", req.Folder));
                    if (!installed) allReqsMet = false;

                    DrawStatusStep($"{req.Name} Unity Package", installed, "Installed",
                        installed ? "Open Package Manager" : "Install Package",
                        () => UnityEditor.PackageManager.UI.Window.Open(req.SearchTerm), req.StoreUrl);
                }
                GUILayout.Space(15);
            }

            // --- ANYRPG ADDON DEPENDENCIES ---
            if (addonReqs != null && addonReqs.Count > 0) {
                EditorGUILayout.LabelField("REQUIRED ANYRPG ADDONS", EditorStyles.boldLabel);
                GUILayout.Space(5);

                foreach (var req in addonReqs) {
                    string relPath = Path.Combine("Assets", "AnyRPG", "Addons", req.Folder);
                    bool installed = Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", relPath)));
                    if (!installed) allReqsMet = false;

                    // Custom Requirement: Removed full terminal boxes. Added clean routing button logic
                    DrawStatusStep($"{req.Name}", installed, "Installed", "Manage Addon",
                        () => NavigateToAddon(req.Folder),
                        string.Empty);
                }
                GUILayout.Space(15);
            }

            // --- THE PRIMARY ADDON ---
            EditorGUILayout.LabelField("INSTALLATION & MANAGEMENT", EditorStyles.boldLabel);
            GUILayout.Space(5);

            string mainRelPath = Path.Combine("Assets", "AnyRPG", "Addons", addonFolder);
            bool mainInstalled = Directory.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", mainRelPath)));

            DrawFullAddonStep($"{addonLabel}", addonFolder, gitUrl, webUrl, mainInstalled, allReqsMet);

            // --- STEP 3+ (Extra Content) ---
            if (mainInstalled && allReqsMet && extraContent != null) {
                GUILayout.Space(15);
                EditorGUILayout.LabelField("POST-INSTALLATION CONFIGURATION", EditorStyles.boldLabel);
                GUILayout.Space(5);
                extraContent.Invoke();
            }

            GUILayout.EndVertical();
        }

        // --- PRESERVED CODEBASES (Untouched logic blocks) ---
        private void DrawFullAddonStep(string stepLabel, string folder, string gitUrl, string webUrl, bool isInstalled, bool requirementsMet) {
            string relPath = Path.Combine("Assets", "AnyRPG", "Addons", folder);
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relPath));

            //GUILayout.BeginVertical(EditorStyles.helpBox);
            DrawStatusStep(stepLabel, isInstalled, "Installed", "", null, gitUrl != string.Empty ? gitUrl : webUrl, requirementsMet);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Manage", EditorStyles.miniBoldLabel);

            if (!isInstalled) {
                if (gitUrl != "") {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUI.enabled = requirementsMet;
                    if (GUILayout.Button($"Install {stepLabel} (Requires Git)", GUILayout.Height(25))) {
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
                if (gitUrl != "") {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    if (GUILayout.Button("Update Addon (Requires Git)", GUILayout.Height(25))) {
                        UpdateAddon(fullPath, folder);
                    }
                    DrawTerminalCommand($"cd \"{fullPath}\" && git pull");
                    GUILayout.EndVertical();

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

            //GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void DrawStatusStep(string label, bool installed, string okText, string btnText, Action onClick, string url, bool enabled = true) {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

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
                    onClick?.Invoke();
                }
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            if (url != "") {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("URL:", GUILayout.Width(40));
                GUIStyle linkStyle = new GUIStyle(EditorStyles.label) {
                    normal = { textColor = new Color(0.3f, 0.6f, 1f) },
                    fontStyle = FontStyle.Italic,
                    fontSize = 12
                };
                if (GUILayout.Button(url, linkStyle)) Application.OpenURL(url);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
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
            //GUILayout.BeginVertical(EditorStyles.helpBox);

            // The main section label is now INSIDE the helpBox
            //EditorGUILayout.LabelField("Post Install Configuration", EditorStyles.boldLabel);
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

            //GUILayout.EndVertical();
        }

        /*
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
        */

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

        private void DrawBottom() {
            GUILayout.BeginHorizontal("box");

            GUILayout.FlexibleSpace();
            GUILayout.Label($"AnyRPG Version {installedVersion}", EditorStyles.miniLabel);

            GUILayout.EndHorizontal();
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

    }

    public class UnityPackageReq {
        public string Name;
        public string Folder;
        public string SearchTerm;
        public string StoreUrl;
    }

    public class AddonReq {
        public string Name = string.Empty;
        public string Folder = string.Empty;
    }

    public class AddonData {
        public string Name = string.Empty;
        public string Description = string.Empty;
        public string Folder = string.Empty;
        public string GitUrl = string.Empty;
        public string WebUrl = string.Empty;
        public string PackageName = string.Empty;
        public List<UnityPackageReq> UnityPackages = new List<UnityPackageReq>();
        public List<AddonReq> AddonDependencies = new List<AddonReq>();
        public Action PostInstallAction = null;
    }

}