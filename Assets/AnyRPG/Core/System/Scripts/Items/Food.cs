using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Food", menuName = "AnyRPG/Inventory/Items/Food", order = 1)]
    public class Food : CastableItem {

        [Header("Food")]

        [Tooltip("The verb to use in the casting tip")]
        [SerializeField]
        private FoodConsumptionVerb consumptionVerb = FoodConsumptionVerb.Eat;

        [Header("Prefabs")]

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [SerializeField]
        private List<AbilityAttachmentNode> holdableObjectList = new List<AbilityAttachmentNode>();

        [Header("Animation")]

        [Tooltip("The animation clip the character will perform")]
        [SerializeField]
        protected AnimationClip animationClip = null;

        [Tooltip("The name of an animation profile to get animations for the character to perform while casting this ability")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        protected string animationProfileName = string.Empty;

        [Header("Audio")]

        [Tooltip("An audio profile to play while the ability is casting")]
        [SerializeField]
        protected AudioClip castingAudioClip = null;

        [Tooltip("An audio profile to play while the ability is casting")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected string castingAudioProfileName = string.Empty;

        [Tooltip("If true, the audio will be looped until the action is complete")]
        [SerializeField]
        protected bool loopAudio = false;

        [Header("Tick")]

        [Tooltip("While consuming, the number of seconds between each resource refill")]
        [SerializeField]
        private float tickRate = 1f;

        [Tooltip("The resources to refill, and the amounts of the refill every tick")]
        [SerializeField]
        private List<PowerResourcePotionAmountNode> resourceAmounts = new List<PowerResourcePotionAmountNode>();

        [Header("Completion Effect")]

        [Tooltip("The status effect to cast if the food is completely consumed")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(StatusEffect))]
        private string statusEffect = string.Empty;

        private ChanneledAbilityProperties channeledAbility = null;

        public override BaseAbilityProperties Ability {
            get {
                return channeledAbility;
            }
        }

        /*
        public override string GetCastableInformation() {
            string returnString = string.Empty;
            if (Ability != null) {
                returnString += string.Format("<color=green>Use: {0}</color>", Ability.Description);
            }
            return returnString;
        }
        */

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            DescribableProperties describable = new DescribableProperties(this);
            describable.DisplayName = consumptionVerb + " " + DisplayName;

            // set up the tick effect
            HealEffectProperties healEffect = new HealEffectProperties();
            healEffect.AllowCriticalStrike = false;

            // target options should not actually be necessary because they are bypassed when directly casted
            healEffect.TargetOptions.AutoSelfCast = true;
            healEffect.TargetOptions.CanCastOnOthers = false;
            healEffect.TargetOptions.CanCastOnSelf = true;
            healEffect.TargetOptions.RequireLiveTarget = true;
            healEffect.TargetOptions.RequireTarget = true;

            foreach (PowerResourcePotionAmountNode powerResourceAmountNode in resourceAmounts) {
                ResourceAmountNode resourceAmountNode = new ResourceAmountNode();
                resourceAmountNode.ResourceName = powerResourceAmountNode.ResourceName;
                resourceAmountNode.AddPower = false;
                resourceAmountNode.MinAmount = powerResourceAmountNode.MinAmount;
                resourceAmountNode.BaseAmount = powerResourceAmountNode.BaseAmount;
                resourceAmountNode.MaxAmount = powerResourceAmountNode.MaxAmount;
                healEffect.ResourceAmounts.Add(resourceAmountNode);
            }
            healEffect.SetupScriptableObjects(systemGameManager, describable);


            // set up the ability
            channeledAbility = new ChanneledAbilityProperties();
            // target options
            AbilityTargetProps abilityTargetProps = new AbilityTargetProps();
            abilityTargetProps.AutoSelfCast = true;
            abilityTargetProps.CanCastOnOthers = false;
            abilityTargetProps.CanCastOnSelf = true;
            abilityTargetProps.RequireLiveTarget = true;
            abilityTargetProps.RequireTarget = true;
            channeledAbility.TargetOptions = abilityTargetProps;

            channeledAbility.AbilityPrefabSource = AbilityPrefabSource.Ability;
            channeledAbility.HoldableObjectList = holdableObjectList;
            channeledAbility.AnimationProfileName = animationProfileName;
            channeledAbility.CastingAnimationClip = animationClip;
            channeledAbility.UseAnimationCastTime = true;
            channeledAbility.CastingAudioClip = castingAudioClip;
            channeledAbility.CastingAudioProfileName = castingAudioProfileName;
            channeledAbility.LoopAudio = loopAudio;
            channeledAbility.UseableWithoutLearning = true;
            channeledAbility.UseSpeedMultipliers = false;
            channeledAbility.TickRate = tickRate;
            if (statusEffect != string.Empty) {
                channeledAbility.AbilityEffectNames = new List<string>() { statusEffect };
            }
            //channeledAbility.AbilityEffectNames.Add(statusEffect);
            channeledAbility.ChanneledAbilityEffects.Add(healEffect);

            channeledAbility.SetupScriptableObjects(systemGameManager, describable);
        }

    }

    public enum FoodConsumptionVerb { Eat, Drink }

}