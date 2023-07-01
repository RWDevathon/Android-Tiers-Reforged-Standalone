using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class QuestScriptDefOf
    {
        static QuestScriptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(QuestScriptDefOf));
        }
        
        [MayRequireRoyalty]
        public static QuestScriptDef ProblemCauser;
    }
}
