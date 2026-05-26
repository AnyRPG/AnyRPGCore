using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CombatTextManager : ConfiguredMonoBehaviour {

        [SerializeField]
        private GameObject combatTextPrefab;

        [SerializeField]
        private Canvas combatTextCanvas;

        //private List<CombatTextController> combatTextControllers = new List<CombatTextController>();

        private List<CombatTextController> inUseCombatTextControllers = new List<CombatTextController>();
        private List<CombatTextController> returnList = new List<CombatTextController>();

        private Dictionary<Interactable, List<CombatTextController>[]> quadrantTracks = new Dictionary<Interactable, List<CombatTextController>[]>();

        // game manager references
        private CameraManager cameraManager = null;
        private ObjectPooler objectPooler = null;
        private CutsceneBarController cutSceneBarController = null;
        private LevelManagerClient levelManagerClient = null;

        public Canvas CombatTextCanvas { get => combatTextCanvas; set => combatTextCanvas = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //Debug.Log("NameplateManager.Awake(): " + SystemGameManager.Instance.UIManager.NameplateManager.gameObject.name);
            SystemEventManager.StartListening("AfterCameraUpdate", HandleAfterCameraUpdate);
            levelManagerClient.OnLevelUnload += HandleLevelUnload;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            cameraManager = systemGameManager.CameraManager;
            objectPooler = systemGameManager.ObjectPooler;
            cutSceneBarController = systemGameManager.UIManager.CutSceneBarController;
            levelManagerClient = systemGameManager.LevelManagerClient;
        }

        public void HandleAfterCameraUpdate(string eventName, EventParamProperties eventParamProperties) {
            UpdateCombatText();
        }

        /*
        public void LateUpdate() {
            if (systemConfigurationManager.UseThirdPartyCameraControl == true
                && cameraManager.ThirdPartyCamera.activeInHierarchy == true
                && playerManager.PlayerUnitSpawned == true) {
                UpdateCombatText();
            }
        }
        */

        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            List<CombatTextController> removeList = new List<CombatTextController>();
            removeList.AddRange(inUseCombatTextControllers);
            foreach (CombatTextController combatTextController in removeList) {
                ReturnControllerToPool(combatTextController);
            }
        }

        private void UpdateCombatText() {
            if (cameraManager.ActiveMainCamera == null) {
                return;
            }
            if (cutSceneBarController.CurrentCutscene != null) {
                return;
            }
            foreach (CombatTextController combatTextController in inUseCombatTextControllers) {
                combatTextController.RunCombatTextUpdate();
            }
            if (returnList.Count > 0) {
                foreach (CombatTextController combatTextController in returnList) {
                    ReturnControllerToPool(combatTextController);
                }
                returnList.Clear();
            }
        }

        public CombatTextController GetCombatTextController() {
            GameObject pooledObject = objectPooler.GetPooledObject(combatTextPrefab, combatTextCanvas.transform);
            if (pooledObject != null) {
                return pooledObject.GetComponent<CombatTextController>();
            }

            return null;
        }

        /// <summary>
        /// wait until the end of the frame and then return the object to the pool to avoid modifying the collection in the foreach loop
        /// </summary>
        /// <param name="combatTextController"></param>
        public void RequestReturnControllerToPool(CombatTextController combatTextController) {
            returnList.Add(combatTextController);
        }

        public void ReturnControllerToPool(CombatTextController combatTextController) {
            inUseCombatTextControllers.Remove(combatTextController);
            objectPooler.ReturnObjectToPool(combatTextController.gameObject);
        }

        private string GetDisplayText(CombatTextType combatType, int damage) {
            if (combatType == CombatTextType.miss) {
                return "(Miss)";
            } else if (combatType == CombatTextType.immune) {
                return "(Immune)";
            } else {
                return damage.ToString();
            }
        }

        public void SpawnCombatText(Interactable target, int damage, CombatTextType combatType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"CombatTextManager.SpawnCombatText({target.gameObject.name}, {damage}, {combatType}, {combatMagnitude})");

            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.Configure(systemGameManager);
                //Debug.Log("About to Set MainTarget on combat text");
                inUseCombatTextControllers.Add(combatTextController);
                /*
                Rect rectTransform = combatTextController.RectTransform.rect;
                rectTransform.width = 0;
                rectTransform.height = 0;
                */
                combatTextController.RectTransform.rect.Set(combatTextController.RectTransform.rect.x, combatTextController.RectTransform.rect.y, 0, 0);
                combatTextController.InitializeCombatTextController(target,
                    (combatType == CombatTextType.gainBuff || combatType == CombatTextType.loseBuff) ? abilityEffectContext.AbilityEffect.Icon : null,
                    (combatType == CombatTextType.gainBuff || combatType == CombatTextType.loseBuff) ? abilityEffectContext.AbilityEffect.DisplayName : GetDisplayText(combatType, damage),
                    combatType,
                    combatMagnitude,
                    abilityEffectContext
                    );
            }
        }

        /*
        public void SpawnCombatText(Interactable target, StatusEffectProperties statusEffect, bool gainEffect) {
            //Debug.Log($"CombatTextManager.SpawnCombatText({target.gameObject.name}, {statusEffect.ResourceName}, {gainEffect})");

            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            //Debug.Log("About to Set MainTarget on combat text");
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.Configure(systemGameManager);
                inUseCombatTextControllers.Add(combatTextController);
                combatTextController.InitializeCombatTextController(target,
                    statusEffect.Icon,
                    statusEffect.DisplayName,
                    (gainEffect == true ? CombatTextType.gainBuff : CombatTextType.loseBuff)
                    );
            }
        }
        */

        public void SpawnCombatText(Interactable target, string displayText, CombatTextType combatType) {
            //Debug.Log($"CombatTextManager.SpawnCombatText({target.gameObject.name}, {displayText}, {combatType})");
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                return;
            }
            //Debug.Log("Combat Text manager Spawning Combat Text attached to: " + target.name);
            //Debug.Log("About to Set MainTarget on combat text");
            CombatTextController combatTextController = GetCombatTextController();
            if (combatTextController != null) {
                combatTextController.Configure(systemGameManager);
                inUseCombatTextControllers.Add(combatTextController);
                combatTextController.InitializeCombatTextController(target,
                    null,
                    displayText,
                    combatType
                    );
            }
        }

        public void RegisterAndPush(Interactable target, CombatTextController newText, int quadrant, float pushAmount) {
            //Debug.Log($"CombatTextManager.RegisterAndPush({target.gameObject.name}, text: {newText.gameObject.name}, quadrant: {quadrant}, pushAmount: {pushAmount})");

            if (!quadrantTracks.ContainsKey(target)) {
                // Initialize 4 lists, one for each quadrant
                quadrantTracks[target] = new List<CombatTextController>[4] {
                    new List<CombatTextController>(),
                    new List<CombatTextController>(),
                    new List<CombatTextController>(),
                    new List<CombatTextController>()
                 };
            }

            List<CombatTextController> track = quadrantTracks[target][quadrant];

            if (track.Count > 0) {
                // Get the most recently spawned text (the one closest to the spawn point)
                CombatTextController lastText = track[track.Count - 1];

                // Check how far it has moved from the origin (0,0)
                // We use the same 'finalY' logic: yUIOffset + currentVisualPush
                float currentPos = lastText.GetCurrentVerticalDisplacement();

                // If the last text hasn't cleared enough room for the NEW text's height...
                if (Mathf.Abs(currentPos) < pushAmount) {
                    // Calculate how much extra we need to shove the whole stack
                    float extraShove = pushAmount - Mathf.Abs(currentPos);

                    foreach (var active in track) {
                        active.PushUp(extraShove);
                    }
                }
            }

            track.Add(newText);
        }

        public void Unregister(Interactable target, CombatTextController text, int quadrant) {
            //Debug.Log($"CombatTextManager.Unregister({target.gameObject.name}, {text.gameObject.name}, quadrant: {quadrant})");

            if (quadrantTracks.ContainsKey(target)) {
                quadrantTracks[target][quadrant].Remove(text);
            }
        }


    }
}