using System;
using System.Collections.Generic;

namespace SenseiSkills.Settings
{
    public interface ISkillLeaf
    {
        String skillName { get; }
        SkillInfo _skillInfo { get; }
        List<ISkillLeaf> Skills { get; }
    }

    public class SkillLeaf : ISkillLeaf
    {
        public string skillName { get; set; }

        private List<ISkillLeaf> _children;
        public List<ISkillLeaf> Skills
        {
            get
            {
                return _children ?? (_children = new List<ISkillLeaf>());
            }
        }

        public SkillInfo _skillInfo { get; set; }

    }
}