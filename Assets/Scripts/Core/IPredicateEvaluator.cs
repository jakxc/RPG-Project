namespace RPG.Core
{
    public interface IPredicateEvaluator 
    {
        // Nullible boolean
        bool? Evaluate(Predicate predicate, string[] parameters);
    }
}
