using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New SceneNode", menuName = "SceneNodes/SceneNode")]
[System.Serializable]
public class SceneNode : DescribableResource {

    [SerializeField]
    private Vector3 defaultSpawnPosition = Vector3.zero;

    [SerializeField]
    private AudioClip ambientAudioClip;

    [SerializeField]
    private AudioClip musicAudioClip;

    [SerializeField]
    private bool suppressCharacterSpawn;

    [SerializeField]
    private bool suppressMainCamera;

    [SerializeField]
    private bool isCutScene;

    [SerializeField]
    private bool allowCutSceneNamePlates;

    [SerializeField]
    private bool cutsceneViewed;

    public string MySceneName { get => resourceName; set => resourceName = value; }
    public Vector3 MyDefaultSpawnPosition { get => defaultSpawnPosition; set => defaultSpawnPosition = value; }
    public AudioClip MyAmbientAudioClip { get => ambientAudioClip; set => ambientAudioClip = value; }
    public AudioClip MyMusicAudioClip { get => musicAudioClip; set => musicAudioClip = value; }
    public bool MySuppressCharacterSpawn { get => suppressCharacterSpawn; set => suppressCharacterSpawn = value; }
    public bool MySuppressMainCamera { get => suppressMainCamera; set => suppressMainCamera = value; }
    public bool MyCutsceneViewed { get => cutsceneViewed; set => cutsceneViewed = value; }
    public bool MyIsCutScene { get => isCutScene; set => isCutScene = value; }
}

}