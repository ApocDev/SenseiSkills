using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseiSkills.CombatHandler
{
    interface ICombatHandler
    {
        Task Combat();
        Task Pull();
        Task Loot();
    }
}
