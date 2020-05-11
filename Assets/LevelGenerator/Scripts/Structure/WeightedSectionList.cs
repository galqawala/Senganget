using System;
using System.Collections.Generic;

namespace LevelGenerator.Scripts.Structure
{
    [Serializable]
    public class WeightedSectionList : WeightedList
    {
        public WeightedListItem[] Items;

        public WeightedSectionList()
        {
            keyedItems = new Dictionary<string, WeightedListItem>();
            foreach (var i in Items)
                keyedItems.Add(i.Key, i);
        }
    }
}
