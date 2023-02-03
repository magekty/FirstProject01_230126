using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameController : MonoBehaviour
{
    public GameObject mc;

    private void Awake()
    {
        mc = Camera.main.gameObject;
    }
    void Update()
    {
        transform.rotation = mc.transform.rotation;
    }
}
