using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UGC.InfiniteScrollView;

namespace UGC.InfiniteScrollView.Samples
{
    /// <summary>
    /// 简单列表项示例
    /// 展示如何实现一个基本的列表项
    /// </summary>
    public class SimpleListItem : ScrollViewItemBase
    {
        #region 序列化字段

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button actionButton;

        #endregion

        #region 私有字段

        private SimpleItemData itemData;
        private Vector3 originalScale;
        private Coroutine scaleCoroutine;

        #endregion

        #region Unity生命周期

        protected override void Awake()
        {
            base.Awake();

            originalScale = transform.localScale;

            // 设置按钮事件
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(OnActionButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (actionButton != null)
            {
                actionButton.onClick.RemoveListener(OnActionButtonClicked);
            }
        }

        #endregion

        #region IScrollViewItem 实现

        public override void SetData(object data, int index)
        {
            base.SetData(data, index);
        }

        //public override Vector2 GetPreferredSize()
        //{
        //    // 返回预设的大小，也可以根据内容动态计算
        //    return new Vector2(300, 80);
        //}

        public override void ResetItem()
        {
            base.ResetItem();

            itemData = null;

            if (titleText != null)
                titleText.text = string.Empty;

            if (descriptionText != null)
                descriptionText.text = string.Empty;

            if (iconImage != null)
                iconImage.sprite = null;

            transform.localScale = originalScale;

            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }
        }

        #endregion

        #region 抽象方法实现

        protected override void OnDataChanged(object data, int index)
        {
            SimpleItemData itemData;
            if (data is GroupedDataHelper<SimpleItemData>.DisplayData displayData)
            {
                itemData = displayData.Data as SimpleItemData;
            }
            else
            {
                itemData = data as SimpleItemData;
            }

            if (itemData != null)
            {
                UpdateDisplay();
            }
        }

        #endregion

        #region 受保护方法重写

        public override void OnItemSelected(bool selected)
        {
            base.OnItemSelected(selected);

            // 可以在这里添加选中状态的特殊处理
            Debug.Log($"Item {Index} selected: {selected}");
        }

        public override void OnItemHovered(bool hovered)
        {
            base.OnItemHovered(hovered);
        }

        public override void OnItemClicked()
        {
            base.OnItemClicked();

            Debug.Log($"Item {Index} clicked: {itemData?.Title}");
        }

        #endregion

        #region 私有方法

        private void UpdateDisplay()
        {
            if (itemData == null) return;

            if (titleText != null)
            {
                titleText.text = itemData.Title;
            }

            if (descriptionText != null)
            {
                descriptionText.text = itemData.Description;
            }

            if (iconImage != null && itemData.Icon != null)
            {
                iconImage.sprite = itemData.Icon;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        private void OnActionButtonClicked()
        {
            Debug.Log($"Action button clicked for item: {itemData?.Title}");

            // 触发自定义事件
            itemData?.OnActionClicked?.Invoke(itemData);
        }

        #endregion
    }

    /// <summary>
    /// 简单列表项数据
    /// </summary>
    [System.Serializable]
    public class SimpleItemData
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private int iconIndex;
        [SerializeField] private Color backgroundColor = Color.white;

        public string Id
        {
            get => id;
            set => id = value;
        }

        public string Title
        {
            get => title;
            set => title = value;
        }

        public string Description
        {
            get => description;
            set => description = value;
        }

        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }

        public int IconIndex
        {
            get => iconIndex;
            set => iconIndex = value;
        }

        public Color BackgroundColor
        {
            get => backgroundColor;
            set => backgroundColor = value;
        }

        /// <summary>
        /// 动作按钮点击事件
        /// </summary>
        public System.Action<SimpleItemData> OnActionClicked { get; set; }

        public SimpleItemData()
        {
        }

        public SimpleItemData(string title, string description = null, Sprite icon = null)
        {
            this.title = title;
            this.description = description;
            this.icon = icon;
        }
    }
}