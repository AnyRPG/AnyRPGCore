using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    public static class TransformExtension {

        public static Transform FindChildByRecursive(this Transform aParent, string aName, bool partialMatch = false, bool caseInsensitive = false, List<string> ignoreBones = null) {
            //Debug.Log("FindChildByRecursive(" + aParent.name + ", " + aName + ", " + partialMatch + ", " + caseInsensitive + ")");
            /*
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            */
            ignoreBones = ignoreBones ?? new List<string>();

            if (caseInsensitive == true) {
                aName = aName.ToLower();
            }
            Transform result = null;
            string searchName = string.Empty;
            foreach (Transform child in aParent) {
                //Debug.Log("searching " + child.name + " for " + aName);
                bool hadIgnoreBone = false;
                foreach (string ignoreBone in ignoreBones) {
                    //Debug.Log("comparing " + child.name.Substring(0, ignoreBone.Length) + " to " + ignoreBone);
                    if (child.name.Substring(0, ignoreBone.Length) == ignoreBone) {
                        hadIgnoreBone = true;
                    }
                }
                if (hadIgnoreBone == true) {
                    continue;
                }

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
                result = child.FindChildByRecursive(aName, partialMatch, caseInsensitive, ignoreBones);
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
