using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class EdibleObject : MonoBehaviour {

        GameObject[] portions;
        int currentIndex;
        float lastChange;

        [Tooltip("The time in seconds between changing to the next object")]
        [SerializeField]
        private float interval = 1f;

        [Tooltip("If true, the last object will stay visible after its interval")]
        [SerializeField]
        private bool keepLastVisible = false;

        void OnEnable() {
            lastChange = Time.time;
            portions = new GameObject[transform.childCount];
            currentIndex = 0;
            for (int i = 0; i < portions.Length; i++) {
                portions[i] = transform.GetChild(i).gameObject;
                if (i == 0) {
                    portions[i].SetActive(true);
                } else {
                    portions[i].SetActive(false);
                }
            }
            /*
            for (int i = 0; i < portions.Length; i++) {
                portions[i] = transform.GetChild(i).gameObject;
                if (portions[i].activeInHierarchy)
                    currentIndex = i;
            }
            */
        }

        void Update() {
            if (Time.time - lastChange > interval) {
                Consume();
                lastChange = Time.time;
            }
        }

        void Consume() {
            if (currentIndex < portions.Length - (keepLastVisible == true ? 1 : 0)) {
                portions[currentIndex].SetActive(false);
            }
            if (currentIndex < portions.Length) {
                currentIndex++;
            }
            if (currentIndex == portions.Length) {
                return;
            }
            portions[currentIndex].SetActive(true);

            /*
            if (currentIndex > portions.Length)
                currentIndex = 0;
            else if (currentIndex == portions.Length)
                return;
            */
        }

        /*
        public void OnSendObjectToPool() {
            Debug.Log("EdibleObject.OnSendObjectToPool()");

            for (int i = 0; i < portions.Length; i++) {
                if (i == 0) {
                    portions[i].SetActive(true);
                } else {
                    portions[i].SetActive(false);
                }
            }
        }
        */

    }

}
