using AnyRPG;
﻿using System;
using System.Collections.Generic;

namespace AnyRPG {
public interface ICharacterSkillManager {
    ICharacter MyBaseCharacter { get; set; }
    Dictionary<string, Skill> MySkillList { get; }

    void UpdateSkillList(int newLevel);

    bool HasSkill(string skillName);
    void LearnSkill(string skillName);
    void LoadSkill(string skillName);
    void UnlearnSkill(string skillName);
    //List<string> GetSkillList();
}
}