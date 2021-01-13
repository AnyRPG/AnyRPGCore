﻿using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class UnitComponentController : MonoBehaviour {

        [Tooltip("Drag an object in the heirarchy here and the nameplate will show at its transform location")]
        [SerializeField]
        private Transform namePlateTransform = null;

        [SerializeField]
        private AggroRange aggroRangeController = null;

        [SerializeField]
        private InteractableRange interactableRange = null;

        [Tooltip("A reference to the unit audio emitter.  This is optional as it will be found automatically if left blank.")]
        [SerializeField]
        private UnitAudioEmitter unitAudioEmitter = null;

        public Transform NamePlateTransform { get => namePlateTransform; set => namePlateTransform = value; }
        public AggroRange AggroRangeController { get => aggroRangeController; set => aggroRangeController = value; }
        public InteractableRange InteractableRange { get => interactableRange; set => interactableRange = value; }
        //public UnitAudioEmitter UnitAudioEmitter { get => unitAudioEmitter; set => unitAudioEmitter = value; }

        private void Awake() {
            if (unitAudioEmitter == null) {
                unitAudioEmitter = GetComponentInChildren<UnitAudioEmitter>();
                Debug.Log(gameObject.name + "UnitAudioController.Awake(): UnitAudioEmitter was not set.  Searching children.");
                if (unitAudioEmitter == null) {
                    Debug.LogError(gameObject.name + "UnitAudioController.Awake(): Could not find UnitAudioEmitter in children.  Check object.");
                }
            }
        }

        public bool MovementIsPlaying() {
            if (unitAudioEmitter == null) {
                return false;
            }
            return unitAudioEmitter.MovementIsPlaying();
        }

        public void PlayCast(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + "UnitAudio.PlayEffect(" + audioClip.name + ")");

            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayCast(audioClip);
            }
        }

        public void PlayEffect(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            //Debug.Log(gameObject.name + "UnitAudio.PlayEffect(" + audioClip.name + ")");

            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayEffect(audioClip);
            }
        }

        public void PlayVoice(AudioClip audioClip) {
            if (audioClip == null) {
                return;
            }
            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayVoice(audioClip);
            }
        }

        public void PlayMovement(AudioClip audioClip, bool loop) {
            //Debug.Log(gameObject.name + "UnitAudio.PlayMovement()");
            if (audioClip == null) {
                return;
            }
            if (unitAudioEmitter != null) {
                unitAudioEmitter.PlayMovement(audioClip, loop);
            }
        }

        public void StopCast() {
            //Debug.Log(gameObject.name + "UnitAudio.StopCast()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopCast();
            }
        }

        public void StopEffect() {
            //Debug.Log(gameObject.name + "UnitAudio.StopEffect()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopEffect();
            }
        }

        public void StopVoice() {
            //Debug.Log(gameObject.name + "UnitAudio.StopVoice()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopVoice();
            }
        }

        public void StopMovement() {
            //Debug.Log(gameObject.name + "UnitAudio.StopMovement()");
            if (unitAudioEmitter != null) {
                unitAudioEmitter.StopMovement();
            }
        }

    }

}
