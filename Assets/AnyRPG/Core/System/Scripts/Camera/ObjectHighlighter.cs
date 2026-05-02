using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;


namespace AnyRPG {

	[RequireComponent(typeof(Camera))]
	public class ObjectHighlighter : MonoBehaviour {
		
		private Color outlineColor = new Color(1, 0, 0, 0.5f);
		public Camera cam;

		private Dictionary<Interactable, Renderer[]> meshRenderersDict = new Dictionary<Interactable, Renderer[]>();
		private Dictionary<Interactable, List<uint>> originalRenderingLayersDict = new Dictionary<Interactable, List<uint>>();

        private void Awake() {
			//cam.depthTextureMode = DepthTextureMode.Depth;
		}

		void OnDisable() {
			Cleanup();
		}

		void Cleanup() {
		}

		public void AddOutlinedObject(Interactable interactable, Color outlineColor, Renderer[] meshRenderers) {
			//Debug.Log($"ObjectHighlighter.AddOutlinedObject({interactable.gameObject.name}, {outlineColor})");

			if (meshRenderersDict.ContainsKey(interactable)) {
                // already highlighted
                return;
			}
			meshRenderersDict.Add(interactable, meshRenderers);
			originalRenderingLayersDict.Add(interactable, new List<uint>());

            OutlineVolumeComponent outlineVolume = VolumeManager.instance.stack.GetComponent<OutlineVolumeComponent>();
			outlineVolume.color1.value = outlineColor;

            // loop through mesh renderers and add Outline_1 to their rendering layers
            foreach (Renderer renderer in meshRenderers) {
				// store original rendering layer
				originalRenderingLayersDict[interactable].Add(renderer.renderingLayerMask);
                // get rendering layer by name
                int outlineLayer = RenderingLayerMask.NameToRenderingLayer("Outline_1");
                // add outline layer to existing rendering layers
				renderer.renderingLayerMask = renderer.renderingLayerMask | (1u << outlineLayer);
            }
        }

        public void RemoveOutlinedObject(Interactable interactable) {
            //Debug.Log($"ObjectHighlighter.RemoveOutlinedObject({interactable.gameObject.name})");

			if (meshRenderersDict.ContainsKey(interactable) == false) {
				return;
			}
            // restore original rendering layers
            for (int i = 0; i < meshRenderersDict[interactable].Length; i++) {
                if (i < originalRenderingLayersDict[interactable].Count) {
                    meshRenderersDict[interactable][i].renderingLayerMask = originalRenderingLayersDict[interactable][i];
                }
            }

			meshRenderersDict.Remove(interactable);
			originalRenderingLayersDict.Remove(interactable);
		}
		
	}

}