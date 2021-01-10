using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

        private void Awake() {
            //Debug.Log("NamePlateManager.Awake(): " + NamePlateManager.MyInstance.gameObject.name);
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
        }

        public void Start() {
            //List<GameObject> foundObjects = combatTextCanvas.transform.fi
            PopulateObjectPool();
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateCombatText();
        }

        public void LateUpdate() {
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyCameraControl == true
                && CameraManager.MyInstance.ThirdPartyCamera.activeInHierarchy == true
                && PlayerManager.MyInstance.PlayerUnitSpawned == true) {
                UpdateCombatText();
            }
        }

        private void UpdateCombatText() {
            foreach (CombatTextController combatTextController in inUseCombatTextControllers) {
                combatTextController.RunCombatTextUpdate();
            }
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
            StartCoroutine(ReturnAtEndOFFrame(combatTextController));
        }

        public IEnumerator ReturnAtEndOFFrame(CombatTextController combatTextController) {
            yield return new WaitForEndOfFrame();
            if (inUseCombatTextControllers.Contains(combatTextController)) {
                inUseCombatTextControllers.Remove(combatTextController);
                combatTextControllers.Add(combatTextController);
            }
            combatTextController.gameObject.SetActive(false);
        }

        public void SpawnCombatText(Interactable target, int damage, CombatTextType combatType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name + "; damage: " + damage + "; type: " + combatType);
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.Image.sprite = null;
                //Debug.Log("About to Set MainTarget on combat text");
                combatTextController.MainTarget = target;
                if (combatType == CombatTextType.miss) {
                    combatTextController.DisplayText = "(Miss)";
                } else if (combatType == CombatTextType.immune) {
                    combatTextController.DisplayText = "(Immune)";
                } else {
                    combatTextController.DisplayText = damage.ToString();
                }
                combatTextController.CombatMagnitude = combatMagnitude;
                combatTextController.CombatType = combatType;
                combatTextController.AbilityEffectContext = abilityEffectContext;
                combatTextController.InitializeCombatTextController();
            }
        }

        public void SpawnCombatText(Interactable target, StatusEffect statusEffect, bool gainEffect) {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            //GameObject _gameObject = Instantiate(combatTextPrefab, target.transform.position, Quaternion.identity, combatTextCanvas.transform);
            //Debug.Log("About to Set MainTarget on combat text");
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.MainTarget = target;
                combatTextController.Image.sprite = statusEffect.Icon;
                combatTextController.DisplayText = statusEffect.DisplayName;
                if (gainEffect) {
                    combatTextController.CombatType = CombatTextType.gainBuff;
                } else {
                    combatTextController.CombatType = CombatTextType.loseBuff;
                }
                combatTextController.InitializeCombatTextController();
            }
        }

        public void CleanupEventSubscriptions() {
            SystemEventManager.StopListening("AfterCameraUpdate", HandleAfterCameraUpdate);
        }

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }


    }

}