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

        private Dictionary<Color, Material> colorDictionary = new Dictionary<Color, Material>();

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
            colorDictionary[Color.red] = SystemConfigurationManager.MyInstance.MyRedFocusProjector;
            colorDictionary[Color.green] = SystemConfigurationManager.MyInstance.MyGreenFocusProjector;
            colorDictionary[Color.gray] = SystemConfigurationManager.MyInstance.MyGrayFocusProjector;
            colorDictionary[Color.yellow] = SystemConfigurationManager.MyInstance.MyYellowFocusProjector;
            colorDictionary[new Color32(255, 125, 0, 255)] = SystemConfigurationManager.MyInstance.MyOrangeFocusProjector;
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
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
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
                SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn() {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitSpawn()");
            PlayerManager.MyInstance.MyCharacter.MyCharacterController.OnSetTarget += HandleSetTarget;
            PlayerManager.MyInstance.MyCharacter.MyCharacterController.OnClearTarget += HandleClearTarget;
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("ActionBarmanager.HandlePlayerUnitDespawn()");
            PlayerManager.MyInstance.MyCharacter.MyCharacterController.OnSetTarget -= HandleSetTarget;
            PlayerManager.MyInstance.MyCharacter.MyCharacterController.OnClearTarget -= HandleClearTarget;
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
            if (characterUnit.MyCharacter.MyCharacterStats.IsAlive == false) {
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
            if (colorDictionary.ContainsKey(materialColor)) {
                targetingProjector.material = colorDictionary[materialColor];
                //Debug.Log("FocusTargettingController.SetMaterial(): dictionary contained color");
            } else {
                targetingProjector.material = SystemConfigurationManager.MyInstance.MyGrayFocusProjector;
                //Debug.Log("FocusTargettingController.SetMaterial(): dictionary did not contain color, setting to gray");
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
            if (characterUnit.MyCharacter.MyCharacterStats.IsAlive == false && targetingProjector.material != colorDictionary[Color.gray]) {
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