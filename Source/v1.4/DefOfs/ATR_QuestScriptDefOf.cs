using RimWorld;

namespace ATReforged
{
    [DefOf]
    public static class ATR_QuestScriptDefOf
    {
        static ATR_QuestScriptDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ATR_QuestScriptDefOf));
        }

        [MayRequireRoyalty]
        public static QuestScriptDef ProblemCauser;
    }
}
