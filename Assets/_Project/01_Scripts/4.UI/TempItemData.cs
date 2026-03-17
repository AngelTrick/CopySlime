using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Temp/TempItemData")]

public class TempItemData : ScriptableObject  //임시 SO용 지우는거 생각하기 나중에 진짜 SO로 교체
{
    public Sprite icon;
    public string itemName;
    public int maxLevel;
    public int currentLevel;
    public int currentValue;
    public int nextValue;
    public int upgradeCost;

    
}
