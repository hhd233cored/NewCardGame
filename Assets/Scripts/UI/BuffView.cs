using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuffView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text stackText;
    private Buff _buff;

    public void SetBuff(Buff buff)
    {
        _buff = buff;
        if (iconRenderer != null && buff.data != null)
        {
            iconRenderer.sprite = buff.data.Image; // 使用 BuffData 里的图标
        }

        if (stackText != null)
        {
            stackText.text = buff.stacks > 1 ? buff.stacks.ToString() : ""; // 显示层数
        }
    }

    private void OnMouseEnter()
    {
        if (_buff != null && _buff.data != null)
        {
            // 调用全局的 Tooltip 管理器显示文本
            // 获取数据来源：BuffData 的 Title 和 Description
            string content = $"{_buff.data.Title}\n{_buff.data.Description}";
            TooltipSystem.Instance.Show(content);
        }
    }

    // 当鼠标离开 Collider 区域
    private void OnMouseExit()
    {
        TooltipSystem.Instance.Hide();
    }
}
