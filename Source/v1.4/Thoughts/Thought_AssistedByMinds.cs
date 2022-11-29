using RimWorld;

namespace ATReforged
{
    public class Thought_AssistedByMinds : Thought_Situational
    {
        public override string LabelCap
        {
            get
            {
                return CurStage.label;
            }
        }
    }
}