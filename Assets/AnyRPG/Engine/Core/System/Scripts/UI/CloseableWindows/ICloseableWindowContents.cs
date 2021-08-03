using AnyRPG;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ICloseableWindowContents {
        //event System.Action<ICloseableWindowContents> OnOpenWindow;
        event System.Action<ICloseableWindowContents> OnCloseWindow;
        void Init(SystemGameManager systemGameManager);
        void RecieveClosedWindowNotification();
        void ReceiveOpenWindowNotification();
        void SetBackGroundColor(Color color);
        GameObject gameObject { get; }
        Image BackGroundImage { get; }
    }
}