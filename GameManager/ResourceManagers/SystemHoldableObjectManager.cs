using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemHoldableObjectManager : SystemResourceManager {

    #region Singleton
    private static SystemHoldableObjectManager instance;

    public static SystemHoldableObjectManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<SystemHoldableObjectManager>();
            }

            return instance;
        }
    }
    #endregion

    const string resourceClassName = "HoldableObject";

    protected override void Awake() {
        //Debug.Log(this.GetType().Name + ".Awake()");
        base.Awake();
    }

    public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
        rawResourceList = Resources.LoadAll<HoldableObject>(resourceClassName);
        base.LoadResourceList();
    }

    public HoldableObject GetResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (resourceList[keyName] as HoldableObject);
            }
        }
        return null;
    }


    public HoldableObject GetNewResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (ScriptableObject.Instantiate(resourceList[keyName]) as HoldableObject);
            }
        }
        return null;
    }


    public List<HoldableObject> GetResourceList() {
        List<HoldableObject> returnList = new List<HoldableObject>();

        foreach (UnityEngine.Object listItem in resourceList.Values) {
            returnList.Add(listItem as HoldableObject);
        }
        return returnList;
    }
}
