using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    
    [CreateAssetMenu(fileName = "New Achievement", menuName = "AnyRPG/Achievement")]
    public class Achievement : QuestBase {

        protected override void ProcessMarkComplete(UnitController sourceUnitController, bool printMessages) {
            base.ProcessMarkComplete(sourceUnitController, printMessages);
            if (printMessages == true) {
                sourceUnitController.WriteMessageFeedMessage(string.Format("Achievement: {0} Complete!", DisplayName));
            }

            //sourceUnitController.CharacterQuestLog.ProcessMarkAchievementComplete();
            SetTurnedIn(sourceUnitController, true);
        }

        protected override QuestSaveData GetSaveData(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.Achievement.GetSaveData({sourceUnitController.gameObject.name})");

            return sourceUnitController.CharacterQuestLog.GetAchievementSaveData(this);
        }

        protected override void SetSaveData(UnitController sourceUnitController, string QuestName, QuestSaveData questSaveData) {
            sourceUnitController.CharacterQuestLog.SetAchievementSaveData(ResourceName, questSaveData);
        }

        protected override bool HasQuest(UnitController sourceUnitController) {
            return sourceUnitController.CharacterQuestLog.HasAchievement(ResourceName);
        }

        public override int GetObjectiveCurrentAmount(UnitController sourceUnitController, string objectiveTypeName, string objectiveName) {
            return sourceUnitController.CharacterQuestLog.GetAchievementObjectiveSaveData(ResourceName, objectiveTypeName, objectiveName).Amount;
        }

        public override void SetObjectiveCurrentAmount(UnitController sourceUnitController, string objectiveTypeName, string objectiveName, int amount) {
            sourceUnitController.CharacterQuestLog.SetAchievementObjectiveCurrentAmount(ResourceName, objectiveTypeName, objectiveName, amount);
        }

        public override void ResetObjectiveSaveData(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.Achievement.ResetObjectiveSaveData({sourceUnitController.gameObject.name})");
            sourceUnitController.CharacterQuestLog.ResetAchievementObjectiveSaveData(ResourceName);
        }

        public override void NotifyOnObjectiveStatusUpdated(UnitController sourceUnitController) {
            sourceUnitController.UnitEventController.NotifyOnAchievementObjectiveStatusUpdated(this);
        }

        public override void NotifyOnMarkComplete(UnitController sourceUnitController) {
            sourceUnitController.UnitEventController.NotifyOnMarkAchievementComplete(this);
        }

        public override void NotifyOnAcceptQuest(UnitController sourceUnitController) {
            sourceUnitController.UnitEventController.NotifyOnAcceptAchievement(this);
        }

        public override void RemoveQuest(UnitController sourceUnitController, bool resetQuestStep = true) {
            base.RemoveQuest(sourceUnitController, resetQuestStep);

            base.NotifyOnQuestBaseStatusUpdated(sourceUnitController);
        }

        public override void SetCurrentStep(UnitController sourceUnitController, int value) {
            base.SetCurrentStep(sourceUnitController, value);
            sourceUnitController.UnitEventController.NotifyOnAchievementObjectiveStatusUpdated(this);
        }

    }

}