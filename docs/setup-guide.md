# HideInAI — 팀 셋업 가이드

## 프로젝트 클론

```bash
git clone https://github.com/b-hyoung/HideInAI.git
```

Unity Hub에서 `My project` 폴더를 열기 (Unity 2022.3 LTS)

> 클론하면 Photon, XR Settings, IDE 설정 등 다 들어있어서 별도 임포트 필요 없음.
> Unity가 처음 열 때 Library 폴더 자동 생성 (시간 좀 걸림).

---

## 필수 설정 (순서대로)

### 1. .env 파일 생성

`My project/.env` 파일을 직접 만들고 아래 내용 입력:

```
PHOTON_APP_ID=fa296fbc-7d01-4bf5-9914-31ab250dd48d
```

> .env 파일은 git에 안 올라감. 반드시 로컬에서 직접 생성해야 Photon 접속됨.

### 2. 플랫폼 변경 — Android (Quest 빌드용)

```
File > Build Settings > Android > Switch Platform
```

Player Settings 확인:
- Minimum API Level: **29**
- Scripting Backend: **IL2CPP**
- Target Architectures: **ARM64**

### 3. XR Plug-in Management 확인

```
Edit > Project Settings > XR Plug-in Management
```

**모니터(Standalone) 탭:**
- **OpenXR** 체크
- OpenXR 아래 **Oculus Touch Controller Profile** 체크 (Quest 컨트롤러 인식용)

**Android 탭:**
- **OpenXR** 체크

### 4. Quest Link 연결 (VR 테스트)

1. Meta Quest Link 앱 PC에 설치
2. Quest 2/3을 USB-C 케이블로 PC 연결
3. Quest에서 개발자 모드 활성화 (Meta 앱 > 기기 > 개발자 모드 ON)
4. Quest에서 Quest Link 활성화
5. Unity에서 Play 누르면 VR로 테스트 가능

---

## 현재 진행 상황

### 완료된 것
- [x] Unity VR 프로젝트 생성 (XR Interaction Toolkit + OpenXR)
- [x] Photon PUN2 네트워크 연결 (서버 접속 → 로비 → 방 입장)
- [x] VR 컨트롤러 트래킹 (DirectXRTracker — InputDevices 직접 읽음)
- [x] VR 이동/회전 (DirectXRMover/Turner — XRI Action 시스템 우회)
- [x] 총 발사 + 레이저 포인터 (VRGun.cs)
- [x] 에너미 (EnemyHealth.cs / EnemySpawner.cs — 사람 모양)
- [x] **멀티플레이 아바타 동기화** (PlayerAvatar + CharacterModel + Photon)
- [x] 테스트 맵 자동 생성 도구 (Tools > Generate Test Map)
- [x] Photon App ID를 .env로 관리
- [x] GitHub Collaborator 초대 완료

### 아직 안 된 것
- [ ] 맵 제작 (80x80m 야외 마을)
- [ ] AI NPC FSM 행동 (배회/멈춤/둘러보기/군중 형성)
- [ ] 킬 판정 & 오킬 시스템 (호스트 권위 방식)
- [ ] 무기 시스템 (기본 칼 3회, 습득 칼 1회)
- [ ] 게임 매니저 (라운드 타이머, 승리 조건)
- [ ] 관전 모드
- [ ] 결과 화면 (승리/패배)
- [ ] AI 최적화 (거리 기반 업데이트)
- [ ] 실제 캐릭터 모델 + IK 셋업 (Animation Rigging)
- [ ] Quest APK 빌드

---

## 프로젝트 구조

```
My project/
├── Assets/
│   ├── Scripts/
│   │   ├── NetworkManager.cs       # Photon 서버 연결 + 아바타 스폰
│   │   ├── VRGun.cs                # VR 총 발사 + 레이저 포인터
│   │   ├── EnemyHealth.cs          # 에너미 체력/피격/사망
│   │   ├── EnemySpawner.cs         # 사람 모양 에너미 자동 스폰
│   │   ├── CharacterModel.cs       # 캐릭터 비주얼 (placeholder + 슬롯)
│   │   ├── PlayerAvatar.cs         # Photon 동기화 + XR 위치 반영
│   │   ├── DirectXRTracker.cs      # 컨트롤러 트래킹 (Action 우회)
│   │   ├── DirectXRMover.cs        # 스틱 이동 (Action 우회)
│   │   ├── DirectXRTurner.cs       # 스틱 회전 (Action 우회)
│   │   ├── TestMapMarker.cs        # 스폰 포인트 시각화
│   │   ├── TrackingDebugger.cs     # 트래킹 진단용 (재사용)
│   │   ├── XRDeviceDebugger.cs     # XR 디바이스 진단용 (재사용)
│   │   └── Editor/
│   │       ├── TestMapGenerator.cs # Tools > Generate Test Map
│   │       └── VRPlayerSetup.cs    # Tools > Setup VR Player Visuals
│   ├── Scenes/
│   │   ├── MapScene.unity          # 메인 작업 씬
│   │   └── SampleScene.unity
│   ├── Resources/
│   │   └── PlayerAvatar.prefab     # 멀티 아바타 (PhotonNetwork.Instantiate용)
│   ├── Photon/                     # PUN2 패키지 (git에 포함)
│   └── XR/                         # XR 설정 (git에 포함)
├── .env                            # Photon App ID (git 제외)
├── ProjectSettings/
└── Packages/
```

---

## 담당 배정

| 역할 | 담당 업무 |
|------|----------|
| 맵 담당 | 마을 맵 제작, NavMesh, 스폰 포인트 |
| Unity 개발자 A (VR+캐릭터) | VR 이동, 무기 시스템, 손 상호작용, 캐릭터 모델 통합 + IK |
| Unity 개발자 B (네트워크+로직) | Photon 동기화, 킬 판정, 게임 매니저 |
| AI 담당 | AI NPC FSM 행동, 최적화 |

---

## 주요 도구 / 스크립트 사용법

### Tools 메뉴 (상단 메뉴바)
- `Tools > Generate Test Map` — 바닥/벽/장애물/스폰포인트 자동 생성
- `Tools > Clear Test Map` — 위에서 생성한 거 삭제
- `Tools > Setup VR Player Visuals` — 양손 + 총 비주얼 자동 부착
- `Tools > Clear VR Player Visuals` — 위에서 부착한 거 제거

### 새 씬 만들 때 체크리스트
1. XR Origin (XR Rig) 추가
2. NetworkManager GameObject 추가 (스크립트 부착)
3. Left/Right Controller에 **DirectXRTracker** 부착 (Hand 각각 Left/Right)
4. Locomotion > Move에 **DirectXRMover** 부착 (Forward Source = Main Camera)
5. Locomotion > Turn에 **DirectXRTurner** 부착 (Pivot Transform = Main Camera)
6. 기존 Dynamic Move Provider, Continuous/Snap Turn Provider는 비활성화
7. `Tools > Setup VR Player Visuals` 실행

---

## 멀티플레이 테스트

별도 가이드 참고: `docs/test.md`

---

## 주의사항

- **`.env` 파일은 git에 안 올라감** — 클론 후 직접 생성 필수
- **씬 작업 시 충돌 주의** — 같은 씬 동시 수정 피하기 (브랜치 따로 작업 권장)
- **Library 폴더는 git에 안 올라감** — Unity가 자동 생성 (수GB, 첫 열 때 시간 걸림)
- **XRI Default Input Actions의 Position 바인딩이 Quest+OpenXR에서 안 잡히는 이슈 있음** → DirectXRTracker로 우회 중. 새 컨트롤러 셋업 시 이거 부착 필수.
