using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MusicPlayerUI : WindowContentController {

        //private MusicPlayerComponent musicPlayer = null;

        [SerializeField]
        private HighlightButton playButton = null;

        [SerializeField]
        private HighlightButton pauseButton = null;

        [SerializeField]
        private HighlightButton stopButton = null;

        [SerializeField]
        private GameObject highlightButtonPrefab = null;

        //[SerializeField]
        //private Transform highlightButtonParent = null;

        [SerializeField]
        private TextMeshProUGUI musicDescription = null;

        [SerializeField]
        private GameObject availableArea = null;

        private List<AudioProfile> audioProfileList = new List<AudioProfile>();

        private List<MusicPlayerHighlightButton> musicPlayerHighlightButtons = new List<MusicPlayerHighlightButton>();

        //private MusicPlayerHighlightButton selectedMusicPlayerHighlightButton;

        private AudioProfile currentAudioProfile = null;

        private AudioType audioType = AudioType.Music;

        // game manager references
        private ObjectPooler objectPooler = null;
        private UIManager uIManager = null;
        private MusicPlayerManager musicPlayerManager = null;

        //public MusicPlayerHighlightButton SelectedMusicPlayerHighlightButton { get => selectedMusicPlayerHighlightButton; set => selectedMusicPlayerHighlightButton = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playButton.Configure(systemGameManager);
            pauseButton.Configure(systemGameManager);
            stopButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            uIManager = systemGameManager.UIManager;
            musicPlayerManager = systemGameManager.MusicPlayerManager;
        }

        public void DeactivateButtons() {
            if (playButton != null) {
                playButton.Button.interactable = false;
            }
            if (pauseButton != null) {
                pauseButton.Button.interactable = false;
            }
            if (stopButton != null) {
                stopButton.Button.interactable = false;
            }
        }

        public void ShowAudioProfilesCommon() {
            //Debug.Log("MusicPlayerUI.ShowAudioProfilesCommon()");

            ClearMusicProfiles();

            MusicPlayerHighlightButton firstAvailableAudioProfile = null;

            foreach (AudioProfile audioProfile in musicPlayerManager.MusicPlayerProps.AudioProfileList) {
                GameObject go = objectPooler.GetPooledObject(highlightButtonPrefab, availableArea.transform);
                MusicPlayerHighlightButton qs = go.GetComponent<MusicPlayerHighlightButton>();
                qs.Configure(systemGameManager);
                qs.Text.text = audioProfile.DisplayName;
                qs.Text.color = Color.white;
                qs.SetMusicProfile(this, audioProfile);
                musicPlayerHighlightButtons.Add(qs);
                audioProfileList.Add(audioProfile);
                uINavigationControllers[0].AddActiveButton(qs);
                if (firstAvailableAudioProfile == null) {
                    firstAvailableAudioProfile = qs;
                }
            }

            if (firstAvailableAudioProfile == null) {
                // no available skills anymore, close window
                uIManager.musicPlayerWindow.CloseWindow();
            }

            //if (SelectedMusicPlayerHighlightButton == null && firstAvailableAudioProfile != null) {
            if (firstAvailableAudioProfile != null) {
                //firstAvailableAudioProfile.Select();
                uINavigationControllers[0].FocusFirstButton();
            }
        }

        /*
        public void ShowMusicProfiles() {
            //Debug.Log("SkillTrainerUI.ShowSkills()");
            ShowAudioProfilesCommon(musicPlayer);
        }
        */

        public void ShowAudioProfiles() {
            //Debug.Log("SkillTrainerUI.ShowSkills(" + skillTrainer.name + ")");
            //this.musicPlayer = musicPlayer;
            audioType = musicPlayerManager.MusicPlayerProps.AudioType;
            ShowAudioProfilesCommon();
        }

        /*
        public void UpdateSelected() {
            //Debug.Log("SkillTrainerUI.UpdateSelected()");
            if (SelectedMusicPlayerHighlightButton != null) {
                ShowDescription(SelectedMusicPlayerHighlightButton.MyMusicProfile);
            }
        }
        */

        private void UpdateButtons(AudioProfile musicProfile) {
            //Debug.Log("MusicPlayerUI.UpdateButtons(" + musicProfile + ")");
            playButton.Button.interactable = true;
            if (audioType == AudioType.Music) {
                if (audioManager.MusicAudioSource.isPlaying == true) {
                    stopButton.Button.interactable = true;
                    pauseButton.Button.interactable = true;
                } else {
                    stopButton.Button.interactable = false;
                    pauseButton.Button.interactable = false;
                }
            } else if (audioType == AudioType.Ambient) {
                if (audioManager.AmbientAudioSource.isPlaying == true) {
                    stopButton.Button.interactable = true;
                    pauseButton.Button.interactable = true;
                } else {
                    stopButton.Button.interactable = false;
                    pauseButton.Button.interactable = false;
                }
            } else if (audioType == AudioType.Effect) {
                stopButton.Button.interactable = true;
                pauseButton.Button.interactable = false;
            }

            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[1].FocusCurrentButton();
        }

        public void ShowDescription(AudioProfile musicProfile) {
            //Debug.Log("SkillTrainerUI.ShowDescription(" + skillName + ")");
            ClearDescription();

            if (musicProfile == null) {
                return;
            }
            currentAudioProfile = musicProfile;

            UpdateButtons(musicProfile);

            musicDescription.text = string.Format("<size=30><b><color=yellow>{0}</color></b></size>\n\n<size=18>{1}</size>", musicProfile.DisplayName, musicProfile.Description);
            if (musicProfile.ArtistName != null && musicProfile.ArtistName != string.Empty) {
                musicDescription.text += string.Format("\n\n<size=20><b>Author:</b></size> {0}\n\n", musicProfile.ArtistName);
            }
        }

        public void ClearDescription() {
            //Debug.Log("SkillTrainerUI.ClearDescription()");
            musicDescription.text = string.Empty;
        }

        public void SetSelectedButton(MusicPlayerHighlightButton musicPlayerHighlightButton) {
            uINavigationControllers[0].UnHightlightButtonBackgrounds(musicPlayerHighlightButton);
        }

        public void ClearMusicProfiles() {
            //Debug.Log("SkillTrainerUI.ClearSkills()");
            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (MusicPlayerHighlightButton musicPlayerHighlightButton in musicPlayerHighlightButtons) {
                if (musicPlayerHighlightButton != null) {
                    musicPlayerHighlightButton.gameObject.transform.SetParent(null);
                    musicPlayerHighlightButton.DeSelect();
                    objectPooler.ReturnObjectToPool(musicPlayerHighlightButton.gameObject);
                }
            }
            musicPlayerHighlightButtons.Clear();
            uINavigationControllers[0].ClearActiveButtons();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("SkillTrainerUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            //SelectedMusicPlayerHighlightButton = null;
        }

        public void PlayMusic() {
            //Debug.Log("MusicPlayerUI.PlayMusic()");
            if (currentAudioProfile != null && currentAudioProfile.AudioClip != null) {
                if (audioType == AudioType.Music) {
                    audioManager.PlayMusic(currentAudioProfile.AudioClip);
                    playButton.Button.interactable = false;
                    pauseButton.Button.interactable = true;
                    stopButton.Button.interactable = true;
                } else if (audioType == AudioType.Ambient) {
                    audioManager.PlayAmbient(currentAudioProfile.AudioClip);
                    playButton.Button.interactable = false;
                    pauseButton.Button.interactable = true;
                    stopButton.Button.interactable = true;
                } else if (audioType == AudioType.Effect) {
                    audioManager.PlayEffect(currentAudioProfile.AudioClip);
                    playButton.Button.interactable = true;
                    pauseButton.Button.interactable = false;
                    stopButton.Button.interactable = true;
                }
            }
            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[1].FocusCurrentButton();
        }

        public void PauseMusic() {
            if (audioType == AudioType.Music) {
                audioManager.PauseMusic();
            } else if (audioType == AudioType.Ambient) {
                audioManager.PauseAmbient();
            }
            playButton.Button.interactable = true;
            pauseButton.Button.interactable = false;
            stopButton.Button.interactable = true;

            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[1].FocusCurrentButton();
        }

        public void StopMusic() {
            //Debug.Log("SkillTrainerUI.UnlearnSkill()");
            if (audioType == AudioType.Music) {
                audioManager.StopMusic();
            } else if (audioType == AudioType.Ambient) {
                audioManager.StopAmbient();
            } else if (audioType == AudioType.Effect) {
                audioManager.StopEffects();
            }
            playButton.Button.interactable = true;
            stopButton.Button.interactable = false;
            pauseButton.Button.interactable = false;

            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[1].FocusCurrentButton();
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("MusicPlayerUI.OnOpenWindow()");
            SetNavigationController(uINavigationControllers[0]);
            base.ProcessOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            DeactivateButtons();
            ClearDescription();
            ShowAudioProfiles();

        }
    }

}