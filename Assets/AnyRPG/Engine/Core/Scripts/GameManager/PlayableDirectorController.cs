using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace AnyRPG {
    public class PlayableDirectorController : MonoBehaviour {

        PlayableDirector playableDirector = null;

        private void Awake() {
            //Debug.Log("SystemGameManager.Awake()");
            playableDirector = GetComponent<PlayableDirector>();
            if (playableDirector != null) {
                //Debug.Log("SystemGameManager.Awake(): playableDirector.playableAsset.name: " + playableDirector.playableAsset.name);
                SystemPlayableDirectorManager.MyInstance.MyPlayableDirectorDictionary[playableDirector.playableAsset.name] = playableDirector;
            }
        }


        public void OnDisable() {
            if (SystemPlayableDirectorManager.MyInstance == null || playableDirector == null) {
                return;
            }
            if (SystemPlayableDirectorManager.MyInstance.MyPlayableDirectorDictionary.ContainsKey(playableDirector.playableAsset.name)) {
                SystemPlayableDirectorManager.MyInstance.MyPlayableDirectorDictionary.Remove(playableDirector.playableAsset.name);
            }

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