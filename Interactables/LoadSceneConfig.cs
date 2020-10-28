using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Load Scene Config", menuName = "AnyRPG/Interactable/LoadSceneConfig")]
    public class LoadSceneConfig : PortalConfig {

        [SerializeField]
        private LoadSceneProps interactableOptionProps = new LoadSceneProps();

        [Header("Scene Options")]

        [Tooltip("When interacted with, this scene will load directly.")]
        [SerializeField]
        private string sceneName = string.Empty;

        public string SceneName { get => sceneName; set => sceneName = value; }

    }

}