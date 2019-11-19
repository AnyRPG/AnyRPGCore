using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
public class TransparencyButton : MonoBehaviour {

    [SerializeField]
    private Image backGroundImage;

    protected bool eventSubscriptionsInitialized = false;


    protected virtual void Start() {
        //Debug.Log("TransparencyButton.Start()");
        GetComponentReferences();
        SetBackGroundTransparency();
        CreateEventSubscriptions();
    }

    private void CreateEventSubscriptions() {
        //Debug.Log("TransparencyButton.CreateEventSubscriptions()");
        if (eventSubscriptionsInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPagedButtonsTransparencyUpdate += SetBackGroundTransparency;
        eventSubscriptionsInitialized = true;
    }

    private void CleanupEventSubscriptions() {
        //Debug.Log("TransparencyButton.CleanupEventSubscriptions()");
        if (!eventSubscriptionsInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPagedButtonsTransparencyUpdate -= SetBackGroundTransparency;
        eventSubscriptionsInitialized = false;
    }

    public void OnDestroy() {
        //Debug.Log("PlayerManager.OnDisable()");
        CleanupEventSubscriptions();
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

}