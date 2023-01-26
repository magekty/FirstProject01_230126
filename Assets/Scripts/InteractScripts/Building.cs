using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private int incomeMoney = 500;
    [SerializeField] private int jobsDoneMoney = 5000;
    [SerializeField] private float jobsDoneDeley = 10f;

    public float GetJobsDoneDeley()
    {
        return jobsDoneDeley;
    }
    public int GetMoney()
    {
        return incomeMoney;
    }
    public int GetJobsDoneMoney()
    {
        return jobsDoneMoney;
    }
}
