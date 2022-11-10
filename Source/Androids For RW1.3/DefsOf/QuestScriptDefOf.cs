using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ATReforged
{
    [DefOf]
    public static class QuestScriptDefOf
    {
        static QuestScriptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(QuestScriptDefOf));
        }

        public static QuestScriptDef ProblemCauser;
    }
}
