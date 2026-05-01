# HideInAI — 멀티플레이 테스트 가이드

## 목적

플레이어 아바타 동기화가 실제로 작동하는지 확인. 두 명 이상이 같은 방에 들어와서 서로 머리/손이 동기화되는지 검증.

---

## 방법 A — 친구 VR로 같이 들어오기 (실제 테스트)

가장 정확한 방법. 친구가 Quest 가지고 있어야 함.

### 친구 셋업 (1~2시간)

친구한테 다음 단계 따라하라고 안내:

1. **저장소 클론**
   ```bash
   git clone https://github.com/b-hyoung/HideInAI.git
   ```

2. **Unity 셋업** — `docs/setup-guide.md` 따라하기
   - Unity 2022.3 LTS 설치
   - 프로젝트 열고 Android Switch Platform
   - XR Plug-in Management → OpenXR + Oculus Touch Controller Profile
   - Photon PUN2 임포트
   - `My project/.env` 파일 생성, App ID 입력 (호스트랑 동일)

3. **씬 설정**
   - `Assets/Scenes/MapScene.unity` 열기
   - XR Origin 자식의 `Left Controller`, `Right Controller`에 **DirectXRTracker** 컴포넌트 부착 (Hand 각각 Left/Right)
   - `Locomotion > Move`에 **DirectXRMover** 부착, Forward Source = Main Camera
   - `Locomotion > Turn`에 **DirectXRTurner** 부착, Pivot Transform = Main Camera
   - 기존 Dynamic Move Provider, Continuous/Snap Turn Provider는 비활성화

4. **PlayerAvatar 프리팹 확인**
   - `Assets/Resources/PlayerAvatar.prefab` 존재하는지 확인 (없으면 자동 동기화 안 됨)

### 동시 접속 테스트

1. 호스트 (너): Quest Link 연결 → Unity Play
2. 친구: Quest Link 연결 → Unity Play
3. 둘 다 같은 방 `HideInAIRoom` 자동 입장
4. Console에 `방 입장 성공! 현재 인원: 2명` 로그
5. 헤드셋 안에서 상대방 아바타가 보여야 정상

---

## 방법 B — 혼자서 두 인스턴스로 시뮬레이션 (5분, 동기화만 확인)

친구 없이 동기화 동작 자체만 확인할 때.

### 단계

1. **Windows Standalone 빌드**
   - File > Build Settings
   - Platform이 `PC, Mac & Linux Standalone`인지 확인 (Android면 Switch Platform)
   - Build 클릭 → 폴더 선택 (예: `Builds/`)
   - .exe 생성됨

2. **빌드된 .exe 실행** (Player 1)
   - 헤드셋 없이도 Photon은 접속됨
   - 화면엔 빈 상태로 보임 (XR 입력 없으니 가만히 있음)

3. **동시에 Unity 에디터에서 Play** (Player 2)
   - Quest Link 연결한 상태로 Play
   - 헤드셋 안에서 빌드 인스턴스의 아바타가 (0,0,0)에 가만히 떠있어야 함
   - 너가 움직이면 빌드 인스턴스 화면에 너 아바타 움직이는 게 보여야 함

4. **확인 포인트**
   - Console 양쪽 다: `방 입장 성공! 현재 인원: 2명`
   - 빌드 화면에 너 아바타 보임
   - 너가 손/머리 움직이면 빌드 화면 아바타도 움직임

### 한계
- 빌드 인스턴스는 VR 입력 없어서 자기는 안 움직임
- 진짜 양쪽 동시 움직임 테스트는 안 됨 (그건 방법 A)

---

## 트러블슈팅

### "방 입장 성공! 현재 인원: 1명"만 나오고 다른 사람 안 보임
- App ID 다른지 확인 (둘 다 .env 파일의 PHOTON_APP_ID 동일해야 함)
- 같은 리전인지 확인 (PhotonServerSettings의 Dev Region)
- 방 이름이 같은지 (지금은 `HideInAIRoom` 고정)

### 상대방 아바타가 (0,0,0)에 박혀있음
- PhotonView의 Observed Components에 PlayerAvatar 들어가있는지
- Synchronization이 `Unreliable On Change` 또는 `Reliable Delta Compressed`인지

### 컨트롤러 트래킹 안 됨
- DirectXRTracker가 Left/Right Controller에 부착되었는지
- Hand 필드가 정확한지 (Left = Left, Right = Right)
- TrackedPoseDriver는 비활성화 또는 그대로 둬도 됨 (DirectXRTracker가 덮어씀)

### 이동/회전 안 됨
- DirectXRMover/Turner가 부착됐는지
- Forward Source / Pivot Transform이 Main Camera 가리키는지
- 기존 Dynamic Move Provider, Continuous Turn Provider 비활성화됐는지
