using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestNode {

    [SerializeField]
    private bool startQuest = true;

    [SerializeField]
    private bool endQuest = true;

    [SerializeField]
    private Quest questTemplate;

    private GameObject questObject;

    public bool MyStartQuest { get => startQuest; set => startQuest = value; }
    public bool MyEndQuest { get => endQuest; set => endQuest = value; }
    public Quest MyQuest { get => questTemplate; set => questTemplate = value; }
    public GameObject MyGameObject { get => questObject; set => questObject = value; }

    // TESTING
    /*
    public void Awake() {
    }
    */
}
