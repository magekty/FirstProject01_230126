using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using System.Runtime.InteropServices;


public class PlayerCtrl : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private Rigidbody rb = null;

    [SerializeField] private GameObject bulletPrefab = null;

    [SerializeField] private Color[] colors = null;
    [SerializeField] private float speed = 3.0f;

    private int hp = 3;
    private bool isDead = false;


    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        isDead = false;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (isDead) return;

        if (Input.GetKey(KeyCode.W))
            rb.AddForce(Vector3.forward * speed);
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(Vector3.back * speed);
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(Vector3.left * speed);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(Vector3.right * speed);

        if (Input.GetMouseButtonDown(0)) ShootBullet();

        LookAtMouseCursor();
    }

    public void SetMaterial(int _colorIdx)
    {
        //Debug.LogError(_colorIdx + " : " + colors.Length);
        //playerIdx = _colorIdx;

        if (_colorIdx > colors.Length-1) return;
        this.GetComponent<MeshRenderer>().material.color = colors[_colorIdx];
    }


    private void ShootBullet()
    {
        if (bulletPrefab)
        {
            GameObject go = PhotonNetwork.Instantiate(
                bulletPrefab.name,
                this.transform.position,
                Quaternion.identity);
            go.GetComponent<Bullet>().Shoot(this.gameObject, this.transform.forward);
        }
    }

    public void LookAtMouseCursor()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(this.transform.position);
        Vector3 dir = mousePos - playerPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.AngleAxis(-angle + 90.0f, Vector3.up);
    }

    [PunRPC]
    public void ApplyHp(int _hp)
    {
        hp = _hp;
        Debug.LogErrorFormat("{0} Hp: {1}",
            PhotonNetwork.NickName,
            hp
            );

        if (hp <= 0)
        {
            Debug.LogErrorFormat("Destroy: {0}", PhotonNetwork.NickName);
            isDead = true;
            PhotonNetwork.Destroy(this.gameObject);
        }
    }

    [PunRPC]
    public void OnDamage(int _dmg)
    {
        hp -= _dmg;
        photonView.RPC("ApplyHp", RpcTarget.Others, hp);
    }

    // PhotonNetwork.Instantiate로 객체가 생성되면 호출되는 콜백함수
    // -> IPunInstantiateMagicCallback 필요
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // 전체에게 통지하기 떄문에 마스터만 처리
        if (!PhotonNetwork.IsMasterClient) return;

        // 게임매니저에 정의되어 있는 함수 호출
        FindObjectOfType<GameManager>().ApplyPlayerList();
    }

}