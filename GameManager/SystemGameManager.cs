using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemGameManager : MonoBehaviour {

        #region Singleton
        private static SystemGameManager instance;

        public static SystemGameManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemGameManager>();
                }

                return instance;
            }
        }
        #endregion

        private int spawnCount = 0;

        private void Awake() {
            //Debug.Log("SystemGameManager.Awake()");
        }

        private void Start() {
            //Debug.Log("SystemGameManager.Start()");

            // we are going to handle the initialization of all system managers here so we can control the start order and it isn't random

            // first turn off the UI
            UIManager.MyInstance.PerformSetupActivities();

            // next, load scriptable object resources
            LoadResources();

            PlayerManager.MyInstance.OrchestratorStart();

            // then launch level manager to start loading the game
            LevelManager.MyInstance.PerformSetupActivities();

        }

        public void LoadResources() {
            //Debug.Log("Loading ScriptableObject Resources From Disk");
        }

        public int GetSpawnCount() {
            spawnCount++;
            return spawnCount;
        }

    }

}