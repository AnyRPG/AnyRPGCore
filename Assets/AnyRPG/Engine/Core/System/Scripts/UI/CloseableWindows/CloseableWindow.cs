using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CloseableWindow : MonoBehaviour {

        public event System.Action OnOpenWindowCallback = delegate { };
        public event System.Action OnCloseWindowCallback = delegate { };

        protected ICloseableWindowContents windowContents;

        [Header("Closeable Window")]

        [SerializeField]
        protected GameObject contentParent;

        [SerializeField]
        protected GameObject contentPrefab;

        // put the prefab here if its included directly and not spawned
        [SerializeField]
        protected GameObject contentGameObject;

        [SerializeField]
        protected string windowTitle;

        [SerializeField]
        protected TextMeshProUGUI windowText;

        // for controlling background color
        [SerializeField]
        protected Image backGroundImage;

        /*
        [SerializeField]
        protected CanvasGroup canvasGroup;
        */

        protected bool windowInitialized = false;

        public ICloseableWindowContents CloseableWindowContents { get => windowContents; set => windowContents = value; }

        public bool IsOpen {
            get {
                //return (canvasGroup == null ? false : canvasGroup.alpha > 0);
                return gameObject.activeSelf == true;
            }
        }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ".CloseableWindow.Awake()");
            if (!windowInitialized) {
                InitializeWindow();
                RawCloseWindow();
            }
        }

        protected virtual void InitializeWindow() {
            //Debug.Log(gameObject.name + ".CloseableWindow.InitializeWindow()");
            if (windowInitialized) {
                return;
            }
            if (windowText != null) {
                windowText.text = windowTitle;
            }
            InitializeWindowContentsCommon();
            windowInitialized = true;
        }

        public void InitializeWindowContentsCommon() {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.InitializeWindowContentsCommon()");
            if (contentPrefab != null && windowContents == null && contentGameObject == null) {
                contentGameObject = ObjectPooler.Instance.GetPooledObject(contentPrefab, contentParent.transform);
                //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.InitializeWindowContentsCommon(): Instantiating window Contents: " + contentPrefab.name + " and got id: " + contentGameObject.GetInstanceID());
            }
            if (contentGameObject != null) {
                //Debug.Log(gameObject.name + ".CloseableWindow.InitializeWindow(): Instanted; setting window Contents");
                windowContents = contentGameObject.GetComponent<ICloseableWindowContents>();
                windowContents.Init();
            }
        }

        public virtual void InitalizeWindowContents(GameObject contentPrefab, string title) {
            //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.InitializeWindowContents(" + contentPrefab.name + ", " + title + ")");

            this.contentPrefab = contentPrefab;
            this.windowTitle = title;
            this.windowText.text = this.windowTitle;

            InitializeWindow();

            InitializeWindowContentsCommon();
        }

        public virtual void DestroyWindowContents() {
            //Debug.Log(gameObject.name + ".CloseableWindow.DestroyWindowContents()");
            if (windowContents != null) {
                //Debug.Log(gameObject.name + gameObject.GetInstanceID() + ".CloseableWindow.DestroyWindowContents(): " + CloseableWindowContents.gameObject.name + CloseableWindowContents.gameObject.GetInstanceID());
                ObjectPooler.Instance.ReturnObjectToPool(CloseableWindowContents.gameObject);
                windowContents = null;
                contentGameObject = null;
            }

        }

        public virtual void OpenWindow() {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow()");
            InitializeWindow();
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
            InitializeWindow();
            if (IsOpen == false) {
                RawCloseWindow();
                return;
            }
            RawCloseWindow();
            //Debug.Log(gameObject.name + ".CloseableWindow.CloseWindow(): alpha should be set to zero: doing callbacks");
            OnCloseWindowCallback();
            if (windowContents != null) {
                windowContents.RecieveClosedWindowNotification();
            }
            if (windowText != null && windowTitle != null && windowTitle != string.Empty) {
                windowText.text = windowTitle;
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
            //Debug.Log("CloseableWindow.ToggleOpenClose()");
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

    }

}