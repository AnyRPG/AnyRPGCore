using UnityEngine;

namespace AnyRPG {
    public interface IDescribable {
        Sprite Icon { get; }
        string ResourceName { get; }
        string DisplayName { get; }
        string Description { get; }
        string GetSummary();
        string GetDescription();
        void ProcessShowTooltip(TooltipController tooltipController);
    }

}