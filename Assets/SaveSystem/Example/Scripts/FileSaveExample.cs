using SaveSystem;
using System.Collections.Generic;
using UnityEngine;

public class FileSaveExample : MonoBehaviour
{
    private void Awake()
    {
        //The standard file path in unity
        Debug.Log("Your files are located here: " + Application.persistentDataPath);

        //Creates a new FileSave object with the file format XML.
        FileSave fileSave = new FileSave(FileFormat.Xml);

        //Writes an XML file to the path.
        fileSave.WriteToFile(Application.persistentDataPath + "/myFile.xml", new MyClass(42, new List<int> { 1, 2, 3 }));

        //Changes the file format to Binary file
        fileSave.fileFormat = FileFormat.Binary;

        //Writes a binary file
        fileSave.WriteToFile(Application.persistentDataPath + "/myFile.bin", new MyClass());

        //Changes the file format back to XML...
        fileSave.fileFormat = FileFormat.Xml;

        //...and loads the data from the Xml file
        MyClass myClass = fileSave.ReadFromFile<MyClass>(Application.persistentDataPath + "/myFile.xml");
        Debug.Log("Loaded data: " + myClass);
    }
}
