using System.Collections.Generic;

namespace AnyRPG {
    public interface IStatProvider {

        List<StatScalingNode> PrimaryStats { get; set; }
        List<PowerResource> PowerResourceList { get; set; }

    }

}