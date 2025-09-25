using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterDialogManager : ConfiguredClass {

        protected UnitController unitController;


        public CharacterDialogManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        /// <summary>
        /// Set the shown value to false for all dialog Nodes and reset the turned in status
        /// </summary>
        public void ResetDialogStatus(Dialog dialog) {
            if (dialog.Repeatable == false) {
                return;
            }
            unitController.CharacterSaveManager.SetDialogTurnedIn(dialog, false);
            unitController.CharacterSaveManager.ResetDialogNodes(dialog);
        }

        public void TurnInDialog(Dialog dialog) {
            //Debug.Log($"CharacterDialogManager.TurnInDialog({dialog.ResourceName})");

            unitController.CharacterSaveManager.SetDialogTurnedIn(dialog, true);
            unitController.UnitEventController.NotifyOnDialogCompleted(dialog);
            dialog.NotifyOnDialogCompleted(unitController);
        }


    }

}