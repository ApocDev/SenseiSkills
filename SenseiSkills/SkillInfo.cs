using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseiSkills
{
    public class SenseiProfile
    {
        public bool attackTarget = true;
        public bool rangedClass = true;
        public int minCastTime = 200;
        public int gapCloseRange = 16;

        public String potKey="6";
        public String dumplingKey = "5";
        public int potUsePct = 20;
        public int dumplingUsePct=20;


        public List<SkillInfo> skillList = new List<SkillInfo>();
    }


    public class SkillCondition
    {
        public ConditionType type;
        public String conditionName;
        public int stackCount = 0;
        public int conditionAmount = 0;
        public bool conditionSelf = false;

    }

    public class SkillInfo
    {
        public String skillName;
 

        public bool ignoreSkillError = false;

        public List<SkillInfo> chainSkill = new List<SkillInfo>();

        public SkillType type;
        public List<SkillCondition> conditions;
        public bool selfSkill = false;

       

    }
   


    
}
