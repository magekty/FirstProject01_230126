using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;

    private List<Collider> colList = new List<Collider>();
    private Camera cm = null;
    private CharacterController cc = null;
    private Vector3 movePoint;
    private Collider closeCollider = null;
    private GameObject meatGo = null;

    private bool isHandEmpty = true;
    private bool isFull = false;

    private int money = 0;
    private float countDeley = 0f;

    private float tempJobsDoneDeley = 0f;
    private float tempIncomeDeley = 0f;
    private float tempBuyMeatDeley = 0f;
    private float tempCookDeley = 0f;
    private float tempFullDeley = 0f;


    private void Awake()
    {
        movePoint = transform.position;
        cc = GetComponent<CharacterController>();
        cm = Camera.main;

    }

    private void Update()
    {
        countDeley += Time.deltaTime;
        // 우클릭 이동함수
        InputMouseRightClick();
        // Q버튼 누르면 줍기, 버리기 함수
        InputKeyboardQ();
        InputKeyboardE();
        PlayerStatusController();
    }
    private void PlayerStatusController()
    {
        if (isFull && countDeley - tempFullDeley > 60f)
            isFull = false;
    }
    private void InputKeyboardE()
    {
        if (Input.GetKeyDown("e") && !isHandEmpty)
        {
            Transform[] trArr = transform.GetComponentsInChildren<Transform>();
            Food food = null;
            foreach (Transform tr in trArr)
            {
                if (!tr.TryGetComponent<Food>(out food)) continue;
                if (food.IsFood())
                {
                    isFull = true;
                    tempFullDeley = countDeley;
                    Destroy(food.gameObject);
                }
            }

        }
    }

    private void InputKeyboardQ()
    {
        if (Input.GetKeyDown("q"))
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
                    if (Vector3.Distance(GetPos(), closeCollider.transform.position) >
                        Vector3.Distance(GetPos(), colList[i].transform.position))
                    {
                        closeCollider = colList[i];
                    }
                }
                // 최종적으로 선별된 게임오브젝트를 줍기
                closeCollider.gameObject.transform.SetParent(transform);
                isHandEmpty = false;
            }
            // 손에 무언가 있을경우
            else
            {
                // 자식오브젝트로 종속된 줍기 가능한 아이템을 해제
                Transform[] trArr = transform.GetComponentsInChildren<Transform>();
                foreach (Transform tr in trArr)
                {
                    if (tr.CompareTag("Can be picked up"))
                        tr.parent = null;
                } 
                isHandEmpty = true;
            }

        }
    }
    private void OnTriggerEnter(Collider _other)
    {
        // 트리거에 들어온 다중 콜라이더를 배열에 넣어서 처리
        if (_other.CompareTag("Can be picked up"))
            colList.Add(_other);
        if (_other.CompareTag("Can be interact_A"))
        {
            if (_other.GetComponent<Building>() != null)
            {
                tempJobsDoneDeley = countDeley;
                tempIncomeDeley = countDeley;
            }
            if (_other.GetComponent<Mart>() != null)
            {
                tempBuyMeatDeley = countDeley;
            }
            if (_other.GetComponent<Home>() != null)
            {
                tempCookDeley = countDeley;
            }
        }

    }
    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("Can be picked up"))
            colList.Remove(_other);
    }
    private void OnTriggerStay(Collider _other)
    {
        if (_other.CompareTag("Can be interact_A"))
        {
            WorkingOnIt(_other);
            BuyingAtTheMart(_other);
            CookingAtTheHome(_other);
        }
    }

    private void CookingAtTheHome(Collider _other)
    {
        if (_other.GetComponent<Home>() == null) return;
        Home tempHome = _other.GetComponent<Home>();
        if (isHandEmpty) return;
        if (countDeley - tempCookDeley > tempHome.GetCookMeatDeley())
        {
            Vector3 foodPos = GetPos();
            foodPos += transform.right;
            Destroy(meatGo);
            Instantiate(tempHome.GetFoodPrefab(), foodPos, Quaternion.identity, transform);
            tempCookDeley = countDeley;
            Debug.Log($"maked Food");
        }

    }
    private void BuyingAtTheMart(Collider _other)
    {
        if (_other.GetComponent<Mart>() == null) return;
        Mart tempMart = _other.GetComponent<Mart>();
        if (!isHandEmpty || money < tempMart.GetMeatPrice()) return;
        if (countDeley - tempBuyMeatDeley > tempMart.GetBuyMeatDeley())
        {
            money -= tempMart.GetMeatPrice();
            Vector3 meatPos = GetPos();
            meatPos += transform.right;
            meatGo = Instantiate(tempMart.GetMeatPrefab(), meatPos, Quaternion.identity,transform);
            isHandEmpty = false;
            tempBuyMeatDeley = countDeley;
            Debug.Log($"currentMoney : {money}");
        }

    }

    private void WorkingOnIt(Collider _other)
    {
        if (_other.GetComponent<Building>() == null) return;
        Building tempBuilding = _other.GetComponent<Building>();
        if (countDeley - tempJobsDoneDeley > tempBuilding.GetJobsDoneDeley())
        {
            money += tempBuilding.GetJobsDoneMoney();
            tempJobsDoneDeley = countDeley;
        }
        if (countDeley - tempIncomeDeley > 1f)
        {
            money += tempBuilding.GetMoney();
            tempIncomeDeley = countDeley;
            Debug.Log($"currentMoney : {money}");
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
                //Debug.Log("movePoint : " + movePoint.ToString());
                //Debug.Log("맞은 객체 : " + raycastHit.transform.name);

            }
        }

        // 목적지까지 거리가 0.1f 보다 멀다면
        if (Vector3.Distance(GetPos(), movePoint) > 0.1f)
        {
            // 이동
            Move();
        }
    }

    private void Move()
    {
        // thisUpdatePoint 는 이번 업데이트(프레임) 에서 이동할 포인트를 담는 변수다.
        // 이동할 방향(이동할 곳-현재 위치) 곱하기 속도를 해서 이동할 위치값을 계산한다.
        Vector3 thisUpdatePoint = (movePoint - transform.position).normalized * moveSpeed;
        // characterController 는 캐릭터 이동에 사용하는 컴포넌트다.
        // simpleMove 는 자동으로 중력을 계산해서 이동시켜주는 메소드다.
        // 값으로 이동할 포인트를 전달해주면 된다.
        cc.SimpleMove(thisUpdatePoint);
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


}
