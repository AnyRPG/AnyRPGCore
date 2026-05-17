using UnityEngine;

namespace AnyRPG {
    public interface IContextMenuTarget {

        void SetupContextMenu(ContextMenuPanel contextMenuPanel);
        void PerformContextMenuAction(string actionName);
    }

}