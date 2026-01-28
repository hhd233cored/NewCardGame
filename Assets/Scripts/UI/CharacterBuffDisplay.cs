using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBuffDisplay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Character owner;
    [SerializeField] private BuffView iconPrefab;
    [SerializeField] private float spacing = 0.4f; // 图标之间的间距

    private List<BuffView> spawnedIcons = new List<BuffView>();

    private void Update()
    {
        // 为了简单起见，我们可以在这里轮询，或者由 Character 类在 AddBuff/RemoveBuff 时调用刷新
        //RefreshIcons();
    }
    public void RefreshIcons()
    {
        // 1. 清理旧图标
        foreach (var icon in spawnedIcons)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        spawnedIcons.Clear();

        if (owner == null || owner.BuffList == null) return;

        // 2. 根据 BuffList 生成新图标
        for (int i = 0; i < owner.BuffList.Count; i++)
        {
            Buff buff = owner.BuffList[i];
            BuffView newIcon = Instantiate(iconPrefab, transform);

            // 3. 计算水平排列位置
            // 假设这个脚本挂在 Character 脚下，图标向右排列
            float xOffset = i * spacing;
            newIcon.transform.localPosition = new Vector3(xOffset, 0, 0);

            newIcon.SetBuff(buff);
            spawnedIcons.Add(newIcon);
        }
    }
}
