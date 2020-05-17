using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class WeaponGenerator : MonoBehaviour
{
    public static GameObject getWeaponGameObject(string name) {
        var weaponResourceNames = new string[] { "Weapon_Blaster","Weapon_Launcher","Weapon_Shotgun" };
        var weaponResourceName = weaponResourceNames[randomIntBySeed(name+"ResGO",0,weaponResourceNames.Length-1)];
        GameObject weaponPrefab = (GameObject) Resources.Load(weaponResourceName);
        return weaponPrefab;
    }

    public static void modWeapon(WeaponController weaponController, string name) {
        weaponController.weaponName = name;
        //Customize the visuals
        var meshRenderers = weaponController.GetComponentsInChildren<MeshRenderer>();
        foreach (var meshRenderer in meshRenderers) {
            var materials = meshRenderer.materials;
            foreach (var material in materials) {
                var colorSeed = name+">"+meshRenderer.name+">"+material.name+">color";
                //the numbered parts like Primary_Weapon_Front_Piece_01 & Primary_Weapon_Front_Piece_02 should get the same color
                //also "WeaponPistol (Instance)" and "WeaponPistol (Instance) (Instance)" should get the same seed
                colorSeed = Regex.Replace(colorSeed, @"([()\s\d]|Instance)+", string.Empty);
                material.color = randomColor(colorSeed);
            }
        }
    }

    public static void dropWeapon(string weaponName, Vector3 position) {
        Debug.Log("Dropping a weapon: "+weaponName);
        var pickupObject = Resources.Load("Pickup_Weapon"); //generic pickup object
        var pickupInstance = (GameObject) Instantiate(pickupObject, position, Quaternion.identity);
        var pickupMesh = pickupInstance.transform.Find("Mesh");
        var weaponObject = WeaponGenerator.getWeaponGameObject(weaponName);
        var weaponInstance = Instantiate(weaponObject, position, Quaternion.identity, pickupMesh);
        WeaponController weaponController = weaponInstance.GetComponentInChildren(typeof(WeaponController)) as WeaponController;
        WeaponGenerator.modWeapon(weaponController, weaponName);
        WeaponPickup weaponPickupComponent = pickupInstance.AddComponent(typeof(WeaponPickup)) as WeaponPickup;
        weaponPickupComponent.weaponPrefab = weaponController;
    }

    public static void dropWeapon(Vector3 position) {
        var weaponName = Utilities.FirstLetterToUpper(Trigrams.randomText(5));
        dropWeapon(weaponName, position);
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

    private static Color randomColor(string seed)
    {
        var red   = randomBySeed(seed+"red");
        var green = randomBySeed(seed+"green");
        var blue  = randomBySeed(seed+"blue");
        return new Color(red, green, blue);
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
