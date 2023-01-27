using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoveController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private GameObject meleeAttackGo = null;
    [SerializeField] private float kickSpeed = 5f;
    [SerializeField] private float forcePower = 0.01f;


    private List<Collider> colList = new List<Collider>();
    private Camera cm = null;
    private CharacterController cc = null;
    private Vector3 movePoint;
    private Vector3 attackLocalPos;
    private Collider closeCollider = null;
    private GameObject meatGo = null;

    private bool isHandEmpty = true;
    private bool isFull = false;
    private bool isPunch = false;
    private bool isAttacked = false;
    private bool isMove = false;

    private int money = 0;
    private float countDeley = 0f;

    private float tempJobsDoneDeley = 0f;
    private float tempIncomeDeley = 0f;
    private float tempBuyMeatDeley = 0f;
    private float tempCookDeley = 0f;
    private float tempFullDeley = 0f;
    private float tempMeleeDeley = 0f;
    private float tempAttackedDeley = 0f;




    private void Awake()
    {
        movePoint = GetPos();
        cc = GetComponent<CharacterController>();
        cm = Camera.main;

    }
    private void Start()
    {
        attackLocalPos = GetAttackPos();
    }

    private void Update()
    {
        countDeley += Time.deltaTime;
        // ��Ŭ�� �̵��Լ�
        InputMouseRightClick();
        InputMouseLeftClick();
        // Q��ư ������ �ݱ�, ������ �Լ�
        InputKeyboardQ();
        InputKeyboardE();
        PlayerStatusController();
    }
    private void PlayerStatusController()
    {
        if (isFull && countDeley - tempFullDeley > 60f)
            isFull = false;
    }
    private void InputMouseLeftClick()
    {
        if (Input.GetMouseButtonUp(0) && !isPunch)
        {
            meleeAttackGo.SetActive(true);
            tempMeleeDeley = countDeley;
            StartCoroutine("MeleeAttack");
            isPunch = true;
        }


    }

    private IEnumerator MeleeAttack()
    {
        float t = 0f ;
        while (true)
        {
            t += Time.deltaTime;
            meleeAttackGo.transform.localPosition = Vector3.Lerp(attackLocalPos, 
                attackLocalPos + Vector3.forward, t * kickSpeed);
            if (isPunch && countDeley - tempMeleeDeley > 1f)
            {
                StopCoroutine("MeleeAttack");
                meleeAttackGo.SetActive(false);
                isPunch = false;
                meleeAttackGo.transform.localPosition = attackLocalPos;

            }
            //meleeAttackGo.transform.Translate(Vector3.forward * Time.deltaTime * punchSpeed);
            yield return null;
        }
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
                    if (Vector3.Distance(GetPos(), closeCollider.transform.position) >
                        Vector3.Distance(GetPos(), colList[i].transform.position))
                    {
                        closeCollider = colList[i];
                    }
                }
                // ���������� ������ ���ӿ�����Ʈ�� �ݱ�
                closeCollider.gameObject.transform.SetParent(transform);
                isHandEmpty = false;
            }
            // �տ� ���� �������
            else
            {
                // �ڽĿ�����Ʈ�� ���ӵ� �ݱ� ������ �������� ����
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
        // Ʈ���ſ� ���� ���� �ݶ��̴��� �迭�� �־ ó��
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
        if (_other.CompareTag("Melee"))
        {
            Debug.Log("melee");
            isAttacked = true;
            Vector3 a = (transform.position - _other.transform.position) * forcePower;
            a.y = 0;
            tempAttackedDeley = countDeley;
            StartCoroutine("AttackedAfter", a);
            isMove = false;
        }
        if (!_other.CompareTag("Player"))
        {
            isMove = false;
        }
    }
    private IEnumerator AttackedAfter(Vector3 _a)
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime;
            this.transform.Translate(_a);
            Debug.Log("Start");
            if (isAttacked && countDeley - tempAttackedDeley > 1f)
            {
                StopCoroutine("AttackedAfter");
                Debug.Log("Stop");
                isAttacked = false;
            }
            yield return null;
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
                //Debug.Log("movePoint : " + movePoint.ToString());
                //Debug.Log("���� ��ü : " + raycastHit.transform.name);
                isMove = true;
            }
        }

        // ���������� �Ÿ��� 0.1f ���� �ִٸ�
        if (isMove && Vector3.Distance(GetPos(), movePoint) > 0.2f)
        {
            // �̵�
            Move();
        }
    }

    private void Move()
    {
        if (Vector3.Distance(GetPos(), movePoint) < 1.9f)
        {
            isMove = false;
            Debug.Log("MoveStop");
        }
        // thisUpdatePoint �� �̹� ������Ʈ(������) ���� �̵��� ����Ʈ�� ��� ������.
        // �̵��� ����(�̵��� ��-���� ��ġ) ���ϱ� �ӵ��� �ؼ� �̵��� ��ġ���� ����Ѵ�.
        Vector3 thisUpdatePoint = (movePoint - transform.position).normalized * moveSpeed;
        // characterController �� ĳ���� �̵��� ����ϴ� ������Ʈ��.
        // simpleMove �� �ڵ����� �߷��� ����ؼ� �̵������ִ� �޼ҵ��.
        // ������ �̵��� ����Ʈ�� �������ָ� �ȴ�.
        cc.SimpleMove(thisUpdatePoint);
        Debug.Log($"curPos : {GetPos()} movePos : {movePoint}");
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

    private Vector3 GetAttackPos()
    {
        return meleeAttackGo.transform.localPosition;
    }


}
