using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class AnimatedObjectProps : InteractableOptionProps {

        [Header("Animated Object")]

        [Tooltip("If true, this option can only be interacted with via a switch.")]
        [SerializeField]
        protected bool switchOnly = false;

        [Tooltip("The animation component to interact with")]
        [SerializeField]
        private Animation animationComponent = null;

        [Header("Open")]

        [Tooltip("The animation clip to play when opening the object")]
        [SerializeField]
        private AnimationClip openAnimationClip = null;

        [Tooltip("The audio clip to play when opening the object")]
        [SerializeField]
        private AudioClip openAudioClip = null;

        [Tooltip("The audio profile containing the clip to play when opening the object")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string openAudioProfile = string.Empty;

        [Header("Close")]

        [Tooltip("The animation clip to play when closing the object")]
        [SerializeField]
        private AnimationClip closeAnimationClip = null;

        [Tooltip("The audio clip to play when opening the object")]
        [SerializeField]
        private AudioClip closeAudioClip = null;

        [Tooltip("The audio profile containing the clip to play when opening the object")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        private string closeAudioProfile = string.Empty;

        /*
        [Tooltip("If true, the animation will keep looping while the switch is in the on state")]
        [SerializeField]
        private bool loop = false;

        public bool Loop { get => loop; set => loop = value; }
        */
        public bool SwitchOnly { get => switchOnly; set => switchOnly = value; }
        public AnimationClip OpenAnimationClip { get => openAnimationClip; set => openAnimationClip = value; }
        public AnimationClip CloseAnimationClip { get => closeAnimationClip; set => closeAnimationClip = value; }
        public Animation AnimationComponent { get => animationComponent; set => animationComponent = value; }
        public AudioClip OpenAudioClip { get => openAudioClip; set => openAudioClip = value; }
        public AudioClip CloseAudioClip { get => closeAudioClip; set => closeAudioClip = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new AnimatedObjectComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (openAudioClip == null && openAudioProfile != null && openAudioProfile != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(openAudioProfile);
                if (audioProfile != null) {
                    openAudioClip = audioProfile.AudioClip;
                } else {
                    Debug.LogError("AnimatedObjectProps.SetupScriptableObjects(): Could not find audio profile: " + openAudioProfile + " while inititalizing an animated object.  CHECK INSPECTOR");
                }
            }

            if (closeAudioClip == null && closeAudioProfile != null && closeAudioProfile != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(closeAudioProfile);
                if (audioProfile != null) {
                    closeAudioClip = audioProfile.AudioClip;
                } else {
                    Debug.LogError("AnimatedObjectProps.SetupScriptableObjects(): Could not find audio profile: " + closeAudioProfile + " while inititalizing an animated object.  CHECK INSPECTOR");
                }
            }
        }
    }

}