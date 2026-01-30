using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 滚动视图列表项基类
    /// 提供IScrollViewItem接口的默认实现，包含状态管理、事件处理和视觉效果。
    /// </summary>
    /// <remarks>
    /// 此基类提供了以下功能：
    /// - 完整的IScrollViewItem接口实现
    /// - 自动的状态管理（选中、悬停、正常）
    /// - 内置的视觉状态切换和动画
    /// - 鼠标事件处理（点击、悬停）
    /// - 可配置的状态样式
    /// - 生命周期管理和对象池支持
    /// 
    /// 继承此类时，主要需要实现：
    /// - OnDataChanged: 处理数据变化
    /// - GetPreferredSize: 返回项目的首选尺寸
    /// - 可选重写其他虚方法以自定义行为
    /// </remarks>
    /// <example>
    /// 继承示例：
    /// <code>
    /// public class ProductItem : ScrollViewItemBase
    /// {
    ///     [SerializeField] private Text nameText;
    ///     [SerializeField] private Text priceText;
    ///     
    ///     protected override void OnDataChanged(object data, int index)
    ///     {
    ///         if (data is Product product)
    ///         {
    ///             nameText.text = product.name;
    ///             priceText.text = $"${product.price:F2}";
    ///         }
    ///     }
    ///     
    ///     public override Vector2 GetPreferredSize()
    ///     {
    ///         return new Vector2(300, 80);
    ///     }
    /// }
    /// </code>
    /// </example>
    [RequireComponent(typeof(RectTransform))]
    public abstract class ScrollViewItemBase : MonoBehaviour, IScrollViewItem, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region 序列化字段

        [Header("视觉状态配置")]
        [SerializeField] protected ItemStateStyle stateStyle;

        [Header("组件引用")]
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected CanvasGroup canvasGroup;

        #endregion

        #region 私有字段

        private int index = -1;
        private int itemType = 0;
        private bool isSelected;
        private bool isHovered;
        private object itemData;
        private RectTransform rectTransform;
        private InfiniteScrollView scrollView;

        #endregion

        #region IScrollViewItem 实现

        public int Index => index;
        public int ItemType
        {
            get => itemType;
            set => itemType = value;
        }
        public bool IsSelected => isSelected;
        public bool IsHovered => isHovered;
        public RectTransform RectTransform => rectTransform;
        public InfiniteScrollView ScrollView => scrollView;

        public virtual void SetData(object data, int index)
        {
            this.itemData = data;
            this.index = index;
            OnDataChanged(data, index);
            RefreshDisplay();
        }

        public virtual void SetScrollView(InfiniteScrollView scrollView)
        {
            this.scrollView = scrollView;
        }

        public virtual void OnItemSelected(bool selected)
        {
            if (isSelected == selected)
                return;

            isSelected = selected;
            UpdateVisualState();
            OnSelectionChanged(selected);
        }

        public virtual void OnItemHovered(bool hovered)
        {
            if (isHovered == hovered)
                return;

            isHovered = hovered;
            UpdateVisualState();
            OnHoverChanged(hovered);
        }

        public virtual void OnItemClicked()
        {
            OnClick();
        }

        public virtual void RefreshDisplay()
        {
            UpdateVisualState();
            OnDisplayRefresh();
        }

        public virtual Vector2 GetPreferredSize()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform.sizeDelta;
        }

        public virtual void ResetItem()
        {
            index = -1;
            isSelected = false;
            isHovered = false;
            itemData = null;

            OnItemReset();
            UpdateVisualState();
        }

        #endregion

        #region Unity生命周期

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            OnAwake();
        }

        protected virtual void Start()
        {
            InitializeStateStyle();
            OnStart();
        }

        #endregion

        #region 事件处理

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            scrollView?.HandleItemClick(index, this);
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            scrollView?.HandleItemHoverEnter(index, this);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            scrollView?.HandleItemHoverExit(index, this);
        }

        #endregion

        #region 视觉状态管理

        protected virtual void UpdateVisualState()
        {
            if (stateStyle == null)
                return;

            ItemVisualState targetState = GetCurrentVisualState();
            ApplyVisualState(targetState);
        }

        protected virtual ItemVisualState GetCurrentVisualState()
        {
            if (isSelected)
                return stateStyle.selectedState;
            else if (isHovered)
                return stateStyle.hoverState;
            else
                return stateStyle.normalState;
        }

        protected virtual void ApplyVisualState(ItemVisualState state)
        {
            if (state == null)
                return;

            // 应用背景颜色
            if (backgroundImage != null && state.useBackgroundColor)
            {
                backgroundImage.color = state.backgroundColor;
            }

            // 应用透明度
            if (canvasGroup != null && state.useAlpha)
            {
                canvasGroup.alpha = state.alpha;
            }

            // 应用缩放
            if (state.useScale)
            {
                rectTransform.localScale = Vector3.one * state.scale;
            }

            // 应用自定义状态
            OnVisualStateApplied(state);
        }

        private void InitializeStateStyle()
        {
            if (stateStyle == null)
            {
                stateStyle = CreateDefaultStateStyle();
            }
        }

        protected virtual ItemStateStyle CreateDefaultStateStyle()
        {
            var style = ScriptableObject.CreateInstance<ItemStateStyle>();

            // 默认状态
            style.normalState = new ItemVisualState
            {
                useBackgroundColor = true,
                backgroundColor = Color.white,
                useAlpha = true,
                alpha = 1f,
                useScale = false,
                scale = 1f
            };

            // 悬停状态
            style.hoverState = new ItemVisualState
            {
                useBackgroundColor = true,
                backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1f),
                useAlpha = true,
                alpha = 1f,
                useScale = true,
                scale = 1.05f
            };

            // 选中状态
            style.selectedState = new ItemVisualState
            {
                useBackgroundColor = true,
                backgroundColor = new Color(0.2f, 0.6f, 1f, 1f),
                useAlpha = true,
                alpha = 1f,
                useScale = false,
                scale = 1f
            };

            return style;
        }

        #endregion

        #region 受保护的虚方法（供子类重写）

        /// <summary>
        /// 当Awake时调用
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>
        /// 当Start时调用
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// 当数据改变时调用
        /// </summary>
        /// <param name="data">新数据</param>
        /// <param name="index">新索引</param>
        protected abstract void OnDataChanged(object data, int index);

        /// <summary>
        /// 当选中状态改变时调用
        /// </summary>
        /// <param name="selected">是否选中</param>
        protected virtual void OnSelectionChanged(bool selected) { }

        /// <summary>
        /// 当悬停状态改变时调用
        /// </summary>
        /// <param name="hovered">是否悬停</param>
        protected virtual void OnHoverChanged(bool hovered) { }

        /// <summary>
        /// 当被点击时调用
        /// </summary>
        protected virtual void OnClick() { }

        /// <summary>
        /// 当显示刷新时调用
        /// </summary>
        protected virtual void OnDisplayRefresh() { }

        /// <summary>
        /// 当项被重置时调用
        /// </summary>
        protected virtual void OnItemReset() { }

        /// <summary>
        /// 当视觉状态被应用时调用
        /// </summary>
        /// <param name="state">应用的状态</param>
        protected virtual void OnVisualStateApplied(ItemVisualState state) { }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取当前数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        public T GetData<T>() where T : class
        {
            return itemData as T;
        }

        /// <summary>
        /// 设置状态样式
        /// </summary>
        /// <param name="style">状态样式</param>
        public void SetStateStyle(ItemStateStyle style)
        {
            stateStyle = style;
            UpdateVisualState();
        }

        #endregion
    }

}