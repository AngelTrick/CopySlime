using System;
using UnityEngine;

/* 
 * [인증 매니저]
 * 역할 : 게임 최초 접속 시 게스트 ID와 임시 닉네임 발급하고 기기에 저장합니다.
 *  BootScene이나 TitleScene에서 최초 호출 될 예정
 */

public class AuthManager : Singleton<AuthManager>
{
    // 외부에서 읽기만 가능 하고, 발급은 내부에서만 하도록 프로퍼티 선언
    public string CurrentGuestID { get; private set; }
    public string CurrentNickname { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }
    /// <summary>
    /// 게임 부팅 / 타이틀 씬에서 호출 될 로그인 검사 로직
    /// </summary>
    public void CheckLoginSequence()
    {
        Debug.Log("[AuthManager] 게스트 로그인 시퀀스를 확인합니다....");

        // 1.기기에 저장된 게스트ID가 있는지 확인
        string saveID = PlayerPrefs.GetString("Guest_ID", "");

        if (string.IsNullOrEmpty(saveID))
        {
            // 2. 저장된 ID가 없다면 = 신규 ID
            Debug.Log("[AuthManager] 신규 유저 접속! 새 게스트 계정을 발급합니다.");
            // 신규 계정 발급 함수 호출
            GenerateNewGuest();
        }
        else
        {
            // 3. 기존 유저라면 저장된 ID와 닉네임을 불러오고 로그인 성공 처리
            CurrentGuestID = saveID;

            // "Guest_Nickname"을 불러오되, 혹시 몰라 데이터가 날아갔을 경우를 대비해 "Unkown_Guest"를 기본값으로 둔다.
            CurrentNickname = PlayerPrefs.GetString("Guest_Nickname", "Unknown_Guest");

            Debug.Log($"[AuthManager] 기존 게스트 로그인 완료 : {CurrentNickname} ({CurrentGuestID} ");
        }

    }

    /// <summary>
    /// 신규 유저용 게스트 ID 및 닉네임 발급 로직
    /// </summary>
    private void GenerateNewGuest()
    {
        // 4. 고유  식별(UUID) 생성
        // System.Guid를 사용하면 전 세계에서 유일한 무작위 문자열이 생성
        CurrentGuestID = Guid.NewGuid().ToString();

        // 5. 임시 닉네임 생성 (예 : Guest_1234);
        // 1000~9999 사이의 랜덤 숫자를 부여합니다.
        int randomNum = UnityEngine.Random.Range(1000, 9999);
        CurrentNickname = "Guest_" + randomNum.ToString();

        // 6. 발급받은 ID와 닉네임을 기기에 영구 저장
        // 닉네임 / 디바이스 식별자 정도는 PlayerPrefs 에 저장
        // 나중에 보안 적으로 제한 한다면 추가 로직 생성 여부 있음
        PlayerPrefs.SetString("Guest_ID", CurrentGuestID);
        PlayerPrefs.SetString("Guest_Nickname", CurrentNickname);

        // 7. 디스크에 즉시 기록 (앱이 비정상 종료되어도 날아가지 않도록 강제 세이브!
        PlayerPrefs.Save();

        Debug.Log($"[AuthManager] 신규 게스트 계정 발급 및 저장 완료 : {CurrentNickname} ({CurrentGuestID} ");
    }
}
