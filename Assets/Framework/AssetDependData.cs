using System;
using System.Collections.Generic;

[Serializable]
public class AssetDependData
{
    [Serializable]
    public class DependencyEntry
    {
        public string key;
        public List<string> dependencies;
    }

    public List<DependencyEntry> dependEntries = new();

    public Dictionary<string, List<string>> ToDictionary()
    {
        Dictionary<string, List<string>> dict = new();
        foreach (var entry in dependEntries)
            dict[entry.key] = entry.dependencies;
        return dict;
    }
}
