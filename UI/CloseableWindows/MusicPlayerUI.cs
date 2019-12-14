using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
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

        private MusicPlayer musicPlayer;

        [SerializeField]
        private Button playButton, pauseButton, stopButton;

        [SerializeField]
        private GameObject highlightButtonPrefab;

        [SerializeField]
        private Transform highlightButtonParent;

        [SerializeField]
        private Text musicDescription;

        [SerializeField]
        private GameObject availableHeading;

        [SerializeField]
        private GameObject availableArea;

        [SerializeField]
        private GameObject learnedHeading;

        [SerializeField]
        private GameObject learnedArea;

        //private List<GameObject> Skills = new List<GameObject>();
        private List<MusicProfile> musicProfileList = new List<MusicProfile>();

        private List<MusicPlayerHighlightButton> musicPlayerHighlightButtons = new List<MusicPlayerHighlightButton>();

        private MusicPlayerHighlightButton selectedMusicPlayerHighlightButton;

        private MusicProfile currentMusicProfile = null;

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

            foreach (MusicProfile musicProfile in musicPlayer.MyMusicProfileList) {
                GameObject go = Instantiate(highlightButtonPrefab, availableArea.transform);
                MusicPlayerHighlightButton qs = go.GetComponent<MusicPlayerHighlightButton>();
                qs.MyText.text = musicProfile.MyName;
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
        private void UpdateButtons(MusicProfile musicProfile) {
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

        public void ShowDescription(MusicProfile musicProfile) {
            //Debug.Log("SkillTrainerUI.ShowDescription(" + skillName + ")");
            ClearDescription();

            if (musicProfile == null) {
                return;
            }
            currentMusicProfile = musicProfile;

            UpdateButtons(musicProfile);

            musicDescription.text = string.Format("<size=30><b><color=yellow>{0}</color></b></size>\n\n<size=18>{1}</size>", musicProfile.MyName, musicProfile.MyDescription);
            if (musicProfile.MyArtistName != null && musicProfile.MyArtistName != string.Empty) {
                musicDescription.text += string.Format("\n\n<size=20><b>Author:</b></size> {0}\n\n", musicProfile.MyArtistName);
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
            if (currentMusicProfile != null && currentMusicProfile.MyAudioClip != null) {
                AudioManager.MyInstance.PlayMusic(currentMusicProfile.MyAudioClip);
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