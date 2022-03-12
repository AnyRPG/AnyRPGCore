using AnyRPG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IUseable {
        Sprite Icon { get; }
        string DisplayName { get; }
        float CoolDown { get; }
        bool RequireOutOfCombat { get; }
        bool Use();
        bool ActionButtonUse();
        Coroutine ChooseMonitorCoroutine(ActionButton actionButton);
        bool IsUseableStale();
        void UpdateActionButtonVisual(ActionButton actionButton);
        void UpdateChargeCount(ActionButton actionButton);
        IUseable GetFactoryUseable();
        void AssignToActionButton(ActionButton actionButton);
        void UpdateTargetRange(ActionBarManager actionBarManager, ActionButton actionButton);
    }
}