using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseiSkills
{
     public   enum SkillType
        {
            DPS,
            CC,
            CCBREAK,
            GAPCLOSER,
            DEFAULT,
            HEAL,
            EVADE,
            PULL
        };

        public  enum ConditionType
        {
            NONE,
            DISTLT,
            DISTGT,
            STACK,
            DEBUFF,
            BUFF,
            DEADSUMMON,
            STANCE,
            DONTUSE

        };
    
}
