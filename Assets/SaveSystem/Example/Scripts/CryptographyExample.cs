using SaveSystem;
using System.Collections.Generic;
using UnityEngine;

public class CryptographyExample : MonoBehaviour
{
    private void Awake()
    {
        Example1();
        Example2();
    }

    private void Example1()
    {
        //Creates a new Cryptography object with the encryption password "pa$$w0rd".
        Cryptography cryptography = new Cryptography("pa$$w0rd");

        //This string will be encrypted
        string toEncrypt = "Hello World!";
        //Encrypts the string
        string encrypted = cryptography.Encrypt(toEncrypt);

        //Decrypts the encrypted string
        string decrypted = cryptography.Decrypt<string>(encrypted);

        Debug.Log("Encrypt: " + toEncrypt + "\nResult: " + encrypted);
        Debug.Log("Decrypt: " + encrypted + "\nResult: " + decrypted);
    }

    private void Example2()
    {
        //Creates a new Cryptography object with the encryption password "test-pa$$w0rd".
        Cryptography cryptography = new Cryptography("test-pa$$w0rd");

        //This class will be encrypted
        MyClass toEncrypt = new MyClass(42, new List<int>() { 1, 2, 3 });
        //Encrypts the class and saves the result in a string
        string encrypted = cryptography.Encrypt(toEncrypt);

        //Decrypts the encrypted string
        MyClass decrypted = cryptography.Decrypt<MyClass>(encrypted);

        Debug.Log("Encrypt: " + toEncrypt + "\nResult: " + encrypted);
        Debug.Log("Decrypt: " + encrypted + "\nResult: " + decrypted);
    }
}
