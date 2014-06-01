﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNA_ScreenManager.SkillClasses
{
    public sealed class SkillTree
    {
        public List<Skill> skill_list { get; set; }

        private static SkillTree instance;
        private SkillTree()
        {
            skill_list = new List<Skill>();
        }

        public static SkillTree Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SkillTree();
                }
                return instance;
            }
        }

        public void addSkill(Skill addSkill)
        {
            skill_list.Add(addSkill);
        }

        public void removeSkill(string name)
        {
            skill_list.Remove(new Skill() { Name = name });
        }

        public Skill getSkill(int ID)
        {
            return this.skill_list.Find(delegate(Skill skill) { return skill.ID == ID; });
        }

        public Skill getSkill(string Name)
        {
            return this.skill_list.Find(delegate(Skill skill) { return skill.Name == Name; });
        }

        public bool getSkillRequiments(int ID)
        {
            for (int i = 0; i < 4; i++)
            {
                string ulskill = SkillStore.Instance.skill_list.Find(delegate(Skill skill) { return skill.ID == ID; }).UnlockSkill[i];
                int ullevel = SkillStore.Instance.skill_list.Find(delegate(Skill skill) { return skill.ID == ID; }).UnlockLevel[i];

                // check if all prerequisite skill levels are OK
                if (ulskill != null)
                {
                    if (getSkill(ulskill) != null)
                    {
                        if (getSkill(ulskill).Level < ullevel)
                            return false;
                    }
                    else
                        return false;
                }

            }
                
            return true;
        }
    }
}
