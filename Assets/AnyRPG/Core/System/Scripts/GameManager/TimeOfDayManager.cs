using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    public class TimeOfDayManager : ConfiguredMonoBehaviour {

        // calculate in-game time every x seconds
        //private const float tickLength = 1f;

        // track the passage of in-game time
        //protected int secondsSinceMidnight = 0;
        private DateTime realCurrentTime;
        private DateTime inGameTime;
        private bool nightSounds = true;

        protected Light sunLight = null;
        protected GameObject sunObject = null;

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

        // the number of seconds elapsed since the last time calculation
        //private float elapsedSeconds = 0f;

        // game manager references
        protected SystemDataFactory systemDataFactory = null;
        protected LevelManager levelManager = null;
        protected AudioManager audioManager = null;

        public DateTime InGameTime { get => inGameTime; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnLevelLoad", HandleLevelLoad);
            CalculateRelativeTime();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            systemDataFactory = systemGameManager.SystemDataFactory;
            levelManager = systemGameManager.LevelManager;
            audioManager = systemGameManager.AudioManager;
        }

        private void Update() {
            //elapsedSeconds += Time.deltaTime;
            
            // only calculate time every second
            //if (elapsedSeconds > tickLength) {
                //elapsedSeconds -= tickLength;
                CalculateRelativeTime();
                PerformTimeOfDayOperations();
            //}
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
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
    }

    public void HandleLevelLoad(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("TimeOfDayManager.HandleLevelLoad()");

            if (levelManager.GetActiveSceneNode() != null) {
                sunLight = RenderSettings.sun;
                if (sunLight != null) {
                    sunObject = RenderSettings.sun.gameObject;
                    if (levelManager.GetActiveSceneNode().RotateSunDirection == true) {
                        rotateSunDirection = true;
                        if (levelManager.GetActiveSceneNode().UseDefaultSunAngle == true) {
                            sunAngle = systemConfigurationManager.DefaultSunAngle;
                        } else {
                            sunAngle = levelManager.GetActiveSceneNode().SunAngle;
                        }
                    }
                    if (levelManager.GetActiveSceneNode().RotateSunColor == true) {
                        rotateSunColor = true;
                        if (levelManager.GetActiveSceneNode().UseDefaultSunGradient == true) {
                            sunGradient = systemConfigurationManager.DefaultSunGradient;
                        } else {
                            sunGradient = levelManager.GetActiveSceneNode().SunGradient;
                        }
                    }
                    defaultSunIntensity = sunLight.intensity;

                    if (RenderSettings.skybox != null) {
                        if (levelManager.GetActiveSceneNode().BlendedSkybox == true) {
                            blendSkybox = true;
                            skyboxMaterial = RenderSettings.skybox;
                        }
                        if (levelManager.GetActiveSceneNode().RotateSkybox == true) {
                            rotateSkybox = true;
                            skyboxRotationOffset = levelManager.GetActiveSceneNode().SkyboxRotationOffset;
                            if (levelManager.GetActiveSceneNode().ReverseSkyboxRotation == true) {
                                skyboxRotationDirection = -1f;
                            }
                        }
                    }
                }

                PlayAmbientSounds();
            }
        }

        /// <summary>
        /// calculate in-game time relative to real world time
        /// </summary>
        private void CalculateRelativeTime() {
            realCurrentTime = DateTime.Now;
            inGameTime = DateTime.Now;
            inGameTime = realCurrentTime.AddSeconds((realCurrentTime.TimeOfDay.TotalSeconds * systemConfigurationManager.TimeOfDaySpeed) - realCurrentTime.TimeOfDay.TotalSeconds);
            //Debug.Log("Time is " + inGameTime.ToShortTimeString());

            if (nightSounds == true && inGameTime.TimeOfDay.TotalSeconds >= 10800 && inGameTime.TimeOfDay.TotalSeconds < 61800) {
                nightSounds = false;
                PlayAmbientSounds();
            } else if (nightSounds == false && (inGameTime.TimeOfDay.TotalSeconds < 10800 || inGameTime.TimeOfDay.TotalSeconds >= 61800)) {
                nightSounds = true;
                PlayAmbientSounds();
            }
        }

        private void PerformTimeOfDayOperations() {
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

            sunObject.transform.localRotation = Quaternion.Euler(sunAngle + 90f, 0f, 0f);
            sunObject.transform.localRotation *= Quaternion.AngleAxis(Mathf.Clamp((((float)inGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f) -180f, -91f, 91f), Vector3.up);
        }

        private void RotateSunColor() {
            sunLight.color = sunGradient.Evaluate((float)inGameTime.TimeOfDay.TotalSeconds / 86400f);
            sunLight.intensity = defaultSunIntensity * sunLight.color.a;
        }

        private void BlendSkybox() {
            skyboxMaterial.SetFloat("_Blend", (sunLight.color.a * -1f) + 1f);
        }

        private void RotateSkybox() {
            currentSkyboxRotation = ((((float)inGameTime.TimeOfDay.TotalSeconds / 86400f) * 360f) * skyboxRotationDirection) + skyboxRotationOffset;
            if (currentSkyboxRotation > 360f) {
                currentSkyboxRotation -= 360f;
            } else if (currentSkyboxRotation < 0f) {
                currentSkyboxRotation += 360f;
            }
            skyboxMaterial.SetFloat("_Rotation", currentSkyboxRotation);
        }

        private void PlayAmbientSounds() {
            if (levelManager.GetActiveSceneNode() != null) {
                AudioClip audioClip = null;
                if (nightSounds == true) {
                    audioClip = levelManager.GetActiveSceneNode().NightAmbientSound;
                } else {
                    audioClip = levelManager.GetActiveSceneNode().DayAmbientSound;
                }
                if (audioClip != null) {
                    // 10800 = 3 hours
                    audioManager.CrossFadeAmbient(audioClip, 10800 / systemConfigurationManager.TimeOfDaySpeed);
                } else {
                    audioManager.StopAmbient();
                }

            }

        }

    }

}