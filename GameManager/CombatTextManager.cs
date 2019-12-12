using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CombatTextManager : MonoBehaviour {

        #region Singleton
        private static CombatTextManager instance;

        public static CombatTextManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<CombatTextManager>();
                }

                return instance;
            }
        }

        #endregion

        [SerializeField]
        private GameObject combatTextPrefab;

        [SerializeField]
        private Canvas combatTextCanvas;

        private List<CombatTextController> combatTextControllers = new List<CombatTextController>();

        private List<CombatTextController> inUseCombatTextControllers = new List<CombatTextController>();

        public Canvas MyCombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }

        public void Start() {
            //List<GameObject> foundObjects = combatTextCanvas.transform.fi
            PopulateObjectPool();
        }

        public void PopulateObjectPool() {
            foreach (Transform child in combatTextCanvas.transform) {
                CombatTextController combatTextController = child.GetComponent<CombatTextController>();
                if (combatTextController != null) {
                    combatTextControllers.Add(combatTextController);
                }
            }
        }

        public CombatTextController GetCombatTextController() {
            CombatTextController returnValue = null;
            if (combatTextControllers.Count > 0) {
                returnValue = combatTextControllers[0];
                inUseCombatTextControllers.Add(combatTextControllers[0]);
                combatTextControllers.RemoveAt(0);
            } else {
                if (inUseCombatTextControllers.Count > 0) {
                    returnValue = inUseCombatTextControllers[0];
                    inUseCombatTextControllers.RemoveAt(0);
                    inUseCombatTextControllers.Add(returnValue);
                }
            }
            return returnValue;
        }

        public void returnControllerToPool(CombatTextController combatTextController) {
            if (inUseCombatTextControllers.Contains(combatTextController)) {
                inUseCombatTextControllers.Remove(combatTextController);
                combatTextControllers.Add(combatTextController);
            }
            combatTextController.gameObject.SetActive(false);
        }

        public void SpawnCombatText(GameObject target, int damage, CombatTextType combatType, CombatMagnitude combatMagnitude) {
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name + "; damage: " + damage + "; type: " + combatType);
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                //Debug.Log("About to Set MainTarget on combat text");
                combatTextController.MyMainTarget = target;
                if (combatType == CombatTextType.miss) {
                    combatTextController.MyDisplayText = "(Miss)";
                } else {
                    combatTextController.MyDisplayText = damage.ToString();
                }
                combatTextController.MyCombatMagnitude = combatMagnitude;
                combatTextController.MyCombatType = combatType;
                combatTextController.InitializeCombatTextController();
            }
        }

        public void SpawnCombatText(GameObject target, StatusEffect statusEffect, bool gainEffect) {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            //GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            //Debug.Log("About to Set MainTarget on combat text");
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.MyMainTarget = target;
                combatTextController.MyImage.sprite = statusEffect.MyIcon;
                combatTextController.MyDisplayText = statusEffect.MyName;
                if (gainEffect) {
                    combatTextController.MyCombatType = CombatTextType.gainBuff;
                } else {
                    combatTextController.MyCombatType = CombatTextType.loseBuff;
                }
                combatTextController.InitializeCombatTextController();
            }
        }

    }

}