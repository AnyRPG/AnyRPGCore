using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogPrerequisite : IPrerequisite {

    [SerializeField]
    private string prerequisiteName;

    public virtual bool IsMet(BaseCharacter baseCharacter) {
        //Debug.Log("QuestPrerequisite.IsMet()");
        Dialog _dialog = SystemDialogManager.MyInstance.GetResource(prerequisiteName);
        if ( _dialog != null) {
            if (_dialog.TurnedIn == true) {
                return true;
            }
        }
        return false;
    }
}
