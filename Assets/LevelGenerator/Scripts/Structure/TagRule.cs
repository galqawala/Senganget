using System;

namespace LevelGenerator.Scripts.Structure
{
    [Serializable]
    public class TagRule
    {
        public string Tag;
        public int MinAmount;
        public int MaxAmount;

        RuleStatus Status => sectionsPlaced < MinAmount
            ? RuleStatus.NotSatisfied
            : sectionsPlaced < MaxAmount
                ? RuleStatus.Satisfied
                : RuleStatus.Completed;

        public bool Satisfied => Status == RuleStatus.Satisfied;

        public bool Completed => Status == RuleStatus.Completed;

        public bool NotSatisfied => Status == RuleStatus.NotSatisfied;

        int sectionsPlaced;

        public void PlaceRuleSection() => sectionsPlaced++;
    }

    public enum RuleStatus
    {
        NotSatisfied,
        Satisfied,
        Completed
    }
}
