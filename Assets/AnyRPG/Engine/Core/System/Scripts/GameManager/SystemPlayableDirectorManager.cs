using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace AnyRPG {
    public class SystemPlayableDirectorManager : MonoBehaviour {

        #region Singleton
        private static SystemPlayableDirectorManager instance;

        public static SystemPlayableDirectorManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemPlayableDirectorManager>();
                }

                return instance;
            }
        }

        #endregion

        // key is the timeline name as a string
        private Dictionary<string, PlayableDirector> playableDirectorDictionary = new Dictionary<string, PlayableDirector>();

        public Dictionary<string, PlayableDirector> MyPlayableDirectorDictionary { get => playableDirectorDictionary; set => playableDirectorDictionary = value; }

        private void Awake() {
            //Debug.Log("SystemGameManager.Awake()");
        }

        private void Start() {

        }

        /*
        public virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn += ReloadResourceLists;
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= ReloadResourceLists;
            eventSubscriptionsInitialized = false;
        }
        */

       

    }

}