using System.Collections.Generic;
using System.Linq;
using LevelGenerator.Scripts.Helpers;

namespace LevelGenerator.Scripts.Structure
{
    public abstract class WeightedList
    {
        protected Dictionary<string, WeightedListItem> keyedItems;

        public WeightedListItem GetItem()
        {
            var distributedList = new List<string>();
            foreach (var key in keyedItems.Keys)
            {
                foreach (var i in Enumerable.Range(0, keyedItems[key].Weight))
                {
                    distributedList.Add(key);
                }
            }

            return keyedItems[distributedList.PickOne()];
        }

        public void RemoveItem(string key)
        {
            keyedItems.Remove(key);
        }
    }
}
