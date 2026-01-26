using UnityEngine;
using UnityEngine.UI;
using static TreeEditor.TreeEditorHelper;

public class MapNodeView : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private Image iconImage;       // 显示节点类型的图标
    [SerializeField] private Button nodeButton;     // 点击按钮
    [SerializeField] private Image outlineImage;    // (可选) 选中/可到达时的高亮框

    [Header("图标资源配置")]
    [SerializeField] private Sprite monsterSprite;
    [SerializeField] private Sprite eliteSprite;
    [SerializeField] private Sprite bossSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private Sprite restSprite;
    [SerializeField] private Sprite treasureSprite;
    [SerializeField] private Sprite eventSprite;
    [SerializeField] private Sprite unknownSprite;

    //持有数据引用
    private MapNode nodeData;

    //状态标记
    private bool isInteractable = false;

    /// <summary>
    /// 初始化节点视图 (由MapManager调用)
    /// </summary>
    public void Setup(MapNode node)
    {
        this.nodeData = node;

        //设置图标
        iconImage.sprite = GetSpriteByType(node.nodeType);

        nodeButton.onClick.RemoveAllListeners();
        nodeButton.onClick.AddListener(OnNodeClicked);

        //默认设为不可交互（需要MapManager计算出可选路径后再开启）
        SetInteractable(false);
    }

    /// <summary>
    /// 设置节点是否可以被点击（用于控制玩家只能点下一层的节点）
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        nodeButton.interactable = interactable;

        //视觉反馈：可点时正常显示，不可点时变半透明
        var color = iconImage.color;
        color.a = interactable ? 1f : 0.5f;
        iconImage.color = color;

        //如果有高亮框，可点时显示
        if (outlineImage != null)
        {
            outlineImage.enabled = interactable;
        }
    }

    private void OnNodeClicked()
    {
        if (!isInteractable) return;

        Debug.Log($"[Map] Player selected node: {nodeData.nodeType} at Layer {nodeData.y}");

        //点击后锁定，防止连点
        SetInteractable(false);

        HandleNodeInteraction();
    }

    private void HandleNodeInteraction()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // TODO: 在GameManager中加一个SetCurrentMapNode(nodeData)来记录玩家进度

        switch (nodeData.nodeType)
        {
            case NodeType.Monster:
                GameManager.Instance.EnterBattle(BattleType.Normal);
                GlobalController.Instance.NewBattle(nodeData.battleData);
                break;

            case NodeType.Elite:
                GameManager.Instance.EnterBattle(BattleType.Elite);
                GlobalController.Instance.NewBattle(nodeData.battleData);
                break;

            case NodeType.Boss:
                GameManager.Instance.EnterBattle(BattleType.Boss);
                GlobalController.Instance.NewBattle(nodeData.battleData);
                break;

            case NodeType.Event:
                HandleEventRoom();
                break;

            case NodeType.Shop:
                //TODO: 加载商店场景或打开商店UI
                Debug.Log("Enter Shop Logic...");
                break;

            case NodeType.Rest:
                //TODO: 加载安全屋场景或打开休息UI
                Debug.Log("Enter Rest Site Logic...");
                break;

            case NodeType.Treasure:
                //TODO: 打开宝箱奖励UI
                Debug.Log("Enter Treasure Logic...");
                break;

            default:
                Debug.LogWarning("Unknown node type clicked.");
                break;
        }
    }

    private void HandleEventRoom()
    {
        //调用GameManager里的概率逻辑
        var outcome = GameManager.Instance.ResolveEventRoom();

        if (outcome == EventRoomOutcome.Combat)
        {
            Debug.Log("Event Result: Combat!");
            GameManager.Instance.EnterBattle(BattleType.Normal);
        }
        else
        {
            Debug.Log("Event Result: Non-Combat Story");
            //调用UI系统弹出一个事件对话框
        }
    }

    private Sprite GetSpriteByType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Monster: return monsterSprite;
            case NodeType.Elite: return eliteSprite;
            case NodeType.Boss: return bossSprite;
            case NodeType.Shop: return shopSprite;
            case NodeType.Rest: return restSprite;
            case NodeType.Treasure: return treasureSprite;
            case NodeType.Event: return eventSprite;
            default: return unknownSprite;
        }
    }
}