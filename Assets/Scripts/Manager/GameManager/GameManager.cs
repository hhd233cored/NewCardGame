using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    //public PlayerData CurrentPlayerData { get; private set; }
    public BattleType NextBattleType { get; private set; }

    [SerializeField] private List<CardData> CardDataLibrary;
    public List<CardData> CardDataList => CardDataLibrary;

    [SerializeField] private PlayerData playerData;
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private BattleData battleData;
    [SerializeField] private GameObject PlayerView;
    [SerializeField] private GameObject StartMenu;
    [SerializeField] private GameObject VictoryMenu;


    [SerializeField] private TMP_Text victoryText;

    [Header("事件房概率控制")]
    [SerializeField] private float baseEventCombatChance = 0.1f; //初始10%
    [SerializeField] private float chanceIncrement = 0.15f;      //每次非战斗增加15%
    private float currentEventCombatChance;
    [HideInInspector]

    public List<MapLayer> CurrentMapData = new List<MapLayer>(); //保存整张地图结构

    public List<BattleData> NormalBattles = new List<BattleData>();
    public List<BattleData> EliteBattles = new List<BattleData>();
    public List<BattleData> BossBattles = new List<BattleData>();

    public MapNode CurrentNode { get; set; } //记录玩家当前站在哪个点上

    protected override void Awake()
    {
        base.Awake();
        currentEventCombatChance = baseEventCombatChance;
    }
    //测试用
    private void Start()
    {
        //StartNewRun();
        StartMenu.SetActive(true);
    }

    public void StartGame()
    {
        StartNewRun();
        var mapManager = FindObjectOfType<MapManager>();
        mapManager.StartNewGame();
        StartMenu.SetActive(false);
        VictoryMenu.SetActive(false);
    }
    public void BackToMenu()
    {
        VictoryMenu.SetActive(false);
        StartMenu.SetActive(true);
    }
    public void OpenGameOverMenu()
    {
        StartCoroutine(EnterMapScene());
        VictoryMenu.SetActive(true);
        victoryText.text = "Game Over";
    }
    public void OpenVictoryMenu()
    {
        VictoryMenu.SetActive(true);
        victoryText.text = "恭喜通关";
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    //新的一局开始时调用
    public void StartNewRun()
    {
        //初始化玩家数据
        PlayerSystem.Instance.Setup(playerData);
        PlayerView.SetActive(false);
        //生成地图
        if (CurrentMapData == null)
        {
            CurrentMapData = new List<MapLayer>();
        }
        else
        {
            CurrentMapData.Clear();
        }
        CurrentNode = null;
        currentEventCombatChance = baseEventCombatChance;
    }

    //进入战斗场景
    /*public void EnterBattle(BattleType type, string battleSceneName = "BattleScene", string mapSceneName = "MapScene")
    {
        NextBattleType = type;
        SceneManager.LoadScene(battleSceneName, LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync(mapSceneName);
    }*/

    //处理事件房逻辑
    public EventRoomOutcome ResolveEventRoom()
    {
        float roll = Random.Range(0f, 1f);
        Debug.Log($"[Event] Roll: {roll}, Current Chance: {currentEventCombatChance}");

        if (roll < currentEventCombatChance)
        {
            //触发战斗
            currentEventCombatChance = baseEventCombatChance; //重置概率
            return EventRoomOutcome.Combat;
        }
        else
        {
            //触发非战斗事件
            currentEventCombatChance += chanceIncrement; //增加下次概率
            return EventRoomOutcome.NonCombat;
        }
    }
    //进入战斗场景（协程）
    public IEnumerator EnterBattleRoutine(BattleType type, List<EnemyData> enemies, string battleSceneName = "BattleScene", string mapSceneName = "MapScene")
    {
        NextBattleType = type;

        //异步加载并等待完成
        AsyncOperation op = SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        yield return op;

        //卸载地图
        SceneManager.UnloadSceneAsync(mapSceneName);

        //此时场景加载完毕，实例已存在，可以安全初始化
        NewBattle(enemies);
    }
    //进入商店
    public IEnumerator EnterShopScene(string SceneName = "ShopScene", string mapSceneName = "MapScene")
    {
        //异步加载并等待完成
        AsyncOperation op = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
        yield return op;

        //卸载地图
        SceneManager.UnloadSceneAsync(mapSceneName);

        PlayerView.gameObject.SetActive(false);
    }
    //战斗结束返回地图界面
    public IEnumerator EnterMapScene(string SceneName = "BattleScene", string mapSceneName = "MapScene")
    {

        //异步加载并等待完成
        AsyncOperation op = SceneManager.LoadSceneAsync(mapSceneName, LoadSceneMode.Additive);
        yield return op;

        var mapManager = FindObjectOfType<MapManager>();
        mapManager.StartNewGame();

        //卸载地图
        SceneManager.UnloadSceneAsync(SceneName);

        PlayerView.gameObject.SetActive(false); 
        if (GameManager.Instance.CurrentNode.nodeType == NodeType.Boss)
        {
            GameManager.Instance.OpenVictoryMenu();
        }
    }

    //初始化新的战斗场景
    public void NewBattle(List<EnemyData> enemiesD, List<CardData> cardD = null)
    {
        PlayerView.SetActive(true);
        EnemySystem.Instance.Setup(enemiesD);
        CardSystem.Instance.Setup(PlayerSystem.Instance.player.CurrentCards);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
    public void NewBattle(BattleData battle, List<CardData> cardD = null)
    {
        PlayerView.gameObject.SetActive(true);
        EnemySystem.Instance.Setup(battle.enemies);
        CardSystem.Instance.Setup(PlayerSystem.Instance.player.CurrentCards);
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }

    //提供给MapManager保存数据用
    public void SaveMapState(List<List<MapNode>> map)
    {
        if (CurrentMapData == null) CurrentMapData = new List<MapLayer>();
        CurrentMapData.Clear();

        foreach (var layerList in map)
        {
            foreach (var node in layerList)
            {
                node.childrenCoordinates.Clear();
                foreach (var child in node.children)
                {
                    node.childrenCoordinates.Add(new Vector2Int(child.x, child.y));
                }
            }
            //包装进MapLayer
            CurrentMapData.Add(new MapLayer(layerList));
        }
        Debug.Log($"[GameManager] Map Saved! Layers count: {CurrentMapData.Count}");
    }
}


public enum EventRoomOutcome { Combat, NonCombat }
public enum BattleType { None, Normal, Elite, Boss }