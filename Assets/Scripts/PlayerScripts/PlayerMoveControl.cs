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
        // 좌클릭 누르면 줍기, 버리기 함수
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
        // 손이 비어있을경우
        if (isHandEmpty)
        {
            // 가장 가까운 줍기가능한 오브젝트를 선별
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

            // 최종적으로 선별된 게임오브젝트를 줍기
            colList.Clear();
            garbagesGo = closeCollider.transform.parent.gameObject;
            closeCollider.transform.SetParent(transform);
            closeCollider.transform.position = transform.position;
            closeCollider.transform.localPosition += new Vector3(0f, 0f, 2f);
            closeCollider = null;
            isHandEmpty = false;
            moveSpeed = 6f;
        }
        // 손에 무언가 있을경우
        else
        {
            // 자식오브젝트로 종속된 줍기 가능한 아이템을 해제
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
        // 트리거에 들어온 다중 콜라이더를 배열에 넣어서 처리
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
        // 우클릭 이벤트가 들어왔다면
        if (Input.GetMouseButtonUp(1))
        {
            // 카메라에서 레이저를 쏜다.
            Ray ray = cm.ScreenPointToRay(Input.mousePosition);
            // Scence 에서 카메라에서 나오는 레이저 눈으로 확인하기
            //Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);

            // 레이저가 뭔가에 맞았다면
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                // 맞은 위치를 목적지로 저장
                movePoint = raycastHit.point;
                movePoint.y = 0f;
                //Debug.Log("movePoint : " + movePoint.ToString());
                //Debug.Log("맞은 객체 : " + raycastHit.transform.name);
                isMove = true;
                photonView.RPC("RPCisMoveTrue", RpcTarget.Others);
            }
        }

        // 목적지까지 거리가 0.1f 보다 멀다면
        if (isMove && Vector3.Distance(GetPos(), movePoint) > 0.2f)
        {
            // 이동
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
        // thisUpdatePoint 는 이번 업데이트(프레임) 에서 이동할 포인트를 담는 변수다.
        // 이동할 방향(이동할 곳-현재 위치) 곱하기 속도를 해서 이동할 위치값을 계산한다.
        Vector3 thisUpdatePoint = (movePoint - transform.position).normalized * moveSpeed;
        // characterController 는 캐릭터 이동에 사용하는 컴포넌트다.
        // simpleMove 는 자동으로 중력을 계산해서 이동시켜주는 메소드다.
        // 값으로 이동할 포인트를 전달해주면 된다.
        cc.SimpleMove(thisUpdatePoint);
        //Debug.Log($"curPos : {GetPos()} movePos : {movePoint}");
        thisUpdatePoint.y = 0f;
        // 간단한 화면 회전
        //transform.LookAt(thisUpdatePoint);
        //thisUpdatePoint = new Vector3(transform.rotation.x, thisUpdatePoint.y, 0f);
        // Lerp를 이용한 부드러운 화면 회전
        Quaternion targetRot = Quaternion.LookRotation(thisUpdatePoint);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

    }

    private Vector3 GetPos()
    {
        return transform.position;
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
