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
                SystemGameManager.Instance.SystemPlayableDirectorManager.PlayableDirectorDictionary[playableDirector.playableAsset.name] = playableDirector;
            }
        }


        public void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            /*
            if (SystemGameManager.Instance.SystemPlayableDirectorManager == null || playableDirector == null) {
                return;
            }
            */
            if (SystemGameManager.Instance.SystemPlayableDirectorManager.PlayableDirectorDictionary.ContainsKey(playableDirector.playableAsset.name)) {
                SystemGameManager.Instance.SystemPlayableDirectorManager.PlayableDirectorDictionary.Remove(playableDirector.playableAsset.name);
            }

        }

        /*
        public virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemGameManager.Instance.EventManager.OnPlayerConnectionDespawn += ReloadResourceLists;
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemGameManager.Instance.EventManager.OnPlayerConnectionDespawn -= ReloadResourceLists;
            eventSubscriptionsInitialized = false;
        }
        */



    }

}