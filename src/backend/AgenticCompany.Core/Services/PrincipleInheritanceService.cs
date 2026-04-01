using AgenticCompany.Core.Entities;

namespace AgenticCompany.Core.Services;

public class PrincipleInheritanceService
{
    public record EffectivePrinciple(Principle Principle, Guid SourceNodeId, bool IsInherited, bool IsOverridden);
    public record PrincipleConflict(Principle Local, Principle Inherited, string Reason);

    public List<EffectivePrinciple> ResolveEffective(
        List<(Guid NodeId, List<Principle> Principles)> ancestorPrinciples,
        Guid targetNodeId)
    {
        var effective = new List<EffectivePrinciple>();

        // Build a map: title → deepest index where an override exists
        var overrideDepthByTitle = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < ancestorPrinciples.Count; i++)
        {
            foreach (var p in ancestorPrinciples[i].Principles.Where(p => p.IsOverride))
            {
                overrideDepthByTitle[p.Title] = i; // last (deepest) wins
            }
        }

        // Walk from root to target, adding principles
        for (int i = 0; i < ancestorPrinciples.Count; i++)
        {
            var (nodeId, principles) = ancestorPrinciples[i];
            foreach (var principle in principles.OrderBy(p => p.Order))
            {
                bool isTarget = nodeId == targetNodeId;
                // A principle is overridden only if a same-titled override exists at a deeper level
                bool isOverridden = !principle.IsOverride
                    && overrideDepthByTitle.TryGetValue(principle.Title, out var overrideIdx)
                    && overrideIdx > i;

                effective.Add(new EffectivePrinciple(
                    principle,
                    nodeId,
                    IsInherited: !isTarget,
                    IsOverridden: isOverridden
                ));
            }
        }

        return effective;
    }

    public List<PrincipleConflict> DetectConflicts(List<EffectivePrinciple> effective)
    {
        var conflicts = new List<PrincipleConflict>();
        var byTitle = effective.GroupBy(e => e.Principle.Title, StringComparer.OrdinalIgnoreCase);

        foreach (var group in byTitle.Where(g => g.Count() > 1))
        {
            var items = group.ToList();
            var inherited = items.Where(i => i.IsInherited).ToList();
            var local = items.FirstOrDefault(i => !i.IsInherited);

            if (local != null)
            {
                foreach (var inh in inherited)
                {
                    if (!local.Principle.IsOverride)
                    {
                        conflicts.Add(new PrincipleConflict(
                            local.Principle,
                            inh.Principle,
                            $"Local principle '{local.Principle.Title}' has same title as inherited principle but is not marked as override"
                        ));
                    }
                }
            }
        }

        return conflicts;
    }
}
