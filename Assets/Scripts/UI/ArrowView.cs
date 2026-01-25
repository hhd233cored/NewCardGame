using UnityEngine;

public class ArrowView : MonoBehaviour
{
    [SerializeField] private Transform arrowHead;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float headBackOffset = 0.5f;
    [SerializeField] private Camera cam;

    private Vector3 startPosition;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        Vector3 endPosition = GetMouseWorldOnZ0();   // 如果你的卡牌不在 z=0，改这里
        Vector3 dir = endPosition - startPosition;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Vector3 dirN = dir.normalized;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition - dirN * headBackOffset);

        arrowHead.position = endPosition;
        arrowHead.right = dirN; // 头部默认朝右，使用 right
    }

    public void SetupArrow(Vector3 startPosition)
    {
        this.startPosition = startPosition;
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition);
    }

    private Vector3 GetMouseWorldOnZ0()
    {
        Vector3 mp = Input.mousePosition;

        // 把鼠标点投射到 z=0 平面：给 z 一个“相机到平面的距离”
        mp.z = -cam.transform.position.z;   // 常见 2D 相机 z=-10，则这里=10

        Vector3 wp = cam.ScreenToWorldPoint(mp);
        wp.z = 0f;
        return wp;
    }
}