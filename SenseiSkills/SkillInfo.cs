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
        public int evadeRange = 5;
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
        public int conditionAmt = 0;
        public bool conditionSelf = false;

    }

    public class SkillInfo
    {
        public String skillName;
 

        public bool ignoreSkillError = false;

        public List<SkillInfo> chainSkill = new List<SkillInfo>();

        public SkillType type;
        public SkillCondition condition;
        public bool selfSkill = false;

       

    }
   

    /*
    public static SkillType getSkillType(String label)
    {
        switch (label)
        {
            case "GAPCLOSER":
                return SkillType.GAPCLOSER;
            case "CCBREAK":
                return SkillType.CCBREAK;
            case "DPS":
                return SkillType.DPS;
            case "EVADE":
                return SkillType.EVADE;
            case "CC":
                return SkillType.CC;
            case "HEAL":
                return SkillType.HEAL;
            case "DEFAULT":
                return SkillType.DEFAULT;

        }

        return SkillType.DEFAULT;
    }*/

   

    static class CircularLinkedList
    {
        public static LinkedListNode<object> NextOrFirst(this LinkedListNode<object> current)
        {
            if (current.Next == null)
                return current.List.First;
            return current.Next;
        }

        public static LinkedListNode<object> PreviousOrLast(this LinkedListNode<object> current)
        {
            if (current.Previous == null)
                return current.List.Last;
            return current.Previous;
        }
    }

    
}
