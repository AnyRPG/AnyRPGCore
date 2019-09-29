﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// responsible for detecting mouseover the nameplate canvas to allow override of overgameobject detection
public class NamePlateCanvasController : MonoBehaviour {

    #region Singleton
    private static NamePlateCanvasController instance;

    public static NamePlateCanvasController MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<NamePlateCanvasController>();
            }

            return instance;
        }
    }

    #endregion

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    void Start() {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
    }

    public bool MouseOverNamePlate() {

        //Set up the new Pointer Event
        m_PointerEventData = new PointerEventData(m_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        m_Raycaster.Raycast(m_PointerEventData, results);

        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        bool gotMiniMap = false;
        foreach (RaycastResult result in results) {
            //Debug.Log("Hit " + result.gameObject.name);
            if (result.gameObject.layer == LayerMask.NameToLayer("MiniMap")) {
                gotMiniMap = true;
            } //else {
                //Debug.Log(result.gameObject.name + " got hit with raycast");
            //}
        }

        return gotMiniMap;
    }
}
