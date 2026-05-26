using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Game Extension", menuName = "AnyRPG/Game Extension")]
    public class GameExtension : DescribableResource {

        [Header("Don't Destroy On Load")]

        [Tooltip("A list of prefabs that will be instantiated and set to DontDestroyOnLoad when the game starts.")]
        [SerializeField]
        private List<GameObject> dontDestroyOnLoadPrefabsList = new List<GameObject>();

        [Tooltip("If set, an extension to override.")]
        [ResourceSelector(resourceType = typeof(GameExtension))]
        [SerializeField]
        private string overrideExtension = string.Empty;

        public List<GameObject> PrefabList { get => dontDestroyOnLoadPrefabsList; set => dontDestroyOnLoadPrefabsList = value; }
        public string OverrideExtension { get => overrideExtension; set => overrideExtension = value; }

    }

}