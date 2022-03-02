using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Scriptable Content Template", menuName = "AnyRPG/Scriptable Content Template")]
    [System.Serializable]
    public class ScriptableContentTemplate : DescribableResource {

        [Header("Scriptable Content")]

        [Tooltip("Resources to copy")]
        [SerializeField]
        private List<DescribableResource> resources = new List<DescribableResource>();

        [Tooltip("Resources to copy")]
        [SerializeField]
        private List<GameObject> prefabs = new List<GameObject>();

        [Header("Dependencies")]

        [Tooltip("Other scriptable content templates this template relies on")]
        [SerializeField]
        private List<ScriptableContentTemplate> dependencies = new List<ScriptableContentTemplate>();

        public List<DescribableResource> Resources { get => resources; set => resources = value; }
        public List<ScriptableContentTemplate> Dependencies { get => dependencies; set => dependencies = value; }
        public List<GameObject> Prefabs { get => prefabs; set => prefabs = value; }
    }

}