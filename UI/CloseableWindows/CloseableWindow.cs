using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseableWindow : MonoBehaviour {

    public event System.Action OnOpenWindowCallback = delegate { };
    public event System.Action OnCloseWindowCallback = delegate { };

    protected ICloseableWindowContents windowContents;

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
    protected Text windowText;

    // for controlling background color
    [SerializeField]
    protected Image backGroundImage;

    protected CanvasGroup canvasGroup;

    protected bool windowInitialized = false;

    public ICloseableWindowContents MyCloseableWindowContents { get => windowContents; set => windowContents = value; }

    public bool IsOpen {
        get {
            return (canvasGroup == null ? false : canvasGroup.alpha > 0);
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
        canvasGroup = GetComponent<CanvasGroup>();
        if (windowText != null) {
            windowText.text = windowTitle;
            //Debug.Log("CloseableWindow.InitializeWindow(): windowTitle: " + windowTitle);
        }
        InitializeWindowContentsCommon();
        windowInitialized = true;
    }

    public void InitializeWindowContentsCommon() {
        //Debug.Log(gameObject.name + ".CloseableWindow.InitializeWindow()");
        if (contentPrefab != null && windowContents == null && contentGameObject == null) {
            //Debug.Log(gameObject.name + ".CloseableWindow.InitializeWindow(): Instantiating window Contents");
            contentGameObject = Instantiate(contentPrefab, contentParent.transform);
        }
        if (contentGameObject != null) {
            //Debug.Log(gameObject.name + ".CloseableWindow.InitializeWindow(): Instanted; setting window Contents");
            windowContents = contentGameObject.GetComponent<ICloseableWindowContents>();
        }
    }

    public virtual void InitalizeWindowContents(GameObject contentPrefab, string title) {
        //Debug.Log("CloseableWindow.InitializeWindowContents(" + title + ")");

        this.contentPrefab = contentPrefab;
        this.windowTitle = title;
        this.windowText.text = this.windowTitle;

        InitializeWindow();

        InitializeWindowContentsCommon();
    }

    public virtual void DestroyWindowContents() {
        //Debug.Log("CloseableWindow.DestroyWindowContents()");
        if (windowContents != null) {
            Destroy((MyCloseableWindowContents as MonoBehaviour).gameObject);
            windowContents = null;
        }

    }

    public virtual void OpenWindow() {
        //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow()");
        InitializeWindow();
        if (IsOpen) {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow(): window already open.  doing nothing");
            return;
        } else {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow(): window was not open, proceeding");
        }
        if (windowContents != null) {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow(): turning up alpha and setting to interactable");
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        } else {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow():  + windowContents was null!");
        }

        // THIS MUST BE HERE SO WINDOWS THAT NEED TO AUTO-CLOSE THEMSELVES ON OPEN IN THE OPEN WINDOW HANDLER DON'T END UP JUST RE-ENABLING THEMSELVES WITH THE ABOVE CODE AFTER THE CLOSE FUNCTION IS RUN
        if (windowContents != null) {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow(): window contents was not null, calling openwindow");
            windowContents.OnOpenWindow();
        } else {
            //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow():  + windowContents is null!");
        }
        //Debug.Log(gameObject.name + ".CloseableWindow.OpenWindow(): calling onopenwindowcallback");
        OnOpenWindowCallback();

    }

    public virtual void CloseWindow() {
        //Debug.Log(gameObject.name + ".CloseableWindow.CloseWindow()");
        InitializeWindow();
        if (canvasGroup.alpha == 0) {
            RawCloseWindow();
            return;
        }
        RawCloseWindow();
        //Debug.Log(gameObject.name + ".CloseableWindow.CloseWindow(): alpha should be set to zero: doing callbacks");
        OnCloseWindowCallback();
        if (windowContents != null) {
            windowContents.OnCloseWindow();
        }
        if (windowText != null && windowTitle != null && windowTitle != string.Empty) {
            windowText.text = windowTitle;
        }
    }

    public void RawCloseWindow() {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void ToggleOpenClose() {
        //Debug.Log("CloseableWindow.ToggleOpenClose()");
        if (canvasGroup.alpha > 0) {
            CloseWindow();
        } else {
            OpenWindow();
        }
    }

    public void SetBackGroundColor(Color color) {
        if (MyCloseableWindowContents != null) {
            MyCloseableWindowContents.SetBackGroundColor(color);
        }
    }

    public void SetWindowTitle(string newTitle) {
        windowText.text = newTitle;
    }

}
