using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class WeaponGenerator : MonoBehaviour
{
    public static GameObject getWeaponGameObject(string name) {
        var weaponResourceNames = new string[] { "Weapon_Blaster","Weapon_Launcher","Weapon_Shotgun" };
        var weaponResourceName = weaponResourceNames[randomIntBySeed(name,0,weaponResourceNames.Length-1)];
        GameObject weaponPrefab = (GameObject) Resources.Load(weaponResourceName);
        return weaponPrefab;
    }

    private static float randomBySeed(byte[] seed) // random seeded float between 0 and 1
    {
        SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
        byte[] hash = sha256.ComputeHash(seed); //32 x 0...255
        Debug.Assert(hash.Length == 32);
        float value = hash[0] + (hash[1]*256);
        assertValueBetween(value, 0, 65535);
        value = value / 65535;
        assertValueBetween(value, 0, 1);
        return value;
    }

    private static float randomBySeed(string seed)
    {
        return randomBySeed(Encoding.ASCII.GetBytes(seed));
    }

    private static int randomIntBySeed(string seed, int min, int max)
    {
        float rnd = randomBySeed(Encoding.ASCII.GetBytes(seed));
        float span = max-min+1; //0..2 --> 3 options
        //clamp just in case rnd=1
        return Mathf.Clamp(Mathf.FloorToInt(min+(rnd*span)), min, max);
    }

    private static void assertValueBetween(float value, float min, float max) {
        Debug.Assert(value >= min);
        Debug.Assert(value <= max);
    }
}
