using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStats : MonoBehaviour
{

    [Header("스탯 텍스트")]
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI critDamageText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI luckText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;


    private PlayerController _player; // 💥 대신 이렇게 숨겨서 선언합니다.

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerSpawned += (p) => _player = p;

            if (GameManager.Instance.CurrentPlayer != null)
                _player = GameManager.Instance.CurrentPlayer;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerSpawned -= (p) => _player = p;
    }

    private void Update() // 나중에 이벤트 기반으로 변경되게
    {
        if (_player == null) return; // 플레이어가 없으면 에러 안 나게 방어

        attackPowerText.text = $"attackPower: {_player.attackPower}";
        critDamageText.text = $"critDamage: {_player.critDamage}%";
        critRateText.text = $"critRate: {_player.critRate}%";
        luckText.text = $"luck: {_player.luck}%";
        attackSpeedText.text = $"attackSpeed: {_player.attackSpeed}%";
    }
}
