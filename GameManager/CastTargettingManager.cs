using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CastTargettingManager : MonoBehaviour {

    #region Singleton
    private static CastTargettingManager instance;

    public static CastTargettingManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<CastTargettingManager>();
            }
            return instance;
        }
    }

    #endregion

    [SerializeField]
    private CastTargettingController castTargettingController;

    [SerializeField]
    private Vector3 offset;

    private Color circleColor;

    public Color MyCircleColor { get => circleColor; set => circleColor = value; }

    void Start() {
        //Debug.Log("CastTargettingmanager.Start()");
        DisableProjector();
    }

    public void DisableProjector() {
        //Debug.Log("CastTargettingmanager.DisableProjector()");
        castTargettingController.gameObject.SetActive(false);
    }

    public void EnableProjector(Color groundTargetColor) {
        //Debug.Log("CastTargettingmanager.EnableProjector()");
        castTargettingController.gameObject.SetActive(true);
        castTargettingController.SetCircleColor(groundTargetColor);
    }

    /*
    void Update() {
        Debug.Log("CastTargettingmanager);
        icon.transform.position = Input.mousePosition+offset;

    }
    */
}
