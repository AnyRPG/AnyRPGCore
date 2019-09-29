using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTrainerUI : WindowContentController {

    #region Singleton
    private static SkillTrainerUI instance;

    public static SkillTrainerUI MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<SkillTrainerUI>();
            }

            return instance;
        }
    }

    #endregion

    private SkillTrainer skillTrainer;

    [SerializeField]
    private GameObject learnButton, unlearnButton;

    [SerializeField]
    private GameObject skillPrefab;

    [SerializeField]
    private Transform skillParent;

    [SerializeField]
    private Text skillDescription;

    [SerializeField]
    private GameObject availableHeading;

    [SerializeField]
    private GameObject availableArea;

    [SerializeField]
    private GameObject learnedHeading;

    [SerializeField]
    private GameObject learnedArea;

    [SerializeField]
    private DescribableIcon[] rewardButtons;

    //private List<GameObject> Skills = new List<GameObject>();
    private List<Skill> skills = new List<Skill>();

    private List<SkillTrainerSkillScript> skillScripts = new List<SkillTrainerSkillScript>();

    private SkillTrainerSkillScript selectedSkillTrainerSkillScript;

    private string currentSkillName = null;

    public override event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };

    public SkillTrainerSkillScript MySelectedSkillTrainerSkillScript { get => selectedSkillTrainerSkillScript; set => selectedSkillTrainerSkillScript = value; }

    private void Start() {
        DeactivateButtons();
    }

    public void DeactivateButtons() {
        learnButton.GetComponent<Button>().enabled = false;
        unlearnButton.GetComponent<Button>().enabled = false;
    }

    public void ShowSkillsCommon(SkillTrainer skillTrainer) {
        Debug.Log("SkillTrainerUI.ShowSkillsCommon(" + skillTrainer.name + ")");

        ClearSkills();

        SkillTrainerSkillScript firstAvailableSkill = null;

        foreach (Skill skill in skillTrainer.MySkills) {
            if (!PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(skill.MyName)) {
                GameObject go = Instantiate(skillPrefab, availableArea.transform);
                SkillTrainerSkillScript qs = go.GetComponent<SkillTrainerSkillScript>();
                qs.MyText.text = skill.MyName;
                qs.MyText.color = Color.white;
                qs.SetSkillName(skill.MyName);
                skillScripts.Add(qs);
                skills.Add(skill);
                if (firstAvailableSkill == null) {
                    firstAvailableSkill = qs;
                }
            }
        }

        if (firstAvailableSkill == null) {
            // no available skills anymore, close window
            PopupWindowManager.MyInstance.skillTrainerWindow.CloseWindow();
        }

        if (MySelectedSkillTrainerSkillScript == null && firstAvailableSkill != null) {
            firstAvailableSkill.Select();
        }
    }


    public void ShowSkills() {
        Debug.Log("SkillTrainerUI.ShowSkills()");
        ShowSkillsCommon(skillTrainer);
    }

    public void ShowSkills(SkillTrainer skillTrainer) {
        Debug.Log("SkillTrainerUI.ShowSkills(" + skillTrainer.name + ")");
        this.skillTrainer = skillTrainer;
        ShowSkillsCommon(this.skillTrainer);
    }

    public void UpdateSelected() {
        //Debug.Log("SkillTrainerUI.UpdateSelected()");
        if (selectedSkillTrainerSkillScript != null) {
            ShowDescription(selectedSkillTrainerSkillScript.MySkillName);
        }
    }

    // Enable or disable learn and unlearn buttons based on what is selected
    private void UpdateButtons(string skillName) {
        Debug.Log("SkillTrainerUI.UpdateButtons(" + skillName + ")");
        if (PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(skillName)) {
            learnButton.gameObject.SetActive(false);
            learnButton.GetComponent<Button>().enabled = false;
            unlearnButton.gameObject.SetActive(true);
            unlearnButton.GetComponent<Button>().enabled = true;
        } else {
            learnButton.gameObject.SetActive(true);
            learnButton.GetComponent<Button>().enabled = true;
            unlearnButton.GetComponent<Button>().enabled = false;
            unlearnButton.gameObject.SetActive(false);
        }
    }

    private void ClearRewardButtons() {
        //Debug.Log("SkillTrainerUI.ClearRewardButtons()");
        foreach (DescribableIcon rewardButton in rewardButtons) {
            rewardButton.gameObject.SetActive(false);
        }
    }

    public void ShowDescription(string skillName) {
        Debug.Log("SkillTrainerUI.ShowDescription(" + skillName + ")");
        ClearDescription();

        if (skillName == null && skillName == string.Empty) {
            return;
        }
        currentSkillName = skillName;

        Skill skill = SystemSkillManager.MyInstance.GetResource(skillName);
        if (skill == null) {
            Debug.Log("SkillTrainerUI.ShowDescription(" + skillName + "): failed to get skill from SystemSkillManager");
        }
        UpdateButtons(skillName);


        skillDescription.text = skill.GetDescription();
        skillDescription.text += "\n\n<b>Abilities Learned:</b>\n\n";

        // show abilities learned
        for (int i = 0; i < skill.MyAbilityList.Count; i++) {
            rewardButtons[i].gameObject.SetActive(true);
            rewardButtons[i].SetDescribable(skill.MyAbilityList[i]);
        }
    }

    public void ClearDescription() {
        Debug.Log("SkillTrainerUI.ClearDescription()");
        skillDescription.text = string.Empty;
        ClearRewardButtons();
        DeselectSkillScripts();
    }

    public void DeselectSkillScripts() {
        Debug.Log("SkillTrainerUI.DeselectSkillScripts()");
        foreach (SkillTrainerSkillScript skill in skillScripts) {
            if (skill != MySelectedSkillTrainerSkillScript) {
                skill.DeSelect();
            }
        }
    }

    public void ClearSkills() {
        //Debug.Log("SkillTrainerUI.ClearSkills()");
        // clear the skill list so any skill left over from a previous time opening the window aren't shown
        foreach (SkillTrainerSkillScript skill in skillScripts) {
            if (skill != null) {
                skill.gameObject.transform.SetParent(null);
                Destroy(skill.gameObject);
            }
        }
        skillScripts.Clear();
    }

    public override void OnCloseWindow() {
        //Debug.Log("SkillTrainerUI.OnCloseWindow()");
        base.OnCloseWindow();
        DeactivateButtons();
        MySelectedSkillTrainerSkillScript = null;
    }

    public void LearnSkill() {
        Debug.Log("SkillTrainerUI.LearnSkill()");
        if (currentSkillName != null && currentSkillName != string.Empty) {
            //if (MySelectedSkillTrainerSkillScript != null && MySelectedSkillTrainerSkillScript.MySkillName != null) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.LearnSkill(MySelectedSkillTrainerSkillScript.MySkillName);
            //UpdateButtons(MySelectedSkillTrainerSkillScript.MySkillName);
            MySelectedSkillTrainerSkillScript = null;
            ClearDescription();
            ShowSkills();
        }
    }

    public void UnlearnSkill() {
        //Debug.Log("SkillTrainerUI.UnlearnSkill()");
        if (MySelectedSkillTrainerSkillScript != null && MySelectedSkillTrainerSkillScript.MySkillName != null) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.UnlearnSkill(MySelectedSkillTrainerSkillScript.MySkillName);
            UpdateButtons(MySelectedSkillTrainerSkillScript.MySkillName);
            ShowSkills();
        }
    }

    public override void OnOpenWindow() {
        //Debug.Log("SkillTrainerUI.OnOpenWindow()");
        // clear before open window handler, because it shows quests
        ClearDescription();

        OnOpenWindowHandler(this);
    }
}
