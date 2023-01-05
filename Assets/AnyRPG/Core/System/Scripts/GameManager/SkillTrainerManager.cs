using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SkillTrainerManager : ConfiguredMonoBehaviour {

        public event System.Action OnConfirmAction = delegate { };
        public event System.Action OnEndInteraction = delegate { };

        private SkillTrainerComponent skillTrainer = null;

        // game manager references
        private PlayerManager playerManager = null;

        public SkillTrainerComponent SkillTrainer { get => skillTrainer; set => skillTrainer = value; }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
        }

        public List<Skill> GetAvailableSkillList() {
            List<Skill> returnList = new List<Skill>();

            foreach (Skill skill in skillTrainer.Props.Skills) {
                if (!playerManager.MyCharacter.CharacterSkillManager.HasSkill(skill)) {
                    returnList.Add(skill);
                }
            }

            return returnList;
        }

        public void LearnSkill(Skill skill) {
            playerManager.MyCharacter.CharacterSkillManager.LearnSkill(skill);

            OnConfirmAction();
        }

        public void UnlearnSkill(Skill skill) {
            playerManager.MyCharacter.CharacterSkillManager.UnlearnSkill(skill);
        }

        public bool SkillIsKnown(Skill skill) {
            return playerManager.MyCharacter.CharacterSkillManager.HasSkill(skill);
        }

        public void EndInteraction() {
            OnEndInteraction();
        }

        public void SetSkillTrainer(SkillTrainerComponent skillTrainerComponent) {
            //Debug.Log("ClassChangeManager.SetDisplayClass(" + characterClass + ")");
            this.skillTrainer = skillTrainerComponent;
        }


    }

}