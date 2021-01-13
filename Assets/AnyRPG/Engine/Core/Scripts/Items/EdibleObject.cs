using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class EdibleObject : MonoBehaviour {

        GameObject[] portions;
        int currentIndex;
        float lastChange;

        [SerializeField]
        private float interval = 1f;

        void Start() {
            bool skipFirst = transform.childCount > 4;
            portions = new GameObject[skipFirst ? transform.childCount - 1 : transform.childCount];
            for (int i = 0; i < portions.Length; i++) {
                portions[i] = transform.GetChild(skipFirst ? i + 1 : i).gameObject;
                if (portions[i].activeInHierarchy)
                    currentIndex = i;
            }
        }

        void Update() {
            if (Time.time - lastChange > interval) {
                Consume();
                lastChange = Time.time;
            }
        }

        void Consume() {
            if (currentIndex != portions.Length)
                portions[currentIndex].SetActive(false);
            currentIndex++;
            if (currentIndex > portions.Length)
                currentIndex = 0;
            else if (currentIndex == portions.Length)
                return;
            portions[currentIndex].SetActive(true);
        }

    }

}
