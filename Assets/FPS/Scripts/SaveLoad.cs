using System.IO;
using UnityEngine;
 
public class SaveLoad {
    //If something big has been achieved: https://docs.unity3d.com/ScriptReference/PlayerPrefs.Save.html

    public void Save() {
        GameObject player = GameObject.Find("Player");

        PlayerPrefs.SetFloat("position.x", player.transform.position.x);
        PlayerPrefs.SetFloat("position.y", player.transform.position.y);
        PlayerPrefs.SetFloat("position.z", player.transform.position.z);

        //This saves rotation around Y (heading), but not vertical angle that comes from GetLookInputsVertical()
        PlayerPrefs.SetFloat("eulerAngles.x", player.transform.eulerAngles.x);
        PlayerPrefs.SetFloat("eulerAngles.y", player.transform.eulerAngles.y);
        PlayerPrefs.SetFloat("eulerAngles.z", player.transform.eulerAngles.z);

        Health health = player.GetComponentInChildren<Health>();
        PlayerPrefs.SetFloat("health.currentHealth", health.currentHealth);
    }   
     
    public void Load() {
        GameObject player = GameObject.Find("Player");

        if (PlayerPrefs.HasKey("position.x") && PlayerPrefs.HasKey("position.y") && PlayerPrefs.HasKey("position.z")) {
            player.transform.position = new Vector3(
                PlayerPrefs.GetFloat("position.x")
            ,   PlayerPrefs.GetFloat("position.y")
            ,   PlayerPrefs.GetFloat("position.z")
            );
        }

        if (PlayerPrefs.HasKey("eulerAngles.x") && PlayerPrefs.HasKey("eulerAngles.y") && PlayerPrefs.HasKey("eulerAngles.z")) {
            player.transform.eulerAngles = new Vector3(
                PlayerPrefs.GetFloat("eulerAngles.x")
            ,   PlayerPrefs.GetFloat("eulerAngles.y")
            ,   PlayerPrefs.GetFloat("eulerAngles.z")
            );
        }

        Health health = player.GetComponentInChildren<Health>();
        if (PlayerPrefs.HasKey("health.currentHealth")) health.currentHealth = PlayerPrefs.GetFloat("health.currentHealth");
    }
}