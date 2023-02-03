using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private GameObject[] tmpGoList = null;
    [SerializeField] private Button startBtn = null;
    [SerializeField] private TMP_Text tmpStart = null;
    [SerializeField] private TMP_Text tmpCount = null;
    [SerializeField] private TMP_Text tmpEnd = null;
    [SerializeField] private Transform[] garbageList = null;
    [SerializeField] private int endTime = 300;
    [SerializeField] private GameObject garbagesGo = null;

    // 각 클라이언트 마다 생성된 플레이어 게임 오브젝트를 리스트로 관리
    private List<GameObject> playerGoList = new List<GameObject>();
    private int[] playerIdx = { 0, 0, 0, 0, 0, 0, 0, 0 };
    private GameObject go = null;
    private List<GameObject> teamOneGo = new List<GameObject>();
    private List<GameObject> teamTwoGo = new List<GameObject>();
    private int blueTeamScore = 0;
    private int redTeamScore = 0;

    private void Start()
    {
        garbageList = garbagesGo.GetComponentsInChildren<Transform>();
        
        if (playerPrefab != null)
        {
            go = PhotonNetwork.Instantiate($"Prefabs\\{playerPrefab.name}",
                new Vector3(Random.Range(-20.0f, 20.0f), 0.0f,Random.Range(-15.0f, 15.0f)),
                Quaternion.identity, 0);
        }
        //Debug.LogError($"photonView : {photonView.ViewID}");
        if (!PhotonNetwork.IsMasterClient) startBtn.gameObject.SetActive(false);
    }

    // PhotonNetwork.LeaveRooom 함수가 호출되면 호출
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Launcher");
    }

    // 플레이어가 입장할 때 호출되는 함수
    public override void OnPlayerEnteredRoom(Player otherPlayer)
    {
        //Debug.LogFormat("Player Entered Room: {0}",otherPlayer.NickName);
    }

    public void ApplyPlayerList()
    {
        // 전체 클라이언트에서 함수 호출
        photonView.RPC("RPCApplyPlayerList", RpcTarget.All);
    }

    [PunRPC]
    public void RPCApplyPlayerList()
    {
        int playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        // 플레이어 리스트가 최신이라면 건너뜀
        //if (playerCnt == playerGoList.Count) return;

        // 현재 방에 접속해 있는 플레이어의 수
        //Debug.LogError("CurrentRoom PlayerCount : " + playerCnt);
        // 현재 생성되어 있는 모든 포톤뷰 가져오기
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();

        // 매번 재정렬을 하는게 좋으므로 플레이어 게임오브젝트 리스트를 초기화
        playerGoList.Clear();
        // 현재 생성되어 있는 포톤뷰 전체와
        // 접속중인 플레이어들의 액터넘버를 비교해,
        // 플레이어 게임오브젝트 리스트에 추가
        List<int> ints = new List<int>();
        
        foreach (var item in PhotonNetwork.CurrentRoom.Players)
        {
            ints.Add(PhotonNetwork.CurrentRoom.Players[item.Key].ActorNumber);
            //Debug.LogError($"key : {item.Key}");
        }
       /* {
            Debug.LogError($"key1 : {item.Owner.ActorNumber}");
        }*/
        for (int i = 0; i < playerCnt; ++i)
        {
            // 키는 0이 아닌 1부터 시작
            for (int j = 0; j < photonViews.Length; ++j)
            {
                // 만약 PhotonNetwork.Instantiate를 통해서 생성된 포톤뷰가 아니라면 넘김
                if (photonViews[j].isRuntimeInstantiated == false) continue;
                // 만약 현재 키 값이 딕셔너리 내에 존재하지 않는다면 넘김
                if (PhotonNetwork.CurrentRoom.Players.ContainsKey(ints[i]) == false) continue;

                // 포톤뷰의 액터넘버
                int viewNum = photonViews[j].Owner.ActorNumber;
                // 접속중인 플레이어의 액터넘버
                int playerNum = PhotonNetwork.CurrentRoom.Players[ints[i]].ActorNumber;

                // 액터넘버가 같은 오브젝트가 있다면,
                if (viewNum == playerNum)
                {
                    // 게임오브젝트 이름도 알아보기 쉽게 변경
                    photonViews[j].gameObject.name = "Player_" + photonViews[j].Owner.NickName;
                    photonViews[j].gameObject.GetComponent<PlayerMoveControl>().SetPlayerName();
                    // 실제 게임오브젝트를 리스트에 추가
                    playerGoList.Add(photonViews[j].gameObject);
                }
            }
        }

        SetMasterColorControl();
        // 디버그용
        //PrintPlayerList();
    }


    private void SetMasterColorControl()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetPlayerListColor();
            photonView.RPC("SetPlayerListRPC", RpcTarget.Others, playerGoList);
            photonView.RPC("SetColorRPC", RpcTarget.Others, playerIdx);
            photonView.RPC("SetPlayerColor", RpcTarget.All);
        }
    }
    private void SetPlayerListColor()
    {
        //Debug.LogError(playerGoList.Count);

        for (int i = 0; i < playerIdx.Length; i++)
        {
            if (playerIdx[i] == 0)
            {
                for (int j = 0; j < playerGoList.Count; j++)
                {
                    if (!playerIdx.Contains(playerGoList[j].GetPhotonView().ViewID))
                        playerIdx[i] = playerGoList[j].GetPhotonView().ViewID;
                }
            }
        }
    }
    [PunRPC]
    private void SetPlayerListRPC(List<GameObject> _playerGoList)
    {
        playerGoList = _playerGoList;
    }

    [PunRPC]
    private void SetPlayerColor()
    {
        for (int i = 0; i < playerGoList.Count; i++)
        {
            for (int j = 0; j < playerIdx.Length; j++)
            {
                //Debug.LogError(playerGoList[i].GetPhotonView().ViewID);
                if(playerGoList[i] != null && playerGoList[i].GetPhotonView().ViewID == playerIdx[j])
                {
                    /*                    GameObject go = PhotonNetwork.Instantiate($"Prefabs\\{tmpGo.name}", Vector3.zero, Quaternion.identity);
                                        go.transform.parent = contentGo.transform;*/
                    if (PhotonNetwork.IsMasterClient)
                    {
                        startBtn.interactable = true;
                    }
                    if (j%2 == 1)
                    {
                        teamTwoGo.Add(playerGoList[i]);
                        playerGoList[i].GetComponent<PlayerMoveControl>().SetColorBlue();
                        tmpGoList[i].SetActive(true);
                        tmpGoList[i].GetComponent<TMP_Text>().color = Color.blue;
                        tmpGoList[i].GetComponent<TMP_Text>().text =
                            playerGoList[i].GetComponent<PlayerMoveControl>().GetNickName();
                    }
                    else
                    {
                        teamOneGo.Add(playerGoList[i]);
                        playerGoList[i].GetComponent<PlayerMoveControl>().SetColorRed();
                        tmpGoList[i].SetActive(true);
                        tmpGoList[i].GetComponent<TMP_Text>().color = Color.red;
                        tmpGoList[i].GetComponent<TMP_Text>().text =
                            playerGoList[i].GetComponent<PlayerMoveControl>().GetNickName();
                    }

                }
            }
        }
    }

    [PunRPC]
    private void SetColorRPC(int[] _mColorArr)
    {
        playerIdx = _mColorArr;
    }

    [PunRPC]
    private void SetPlayerGoListRPC(int _removeIdx)
    {
        playerIdx[_removeIdx] = 0;
    }

    private void PrintPlayerList()
    {
        foreach (GameObject go in playerGoList)
        {
            if (go != null)
            {
                Debug.LogError(go.name);
            }
        }
    }

    // 플레이어가 나갈 때 호출되는 함수
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Debug.LogFormat("Player Left Room: {0}",otherPlayer.NickName);
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (GameObject go in playerGoList)
            {
                if (go != null)
                {
                    if (otherPlayer.NickName == go.GetPhotonView().Owner.NickName)
                    {
                        for (int i = 0; i < playerIdx.Length; ++i)
                        {
                            if (playerIdx[i] == go.GetPhotonView().ViewID) playerIdx[i] = 0;
                        }
                    }
                }

            }
        }
    }

    public void BeforeLeave()
    {
        for (int i = 0; i < playerIdx.Length; i++)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (playerIdx[i] == go.GetPhotonView().ViewID)
                {
                    photonView.RPC("SetColorRPC", RpcTarget.OthersBuffered, playerIdx);
                    photonView.RPC("SetPlayerGoListRPC", RpcTarget.OthersBuffered, i);
                    return;
                }
            }
            else
            {
                if (playerIdx[i] == go.GetPhotonView().ViewID)
                {
                    photonView.RPC("SetPlayerGoListRPC", RpcTarget.MasterClient, i);
                    return;
                }
            }


        }
    }
    public void LeaveRoom()
    {

        //Debug.Log("Leave Room");

        PhotonNetwork.LeaveRoom();
    }

    public void BtnStart()
    {
        startBtn.interactable = false;
        photonView.RPC("RPCCoroutineStartCnt", RpcTarget.All);
    }

    [PunRPC]
    public void RPCCoroutineStartCnt()
    {
        foreach (Transform garbage in garbageList)
        {
            garbage.gameObject.SetActive(false);
        }
        

        StartCoroutine("CountCoroutine");

    }
    private IEnumerator CountCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < playerGoList.Count; ++i)
            {
                // Red Team
                if (i % 2 == 0)
                {
                    for (int j = 0; j < 3; ++j)
                        garbageList[i + j].gameObject.SetActive(true);
                    playerGoList[i].transform.position = new Vector3(-4f + (i * 2), 0f, -76f);
                    Debug.Log("Red");
                }
                // Blue Team
                else
                {
                    for (int j = 0; j < 3; ++j)
                        garbageList[i + j].gameObject.SetActive(true);
                    playerGoList[i].transform.position = new Vector3(4f - (i * 2), 0f, 76f);
                    Debug.Log("Blue");
                }
                    
            }
            tmpStart.gameObject.SetActive(true);
            tmpStart.text = "3";
            tmpStart.color= Color.red;
            yield return new WaitForSeconds(1f);
            tmpStart.text = "2";
            tmpStart.color = Color.blue;
            yield return new WaitForSeconds(1f);
            tmpStart.text = "1";
            tmpStart.color = Color.green;
            yield return new WaitForSeconds(1f);
            tmpStart.text = "Start";
            tmpStart.color = Color.green;
            StartCoroutine("PlayTimeCoroutine");
            StartCoroutine("TempScoreCoroutine");
            //StartCoroutine("StartCoroutineTime");
            yield return new WaitForSeconds(1f);
            photonView.RPC("RPCStartCount", RpcTarget.All);
            StopCoroutine("CountCoroutine");
            tmpStart.gameObject.SetActive(false);
            yield return new WaitForSeconds(1f);
        }
    }
    [PunRPC]
    public void RPCStartCount()
    {
        for (int i = 0; i < playerGoList.Count; i++)
        {
            playerGoList[i].GetComponent<PlayerMoveControl>().SetStart();
        }
    }

    private IEnumerator PlayTimeCoroutine()
    {
        int i = 1;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            tmpCount.text = $"EndTime : {endTime.ToString()}\n Now : {i.ToString()}";
            ++i;
            if (endTime < i)
            {
                StopAllCoroutines();
                photonView.RPC("EndGameScore", RpcTarget.All);
                photonView.RPC("RPCEndCount", RpcTarget.All);
            }

        }
    }

    private IEnumerator TempScoreCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(50f);
            for (int i = 0; i < garbageList.Length; i++)
            {
                if (garbageList[i].transform.position.z > 0)
                    ++redTeamScore;
                else
                    ++blueTeamScore;
            }
            if (redTeamScore > blueTeamScore)
            {
                tmpEnd.text = $"RedWin - Score : Max({garbageList.Length}) - Cur({redTeamScore})";
                tmpEnd.color = Color.red;
            }
            else if (redTeamScore < blueTeamScore)
            {
                tmpEnd.text = $"BlueWin - Score : Max({garbageList.Length}) - Cur({blueTeamScore})";
                tmpEnd.color = Color.blue;
            }
            else
            {
                tmpEnd.text = $"Draw Red - Score : {garbageList.Length/2} \n Blue - Score : {garbageList.Length/2}";
                tmpEnd.color = Color.cyan;
            }
            redTeamScore = 0;
            blueTeamScore = 0;
            yield return new WaitForSeconds(10f);
            tmpEnd.text = $"";
            tmpEnd.color = Color.cyan;
        }
    }

    [PunRPC]
    public void EndGameScore()
    {
        for (int i = 0; i < garbageList.Length; i++)
        {
            if (garbageList[i].transform.position.z > 0)
                ++redTeamScore;
            else
                ++blueTeamScore;
        }
        if (redTeamScore > blueTeamScore)
        {
            tmpEnd.text = $"RedWin - Score : Max({garbageList.Length}) - Cur({redTeamScore})";
            tmpEnd.color = Color.red;
        }
        else if(redTeamScore < blueTeamScore)
        {
            tmpEnd.text = $"BlueWin - Score : Max({garbageList.Length}) - Cur({blueTeamScore})";
            tmpEnd.color = Color.blue;
        }
        else
        {
            tmpEnd.text = $"Draw - Rematch";
            startBtn.interactable = true;
            endTime = 120;
            tmpEnd.color = Color.cyan;
        }
        redTeamScore = 0;
        blueTeamScore = 0;
    }

    [PunRPC]
    public void RPCEndCount()
    {
        for (int i = 0; i < playerGoList.Count; i++)
        {
            playerGoList[i].GetComponent<PlayerMoveControl>().SetEnd();
        }
    }

/*    private IEnumerator StartCoroutineTime()
    {
        while (true)
        {
            for (int i = 0; i < garbageList.Length; i++)
            {
                if (garbageList[i].transform.position.z > 0)
                    ++redTeamScore;
                else
                    ++blueTeamScore;
            }
            if (redTeamScore > blueTeamScore)
            {
                tmpEnd.text = $"RedWin - Score : {redTeamScore}";
                tmpEnd.color = Color.red;
            }
            else
            {
                tmpEnd.text = $"BlueWin - Score : {blueTeamScore}";
                tmpEnd.color = Color.blue;
            }
            redTeamScore = 0;
            blueTeamScore = 0;
            yield return new WaitForSeconds(1f);

        }
    }*/

}