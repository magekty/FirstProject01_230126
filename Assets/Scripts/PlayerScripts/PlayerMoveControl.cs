using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoveControl : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private GameObject psGo;
    [SerializeField] private TextMesh tm = null;

    private Animator anim = null;
    private List<Collider> colList = new List<Collider>();
    private Camera cm = null;
    private CharacterController cc = null;
    private Vector3 movePoint;
    private Collider closeCollider = null;
    private GameObject garbagesGo = null;

    private bool isHandEmpty = true;
    private bool isMove = false;
    private bool isStart = false;
    private bool isPickupKeyPress = false;

    private float countDeley = 0f;
    private float tempSearchDeley = 0f;
    private float keyboardDeley = 0f;


    private void Awake()
    {
        anim = GetComponent<Animator>();
        movePoint = GetPos();
        cc = GetComponent<CharacterController>();
        if (!photonView.IsMine) return;
        cm = Camera.main;
        cm.transform.parent = this.transform;
        cm.transform.localPosition = new Vector3(0f, 20f, -15f);
        cm.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
        tm.text = photonView.Owner.NickName;

    }
    private void Start()
    {
        if (!photonView.IsMine) return;
        psGo = GameObject.FindWithTag("SearchParticle");
        psGo.transform.parent = this.transform;
        psGo.transform.localPosition = new Vector3(0f, 0.2f, 0f);
        psGo.SetActive(false);
    }

    private void Update()
    {
        AnimatorController();
        if (!photonView.IsMine) return;
        countDeley += Time.deltaTime;

        InputMouseRightClick();
        if (!isStart) return;
        // ��Ŭ�� ������ �ݱ�, ������ �Լ�
        InputMouseLeftClick();
        InputKeyboardSpace();

    }
    private void AnimatorController()
    {
        if (isMove)
            anim.SetBool("isMove", true);
        else
            anim.SetBool("isMove", false);
    }



    private void InputKeyboardSpace()
    {
        if (!psGo.activeSelf && Input.GetKeyDown("space"))
        {
            tempSearchDeley = countDeley;
            psGo.SetActive(true);
        }
        if(psGo.activeSelf && countDeley - tempSearchDeley > 30f)
        {
            psGo.SetActive(false);
        }
    }

    private void InputMouseLeftClick()
    {
        if (!isPickupKeyPress && Input.GetMouseButtonUp(0))
        {
            photonView.RPC("GetItem", RpcTarget.All);
        }
    }
    [PunRPC]
    public void GetItem()
    {
        // ���� ����������
        if (isHandEmpty)
        {
            // ���� ����� �ݱⰡ���� ������Ʈ�� ����
            if (colList.Count == 0) return;
            if (colList[0] != null)
                closeCollider = colList[0];

            for (int i = 1; i < colList.Count; i++)
            {
                if (colList[i] == null) continue;
                if (colList[i].transform.parent.CompareTag("Player")) continue;
                if (Vector3.Distance(GetPos(), closeCollider.transform.position) >
                    Vector3.Distance(GetPos(), colList[i].transform.position))
                {
                    closeCollider = colList[i];
                }
                
            }

            // ���������� ������ ���ӿ�����Ʈ�� �ݱ�
            colList.Clear();
            garbagesGo = closeCollider.transform.parent.gameObject;
            closeCollider.transform.SetParent(transform);
            closeCollider.transform.position = transform.position;
            closeCollider.transform.localPosition += new Vector3(0f, 0f, 2f);
            closeCollider = null;
            isHandEmpty = false;
            moveSpeed = 6f;
        }
        // �տ� ���� �������
        else
        {
            // �ڽĿ�����Ʈ�� ���ӵ� �ݱ� ������ �������� ����
            Transform[] trArr = transform.GetComponentsInChildren<Transform>();
            foreach (Transform tr in trArr)
            {
                if (tr.CompareTag("Can be picked up"))
                {
                    tr.parent = garbagesGo.transform;
                }
            }
            isHandEmpty = true;
            moveSpeed = 10f;
        }
        isPickupKeyPress = false;
    }
    private void OnTriggerEnter(Collider _other)
    {
        // Ʈ���ſ� ���� ���� �ݶ��̴��� �迭�� �־ ó��
        if (_other.CompareTag("Can be picked up") && !_other.transform.parent.CompareTag("Player"))
        {
            colList.Add(_other);
        }

/*        if (!_other.CompareTag("Floor") && !_other.CompareTag("Player"))
        {
            isMove = false;
        }*/
    }


    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("Can be picked up") && colList.Contains(_other))
        {
            colList.Remove(_other);
        }
    }




    private void InputMouseRightClick()
    {
        // ��Ŭ�� �̺�Ʈ�� ���Դٸ�
        if (Input.GetMouseButtonUp(1))
        {
            // ī�޶󿡼� �������� ���.
            Ray ray = cm.ScreenPointToRay(Input.mousePosition);
            // Scence ���� ī�޶󿡼� ������ ������ ������ Ȯ���ϱ�
            //Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);

            // �������� ������ �¾Ҵٸ�
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                // ���� ��ġ�� �������� ����
                movePoint = raycastHit.point;
                movePoint.y = 0f;
                //Debug.Log("movePoint : " + movePoint.ToString());
                //Debug.Log("���� ��ü : " + raycastHit.transform.name);
                isMove = true;
                photonView.RPC("RPCisMoveTrue", RpcTarget.Others);
            }
        }

        // ���������� �Ÿ��� 0.1f ���� �ִٸ�
        if (isMove && Vector3.Distance(GetPos(), movePoint) > 0.2f)
        {
            // �̵�
            Move();
        }
        else
        {
            photonView.RPC("RPCisMoveFalse", RpcTarget.Others);

            isMove = false;
        }

    }

    private void Move()
    {
        if (isMove && Vector3.Distance(GetPos(), movePoint) <= 0.2f)
        {

            photonView.RPC("RPCisMoveFalse", RpcTarget.Others);

            isMove = false;
        }
        // thisUpdatePoint �� �̹� ������Ʈ(������) ���� �̵��� ����Ʈ�� ��� ������.
        // �̵��� ����(�̵��� ��-���� ��ġ) ���ϱ� �ӵ��� �ؼ� �̵��� ��ġ���� ����Ѵ�.
        Vector3 thisUpdatePoint = (movePoint - transform.position).normalized * moveSpeed;
        // characterController �� ĳ���� �̵��� ����ϴ� ������Ʈ��.
        // simpleMove �� �ڵ����� �߷��� ����ؼ� �̵������ִ� �޼ҵ��.
        // ������ �̵��� ����Ʈ�� �������ָ� �ȴ�.
        cc.SimpleMove(thisUpdatePoint);
        //Debug.Log($"curPos : {GetPos()} movePos : {movePoint}");
        thisUpdatePoint.y = 0f;
        // ������ ȭ�� ȸ��
        //transform.LookAt(thisUpdatePoint);
        //thisUpdatePoint = new Vector3(transform.rotation.x, thisUpdatePoint.y, 0f);
        // Lerp�� �̿��� �ε巯�� ȭ�� ȸ��
        Quaternion targetRot = Quaternion.LookRotation(thisUpdatePoint);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

    }

    private Vector3 GetPos()
    {
        return transform.position;
    }

    // PhotonNetwork.Instantiate�� ��ü�� �����Ǹ� ȣ��Ǵ� �ݹ��Լ�
    // -> IPunInstantiateMagicCallback �ʿ�
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // ��ü���� �����ϱ� ������ �����͸� ó��
        if (!PhotonNetwork.IsMasterClient) return;

        // ���ӸŴ����� ���ǵǾ� �ִ� �Լ� ȣ��
        FindObjectOfType<GameManager>().ApplyPlayerList();
    }
    [PunRPC]
    public void RPCisMoveTrue()
    {
        isMove = true;
    }
    [PunRPC]
    public void RPCisMoveFalse()
    {
        isMove = false;
    }
    public void SetPlayerName()
    {
        tm.text = photonView.Owner.NickName;
        
    }
    public void SetColorRed()
    {
        tm.color = Color.red;
    }
    public void SetColorBlue()
    {
        tm.color= Color.blue;
    }

    public string GetNickName()
    {
        return photonView.Owner.NickName;
    }

    public void SetStart()
    {
        if(photonView.IsMine)
            isStart = true;
    }

    public void SetEnd()
    {
        if (photonView.IsMine)
            isStart = false;
    }

}
