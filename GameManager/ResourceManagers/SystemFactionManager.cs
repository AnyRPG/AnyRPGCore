using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
public class SystemFactionManager : SystemResourceManager {

    #region Singleton
    private static SystemFactionManager instance;

    public static SystemFactionManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<SystemFactionManager>();
            }

            return instance;
        }
    }

    #endregion

    const string resourceClassName = "Faction";

    // the icon shown when a player has no faction
    [SerializeField]
    private Sprite defaultIcon;

    public Sprite MyDefaultIcon { get => defaultIcon; set => defaultIcon = value; }

    protected override void Awake() {
        base.Awake();
    }

    public override void LoadResourceList() {
        rawResourceList = Resources.LoadAll<Faction>(resourceClassName);
        base.LoadResourceList();
    }

    public Faction GetResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (resourceList[keyName] as Faction);
            }
        }
        return null;
    }

    public List<Faction> GetResourceList() {
        List<Faction> returnList = new List<Faction>();

        foreach (UnityEngine.Object listItem in resourceList.Values) {
            returnList.Add(listItem as Faction);
        }
        return returnList;
    }

}

}