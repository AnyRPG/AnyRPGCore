using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

public class SystemCurrencyManager : SystemResourceManager {

    #region Singleton
    private static SystemCurrencyManager instance;

    public static SystemCurrencyManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<SystemCurrencyManager>();
            }

            return instance;
        }
    }
    #endregion

    const string resourceClassName = "Currency";

    protected override void Awake() {
        //Debug.Log(this.GetType().Name + ".Awake()");
        base.Awake();
    }

    public override void LoadResourceList() {
        //Debug.Log(this.GetType().Name + ".LoadResourceList()");
        rawResourceList = Resources.LoadAll<Currency>(resourceClassName);
        base.LoadResourceList();
    }

    public Currency GetResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (resourceList[keyName] as Currency);
            }
        }
        return null;
    }

    public List<Currency> GetResourceList() {
        List<Currency> returnList = new List<Currency>();

        foreach (UnityEngine.Object listItem in resourceList.Values) {
            returnList.Add(listItem as Currency);
        }
        return returnList;
    }



}

}