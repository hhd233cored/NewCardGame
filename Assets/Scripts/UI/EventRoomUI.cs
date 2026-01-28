using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventRoomUI : Singleton<EventRoomUI>
{
    [Header("UI引用")]
    [SerializeField] private Image eventImage;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private Transform optionContainer;
    [SerializeField] private Button optionButtonPrefab;

    [Header("测试数据")]
    [SerializeField] private List<EventData> randomEvents;

    private System.Action onEventComplete;

    private void Start()
    {
        //初始隐藏
        gameObject.SetActive(false);
    }

    public void ShowEvent(EventData data, System.Action onComplete)
    {
        onEventComplete = onComplete;
        gameObject.SetActive(true);

        //设置显示内容
        if (data.Image != null) eventImage.sprite = data.Image;
        contentText.text = data.Content;

        foreach (Transform child in optionContainer)
        {
            Destroy(child.gameObject);
        }
        // 生成新按钮
        foreach (var option in data.Options)
        {
            Button btn = Instantiate(optionButtonPrefab, optionContainer);
            btn.GetComponentInChildren<TMP_Text>().text = option.Description;
            btn.onClick.AddListener(() => OnOptionSelected(option));
        }
    }

    private void OnOptionSelected(EventOption option)
    {
        //执行选项逻辑
        switch (option.Type)
        {
            case EventOptionType.GainGold:
                PlayerSystem.Instance.player.ChangeGold(option.IntValue);
                CloseEvent();
                break;

            case EventOptionType.RecoverHealth:
                PlayerSystem.Instance.player.Recover(option.IntValue);
                CloseEvent();
                break;

            case EventOptionType.LoseHealth:
                PlayerSystem.Instance.player.Damage(option.IntValue); // 也可以写个扣血方法
                CloseEvent();
                break;

            case EventOptionType.Combat:
                //先关闭界面，然后切战斗
                gameObject.SetActive(false);
                StartCoroutine(GameManager.Instance.EnterBattleRoutine(BattleType.Normal, option.BattleData.enemies));
                break;

            case EventOptionType.Leave:
            default:
                CloseEvent();
                break;
        }
    }

    public void CloseEvent()
    {
        gameObject.SetActive(false);
        onEventComplete?.Invoke();
    }

    //随机获取一个事件（供外部调用）
    public EventData GetRandomEvent()
    {
        if (randomEvents.Count == 0) return null;
        return randomEvents[Random.Range(0, randomEvents.Count)];
    }
}