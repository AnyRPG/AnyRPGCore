using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ProjectileScript : MonoBehaviour {

        public event System.Action<IAbilityCaster, Interactable, GameObject, AbilityEffectContext> OnCollission = delegate { };

        [SerializeField]
        private AudioSource audioSource = null;

        private IAbilityCaster source;

        private Interactable target;

        private Vector3 positionOffset;

        private Vector3 targetPosition;

        private float velocity;

        private bool initialized = false;

        private AbilityEffectContext abilityEffectInput = null;

        //public AudioSource AudioSource { get => audioSource; }

        private void Update() {
            MoveTowardTarget();
        }

        public void Initialize(float velocity, IAbilityCaster source, Interactable target, Vector3 positionOffset, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("ProjectileScript.Initialize(" + velocity + ", " + source.name + ", " + (target == null ? "null" : target.name) + ", " + positionOffset + ")");
            this.source = source;
            this.velocity = velocity;
            this.target = target;
            this.positionOffset = positionOffset;
            this.abilityEffectInput = abilityEffectInput;
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
                targetPosition = new Vector3(target.transform.position.x + positionOffset.x, target.transform.position.y + positionOffset.y, target.transform.position.z + positionOffset.z);
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
            //Debug.Log("ProjectileScript.OnTriggerEnter(" + other.name + ")");
            if ((target != null && other.gameObject == target.gameObject) || target == null) {
                if (abilityEffectInput != null && abilityEffectInput.groundTargetLocation != null) {
                    abilityEffectInput.groundTargetLocation = transform.position;
                }
                OnCollission(source, target, gameObject, abilityEffectInput);
            }
        }

        private void OnCollisionEnter(Collision collision) {
            //Debug.Log("ProjectileScript.OnCollissionEnter()");
        }

        private void OnDestroy() {
            //Debug.Log(abilityEffectInput.baseAbility + ".ProjectileScript.OnDestroy()");
        }
    }

}