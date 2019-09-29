using UnityEngine;
using UnityEngine.UI;

public interface ICloseableWindowContents {
    event System.Action<ICloseableWindowContents> OnOpenWindowHandler;
    event System.Action<ICloseableWindowContents> OnCloseWindowHandler;
    void OnCloseWindow();
    void OnOpenWindow();
    void SetBackGroundColor(Color color);

    Image MyBackGroundImage { get; }
}