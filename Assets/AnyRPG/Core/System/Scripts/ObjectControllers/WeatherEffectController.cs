using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

namespace AnyRPG {

    public class WeatherEffectController : MonoBehaviour {

        [Header("Weather")]

        [SerializeField]
        protected GameObject followTarget = null;

        [SerializeField]
        protected AudioSource audioSource = null;

        [SerializeField]
        protected ParticleSystem weatherParticleSystem = null;

        [Tooltip("The time in seconds that the system should be allowed to stop playing after a stop command is issued before the particlesystem is removed")]
        [SerializeField]
        protected float fadeTime = 0f;

        [Tooltip("Set to true to move out of range world space particles close to the target when the target is moving")]
        [SerializeField]
        protected bool teleportParticles = false;

        private UpdateParticlesJob job = new UpdateParticlesJob();

        protected Vector3 forwardDirection = new Vector3(0, 0, 0);


        public float FadeTime { get => fadeTime; set => fadeTime = value; }

        public void StartPlaying() {
            if (weatherParticleSystem != null) {
                weatherParticleSystem.Play(true);
            }
        }

        public void StopPlaying(bool immediate = false) {
            StopAudio();
            if (weatherParticleSystem != null) {
                if (immediate == true) {
                    weatherParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                } else {
                    weatherParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

        public void StopAudio() {
            if (audioSource != null) {
                audioSource.Stop();
            }
        }

        public virtual void SetTarget(GameObject followTarget) {
            this.followTarget = followTarget;
        }

        void Update() {
            if (followTarget != null) {
                UpdatePosition();
            }
        }

        protected virtual void UpdatePosition() {
            transform.position = followTarget.transform.position;

            // since this weather effect is following and turning with the camera, it is necessary to exclude the y direction
            // to keep the weather effect facing in the same direction, but not looking up or down with the camera
            forwardDirection.x = followTarget.transform.forward.x;
            forwardDirection.y = 0;
            forwardDirection.z = followTarget.transform.forward.z;
            transform.forward = forwardDirection;

            // transforms cannot be accessed from inside the UpdateParticlesJob struct so it is necessary to
            // send the position and rotation as Vector3 and Quaternion structs
            job.followTransformPosition = followTarget.transform.position;
            job.followTransformDirection = followTarget.transform.rotation;
        }

        /// <summary>
        /// Teleport the particles to stay within +/- 25m horizontal and +/- 10m vertical from the camera when it is moving.
        /// This particle teleportation looks much more realistic than setting the snow effect to world space and having it move with the character
        /// </summary>
        struct UpdateParticlesJob : IJobParticleSystemParallelFor {

            public Vector3 followTransformPosition;
            public Quaternion followTransformDirection;

            public void Execute(ParticleSystemJobData particles, int i) {

                NativeArray<float> positionsX = particles.positions.x;
                NativeArray<float> positionsY = particles.positions.y;
                NativeArray<float> positionsZ = particles.positions.z;

                //Debug.Log(positionsX[i] + " " + positionsY[i] + " " + positionsZ[i]);

                // convert world space positions of particles into local space
                Vector3 particleWorldPosition = new Vector3(positionsX[i], positionsY[i], positionsZ[i]);
                Vector3 worldSpaceDir = particleWorldPosition - followTransformPosition;
                Vector3 localSpaceDir = Quaternion.Inverse(followTransformDirection) * worldSpaceDir;

                // perform position modifications in local space
                bool changedPosition = false;
                if (localSpaceDir.z < -25f) {
                    localSpaceDir.z += 50f;
                    changedPosition = true;
                }
                if (localSpaceDir.z > 25f) {
                    localSpaceDir.z -= 50f;
                    changedPosition = true;
                }
                if (localSpaceDir.x < -25f) {
                    localSpaceDir.x += 50f;
                    changedPosition = true;
                }
                if (localSpaceDir.x > 25f) {
                    localSpaceDir.x -= 50f;
                    changedPosition = true;
                }
                if (localSpaceDir.y < -10f) {
                    localSpaceDir.y += 20f;
                    changedPosition = true;
                }
                if (localSpaceDir.y > 10f) {
                    localSpaceDir.y -= 20f;
                    changedPosition = true;
                }

                if (changedPosition == true) {
                    // convert local space back to world space
                    Vector3 newDirection = followTransformDirection * localSpaceDir;
                    Vector3 newPosition = followTransformPosition + newDirection;

                    // update particle positions
                    positionsX[i] = newPosition.x;
                    positionsY[i] = newPosition.y;
                    positionsZ[i] = newPosition.z;
                }

            }
        }

        void OnParticleUpdateJobScheduled() {
            if (followTarget != null && teleportParticles == true) {
                job.Schedule(weatherParticleSystem, 5000);
            }
        }
    }

}

