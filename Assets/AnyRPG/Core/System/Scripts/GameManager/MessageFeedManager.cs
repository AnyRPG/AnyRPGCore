using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    /// <summary>
    /// Manages the messages displayed on the screen for quest status updates
    /// </summary>
    public class MessageFeedManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private GameObject messagePrefab = null;

        [SerializeField]
        private CloseableWindow messageFeedWindow = null;

        [SerializeField]
        private GraphicRaycaster raycaster = null;

        // GameManager references
        private ObjectPooler objectPooler = null;
        private LogManager logManager = null;

        public CloseableWindow MessageFeedWindow { get => messageFeedWindow; set => messageFeedWindow = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            messageFeedWindow.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            logManager = systemGameManager.LogManager;
        }

        public void WriteMessage(string message) {
            //Debug.Log("MessageFeedManager.WriteMessage(" + message + ")");
            if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
                return;
            }
            GameObject go = objectPooler.GetPooledObject(messagePrefab, messageFeedWindow.transform);
            go.GetComponent<TextMeshProUGUI>().text = message;
            //uncomment the next line to make the messages spawn at the top instead of the bottom
            //go.transform.SetAsFirstSibling();
            objectPooler.ReturnObjectToPool(go, 2);
            if (logManager != null) {
                logManager.WriteSystemMessage(message);
            } else {
                //Debug.Log("CombatLogUI.Myinstance was null!!");
            }
        }
        
        public void LockUI() {
            if (PlayerPrefs.HasKey("LockUI")) {
                if (PlayerPrefs.GetInt("LockUI") == 0) {
                    raycaster.enabled = true;
                } else {
                    raycaster.enabled = false;
                }
            }
            MessageFeedWindow.LockUI();
        }
    }

}