using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionChangePanelController : WindowContentController {

    public event System.Action OnConfirmAction = delegate { };
    public override event Action<ICloseableWindowContents> OnCloseWindowHandler = delegate { };

    [SerializeField]
    private GameObject rewardIconPrefab;

    [SerializeField]
    private FactionButton factionButton;

    [SerializeField]
    private GameObject abilitiesArea;

    [SerializeField]
    private GameObject abilityIconsArea;

    private List<RewardButton> abilityRewardIcons = new List<RewardButton>();

    private string factionName;

    public void Setup(string newFactionName) {
        //Debug.Log("FactionChangePanelController.Setup(" + newFactionName + ")");
        factionName = newFactionName;
        factionButton.AddFaction(factionName);
        PopupWindowManager.MyInstance.factionChangeWindow.SetWindowTitle(factionName);
        ShowAbilityRewards();
        PopupWindowManager.MyInstance.factionChangeWindow.OpenWindow();
    }

    public void ShowAbilityRewards() {
        //Debug.Log("FactionChangePanelController.ShowAbilityRewards()");

        ClearRewardIcons();
        // show ability rewards
        Faction faction = SystemFactionManager.MyInstance.GetResource(factionName);
        if (faction.MyLearnedAbilityList.Count > 0) {
            abilitiesArea.gameObject.SetActive(true);
        } else {
            abilitiesArea.gameObject.SetActive(false);
        }
        for (int i = 0; i < faction.MyLearnedAbilityList.Count; i++) {
            RewardButton rewardIcon = Instantiate(rewardIconPrefab, abilityIconsArea.transform).GetComponent<RewardButton>();
            rewardIcon.SetDescribable(SystemAbilityManager.MyInstance.GetResource(faction.MyLearnedAbilityList[i]));
            abilityRewardIcons.Add(rewardIcon);
            if (SystemAbilityManager.MyInstance.GetResource(faction.MyLearnedAbilityList[i]).MyRequiredLevel > PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel) {
                rewardIcon.MyStackSizeText.text = "Level\n" + SystemAbilityManager.MyInstance.GetResource(faction.MyLearnedAbilityList[i]).MyRequiredLevel;
                rewardIcon.MyHighlightIcon.color = new Color32(255, 255, 255, 80);
            }
        }
    }

    private void ClearRewardIcons() {
        //Debug.Log("FactionChangePanelController.ClearRewardIcons()");

        foreach (RewardButton rewardIcon in abilityRewardIcons) {
            Destroy(rewardIcon.gameObject);
        }
        abilityRewardIcons.Clear();
    }

    public void CancelAction() {
        //Debug.Log("FactionChangePanelController.CancelAction()");
        PopupWindowManager.MyInstance.factionChangeWindow.CloseWindow();
    }

    public void ConfirmAction() {
        //Debug.Log("FactionChangePanelController.ConfirmAction()");
        PlayerManager.MyInstance.SetPlayerFaction(factionName);
        OnConfirmAction();
        PopupWindowManager.MyInstance.factionChangeWindow.CloseWindow();
    }

    public override void OnOpenWindow() {
        //Debug.Log("FactionChangePanelController.OnOpenWindow()");
        base.OnOpenWindow();
    }

    public override void OnCloseWindow() {
        //Debug.Log("FactionChangePanelController.OnCloseWindow()");
        base.OnCloseWindow();
        OnCloseWindowHandler(this);
    }
}
