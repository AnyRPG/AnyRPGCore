using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SetRenderQueue : MonoBehaviour {

        [Header("Render Queue")]

        [Tooltip("The render queue to use for any materials attached to this object")]
        [SerializeField]
        private int renderQueue = 3100;

        void Start() {
            Debug.Log(gameObject.name + ".SetRenderQueue.Start()");
            SetCustomQueue();
        }

        /// <summary>
        /// set a custom render queue on all materials attached to all renderers on this object
        /// </summary>
        public void SetCustomQueue() {
            Debug.Log(gameObject.name + ".SetRenderQueue.SetCustomQueue()");
            MeshRenderer[] meshRenderers = GetComponents<MeshRenderer>();
            if (meshRenderers != null) {
                foreach (Renderer renderer in meshRenderers) {
                    Debug.Log(gameObject.name + ".SetRenderQueue.SetCustomQueue(): found a renderer");
                    if (renderer.materials != null) {
                        foreach (Material material in renderer.materials) {
                            Debug.Log(gameObject.name + ".SetRenderQueue.SetCustomQueue(): found material: " + material.name + "; setting queue: " + renderQueue);
                            material.renderQueue = renderQueue;
                        }
                    }
                }
            }
        }

    }
}
