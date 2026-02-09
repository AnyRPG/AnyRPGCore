using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemAbilityController : ConfiguredMonoBehaviour, IAbilityCaster {

        private AbilityManager abilityManager = null;

        /// <summary>
        /// sceneHandle, gameObject
        /// </summary>
        protected Dictionary<int, List<GameObject>> abilityEffectGameObjects = new Dictionary<int, List<GameObject>>();

        /// <summary>
        /// gameobject, scene handle
        /// </summary>
        private Dictionary<GameObject, int> abilityEffectGameObjectLookup = new Dictionary<GameObject, int>();

        /// <summary>
        /// sceneHandle, coroutine
        /// </summary>
        protected Dictionary<int, List<Coroutine>> destroyAbilityEffectObjectCoroutines = new Dictionary<int, List<Coroutine>>();


        private Dictionary<GameObject, Coroutine> coroutineLookup = new Dictionary<GameObject, Coroutine>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private NetworkManagerServer networkManagerServer = null;

        public IAbilityManager AbilityManager { get => abilityManager; }
        public MonoBehaviour MonoBehaviour { get => this; }

        public override  void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            abilityManager = new AbilityManager(this, systemGameManager);
            systemEventManager.OnLevelUnloadClient += HandleLevelUnload;
            systemEventManager.OnLevelUnloadServer += HandleLevelUnload;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public virtual void AddDestroyAbilityEffectObjectCoroutine(int sceneHandle, Coroutine coroutine) {
            if (destroyAbilityEffectObjectCoroutines.ContainsKey(sceneHandle) == false) {
                destroyAbilityEffectObjectCoroutines[sceneHandle] = new List<Coroutine>();
            }
            destroyAbilityEffectObjectCoroutines[sceneHandle].Add(coroutine);
        }

        // ensure that no coroutine continues or other spell effects exist past the end of a level
        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            //Debug.Log($"SystemAbilityController.HandleLevelUnload({sceneHandle}, {sceneName})");

            if (destroyAbilityEffectObjectCoroutines.ContainsKey(sceneHandle) == true) {
                foreach (Coroutine coroutine in destroyAbilityEffectObjectCoroutines[sceneHandle]) {
                    StopCoroutine(coroutine);
                }
                destroyAbilityEffectObjectCoroutines.Remove(sceneHandle);
            }

            if (abilityEffectGameObjects.ContainsKey(sceneHandle) == true) {
                foreach (GameObject go in abilityEffectGameObjects[sceneHandle]) {
                    abilityEffectGameObjectLookup.Remove(go);
                    if (go != null) {
                        objectPooler.ReturnObjectToPool(go);
                    }
                }
                abilityEffectGameObjects.Remove(sceneHandle);
            }
        }

        public void BeginDestroyAbilityEffectObject(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffectProperties fixedLengthEffect) {
            //Debug.Log($"SystemAbilityController.BeginDestroyAbilityEffectObject(objectCount: {abilityEffectObjects.Count}, {(source == null ? "null" : source.AbilityManager.Name)}, {(target == null ? "null" : target.gameObject.name)}, {timer}, {fixedLengthEffect.ResourceName})");

            if (abilityEffectGameObjects.ContainsKey(source.gameObject.scene.handle) == false) {
                abilityEffectGameObjects.Add(source.gameObject.scene.handle, new List<GameObject>());
            }
            foreach (List<GameObject> gameObjectList in abilityEffectObjects.Values) {
                foreach (GameObject go in gameObjectList) {
                    //Debug.Log($"SystemAbilityController.BeginDestroyAbilityEffectObject(objectCount: {abilityEffectObjects.Count}, {(source == null ? "null" : source.AbilityManager.Name)}, {(target == null ? "null" : target.gameObject.name)}, {timer}, {fixedLengthEffect.ResourceName}) tracking : {go.name} ({go.GetInstanceID()}) scene handle: {source.gameObject.scene.handle}");
                    abilityEffectGameObjects[source.gameObject.scene.handle].Add(go);
                    abilityEffectGameObjectLookup.Add(go, source.gameObject.scene.handle);
                }
            }
            Coroutine coroutine = StartCoroutine(DestroyAbilityEffectObject(abilityEffectObjects, source, target, timer, abilityEffectInput, fixedLengthEffect));
            AddDestroyAbilityEffectObjectCoroutine(source.gameObject.scene.handle, coroutine);
            if (timer > 0f) {
                foreach (List<GameObject> gameObjectList in abilityEffectObjects.Values) {
                    foreach (GameObject go in gameObjectList) {
                        coroutineLookup.Add(go, coroutine);
                    }
                }
            }
        }

        public void CancelDestroyAbilityEffectObject(GameObject abilityEffectObject) {
            //Debug.Log($"SystemAbilityController.CancelDestroyAbilityEffectObject({abilityEffectObject.name} ({abilityEffectObject.GetInstanceID()})) scene handle: {abilityEffectObject.scene.handle}");

            if (coroutineLookup.ContainsKey(abilityEffectObject)) {
                //Debug.Log($"SystemAbilityController.CancelDestroyAbilityEffectObject({abilityEffectObject.name} ({abilityEffectObject.GetInstanceID()})) coroutine existed");
                Coroutine coroutine = coroutineLookup[abilityEffectObject];
                StopCoroutine(coroutine);
                coroutineLookup.Remove(abilityEffectObject);
            }
            if (abilityEffectGameObjectLookup.ContainsKey(abilityEffectObject)) {
                if (abilityEffectGameObjects.ContainsKey(abilityEffectGameObjectLookup[abilityEffectObject])) {
                    abilityEffectGameObjects[abilityEffectGameObjectLookup[abilityEffectObject]].Remove(abilityEffectObject);
                }
                abilityEffectGameObjectLookup.Remove(abilityEffectObject);
            }
        }

        public IEnumerator DestroyAbilityEffectObject(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffectProperties fixedLengthEffect) {
            //Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(objectCount: {abilityEffectObjects.Count}, {(source == null ? "null" : source.AbilityManager.Name)}, {(target == null ? "null" : target.gameObject.name)}, {timer}, {fixedLengthEffect.ResourceName})");

            float timeRemaining = timer;
            // debug print instance ids of the objects we are about to destroy
            /*
            foreach (List<GameObject> gameObjectList in abilityEffectObjects.Values) {
                foreach (GameObject go in gameObjectList) {
                    if (go != null) {
                        //Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(): starting timer for {go.name} {go.GetInstanceID()}");
                    }
                }
            }
            //Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(): fixedLengthEffect: {(fixedLengthEffect == null ? "null" : fixedLengthEffect.ResourceName)}");
            */

            // keep track of temporary elapsed time between ticks
            float elapsedTime = 0f;

            bool nullTarget = false;
            CharacterStats targetStats = null;
            if (target != null) {
                CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(target);
                if (_characterUnit != null) {
                    targetStats = _characterUnit.UnitController.CharacterStats;
                }
            } else {
                nullTarget = true;
            }

            int milliseconds = (int)((fixedLengthEffect.TickRate - (int)fixedLengthEffect.TickRate) * 1000);
            float finalTickRate = fixedLengthEffect.TickRate;
            if (finalTickRate == 0) {
                finalTickRate = timer + 1;
            }
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() milliseconds: " + milliseconds);
            //TimeSpan tickRateTimeSpan = new TimeSpan(0, 0, 0, (int)finalTickRate, milliseconds);
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() tickRateTimeSpan: " + tickRateTimeSpan);
            //fixedLengthEffect.MyNextTickTime = System.DateTime.Now + tickRateTimeSpan;
            //Debug.Log(abilityEffectName + ".FixedLengthEffect.Tick() nextTickTime: " + nextTickTime);

            while (timeRemaining > 0f) {
                yield return null;

                if (nullTarget == false && (targetStats == null || fixedLengthEffect == null)) {
                    //Debug.Log("SystemAbilityController.DestroyAbilityEffectObject: BREAKING!: fixedLengthEffect: " + (fixedLengthEffect == null ? "null" : fixedLengthEffect.DisplayName) + "; targetstats: " + (targetStats == null ? "null" : targetStats.BaseCharacter.CharacterName));
                    break;
                }

                if (fixedLengthEffect.PrefabSpawnLocation != PrefabSpawnLocation.GroundTarget
                    && fixedLengthEffect.GetTargetOptions(source).RequireTarget == true
                    && (target == null || (targetStats.IsAlive == true && fixedLengthEffect.GetTargetOptions(source).RequireDeadTarget == true) || (targetStats.IsAlive == false && fixedLengthEffect.GetTargetOptions(source).RequireLiveTarget == true))) {
                    //Debug.Log("BREAKING!!!!!!!!!!!!!!!!!");
                    break;
                } else {
                    timeRemaining -= Time.deltaTime;
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime > finalTickRate) {
                        //Debug.Log(fixedLengthEffect.DisplayName + ".FixedLengthEffect.Tick() TickTime!");
                        if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                            fixedLengthEffect.CastTick(source, target, abilityEffectInput);
                        }
                        elapsedTime -= finalTickRate;
                    }
                }
            }
            //Debug.Log(fixedLengthEffect.DisplayName + ".FixedLengthEffect.Tick() Done ticking and about to perform ability affects.");
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                fixedLengthEffect.CastComplete(source, target, abilityEffectInput);
            }
            foreach (List<GameObject> gameObjectList in abilityEffectObjects.Values) {
                foreach (GameObject go in gameObjectList) {
                    if (abilityEffectGameObjectLookup.ContainsKey(go)) {
                        if (abilityEffectGameObjects.ContainsKey(abilityEffectGameObjectLookup[go])) {
                            abilityEffectGameObjects[abilityEffectGameObjectLookup[go]].Remove(go);
                        }
                        abilityEffectGameObjectLookup.Remove(go);
                        coroutineLookup.Remove(go);
                    }
                    if (go != null) {
                        //Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(objectCount: {abilityEffectObjects.Count}, {(source == null ? "null" : source.AbilityManager.Name)}, {(target == null ? "null" : target.gameObject.name)}, {timer}, {fixedLengthEffect.ResourceName}) DESTROYING AT END OF TIMER : {go.name} ({go.GetInstanceID()})");
                        objectPooler.ReturnObjectToPool(go, fixedLengthEffect.PrefabDestroyDelay);
                        // since effects can persist beyond death (like aoe clouds etc), the source could be dead or despawned by the time we get here
                        if (source?.AbilityManager != null) {
                            source.AbilityManager.ProcessAbilityEffectPooled(go);
                        }
                    }
                }
            }

            // this should not be necessary because it's a local variable
            //abilityEffectObjects.Clear();
        }

        public static string GetTimeText(float durationSeconds) {
            string returnText = string.Empty;
            if (durationSeconds < 60f && durationSeconds >= 0f) {
                // less than 1 minute
                returnText = ((int)durationSeconds).ToString() + " second";
                if ((int)durationSeconds != 1) {
                    returnText += "s";
                }
            } else if (durationSeconds < 3600) {
                //less than 1 hour
                returnText = ((int)(durationSeconds / 60)).ToString() + " minute";
                if (((int)durationSeconds / 60) != 1) {
                    returnText += "s";
                }
            } else if (durationSeconds > 3600f) {
                //greater than 1 hour
                returnText = ((int)(durationSeconds / 3600)).ToString() + " hour";
                if (((int)durationSeconds / 3600) != 1) {
                    returnText += "s";
                }
            }
            return returnText;
        }

        public void OnDestroy() {
            StopAllCoroutines();
            destroyAbilityEffectObjectCoroutines.Clear();

            //systemEventManager.OnLevelUnloadClient -= HandleLevelUnload;
            //systemEventManager.OnLevelUnloadClient -= HandleLevelUnload;
        }
    }

}
