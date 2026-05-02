# Gun System — 추후 수정 / 셋업 TODO

브랜치: `feat/Gun_b-hyoung`
최종 업데이트: 2026-05-02

---

## 1. Unity Inspector 셋업 (각 커밋별)

각 기능 커밋은 코드만 들어있음. Unity 에디터에서 컴포넌트 부착/필드 할당이 필요함.

### `ba524b5` — #2 탄피 배출
- [ ] `Assets/Prefabs/Gun.prefab` 또는 BobScene의 `Gun` 인스턴스 인스펙터
  - **Shell Prefab**: `Assets/Sci-fi Pistol/Prefabs/BulletShell.prefab` 드래그
  - **Shell Eject Point** (선택): Pistol Slide 옆에 빈 GameObject 만들어서 할당. 비우면 muzzle에서 나감
  - Shell Eject Force / Up Force / Lifetime 튜닝

### `20bc814` — #4 반동 회전
- [ ] `Gun → ModelSlot`에 **Add Component → Recoil Kick**
- 기본 -12도 X축 (총구 위로 꺾임), Inspector에서 조절 가능
- Gun 루트에 붙이지 말 것 (그립 트래킹 흔들림)

### `c1bf1cd` — #5 3D 사운드
- (자동 적용) 따로 컴포넌트 추가 X
- 튜닝하려면 `Gun` 인스펙터 → Spatial Blend / Min Distance / Max Distance

### `631eb8f` — #6 트리거 당김
- [ ] `Gun → ModelSlot → Pistol → Trigger`에 **Add Component → Trigger Pull**
- 기본 -15도 X축, pullTime 0.03s, returnTime 0.08s

### `36f324e` — #8 수동 슬라이드 장전
- [ ] `Gun → ModelSlot → Pistol → Slide`에:
  - **Add Component → Box Collider** (없으면)
  - **Add Component → Manual Rack**
- [ ] `Gun` 인스펙터의 **Require Chambering** 체크
- 자유 손으로 슬라이드 잡으면 `ChamberRound()` 호출됨 (한 번 장전하면 영구)

### `4fb60e9` — #10 Photon 발사 RPC
- [ ] `Gun` GameObject에 **Add Component → Photon View**
- [ ] PhotonView 인스펙터:
  - **Ownership Transfer**: `Takeover` (RequestOwnership 작동)
  - **Observed Components**: 비워둬도 됨 (Transform 동기화는 필요 시 PhotonTransformView 추가)

### `a8884b7` — #12 데미지 서버 권한
- [ ] 각 Enemy GameObject에 **Add Component → Photon View**
- ⚠️ **Enemy가 EnemySpawner의 `Instantiate`로 생성 중이면 RPC 안 됨**
  - `PhotonNetwork.Instantiate("EnemyPrefabName", ...)`로 바꿔야 함
  - Enemy 프리팹을 `Assets/Resources/`에 넣어야 PhotonNetwork.Instantiate에서 찾음
  - 별도 작업 필요 — 미구현

---

## 2. Known Caveats

- **`.meta` 파일 부재**: 새 .cs 파일들 (`RecoilKick.cs`, `TriggerPull.cs`, `ManualRack.cs`)의 .meta는 Unity 켜야 자동 생성됨. 첫 컴파일 후 .meta 추가 커밋 필요할 수 있음
- **Gun.cs의 `audioSource.spatialBlend` 1.0**: 본인이 쏠 때 본인 머리 위치 기준 거리 계산. 너무 가까워서 갑자기 큰 소리 날 수 있음 → minDistance 1m로 막아둠. 더 줄이고 싶으면 0.3 정도
- **#10 RPC 발사**: PhotonView 안 부착돼있으면 `photonView == null`이라 RPC 송신 스킵. 오프라인 모드에서 정상 작동
- **#10 OwnershipTransfer**: `Takeover` 모드는 누구나 강탈 가능. 더 엄격하게 하려면 `Request` + 마스터 승인 로직 필요
- **#12 EnemyHealth**: Master가 죽거나 끊기면 데미지 적용 끊김. 호스트 마이그레이션 처리 X (미구현)

---

## 3. 후속 작업 아이디어 (당장 안 한 거)

### 우선순위 높음
- [ ] **햅틱 피드백** (옵션 #1) — 발사 시 컨트롤러 진동
- [ ] **머즐 라이트 플래시** (옵션 #3) — 발사 순간 짧은 점광원
- [ ] **EnemySpawner를 PhotonNetwork.Instantiate로 변경** — #12 작동 위해 필수

### 중간
- [ ] **탄약 시스템** (옵션 #7) — 매거진 N발, 0발이면 click 사운드
- [ ] **슬라이드 락** — 탄약 0발이면 슬라이드 뒤로 박혀있음 (시각)
- [ ] **사운드 변형** — GunShot.mp3를 3개로 분리해서 랜덤 재생 (반복감 줄임)

### 큰 작업
- [ ] **매거진 분리/장착** (옵션 #9) — 매거진 잡고 빼기, 새 매거진 끼우기
- [ ] **PlayerAvatar 풀바디 통합** — XR Origin에 IK로 풀바디 캐릭터 붙이기 (별도 브랜치 권장)
- [ ] **여러 총 종류 추가** — Rifle/Shotgun. ModelSlot에 다른 모델 + SlideRecoil/RecoilKick/TriggerPull 값 조정

---

## 4. 디버깅 팁

- 컴파일 에러: Console에서 빨간 에러 우선 처리. .meta 누락이면 한 번 Reimport
- VR에서 총 안 잡힘: `XR Grab Interactable`의 Layer Mask, `BoxCollider` 크기 확인
- 발사 안 됨: `Trigger Action` (InputActionReference) 비어있는지 확인. `XRI Default Input Actions/RightHand/Activate` 권장
- RPC 작동 X: PhotonView 부착됐는지, Photon 연결 상태(`PhotonNetwork.IsConnected`) 확인
- 슬라이드 위치 이상: SlideRecoil의 recoil 방향이 -Z인지. Pistol 모델마다 forward 다를 수 있음
