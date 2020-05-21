using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    PlayerInputHandler m_InputHandler;
    PlayerWeaponsManager m_PlayerWeaponsManager;
    Transform canvas;
    Transform table;
    GameObject rowPrefab;

    void Start()
    {
        m_InputHandler = FindObjectOfType<PlayerInputHandler>();
        Assert.IsNotNull(m_InputHandler);

        m_PlayerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
        Assert.IsNotNull(m_PlayerWeaponsManager);

        canvas = transform.Find("Canvas");
        Assert.IsNotNull(canvas);
        
        table = canvas.Find("Table");
        Assert.IsNotNull(table);

        rowPrefab = (GameObject) Resources.Load("WeaponRow");
        Assert.IsNotNull(rowPrefab);
    }


    void Update()
    {
        if (m_InputHandler.GetInventoryInputDown()) {
            canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);

            if (canvas.gameObject.activeSelf) {
                ClearRows(table);
                for (int i = 0; i < m_PlayerWeaponsManager.m_WeaponSlots.Length; i++)
                {
                    var weapon = m_PlayerWeaponsManager.m_WeaponSlots[i];
                    if (weapon) {
                        var row = (GameObject) Instantiate(rowPrefab, Vector3.zero, Quaternion.identity, table);
                        row.name = "RowWeapon"+(i+1);
                        SetText(row, "Index", (i+1).ToString());
                        SetText(row, "Name", weapon.weaponName);
                        
                        //Ammo
                        if (weapon.ammoType >= 0) {
                            SetText(row, "Ammo", PlayerPrefs.GetInt("ammo"+weapon.ammoType)+$" ({weapon.ammoType})");
                        } else {
                            SetText(row, "Ammo", "∞");
                        }
                        
                        SetText(row, "DelayBetweenShots", weapon.delayBetweenShots.ToString("0.00"));
                    }
                }
            }
        }
    }

    void ClearRows(Transform table) {
        //delete all rows except header
        for (int i = table.childCount-1; i>0; i--)
        {
            Destroy(table.GetChild(i).gameObject);            
        }
    }

    void SetText(GameObject row, string column, string value) {
        var cellName = row.transform.Find(column+"/Text");
        Assert.IsNotNull(cellName);
        TMPro.TextMeshProUGUI textName = cellName.GetComponent<TMPro.TextMeshProUGUI>();
        textName.text = value;
    }
}
