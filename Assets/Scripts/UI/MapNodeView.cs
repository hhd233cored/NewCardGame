using UnityEngine;
using UnityEngine.UI;

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
    private Material _dynamicMaterial;

    //持有数据引用
    private MapNode nodeData;

    //状态标记
    private bool isInteractable = false;

    [SerializeField] private BattleData battleData;//这句仅测试用，更规范的写法应该是写在MapNode里

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

        if (_dynamicMaterial == null)
        {
            _dynamicMaterial = Instantiate(iconImage.material);
            iconImage.material = _dynamicMaterial;
        }
        //初始化时先关掉描边
        _dynamicMaterial.SetFloat("_ShowOutline", 0f);

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
        if (_dynamicMaterial != null)
        {
            float val = interactable ? 1f : 0f;
            _dynamicMaterial.SetFloat("_ShowOutline", val);
            iconImage.SetMaterialDirty(); 
        }
        var color = iconImage.color;
        color.a = interactable ? 1f : 0.5f;
        iconImage.color = color;

        iconImage.SetMaterialDirty();
    }

    private void OnNodeClicked()
    {
        if (!isInteractable) return;

        Debug.Log($"[Map] Player selected node: {nodeData.nodeType} at Layer {nodeData.y}");
        SetInteractable(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CurrentNode = nodeData;
        }
        HandleNodeInteraction();
    }

    private void HandleNodeInteraction()
    {
        var mapManager = FindObjectOfType<MapManager>();
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // TODO: 在GameManager中加一个SetCurrentMapNode(nodeData)来记录玩家进度

        switch (nodeData.nodeType)
        {
            case NodeType.Monster:
                if (mapManager != null) mapManager.SaveMap();
                // 使用 StartCoroutine 启动

                int rand = Random.Range(0, GameManager.Instance.NormalBattles.Count);
                BattleData battle = GameManager.Instance.NormalBattles[rand];

                StartCoroutine(GameManager.Instance.EnterBattleRoutine(BattleType.Normal, battle.enemies));
                break;

            case NodeType.Elite:
                if (mapManager != null) mapManager.SaveMap();
                // 使用 StartCoroutine 启动

                int rand2 = Random.Range(0, GameManager.Instance.EliteBattles.Count);
                BattleData battle2 = GameManager.Instance.EliteBattles[rand2];

                StartCoroutine(GameManager.Instance.EnterBattleRoutine(BattleType.Normal, battle2.enemies));
                break;

            case NodeType.Boss:
                if (mapManager != null) mapManager.SaveMap();
                // 使用 StartCoroutine 启动

                int rand3 = Random.Range(0, GameManager.Instance.BossBattles.Count);
                BattleData battle3 = GameManager.Instance.BossBattles[rand3];
                StartCoroutine(GameManager.Instance.EnterBattleRoutine(BattleType.Normal, battle3.enemies));
                break;

            case NodeType.Event:
                if (mapManager != null) mapManager.SaveMap();
                HandleEventRoom(mapManager);
                break;

            case NodeType.Shop:
                if (mapManager != null) mapManager.SaveMap();
                Debug.Log("进入商店...");
                //mapManager?.UnlockNextLayer();
                ShopManager.Instance.EnterShop();
                break;

            case NodeType.Rest:
                if (mapManager != null) mapManager.SaveMap();
                Debug.Log("进入安全屋...");
                // 简单模拟回血
                PlayerSystem.Instance.player.Recover(30);
                mapManager?.UnlockNextLayer();
                break;

            case NodeType.Treasure:
                if (mapManager != null) mapManager.SaveMap();
                Debug.Log("打开宝箱...");
                mapManager?.UnlockNextLayer();
                break;
        }
    }

    private void HandleEventRoom(MapManager mapManager)
    {
        var outcome = GameManager.Instance.ResolveEventRoom();

        if (outcome == EventRoomOutcome.Combat)
        {
            Debug.Log("事件结果：遭遇战斗！");
            StartCoroutine(GameManager.Instance.EnterBattleRoutine(BattleType.Normal, battleData.enemies));
        }
        else
        {
            Debug.Log("事件节点：进入事件剧情面板");

            //获取事件UI单例
            var eventUI = EventRoomUI.Instance;
            if (eventUI == null)
            {
                Debug.LogError("场景中找不到 EventRoomUI！无法显示事件。直接跳过。");
                mapManager?.UnlockNextLayer();
                return;
            }
            //获取一个随机事件数据
            EventData data = eventUI.GetRandomEvent();
            if (data == null)
            {
                Debug.LogWarning("EventRoomUI里没有配置RandomEvents！");
                mapManager?.UnlockNextLayer();
                return;
            }
            //显示面板，并传入解锁回调
            eventUI.ShowEvent(data, () =>
            {
                //当事件没有进入战斗时，执行这里的代码
                mapManager?.UnlockNextLayer();
            });
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