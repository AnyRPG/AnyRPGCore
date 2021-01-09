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
        bool Use();
        bool ActionButtonUse();
        Coroutine ChooseMonitorCoroutine(ActionButton actionButton);
        bool IsUseableStale(ActionButton actionButton);
        IUseable GetFactoryUseable();
    }
}