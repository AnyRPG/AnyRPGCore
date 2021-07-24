using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ProjectileScript : MonoBehaviour {

        public event System.Action<IAbilityCaster, Interactable, GameObject, AbilityEffectContext, ProjectileScript> OnCollission = delegate { };

        [SerializeField]
        private AudioSource audioSource = null;

        private IAbilityCaster source = null;

        private Interactable target = null;

        private Vector3 positionOffset = Vector3.zero;

        private Vector3 targetPosition = Vector3.zero;

        private float velocity = 0f;

        private bool initialized = false;

        private AbilityEffectContext abilityEffectContext = null;

        private void Update() {
            MoveTowardTarget();
        }

        public void Initialize(float velocity, IAbilityCaster source, Interactable target, Vector3 positionOffset, AbilityEffectContext abilityEffectContext) {
            Debug.Log("ProjectileScript.Initialize(" + velocity + ", " + source.name + ", " + (target == null ? "null" : target.name) + ", " + positionOffset + ")");
            this.source = source;
            this.velocity = velocity;
            this.target = target;
            this.positionOffset = positionOffset;
            this.abilityEffectContext = abilityEffectContext;
            initialized = true;
        }

        public void PlayFlightAudio(List<AudioProfile> audioProfiles, bool randomAudioProfiles = false) {
            List<AudioProfile> usedAudioProfiles = new List<AudioProfile>();
            if (audioSource != null && audioProfiles != null && audioProfiles.Count > 0) {
                audioSource.enabled = true;
                if (randomAudioProfiles == true) {
                    usedAudioProfiles.Add(audioProfiles[UnityEngine.Random.Range(0, audioProfiles.Count)]);
                } else {
                    usedAudioProfiles = audioProfiles;
                }
                foreach (AudioProfile audioProfile in usedAudioProfiles) {
                    if (audioProfile.AudioClip != null) {
                        //Debug.Log(MyName + ".AbilityEffect.PerformAbilityHit(): playing audio clip: " + audioProfile.MyAudioClip.name);
                        audioSource.PlayOneShot(audioProfile.AudioClip);
                    }
                }
            }
        }

        private void UpdateTargetPosition() {
            //Debug.Log("ProjectileScript.UpdateTargetPosition()");
            if (target != null) {
                if (target.CharacterUnit != null) {
                    targetPosition = new Vector3(target.InteractableGameObject.transform.position.x + positionOffset.x, target.InteractableGameObject.transform.position.y + positionOffset.y, target.InteractableGameObject.transform.position.z + positionOffset.z);
                } else {
                    targetPosition = new Vector3(target.transform.position.x + positionOffset.x, target.transform.position.y + positionOffset.y, target.transform.position.z + positionOffset.z);
                }
            }
        }

        private void MoveTowardTarget() {
            //Debug.Log("ProjectileScript.MoveTowardTarget()");
            if (initialized) {
                UpdateTargetPosition();
                if (target != null) {
                    transform.forward = (targetPosition - transform.position).normalized;
                } else {
                    //transform.forward = Vector3.down;
                }

                //Debug.Log("ProjectileScript.MoveTowardTarget(): transform.forward: " + transform.forward);
                transform.position += (transform.forward * (Time.deltaTime * velocity));
            }
        }

        private void OnTriggerEnter(Collider other) {
            //Debug.Log(gameObject.name + ".ProjectileScript.OnTriggerEnter(" + other.name + ")");
            if (!initialized) {
                // could potentially respawn from pool on top of old target
                return;
            }
            if ((target != null && other.gameObject == target.InteractableGameObject) || target == null) {
                if (abilityEffectContext != null && abilityEffectContext.groundTargetLocation != null) {
                    abilityEffectContext.groundTargetLocation = transform.position;
                }
                OnCollission(source, target, gameObject, abilityEffectContext, this);
            }
        }

        private void OnDisable() {
            //Debug.Log(gameObject.name + " " + gameObject.GetInstanceID() + ".ProjectileScript.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            source = null;
            target = null;
            positionOffset = Vector3.zero;
            targetPosition = Vector3.zero;
            velocity = 0f;
            initialized = false;
            abilityEffectContext = null;
            OnCollission = delegate { };
        }

    }

}