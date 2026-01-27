using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class HandView : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float cardSpacing = 1f / 10f;
    [SerializeField] private float depthStep = 0.01f;

    [Header("Hover Layout (spline-space)")]
    [SerializeField] private float hoverGapParam = 0.06f;  // 右侧整体沿弧线“插入空位”的大小（建议先设为 cardSpacing 左右）
    [SerializeField] private float hoverPopWorld = 0.8f;   // hover 卡沿法线突出（世界单位）
    [SerializeField] private float hoverTween = 0.12f;

    private readonly List<CardView> cards = new();
    private CardView hovered;
    private readonly HashSet<CardView> locked = new();
    public void LockCard(CardView cv) { if (cv != null) locked.Add(cv); }
    public void UnlockCard(CardView cv) { if (cv != null) locked.Remove(cv); }

    public IEnumerator AddCard(CardView cardView)
    {
        cards.Add(cardView);
        cardView.SetOwnerHand(this);
        yield return UpdateLayout(0.15f);
    }

    public IEnumerator RemoveCard(CardView cardView, float duration = 0.15f)
    {
        if (cardView == null) yield break;

        cards.Remove(cardView);
        if (hovered == cardView) hovered = null;

        yield return UpdateLayout(duration);
    }
    private CardView GetCardView(Card card)
    {
        return cards.Where(cardView=>cardView.card == card).FirstOrDefault();
    }
    public void OnCardHoverEnter(CardView card)
    {
        hovered = card;
        StartCoroutine(UpdateLayout(hoverTween));
    }

    public void OnCardHoverExit(CardView card)
    {
        if (hovered == card) hovered = null;
        StartCoroutine(UpdateLayout(hoverTween));
    }

    private IEnumerator UpdateLayout(float duration)
    {
        if (cards.Count == 0) yield break;

        int hoverIndex = hovered == null ? -1 : cards.IndexOf(hovered);

        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;
        Spline spline = splineContainer.Spline;

        // 重置排序（避免 hover 后不恢复）
        for (int i = 0; i < cards.Count; i++)
            SetCardSorting(cards[i], i);

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].ResetDescription();

            if (locked.Contains(cards[i]))
                continue;
            // 1) 先算原始参数 p（不带挤开）
            float p = firstCardPosition + i * cardSpacing;

            // 2) 沿弧线“挤开”：hover 右侧整体 p += gap
            float pShift = 0f;
            if (hoverIndex >= 0 && i > hoverIndex)
                pShift = hoverGapParam;

            float p2 = p + pShift;

            // 防止 Evaluate 越界（有些版本会 clamp，有些不会，保险起见手动 clamp）
            p2 = Mathf.Clamp01(p2);

            // 3) 从 spline 取位置和切线
            Vector3 localPos = spline.EvaluatePosition(p2);
            Vector3 tangent = spline.EvaluateTangent(p2);

            Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);

            // 4) 旋转：只绕 Z
            Vector2 t2 = new Vector2(tangent.x, tangent.y);
            if (t2.sqrMagnitude < 1e-6f) t2 = Vector2.right;
            float angleZ = Mathf.Atan2(t2.y, t2.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0f, 0f, angleZ);

            // 5) hover 卡突出：沿切线左法线
            Vector2 normal2 = new Vector2(-t2.y, t2.x);
            if (normal2.sqrMagnitude < 1e-6f) normal2 = Vector2.up;
            normal2.Normalize();
            Vector3 popDir = new Vector3(normal2.x, normal2.y, 0f);

            Vector3 popOffset = Vector3.zero;
            if (i == hoverIndex)
            {
                popOffset = popDir * hoverPopWorld;
                BringCardToFront(cards[i], 1000);
            }

            // 6) 最终位置
            Vector3 targetPos = worldPos + popOffset + depthStep * i * Vector3.back;

            var tr = cards[i].transform;

            // 关键：先杀掉该卡上一轮布局 tween，避免堆积
            tr.DOKill(); // 会 kill 该 transform 上的所有 tween（move/rotate/scale 等）

            tr.DOMove(targetPos, duration).SetEase(Ease.OutCubic);
            tr.DORotateQuaternion(rot, duration).SetEase(Ease.OutCubic);
        }

        yield return new WaitForSeconds(duration);
    }

    private void SetCardSorting(CardView cv, int order)
    {
        // SpriteRenderer
        var srs = cv.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in srs)
            r.sortingOrder = order;

        // TextMeshPro (3D)：调它的 MeshRenderer.sortingOrder
        var tmp3ds = cv.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var t in tmp3ds)
        {
            var mr = t.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = order;
        }

        // TextMeshProUGUI：如果你卡牌文字是UGUI，必须依赖 Canvas 排序
        // 让卡牌内部 Canvas（如果有）覆盖排序
        var canvases = cv.GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
        {
            c.overrideSorting = true;
            c.sortingOrder = order;
        }
    }

    private void BringCardToFront(CardView cv, int add)
    {
        var srs = cv.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in srs)
            r.sortingOrder += add;

        var tmp3ds = cv.GetComponentsInChildren<TextMeshPro>(true);
        foreach (var t in tmp3ds)
        {
            var mr = t.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder += add;
        }

        var canvases = cv.GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
        {
            c.overrideSorting = true;
            c.sortingOrder += add;
        }
    }
    public bool TryGetBasePose(CardView cv, out Vector3 pos, out Quaternion rot)
    {
        pos = default;
        rot = default;

        if (cv == null) return false;
        int i = cards.IndexOf(cv);
        if (i < 0) return false;
        if (cards.Count == 0) return false;

        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;
        float p = firstCardPosition + i * cardSpacing;

        Spline spline = splineContainer.Spline;
        float p2 = Mathf.Clamp01(p);

        Vector3 localPos = spline.EvaluatePosition(p2);
        Vector3 tangent = spline.EvaluateTangent(p2);

        Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);

        Vector2 t2 = new Vector2(tangent.x, tangent.y);
        if (t2.sqrMagnitude < 1e-6f) t2 = Vector2.right;
        float angleZ = Mathf.Atan2(t2.y, t2.x) * Mathf.Rad2Deg;
        rot = Quaternion.Euler(0f, 0f, angleZ);

        // 注意：这里不加 hover 的 popOffset，也不做右侧挤开 gap
        pos = worldPos + depthStep * i * Vector3.back;

        return true;
    }
}