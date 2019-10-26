using AnyRPG;
using UnityEngine;
using System.Collections;

namespace AnyRPG {
    public static class TransformExtension {
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
    }
}
