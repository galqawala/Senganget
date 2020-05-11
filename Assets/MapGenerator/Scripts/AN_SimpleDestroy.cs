using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_SimpleDestroy : MonoBehaviour
{
    [Range(0, 100)]
    public int Chance = 49;
    public bool DestroyChildrens = false;
    [Range(1, 10)]
    public int LeaveCount = 1;

    int i;

    void Start()
    {
        if (DestroyChildrens)
        {
            StartCoroutine(Delete());
        }
        else
        {
            i = Random.Range(0, 100);
            if (i > Chance) Destroy(gameObject);
        }
    }

    IEnumerator Delete()
    {
        int itarations = transform.childCount - LeaveCount;
        for (int k = 0; k < itarations; k++)
        {
            i = Random.Range(0, transform.childCount);
            //Debug.Log(i);
            Destroy(transform.GetChild(i).gameObject);
            yield return new WaitForEndOfFrame();
        }
        //Debug.Log("deleted");
    }
}
