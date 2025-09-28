using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AbilityManager : ConfiguredClass, IAbilityManager {

        protected AbilityProperties currentCastAbility = null;

        protected Coroutine globalCoolDownCoroutine = null;
        protected Coroutine currentCastCoroutine = null;
        protected Coroutine abilityHitDelayCoroutine = null;

        protected bool eventSubscriptionsInitialized = false;

        protected Dictionary<string, AbilityCoolDownNode> abilityCoolDownDictionary = new Dictionary<string, AbilityCoolDownNode>();

        protected IAbilityCaster abilityCaster = null;
        protected MonoBehaviour abilityCasterMonoBehaviour = null;

        // game manager references
        protected ObjectPooler objectPooler = null;
        protected LevelManager levelManager = null;

        public virtual bool ControlLocked {
            get {
                return false;
            }
        }

        public virtual bool IsOnCoolDown(string abilityName) {
            return abilityCoolDownDictionary.ContainsKey(abilityName);
        }

        public virtual void AddAbilityObject(AbilityAttachmentNode abilityAttachmentNode, GameObject go) {
            // do nothing
        }

        public virtual void AddAbilityEffectObject(AbilityAttachmentNode abilityAttachmentNode, GameObject go) {
            // do nothing
        }

        public virtual GameObject UnitGameObject {
            get {
                if (abilityCasterMonoBehaviour != null) {
                    return abilityCasterMonoBehaviour.gameObject;
                }
                return null;
            }
        }

        public virtual bool PerformingAbility {
            get {
                return false;
            }
        }

        // for now, all environmental effects will calculate their ability damage as if they were level 1
        public virtual int Level {
            get {
                return 1;
            }
        }

        public virtual string Name {
            get {
                return string.Empty;
            }
        }

        public virtual Dictionary<string, AbilityProperties> RawAbilityList {
            get => new Dictionary<string, AbilityProperties>();
        }

        public AbilityProperties CurrentCastAbility { get => currentCastAbility; }

        public AbilityManager(IAbilityCaster abilityCaster, SystemGameManager systemGameManager) {
            this.abilityCaster = abilityCaster;
            this.abilityCasterMonoBehaviour = abilityCaster.MonoBehaviour;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            levelManager = systemGameManager.LevelManager;
        }

        public virtual CharacterUnit GetCharacterUnit() {
            return null;
        }

        public virtual void SummonMount(UnitProfile mountUnitProfile) {
            // nothing here for now
        }

        public virtual void AddTemporaryPet(UnitProfile unitProfile, UnitController unitController) {
            // nothing here for now
        }

        public virtual AttachmentPointNode GetHeldAttachmentPointNode(AbilityAttachmentNode attachmentNode) {
            if (attachmentNode.UseUniversalAttachment == false) {
                AttachmentPointNode attachmentPointNode = new AttachmentPointNode();
                attachmentPointNode.TargetBone = attachmentNode.HoldableObject.TargetBone;
                attachmentPointNode.Position = attachmentNode.HoldableObject.Position;
                attachmentPointNode.Rotation = attachmentNode.HoldableObject.Rotation;
                attachmentPointNode.RotationIsGlobal = attachmentNode.HoldableObject.RotationIsGlobal;
                return attachmentPointNode;
            }

            return null;
        }

        public virtual List<AbilityEffectProperties> GetDefaultHitEffects() {
            return new List<AbilityEffectProperties>();
        }

        public virtual List<AbilityAttachmentNode> GetWeaponAbilityAnimationObjectList() {
            return null;
        }

        public virtual List<AbilityAttachmentNode> GetWeaponAbilityObjectList() {
            return null;
        }


        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public virtual bool CanCastAbility(AbilityProperties ability, bool playerInitiated = false) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + ")");

            return true;
        }

        public virtual void GeneratePower(AbilityProperties ability) {
            // do nothing
        }

        public virtual AudioClip GetAnimatedAbilityHitSound() {
            return null;
        }

        public virtual List<AnimationClip> GetDefaultAttackAnimations() {
            return new List<AnimationClip>();
        }

        public virtual List<AnimationClip> GetUnitAttackAnimations() {
            return new List<AnimationClip>();
        }

        public virtual List<AnimationClip> GetUnitCastAnimations() {
            return new List<AnimationClip>();
        }

        public virtual AnimationProps GetUnitAnimationProps() {
            return null;
        }

        public virtual bool PerformLOSCheck(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            return true;
        }

        public virtual bool HasAbility(AbilityProperties baseAbility) {
            
            return false;
        }

        public virtual bool HasAbility(string abilityName) {

            return false;
        }

        public virtual float GetMeleeRange() {
            return 1f;
        }

        public void BeginPerformAbilityHitDelay(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput, ChanneledEffectProperties channeledEffect) {
            abilityHitDelayCoroutine = abilityCasterMonoBehaviour.StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput, channeledEffect));
        }

        public IEnumerator PerformAbilityHitDelay(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectInput, ChanneledEffectProperties channeledEffect) {
            //Debug.Log("ChanelledEffect.PerformAbilityEffectDelay()");
            float timeRemaining = channeledEffect.effectDelay;
            while (timeRemaining > 0f) {
                yield return null;
                timeRemaining -= Time.deltaTime;
            }
            if (target == null || (target != null && target.gameObject.activeSelf == true)) {
                channeledEffect.PerformAbilityHit(source, target, abilityEffectInput);
            }
            abilityHitDelayCoroutine = null;
        }

        

        public virtual bool AddToAggroTable(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            return false;
        }

        public virtual void GenerateAgro(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            // do nothing for now
        }

        public virtual void ProcessWeaponHitEffects(AttackEffectProperties attackEffect, Interactable target, AbilityEffectContext abilityEffectOutput) {
            // do nothing.  There is no weapon on the base class
        }

        public virtual void CapturePet(UnitController targetUnitController) {
            // do nothing.  environment effects cannot have pets
        }

        public virtual void PerformCastingAnimation(AbilityProperties baseAbility) {
            // do nothing.  environmental effects have no animations for now
        }

        public virtual void DespawnAbilityObjects() {
            // do nothing
        }

        public virtual void InitiateGlobalCooldown(float coolDownToUse) {
            // do nothing
        }

        public virtual void BeginAbilityCoolDown(AbilityProperties baseAbility, float coolDownLength = -1f) {
            // do nothing
        }

        public virtual void BeginActionCoolDown(InstantiatedActionItem actionItem, float coolDownLength = -1f) {
            // do nothing
        }

        /// <summary>
        /// This is the entrypoint for character behavior calls and should not be used for anything else due to the runtime ability lookup that happens
        /// </summary>
        /// <param name="abilityName"></param>
        public virtual bool BeginAbility(string abilityName) {
            // nothing here
            return true;
        }


        public virtual void ProcessAbilityCoolDowns(AbilityProperties baseAbility, float animationLength, float abilityCoolDown) {
            // do nothing
        }


        public virtual void AddPet(CharacterUnit target) {
            // do nothing, we can't have pets
        }

        /*
        public virtual void CleanupAbilityEffectGameObjects() {
            foreach (List<GameObject> goList in abilityEffectGameObjects.Values) {
                foreach (GameObject go in goList) {
                    if (go != null) {
                        objectPooler.ReturnObjectToPool(go);
                    }
                }
            }
            abilityEffectGameObjects.Clear();
        }
        */

        public virtual void CleanupCoroutines() {
            //Debug.Log(abilityCaster.gameObject.name + ".Abilitymanager.CleanupCoroutines()");
            if (currentCastCoroutine != null) {
                abilityCasterMonoBehaviour.StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
            }
            if (abilityHitDelayCoroutine != null) {
                abilityCasterMonoBehaviour.StopCoroutine(abilityHitDelayCoroutine);
                abilityHitDelayCoroutine = null;
            }
            CleanupCoolDownRoutines();

            if (globalCoolDownCoroutine != null) {
                abilityCasterMonoBehaviour.StopCoroutine(globalCoolDownCoroutine);
                globalCoolDownCoroutine = null;
            }
        }

        public virtual void EndCastCleanup() {
            //Debug.Log(abilityCaster.gameObject.name + ".Abilitymanager.EndCastCleanup()");
            currentCastCoroutine = null;
            currentCastAbility = null;
        }

        public void CleanupCoolDownRoutines() {
            foreach (AbilityCoolDownNode abilityCoolDownNode in abilityCoolDownDictionary.Values) {
                if (abilityCoolDownNode.Coroutine != null) {
                    abilityCasterMonoBehaviour.StopCoroutine(abilityCoolDownNode.Coroutine);
                }
            }
            abilityCoolDownDictionary.Clear();
        }


        public virtual void OnDisable() {
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
            CleanupCoroutines();
            //CleanupAbilityEffectGameObjects();
        }

        public virtual void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        public virtual float GetThreatModifiers() {
            return 1f;
        }

        public virtual bool IsPlayerControlled() {
            return false;
        }


        public virtual float GetAnimationLengthMultiplier() {
            // environmental effects don't need casting animations
            // this is a multiplier, so needs to be one for normal damage
            return 1f;
        }

        public virtual float GetOutgoingDamageModifiers() {
            // this is a multiplier, so needs to be one for normal damage
            return 1f;
        }

        public virtual float GetPhysicalDamage() {
            return 0f;
        }

        public virtual float GetPhysicalPower() {
            return 0f;
        }

        public virtual float GetSpellPower() {
            return 0f;
        }

        public virtual float GetCritChance() {
            return 0f;
        }

        public virtual float GetSpeed() {
            return 1f;
        }

        public virtual bool IsTargetInMeleeRange(Interactable target) {
            return true;
        }

        public virtual bool PerformFactionCheck(ITargetable targetableEffect, CharacterUnit targetCharacterUnit, bool targetIsSelf) {
            // environmental effects should be cast on all units, regardless of faction
            return true;
        }

        public virtual bool IsTargetInRange(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            return true;
        }
        /*
        public virtual bool IsTargetInAbilityRange(BaseAbility baseAbility, Interactable target, AbilityEffectContext abilityEffectContext = null, bool notify = false) {
            // environmental effects only target things inside their collider, so everything is always in range
            return true;
        }

        public virtual bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, Interactable target, AbilityEffectContext abilityEffectContext = null) {
            // environmental effects only target things inside their collider, so everything is always in range
            return true;
        }
        */

        public virtual bool PerformWeaponAffinityCheck(AbilityProperties baseAbility, bool playerInitiated = false) {
            return true;
        }

        public virtual bool PerformAbilityActionCheck(AbilityProperties baseAbility) {
            return true;
        }

        public virtual bool ProcessAnimatedAbilityHit(Interactable target, bool deactivateAutoAttack) {
            // we can now continue because everything beyond this point is single target oriented and it's ok if we cancel attacking due to lack of alive/unfriendly target
            // check for friendly target in case it somehow turned friendly mid swing
            if (target == null || deactivateAutoAttack == true) {
                //baseCharacter.MyCharacterCombat.DeActivateAutoAttack();
                return false;
            }
            return true;
        }

        /*
        public virtual Interactable ReturnTarget(AbilityEffect abilityEffect, Interactable target) {
            return target;
        }
        */


        public virtual float PerformAbilityAction(AbilityProperties baseAbility, AnimationClip animationClip, int clipIndex, UnitController targetUnitController, AbilityEffectContext abilityEffectContext) {

            // do nothing for now
            return 0f;
        }

        public virtual bool DidAbilityHit(Interactable target, AbilityEffectContext abilityEffectContext) {
            return true;
        }

        public virtual void ReceiveCombatMessage(string messageText) {
            // do nothing
            return;
        }

        public virtual void ReceiveMessageFeedMessage(string messageText) {
            // do nothing
            return;
        }

        public virtual Dictionary<PrefabProfile, List<GameObject>> SpawnAbilityEffectPrefabs(Interactable target, Interactable originalTarget, FixedLengthEffectProperties fixedLengthEffectProperties, AbilityEffectContext abilityEffectContext) {
            return ProcessSpawnAbilityEffectPrefabs(target, originalTarget, fixedLengthEffectProperties, abilityEffectContext);
        }

        public virtual Dictionary<PrefabProfile, List<GameObject>> ProcessSpawnAbilityEffectPrefabs(Interactable target, Interactable originalTarget, FixedLengthEffectProperties fixedLengthEffectProperties, AbilityEffectContext abilityEffectInput) {
            //Debug.Log($"{abilityCaster.gameObject.name}.AbilityManager.ProcessSpawnAbilityEffectPrefabs({target}, {(originalTarget == null ? "null" : originalTarget.name)}, {fixedLengthEffectProperties.ResourceName})");

            Dictionary<PrefabProfile, List<GameObject>> prefabObjects = new Dictionary<PrefabProfile, List<GameObject>>();

            if (fixedLengthEffectProperties.GetPrefabProfileList(abilityCaster) != null) {
                List<AbilityAttachmentNode> usedAbilityAttachmentNodeList = new List<AbilityAttachmentNode>();
                if (fixedLengthEffectProperties.RandomPrefabs == false) {
                    usedAbilityAttachmentNodeList = fixedLengthEffectProperties.GetPrefabProfileList(abilityCaster);
                } else {
                    //PrefabProfile copyProfile = prefabProfileList[UnityEngine.Random.Range(0, prefabProfileList.Count -1)];
                    usedAbilityAttachmentNodeList.Add(fixedLengthEffectProperties.GetPrefabProfileList(abilityCaster)[UnityEngine.Random.Range(0, fixedLengthEffectProperties.GetPrefabProfileList(abilityCaster).Count)]);
                }
                foreach (AbilityAttachmentNode abilityAttachmentNode in usedAbilityAttachmentNodeList) {
                    if (abilityAttachmentNode.HoldableObject != null && abilityAttachmentNode.HoldableObject.Prefab != null) {
                        Vector3 spawnLocation = Vector3.zero;
                        Transform prefabParent = null;
                        Vector3 nodePosition = abilityAttachmentNode.HoldableObject.Position;
                        Vector3 nodeRotation = abilityAttachmentNode.HoldableObject.Rotation;
                        Vector3 nodeScale = abilityAttachmentNode.HoldableObject.Scale;
                        if (fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.GroundTarget) {
                            spawnLocation = abilityEffectInput.groundTargetLocation;
                            prefabParent = null;
                        }
                        if (fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.TargetPoint && target != null) {
                            spawnLocation = target.transform.position;
                            prefabParent = null;
                        }
                        if ((fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.Caster || fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.CasterPoint) && (target != null || fixedLengthEffectProperties.GetTargetOptions(abilityCaster).RequireTarget == false)) {
                            //Debug.Log(DisplayName + ".LengthEffect.Cast(): PrefabSpawnLocation is Caster");
                            AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(abilityAttachmentNode);
                            nodeRotation = attachmentPointNode.Rotation;
                            nodeScale = attachmentPointNode.Scale;
                            spawnLocation = UnitGameObject.transform.position;
                            if (fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.CasterPoint) {
                                // transform node position here to ensure that the position is not calculated later on with world coordinates
                                nodePosition = UnitGameObject.transform.TransformDirection(attachmentPointNode.Position);
                                prefabParent = null;
                            } else {
                                nodePosition = attachmentPointNode.Position;
                                prefabParent = UnitGameObject.transform;
                                Transform usedPrefabSourceBone = null;
                                if (attachmentPointNode.TargetBone != null && attachmentPointNode.TargetBone != string.Empty) {
                                    usedPrefabSourceBone = prefabParent.FindChildByRecursive(attachmentPointNode.TargetBone);
                                }
                                if (usedPrefabSourceBone != null) {
                                    prefabParent = usedPrefabSourceBone;
                                }
                            }
                        }
                        if (fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.Target && target != null) {
                            //spawnLocation = target.GetComponent<Collider>().bounds.center;
                            spawnLocation = target.transform.position;
                            prefabParent = target.transform;
                        }
                        if (fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.OriginalTarget && target != null) {
                            //spawnLocation = target.GetComponent<Collider>().bounds.center;
                            spawnLocation = originalTarget.transform.position;
                            prefabParent = originalTarget.transform;
                        }
                        if (fixedLengthEffectProperties.PrefabSpawnLocation != PrefabSpawnLocation.None &&
                            (target != null || fixedLengthEffectProperties.PrefabSpawnLocation == PrefabSpawnLocation.GroundTarget || fixedLengthEffectProperties.GetTargetOptions(abilityCaster).RequireTarget == false)) {
                            float finalX = (prefabParent == null ? spawnLocation.x + nodePosition.x : prefabParent.TransformPoint(nodePosition).x);
                            float finalY = (prefabParent == null ? spawnLocation.y + nodePosition.y : prefabParent.TransformPoint(nodePosition).y);
                            float finalZ = (prefabParent == null ? spawnLocation.z + nodePosition.z : prefabParent.TransformPoint(nodePosition).z);
                            //Vector3 finalSpawnLocation = new Vector3(spawnLocation.x + finalX, spawnLocation.y + prefabOffset.y, spawnLocation.z + finalZ);
                            Vector3 finalSpawnLocation = new Vector3(finalX, finalY, finalZ);
                            //Debug.Log("Instantiating Ability Effect Prefab for: " + DisplayName + " at " + finalSpawnLocation + "; prefabParent: " + (prefabParent == null ? "null " : prefabParent.name) + ";");
                            Vector3 usedForwardDirection = Vector3.forward;
                            if (UnitGameObject != null) {
                                usedForwardDirection = UnitGameObject.transform.forward;
                            }
                            if (prefabParent != null) {
                                usedForwardDirection = prefabParent.transform.forward;
                            }
                            GameObject prefabObject = objectPooler.GetPooledObject(abilityAttachmentNode.HoldableObject.Prefab,
                                finalSpawnLocation,
                                Quaternion.LookRotation(usedForwardDirection) * Quaternion.Euler(nodeRotation),
                                prefabParent);
                            prefabObject.transform.localScale = nodeScale;
                            if (prefabObjects.ContainsKey(abilityAttachmentNode.HoldableObject) == false) {
                                prefabObjects[abilityAttachmentNode.HoldableObject] = new List<GameObject>();
                            }
                            prefabObjects[abilityAttachmentNode.HoldableObject].Add(prefabObject);
                            if (fixedLengthEffectProperties.DestroyOnEndCast) {
                                AddAbilityEffectObject(abilityAttachmentNode, prefabObject);
                            }
                        }
                    }
                }
                abilityEffectInput.PrefabObjects = prefabObjects;
                fixedLengthEffectProperties.BeginMonitoring(prefabObjects, abilityCaster, target, abilityEffectInput);
            }
            return prefabObjects;
        }

        public virtual Dictionary<PrefabProfile, List<GameObject>> SpawnStatusEffectPrefabs(Interactable target, StatusEffectProperties statusEffectProperties, AbilityEffectContext abilityEffectContext) {

            Dictionary<PrefabProfile, List<GameObject>> prefabObjects = new Dictionary<PrefabProfile, List<GameObject>>();
            foreach (AbilityAttachmentNode abilityAttachmentNode in statusEffectProperties.StatusEffectObjectList) {
                if (abilityAttachmentNode.HoldableObject != null && abilityAttachmentNode.HoldableObject.Prefab != null) {
                    Transform prefabParent = null;
                    Vector3 nodePosition = abilityAttachmentNode.HoldableObject.Position;
                    Vector3 nodeRotation = abilityAttachmentNode.HoldableObject.Rotation;
                    Vector3 nodeScale = abilityAttachmentNode.HoldableObject.Scale;
                    if (target != null) {
                        AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(abilityAttachmentNode);
                        nodeRotation = attachmentPointNode.Rotation;
                        nodeScale = attachmentPointNode.Scale;
                        nodePosition = attachmentPointNode.Position;
                        prefabParent = target.transform;
                        Transform usedPrefabSourceBone = null;
                        if (attachmentPointNode.TargetBone != null && attachmentPointNode.TargetBone != string.Empty) {
                            usedPrefabSourceBone = prefabParent.FindChildByRecursive(attachmentPointNode.TargetBone);
                        }
                        if (usedPrefabSourceBone != null) {
                            prefabParent = usedPrefabSourceBone;
                        }
                        float finalX = (prefabParent.TransformPoint(nodePosition).x);
                        float finalY = (prefabParent.TransformPoint(nodePosition).y);
                        float finalZ = (prefabParent.TransformPoint(nodePosition).z);
                        Vector3 finalSpawnLocation = new Vector3(finalX, finalY, finalZ);
                        //Debug.Log("Instantiating Ability Effect Prefab for: " + DisplayName + " at " + finalSpawnLocation + "; prefabParent: " + (prefabParent == null ? "null " : prefabParent.name) + ";");
                        Vector3 usedForwardDirection = prefabParent.transform.forward;
                        GameObject prefabObject = objectPooler.GetPooledObject(abilityAttachmentNode.HoldableObject.Prefab,
                            finalSpawnLocation,
                            Quaternion.LookRotation(usedForwardDirection) * Quaternion.Euler(nodeRotation),
                            prefabParent);
                        prefabObject.transform.localScale = nodeScale;
                        if (prefabObjects.ContainsKey(abilityAttachmentNode.HoldableObject) == false) {
                            prefabObjects[abilityAttachmentNode.HoldableObject] = new List<GameObject>();
                        }
                        prefabObjects[abilityAttachmentNode.HoldableObject].Add(prefabObject);
                    }
                }
            }
            abilityEffectContext.PrefabObjects = prefabObjects;
            statusEffectProperties.BeginMonitoring(prefabObjects, abilityCaster, target, abilityEffectContext);
            return prefabObjects;
        }

        public virtual Dictionary<PrefabProfile, List<GameObject>> SpawnProjectileEffectPrefabs(Interactable target, Interactable originalTarget, ProjectileEffectProperties projectileEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{abilityCaster.gameObject.name}.AbilityManager.SpawnProjectileEffectPrefabs({target}, {(originalTarget == null ? "null" : originalTarget.name)}, {projectileEffectProperties.ResourceName})");

            Dictionary<PrefabProfile, List<GameObject>> prefabObjects = ProcessSpawnAbilityEffectPrefabs(target, originalTarget, projectileEffectProperties, abilityEffectContext);
            
            if (prefabObjects != null) {
                foreach (List<GameObject> gameObjectList in prefabObjects.Values) {
                    foreach (GameObject go in gameObjectList) {
                        //Debug.Log($"{abilityCaster.gameObject.name}.AbilityManager.SpawnProjectileEffectPrefabs(): found gameobject: " + go.name);
                        //go.transform.parent = playerManager.EffectPrefabParent.transform;
                        go.transform.parent = null;
                        ProjectileScript projectileScript = go.GetComponentInChildren<ProjectileScript>();
                        if (projectileScript != null) {
                            //Debug.Log(DisplayName + ".ProjectileEffect.Cast(): found gameobject: " + go.name + " and it has projectile script");
                            abilityEffectContext = projectileEffectProperties.ApplyInputMultiplier(abilityEffectContext);
                            projectileScript.Initialize(systemGameManager, projectileEffectProperties, abilityCaster, target, new Vector3(0, 1, 0), go, abilityEffectContext);
                            if (networkManagerServer.ServerModeActive == false) { 
                                if (projectileEffectProperties.FlightAudioProfiles != null && projectileEffectProperties.FlightAudioProfiles.Count > 0) {
                                    projectileScript.PlayFlightAudio(projectileEffectProperties.FlightAudioProfiles, projectileEffectProperties.RandomFlightAudioProfiles);
                                }
                            }
                            projectileScript.OnCollission += HandleProjectileCollision;
                            projectileScript.OnFlightTimeout += HandleFlightTimeout;
                        }
                    }
                }
            }
            
            return prefabObjects;
        }

        public virtual Dictionary<PrefabProfile, List<GameObject>> SpawnChanneledEffectPrefabs(Interactable target, Interactable originalTarget, ChanneledEffectProperties channeledEffectProperties, AbilityEffectContext abilityEffectContext) {
            Dictionary<PrefabProfile, List<GameObject>> prefabObjects = ProcessSpawnAbilityEffectPrefabs(target, originalTarget, channeledEffectProperties, abilityEffectContext);

            if (prefabObjects != null) {
                //Debug.Log(DisplayName + "ChanneledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + ") PREFABOBJECTS WAS NOT NULL");

                foreach (PrefabProfile prefabProfile in prefabObjects.Keys) {
                    foreach (GameObject go in prefabObjects[prefabProfile]) {
                        // recently added code will properly spawn the object based on universal attachments
                        // get references to the parent and rotation to pass them onto the channeled object script
                        // since this object will switch parents to avoid moving/rotating with the character body
                        GameObject prefabParent = go.transform.parent.gameObject;
                        Vector3 sourcePosition = go.transform.localPosition;

                        //go.transform.parent = playerManager.EffectPrefabParent.transform;
                        go.transform.parent = null;
                        IChanneledObject channeledObjectScript = go.GetComponent<IChanneledObject>();
                        if (channeledObjectScript != null) {
                            Vector3 endPosition = Vector3.zero;
                            Interactable usedTarget = target;
                            if (abilityEffectContext.baseAbility != null && abilityEffectContext.baseAbility.GetTargetOptions(abilityCaster).RequiresGroundTarget == true) {
                                endPosition = abilityEffectContext.groundTargetLocation;
                                usedTarget = null;
                                //Debug.Log(DisplayName + "ChanneledEffect.Cast() abilityEffectInput.prefabLocation: " + abilityEffectInput.prefabLocation);
                            } else {
                                endPosition = target.GetComponent<Collider>().bounds.center - target.transform.position;
                            }

                            channeledObjectScript.Setup(prefabParent, sourcePosition, usedTarget?.gameObject, endPosition, systemGameManager);
                        } else {
                            Debug.LogError($"{abilityCaster.gameObject.name}.AbilityManager.SpawnChanneledEffectPrefabs.(): CHECK INSPECTOR, IChanneledObject NOT FOUND");
                        }
                    }

                }

                // delayed damage
                if (networkManagerServer.ServerModeActive == true || systemGameManager.GameMode == GameMode.Local || levelManager.IsCutscene()) {
                    BeginPerformAbilityHitDelay(abilityCaster, target, abilityEffectContext, channeledEffectProperties);
                }
            } else {
                //Debug.Log(DisplayName + ".ChanneledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + ") PREFABOBJECTS WAS NULL");

            }

            return prefabObjects;
        }

        public void HandleProjectileCollision(IAbilityCaster source, Interactable target, GameObject abilityEffectObject, AbilityEffectContext abilityEffectInput, ProjectileScript projectileScript) {
            //Debug.Log($"{abilityCaster.gameObject.name}.AbilityManager.HandleProjectileCollision({source.AbilityManager.Name}, {(target == null ? "null" : target.gameObject.name)}, {abilityEffectObject.name}, {projectileScript.ProjectileEffectProperties.ResourceName})");

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                projectileScript.ProjectileEffectProperties.PerformAbilityHit(source, target, abilityEffectInput);
            }
            projectileScript.OnCollission -= HandleProjectileCollision;
            projectileScript.OnFlightTimeout -= HandleFlightTimeout;
            objectPooler.ReturnObjectToPool(abilityEffectObject);
        }

        public void HandleFlightTimeout(ProjectileScript projectileScript) {
            //Debug.Log($"{abilityCaster.gameObject.name}.AbilityManager.HandleFlightTimeout({projectileScript.gameObject.name})");

            projectileScript.OnFlightTimeout -= HandleFlightTimeout;
            projectileScript.OnCollission -= HandleProjectileCollision;
        }

        public virtual void ReceiveCombatTextEvent(UnitController unitController, int damage, CombatTextType combatTextType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
        }

        public virtual void ProcessAbilityEffectPooled(GameObject go) {
            // nothing here
        }
    }
}