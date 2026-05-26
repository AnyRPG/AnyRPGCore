using UnityEngine;

namespace AnyRPG {
[ExecuteInEditMode]
public class ShowRealEulerAngles : MonoBehaviour {


    [SerializeField]
    float eulerAngX;
    [SerializeField]
    float eulerAngY;
    [SerializeField]
    float eulerAngZ;


    void Update() {

        eulerAngX = transform.localEulerAngles.x;
        eulerAngY = transform.localEulerAngles.y;
        eulerAngZ = transform.localEulerAngles.z;

    }
}

}