using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Parameters")]
    [Tooltip("Type of ammo to pickup")]
    public int ammoType = 0;
    public int numberOfAmmo = 2;

    Pickup m_Pickup;

    void Start()
    {
        m_Pickup = GetComponent<Pickup>();
        DebugUtility.HandleErrorIfNullGetComponent<Pickup, AmmoPickup>(m_Pickup, this, gameObject);

        // Subscribe to pickup action
        m_Pickup.onPick += OnPicked;
    }

    void OnPicked(PlayerCharacterController player)
    {
        var ammoCount = PlayerPrefs.GetInt("ammo"+ammoType)+numberOfAmmo;
        PlayerPrefs.SetInt("ammo"+ammoType, ammoCount);
        Debug.Log($"You now have {ammoCount} of ammo {ammoType}");
        Destroy(gameObject);
    }
}
