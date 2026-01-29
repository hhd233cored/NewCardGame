using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Data/Buff")]//仅用于存储显示信息
public class BuffData : ScriptableObject
{
    [field: SerializeField] public string Title { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public BuffType Type { get; private set; }
    public void InInitialize(BuffData data)
    {
        Title = data.Title;
        Description = data.Description;
        Image = data.Image;
        Type = data.Type;
    }
}
