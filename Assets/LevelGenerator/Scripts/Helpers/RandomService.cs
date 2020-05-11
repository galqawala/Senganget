
using System;

namespace LevelGenerator.Scripts.Helpers
{
    public static class RandomService
    {
        static Random rnd;
        public static int Seed { get; private set; }

        static RandomService()
        {
            rnd = new Random();
            Seed = rnd.Next(Int32.MinValue, Int32.MaxValue);
            
            rnd = new Random(Seed);
        }

        public static void SetSeed(int seed)
        {
            Seed = seed;
            rnd = new Random(Seed);
        }

        public static bool RollD100(int chance) => rnd.Next(1, 101) <= chance;

        public static int GetRandom(int min, int max) => rnd.Next(min, max);
    }
}
