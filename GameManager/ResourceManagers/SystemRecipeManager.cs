using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemRecipeManager : SystemResourceManager {

    #region Singleton
    private static SystemRecipeManager instance;

    public static SystemRecipeManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<SystemRecipeManager>();
            }

            return instance;
        }
    }

    #endregion

    const string resourceClassName = "Recipe";

    protected override void Awake() {
        //Debug.Log(this.GetType().Name + ".Awake()");
        base.Awake();
    }

    public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
        rawResourceList = Resources.LoadAll<Recipe>(resourceClassName);
        base.LoadResourceList();
    }

    public Recipe GetResource(string resourceName) {
        //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
        if (!RequestIsEmpty(resourceName)) {
            string keyName = prepareStringForMatch(resourceName);
            if (resourceList.ContainsKey(keyName)) {
                return (resourceList[keyName] as Recipe);
            }
        }
        return null;
    }

    public List<Recipe> GetResourceList() {
        List<Recipe> returnList = new List<Recipe>();

        foreach (UnityEngine.Object listItem in resourceList.Values) {
            returnList.Add(listItem as Recipe);
        }
        return returnList;
    }


}
