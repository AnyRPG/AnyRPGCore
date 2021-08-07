using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MusicPlayerUI : WindowContentController {

        private MusicPlayerComponent musicPlayer = null;

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

        //[SerializeField]
        //private GameObject availableHeading = null;

        [SerializeField]
        private GameObject availableArea = null;

        //private List<GameObject> Skills = new List<GameObject>();
        private List<AudioProfile> musicProfileList = new List<AudioProfile>();

        private List<MusicPlayerHighlightButton> musicPlayerHighlightButtons = new List<MusicPlayerHighlightButton>();

        private MusicPlayerHighlightButton selectedMusicPlayerHighlightButton;

        private AudioProfile currentMusicProfile = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private UIManager uIManager = null;

        public MusicPlayerHighlightButton MySelectedMusicPlayerHighlightButton { get => selectedMusicPlayerHighlightButton; set => selectedMusicPlayerHighlightButton = value; }

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

        public void ShowMusicProfilesCommon(MusicPlayerComponent musicPlayer) {
            //Debug.Log("SkillTrainerUI.ShowSkillsCommon(" + skillTrainer.name + ")");

            ClearMusicProfiles();

            MusicPlayerHighlightButton firstAvailableMusicProfile = null;

            foreach (AudioProfile musicProfile in musicPlayer.Props.MusicProfileList) {
                GameObject go = objectPooler.GetPooledObject(highlightButtonPrefab, availableArea.transform);
                MusicPlayerHighlightButton qs = go.GetComponent<MusicPlayerHighlightButton>();
                qs.Configure(systemGameManager);
                qs.Text.text = musicProfile.DisplayName;
                qs.Text.color = Color.white;
                qs.SetMusicProfile(this, musicProfile);
                musicPlayerHighlightButtons.Add(qs);
                musicProfileList.Add(musicProfile);
                if (firstAvailableMusicProfile == null) {
                    firstAvailableMusicProfile = qs;
                }
            }

            if (firstAvailableMusicProfile == null) {
                // no available skills anymore, close window
                uIManager.musicPlayerWindow.CloseWindow();
            }

            if (MySelectedMusicPlayerHighlightButton == null && firstAvailableMusicProfile != null) {
                firstAvailableMusicProfile.Select();
            }
        }


        public void ShowMusicProfiles() {
            //Debug.Log("SkillTrainerUI.ShowSkills()");
            ShowMusicProfilesCommon(musicPlayer);
        }

        public void ShowMusicProfiles(MusicPlayerComponent musicPlayer) {
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

        public void ShowDescription(AudioProfile musicProfile) {
            //Debug.Log("SkillTrainerUI.ShowDescription(" + skillName + ")");
            ClearDescription();

            if (musicProfile == null) {
                return;
            }
            currentMusicProfile = musicProfile;

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
                    musicPlayerHighlightButton.DeSelect();
                    objectPooler.ReturnObjectToPool(musicPlayerHighlightButton.gameObject);
                }
            }
            musicPlayerHighlightButtons.Clear();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("SkillTrainerUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            MySelectedMusicPlayerHighlightButton = null;
        }

        public void PlayMusic() {
            //Debug.Log("SkillTrainerUI.LearnSkill()");
            if (currentMusicProfile != null && currentMusicProfile.AudioClip != null) {
                audioManager.PlayMusic(currentMusicProfile.AudioClip);
            }
        }

        public void PauseMusic() {
            audioManager.PauseMusic();
        }

        public void StopMusic() {
            //Debug.Log("SkillTrainerUI.UnlearnSkill()");
            audioManager.StopMusic();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("SkillTrainerUI.OnOpenWindow()");
            // clear before open window handler, because it shows quests
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            DeactivateButtons();
            ClearDescription();
        }
    }

}