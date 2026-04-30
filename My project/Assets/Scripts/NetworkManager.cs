using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // .env에서 Photon App ID 로드
        LoadEnv();

        Debug.Log("서버 접속 중...");
        PhotonNetwork.ConnectUsingSettings();
    }

    private void LoadEnv()
    {
        // .env 파일 경로 (프로젝트 루트)
        string envPath = Path.Combine(Application.dataPath, "..", ".env");

        if (!File.Exists(envPath))
        {
            Debug.LogWarning(".env 파일이 없습니다. PhotonServerSettings의 App ID를 사용합니다.");
            return;
        }

        string[] lines = File.ReadAllLines(envPath);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            int eqIndex = line.IndexOf('=');
            if (eqIndex < 0) continue;

            string key = line.Substring(0, eqIndex).Trim();
            string value = line.Substring(eqIndex + 1).Trim();

            if (key == "PHOTON_APP_ID" && value != "여기에_본인_앱아이디_넣기")
            {
                PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = value;
                Debug.Log(".env에서 Photon App ID 로드 완료");
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료! 로비 입장 중...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 완료! 랜덤 방 입장 시도...");
        PhotonNetwork.JoinOrCreateRoom("HideInAIRoom", new RoomOptions { MaxPlayers = 12 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"방 입장 성공! 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}명");
    }
}
