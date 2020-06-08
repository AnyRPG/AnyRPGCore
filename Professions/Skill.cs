using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Skill", menuName = "AnyRPG/Skills/Skill")]
    public class Skill : DescribableResource {

        [SerializeField]
        private int requiredLevel = 1;

        [SerializeField]
        private bool autoLearn = false;

        [SerializeField]
        private List<string> abilityNames = new List<string>();

        private List<BaseAbility> abilityList = new List<BaseAbility>();

        public int MyRequiredLevel { get => requiredLevel; }
        public bool MyAutoLearn { get => autoLearn; }
        public List<BaseAbility> MyAbilityList { get => abilityList; set => abilityList = value; }

        public override string GetDescription() {
            return string.Format("<color=#ffff00ff>{0}</color>\n\n{1}", resourceName, GetSummary());
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            abilityList = new List<BaseAbility>();
            if (abilityNames != null) {
                foreach (string abilityName in abilityNames) {
                    BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
                    if (baseAbility != null) {
                        abilityList.Add(baseAbility);
                    } else {
                        Debug.LogError("SystemSkillManager.SetupScriptableObjects(): Could not find ability : " + abilityName + " while inititalizing " + MyDisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
        }

    }
}