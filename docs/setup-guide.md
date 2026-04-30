# HideInAI — 팀 셋업 가이드

## 프로젝트 클론

```bash
git clone https://github.com/b-hyoung/HideInAI.git
```

Unity Hub에서 `My project` 폴더를 열기 (Unity 2022.3 LTS)

---

## 필수 설정 (순서대로)

### 1. 플랫폼 변경 — Android

```
File > Build Settings > Android > Switch Platform
```

Player Settings 확인:
- Minimum API Level: **29**
- Scripting Backend: **IL2CPP**
- Target Architectures: **ARM64**

### 2. XR Plug-in Management 설정

```
Edit > Project Settings > XR Plug-in Management
```

**모니터(Standalone) 탭:**
- **OpenXR** 체크
- OpenXR 아래 **Oculus Touch Controller Profile** 체크 (Quest 컨트롤러 인식용)

**Android 탭:**
- **OpenXR** 체크

### 3. Photon PUN2 임포트

Asset Store 또는 Package Manager에서:
```
PUN 2 - FREE 검색 > 임포트
```

임포트 후 PUN Wizard 창이 뜨면 **닫기** (App ID는 .env로 관리)

### 4. .env 파일 생성

`My project/.env` 파일을 직접 만들고 아래 내용 입력:

```
PHOTON_APP_ID=fa296fbc-7d01-4bf5-9914-31ab250dd48d
```

> .env 파일은 git에 올라가지 않음. 반드시 로컬에서 직접 생성해야 함.

### 5. Quest Link 연결 (VR 테스트)

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
- [x] VR 컨트롤러 이동/회전 (XR Origin 기본 세팅)
- [x] 총 발사 스크립트 (VRGun.cs — 오른손 트리거, 총소리, 총알 궤적)
- [x] 에너미 스크립트 (EnemyHealth.cs — 체력, 피격, 사망)
- [x] 에너미 스포너 (EnemySpawner.cs — 자동 Capsule 에너미 생성)
- [x] Photon App ID를 .env로 관리
- [x] GitHub Collaborator 초대 완료

### 아직 안 된 것
- [ ] 맵 제작 (80x80m 야외 마을)
- [ ] AI NPC FSM 행동 (배회/멈춤/둘러보기/군중 형성)
- [ ] 킬 판정 & 오킬 시스템 (호스트 권위 방식)
- [ ] 플레이어 동기화 (Photon 멀티플레이 아바타)
- [ ] 무기 시스템 (기본 칼 3회, 습득 칼 1회)
- [ ] 게임 매니저 (라운드 타이머, 승리 조건)
- [ ] 관전 모드
- [ ] 결과 화면 (승리/패배)
- [ ] AI 최적화 (거리 기반 업데이트)
- [ ] Quest APK 빌드

---

## 프로젝트 구조

```
My project/
├── Assets/
│   ├── Scripts/
│   │   ├── NetworkManager.cs   # Photon 서버 연결, .env에서 App ID 로드
│   │   ├── VRGun.cs            # VR 총 발사 (오른손 트리거)
│   │   ├── EnemyHealth.cs      # 에너미 체력/피격/사망
│   │   └── EnemySpawner.cs     # 에너미 자동 스폰
│   ├── Scenes/
│   │   ├── SampleScene.unity   # 메인 씬
│   │   └── BasicScene.unity
│   ├── Photon/                 # (각자 임포트, git 제외)
│   └── XR/                     # (로컬 설정, git 제외)
├── .env                        # Photon App ID (git 제외)
├── ProjectSettings/
└── Packages/
```

---

## 담당 배정

| 역할 | 담당 업무 |
|------|----------|
| 맵 담당 | 마을 맵 제작, NavMesh, 스폰 포인트 |
| Unity 개발자 A (VR+캐릭터) | VR 이동, 무기 시스템, 손 상호작용 |
| Unity 개발자 B (네트워크+로직) | Photon 동기화, 킬 판정, 게임 매니저 |
| AI 담당 | AI NPC FSM 행동, 최적화 |

---

## 주의사항

- **Photon, XR 폴더는 git에 올리지 않음** — 각자 로컬에서 임포트/설정
- **.env 파일은 git에 올라가지 않음** — 클론 후 직접 생성
- **PhotonServerSettings.asset도 git 제외** — .env에서 App ID 자동 로드
- 씬 작업 시 **충돌 주의** — 같은 씬 동시 수정 피하기
