using AnyRPG;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public interface ICloseableWindowContents {
        //event System.Action<ICloseableWindowContents> OnOpenWindow;
        event System.Action<ICloseableWindowContents> OnCloseWindow;
        void Configure(SystemGameManager systemGameManager);
        void ReceiveClosedWindowNotification();
        void ReceiveOpenWindowNotification();
        void SetBackGroundColor(Color color);
        void SetWindow(CloseableWindow closeableWindow);
        void Close();
        void Init();
        void LeftAnalog(float inputHorizontal, float inputVertical);
        GameObject gameObject { get; }
        Image BackGroundImage { get; }
        //bool Closeable { get; }
    }
}