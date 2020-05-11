using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
 
public class SaveLoad {
    [Serializable]
    public class SaveData
    {
        public float x;
        public float y;
        public float z;
    }
             
    public void Save() {
        GameObject player = GameObject.Find("Player");
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create (Application.persistentDataPath + "/SengangetSave"); //you can call it anything you want

        SaveData data = new SaveData();
        data.x = player.transform.position.x;
        data.y = player.transform.position.y;
        data.z = player.transform.position.z;
        bf.Serialize(file, data);

        file.Close();
    }   
     
    public void Load() {
        if(File.Exists(Application.persistentDataPath + "/SengangetSave")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/SengangetSave", FileMode.Open);
            GameObject player = GameObject.Find("Player");
            SaveData data = (SaveData)bf.Deserialize(file);
            player.transform.position = new Vector3(data.x, data.y, data.z);
            file.Close();
        }
    }
}