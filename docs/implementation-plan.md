# "AI 속에 숨어라" VR 게임 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** AI NPC 속에 숨어서 다른 플레이어를 찾아 제거하는 1인칭 VR 라스트맨 스탠딩 멀티플레이 게임

**Architecture:** Photon PUN2 호스트 권위 방식. 호스트(방장)가 AI NPC 시뮬레이션과 킬 판정을 담당하고, 클라이언트는 VR 입력과 렌더링만 처리. Unity XR Interaction Toolkit으로 VR 조작 구현.

**Tech Stack:** Unity 2022.3 LTS, XR Interaction Toolkit, Photon PUN2, NavMesh, Meta Quest 2/3 스탠드얼론

**담당 배정:**
- 맵 담당: Task 1
- Unity 개발자 A (VR+캐릭터): Task 2, 4, 7
- Unity 개발자 B (네트워크+로직): Task 3, 5, 6
- AI 담당: Task 8, 9

---

## 파일 구조

```
Assets/
├── Scenes/
│   ├── LobbyScene.unity          # 로비 씬
│   └── GameScene.unity           # 인게임 씬
├── Scripts/
│   ├── Player/
│   │   ├── VRPlayerController.cs # VR 이동, 회전
│   │   ├── PlayerAttack.cs       # 기본 칼, 습득 칼 공격
│   │   └── PlayerHealth.cs       # 체력, 사망 처리
│   ├── Network/
│   │   ├── NetworkManager.cs     # Photon 연결, 방 생성/참가
│   │   ├── PlayerSpawner.cs      # 플레이어 스폰 관리
│   │   └── SyncManager.cs       # AI 위치 동기화 브로드캐스트
│   ├── Game/
│   │   ├── GameManager.cs        # 라운드 타이머, 승리 조건
│   │   ├── KillValidator.cs      # 킬 판정, 오킬 카운트 (호스트)
│   │   └── SpectatorMode.cs      # 관전 모드
│   ├── AI/
│   │   ├── AIController.cs       # FSM 상태 머신 (배회/멈춤/둘러보기/이동)
│   │   ├── AISpawner.cs          # AI NPC 스폰, 호스트 최적화
│   │   └── AIOptimizer.cs        # 거리 기반 업데이트 빈도 조절
│   ├── Weapon/
│   │   ├── WeaponBase.cs         # 무기 기본 클래스 (리치, 사용횟수)
│   │   ├── BasicKnife.cs         # 기본 칼 (1.5m, 3회)
│   │   ├── PickupKnife.cs        # 습득 칼 (2.5m, 1회)
│   │   └── WeaponSpawner.cs      # 맵에 무기 랜덤 스폰
│   └── UI/
│       ├── LobbyUI.cs            # 방 목록, 생성, 참가
│       ├── GameHUD.cs            # 오킬 카운트, 타이머, 기본칼 남은 횟수
│       └── ResultUI.cs           # 결과 화면 (승리/패배)
├── Prefabs/
│   ├── VRPlayer.prefab           # VR 플레이어 프리팹
│   ├── AINpc.prefab              # AI NPC 프리팹
│   ├── BasicKnife.prefab         # 기본 칼 프리팹
│   └── PickupKnife.prefab        # 습득 칼 프리팹
└── Art/
    ├── Characters/               # 로우폴리 캐릭터 모델
    ├── Environment/              # 마을 환경 에셋
    └── Weapons/                  # 무기 모델
```

---

## Task 1: 프로젝트 세팅 & 맵 기초 (맵 담당)

**Files:**
- Create: `Assets/Scenes/GameScene.unity`
- Create: `Assets/Scenes/LobbyScene.unity`

- [ ] **Step 1: Unity 프로젝트 생성**

Unity Hub에서 새 3D 프로젝트 생성 (Unity 2022.3 LTS)
```
프로젝트 이름: HideInAI
템플릿: 3D (URP)
```

- [ ] **Step 2: 필수 패키지 설치**

Window > Package Manager에서 설치:
```
- XR Interaction Toolkit (2.5+)
- XR Plugin Management
- Oculus XR Plugin
- TextMeshPro
```

- [ ] **Step 3: XR 빌드 설정**

Edit > Project Settings > XR Plug-in Management:
```
- Android 탭 > Oculus 체크
- Project Settings > Player > Android:
  - Minimum API Level: 29
  - Scripting Backend: IL2CPP
  - Target Architectures: ARM64
```

- [ ] **Step 4: 씬 2개 생성**

```
Assets/Scenes/LobbyScene.unity — 빈 씬, 로비용
Assets/Scenes/GameScene.unity — 빈 씬, 인게임용
```
File > Build Settings > Scenes In Build에 두 씬 추가 (LobbyScene 인덱스 0)

- [ ] **Step 5: 맵 프로토타입 시작**

GameScene에 기본 지형 제작:
```
- 80m × 80m Plane (바닥)
- ProBuilder 또는 기본 Cube로 건물 외벽 배치
- 중앙 광장 공간 확보
- 골목 2~3개 경로 잡기
- 경계: 울타리/벽으로 맵 테두리 막기
```

- [ ] **Step 6: NavMesh 베이크**

Window > AI > Navigation에서:
```
- Agent Radius: 0.5
- Agent Height: 1.8
- Step Height: 0.4
- Bake 클릭
```
AI NPC가 걸어다닐 수 있는 영역이 파란색으로 표시되는지 확인

- [ ] **Step 7: 스폰 포인트 배치**

빈 GameObject로 스폰 위치 마킹:
```
- PlayerSpawnPoint (12개) — 맵 가장자리에 분산, Tag: "PlayerSpawn"
- AISpawnPoint (30개) — 맵 전체에 균등 분포, Tag: "AISpawn"
- WeaponSpawnPoint (8개) — 맵 곳곳에 배치, Tag: "WeaponSpawn"
```

- [ ] **Step 8: 커밋**

```bash
git init
echo "Library/\nTemp/\nObj/\nBuild/\nBuilds/\nLogs/\n*.csproj\n*.sln\n.vs/\n.superpowers/" > .gitignore
git add .
git commit -m "feat: 프로젝트 초기 세팅 + 마을 맵 프로토타입"
```

---

## Task 2: VR 플레이어 이동 (Unity 개발자 A)

**Files:**
- Create: `Assets/Scripts/Player/VRPlayerController.cs`
- Create: `Assets/Prefabs/VRPlayer.prefab`

**의존:** Task 1 완료 후 시작

- [ ] **Step 1: XR Origin 설정**

GameScene에서:
```
- Hierarchy > XR > XR Origin (VR) 추가
- Main Camera가 XR Origin 하위에 있는지 확인
- Left/Right Controller 오브젝트 확인
```

- [ ] **Step 2: VRPlayerController.cs 작성**

```csharp
// Assets/Scripts/Player/VRPlayerController.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRPlayerController : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private ActionBasedContinuousMoveProvider moveProvider;

    [Header("회전")]
    [SerializeField] private ActionBasedSnapTurnProvider snapTurnProvider;
    [SerializeField] private float turnAngle = 45f;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (moveProvider != null)
            moveProvider.moveSpeed = moveSpeed;

        if (snapTurnProvider != null)
            snapTurnProvider.turnAmount = turnAngle;
    }
}
```

- [ ] **Step 3: XR Origin에 컴포넌트 붙이기**

XR Origin 오브젝트에:
```
- CharacterController 추가 (Height: 1.8, Radius: 0.3)
- Continuous Move Provider (Action-based) 추가
- Snap Turn Provider (Action-based) 추가
- VRPlayerController.cs 추가
- Inspector에서 moveProvider, snapTurnProvider 연결
```

- [ ] **Step 4: VRPlayer 프리팹 생성**

```
- XR Origin 전체를 Assets/Prefabs/VRPlayer.prefab으로 드래그
- 프리팹에 로우폴리 캐릭터 모델 자식으로 추가 (임시로 Capsule 사용)
- 본인 시점에서는 모델 안 보이게 Layer 설정 (PlayerModel 레이어, 카메라 Culling Mask에서 제외)
```

- [ ] **Step 5: Quest에서 이동 테스트**

```
Build Settings > Android > Switch Platform
Build and Run
Quest에서:
- 왼쪽 스틱: 이동 확인
- 오른쪽 스틱: 회전 확인
- 이동 속도가 자연스러운지 확인
```

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Player/VRPlayerController.cs Assets/Prefabs/VRPlayer.prefab
git commit -m "feat: VR 플레이어 이동/회전 구현"
```

---

## Task 3: Photon 네트워크 기초 (Unity 개발자 B)

**Files:**
- Create: `Assets/Scripts/Network/NetworkManager.cs`
- Create: `Assets/Scripts/UI/LobbyUI.cs`

**의존:** Task 1 완료 후 시작

- [ ] **Step 1: Photon PUN2 설치**

Asset Store 또는 Package Manager에서:
```
- PUN 2 - FREE 임포트
- Photon 대시보드(photonengine.com)에서 App ID 발급
- Window > Photon Unity Networking > PUN Wizard
- App ID 입력
```

- [ ] **Step 2: NetworkManager.cs 작성**

```csharp
// Assets/Scripts/Network/NetworkManager.cs
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] private byte maxPlayersPerRoom = 12;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 연결 완료");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장");
    }

    public void CreateRoom(string roomName)
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"방 입장: {PhotonNetwork.CurrentRoom.Name} ({PhotonNetwork.CurrentRoom.PlayerCount}명)");
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"연결 해제: {cause}");
        SceneManager.LoadScene("LobbyScene");
    }
}
```

- [ ] **Step 3: LobbyUI.cs 작성**

```csharp
// Assets/Scripts/UI/LobbyUI.cs
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Transform roomListParent;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI playerCountText;

    private Dictionary<string, RoomInfo> cachedRoomList = new();

    private void Start()
    {
        createRoomButton.onClick.AddListener(OnCreateRoom);
        startGameButton.onClick.AddListener(OnStartGame);
        startGameButton.gameObject.SetActive(false);

        NetworkManager.Instance.ConnectToServer();
        statusText.text = "서버 연결 중...";
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "로비 입장 완료";
    }

    private void OnCreateRoom()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
            roomName = $"Room_{Random.Range(1000, 9999)}";
        NetworkManager.Instance.CreateRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"방: {PhotonNetwork.CurrentRoom.Name}";
        UpdatePlayerCount();
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        playerCountText.text = $"플레이어: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListParent)
            Destroy(child.gameObject);

        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
                continue;
            }
            cachedRoomList[info.Name] = info;
        }

        foreach (var room in cachedRoomList.Values)
        {
            if (!room.IsOpen || !room.IsVisible) continue;
            GameObject item = Instantiate(roomListItemPrefab, roomListParent);
            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
            item.GetComponent<Button>().onClick.AddListener(() =>
                NetworkManager.Instance.JoinRoom(room.Name));
        }
    }

    private void OnStartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 4)
        {
            statusText.text = "최소 4명 필요합니다";
            return;
        }
        NetworkManager.Instance.StartGame();
    }
}
```

- [ ] **Step 4: LobbyScene UI 구성**

LobbyScene에서:
```
- Canvas 추가 (World Space — VR용)
- 방 이름 InputField (TMP)
- 방 만들기 Button
- 방 목록 ScrollView + Content (roomListParent)
- 방 목록 아이템 Prefab (Button + TextMeshPro)
- 시작 Button (방장만 보임)
- 상태 텍스트 (TextMeshProUGUI)
- 플레이어 수 텍스트 (TextMeshProUGUI)
- 빈 오브젝트에 NetworkManager.cs 붙이기
- LobbyUI.cs 붙이고 Inspector 연결
```

- [ ] **Step 5: 에디터에서 2명 연결 테스트**

```
- Window > Photon > ParrelSync 또는 빌드 2개로 테스트
- 한쪽에서 방 생성, 다른 쪽에서 방 참가
- 플레이어 수 업데이트 확인
- 시작 버튼 누르면 GameScene 전환 확인
```

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Network/NetworkManager.cs Assets/Scripts/UI/LobbyUI.cs
git commit -m "feat: Photon 로비 시스템 (방 생성/참가/시작)"
```

---

## Task 4: 무기 & 공격 시스템 (Unity 개발자 A)

**Files:**
- Create: `Assets/Scripts/Weapon/WeaponBase.cs`
- Create: `Assets/Scripts/Weapon/BasicKnife.cs`
- Create: `Assets/Scripts/Weapon/PickupKnife.cs`
- Create: `Assets/Scripts/Weapon/WeaponSpawner.cs`
- Create: `Assets/Scripts/Player/PlayerAttack.cs`

**의존:** Task 2 완료 후 시작

- [ ] **Step 1: WeaponBase.cs 작성**

```csharp
// Assets/Scripts/Weapon/WeaponBase.cs
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected float reach = 1.5f;
    [SerializeField] protected int maxUses = 3;
    protected int remainingUses;

    public float Reach => reach;
    public int RemainingUses => remainingUses;
    public bool IsUsable => remainingUses > 0;

    protected virtual void Awake()
    {
        remainingUses = maxUses;
    }

    public virtual bool Use()
    {
        if (!IsUsable) return false;
        remainingUses--;
        return true;
    }
}
```

- [ ] **Step 2: BasicKnife.cs 작성**

```csharp
// Assets/Scripts/Weapon/BasicKnife.cs
using UnityEngine;

public class BasicKnife : WeaponBase
{
    protected override void Awake()
    {
        reach = 1.5f;
        maxUses = 3;
        base.Awake();
    }
}
```

- [ ] **Step 3: PickupKnife.cs 작성**

```csharp
// Assets/Scripts/Weapon/PickupKnife.cs
using UnityEngine;

public class PickupKnife : WeaponBase
{
    protected override void Awake()
    {
        reach = 2.5f;
        maxUses = 1;
        base.Awake();
    }

    public override bool Use()
    {
        bool used = base.Use();
        if (used && remainingUses <= 0)
            Destroy(gameObject);
        return used;
    }
}
```

- [ ] **Step 4: PlayerAttack.cs 작성**

```csharp
// Assets/Scripts/Player/PlayerAttack.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerAttack : MonoBehaviourPun
{
    [SerializeField] private Transform attackOrigin; // 카메라(머리) 위치
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackMotionDuration = 0.7f;

    private WeaponBase currentWeapon;
    private BasicKnife basicKnife;
    private float lastAttackTime = -999f;
    private bool isAttacking;

    private void Start()
    {
        basicKnife = GetComponentInChildren<BasicKnife>();
        currentWeapon = basicKnife;
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        // VR 트리거 입력은 XR Input Action에서 처리
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TryAttack();
    }

    public void TryAttack()
    {
        if (!photonView.IsMine) return;
        if (isAttacking) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (currentWeapon == null || !currentWeapon.IsUsable) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        float reach = currentWeapon.Reach;
        if (Physics.Raycast(attackOrigin.position, attackOrigin.forward, out RaycastHit hit, reach, attackableLayer))
        {
            PhotonView targetView = hit.collider.GetComponentInParent<PhotonView>();
            if (targetView != null)
            {
                currentWeapon.Use();
                // 호스트에게 킬 판정 요청
                photonView.RPC("RPC_RequestKill", RpcTarget.MasterClient,
                    targetView.ViewID, photonView.ViewID);
            }
        }
        else
        {
            // 빗나감 — 무기 사용 횟수 소모하지 않음
        }

        Invoke(nameof(EndAttack), attackMotionDuration);
    }

    private void EndAttack()
    {
        isAttacking = false;
        // 기본 칼 소진 시 습득 칼로 전환, 둘 다 없으면 무기 없음
        if (currentWeapon != null && !currentWeapon.IsUsable)
        {
            currentWeapon = null;
        }
    }

    public void EquipPickupKnife(PickupKnife knife)
    {
        currentWeapon = knife;
        knife.transform.SetParent(transform);
    }

    public WeaponBase CurrentWeapon => currentWeapon;
    public bool IsAttacking => isAttacking;
}
```

- [ ] **Step 5: WeaponSpawner.cs 작성**

```csharp
// Assets/Scripts/Weapon/WeaponSpawner.cs
using UnityEngine;
using Photon.Pun;

public class WeaponSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject pickupKnifePrefab;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        SpawnWeapons();
    }

    private void SpawnWeapons()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("WeaponSpawn");

        foreach (GameObject point in spawnPoints)
        {
            PhotonNetwork.Instantiate(
                pickupKnifePrefab.name,
                point.transform.position,
                Quaternion.identity
            );
        }
    }
}
```

- [ ] **Step 6: 습득 칼 줍기 — PickupKnife에 트리거 콜라이더 추가**

PickupKnife 프리팹에:
```
- SphereCollider (Radius: 1.0, Is Trigger: true)
- 플레이어가 접근하면 자동 습득
```

PickupKnife.cs에 추가:
```csharp
private void OnTriggerEnter(Collider other)
{
    PlayerAttack player = other.GetComponentInParent<PlayerAttack>();
    if (player != null && player.photonView.IsMine)
    {
        player.EquipPickupKnife(this);
        // 다른 플레이어에게도 사라짐 알림
        photonView.RPC("RPC_PickedUp", RpcTarget.All);
    }
}

[PunRPC]
private void RPC_PickedUp()
{
    GetComponent<Collider>().enabled = false;
    GetComponent<MeshRenderer>().enabled = false;
}
```

- [ ] **Step 7: Quest에서 공격 테스트**

```
- 트리거 버튼에 OnAttackInput 바인딩
- Capsule 타겟 배치 후 리치 내에서 공격
- 기본 칼 3회 소진 확인
- 습득 칼 줍기 + 1회 사용 후 소멸 확인
```

- [ ] **Step 8: 커밋**

```bash
git add Assets/Scripts/Weapon/ Assets/Scripts/Player/PlayerAttack.cs
git commit -m "feat: 무기 시스템 (기본 칼 3회, 습득 칼 1회)"
```

---

## Task 5: 킬 판정 & 오킬 시스템 (Unity 개발자 B)

**Files:**
- Create: `Assets/Scripts/Game/KillValidator.cs`
- Create: `Assets/Scripts/Player/PlayerHealth.cs`
- Create: `Assets/Scripts/UI/GameHUD.cs`

**의존:** Task 3, Task 4 완료 후 시작

- [ ] **Step 1: KillValidator.cs 작성 (호스트 전용)**

```csharp
// Assets/Scripts/Game/KillValidator.cs
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class KillValidator : MonoBehaviourPun
{
    public static KillValidator Instance { get; private set; }

    private const int MAX_WRONG_KILLS = 3;
    private Dictionary<int, int> wrongKillCounts = new(); // viewID → 오킬 횟수

    private void Awake()
    {
        Instance = this;
    }

    [PunRPC]
    public void RPC_RequestKill(int targetViewID, int attackerViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView targetView = PhotonView.Find(targetViewID);
        PhotonView attackerView = PhotonView.Find(attackerViewID);

        if (targetView == null || attackerView == null) return;

        bool targetIsAI = targetView.GetComponent<AIController>() != null;
        bool targetIsPlayer = targetView.GetComponent<PlayerHealth>() != null;

        if (targetIsPlayer)
        {
            // 플레이어를 죽임 — 정상 킬
            targetView.RPC("RPC_Die", RpcTarget.All);
        }
        else if (targetIsAI)
        {
            // AI를 죽임 — 오킬
            targetView.RPC("RPC_Die", RpcTarget.All);

            if (!wrongKillCounts.ContainsKey(attackerViewID))
                wrongKillCounts[attackerViewID] = 0;

            wrongKillCounts[attackerViewID]++;
            int count = wrongKillCounts[attackerViewID];

            // 공격자에게 오킬 카운트 알림
            attackerView.RPC("RPC_WrongKillUpdate", RpcTarget.All, count);

            if (count >= MAX_WRONG_KILLS)
            {
                // 오킬 3회 → 공격자 사망
                attackerView.GetComponent<PhotonView>()
                    .RPC("RPC_Die", RpcTarget.All);
            }
        }
    }
}
```

- [ ] **Step 2: PlayerHealth.cs 작성**

```csharp
// Assets/Scripts/Player/PlayerHealth.cs
using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPun
{
    private bool isDead;
    private int wrongKillCount;

    public bool IsDead => isDead;
    public int WrongKillCount => wrongKillCount;

    [PunRPC]
    public void RPC_Die()
    {
        if (isDead) return;
        isDead = true;

        // 쓰러지는 애니메이션 (간단 버전: 그냥 눕기)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        if (photonView.IsMine)
        {
            // 3초 후 관전 모드로 전환
            Invoke(nameof(EnterSpectator), 3f);
        }

        // 3초 후 오브젝트 비활성화
        Invoke(nameof(DisableBody), 3f);
    }

    [PunRPC]
    public void RPC_WrongKillUpdate(int count)
    {
        wrongKillCount = count;
        if (photonView.IsMine)
        {
            GameHUD.Instance.UpdateWrongKillCount(count);
        }
    }

    private void EnterSpectator()
    {
        SpectatorMode.Instance.Activate();
    }

    private void DisableBody()
    {
        gameObject.SetActive(false);
    }
}
```

- [ ] **Step 3: GameHUD.cs 작성**

```csharp
// Assets/Scripts/UI/GameHUD.cs
using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI wrongKillText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI weaponText;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateWrongKillCount(int count)
    {
        wrongKillText.text = $"오킬: {count}/3";
        if (count >= 2)
            wrongKillText.color = Color.red;
        else
            wrongKillText.color = Color.white;
    }

    public void UpdateTimer(float secondsLeft)
    {
        int min = Mathf.FloorToInt(secondsLeft / 60f);
        int sec = Mathf.FloorToInt(secondsLeft % 60f);
        timerText.text = $"{min}:{sec:00}";
    }

    public void UpdateWeaponInfo(string weaponName, int remaining)
    {
        weaponText.text = $"{weaponName}: {remaining}회";
    }
}
```

- [ ] **Step 4: GameScene에 HUD 구성**

```
- World Space Canvas (플레이어 카메라 앞에 고정)
- 왼쪽 상단: 오킬 카운트 텍스트
- 상단 중앙: 라운드 타이머
- 오른쪽 하단: 무기 정보
- KillValidator 빈 오브젝트에 KillValidator.cs 붙이기
```

- [ ] **Step 5: 킬 판정 테스트**

```
에디터에서 2인 테스트:
- 플레이어가 다른 플레이어 공격 → 대상 사망, 오킬 카운트 변동 없음 확인
- 플레이어가 AI(Capsule+AIController) 공격 → 오킬 +1 확인
- 오킬 3회 → 공격자 사망 확인
```

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Game/KillValidator.cs Assets/Scripts/Player/PlayerHealth.cs Assets/Scripts/UI/GameHUD.cs
git commit -m "feat: 킬 판정 + 오킬 페널티 시스템 (호스트 권위)"
```

---

## Task 6: 게임 매니저 — 라운드 & 승리 (Unity 개발자 B)

**Files:**
- Create: `Assets/Scripts/Game/GameManager.cs`
- Create: `Assets/Scripts/Game/SpectatorMode.cs`
- Create: `Assets/Scripts/Network/PlayerSpawner.cs`
- Create: `Assets/Scripts/UI/ResultUI.cs`

**의존:** Task 5 완료 후 시작

- [ ] **Step 1: PlayerSpawner.cs 작성**

```csharp
// Assets/Scripts/Network/PlayerSpawner.cs
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject vrPlayerPrefab;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawn");
        int index = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[index].transform;

        PhotonNetwork.Instantiate(
            vrPlayerPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}
```

- [ ] **Step 2: GameManager.cs 작성**

```csharp
// Assets/Scripts/Game/GameManager.cs
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private float roundDuration = 180f; // 3분

    private double roundStartTime;
    private bool roundActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            roundStartTime = PhotonNetwork.Time;
            photonView.RPC("RPC_StartRound", RpcTarget.All, roundStartTime);
        }
    }

    [PunRPC]
    private void RPC_StartRound(double startTime)
    {
        roundStartTime = startTime;
        roundActive = true;
    }

    private void Update()
    {
        if (!roundActive) return;

        float elapsed = (float)(PhotonNetwork.Time - roundStartTime);
        float remaining = roundDuration - elapsed;

        GameHUD.Instance.UpdateTimer(remaining);

        if (remaining <= 0 && PhotonNetwork.IsMasterClient)
        {
            EndRound("타임오버");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        PlayerHealth[] allPlayers = FindObjectsOfType<PlayerHealth>();
        PlayerHealth[] alive = allPlayers.Where(p => !p.IsDead).ToArray();

        if (alive.Length <= 1)
        {
            string winner = alive.Length == 1
                ? alive[0].photonView.Owner.NickName
                : "무승부";
            EndRound(winner);
        }
    }

    private void EndRound(string result)
    {
        roundActive = false;
        photonView.RPC("RPC_EndRound", RpcTarget.All, result);
    }

    [PunRPC]
    private void RPC_EndRound(string result)
    {
        roundActive = false;
        ResultUI.Instance.Show(result);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // 호스트 전환 시 AI 시뮬레이션 이어받기
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("새 호스트로 전환됨 — AI 시뮬레이션 시작");
        }
    }
}
```

- [ ] **Step 3: SpectatorMode.cs 작성**

```csharp
// Assets/Scripts/Game/SpectatorMode.cs
using UnityEngine;
using Photon.Pun;
using System.Linq;

public class SpectatorMode : MonoBehaviour
{
    public static SpectatorMode Instance { get; private set; }

    private Transform currentTarget;
    private int targetIndex;
    private bool isActive;

    private void Awake()
    {
        Instance = this;
    }

    public void Activate()
    {
        isActive = true;
        // 본인 캐릭터 조작 비활성화
        GetComponent<VRPlayerController>().enabled = false;
        GetComponent<PlayerAttack>().enabled = false;

        FindNextTarget();
    }

    private void Update()
    {
        if (!isActive || currentTarget == null) return;

        // 타겟 따라가기 (머리 위에서 관전)
        transform.position = currentTarget.position + Vector3.up * 2f;
        transform.LookAt(currentTarget);
    }

    public void FindNextTarget()
    {
        PlayerHealth[] alive = FindObjectsOfType<PlayerHealth>()
            .Where(p => !p.IsDead && !p.photonView.IsMine)
            .ToArray();

        if (alive.Length == 0) return;

        targetIndex = (targetIndex + 1) % alive.Length;
        currentTarget = alive[targetIndex].transform;
    }
}
```

- [ ] **Step 4: ResultUI.cs 작성**

```csharp
// Assets/Scripts/UI/ResultUI.cs
using UnityEngine;
using TMPro;
using Photon.Pun;

public class ResultUI : MonoBehaviour
{
    public static ResultUI Instance { get; private set; }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private UnityEngine.UI.Button backToLobbyButton;

    private void Awake()
    {
        Instance = this;
        resultPanel.SetActive(false);
    }

    private void Start()
    {
        backToLobbyButton.onClick.AddListener(BackToLobby);
    }

    public void Show(string result)
    {
        resultPanel.SetActive(true);
        resultText.text = result == "타임오버"
            ? "타임오버! 생존자 승리!"
            : $"승자: {result}";
    }

    private void BackToLobby()
    {
        PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}
```

- [ ] **Step 5: GameScene에 매니저 오브젝트 구성**

```
- 빈 오브젝트 "GameController":
  - GameManager.cs + PhotonView
  - KillValidator.cs
  - PlayerSpawner.cs
  - WeaponSpawner.cs
- 결과 패널 Canvas (비활성 상태)
  - 결과 텍스트
  - 로비로 돌아가기 버튼
```

- [ ] **Step 6: 멀티 테스트**

```
에디터 + 빌드 (2명):
- 방 생성 → 참가 → 시작
- 두 플레이어 GameScene에 스폰 확인
- 한 명 사망 → 관전 모드 전환 확인
- 라운드 종료 → 결과 화면 확인
- 로비 복귀 확인
```

- [ ] **Step 7: 커밋**

```bash
git add Assets/Scripts/Game/ Assets/Scripts/Network/PlayerSpawner.cs Assets/Scripts/UI/ResultUI.cs
git commit -m "feat: 게임 매니저 (라운드 타이머, 승리 조건, 관전, 결과 화면)"
```

---

## Task 7: VR 손 & 상호작용 다듬기 (Unity 개발자 A)

**Files:**
- Modify: `Assets/Scripts/Player/VRPlayerController.cs`
- Modify: `Assets/Scripts/Player/PlayerAttack.cs`
- Modify: `Assets/Prefabs/VRPlayer.prefab`

**의존:** Task 4 완료 후 시작 (Task 5, 6과 병렬 가능)

- [ ] **Step 1: VR 손 모델 설정**

```
XR Origin > Camera Offset > Left/Right Controller:
- XR Controller (Action-based) 확인
- 손 모델 추가 (XR Interaction Toolkit 기본 손 또는 로우폴리 손)
- 공격 시 손 모델에 칼 쥐는 포지션 설정
```

- [ ] **Step 2: 공격 입력을 VR 트리거에 바인딩**

XR Input Actions 에셋에서:
```
- Attack 액션 추가 (Type: Button)
- 오른손 트리거 바인딩: <XRController>{RightHand}/triggerPressed
- PlayerAttack.OnAttackInput에 연결
```

- [ ] **Step 3: 공격 피드백 추가**

PlayerAttack.cs에 햅틱(진동) 추가:
```csharp
// PlayerAttack.cs에 추가
using UnityEngine.XR.Interaction.Toolkit;

[SerializeField] private XRBaseController rightHandController;

private void PlayAttackHaptic()
{
    if (rightHandController != null)
        rightHandController.SendHapticImpulse(0.5f, 0.2f);
}

// TryAttack() 내부, 공격 성공 시:
PlayAttackHaptic();
```

- [ ] **Step 4: 무기 줍기 피드백**

습득 칼에 접근하면:
```
- 칼이 살짝 빛나는 효과 (Emission 머티리얼)
- 줍는 순간 햅틱 진동
- 간단한 사운드 (AudioSource.PlayOneShot)
```

- [ ] **Step 5: Quest 테스트**

```
- 트리거로 공격 → 진동 확인
- 습득 칼 접근 → 빛남 확인
- 줍기 → 진동 + 사운드 확인
- 공격 리치가 기본 칼(1.5m) vs 습득 칼(2.5m) 다른지 확인
```

- [ ] **Step 6: 커밋**

```bash
git add Assets/Scripts/Player/ Assets/Prefabs/VRPlayer.prefab
git commit -m "feat: VR 손 상호작용, 공격 햅틱/피드백"
```

---

## Task 8: AI NPC 기본 행동 (AI 담당)

**Files:**
- Create: `Assets/Scripts/AI/AIController.cs`
- Create: `Assets/Scripts/AI/AISpawner.cs`
- Create: `Assets/Prefabs/AINpc.prefab`

**의존:** Task 1 완료 후 시작

- [ ] **Step 1: AI NPC 프리팹 생성**

```
- Capsule 오브젝트 (플레이어와 동일한 크기/색상/모델)
- NavMeshAgent 컴포넌트 추가
  - Speed: 2.0 (플레이어와 동일!)
  - Angular Speed: 120
  - Stopping Distance: 0.5
- PhotonView 컴포넌트 추가
- AIController.cs 추가
- Prefab으로 저장: Assets/Prefabs/AINpc.prefab
```

- [ ] **Step 2: AIController.cs — FSM 작성**

```csharp
// Assets/Scripts/AI/AIController.cs
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class AIController : MonoBehaviourPun
{
    private enum AIState { Wander, Idle, LookAround, MoveToNearby }

    [Header("행동 설정")]
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float idleTimeMin = 2f;
    [SerializeField] private float idleTimeMax = 5f;
    [SerializeField] private float lookAroundSpeed = 60f;
    [SerializeField] private float nearbyDetectRange = 10f;

    private NavMeshAgent agent;
    private AIState currentState = AIState.Wander;
    private float stateTimer;
    private float lookAngle;
    private float targetLookAngle;
    private bool isDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // AI 로직은 호스트만 실행
        if (!PhotonNetwork.IsMasterClient) return;
        if (isDead) return;

        switch (currentState)
        {
            case AIState.Wander:
                UpdateWander();
                break;
            case AIState.Idle:
                UpdateIdle();
                break;
            case AIState.LookAround:
                UpdateLookAround();
                break;
            case AIState.MoveToNearby:
                UpdateMoveToNearby();
                break;
        }
    }

    private void UpdateWander()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            // 다음 상태를 랜덤 선택
            float roll = Random.value;
            if (roll < 0.4f)
                SetState(AIState.Idle);
            else if (roll < 0.6f)
                SetState(AIState.LookAround);
            else if (roll < 0.8f)
                SetState(AIState.MoveToNearby);
            else
                WanderToRandomPoint();
        }
    }

    private void WanderToRandomPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
        randomDir += transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void UpdateIdle()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            float roll = Random.value;
            if (roll < 0.5f)
                WanderToRandomPoint();
            SetState(roll < 0.5f ? AIState.Wander : AIState.LookAround);
        }
    }

    private void UpdateLookAround()
    {
        float currentY = transform.eulerAngles.y;
        float newY = Mathf.MoveTowardsAngle(currentY, targetLookAngle, lookAroundSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, newY, 0f);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            SetState(AIState.Wander);
            WanderToRandomPoint();
        }
    }

    private void UpdateMoveToNearby()
    {
        if (!agent.hasPath || agent.remainingDistance < 1.5f)
        {
            SetState(AIState.Idle);
        }
    }

    private void SetState(AIState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case AIState.Idle:
                agent.ResetPath();
                stateTimer = Random.Range(idleTimeMin, idleTimeMax);
                break;

            case AIState.LookAround:
                agent.ResetPath();
                stateTimer = Random.Range(2f, 4f);
                targetLookAngle = transform.eulerAngles.y + Random.Range(-120f, 120f);
                break;

            case AIState.MoveToNearby:
                MoveToNearbyAgent();
                break;

            case AIState.Wander:
                WanderToRandomPoint();
                break;
        }
    }

    private void MoveToNearbyAgent()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, nearbyDetectRange);
        Transform closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            // AI든 플레이어든 근처로 이동 (군중 형성)
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = col.transform;
            }
        }

        if (closest != null)
        {
            agent.SetDestination(closest.position);
        }
        else
        {
            SetState(AIState.Wander);
        }
    }

    [PunRPC]
    public void RPC_Die()
    {
        isDead = true;
        agent.enabled = false;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Invoke(nameof(DisableNPC), 3f);
    }

    private void DisableNPC()
    {
        gameObject.SetActive(false);
    }
}
```

- [ ] **Step 3: AISpawner.cs 작성**

```csharp
// Assets/Scripts/AI/AISpawner.cs
using UnityEngine;
using Photon.Pun;

public class AISpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject aiNpcPrefab;
    [SerializeField] private int npcCount = 25;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        SpawnAIs();
    }

    private void SpawnAIs()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("AISpawn");

        for (int i = 0; i < npcCount && i < spawnPoints.Length; i++)
        {
            PhotonNetwork.Instantiate(
                aiNpcPrefab.name,
                spawnPoints[i].transform.position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            );
        }
    }
}
```

- [ ] **Step 4: 에디터에서 AI 행동 테스트**

```
- GameScene에 AISpawner 오브젝트 추가
- Play → AI NPC 25개 스폰 확인
- 배회/멈춤/둘러보기/근처이동 상태 전환 관찰
- 이동 속도가 플레이어와 동일한지 확인 (2.0)
- AI끼리 겹치지 않는지 확인
```

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/AI/AIController.cs Assets/Scripts/AI/AISpawner.cs Assets/Prefabs/AINpc.prefab
git commit -m "feat: AI NPC FSM 행동 (배회/멈춤/둘러보기/군중 형성)"
```

---

## Task 9: AI 최적화 — Quest 성능 확보 (AI 담당)

**Files:**
- Create: `Assets/Scripts/AI/AIOptimizer.cs`
- Modify: `Assets/Scripts/AI/AIController.cs`

**의존:** Task 8 완료 후 시작

- [ ] **Step 1: AIOptimizer.cs 작성**

```csharp
// Assets/Scripts/AI/AIOptimizer.cs
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class AIOptimizer : MonoBehaviour
{
    [SerializeField] private float fullUpdateRadius = 10f;
    [SerializeField] private float reducedUpdateInterval = 0.5f;

    private List<AIController> allAIs = new();
    private Transform localPlayer;

    public void RegisterAI(AIController ai)
    {
        allAIs.Add(ai);
    }

    public void UnregisterAI(AIController ai)
    {
        allAIs.Remove(ai);
    }

    private void Start()
    {
        // 로컬 플레이어 찾기 (스폰 후)
        Invoke(nameof(FindLocalPlayer), 1f);
    }

    private void FindLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<VRPlayerController>())
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                localPlayer = player.transform;
                break;
            }
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (localPlayer == null) return;

        foreach (var ai in allAIs)
        {
            if (ai == null) continue;

            float dist = Vector3.Distance(localPlayer.position, ai.transform.position);
            ai.SetUpdateMode(dist <= fullUpdateRadius);
        }
    }
}
```

- [ ] **Step 2: AIController.cs에 최적화 모드 추가**

AIController.cs에 추가:
```csharp
// AIController.cs에 추가할 필드와 메서드

private bool fullUpdate = true;
private float reducedTimer;
private const float REDUCED_INTERVAL = 0.5f;

public void SetUpdateMode(bool isFull)
{
    fullUpdate = isFull;
}

// Update() 메서드를 수정:
private void Update()
{
    if (!PhotonNetwork.IsMasterClient) return;
    if (isDead) return;

    if (!fullUpdate)
    {
        // 먼 AI는 0.5초마다 업데이트
        reducedTimer -= Time.deltaTime;
        if (reducedTimer > 0f) return;
        reducedTimer = REDUCED_INTERVAL;
    }

    switch (currentState)
    {
        case AIState.Wander: UpdateWander(); break;
        case AIState.Idle: UpdateIdle(); break;
        case AIState.LookAround: UpdateLookAround(); break;
        case AIState.MoveToNearby: UpdateMoveToNearby(); break;
    }
}
```

- [ ] **Step 3: AI 등록 연결**

AIController.cs의 Awake/OnDestroy에 추가:
```csharp
private void Start()
{
    var optimizer = FindObjectOfType<AIOptimizer>();
    if (optimizer != null)
        optimizer.RegisterAI(this);
}

private void OnDestroy()
{
    var optimizer = FindObjectOfType<AIOptimizer>();
    if (optimizer != null)
        optimizer.UnregisterAI(this);
}
```

- [ ] **Step 4: Quest 프로파일링 테스트**

```
Quest에서 빌드 후:
- Android Logcat으로 FPS 확인
- AI 25개 + 플레이어 2명 기준
- 목표: 72 FPS 이상 유지 (Quest 기본 주사율)
- FPS 부족 시: npcCount 줄이기 (25 → 20)
```

- [ ] **Step 5: 커밋**

```bash
git add Assets/Scripts/AI/
git commit -m "feat: AI 거리 기반 최적화 (먼 NPC 업데이트 빈도 감소)"
```

---

## Task 10: 통합 & Quest 빌드 (전원)

**Files:**
- Modify: 전체 씬/프리팹 연결

**의존:** Task 1~9 전체 완료 후

- [ ] **Step 1: GameScene 통합 조립**

```
GameScene에 다음 오브젝트 확인:
- GameController (GameManager, KillValidator, PlayerSpawner, WeaponSpawner, AISpawner, AIOptimizer)
- 모든 스크립트의 Inspector 참조 연결
- 프리팹 참조 연결 (VRPlayer, AINpc, PickupKnife)
```

- [ ] **Step 2: 씬 전환 플로우 테스트**

```
LobbyScene에서 시작:
1. 서버 연결 → 로비 입장
2. 방 생성 → 다른 클라이언트 참가
3. 시작 → GameScene 로드
4. 플레이어 + AI 스폰 확인
5. 킬 / 오킬 테스트
6. 라운드 종료 → 결과 화면
7. 로비 복귀
```

- [ ] **Step 3: 4인 플레이 테스트**

```
4명 전원 Quest로 접속:
- 방 생성/참가 원활한지
- 모든 플레이어 + AI 보이는지
- 킬 판정 정확한지
- 오킬 카운트 제대로 동작하는지
- 프레임 72 FPS 유지하는지
- 호스트 퇴장 시 게임 이어지는지
```

- [ ] **Step 4: 밸런스 조정**

테스트 후 조절할 수치들:
```
- AI 수: 25 기본, 성능에 따라 20~30 조절
- 라운드 시간: 180초 기본, 필요 시 조절
- 기본 칼 사용 횟수: 3회
- 오킬 한계: 3회
- 공격 쿨타임: 2초
- 습득 칼 스폰 수: 5~8개
```

- [ ] **Step 5: 최종 Quest 빌드**

```
Build Settings:
- Scenes: LobbyScene (0), GameScene (1)
- Platform: Android
- Build > APK 생성
- Quest에 설치: adb install -r build.apk
```

- [ ] **Step 6: 최종 커밋**

```bash
git add .
git commit -m "feat: 통합 완료 — 4인 멀티플레이 VR 게임 빌드"
```
