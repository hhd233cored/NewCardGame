using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CardView : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text SuitAndNum;
    [SerializeField] private SpriteRenderer image;
    [SerializeField] private SpriteRenderer backgroundImage;
    [SerializeField] private GameObject wrapper;

    [Header("Drag")]
    [SerializeField] private float dragLiftZ = -2f;          // 拖拽时提到镜头前一点（2D 可不改）
    [SerializeField] private float playThresholdY = 1.5f;    // 世界坐标：超过/低于该线算“出牌”
    [SerializeField] private float snapBackTween = 0.12f;

    public event Action<CardView> Clicked;

    public Card card { get; private set; }
    private HandView ownerHand;

    private bool isInDeckView => DeckViewUI.Instance.active;
    private bool isDragging;
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    private Vector3 dragMouseOffset; // 鼠标点与卡中心偏移
    [SerializeField] private Camera cam;

    [Header("属性")]
    [SerializeField] private SuitStyle Suit;
    [SerializeField] private int Num;
    public SuitStyle CardSuit => Suit;
    public int CardNum => Num;

    private Material _dynamicMaterial;

    public void Setup(Card c)
    {
        card = c;
        title.text = c.Title;
        description.text = c.GetDynamicDescription();
        SuitAndNum.text = BattleSystem.SuitToStr(c.Suit) + BattleSystem.NumToStr(c.Num);
        image.sprite = c.Image;

        Suit = c.Suit;
        Num = c.Num;

        if (_dynamicMaterial == null)
        {
            _dynamicMaterial = Instantiate(backgroundImage.material);
            backgroundImage.material = _dynamicMaterial;
            _dynamicMaterial.SetFloat("_ShowOutline", 0f);
        }
        Debug.Log(_dynamicMaterial);
    }
    public void SetOutLine(int num)
    {
        _dynamicMaterial.SetFloat("_ShowOutline", num);
    }
    public void ResetDescription()
    {
        description.text = card.GetDynamicDescription();
    }

    public void SetOwnerHand(HandView hand) => ownerHand = hand;

    private void OnMouseEnter()
    {
        if (isInDeckView) return;
        if (!Interactions.Instance.PlayerCanHover()) return;
        if (isDragging) return;
        ownerHand?.OnCardHoverEnter(this);
    }

    private void OnMouseExit()
    {
        if (isInDeckView) return;
        if (!Interactions.Instance.PlayerCanHover()) return;
        if (isDragging) return;
        ownerHand?.OnCardHoverExit(this);
    }

    private void OnMouseDown()
    {
        if (isInDeckView) return;
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (CardSystem.Instance.IsChoosingDiscard)
        {
            Clicked?.Invoke(this);
            return;
        }
        if (card.ManualTargetEffects != null && BattleSystem.Instance.HasSameSuitOrNum(CardSuit, CardNum))
        {
            // 1) 先取消 hover，让手牌状态回到“非突出”
            ownerHand?.OnCardHoverExit(this);

            // 2) 锁住，防止 UpdateLayout tween 在瞄准期间继续拉它
            ownerHand?.LockCard(this);

            // 3) 用“基础布局位姿”作为回弹目标（而不是当前 transform）
            if (ownerHand != null && ownerHand.TryGetBasePose(this, out var basePos, out var baseRot))
            {
                dragStartPosition = basePos;
                dragStartRotation = baseRot;
            }
            else
            {
                dragStartPosition = transform.position;
                dragStartRotation = transform.rotation;
            }

            transform.DOKill();

            isDragging = true;
            Interactions.Instance.PlayerIsDragging = true;

            ManualTargetSystem.Instance.StartTargeting(transform.position);
            return;
        }
        else
        {
            Clicked?.Invoke(this);

            // 开始拖拽
            isDragging = true;
            Interactions.Instance.PlayerIsDragging = true;

            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;

            // 计算鼠标世界坐标
            Vector3 mouseWorld = GetMouseWorld();
            dragMouseOffset = transform.position - mouseWorld;

            // 停止布局 tween（否则手牌布局会和拖拽打架）
            transform.DOKill();
        }
    }

    private void OnMouseDrag()
    {
        if (isInDeckView) return;
        if (CardSystem.Instance.IsChoosingDiscard) return;
        if (!Interactions.Instance.PlayerCanInteract()) return;
        if (!isDragging) return;
        if (card.ManualTargetEffects != null && BattleSystem.Instance.HasSameSuitOrNum(CardSuit, CardNum)) return;
        Vector3 mouseWorld = GetMouseWorld();
        Vector3 target = mouseWorld + dragMouseOffset;

        // 拖拽时可以固定 z（2D 一般 z=0）
        target.z = dragStartPosition.z + dragLiftZ;

        transform.position = target;
    }

    private void OnMouseUp()
    {
        if (isInDeckView) return;
        if (!isDragging) return;

        isDragging = false;

        Interactions.Instance.PlayerIsDragging = false;
        if (card.ManualTargetEffects != null && BattleSystem.Instance.HasSameSuitOrNum(CardSuit, CardNum))
        {
            Enemy target = ManualTargetSystem.Instance.EndTargeting(GetMouseWorldOnZ(0f));
            ownerHand?.UnlockCard(this);

            if (target != null)
            {
                ActionSystem.Instance.Perform(new PlayCardGA(this, target));
                return;
            }

            SnapBack();
            return;
        }
        else
        {
            // 判定是否“出牌”
            bool shouldPlay = transform.position.y >= playThresholdY;

            if (shouldPlay)
            {
                bool accepted = CardSystem.Instance.TryPlayCardFromDrag(this);
                if (!accepted)
                {
                    // 没被接受（比如不在手牌/系统忙/费用不够），回弹
                    SnapBack();
                }
                // accepted 的情况下，后续由 ActionSystem / DiscardOne 动画接管
                return;
            }
        }
        // 没到出牌区：回弹
        SnapBack();
    }

    private void CancelDragSnapBack()
    {
        // 交互被禁止时，强制回弹并清理 dragging 状态
        isDragging = false;
        Interactions.Instance.PlayerIsDragging = false;
        SnapBack();
    }

    private void SnapBack()
    {
        transform.DOKill();
        transform.DOMove(dragStartPosition, snapBackTween).SetEase(Ease.OutCubic);
        transform.DORotateQuaternion(dragStartRotation, snapBackTween).SetEase(Ease.OutCubic);
    }

    private Vector3 GetMouseWorld()
    {
        var cam = Camera.main;
        Vector3 m = Input.mousePosition;

        // 2D：z 用当前物体到摄像机的距离，保证转换正确
        float z = Mathf.Abs(transform.position.z - cam.transform.position.z);
        m.z = z;

        return cam.ScreenToWorldPoint(m);
    }
    private Vector3 GetMouseWorldOnZ(float zPlane = 0f)
    {
        var cam = Camera.main;
        Vector3 m = Input.mousePosition;
        m.z = zPlane - cam.transform.position.z; // 相机到平面的距离
        Vector3 w = cam.ScreenToWorldPoint(m);
        w.z = zPlane;
        return w;
    }
}