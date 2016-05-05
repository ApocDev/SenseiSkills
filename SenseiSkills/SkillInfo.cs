using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseiSkills
{
    public class SenseiProfile
    {
        public bool rangedClass = false;
        public int minCastTime = 200;
        public List<SkillInfo> skillList = new List<SkillInfo>();
    }


    public class SkillInfo
    {
        public String skillName;
 

        public bool ignoreSkillError = false;

        public List<SkillInfo> chainSkill = new List<SkillInfo>();

        public SkillType type;

       

    }


    public enum SkillType
    {
        DPS,
        CC,
        CCBREAK,
        GAPCLOSER,
        DEFAULT,
        HEAL,
        EVADE
    };

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

    static class GeneralSettings
    {
        public static int gcd = 100;
        public static float gapCloseRange = 16;
    
    }
}
