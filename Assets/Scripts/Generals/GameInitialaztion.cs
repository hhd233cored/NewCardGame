using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitialaztion : MonoBehaviour
{    private void Awake()
    {
        // 1. 首先必须关闭垂直同步，否则帧率限制可能失效
        QualitySettings.vSyncCount = 0;

        // 2. 设置目标帧率
        Application.targetFrameRate = 60;

        // 2. 检查逻辑（如是否登录）
        Debug.Log("游戏初始化中...");

        // 3. 异步加载主菜单场景 (假设主菜单名字叫 "MainMenu")
        // 使用 LoadSceneMode.Single 会关闭当前初始化场景并打开新场景
        SceneManager.LoadScene("Global");
        SceneManager.LoadScene("MapScene",LoadSceneMode.Additive);
    }
}
