using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Game Extension", menuName = "AnyRPG/Game Extension")]
    public class GameExtension : DescribableResource {

        [Header("Don't Destroy On Load")]

        [Tooltip("A prefab that will be instantiated and set to DontDestroyOnLoad when the game starts.")]
        [SerializeField]
        private GameObject dontDestroyOnLoadPrefabs = null;

        public GameObject Prefab { get => dontDestroyOnLoadPrefabs; set => dontDestroyOnLoadPrefabs = value; }

    }

}