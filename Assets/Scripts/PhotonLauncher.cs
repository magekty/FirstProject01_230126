using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;


// Photon.PunBehaviour 사용안함
public class PhotonLauncher : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameVersion = "0.0.1";
    [SerializeField] private byte maxPlyaerPerRoom = 8;

    [SerializeField] private string nickName = string.Empty;

    [SerializeField] private Button connectButton = null;


    private void Awake()
    {
        // 마스터가 PhotonNetwork.LoadLevel()을 호출하면,
        // 모든 플레이어가 동일한 레벨을 자동으로 로드
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        connectButton.interactable = true;
    }

    // Connect Button이 눌러지면 호출
    public void Connect()
    {
        connectButton.interactable = false;
        if (string.IsNullOrEmpty(nickName))
        {
            //Debug.Log("NickName is empty");

            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom(null, maxPlyaerPerRoom);
        }
        else
        {
            //Debug.LogFormat("Connect : {0}", gameVersion);

            PhotonNetwork.GameVersion = gameVersion;
            // 포톤 클라우드에 접속을 시작하는 지점
            // 접속에 성공하면 OnConnectedToMaster 메서드 호출
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    // InputField_NickName과 연결해 닉네임을 가져옴
    public void OnValueChangedNickName(string _nickName)
    {
        nickName = _nickName;

        // 유저 이름 지정
        PhotonNetwork.NickName = nickName;
    }

    public override void OnConnectedToMaster()
    {
        //Debug.LogFormat("Connected to Master: {0}", nickName);
        
        connectButton.interactable = false;

        PhotonNetwork.JoinRandomRoom(null, maxPlyaerPerRoom);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //Debug.LogWarningFormat("Disconnected: {0}", cause);

        connectButton.interactable = true;

        // 방을 생성하면 OnJoinedRoom 호출
        //Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(
            null,
            new RoomOptions {
                MaxPlayers = maxPlyaerPerRoom });
    }

    public override void OnJoinedRoom()
    {
        //Debug.Log("Joined Room");

        // 마스터가 동시에 게임을 시작하게하는 구조가 아니기 때문에 각자 씬을 부르면 됨
        //PhotonNetwork.LoadLevel("Room");
        SceneManager.LoadScene("GameScene");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //Debug.LogErrorFormat("JoinRandomFailed({0}): {1}", returnCode, message);

        connectButton.interactable = true;

        //Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlyaerPerRoom });
    }
}