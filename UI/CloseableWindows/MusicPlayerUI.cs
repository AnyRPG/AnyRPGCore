using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MusicPlayerUI : WindowContentController {

        #region Singleton
        private static MusicPlayerUI instance;

        public static MusicPlayerUI MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<MusicPlayerUI>();
                }

                return instance;
            }
        }

        #endregion

        private MusicPlayer musicPlayer = null;

        [SerializeField]
        private Button playButton = null;

        [SerializeField]
        private Button pauseButton = null;

        [SerializeField]
        private Button stopButton = null;

        [SerializeField]
        private GameObject highlightButtonPrefab = null;

        //[SerializeField]
        //private Transform highlightButtonParent = null;

        [SerializeField]
        private TextMeshProUGUI musicDescription = null;

        //[SerializeField]
        //private GameObject availableHeading = null;

        [SerializeField]
        private GameObject availableArea = null;

        //private List<GameObject> Skills = new List<GameObject>();
        private List<AudioProfile> musicProfileList = new List<AudioProfile>();

        private List<MusicPlayerHighlightButton> musicPlayerHighlightButtons = new List<MusicPlayerHighlightButton>();

        private MusicPlayerHighlightButton selectedMusicPlayerHighlightButton;

        private AudioProfile currentMusicProfile = null;

        public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

        public MusicPlayerHighlightButton MySelectedMusicPlayerHighlightButton { get => selectedMusicPlayerHighlightButton; set => selectedMusicPlayerHighlightButton = value; }

        private void Start() {
            //DeactivateButtons();
        }

        public void DeactivateButtons() {
            if (playButton != null) {
                playButton.interactable = false;
            }
            if (pauseButton != null) {
                pauseButton.interactable = false;
            }
            if (stopButton != null) {
                stopButton.interactable = false;
            }
        }

        public void ShowMusicProfilesCommon(MusicPlayer musicPlayer) {
            //Debug.Log("SkillTrainerUI.ShowSkillsCommon(" + skillTrainer.name + ")");

            ClearMusicProfiles();

            MusicPlayerHighlightButton firstAvailableMusicProfile = null;

            foreach (AudioProfile musicProfile in musicPlayer.MyMusicProfileList) {
                GameObject go = Instantiate(highlightButtonPrefab, availableArea.transform);
                MusicPlayerHighlightButton qs = go.GetComponent<MusicPlayerHighlightButton>();
                qs.MyText.text = musicProfile.DisplayName;
                qs.MyText.color = Color.white;
                qs.SetMusicProfile(musicProfile);
                musicPlayerHighlightButtons.Add(qs);
                musicProfileList.Add(musicProfile);
                if (firstAvailableMusicProfile == null) {
                    firstAvailableMusicProfile = qs;
                }
            }

            if (firstAvailableMusicProfile == null) {
                // no available skills anymore, close window
                PopupWindowManager.MyInstance.musicPlayerWindow.CloseWindow();
            }

            if (MySelectedMusicPlayerHighlightButton == null && firstAvailableMusicProfile != null) {
                firstAvailableMusicProfile.Select();
            }
        }


        public void ShowMusicProfiles() {
            //Debug.Log("SkillTrainerUI.ShowSkills()");
            ShowMusicProfilesCommon(musicPlayer);
        }

        public void ShowMusicProfiles(MusicPlayer musicPlayer) {
            //Debug.Log("SkillTrainerUI.ShowSkills(" + skillTrainer.name + ")");
            this.musicPlayer = musicPlayer;
            ShowMusicProfilesCommon(this.musicPlayer);
        }

        public void UpdateSelected() {
            //Debug.Log("SkillTrainerUI.UpdateSelected()");
            if (MySelectedMusicPlayerHighlightButton != null) {
                ShowDescription(MySelectedMusicPlayerHighlightButton.MyMusicProfile);
            }
        }

        // Enable or disable learn and unlearn buttons based on what is selected
        private void UpdateButtons(AudioProfile musicProfile) {
            //Debug.Log("SkillTrainerUI.UpdateButtons(" + skillName + ")");
            /*
            if (PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.HasSkill(musicProfileName)) {
                playButton.gameObject.SetActive(false);
                playButton.GetComponent<Button>().enabled = false;
                stopButton.gameObject.SetActive(true);
                stopButton.GetComponent<Button>().enabled = true;
            } else {
                playButton.gameObject.SetActive(true);
                playButton.GetComponent<Button>().enabled = true;
                stopButton.GetComponent<Button>().enabled = false;
                stopButton.gameObject.SetActive(false);
            }
            */
        }

        public void ShowDescription(AudioProfile musicProfile) {
            //Debug.Log("SkillTrainerUI.ShowDescription(" + skillName + ")");
            ClearDescription();

            if (musicProfile == null) {
                return;
            }
            currentMusicProfile = musicProfile;

            UpdateButtons(musicProfile);

            musicDescription.text = string.Format("<size=30><b><color=yellow>{0}</color></b></size>\n\n<size=18>{1}</size>", musicProfile.DisplayName, musicProfile.MyDescription);
            if (musicProfile.ArtistName != null && musicProfile.ArtistName != string.Empty) {
                musicDescription.text += string.Format("\n\n<size=20><b>Author:</b></size> {0}\n\n", musicProfile.ArtistName);
            }

        }

        public void ClearDescription() {
            //Debug.Log("SkillTrainerUI.ClearDescription()");
            musicDescription.text = string.Empty;
            DeselectMusicButtons();
        }

        public void DeselectMusicButtons() {
            //Debug.Log("MusicPlayerUI.DeselectMusicButtons()");
            foreach (MusicPlayerHighlightButton musicPlayerHighlightButton in musicPlayerHighlightButtons) {
                //Debug.Log("MusicPlayerUI.DeselectMusicButtons(): got a button");
                if (musicPlayerHighlightButton != MySelectedMusicPlayerHighlightButton) {
                    //Debug.Log("MusicPlayerUI.DeselectMusicButtons(): got a button and clearing it");
                    musicPlayerHighlightButton.DeSelect();
                }
            }
        }

        public void ClearMusicProfiles() {
            //Debug.Log("SkillTrainerUI.ClearSkills()");
            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (MusicPlayerHighlightButton musicPlayerHighlightButton in musicPlayerHighlightButtons) {
                if (musicPlayerHighlightButton != null) {
                    musicPlayerHighlightButton.gameObject.transform.SetParent(null);
                    Destroy(musicPlayerHighlightButton.gameObject);
                }
            }
            musicPlayerHighlightButtons.Clear();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("SkillTrainerUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            //DeactivateButtons();
            MySelectedMusicPlayerHighlightButton = null;
        }

        public void PlayMusic() {
            //Debug.Log("SkillTrainerUI.LearnSkill()");
            if (currentMusicProfile != null && currentMusicProfile.AudioClip != null) {
                AudioManager.MyInstance.PlayMusic(currentMusicProfile.AudioClip);
            }
        }

        public void PauseMusic() {
            /*
            if (currentMusicProfileName != null && currentMusicProfileName != string.Empty) {
                MusicProfile musicProfile = SystemMusicProfileManager.MyInstance.GetResource(currentMusicProfileName);
                if (musicProfile != null && musicProfile.MyAudioClip != null) {
                    AudioManager.MyInstance.PauseMusic();
                }
            }
            */
            AudioManager.MyInstance.PauseMusic();
        }

        public void StopMusic() {
            //Debug.Log("SkillTrainerUI.UnlearnSkill()");
            AudioManager.MyInstance.StopMusic();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("SkillTrainerUI.OnOpenWindow()");
            // clear before open window handler, because it shows quests
            ClearDescription();

            OnOpenWindow(this);
        }
    }

}