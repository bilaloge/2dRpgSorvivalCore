using System.Collections.Generic;
using UnityEngine;
public static class TargetRegistry
{
    private static readonly HashSet<Transform> _targets = new();

    public static IReadOnlyCollection<Transform> Targets => _targets;
    public static void Register(Transform t)
    {
        if (t != null && !_targets.Contains(t))
            _targets.Add(t);
    }
    public static void Unregister(Transform t)
    {
        if (_targets.Contains(t))
            _targets.Remove(t);
    }
}