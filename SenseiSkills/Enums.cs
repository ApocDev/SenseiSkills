using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseiSkills
{
     public   enum SkillType
        {
            DEFAULT,
            DPS,
            PULL,
            CCBREAK,
            EVADE,
            HEAL
          
           
        };

        public  enum ConditionType
        {
            NONE,
            DISTLT,
            DISTGT,
            STACK,
            EFFECT,
            DEADSUMMON,
            STANCE,
            DONTUSE

        };
    
}
