using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SkillTrainerUI : WindowContentController {

        [SerializeField]
        private HighlightButton learnButton = null;

        [SerializeField]
        private HighlightButton unlearnButton = null;

        [SerializeField]
        private GameObject skillPrefab = null;

        [SerializeField]
        private TextMeshProUGUI skillDescription = null;

        [SerializeField]
        private GameObject availableArea = null;

        [SerializeField]
        private List<DescribableIcon> rewardButtons = new List<DescribableIcon>();

        private List<Skill> skills = new List<Skill>();

        private List<SkillTrainerSkillScript> skillScripts = new List<SkillTrainerSkillScript>();

        private SkillTrainerSkillScript selectedSkillTrainerSkillScript;

        private Skill currentSkill = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private UIManager uIManager = null;
        private SkillTrainerManager skillTrainerManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            learnButton.Configure(systemGameManager);
            unlearnButton.Configure(systemGameManager);

            foreach (DescribableIcon describableIcon in rewardButtons) {
                describableIcon.SetToolTipTransform(rectTransform);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            uIManager = systemGameManager.UIManager;
            skillTrainerManager = systemGameManager.SkillTrainerManager;
        }

        public void SetSelectedButton(SkillTrainerSkillScript selectedSkillTrainerSkillScript) {
            this.selectedSkillTrainerSkillScript = selectedSkillTrainerSkillScript;
            ShowDescription(selectedSkillTrainerSkillScript.Skill);
        }

        public void DeactivateButtons() {
            learnButton.Button.enabled = false;
            unlearnButton.Button.enabled = false;
        }

        public void ShowSkills() {
            //Debug.Log("SkillTrainerUI.ShowSkills()");

            ClearSkills();

            SkillTrainerSkillScript firstAvailableSkill = null;

            foreach (Skill skill in skillTrainerManager.GetAvailableSkillList()) {
                GameObject go = objectPooler.GetPooledObject(skillPrefab, availableArea.transform);
                SkillTrainerSkillScript qs = go.GetComponent<SkillTrainerSkillScript>();
                qs.Configure(systemGameManager);
                qs.Text.text = skill.DisplayName;
                qs.Text.color = Color.white;
                qs.SetSkill(this, skill);
                skillScripts.Add(qs);
                skills.Add(skill);
                uINavigationControllers[0].AddActiveButton(qs);
                if (firstAvailableSkill == null) {
                    firstAvailableSkill = qs;
                }
            }

            if (firstAvailableSkill == null) {
                // no available skills anymore, close window
                uIManager.skillTrainerWindow.CloseWindow();
            }

            if (selectedSkillTrainerSkillScript == null && firstAvailableSkill != null) {
                //firstAvailableSkill.Select();
                uINavigationControllers[0].FocusFirstButton();
            }
            SetNavigationController(uINavigationControllers[0]);
        }

        public void UpdateSelected() {
            //Debug.Log("SkillTrainerUI.UpdateSelected()");
            if (selectedSkillTrainerSkillScript != null) {
                ShowDescription(selectedSkillTrainerSkillScript.Skill);
            }
        }

        // Enable or disable learn and unlearn buttons based on what is selected
        private void UpdateButtons(Skill newSkill) {
            //Debug.Log("SkillTrainerUI.UpdateButtons(" + skillName + ")");
            if (skillTrainerManager.SkillIsKnown(newSkill)) {
                learnButton.gameObject.SetActive(false);
                learnButton.Button.enabled = false;
                unlearnButton.gameObject.SetActive(true);
                unlearnButton.Button.enabled = true;
            } else {
                learnButton.gameObject.SetActive(true);
                learnButton.Button.enabled = true;
                unlearnButton.Button.enabled = false;
                unlearnButton.gameObject.SetActive(false);
            }
        }

        private void ClearRewardButtons() {
            //Debug.Log("SkillTrainerUI.ClearRewardButtons()");
            foreach (DescribableIcon rewardButton in rewardButtons) {
                rewardButton.gameObject.SetActive(false);
            }
        }

        public void ShowDescription(Skill describeSkill) {
            //Debug.Log("SkillTrainerUI.ShowDescription(" + skillName + ")");
            ClearDescription();

            if (describeSkill == null) {
                return;
            }
            currentSkill = describeSkill;

            UpdateButtons(describeSkill);


            skillDescription.text = string.Format("<size=30><b><color=yellow>{0}</color></b></size>\n\n<size=18>{1}</size>", currentSkill.DisplayName, currentSkill.Description);

            skillDescription.text += "\n\n<size=20><b>Abilities Learned:</b></size>\n\n";

            // show abilities learned
            for (int i = 0; i < currentSkill.AbilityList.Count; i++) {
                rewardButtons[i].gameObject.SetActive(true);
                rewardButtons[i].SetDescribable(currentSkill.AbilityList[i]);
            }
        }

        public void ClearDescription() {
            //Debug.Log("SkillTrainerUI.ClearDescription()");
            skillDescription.text = string.Empty;
            ClearRewardButtons();
            DeselectSkillScripts();
        }

        public void DeselectSkillScripts() {
            //Debug.Log("SkillTrainerUI.DeselectSkillScripts()");
            foreach (SkillTrainerSkillScript skillTrainerSkillScript in skillScripts) {
                if (skillTrainerSkillScript != selectedSkillTrainerSkillScript) {
                    skillTrainerSkillScript.DeSelect();
                }
            }
            uINavigationControllers[0].UnHightlightButtonBackgrounds(selectedSkillTrainerSkillScript);

        }

        public void ClearSkills() {
            //Debug.Log("SkillTrainerUI.ClearSkills()");
            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (SkillTrainerSkillScript skill in skillScripts) {
                if (skill != null) {
                    skill.gameObject.transform.SetParent(null);
                    skill.DeSelect();
                    objectPooler.ReturnObjectToPool(skill.gameObject);
                }
            }
            skillScripts.Clear();
            uINavigationControllers[0].ClearActiveButtons();
        }

       

        public void LearnSkill() {
            //Debug.Log("SkillTrainerUI.LearnSkill()");
            if (currentSkill != null) {
                skillTrainerManager.LearnSkill(selectedSkillTrainerSkillScript.Skill);
                selectedSkillTrainerSkillScript = null;
                ClearDescription();
                ShowSkills();
            }
        }

        public void UnlearnSkill() {
            //Debug.Log("SkillTrainerUI.UnlearnSkill()");
            if (selectedSkillTrainerSkillScript != null && selectedSkillTrainerSkillScript.Skill != null) {
                skillTrainerManager.UnlearnSkill(selectedSkillTrainerSkillScript.Skill);
                UpdateButtons(selectedSkillTrainerSkillScript.Skill);
                ShowSkills();
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("SkillTrainerUI.OnOpenWindow()");
            base.ProcessOpenWindowNotification();

            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // reset button state from last window open
            DeactivateButtons();

            // clear description from last window open
            ClearDescription();

            ShowSkills();

        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("SkillTrainerUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            DeactivateButtons();
            selectedSkillTrainerSkillScript = null;
            skillTrainerManager.EndInteraction();
        }
    }

}