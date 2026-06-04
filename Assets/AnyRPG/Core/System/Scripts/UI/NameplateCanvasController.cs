using UnityEngine;
using UnityEngine.EventSystems;

namespace AnyRPG {
    // responsible for detecting mouseover the nameplate canvas to allow override of overgameobject detection
    public class NameplateCanvasController : MonoBehaviour {
        EventSystem m_EventSystem;

        private bool LocalComponentsInitialized = false;

        public void Awake() {
            GetLocalComponents();
        }

        private void GetLocalComponents() {
            //Debug.Log($"{gameObject.name}.NameplateCanvasController.GetLocalComponents()");
            if (LocalComponentsInitialized) {
                return;
            }
            //Fetch the Event System from the Scene
            m_EventSystem = GetComponent<EventSystem>();

            LocalComponentsInitialized = true;
        }
        
    }

}