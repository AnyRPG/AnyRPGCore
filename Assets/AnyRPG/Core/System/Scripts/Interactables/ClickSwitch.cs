using UnityEngine;

namespace AnyRPG {
    public class ClickSwitch : ControlSwitch {

        [SerializeField]
        private ClickSwitchProps clickSwitchProps = new ClickSwitchProps();

        public override InteractableOptionProps InteractableOptionProps { get => clickSwitchProps; }

    }

}