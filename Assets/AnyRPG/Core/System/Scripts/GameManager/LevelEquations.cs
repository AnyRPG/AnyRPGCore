using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    static class LevelEquations {

        public static int GetXPNeededForLevel(int _level, SystemConfigurationManager systemConfigurationManager) {
            return _level * systemConfigurationManager.XpRequiredPerLevel;
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

        public static int GetXPAmountForKill(int sourceLevel, UnitController targetCharacter, SystemConfigurationManager systemConfigurationManager) {

            float multiplierValue = 1f;
            float toughnessMultiplierValue = 1f;

            if (systemConfigurationManager.UseKillXPLevelMultiplierDemoninator == true) {
                multiplierValue = 1f / Mathf.Clamp(sourceLevel, 0, (systemConfigurationManager.KillXPMultiplierLevelCap > 0 ? systemConfigurationManager.KillXPMultiplierLevelCap : Mathf.Infinity));
            }
            if (targetCharacter.BaseCharacter.UnitToughness != null) {
                toughnessMultiplierValue = targetCharacter.BaseCharacter.UnitToughness.ExperienceMultiplier;
            }

            int baseXP = (int)((((sourceLevel * systemConfigurationManager.KillXPPerLevel) * multiplierValue) + systemConfigurationManager.BaseKillXP) * toughnessMultiplierValue);

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

        public static int GetXPAmountForQuest(int sourceLevel, Quest quest, SystemConfigurationManager systemConfigurationManager) {

            float multiplierValue = 1f;

            if (systemConfigurationManager.UseQuestXPLevelMultiplierDemoninator == true) {
                multiplierValue = 1f / Mathf.Clamp(sourceLevel, 0, (systemConfigurationManager.QuestXPMultiplierLevelCap > 0 ? systemConfigurationManager.QuestXPMultiplierLevelCap : Mathf.Infinity));
            }

            int experiencePerLevel = systemConfigurationManager.QuestXPPerLevel + quest.ExperienceRewardPerLevel;
            int baseExperience = systemConfigurationManager.BaseQuestXP + quest.BaseExperienceReward;

            int baseXP = (int)(((quest.ExperienceLevel * experiencePerLevel) * multiplierValue) + baseExperience);

            if (sourceLevel <= quest.ExperienceLevel + 5) {
                return baseXP;
            }
            if (sourceLevel == quest.ExperienceLevel + 6) {
                return (int)(baseXP * 0.8);
            }
            if (sourceLevel == quest.ExperienceLevel + 7) {
                return (int)(baseXP * 0.6);
            }
            if (sourceLevel == quest.ExperienceLevel + 8) {
                return (int)(baseXP * 0.4);
            }
            if (sourceLevel == quest.ExperienceLevel + 9) {
                return (int)(baseXP * 0.2);
            }
            if (sourceLevel == quest.ExperienceLevel + 10) {
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

        public static float GetPrimaryStatForLevel(string statName, int level, BaseCharacter baseCharacter, SystemConfigurationManager systemConfigurationManager) {

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

            // not needed because it should be part of stat providers already ?
            /*
            foreach (StatScalingNode statScalingNode in systemConfigurationManager.PrimaryStats) {
                if (statScalingNode.StatName == statName) {
                    extraStatPerLevel += statScalingNode.BudgetPerLevel;
                    break;
                }
            }
            */

            return systemConfigurationManager.StatBudgetPerLevel + extraStatPerLevel;
        }

        public static float GetBaseSecondaryStatForCharacter(SecondaryStatType secondaryStatType, UnitController unitController) {
            //Debug.Log("LevelEquations.GetSecondaryStatForCharacter(" + secondaryStatType.ToString() + ", " + sourceCharacter.AbilityManager.CharacterName + ")");
            float returnValue = 0f;

            foreach (IStatProvider statProvider in unitController.BaseCharacter.StatProviders) {
                if (statProvider != null) {
                    foreach (StatScalingNode statScalingNode in statProvider.PrimaryStats) {
                        foreach (PrimaryToSecondaryStatNode primaryToSecondaryStatNode in statScalingNode.PrimaryToSecondaryConversion) {
                            if (primaryToSecondaryStatNode.SecondaryStatType == secondaryStatType) {
                                if (primaryToSecondaryStatNode.RatedConversion == true) {
                                    returnValue += primaryToSecondaryStatNode.ConversionRatio * (unitController.CharacterStats.PrimaryStats[statScalingNode.StatName].CurrentValue / unitController.CharacterStats.Level);
                                } else {
                                    returnValue += primaryToSecondaryStatNode.ConversionRatio * unitController.CharacterStats.PrimaryStats[statScalingNode.StatName].CurrentValue;
                                }
                            }
                        }
                    }
                }
            }

            returnValue += unitController.CharacterStats.SecondaryStats[secondaryStatType].DefaultAddValue;
            return returnValue;
        }

        public static float GetSecondaryStatForCharacter(SecondaryStatType secondaryStatType, UnitController unitController) {
            //Debug.Log("LevelEquations.GetSecondaryStatForCharacter(" + sourceCharacter.AbilityManager.CharacterName + ")");
            float returnValue = GetBaseSecondaryStatForCharacter(secondaryStatType, unitController);
            returnValue += unitController.CharacterStats.GetSecondaryAddModifiers(secondaryStatType);
            returnValue *= unitController.CharacterStats.GetSecondaryMultiplyModifiers(secondaryStatType);
            return returnValue;
        }

        public static float GetArmorForClass(ArmorClass armorClass) {
            float returnValue = 0f;
            if (armorClass != null) {
                return armorClass.ArmorPerLevel;
            }
            return returnValue;
        }


    }

}