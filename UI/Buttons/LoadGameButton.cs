using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadGameButton : HighlightButton {

    [SerializeField]
    private Faction faction;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text playerName;

    [SerializeField]
    private Text description;

    [SerializeField]
    private AnyRPGSaveData mySaveData;

    public AnyRPGSaveData MySaveData { get => mySaveData; set => mySaveData = value; }

    public void AddSaveData(AnyRPGSaveData mySaveData) {
        //Debug.Log("LoadGameButton.AddSaveData()");
        this.mySaveData = mySaveData;

        icon.sprite = null;
        if (mySaveData.playerFaction != null && MySaveData.playerFaction != string.Empty) {
            Faction playerFaction = SystemFactionManager.MyInstance.GetResource(mySaveData.playerFaction);
            // needs to be checked anyway.  could have invalid faction in save data
            if (playerFaction != null) {
                icon.sprite = playerFaction.MyIcon;
            } else {
                icon.sprite = SystemFactionManager.MyInstance.MyDefaultIcon;
            }
        } else {
            icon.sprite = SystemFactionManager.MyInstance.MyDefaultIcon;
        }
        icon.color = Color.white;
        //Debug.Log("LoadGameButton.AddSaveData(): Setting playerName.text: " + mySaveData.playerName);
        //Debug.Log("LoadGameButton.AddSaveData(): Setting DataFileName: " + mySaveData.DataFileName);
        playerName.text = mySaveData.playerName;

        // format the button text
        string descriptionText = string.Empty;
        descriptionText += "Zone: " + mySaveData.CurrentScene + "\n";
        descriptionText += "Level: " + mySaveData.PlayerLevel + "\n";
        descriptionText += "Experience: " + mySaveData.currentExperience + "\n";
        descriptionText += "Faction: " + (mySaveData.playerFaction == string.Empty ? "None" : MySaveData.playerFaction) + "\n";
        descriptionText += "Created: " + mySaveData.DataCreatedOn + "\n";
        descriptionText += "Saved: " + mySaveData.DataSavedOn + "\n";
        descriptionText += "FileName: " + mySaveData.DataFileName + "\n";

        // set the text on the button
        description.text = descriptionText;
    }

    /*
    public void ClearSaveData() {
        icon.sprite = null;
        icon.color = new Color32(0, 0, 0, 0);
        factionName.text = string.Empty;
        description.text = string.Empty;
    }
    */

    public void CommonSelect() {
        if (LoadGamePanel.MyInstance.MySelectedLoadGameButton != null && LoadGamePanel.MyInstance.MySelectedLoadGameButton != this) {
            LoadGamePanel.MyInstance.MySelectedLoadGameButton.DeSelect();
        }
        LoadGamePanel.MyInstance.ShowSavedGame(this);
    }

    public void RawSelect() {
        CommonSelect();
    }

    public override void Select() {
        CommonSelect();
        base.Select();
    }

}
