using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistentSingleton<GameManager>
{
    public PlayerData CurrentPlayerData { get; private set; }
    public BattleType NextBattleType { get; private set; }

    [Header("事件房概率控制")]
    [SerializeField] private float baseEventCombatChance = 0.1f; //初始10%
    [SerializeField] private float chanceIncrement = 0.15f;      //每次非战斗增加15%
    private float currentEventCombatChance;

    protected override void Awake()
    {
        base.Awake();
        currentEventCombatChance = baseEventCombatChance;
    }

    //新的一局开始时调用
    public void StartNewRun(PlayerData initialData)
    {
        CurrentPlayerData = initialData;
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
        GlobalController.Instance.NewBattle(enemies);
    }
}


public enum EventRoomOutcome { Combat, NonCombat }
public enum BattleType { None, Normal, Elite, Boss }