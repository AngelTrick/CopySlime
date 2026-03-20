using System;
using System.IO;
using UnityEngine;

/* [JSON 데이터 클래스 정의]
 * 이 클래스 메모장(JSON) 에 썼다 지웠다 할 "내용물" 자체
 * [Serializable] < 유니티가 JSON 텍스트 변환 가능 
 */
[Serializable]
public class PlayerSaveData
{
    public int gold = 0;            // 현재 골드
    public int copyFragments = 0;   // 카피 조각 
    public int currentStage = 1;    // 현재 스테이지

    public int attackLevel = 1;     // 공격력 업그레이드 레벨 (기본 1렙)

    // 나중에 스킬(해금) 단계 에서 목록 추가 (확장성)
    // public List<int> unlockSkillDs = new List<int>(); << Ex
}


/*
 *  역할 : 게임 내 모든 변동 데이터 (재화, 스테이지 진행도 등)를 중앙에서 관리하여 저장/로드 하는 매니저
 */
public class DataManager : Singleton<DataManager>
{
    // [1. 핵심 데이터]
    // 저장할 데이터를 모아 둘 PlayerSavaData 객체
    private PlayerSaveData _saveData = new PlayerSaveData();
    
    // 외부에서 접근을 할 수 있도록 연결
    public int Gold { get { return _saveData.gold; } }
    public int CopyFragments { get { return _saveData.copyFragments; } }
    public int CurrentStage { get { return _saveData.currentStage; } }

    public int AttackLevel { get { return _saveData.attackLevel; } }

    // [ 2. 데이터 변경 알림 방송국]
    // 골드 나 조각이 바뀔 때마다 UI들에게 Ex) "화면 숫자 바꿔!" 라고 알리는 곳

    public event Action OnDataChanged;

    protected override void Awake()
    {
        base.Awake();
        // 초기화 과정은 GameManager 에서 통제 함으로 , 가볍게 가져간다.
    }

    public void LoadGameData()
    {
        Debug.Log("[DataManager] 유저 세이브 데이터를 불러옵니다.");

        // 폰에서 지우지 앱을 지우지 않는 한 안전하게 보관되는 경로
        string path = Application.persistentDataPath + "/SaveData.json";

        if (File.Exists(path))
        {
            // 1. 파일이 있으면 텍스트를 불러 온다.
            string fileText = File.ReadAllText(path);
            string jsonText = "";

            //[핵심 보안] 기기에 '암호화 버전을 한번 이라도 쓴 적 있는지' 낙인 확인
            bool _isMigrated = PlayerPrefs.GetInt("IsSaveMigrated", 0) == 1;
            bool _needForceSave = false;

            try
            {
                //2. 먼저 Bsse64 복호화(디코딩)를 시도해 봅니다. (새로운 암호화 세이브용)
                byte[] bytes = System.Convert.FromBase64String(fileText);
                jsonText = System.Text.Encoding.UTF8.GetString(bytes);

                if (!_isMigrated)
                {
                    PlayerPrefs.SetInt("IsSaveMigrated", 1);
                    PlayerPrefs.Save();
                }
            }
            catch (System.FormatException)
            {
                // 기존의 암호화 안 된 순정 JSON 파일 하위 호환용
                if (!_isMigrated)
                {
                    // 낙인이 없는 진짜 기존 유저 (1회 한정 구제)
                    Debug.LogWarning("[DataManager] 구버전(암호화 안 됨) 세이브 파일이 감지되어 정상 로드합니다.");
                    jsonText = fileText;

                    // 앞으로 다시는 평문을 허용하지 않도록 낙인을 찍음
                    PlayerPrefs.SetInt("IsSaveMigrated", 1);
                    PlayerPrefs.Save();
                    _needForceSave=true; // 로드 완료 직후 즉시 안호화 해서 덮어씌울 예정
                }
                else
                {
                    //이미 낙인 찍힌 유저인데 암호화 안 된 평문이 들어 왔다 = 100% 해커
                    Debug.Log("[보안 경고] 불법적인 세이브 파일 변호 (Downgrade Attack) 가 감지되었습니다! 데이터를 초기화 합니다.");
                    ResetData();
                    return; // 더 이상 데이터 로드를 진행하지 않고 함수를 강제 종료
                }
            }
            // 2.읽어온 텍스트를 PlayerSaveData 객체로 조립
            _saveData = JsonUtility.FromJson<PlayerSaveData>(jsonText);

            // 1회 한정 구버전 유저였다면 , 객체 조립 직후 바로 암호화 된 파일로 덮어씌움 (허점 완전 차단)
            if (_needForceSave)
            {
                SaveGameData();
                Debug.Log("[DataManager] 구버전 세이브 데이터를 암호화 파일로 강제 갱신 완료! ");
            }
            Debug.Log($"[DataManager] JSON 로드 성공! 결로 : {path}");
        }
        else
        {
            // 만약 없다면 새로 만든것으로 관주하여 새로운 데이터 생성
            _saveData = new PlayerSaveData();
            Debug.Log("[DataManager] 세이브 파일 없습니다. 새로이 생성합니다");
        }

        // 로드 완료 후 UI 갱신을 위해 한번 더 방송 알리기
        OnDataChanged?.Invoke();
    }

    public void SaveGameData()
    {
        Debug.Log("{DataManager] 유저 세이브 데이터를 저장합니다.");

        // 1. _saveData 객체를 JSON 형태의 문자열(String)로 변환 (사람이 읽을 필요가 없으니 줄바꿈 false)
        string jsonText = JsonUtility.ToJson(_saveData,false);

        // 2. JSON 문자열을 Base64 외계어로 암호화 (인코딩)
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonText);
        string encryptedText = System.Convert.ToBase64String(bytes);

        // 3. 기기의 안전한 경로에 암호화 된 텍스트 덮어쓰기
        string path = Application.persistentDataPath + "/SaveData.json";
        File.WriteAllText(path, encryptedText);

        Debug.Log("[DataManager] 세이브 암호화 저장 완료");
    }

    //[ 3. 재화 획득 및 소모 로직]
    public void AddGold(int amount)
    {
        _saveData.gold += amount;
        OnDataChanged?.Invoke(); // 돈의 증가로 UI 에게 알림
    }
    
    // 소모 로직에서 bool 값으로 반환 하여 UI에서 성공 여부 체크 쉽도록 위함
    public bool SpendGold(int amount)
    {
        if(_saveData.gold >= amount)
        {
            _saveData.gold -= amount;
            OnDataChanged?.Invoke();
            return true; // 소모 성공 (스텟 업그레이드 성공) 
        }
        return false; // 소모 실패 (골드 부족)
    }

    public void Addfragments(int amount)
    {
        _saveData.copyFragments += amount;
        OnDataChanged?.Invoke();
    }

    public void StageCleared()
    {
        _saveData.currentStage++;
        OnDataChanged?.Invoke();

        // 안전 저장을 위해
        SaveGameData();
    }

    public void UpgradeAttackLevel()
    {
        _saveData.attackLevel++;

        SaveGameData();

        OnDataChanged?.Invoke();
    }

    // JSON 데이터 초기화 함수
    public void ResetData()
    {
        // 1. 메모리상 데이터 완전 새 깡통으로 덮어 씌웁니다.
        _saveData = new PlayerSaveData();
        // 2. 0원이 된 상태를 JSON 파일에 강제로 덮어씁니다.
        SaveGameData();
        // 3. 화면(UI)에 즉시 0원, 1스테이지로 반영되도록 방송
        OnDataChanged?.Invoke();

        Debug.LogWarning("{DataManager] 세이브 데이터가 완벽하게 초기화 되었습니다.");

    }

    //=============================================================
    // (아래) 방치형 게임이 있어야 할 확장 구역(스켈레톤 으로 작성 후 채워 나갈 예정)
    //=============================================================

    // [ 추가 될 기능 1 : 정적 데이터 (SO / CSV) 로드]
    //  '몬스터 능력치 ' 나 ' 스킬 뎀지 계수 ' 같은 데이터를 게임 시작시 로드 합니다.
    public void LoadStaticDataTables()
    {
        /* JSON 형태의 기획 데이터 Resource 폴더에서 불러 오는 스켈
         * TextAsset monsterDataJson = Rewsource.Load<TextAsset> ("Data/MonsterTable");
         * if(monsterTable != null){
         *      //JsonUtility 이용해 딕셔너리 OR 리스트 파싱 하는 로직
         * }
         * 
         */
    }

    // [ 추가 될 기능 2 : 스킬 및 스탯 레벨 정보 저장]
    // 지금은 골드 만 저장 , 나중에 유저가 찍은 스탯 레벨 저장 해야함
    /*
     public Dictionary<int , int> SkillLevelData = new Dictionary<int , int>();
     public int AttackStatLevel {get ; private set;}
    */
    
    // [ 추가 될 기능 3 : 오프라인 보상 계산을 위한 로그아웃 시간 저장
    public void SaveLogoutTime()
    {
        // 기기의 현재 시간을 문자열로 변환하여 로컬(PlayerPrefs)에 저장
        PlayerPrefs.SetString("LastLogOutTIme", DateTime.Now.ToString());
        PlayerPrefs.Save();
        Debug.Log($"[DataManager] 로그아웃 시간 저장 완료: {DateTime.Now}");
    }

    // [ 추가 될 기능 4 : 보안 과 클라우드 저장]
    // PlayerPrefs 는 보안 적으로 취약 합니다. 암호화 필수 적으로 해야 할 듯
    private void SaveToCloud()
    {
        // TODO : JSON 형태로 데이터를 직렬화 (Serialize) 한 뒤 암호화
    }
}
