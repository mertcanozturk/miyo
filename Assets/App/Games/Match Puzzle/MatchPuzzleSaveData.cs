using System;
using System.Collections.Generic;
using System.Linq;

namespace Miyo.Games
{
    [Serializable]
    public class MatchPuzzleSaveData
    {
        public List<ChildSaveEntry> saves = new();

        public ChildSaveEntry GetOrCreate(string childName)
        {
            var entry = saves.FirstOrDefault(s => s.childName == childName);
            if (entry == null)
            {
                entry = new ChildSaveEntry(childName);
                saves.Add(entry);
            }
            return entry;
        }
    }

    [Serializable]
    public class ChildSaveEntry
    {
        public string childName;
        public int levelIndex;

        public ChildSaveEntry() { }

        public ChildSaveEntry(string childName)
        {
            this.childName = childName;
            levelIndex = 0;
        }
    }
}
