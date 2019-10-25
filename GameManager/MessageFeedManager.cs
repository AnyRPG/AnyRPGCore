using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
/// <summary>
/// Manages the messages displayed on the screen for quest status updates
/// </summary>
public class MessageFeedManager : MonoBehaviour {

    #region Singleton
    private static MessageFeedManager instance;

    public static MessageFeedManager MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<MessageFeedManager>();
            }

            return instance;
        }
    }

    #endregion

    [SerializeField]
    private GameObject messagePrefab;

    [SerializeField]
    private GameObject messageFeedGameObject;

    public GameObject MessageFeedGameObject { get => messageFeedGameObject; set => messageFeedGameObject = value; }

    public void WriteMessage(string message) {
        //Debug.Log("MessageFeedManager.WriteMessage(" + message + ")");
        if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
            return;
        }
        GameObject go = Instantiate(messagePrefab, messageFeedGameObject.transform);
        go.GetComponent<Text>().text = message;
        //uncomment the next line to make the messages spawn at the top instead of the bottom
        //go.transform.SetAsFirstSibling();
        Destroy(go, 2);
        if (CombatLogUI.MyInstance != null) {
            CombatLogUI.MyInstance.WriteSystemMessage(message);
        } else {
            //Debug.Log("CombatLogUI.Myinstance was null!!");
        }
    }

    public void LockUI() {
        MessageFeedGameObject.GetComponent<DraggableWindow>().LockUI();
    }
}

}