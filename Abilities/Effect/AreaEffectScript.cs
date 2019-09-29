using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaEffectScript : MonoBehaviour {

    [SerializeField]
    private SphereCollider sphereCollider;

    private CharacterUnit source;

    private GameObject target;

    private bool initialized = false;

    private float radius = 0.5f;

    /*
    private void Update() {
    }
    */

    public void Initialize(CharacterUnit source, GameObject target, float radius) {
        Debug.Log("ProjectileScript.Initialize(" + source.name + ", " + target.name + ", " + radius + ")");
        this.source = source;
        this.target = target;
        sphereCollider.radius = radius;
        initialized = true;
    }
    
}
