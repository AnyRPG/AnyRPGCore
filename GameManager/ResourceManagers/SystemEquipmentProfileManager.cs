using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class SystemEquipmentProfileManager : SystemResourceManager {

    #region Singleton
    private static SystemEquipmentProfileManager instance;

    public static SystemEquipmentProfileManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<SystemEquipmentProfileManager>();
            }

            return instance;
        }
    }
    #endregion

    const string resourceClassName = "EquipmentProfile";

    protected override void Awake() {
        //Debug.Log(this.GetType().Name + ".Awake()");
        base.Awake();
    }

    public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
        rawResourceList = Resources.LoadAll<EquipmentProfile>(resourceClassName);
        base.LoadResourceList();
    }

    public EquipmentProfile GetResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (resourceList[keyName] as EquipmentProfile);
            }
        }
        return null;
    }

    public EquipmentProfile GetNewResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (ScriptableObject.Instantiate(resourceList[keyName]) as EquipmentProfile);
            }
        }
        return null;
    }

    public List<EquipmentProfile> GetResourceList() {
        List<EquipmentProfile> returnList = new List<EquipmentProfile>();

        foreach (UnityEngine.Object listItem in resourceList.Values) {
            returnList.Add(listItem as EquipmentProfile);
        }
        return returnList;
    }
}

}