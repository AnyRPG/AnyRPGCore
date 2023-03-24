using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    public static class LayerUtility {

        public static void SetMeshRendererLayerRecursive(GameObject objectName, int newLayer, int ignoreMask) {
            objectName.layer = newLayer;
            foreach (MeshRenderer meshRenderer in objectName.gameObject.GetComponentsInChildren<MeshRenderer>(true)) {
                if (!IsInLayerMask(meshRenderer.gameObject.layer, ignoreMask)) {
                    meshRenderer.gameObject.layer = newLayer;
                }
            }
        }

        public static void SetTransformLayerRecursive(GameObject objectName, int newLayer, int ignoreMask) {
            objectName.layer = newLayer;
            foreach (Transform childTransform in objectName.gameObject.GetComponentsInChildren<Transform>(true)) {
                if (!IsInLayerMask(childTransform.gameObject.layer, ignoreMask)) {
                    childTransform.gameObject.layer = newLayer;
                }
            }
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask) {
            return layermask == (layermask | (1 << layer));
        }

    }
}
