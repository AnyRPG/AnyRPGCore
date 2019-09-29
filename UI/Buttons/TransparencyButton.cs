using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransparencyButton : MonoBehaviour {

    [SerializeField]
    private Image backGroundImage;

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;


    protected virtual void Start() {
        //Debug.Log("TransparencyButton.Start()");
        GetComponentReferences();
        SetBackGroundTransparency();
        startHasRun = true;
        CreateEventReferences();
    }

    private void CreateEventReferences() {
        //Debug.Log("TransparencyButton.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPagedButtonsTransparencyUpdate += SetBackGroundTransparency;
        eventReferencesInitialized = true;
    }

    private void CleanupEventReferences() {
        //Debug.Log("TransparencyButton.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPagedButtonsTransparencyUpdate -= SetBackGroundTransparency;
        eventReferencesInitialized = false;
    }

    public void OnDestroy() {
        //Debug.Log("PlayerManager.OnDisable()");
        CleanupEventReferences();
    }

    protected virtual void GetComponentReferences() {
        //Debug.Log("TransparencyButton.GetComponentReferences()");
        if (backGroundImage == null) {
            backGroundImage = GetComponent<Image>();
            //Debug.Log("TransparencyButton.GetComponentReferences()");
        } else {
            //Debug.Log("TransparencyButton.GetComponentReferences(): bg image set");
        }
    }

    public void SetBackGroundTransparency() {
        //Debug.Log("TransparencyButton.SetBackGroundTransparency()");
        int opacityLevel = (int)(PlayerPrefs.GetFloat("PagedButtonsOpacity") * 255f);
        //Debug.Log("TransparencyButton.GetComponentReferences(): got opacity: " + opacityLevel);
        backGroundImage.color = new Color32(0, 0, 0, (byte)opacityLevel);
    }
}
