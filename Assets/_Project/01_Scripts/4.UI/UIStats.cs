using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStats : MonoBehaviour
{
    [Header("플레이어 참조")]
    [SerializeField] private PlayerController player;

    [Header("스탯 텍스트")]
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI critDamageText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI luckText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;

    private void Update() // 나중에 이벤트 기반으로 변경되게
    {
        attackPowerText.text = $"attackPower: {player.attackPower}";
        critDamageText.text = $"critDamage: {player.critDamage}%";
        critRateText.text = $"critRate: {player.critRate}%";
        luckText.text = $"luck: {player.luck}%";
        attackSpeedText.text = $"attackSpeed: {player.attackSpeed}%";
    }
}
