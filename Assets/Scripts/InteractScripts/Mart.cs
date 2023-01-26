using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mart : MonoBehaviour
{
    [SerializeField] private float buyMeatDeley = 5f;
    [SerializeField] private int meatPrice = 10000;
    [SerializeField] private GameObject meatPrefab = null;

    public float GetBuyMeatDeley()
    {
        return buyMeatDeley;
    }
    public int GetMeatPrice()
    {
        return meatPrice;
    }

    public GameObject GetMeatPrefab() {
        return meatPrefab;
    }

}
