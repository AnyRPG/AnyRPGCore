using TMPro;
using UnityEngine.UI;

namespace AnyRPG {
    public interface IMinimapUnit {

        event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler;
        bool HasMiniMapText();
        bool HasMiniMapIcon();
        bool SetMiniMapText(TextMeshProUGUI text);
        void SetMiniMapIcon(Image icon);

    }

}