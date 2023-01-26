using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meat : MonoBehaviour
{
    [SerializeField] private float cookDeley = 5f;

    public float GetCookDeley()
    {
        return cookDeley;
    }
}
