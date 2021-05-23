# AppleGame

#### 영상

[![시연영상](./versions/thumnail_ver1.1.jpg)](https://www.youtube.com/watch?v=0GMh36advCI)

#### 사과게임 설계문서

< 기능 명세 >

1. Main 씬이 로드되면, 15 x 8 영역에 랜덤한 숫자의 사과를 생성한다.

   - GameManager의 Awake 함수에서 prefab을 복사하여 생성
   - Apple prefab의 AppleMeta 스크립트에 number 변수를 랜덤으로 부여
   - AppleMeta 스크립트를 applesList에 저장하여 관리

2. 드래그하면 드래그박스가 나타나고 구역 내부의 사과들이 선택됨.

   - 캔버스 자식오브젝트인 selectBox 오브젝트로 드래그영역을 image로 렌더링
   - collideBox rect Transform에 선택영역의 world position 저장
   - OverlapBoxAll 함수로 선택영역 사과들의 select flag 켬
   - 마우스클릭을 떼면, applesList를 순회하며 select flag가 켜진 사과들을 selectList에 저장하여 관리

3. 선택된 사과들의 값의 합이 10이 되면, 제거

   - selectList를 순회하며 Number의 숫자를 받아와 sum에 더해준다.
   - 순회가 끝나고 sum이 10이면, correctAnswer 함수 호출
   - correctAnswer 호출 후, selectList 초기화

4. 제한시간 표시바와 Game Over

   - 유니티 Slider 오브젝트를 이용
   - fSliderBarTime 값에서 Time.deltaTime을 빼 제한시간 구현
   - fSliderBarTime 값이 0이 되면 GameOver

*ver1.1*

5. 사과 제거 효과

   - correctAnswer에서 AppleMeta의 isAnimated flag를 켜면, update함수에서 효과실행
   - 사과의 위치를 다른 사과들보다 앞에 그려지도록 카메라쪽으로 1만큼 이동
   - RigidBody2D의 bodyType을 중력효과를 받는 Dynamic으로 바꿈
   - throwForces 배열의 벡터값 중 랜덤한 값으로 힘을 줘 던져지는 효과를 줌
   - Collector Object와 충돌하면 isOn이 false가 되며 disable됨

*ver1.1*

6. 점수 추가 UI

   - AddScoreText prefab을 Instantiate로 SelectBox의 중앙에 생성
   - AddScore 스크립트로 moveSpeed와 DestroyTime값 만큼 위로 이동시키고 시간 후 없앰

< 추가 기능 >

1. 플레이어에게 10을 만들 수 있는 사과들의 위치를 알려주는 힌트 기능

   -

2. 플레이어가 10을 만들 수 없는 상황을 판단하고, 남은 숫자들로 shuffle을 하거나 불가능한 숫자들의 조합이라면 새로운 숫자들을 생성하는 기능

   -

*ver1.1*

3. 플레이어가 제한 시간 안에 연속적으로 10을 만들었을 때 콤보로 판단하고 점수를 올리는 시스템

   - calculateAnswer함수에서 comboDelta 함수에 time을 저장
   - comboDelta이 time과의 차가 comboTime보다 작고, 초기값이 아니면 combo += 1
   - combo 값을 animateCorrect로 넘겨주고, score에도 추가

*ver1.1*

4. 3개 이상의 사과들을 묶어서 터뜨린 경우 점수 +10

   - calculateAnswer함수에서 cnt변수에 selectedApple의 개수를 저장
   - cnt가 3이상이면 점수를 10점 더 추가
   - animateCorrect에 isMany flag를 true로 넘겨 UI로 표시

5. 업적 시스템

   -

6. 플레이를 할때마다 주어진 하트를 사용하고, 하트는 일정 시간이 지나면 채워지는 시스템

   -

7. 사과가 없어진 후 위에 사과들이 내려오는 중력 모드 추가

   - 

---

by. JeonBest
