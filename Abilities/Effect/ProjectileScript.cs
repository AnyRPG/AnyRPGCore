using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour {

    public event System.Action<BaseCharacter, GameObject, GameObject, AbilityEffectOutput> OnCollission = delegate { };

    private BaseCharacter source;

    private GameObject target;

    private Vector3 positionOffset;

    private Vector3 targetPosition;

    private float velocity;

    private bool initialized = false;

    private AbilityEffectOutput abilityEffectInput = null;

    private void Update() {
        MoveTowardTarget();
    }

    public void Initialize(float velocity, BaseCharacter source, GameObject target, Vector3 positionOffset, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log("ProjectileScript.Initialize(" + velocity + ", " + source.name + ", " + target.name + ", " + positionOffset + ")");
        this.source = source;
        this.velocity = velocity;
        this.target = target;
        this.positionOffset = positionOffset;
        this.abilityEffectInput = abilityEffectInput;
        initialized = true;
    }
    
    private void UpdateTargetPosition() {
        //Debug.Log("ProjectileScript.UpdateTargetPosition()");
        if (target != null) {
            targetPosition = new Vector3(target.transform.position.x + positionOffset.x, target.transform.position.y + positionOffset.y, target.transform.position.z + positionOffset.z);
        }
    }

    private void MoveTowardTarget() {
        //Debug.Log("ProjectileScript.MoveTowardTarget()");
        if (initialized) {
            UpdateTargetPosition();
            transform.forward = (targetPosition - transform.position).normalized;
            //Debug.Log("ProjectileScript.MoveTowardTarget(): transform.forward: " + transform.forward);
            transform.position += (transform.forward * (Time.deltaTime * velocity));
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Debug.Log("ProjectileScript.OnTriggerEnter(" + other.name + ")");
        if (other.gameObject == target) {
            OnCollission(source, target, gameObject, abilityEffectInput);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("ProjectileScript.OnCollissionEnter()");
    }
}
