using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;


public class Bullet : MonoBehaviourPun
{
    private bool isShoot = false;
    private Vector3 direction = Vector3.zero;
    private float speed = 10.0f;
    private float duration = 5.0f;
    private GameObject owner = null;


    private void Update()
    {
        if (isShoot)
        {
            this.transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    public void Shoot(GameObject _owner, Vector3 _dir)
    {
        owner = _owner;
        direction = _dir;
        isShoot = true;

        Invoke("SelfDestroy", duration);
    }

    private void SelfDestroy()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if( owner != other.gameObject &&
            other.CompareTag("Player"))
        {
            other.GetComponent<PlayerCtrl>().OnDamage(1);
            SelfDestroy();
        }
    }
}