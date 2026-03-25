using UnityEngine;
/// <summary>
/// 게임 내 모든 사운드(BGM, SFX)를 중앙 제어하는 매니저
/// 방치형 게임의 특성 (다수의 효과음 동시 발생)을 고려, 라운드 로빈(Round Robin) 방식 오브젝트 폴링 적용
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
    [Header("BGM")]
    private AudioSource _bgmPlayer;

    [Header("SFX (Round Robin)")]
    public int sfxChannelCount = 15;    // 동시에 재생 가능한 최대 효과음 갯수
    private AudioSource[] _sfxPlayers;  // 효과음을 재생할 오디오 소스 방(배열)
    private int _sfxIndex = 0;          // 현재 차례를 가리키는 인덱스

    [Header("Volume Memory")]
    // 유저가 환경설정에서 지정한 마스터 볼륨을 기억해두는 변수
    private float _bgmVolume = 1.0f;
    private float _sfxVolume = 1.0f;

    protected override void Awake()
    {
        base.Awake();                   // 씬 전환 시 파괴되지 않도록 뼈대 유지 (Singleton 구조)

        InitSoundPlayers();

        LoadVolumeSettings();           // 켜질 때 볼륨 불러오기
    }
    /// <summary>
    /// 기기에서 볼륨 세팅 불러오기
    /// </summary>
    public void LoadVolumeSettings()
    {
        // 저장된 값이 없으면 기본값 1.0f(100%) 사용
        _bgmVolume = PlayerPrefs.GetFloat("BGM_Volume",1.0f);
        _sfxVolume = PlayerPrefs.GetFloat("SFX_Volume",1.0f);

        // 불러온 볼륨을 실제 플레이어들에게 바로 적용
        if (_bgmPlayer != null) _bgmPlayer.volume = _bgmVolume;
        foreach(var sfx in _sfxPlayers)
        {
            if (sfx != null) sfx.volume = _sfxVolume;
        }

        Debug.Log($"[SoundManager] 사운드 설정 로드 완료 (BGM : {_bgmVolume}, SFX : {_sfxVolume}");
    }
    /// <summary>
    /// 게임 시작 시 필요한 AudioSource들을 미리 메모리에 올려두는(캐싱) 초기화 로직
    /// </summary
    private void InitSoundPlayers()
    {
        //---- BGM Setting -----
        // 1. BGM 초기화
        // BGM은 게임 전체에서 1개만 있으면 되므로, SoundManager 본체에 바로 AudioSource 붙여 사용
        // GetOrAddComponent : Extension.cs 에 정의된 안전한 추가/검색 확장 매서드
        _bgmPlayer = this.gameObject.GetOrAddComponent<AudioSource>();
        _bgmPlayer.loop = true; // 배경음악 무한 루프
        
        //---- SFX Setting -----
        // 2. SFX 배열 생성(메모리 공간 할당)
        _sfxPlayers = new AudioSource[sfxChannelCount];

        // 3. Hierarchy 창이 지저분해지지 않도록 빈 오브젝트로 묶어서 관리
        GameObject sfxGo = new GameObject("SFX_Player");
        sfxGo.transform.parent = this.transform;

        // 4. SFX 오디오 소스 15개 미리 생성
        // 주의 : 여기서는 15개의 '서로 다른' 컴포넌트가 필요하므로 GetOrAddComponent를 쓰면 안되고,
        // 순정 AddComponent를 사용 하여 의도적으로 여러개 붙여주어야 합니다.
        for(int i = 0; i< sfxChannelCount; i++)
        {
            _sfxPlayers[i] = sfxGo.AddComponent<AudioSource>();
            _sfxPlayers[i].playOnAwake = false; // 시작하자마자 재생되는 것 방지
        }
    }
    // ===============================================================
    // [ 팀원들이 외부에서 호출할 실제 기능들 ]
    // ===============================================================
    /// <summary>
    /// 배경음악 재생 함수
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        _bgmPlayer.clip = clip;
        _bgmPlayer.volume = _bgmVolume;
        _bgmPlayer.Play();
    }

    /// <summary>
    /// 배경음악 정지 함수 (보스전 진입 , 사망 씬 등에서 사용
    /// </summary>
    public void StopBGM()
    {
        if (_bgmPlayer != null) _bgmPlayer.Stop();
    }

    /// <summary>
    /// 효과음 재생 함수 (라운드 로빈 폴링 적용)
    /// 수많은 타격음이 겹쳐도 메모리 낭비 없이 안전하게 재생됩니다.
    /// </summary>
    /// <param name="clip">재상할 효과음 클립</param>
    /// <param name="pitchRandom">true면 피치를 미세하게 비틀어 타격감을 올리고 청각 피로도를 낮춥니다.</param>
    public void PlaySFX(AudioClip clip, bool pitchRandom = false)
    {
        if (clip == null) return;

        // 1. 배열에서 '이번 차례'의 오디오 소스를 꺼냅니다.
        AudioSource currentPlayer = _sfxPlayers[_sfxIndex];
        // 2. 세팅 (클립 및 마스터 볼륨)
        currentPlayer.clip = clip;
        currentPlayer.volume = _sfxVolume;
        
        // 3. 피치(Pitch) 랜덤화 로직
        // 타격음 등 짧고 반복되는 소리는 피치를 살짝 비틀면 찰진 타격감을 얻을 수 있습니다.
        if (pitchRandom)
        {
            currentPlayer.pitch = Random.Range(0.9f, 1.1f);
        }
        else
        {
            currentPlayer.pitch = 1.0f; // 랜덤이 아닐 경우 재사용을 위해 원래 피치 복구
        }
        // 4. 재생
        currentPlayer.Play();
        // 5. 다음 차례 번호 계산 (Ex: 14번 다음엔 다시 9번으로 돌아 오게 % 연산자 사용)
        _sfxIndex = (_sfxIndex + 1) % sfxChannelCount; //
    }

    /// <summary>
    /// 환경설정 등에서 마스터 볼륨 조절할 때 호출 하는 함수
    /// </summary
    public void SetVolume(Define.Sound type, float volume)
    {
        if(type == Define.Sound.Bgm)
        {
            _bgmVolume = volume;

            if (_bgmPlayer != null) _bgmPlayer.volume = _bgmVolume;

            PlayerPrefs.SetFloat("BGM_Volume", _bgmVolume);
        }
        else if(type == Define.Sound.Effect)
        {
            _sfxVolume = volume;
            foreach(var sfx in _sfxPlayers)
            {
                if (sfx != null) sfx.volume = _sfxVolume;
            }

            PlayerPrefs.SetFloat("SFX_Volume", _sfxVolume);
        }
        PlayerPrefs.Save();
    }
}
