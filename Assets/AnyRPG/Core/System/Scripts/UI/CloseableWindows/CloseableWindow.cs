using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CloseableWindow : ConfiguredMonoBehaviour {

        public event System.Action OnOpenWindowCallback = delegate { };
        public event System.Action OnCloseWindowCallback = delegate { };

        protected ICloseableWindowContents windowContents;

        [Header("Closeable Window")]

        [SerializeField]
        protected DraggableWindow dragHandle = null;

        [SerializeField]
        protected GameObject contentParent = null;

        [SerializeField]
        protected GameObject contentPrefab = null;

        // put the prefab here if its included directly and not spawned
        [SerializeField]
        protected GameObject contentGameObject = null;

        [SerializeField]
        protected string windowTitle = string.Empty;

        [SerializeField]
        protected TextMeshProUGUI windowText = null;

        // for controlling background color
        [SerializeField]
        protected Image backGroundImage = null;

        [Tooltip("Used to save and load window positions")]
        [SerializeField]
        protected RectTransform rectTransform = null;

        [SerializeField]
        protected HintBarController hintBarController = null;

        // game manager references
        protected ObjectPooler objectPooler = null;

        public ICloseableWindowContents CloseableWindowContents { get => windowContents; set => windowContents = value; }

        public bool IsOpen {
            get {
                //return (canvasGroup == null ? false : canvasGroup.alpha > 0);
                return gameObject.activeSelf == true;
            }
        }

        public RectTransform RectTransform { get => rectTransform; }
        public DraggableWindow DragHandle { get => dragHandle; }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".CloseableWindow.Awake()");
            base.Configure(systemGameManager);
            if (dragHandle != null) {
                dragHandle.Configure(systemGameManager);
            }
            if (hintBarController != null) {
                hintBarController.Configure(systemGameManager);
            }
            InitializeWindow();
            RawCloseWindow();
            HideControllerHints();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        public void Init() {
            windowContents.Init();
        }

        protected virtual void InitializeWindow() {
            //Debug.Log(gameObject.name + ".CloseableWindow.InitializeWindow()");
            if (windowText != null) {
                windowText.text = windowTitle;
            }
            InitializeWindowContentsCommon();
        }

        public void InitializeWindowContentsCommon() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.InitializeWindowContentsCommon()");
            if (contentPrefab != null && windowContents == null && contentGameObject == null) {
                contentGameObject = objectPooler.GetPooledObject(contentPrefab, contentParent.transform);
                //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.InitializeWindowContentsCommon(): Instantiating window Contents: " + contentPrefab.name + " and got id: " + contentGameObject.GetInstanceID());
            }
            if (contentGameObject != null) {
                //Debug.Log(gameObject.name + ".CloseableWindow.InitializeWindow(): Instanted; setting window Contents");
                windowContents = contentGameObject.GetComponent<ICloseableWindowContents>();
                windowContents.Configure(systemGameManager);
                SetContentOwner();
            }
        }

        public virtual void SetContentOwner() {
            windowContents.SetWindow(this);
        }

        public virtual void InitalizeWindowContents(GameObject contentPrefab, string title) {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.InitializeWindowContents(" + contentPrefab.name + ", " + title + ")");

            this.contentPrefab = contentPrefab;
            this.windowTitle = title;
            this.windowText.text = this.windowTitle;

            //InitializeWindow();

            InitializeWindowContentsCommon();
        }

        public virtual void DestroyWindowContents() {
            //Debug.Log(gameObject.name + ".CloseableWindow.DestroyWindowContents()");
            if (windowContents != null) {
                //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.DestroyWindowContents(): " + CloseableWindowContents.gameObject.name + CloseableWindowContents.gameObject.GetInstanceID());
                objectPooler.ReturnObjectToPool(CloseableWindowContents.gameObject);
                windowContents = null;
                contentGameObject = null;
            }

        }

        public virtual void OpenWindow() {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow()");
            //InitializeWindow();
            if (IsOpen) {
                return;
            }
            if (windowContents != null) {
                //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow(): turning up alpha and setting to interactable");
                /*
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                */
                gameObject.SetActive(true);
            }

            if (windowContents != null) {
                windowContents.ReceiveOpenWindowNotification();
            }

            OnOpenWindowCallback();
        }

        public virtual void CloseWindow() {
            //Debug.Log(gameObject.name + ".CloseableWindow.CloseWindow()");
            //InitializeWindow();
            if (IsOpen == false) {
                RawCloseWindow();
                return;
            }
            RawCloseWindow();
            //Debug.Log(gameObject.name + ".CloseableWindow.CloseWindow(): alpha should be set to zero: doing callbacks");
            OnCloseWindowCallback();
            if (windowContents != null) {
                windowContents.ReceiveClosedWindowNotification();
            }
            if (windowText != null && windowTitle != null && windowTitle != string.Empty) {
                windowText.text = windowTitle;
            }
            if (hintBarController != null) {
                hintBarController.Hide();
            }
        }

        public void RawCloseWindow() {
            //Debug.Log(gameObject.name + ".CloseableWindow.RawCloseWindow()");
            /*
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            */
            gameObject.SetActive(false);
        }

        public void ToggleOpenClose() {
            //Debug.Log(gameObject.name + ".CloseableWindow.ToggleOpenClose()");
            if (IsOpen) {
                CloseWindow();
            } else {
                OpenWindow();
            }
        }

        public void SetBackGroundColor(Color color) {
            if (CloseableWindowContents != null) {
                CloseableWindowContents.SetBackGroundColor(color);
            }
        }

        public void SetWindowTitle(string newTitle) {
            windowText.text = newTitle;
        }

        public void SetControllerHints(string aOption, string xOption, string yOption, string bOption, string dPadOption, string rDownOption) {
            //Debug.Log(gameObject.name + ".CloseableWindow.SetControllerHints()");
            if (hintBarController != null) {
                hintBarController.SetOptions(aOption, xOption, yOption, bOption, dPadOption, rDownOption);
            }
        }

        public void HideControllerHints() {
            //Debug.Log(gameObject.name + ".CloseableWindow.HideControllerHints()");
            if (hintBarController != null) {
                hintBarController.Hide();
            }
        }

        public void LockUI() {
            if (dragHandle != null) {
                dragHandle.LockUI();
            }
        }

        public void LeftAnalog(float inputHorizontal, float inputVertical) {
            //Debug.Log(gameObject.name + ".CloseableWindow.LeftAnalog()");

            if (dragHandle != null) {
                dragHandle.LeftAnalog(inputHorizontal, inputVertical);
                return;
            }
        }

        /*
        public void OnDisable() {
            Debug.Log(gameObject.name + ".CloseableWindow.OnDisable()");
        }
        */

    }

}