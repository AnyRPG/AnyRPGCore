/// <summary>
/// Sets camera cull distance for all layers to 'MaxCloseDistance' except objects that are on 'alwaysVisibleLayer'
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG{

	public class CameraCullDistances : MonoBehaviour {

		public float maxDistance = 200.0f;
		public int[] alwaysVisibleLayers = new int[] {31};

		void Start () {
			Camera cam = GetComponent<Camera> ();


			float[] distances = new float[32];

			for (int i = 0; i < distances.Length; i++) {
				if (Contains(i, alwaysVisibleLayers) == false){
					distances [i] = maxDistance;	
				}

			}


			cam.layerCullDistances = distances;
			cam.layerCullSpherical = true;
		}

		bool Contains(int x, int[] y){

			if (y.Length == 0) {
				return false;
			}

			for (int i = 0; i < y.Length; i++){
				if (y[i] == x){
					return true;
				}
			}

			return false;
		}
		

	}

}
