using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Temp/TTItembase")]
public class TTItembase : ScriptableObject
{
    [SerializeField] private List<TempItemData> items;
   
    public List<TempItemData> GetAll() => items;
}
