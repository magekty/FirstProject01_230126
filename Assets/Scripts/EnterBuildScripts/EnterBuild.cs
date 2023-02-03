using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterBuild : MonoBehaviour
{
    [SerializeField] GameObject roofGo = null;
    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Player") && isMine(_other))
            roofGo.gameObject.SetActive(false);
    }
    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("Player") && isMine(_other))
            roofGo.gameObject.SetActive(true);
    }

    private bool isMine(Collider _other)
    {
        return _other.GetComponent<PhotonView>().IsMine;
    }
}
