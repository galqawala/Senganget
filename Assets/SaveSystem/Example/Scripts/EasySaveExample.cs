using SaveSystem;
using System.Collections.Generic;
using UnityEngine;

public class EasySaveExample : MonoBehaviour
{
    private void Awake()
    {
        //Pass whatever you want to save in the function.
        EasySave.Save("my-int", 42);
        EasySave.Save("my-class", new MyClass(231, new List<int> { 23, 33 }));

        //Load your values with the key as a parameter and specify the type in the <>.
        Debug.Log("Loaded int: " + EasySave.Load<int>("my-int"));
        Debug.Log("Loaded MyClass: \n" + EasySave.Load<MyClass>("my-class"));

        //Loading with default value: defaultValue is returned if the key doesn't exist.
        Debug.Log("Loaded string: \"" + EasySave.Load("key", "Default Value") + "\"");

        //Does a key exist
        Debug.Log("The key \"int-key\" exists: " + EasySave.HasKey<int>("int-key"));
        Debug.Log("The key \"my-int\" exists: " + EasySave.HasKey<int>("my-int"));

        //Delete a key and it's values
        EasySave.Delete<int>("my-int");
        Debug.Log("The key \"my-int\" has been deleted. ");
    }
}
