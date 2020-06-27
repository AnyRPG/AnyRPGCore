using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    static class LevelEquations {

        public static int GetXPNeededForLevel(int _level) {
            return _level * SystemConfigurationManager.MyInstance.XpRequiredPerLevel;
        }

        /// <summary>
        /// return a color related to the level of the target compared to the source
        /// </summary>
        /// <param name="sourceLevel"></param>
        /// <param name="targetLevel"></param>
        /// <returns></returns>
        public static Color GetTargetColor(int sourceLevel, int targetLevel) {
            if (targetLevel >= sourceLevel + 5) {
                return Color.red;
            } else if (targetLevel >= sourceLevel + 5) {
                return Color.red;
            } else if (targetLevel >= sourceLevel + 3 && targetLevel <= sourceLevel + 4) {
                return new Color32(255, 165, 0, 255);
            } else if (targetLevel >= sourceLevel - 2 && targetLevel <= sourceLevel + 2) {
                return Color.yellow;
            } else if (targetLevel <= sourceLevel - 3 && targetLevel > GetGrayLevel(sourceLevel)) {
                return Color.green;
            } else {
                return Color.gray;
            }
        }

        public static int GetGrayLevel(int sourceLevel) {
            if (sourceLevel <= 5) {
                return 0;
            } else if (sourceLevel <= 49) {
                return sourceLevel - (int)Mathf.Floor(sourceLevel / 10) - 5;
            } else if (sourceLevel <= 50) {
                return sourceLevel - 10;
            } else if (sourceLevel <= 59) {
                return sourceLevel - (int)Mathf.Floor(sourceLevel / 5) - 1;
            } else if (sourceLevel <= 70) {
                return sourceLevel - 9;
            } else {
                return sourceLevel - 9;
            }
        }

        public static int GetXPAmountForKill(int sourceLevel, BaseCharacter targetCharacter) {

            float multiplierValue = 1f;
            float toughnessMultiplierValue = 1f;

            if (SystemConfigurationManager.MyInstance.UseKillXPLevelMultiplierDemoninator == true) {
                multiplierValue = 1f / Mathf.Clamp(sourceLevel, 0, (SystemConfigurationManager.MyInstance.KillXPMultiplierLevelCap > 0 ? SystemConfigurationManager.MyInstance.KillXPMultiplierLevelCap : Mathf.Infinity));
            }
            if (targetCharacter.CharacterStats.Toughness != null) {
                toughnessMultiplierValue = targetCharacter.CharacterStats.Toughness.ExperienceMultiplier;
            }

            int baseXP = (int)((((sourceLevel * SystemConfigurationManager.MyInstance.KillXPPerLevel) * multiplierValue) + SystemConfigurationManager.MyInstance.BaseKillXP) * toughnessMultiplierValue);

            int totalXP = 0;
            if (sourceLevel < targetCharacter.CharacterStats.Level) {
                // higher level mob
                totalXP = (int)(baseXP * (1 + 0.05 * (targetCharacter.CharacterStats.Level - sourceLevel)));
            } else if (sourceLevel == targetCharacter.CharacterStats.Level) {
                totalXP = baseXP;
            } else if (targetCharacter.CharacterStats.Level > GetGrayLevel(sourceLevel)) {
                totalXP = baseXP * (1 - (sourceLevel - targetCharacter.CharacterStats.Level) / ZeroDifference(sourceLevel));
            }
            return totalXP;
        }

        public static int GetXPAmountForQuest(int sourceLevel, Quest quest) {

            float multiplierValue = 1f;

            if (SystemConfigurationManager.MyInstance.UseQuestXPLevelMultiplierDemoninator == true) {
                multiplierValue = 1f / Mathf.Clamp(sourceLevel, 0, (SystemConfigurationManager.MyInstance.QuestXPMultiplierLevelCap > 0 ? SystemConfigurationManager.MyInstance.QuestXPMultiplierLevelCap : Mathf.Infinity));
            }

            int experiencePerLevel = SystemConfigurationManager.MyInstance.QuestXPPerLevel + quest.ExperienceRewardPerLevel;
            int baseExperience = SystemConfigurationManager.MyInstance.BaseQuestXP + quest.BaseExperienceReward;

            int baseXP = (int)(((quest.MyExperienceLevel * experiencePerLevel) * multiplierValue) + baseExperience);

            if (sourceLevel <= quest.MyExperienceLevel + 5) {
                return baseXP;
            }
            if (sourceLevel == quest.MyExperienceLevel + 6) {
                return (int)(baseXP * 0.8);
            }
            if (sourceLevel == quest.MyExperienceLevel + 7) {
                return (int)(baseXP * 0.6);
            }
            if (sourceLevel == quest.MyExperienceLevel + 8) {
                return (int)(baseXP * 0.4);
            }
            if (sourceLevel == quest.MyExperienceLevel + 9) {
                return (int)(baseXP * 0.2);
            }
            if (sourceLevel == quest.MyExperienceLevel + 10) {
                return (int)(baseXP * 0.1);
            }
            return 0;
        }


        private static int ZeroDifference(int sourceLevel) {
            if (sourceLevel <= 7) {
                return 5;
            } else if (sourceLevel <= 9) {
                return 6;
            } else if (sourceLevel <= 11) {
                return 7;
            } else if (sourceLevel <= 15) {
                return 8;
            } else if (sourceLevel <= 19) {
                return 9;
            } else if (sourceLevel <= 29) {
                return 11;
            } else if (sourceLevel <= 39) {
                return 12;
            } else if (sourceLevel <= 44) {
                return 13;
            } else if (sourceLevel <= 49) {
                return 14;
            } else if (sourceLevel <= 54) {
                return 15;
            } else if (sourceLevel <= 59) {
                return 16;
            } else if (sourceLevel <= 79) {
                return 17;
            }
            return 100;
        }

        public static float GetPrimaryStatForLevel(string statName, int level, BaseCharacter baseCharacter) {

            float extraStatPerLevel = 0;

            if (baseCharacter != null) {
                foreach (IStatProvider statProvider in baseCharacter.StatProviders) {
                    if (statProvider != null) {
                        foreach (StatScalingNode statScalingNode in statProvider.PrimaryStats) {
                            if (statScalingNode.StatName == statName) {
                                extraStatPerLevel += statScalingNode.BudgetPerLevel;
                                break;
                            }
                        }
                    }
                }
            }

            foreach (StatScalingNode statScalingNode in SystemConfigurationManager.MyInstance.PrimaryStats) {
                if (statScalingNode.StatName == statName) {
                    extraStatPerLevel += statScalingNode.BudgetPerLevel;
                    break;
                }
            }

            return SystemConfigurationManager.MyInstance.MyStatBudgetPerLevel + extraStatPerLevel;
        }

        public static float GetBaseSecondaryStatForCharacter(SecondaryStatType secondaryStatType, BaseCharacter sourceCharacter) {
            float returnValue = 0f;

            foreach (IStatProvider statProvider in sourceCharacter.StatProviders) {
                if (statProvider != null) {
                    foreach (StatScalingNode statScalingNode in statProvider.PrimaryStats) {
                        foreach (PrimaryToSecondaryStatNode primaryToSecondaryStatNode in statScalingNode.PrimaryToSecondaryConversion) {
                            if (primaryToSecondaryStatNode.SecondaryStatType == secondaryStatType) {
                                if (primaryToSecondaryStatNode.RatedConversion == true) {
                                    returnValue += primaryToSecondaryStatNode.ConversionRatio * (sourceCharacter.CharacterStats.PrimaryStats[statScalingNode.StatName].CurrentValue / sourceCharacter.CharacterStats.Level);
                                } else {
                                    returnValue += primaryToSecondaryStatNode.ConversionRatio * sourceCharacter.CharacterStats.PrimaryStats[statScalingNode.StatName].CurrentValue;
                                }
                            }
                        }
                    }
                }
            }

            returnValue += sourceCharacter.CharacterStats.SecondaryStats[secondaryStatType].DefaultAddValue;
            return returnValue;
        }

        public static float GetSecondaryStatForCharacter(SecondaryStatType secondaryStatType, BaseCharacter sourceCharacter) {
            float returnValue = GetBaseSecondaryStatForCharacter(secondaryStatType, sourceCharacter);
            returnValue += sourceCharacter.CharacterStats.GetSecondaryStatAddModifiers(secondaryStatType);
            return returnValue;
        }

        public static float GetArmorForClass(ArmorClass armorClass) {
            float returnValue = 0f;
            if (armorClass != null) {
                return armorClass.MyArmorPerLevel;
            }
            return returnValue;
        }


    }

}