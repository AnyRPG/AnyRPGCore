using System;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            new ToolBar("Included Demo Games", DemoGamesTabContent),
            new ToolBar("Create Your Game", CreateYourGameTabContent),
            new ToolBar("Support", SupportTabContent)
        };
        #endregion

        public const string installedVersion = "0.11a";

        public const string _projectSettingsPath = "Assets/AnyRPG/Basic Locomotion/Resources/ProjectSettings.unitypackage";

        public static Texture2D welcomeBanner = null;

        public static Vector2 scrollPosition;

        //GUISkin skin;
        private const int windowWidth = 600;
        private const int windowHeight = 600;

        public static bool DisplayWelcomeScreen {
            get { return PlayerPrefs.GetInt("DisplayWelcomeScreen") == 1 ? true : false; }
            set {
                if (value != (PlayerPrefs.GetInt("DisplayWelcomeScreen") == 1 ? true : false)) {
                    PlayerPrefs.SetInt("DisplayWelcomeScreen", value == true ? 1 : 0);
                }
            }
        }

        [MenuItem("Tools/AnyRPG/Welcome Window", false, windowWidth)]
        public static void Open() {
            if (PlayerPrefs.HasKey("DisplayWelcomeWindow") == false) {
                PlayerPrefs.SetInt("DisplayWelcomeWindow", 1);
            }
            GetWindow<WelcomeWindow>(true);
        }

        public void OnEnable() {
            titleContent = new GUIContent("Welcome To AnyRPG");
            maxSize = new Vector2(windowWidth, windowHeight);
            minSize = maxSize;
            InitStyle();
        }

        void InitStyle() {

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
            GUILayout.Label(welcomeBanner);
        }

        private void DrawMenuButtons() {
            //GUILayout.Space(-10);
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
            GUILayout.BeginArea(new Rect(4, 140, 592, 430));
            toolBars[toolBarIndex].Draw();
            GUILayout.EndArea();
            GUILayout.FlexibleSpace();
        }

        private void DrawBottom() {
            GUILayout.BeginHorizontal("box");

            DisplayWelcomeScreen = GUILayout.Toggle(DisplayWelcomeScreen, "Display this window at startup");

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

        public static void DemoGamesTabContent() {
            GUILayout.BeginVertical("window");

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Content Demo")) {
                EditorSceneManager.OpenScene("Assets/AnyRPG/Engine/Games/ContentDemo/Scenes/Game/ContentDemoGame/ContentDemoGame.unity");
            }
            EditorGUILayout.HelpBox("A simple demo of all 3d and audio content including\n -Music\n -Clothing\n -Characters\n -Buildings\n -Props\n -Weapons", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("A Lost Soul Story Demo")) {
                EditorSceneManager.OpenScene("Assets/ALostSoul/Games/ALostSoulStoryDemo/Scenes/Game/ALostSoulStoryDemoGame/ALostSoulStoryDemoGame.unity");
            }
            EditorGUILayout.HelpBox("The first 2 chapters of the game, 'A Lost Soul', re-created using the open source assets included in AnyRPG", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("A Lost Soul Character Demo")) {
                EditorSceneManager.OpenScene("Assets/ALostSoul/Games/ALostSoulCharacterDemo/Scenes/Game/ALostSoulCharacterDemoGame/ALostSoulCharacterDemoGame.unity");
            }
            EditorGUILayout.HelpBox("Explore the game world of A Lost Soul by starting as any character model and faction included in the game", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Core Game")) {
                EditorSceneManager.OpenScene("Assets/AnyRPG/Engine/Core/Games/CoreGame/Scenes/Game/CoreGame/CoreGame.unity");
            }
            EditorGUILayout.HelpBox("A simple 2 level game that provides examples of the most common features and interactables included in AnyRPG for quick reference when implementing them in your own game", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Empty (Zero Config Mode) Game")) {
                EditorSceneManager.OpenScene("Assets/AnyRPG/Engine/Core/Games/EmptyGame/Scenes/EmptyGame/EmptyGame.unity");
            }
            EditorGUILayout.HelpBox("A bare bones single scene with no main menu that demonstrates how to use AnyRPG in Zero Config (Controller Only) mode by including an unconfigured " +
                "GameManager into any scene", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);


            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        public static void CreateYourGameTabContent() {
            GUILayout.BeginVertical("window");

            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Although it is completely possible to create your game(s) manually by setting up your own folder structure, " +
                "making all the basic prefabs and scenes, and creating a Game Manager variant, it's much easier to use the included new game wizard", MessageType.Info);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Create A New Game Now")) {
                NewGameWizard.CreateWizard();
            }
            EditorGUILayout.HelpBox("The New Game Wizard can be found on the Unity Editor menu bar at Tools -> AnyRPG -> Wizard -> New Game Wizard", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        public static void SupportTabContent() {
            GUILayout.BeginVertical("window");

            EditorGUILayout.HelpBox("AnyRPG Installed Version: " + installedVersion, MessageType.Info);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Official Website")) {
                Application.OpenURL("https://www.anyrpg.org");
            }
            EditorGUILayout.HelpBox("Download the latest AnyRPG Unity packages and playable games", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Wiki and Documentation")) {
                Application.OpenURL("https://wiki.anyrpg.org");
            }
            EditorGUILayout.HelpBox("Contribute to AnyRPG by writing documentation/tutorials or reading official and user contributed guides", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Youtube Channel and Tutorials")) {
                Application.OpenURL("https://www.youtube.com/channel/UC-SiqAyRXR6eijPggFhFG2g");
            }
            EditorGUILayout.HelpBox("Development live streams and AnyRPG tutorial screencasts can be found on YouTube", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Support and Contribution Chat via Discord")) {
                Application.OpenURL("https://discord.gg/huSAuqk");
            }
            EditorGUILayout.HelpBox("Get live help, share your work, contribute art, contribute code, and see real-time project updates", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Source Code on GitHub")) {
                Application.OpenURL("https://github.com/michaelday008/AnyRPGCore");
            }
            EditorGUILayout.HelpBox("Report bugs and test features that are under development", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            if (GUILayout.Button("Project Roadmap on Trello")) {
                Application.OpenURL("https://trello.com/anyrpg/");
            }
            EditorGUILayout.HelpBox("Suggest new features and see the project roadmap and status", MessageType.None);
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

       

        #endregion

    }
}