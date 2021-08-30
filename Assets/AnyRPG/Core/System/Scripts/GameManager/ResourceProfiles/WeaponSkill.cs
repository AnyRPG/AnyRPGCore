using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Weapon Skill", menuName = "AnyRPG/WeaponSkill")]
    public class WeaponSkill : DescribableResource {

        [SerializeField]
        private WeaponSkillProps weaponSkillProps = new WeaponSkillProps();

        public WeaponSkillProps WeaponSkillProps { get => weaponSkillProps; set => weaponSkillProps = value; }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);
            weaponSkillProps.SetupScriptableObjects(DisplayName, systemGameManager);
        }
    }

}