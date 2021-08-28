using System;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {
    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow {
        #region ToolBar Drawers
        /// <summary>
        /// ToolBar Class
        /// </summary>
        public class ToolBar {
            public string title;
            public UnityEngine.Events.UnityAction Draw;
            /// <summary>
            /// Create New Toolbar
            /// </summary>
            /// <param name="title">Title</param>
            /// <param name="onDraw">Method to draw when toolbar is selected</param>
            public ToolBar(string title, UnityEngine.Events.UnityAction onDraw) {
                this.title = title;
                this.Draw = onDraw;
            }
            public static implicit operator string(ToolBar tool) {
                return tool.title;
            }
        }

        /// <summary>
        /// Index of selected <seealso cref="toolBars"/>
        /// </summary>
        public int toolBarIndex = 0;

        /// <summary>
        /// List of Toolbars
        /// </summary>
        public ToolBar[] toolBars = new ToolBar[]
        {
            new ToolBar("First Run",FirstRunPageContent),
            new ToolBar("Getting Started",GettingStartedPageContent),
            new ToolBar("Support",Support)
        };
        #endregion

        public const string installedVersion = "0.11a";

        public const string _projectSettingsPath = "Assets/AnyRPG/Basic Locomotion/Resources/ProjectSettings.unitypackage";

        public static Texture2D welcomeBanner = null;

        public static Vector2 scrollPosition;

        //GUISkin skin;
        private const int windowWidth = 600;
        private const int windowHeight = 500;

        [MenuItem("Tools/AnyRPG/Welcome Window", false, windowWidth)]

        public static void Open() {
            GetWindow<WelcomeWindow>(true);
        }

        public void OnEnable() {
            titleContent = new GUIContent("Welcome To AnyRPG");
            maxSize = new Vector2(windowWidth, windowHeight);
            minSize = maxSize;
            InitStyle();
        }

        void InitStyle() {
            //if (!skin) skin = Resources.Load("welcomeWindowSkin") as GUISkin;

            welcomeBanner = (Texture2D)Resources.Load("AnyRPGBanner", typeof(Texture2D));
        }

        public void OnGUI() {
            //GUI.skin = skin;
            DrawHeader();
            DrawMenuButtons();
            DrawPageContent();
            DrawBottom();
        }

        private void DrawHeader() {
            //GUILayout.Label(welcomeBanner, GUILayout.Height(110));
            GUILayout.Label(welcomeBanner);
        }

        private void DrawMenuButtons() {
            GUILayout.Space(-10);
            toolBarIndex = GUILayout.Toolbar(toolBarIndex, ToolbarNames());
        }

        private string[] ToolbarNames() {
            string[] names = new string[toolBars.Length];
            for (int i = 0; i < toolBars.Length; i++) {
                names[i] = toolBars[i];
            }
            return names;
        }

        private void DrawPageContent() {
            GUILayout.BeginArea(new Rect(4, 140, 592, 340));
            toolBars[toolBarIndex].Draw();
            GUILayout.EndArea();
            GUILayout.FlexibleSpace();
        }

        private void DrawBottom() {
            GUILayout.BeginHorizontal("box");

            //EditorStartupPrefs.DisplayWelcomeScreen = GUILayout.Toggle(EditorStartupPrefs.DisplayWelcomeScreen, "Display this window at startup");
            bool tempBool = GUILayout.Toggle(true, "Display this window at startup");

            GUILayout.EndHorizontal();
        }

        private static void ImportPackage(string package) {
            try {
                AssetDatabase.ImportPackage(package, true);
            } catch (Exception) {
                Debug.LogError("Failed to import package: " + package);
                throw;
            }
        }

        #region Static ToolBars

        public static void FirstRunPageContent() {
            GUILayout.BeginVertical("window");

            GUILayout.Space(10);

            EditorGUILayout.HelpBox("AnyRPG Installed Version: " + installedVersion, MessageType.Info);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Official Website")) {
                Application.OpenURL("https://www.anyrpg.org");
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Wiki and Documentation")) {
                Application.OpenURL("https://wiki.anyrpg.org");
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Youtube Channel and Tutorials")) {
                Application.OpenURL("https://www.youtube.com/channel/UC-SiqAyRXR6eijPggFhFG2g");
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Support and Contribution Chat via Discord")) {
                Application.OpenURL("https://discord.gg/huSAuqk");
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Source Code on GitHub")) {
                Application.OpenURL("https://github.com/michaelday008/AnyRPGCore");
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Project Roadmap on Trello")) {
                Application.OpenURL("https://trello.com/anyrpg/");
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        public static void GettingStartedPageContent() {
            GUILayout.BeginVertical("window");

            /*
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("<b>1</b>- First you need to Import our <b>ProjectSettings</b>, otherwise you will get errors about missing Inputs and Layers. Then create a new folder for your Project and put your files there, don't use the AnyRPG Folder to avoid losing files when updating to a new version.");
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
            */

            /*
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("<b>2</b>- Never modify a default resource file (Animator, Prefabs, etc...) that comes with AnyRPG, instead" +
                " create a copy of the original file and place it inside your project folder.");
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("<b>3</b>- When modifying the AnyRPG scripts, make sure to comment the original source and create a #region for ex: 'MyCustomModification' " +
                "so it's easier to find and implement again once you update the template to a newer version.");
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
            */

            EditorGUILayout.HelpBox("- ALWAYS BACKUP your project before updating!", MessageType.Warning, true);
            //EditorGUILayout.HelpBox("- To update your version you need to Delete the AnyRPG folder, this way you won't get any conflicts between old files and newer files.", MessageType.Info, true);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        public static void Support() {
            GUILayout.BeginVertical("window");

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Support is available through Discord.\n\n- Get help \n- Share Your Work \n- Contribute Art \n- Contribute Code \n- Get Project Updates", MessageType.Info);
            if (GUILayout.Button("Open Discord")) {
                Application.OpenURL("https://discord.gg/huSAuqk");
            }
            GUILayout.EndVertical();


            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion

    }
}