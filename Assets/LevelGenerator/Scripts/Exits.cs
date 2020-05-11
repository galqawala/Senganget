using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGenerator.Scripts
{
    public class Exits : MonoBehaviour
    {
        public IEnumerable<Transform> ExitSpots => GetComponentsInChildren<Transform>().Where(t => t != transform);
    }
}
