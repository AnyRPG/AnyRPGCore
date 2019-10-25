using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
static class LevelEquations
{

    public static int GetXPNeededForLevel(int _level) {
        return _level * 100;
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

    public static int GetXPAmountForKill(int sourceLevel, int targetLevel) {
        int baseXP = (sourceLevel * 5) + 45;
        int totalXP = 0;
        if (sourceLevel < targetLevel) {
            // higher level mob
            totalXP = (int)(baseXP * (1 + 0.05 * (targetLevel - sourceLevel)));
        } else if (sourceLevel == targetLevel) {
            totalXP = baseXP;
        } else if (targetLevel > GetGrayLevel(sourceLevel)) {
            totalXP = baseXP * (1 - (sourceLevel - targetLevel) / ZeroDifference(sourceLevel));
        }
        return totalXP;
    }

    public static int GetXPAmountForQuest(int sourceLevel, Quest quest) {
        if (sourceLevel <= quest.MyExperienceLevel + 5) {
            return quest.MyExperienceReward;
        }
        if (sourceLevel == quest.MyExperienceLevel + 6) {
            return (int)(quest.MyExperienceReward * 0.8);
        }
        if (sourceLevel == quest.MyExperienceLevel + 7) {
            return (int)(quest.MyExperienceReward * 0.6);
        }
        if (sourceLevel == quest.MyExperienceLevel + 8) {
            return (int)(quest.MyExperienceReward * 0.4);
        }
        if (sourceLevel == quest.MyExperienceLevel + 9) {
            return (int)(quest.MyExperienceReward * 0.2);
        }
        if (sourceLevel == quest.MyExperienceLevel + 10) {
            return (int)(quest.MyExperienceReward * 0.1);
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
}

}