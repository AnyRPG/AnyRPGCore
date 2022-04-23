using AnyRPG;
using UnityEngine;
using System.Collections;

namespace AnyRPG {
    public static class TransformExtension {

        public static Transform FindChildByRecursive(this Transform aParent, string aName, bool partialMatch = false, bool caseInsensitive = false) {
            /*
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            */
            if (caseInsensitive == true) {
                aName = aName.ToLower();
            }
            Transform result = null;
            string searchName = string.Empty;
            foreach (Transform child in aParent) {
                //Debug.Log("searching " + child.name + " for " + aName);
                searchName = child.name;
                if (caseInsensitive == true) {
                    searchName = searchName.ToLower();
                }
                if (partialMatch == true) {
                    if (searchName.Contains(aName)) {
                        //Debug.Log("searching " + child.name + " for " + aName + ". FOUND match.  returning.");
                        return child;
                    }
                } else {
                    if (searchName == aName) {
                        //Debug.Log("searching " + child.name + " for " + aName + ". FOUND match.  returning.");
                        return child;
                    }
                }
                result = child.FindChildByRecursive(aName, partialMatch);
                if (result != null) {
                    return result;
                }
            }
            return result;
        }

        /*
        public static Transform FindChildByRecursive(this Transform aParent, string aName) {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent) {
                result = child.FindChildByRecursive(aName);
                if (result != null)
                    return result;
            }
            return null;
        }
        */
    }
}
