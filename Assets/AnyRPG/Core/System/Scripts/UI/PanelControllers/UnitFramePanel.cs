using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitFramePanel : UnitFramePanelBase {

        [Header("Unit Frame Panel")]

        [SerializeField]
        protected TextMeshProUGUI unitLevelText = null;

        public override void HandleLevelChanged(int _level) {
            base.HandleLevelChanged(_level);
            if (unitLevelText == null) {
                return;
            }
            unitLevelText.text = _level.ToString();
            if (playerManager.UnitController?.CharacterStats != null) {
                unitLevelText.color = LevelEquations.GetTargetColor(playerManager.UnitController.CharacterStats.Level, _level);
            }

        }

    }

}