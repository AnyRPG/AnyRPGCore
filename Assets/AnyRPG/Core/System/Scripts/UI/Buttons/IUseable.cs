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
        string ResourceName { get; }
        string DisplayName { get; }
        float CoolDown { get; }
        bool RequireOutOfCombat { get; }
        bool RequireStealth { get; }
        bool AlwaysDisplayCount { get; }
        bool Use(UnitController sourceUnitController);
        bool ActionButtonUse(UnitController sourceUnitController);
        Coroutine ChooseMonitorCoroutine(ActionButton actionButton);
        bool IsUseableStale(UnitController sourceUnitController);
        void UpdateActionButtonVisual(ActionButton actionButton);
        int GetChargeCount();
        IUseable GetFactoryUseable();
        void AssignToActionButton(ActionButton actionButton);
        void HandleRemoveFromActionButton(ActionButton actionButton);
        void UpdateTargetRange(ActionBarManager actionBarManager, ActionButton actionButton);
    }
}