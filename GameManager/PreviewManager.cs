using AnyRPG;
using System.Collections;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;


namespace AnyRPG {
    public abstract class PreviewManager : MonoBehaviour {

        [SerializeField]
        protected GameObject previewUnit;

        [SerializeField]
        protected Vector3 previewSpawnLocation;

        [Tooltip("The name of the layer to set the preview unit to")]
        [SerializeField]
        protected string layerName;

        protected int previewLayer;

        // the source we are going to clone from 
        protected GameObject cloneSource;

        public GameObject PreviewUnit { get => previewUnit; set => previewUnit = value; }
        public int PreviewLayer { get => previewLayer; set => previewLayer = value; }

        protected void Awake() {
            previewLayer = LayerMask.NameToLayer(layerName);
        }

        protected void Start() {
            if (previewSpawnLocation == null) {
                previewSpawnLocation = Vector3.zero;
            }
        }

        public void HandleCloseWindow() {
            //Debug.Log("CharacterCreatorManager.HandleCloseWindow();");
            if (previewUnit != null) {
                Destroy(previewUnit);
            }
        }

        public virtual GameObject GetCloneSource() {
            // override this in all child classes
            return null;
        }

        public void OpenWindowCommon() {
            previewUnit = Instantiate(cloneSource, transform.position, Quaternion.identity, transform);
            UIManager.MyInstance.SetLayerRecursive(previewUnit, previewLayer);

            // disable any components on the cloned unit that may give us trouble since this unit cannot move
            MonoBehaviour[] monoBehaviours = previewUnit.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour monoBehaviour in monoBehaviours) {
                //Debug.Log("CharacterCreatorManager.HandleOpenWindow(): disable monobehavior: " + monoBehaviour.GetType().Name);
                monoBehaviour.enabled = false;
            }

            // NavMeshAgent is technically not a Monobehavior so has to be handled separately
            if (previewUnit.GetComponent<NavMeshAgent>() != null) {
                previewUnit.GetComponent<NavMeshAgent>().enabled = false;
            }

            // prevent the character from enabling the navmeshagent
            if (previewUnit.GetComponent<BaseCharacter>() != null) {
                previewUnit.GetComponent<BaseCharacter>().PreviewCharacter = true;
            }

            // re-enable behaviors needed for character animation
            if (previewUnit.GetComponent<DynamicCharacterAvatar>() != null) {
                previewUnit.GetComponent<DynamicCharacterAvatar>().enabled = true;
            }
            if (previewUnit.GetComponent<CharacterAnimator>() != null) {
                previewUnit.GetComponent<CharacterAnimator>().enabled = true;
            }
            if (previewUnit.GetComponent<AnimatedUnit>() != null) {
                previewUnit.GetComponent<AnimatedUnit>().enabled = true;
            }

            // prevent preview unit from moving around
            if (previewUnit.GetComponent<Rigidbody>() != null) {
                previewUnit.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                previewUnit.GetComponent<Rigidbody>().isKinematic = true;
                previewUnit.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                previewUnit.GetComponent<Rigidbody>().useGravity = false;
            }


            AIEquipmentManager aIEquipmentManager = previewUnit.AddComponent<AIEquipmentManager>();
        }

    }
}