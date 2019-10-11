using UnityEngine;
using UnityEngine.UI;

public interface ICloseableWindowContents {
    event System.Action<ICloseableWindowContents> OnOpenWindow;
    event System.Action<ICloseableWindowContents> OnCloseWindow;
    void RecieveClosedWindowNotification();
    void ReceiveOpenWindowNotification();
    void SetBackGroundColor(Color color);

    Image MyBackGroundImage { get; }
}