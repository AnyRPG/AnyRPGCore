using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class FocusTargettingController : MonoBehaviour {

        [SerializeField]
        private Projector targetingProjector = null;

        [SerializeField]
        private Vector3 offset = Vector3.zero;

        private GameObject target = null;

        CharacterUnit characterUnit = null;

        protected bool eventSubscriptionsInitialized = false;

        //private Dictionary<Color, Material> colorDictionary = new Dictionary<Color, Material>();

        private Dictionary<string, Material> colorDictionary = new Dictionary<string, Material>();

        private Dictionary<string, Material> colorOverrideDictionary = new Dictionary<string, Material>();

        private Color circleColor;

        private float circleRadius = 0f;

        public Color MyCircleColor { get => circleColor; set => circleColor = value; }

        void Start() {
            SetupController();
            gameObject.SetActive(false);
        }


        public void SetupController() {
            /*
            if (targetingProjector != null) {
                targetingProjector.material = SystemConfigurationManager.MyInstance.MyDefaultFocusLightProjector;
                if (targetingProjector.material != null) {
                    circleColor = targetingProjector.material.color;
                }
            }
            */
            foreach (ProjectorColorMapNode colorMapNode in SystemConfigurationManager.MyInstance.MyFocusProjectorColorMap) {
                colorDictionary[ColorUtility.ToHtmlStringRGBA(colorMapNode.MySourceColor)] = colorMapNode.MyProjectorMaterial;
                //Debug.Log("FocusTargettingController.SetupController(): added " + ColorUtility.ToHtmlStringRGBA(colorMapNode.MySourceColor));
            }
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitSpawn()");
            PlayerManager.MyInstance.MyCharacter.CharacterController.OnSetTarget += HandleSetTarget;
            PlayerManager.MyInstance.MyCharacter.CharacterController.OnClearTarget += HandleClearTarget;
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitDespawn()");
            PlayerManager.MyInstance.MyCharacter.CharacterController.OnSetTarget -= HandleSetTarget;
            PlayerManager.MyInstance.MyCharacter.CharacterController.OnClearTarget -= HandleClearTarget;
        }

        public void HandleSetTarget(GameObject target) {
            //Debug.Log("FocusTargettingController.HandleSetTarget()");
            characterUnit = target.GetComponent<CharacterUnit>();
            if (characterUnit == null) {
                // don't show this under inanimate units
                HandleClearTarget();
                return;
            }
            this.target = target;
            gameObject.SetActive(true);
            colorOverrideDictionary.Clear();
            if (characterUnit.MyCharacter.CharacterStats.MyToughness != null && characterUnit.MyCharacter.CharacterStats.MyToughness.MyFocusProjectorOverrideMap != null) {
                foreach (ProjectorColorMapNode colorMapNode in characterUnit.MyCharacter.CharacterStats.MyToughness.MyFocusProjectorOverrideMap) {
                    colorOverrideDictionary[ColorUtility.ToHtmlStringRGBA(colorMapNode.MySourceColor)] = colorMapNode.MyProjectorMaterial;
                    //Debug.Log("FocusTargettingController.SetupController(): added override " + ColorUtility.ToHtmlStringRGBA(colorMapNode.MySourceColor));
                }
            }

            if (characterUnit.MyCharacter.CharacterStats.IsAlive == false) {
                //SetCircleColor(Color.gray);
                SetMaterial(Color.gray);
            } else {
                Color newColor = Faction.GetFactionColor(characterUnit);
                //SetCircleColor(newColor);
                SetMaterial(newColor);
            }

            float hitBoxSize = characterUnit.MyHitBoxSize;
            SetCircleRadius(hitBoxSize * 2f);
        }

        public void SetMaterial(Color materialColor) {
            //Debug.Log("FocusTargettingController.SetMaterial(" + (materialColor == null ? "null" : materialColor.ToString()) + ")");
            foreach (string tmpColor in colorDictionary.Keys) {
                //Debug.Log("Dictionary contains key: " + tmpColor.ToString());
            }
            if (colorOverrideDictionary.ContainsKey(ColorUtility.ToHtmlStringRGBA(materialColor))) {
                targetingProjector.material = colorOverrideDictionary[ColorUtility.ToHtmlStringRGBA(materialColor)];
                //Debug.Log("FocusTargettingController.SetMaterial(): override dictionary contained color  " + ColorUtility.ToHtmlStringRGBA(materialColor));
            } else {
                if (colorDictionary.ContainsKey(ColorUtility.ToHtmlStringRGBA(materialColor))) {
                    targetingProjector.material = colorDictionary[ColorUtility.ToHtmlStringRGBA(materialColor)];
                    //Debug.Log("FocusTargettingController.SetMaterial(): dictionary contained color  " + ColorUtility.ToHtmlStringRGBA(materialColor));
                } else {
                    targetingProjector.material = SystemConfigurationManager.MyInstance.MyDefaultCastingLightProjector;
                    //Debug.Log("FocusTargettingController.SetMaterial(): dictionary did not contain color " + ColorUtility.ToHtmlStringRGBA(materialColor) + ", setting to default casting projector");
                }
            }
        }

        public void HandleClearTarget() {
            //Debug.Log("ActionBarmanager.HandleClearTarget()");
            gameObject.SetActive(false);
            target = null;
        }


        void Update() {
            //Debug.Log("CastTargettingController.Update()");
            if (target == null) {
                HandleClearTarget();
                return;
            }
            FollowTarget();
        }

        /*
        private void SetOutOfRange(bool outOfRange) {
            //Debug.Log("CastTargettingController.HandleOutOfRange()");
            if (outOfRange == true) {
                if (castTargettingProjector.enabled) {
                    castTargettingProjector.enabled = false;
                }
            } else {
                if (!castTargettingProjector.enabled) {
                    castTargettingProjector.enabled = true;
                }
            }
        }
        */

        private void FollowTarget() {
            //Debug.Log("CastTargettingController.FollowMouse()");

            this.transform.position = new Vector3(target.transform.position.x, target.transform.position.y + 1, target.transform.position.z);
            this.transform.forward = target.transform.forward;
            transform.Rotate(new Vector3(90f, 0f, 0f));
            if (characterUnit.MyCharacter.CharacterStats.IsAlive == false && targetingProjector.material != colorDictionary[ColorUtility.ToHtmlStringRGBA(Color.gray)]) {
                //SetCircleColor(Color.gray);
                SetMaterial(Color.gray);
            }
        }

        public void SetCircleColor(Color newColor) {
            //Debug.Log("CastTargettingController.SetCircleColor()");
            circleColor = newColor;
            targetingProjector.material.color = circleColor;
        }

        public void SetCircleRadius(float newRadius) {
            //Debug.Log("CastTargettingController.SetCircleRadius()");
            circleRadius = newRadius;
            targetingProjector.orthographicSize = circleRadius;
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();

        }


    }

}