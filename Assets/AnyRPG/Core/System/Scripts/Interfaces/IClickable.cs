using TMPro;
using UnityEngine.UI;

namespace AnyRPG {
public interface IClickable {
    Image Icon {
        get;
        set;
    }

    int Count {
        get;
    }

    TextMeshProUGUI StackSizeText {
        get;
    }
}

}