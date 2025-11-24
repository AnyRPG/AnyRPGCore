using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class TimeOfDayManagerClient : ConfiguredClass {

        private bool nightTime = true;

        protected Light sunLight = null;
        protected GameObject sunObject = null;
        protected ParticleSystem sunParticle = null;

        // settings from the scene node
        protected bool rotateSunDirection = false;
        private float sunAngle = 0f;
        private bool rotateSunColor = false;
        private Gradient sunGradient;
        private float defaultSunIntensity = 1f;
        private bool blendSkybox = false;
        private Material skyboxMaterial = null;
        private bool rotateSkybox = false;
        private float skyboxRotationOffset = 0f;
        private float skyboxRotationDirection = 1f;
        private float currentSkyboxRotation = 0f;

        // state tracking
        private SceneNode activeSceneNode = null;
        private bool playAmbientSounds = true;
        private bool levelLoaded = false;
        private AudioClip previousAudioClip = null;
        private AudioClip currentAudioClip = null;

        // the number of seconds elapsed since the last time calculation
        //private float elapsedSeconds = 0f;

        // game manager references
        protected LevelManager levelManager = null;
        protected AudioManager audioManager = null;
        protected WeatherManagerClient weatherManager = null;
        protected TimeOfDayManagerServer timeOfDayManagerServer = null;
        protected SystemEventManager systemEventManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            systemEventManager.OnLevelUnloadClient += HandleLevelUnload;
            systemEventManager.OnLevelLoad += HandleLevelLoad;

            networkManagerServer.OnStartServer += HandleStartServer;
            networkManagerServer.OnStopServer += HandleStopServer;
            systemEventManager.OnCalculateRelativeTime += HandleCalculateRelativeTime;
        }

        public void HandleStopServer() {
            systemEventManager.OnCalculateRelativeTime += HandleCalculateRelativeTime;
        }

        public void HandleStartServer() {
            systemEventManager.OnCalculateRelativeTime -= HandleCalculateRelativeTime;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            levelManager = systemGameManager.LevelManager;
            audioManager = systemGameManager.AudioManager;
            weatherManager = systemGameManager.WeatherManagerClient;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        private void HandleCalculateRelativeTime() {
            PerformTimeOfDayOperations();
        }

        public void SuppressAmbientSounds() {
            playAmbientSounds = false;
            //audioManager.CrossFadeAmbient(null, 3f);
        }

        public void AllowAmbientSounds() {
            playAmbientSounds = true;
            if (levelLoaded == false) {
                return;
            }
            //PlayAmbientSounds(3f);
        }

        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            //Debug.Log("TimeOfDayManager.HandleLevelUnload()");

            rotateSunDirection = false;
            rotateSunColor = false;
            sunAngle = 0f;
            sunObject = null;
            defaultSunIntensity = 1f;
            blendSkybox = false;
            skyboxMaterial = null;
            rotateSkybox = false;
            skyboxRotationOffset = 0f;
            skyboxRotationDirection = 1f;
            levelLoaded = false;
            activeSceneNode = null;
            sunParticle = null;

            audioManager.StopAmbient();
        }

        public void HandleLevelLoad() {
            //Debug.Log("TimeOfDayManager.HandleLevelLoad()");

            if (systemGameManager.GameMode == GameMode.Network && networkManagerServer.ServerModeActive == true) {
                return;
            }

            levelLoaded = true;
            activeSceneNode = levelManager.GetActiveSceneNode();
            if (activeSceneNode != null) {
                sunLight = RenderSettings.sun;
                if (sunLight != null) {
                    if (activeSceneNode.SunRotationMode != SunRotationMode.None) {
                        rotateSunDirection = true;
                        if (activeSceneNode.UseDefaultSunAngle == true) {
                            sunAngle = systemConfigurationManager.DefaultSunAngle;
                        } else {
                            sunAngle = activeSceneNode.SunAngle;
                        }
                    }
                    if (activeSceneNode.SunRotationMode == SunRotationMode.Parent) {
                        sunObject = RenderSettings.sun.transform.parent.gameObject;
                        sunParticle = sunLight.GetComponent<ParticleSystem>();
                    } else if (activeSceneNode.SunRotationMode == SunRotationMode.Sun) {
                        sunObject = RenderSettings.sun.gameObject;
                    }
                    if (activeSceneNode.RotateSunColor == true) {
                        rotateSunColor = true;
                        if (activeSceneNode.UseDefaultSunGradient == true) {
                            sunGradient = systemConfigurationManager.DefaultSunGradient;
                        } else {
                            sunGradient = activeSceneNode.SunGradient;
                        }
                    }
                    defaultSunIntensity = sunLight.intensity;

                    if (RenderSettings.skybox != null) {
                        // make copy of material so the original skybox material on disk is not changed
                        skyboxMaterial = new Material(RenderSettings.skybox);
                        RenderSettings.skybox = skyboxMaterial;

                        if (activeSceneNode.BlendedSkybox == true) {
                            blendSkybox = true;
                        }
                        if (activeSceneNode.RotateSkybox == true) {
                            rotateSkybox = true;
                            skyboxRotationOffset = activeSceneNode.SkyboxRotationOffset;
                            if (activeSceneNode.ReverseSkyboxRotation == true) {
                                skyboxRotationDirection = -1f;
                            }
                        }
                    }
                }

                // special check on level load to activate day or night settings
                // otherwise sun could remain on if level was entered at night but particle system was on by default
                if (IsDayTime()) {
                    ActivateDay();
                } else {
                    ActivateNight(true);
                }

                // since this is a level load, set fade-in to 3 seconds, instead of using time of day speed
                PlayAmbientSounds(3f);
            }
        }

        private bool IsDayTime() {
            if (timeOfDayManagerServer.InGameTime.TimeOfDay.TotalSeconds >= 10800 && timeOfDayManagerServer.InGameTime.TimeOfDay.TotalSeconds < 61800) {
                return true;
            }

            return false;
        }

        private bool IsNightTime() {
            if (timeOfDayManagerServer.InGameTime.TimeOfDay.TotalSeconds < 10800 || timeOfDayManagerServer.InGameTime.TimeOfDay.TotalSeconds >= 61800) {
                return true;
            }

            return false;
        }


        private void ActivateDay() {
            if (sunParticle != null) {
                sunParticle.Play();
            }
        }

        private void ActivateNight(bool immediateStop) {
            if (sunParticle != null) {
                if (immediateStop == true) {
                    sunParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                } else {
                    sunParticle.Stop(true);
                }
            }
        }

        private void PerformTimeOfDayOperations() {

            if (nightTime == true && IsDayTime()) {
                nightTime = false;
                ActivateDay();
                PlayAmbientSounds();
            } else if (nightTime == false && IsNightTime()) {
                nightTime = true;
                ActivateNight(false);
                PlayAmbientSounds();
            }

            if (rotateSunDirection == true) {
                RotateSunDirection();
            }
            if (rotateSunColor == true) {
                RotateSunColor();
            }
            if (blendSkybox == true) {
                BlendSkybox();
            }
            if (rotateSkybox == true) {
                RotateSkybox();
            }
        }

        private void RotateSunDirection() {
            if (sunObject == null) {
                return;
            }

            //sunSource.transform.localEulerAngles = new Vector3(((float)inGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f, -30f, 0f);
            //sunSource.transform.localEulerAngles = new Vector3(90f, ((float)inGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f, 0f);
            //sunSource.transform.localRotation = Quaternion.Euler(90f, 0, ((float)inGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f);
            //sunSource.transform.localRotation = Quaternion.Euler(90f, ((float)inGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f, 0f);
            //sunSource.transform.RotateAround(Vector3.zero, Vector3.right, ((float)inGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f);

            sunObject.transform.localRotation = Quaternion.Euler(sunAngle, 0f, 0f);

            sunObject.transform.localRotation *= Quaternion.AngleAxis(Mathf.Clamp((((float)timeOfDayManagerServer.InGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f) -180f, -91f, 91f), Vector3.up);
        }

        private void RotateSunColor() {
            sunLight.color = sunGradient.Evaluate((float)timeOfDayManagerServer.InGameTime.TimeOfDay.TotalSeconds / 86400f);
            sunLight.intensity = defaultSunIntensity * sunLight.color.a;
        }

        private void BlendSkybox() {
            skyboxMaterial.SetFloat("_Blend", (sunLight.color.a * -1f) + 1f);
        }

        private void RotateSkybox() {
            currentSkyboxRotation = ((((float)timeOfDayManagerServer.InGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f) * skyboxRotationDirection) + skyboxRotationOffset;
            if (currentSkyboxRotation > 360f) {
                currentSkyboxRotation -= 360f;
            } else if (currentSkyboxRotation < 0f) {
                currentSkyboxRotation += 360f;
            }
            skyboxMaterial.SetFloat("_Rotation", currentSkyboxRotation);
        }

        public void PlayAmbientSounds() {
            // 10800 = 3 hours
            PlayAmbientSounds(10800 / systemConfigurationManager.TimeOfDaySpeed);
        }

        public void PlayAmbientSounds(float fadeTime) {
            if (activeSceneNode != null) {
                previousAudioClip = currentAudioClip;

                if (playAmbientSounds == false) {
                    currentAudioClip = null;
                } else {
                    if (weatherManager.CurrentAmbientSound != null) {
                        currentAudioClip = weatherManager.CurrentAmbientSound;
                    } else {
                        if (nightTime == true) {
                            currentAudioClip = activeSceneNode.NightAmbientSound;
                        } else {
                            currentAudioClip = activeSceneNode.DayAmbientSound;
                        }
                    }
                }

                if (currentAudioClip == previousAudioClip
                    && audioManager.AmbientAudioSource.clip == currentAudioClip
                    && audioManager.AmbientAudioSource.isPlaying == true) {
                    return;
                }
                //if (currentAudioClip != null) {
                if (previousAudioClip == null && currentAudioClip == null) {
                    // no need to crossfade since nothing was playing and nothing will be playing
                    return;
                }
                    audioManager.CrossFadeAmbient(currentAudioClip, fadeTime);
                //} else {
                //    audioManager.StopAmbient();
                //}

            }

        }

    }

}