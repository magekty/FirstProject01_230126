using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Home : MonoBehaviour
{
    [SerializeField] private float CookMeatDeley = 5f;
    [SerializeField] private GameObject foodPrefab = null;

    public float GetCookMeatDeley()
    {
        return CookMeatDeley;
    }

    public GameObject GetFoodPrefab()
    {
        return foodPrefab;
    }
}
