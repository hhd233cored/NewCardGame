using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipSystem : Singleton<TooltipSystem>
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text contentText;

    [SerializeField] private Vector2 offset = new Vector2(20, 20);

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // 让提示框跟随鼠标位置
            transform.position = Input.mousePosition + (Vector3)offset;
        }
    }

    public void Show(string text)
    {
        tooltipPanel.SetActive(true);
        contentText.text = text;
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }
}
