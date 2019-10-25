using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[System.Serializable]
public class QuestPrerequisite : IPrerequisite {

    [SerializeField]
    private string prerequisiteName;

    // does the quest need to be complete, or just in progress for this prerequisite to be met
    [SerializeField]
    private bool requireComplete = true;

    [SerializeField]
    private bool requireTurnedIn = true;

    public virtual bool IsMet(BaseCharacter baseCharacter) {
        //Debug.Log("QuestPrerequisite.IsMet()");
        if (prerequisiteName == null || prerequisiteName == string.Empty) {
            Debug.Log("QuestPrerequisite.IsMet(): PREREQUISITE IS NULL!  FIX THIS!  DO NOT COMMENT THIS LINE");
            return false;
        }
        Quest _quest = SystemQuestManager.MyInstance.GetResource(prerequisiteName);
        if ( _quest != null) {
            if (requireTurnedIn && _quest.TurnedIn == true) {
                return true;
            }
            if (!requireTurnedIn && requireComplete && _quest.IsComplete && QuestLog.MyInstance.HasQuest(_quest.MyName)) {
                return true;
            }
            if (!requireTurnedIn && !requireComplete && QuestLog.MyInstance.HasQuest(_quest.MyName)) {
                return true;
            }
        }
        return false;
    }
}

}