using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UGC.InfiniteScrollView;

namespace UGC.InfiniteScrollView.Samples
{
    /// <summary>
    /// 简单滚动视图演示
    /// 展示如何使用InfiniteScrollView组件
    /// </summary>
    public class SimpleScrollViewDemo : MonoBehaviour
    {
        #region 序列化字段
        
        [Header("Scroll View")]
        [SerializeField] private InfiniteScrollView scrollView;
        
        [Header("Demo Controls")]
        [SerializeField] private Button addItemButton;
        [SerializeField] private Button removeItemButton;
        [SerializeField] private Button clearAllButton;
        [SerializeField] private Button scrollToTopButton;
        [SerializeField] private Button scrollToBottomButton;
        [SerializeField] private Button generateDataButton;
        
        [Header("Demo Settings")]
        [SerializeField] private int initialItemCount = 100;
        [SerializeField] private string[] sampleTitles = {
            "Sample Item",
            "Demo Content",
            "Test Data",
            "Example Entry",
            "List Item",
            "Scroll Element",
            "UI Component",
            "Data Entry"
        };
        
        [SerializeField] private string[] sampleDescriptions = {
            "This is a sample description for the list item.",
            "Demo content with some additional information.",
            "Test data entry with detailed description.",
            "Example entry showing various content types.",
            "List item with customizable properties.",
            "Scroll element demonstrating smooth scrolling.",
            "UI component with interactive features.",
            "Data entry with rich content support."
        };
        
        #endregion
        
        #region 私有字段
        
        private List<SimpleItemData> itemDataList = new List<SimpleItemData>();
        private int itemCounter = 0;
        
        #endregion
        
        #region Unity生命周期
        
        private void Start()
        {
            InitializeDemo();
            SetupEventHandlers();
            GenerateInitialData();
        }
        
        private void OnDestroy()
        {
            CleanupEventHandlers();
        }
        
        #endregion
        
        #region 初始化
        
        private void InitializeDemo()
        {
            if (scrollView == null)
            {
                scrollView = FindObjectOfType<InfiniteScrollView>();
                if (scrollView == null)
                {
                    Debug.LogError("InfiniteScrollView not found in the scene!");
                    return;
                }
            }
            
            // 设置滚动视图事件
            scrollView.OnItemClicked += OnItemClicked;
            scrollView.OnItemSelected += OnItemSelected;
            scrollView.OnItemDeselected += OnItemDeselected;
            scrollView.OnItemHoverEnter += OnItemHoverEnter;
            scrollView.OnItemHoverExit += OnItemHoverExit;
            scrollView.OnScrollPositionChanged += OnScrollValueChanged;
        }
        
        private void SetupEventHandlers()
        {
            if (addItemButton != null)
                addItemButton.onClick.AddListener(AddRandomItem);
            
            if (removeItemButton != null)
                removeItemButton.onClick.AddListener(RemoveLastItem);
            
            if (clearAllButton != null)
                clearAllButton.onClick.AddListener(ClearAllItems);
            
            if (scrollToTopButton != null)
                scrollToTopButton.onClick.AddListener(ScrollToTop);
            
            if (scrollToBottomButton != null)
                scrollToBottomButton.onClick.AddListener(ScrollToBottom);
            
            if (generateDataButton != null)
                generateDataButton.onClick.AddListener(GenerateRandomData);
        }
        
        private void CleanupEventHandlers()
        {
            if (addItemButton != null)
                addItemButton.onClick.RemoveListener(AddRandomItem);
            
            if (removeItemButton != null)
                removeItemButton.onClick.RemoveListener(RemoveLastItem);
            
            if (clearAllButton != null)
                clearAllButton.onClick.RemoveListener(ClearAllItems);
            
            if (scrollToTopButton != null)
                scrollToTopButton.onClick.RemoveListener(ScrollToTop);
            
            if (scrollToBottomButton != null)
                scrollToBottomButton.onClick.RemoveListener(ScrollToBottom);
            
            if (generateDataButton != null)
                generateDataButton.onClick.RemoveListener(GenerateRandomData);
            
            if (scrollView != null)
            {
                scrollView.OnItemClicked -= OnItemClicked;
                scrollView.OnItemSelected -= OnItemSelected;
                scrollView.OnItemDeselected -= OnItemDeselected;
                scrollView.OnItemHoverEnter -= OnItemHoverEnter;
                scrollView.OnItemHoverExit -= OnItemHoverExit;
                scrollView.OnScrollPositionChanged -= OnScrollValueChanged;
            }
        }
        
        #endregion
        
        #region 数据管理
        
        private void GenerateInitialData()
        {
            itemDataList.Clear();
            
            for (int i = 0; i < initialItemCount; i++)
            {
                var itemData = CreateRandomItemData();
                itemDataList.Add(itemData);
            }
            
            UpdateScrollView();
        }
        
        private SimpleItemData CreateRandomItemData()
        {
            string title = sampleTitles[Random.Range(0, sampleTitles.Length)] + " #" + (++itemCounter);
            string description = sampleDescriptions[Random.Range(0, sampleDescriptions.Length)];
            
            var itemData = new SimpleItemData(title, description);
            itemData.BackgroundColor = new Color(
                Random.Range(0.8f, 1f),
                Random.Range(0.8f, 1f),
                Random.Range(0.8f, 1f),
                1f
            );
            
            // 设置动作按钮回调
            itemData.OnActionClicked = OnItemActionClicked;
            
            return itemData;
        }
        
        private void UpdateScrollView()
        {
            if (scrollView != null)
            {
                scrollView.SetData(itemDataList);
            }
        }
        
        #endregion
        
        #region 按钮事件处理
        
        private void AddRandomItem()
        {
            var newItem = CreateRandomItemData();
            itemDataList.Add(newItem);
            
            scrollView.AddItem(newItem);
            
            Debug.Log($"Added item: {newItem.Title}");
        }
        
        private void RemoveLastItem()
        {
            if (itemDataList.Count > 0)
            {
                int lastIndex = itemDataList.Count - 1;
                var removedItem = itemDataList[lastIndex];
                
                itemDataList.RemoveAt(lastIndex);
                scrollView.RemoveItem(lastIndex);
                
                Debug.Log($"Removed item: {removedItem.Title}");
            }
        }
        
        private void ClearAllItems()
        {
            itemDataList.Clear();
            scrollView.ClearData();
            
            Debug.Log("Cleared all items");
        }
        
        private void ScrollToTop()
        {
            scrollView.ScrollToIndex(0, true);
        }
        
        private void ScrollToBottom()
        {
            if (itemDataList.Count > 0)
            {
                scrollView.ScrollToIndex(itemDataList.Count - 1, true);
            }
        }
        
        private void GenerateRandomData()
        {
            int count = Random.Range(50, 200);
            
            itemDataList.Clear();
            for (int i = 0; i < count; i++)
            {
                var itemData = CreateRandomItemData();
                itemDataList.Add(itemData);
            }
            
            UpdateScrollView();
            
            Debug.Log($"Generated {count} random items");
        }
        
        #endregion
        
        #region 滚动视图事件处理
        
        private void OnItemClicked(int index, IScrollViewItem item)
        {
            if (index >= 0 && index < itemDataList.Count)
            {
                var itemData = itemDataList[index];
                Debug.Log($"Item clicked: {itemData.Title} (Index: {index})");
            }
        }
        
        private void OnItemSelected(int index, IScrollViewItem item)
        {
            if (index >= 0 && index < itemDataList.Count)
            {
                var itemData = itemDataList[index];
                Debug.Log($"Item selected: {itemData.Title} (Index: {index})");
            }
        }
        
        private void OnItemDeselected(int index, IScrollViewItem item)
        {
            if (index >= 0 && index < itemDataList.Count)
            {
                var itemData = itemDataList[index];
                Debug.Log($"Item deselected: {itemData.Title} (Index: {index})");
            }
        }
        
        private void OnItemHoverEnter(int index, IScrollViewItem item)
        {
            if (index >= 0 && index < itemDataList.Count)
            {
                var itemData = itemDataList[index];
                Debug.Log($"Item hover enter: {itemData.Title} (Index: {index})");
            }
        }
        
        private void OnItemHoverExit(int index, IScrollViewItem item)
        {
            if (index >= 0 && index < itemDataList.Count)
            {
                var itemData = itemDataList[index];
                Debug.Log($"Item hover exit: {itemData.Title} (Index: {index})");
            }
        }
        
        private void OnScrollValueChanged(Vector2 scrollPosition)
        {
            // 可以在这里处理滚动位置变化
            // Debug.Log($"Scroll position: {scrollPosition}");
        }
        
        private void OnItemActionClicked(SimpleItemData itemData)
        {
            Debug.Log($"Action clicked for: {itemData.Title}");
            
            // 可以在这里处理特定的动作，比如显示详情、编辑等
            ShowItemDetails(itemData);
        }
        
        #endregion
        
        #region 辅助方法
        
        private void ShowItemDetails(SimpleItemData itemData)
        {
            // 这里可以显示一个详情面板或执行其他操作
            Debug.Log($"Showing details for: {itemData.Title}\nDescription: {itemData.Description}");
        }
        
        #endregion
        
        #region 公共方法（供外部调用）
        
        /// <summary>
        /// 添加指定数量的随机项
        /// </summary>
        /// <param name="count">要添加的项数量</param>
        public void AddRandomItems(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var newItem = CreateRandomItemData();
                itemDataList.Add(newItem);
                scrollView.AddItem(newItem);
            }
            
            Debug.Log($"Added {count} random items");
        }
        
        /// <summary>
        /// 滚动到指定索引
        /// </summary>
        /// <param name="index">目标索引</param>
        /// <param name="animated">是否使用动画</param>
        public void ScrollToIndex(int index, bool animated = true)
        {
            if (index >= 0 && index < itemDataList.Count)
            {
                scrollView.ScrollToIndex(index, animated);
            }
        }
        
        /// <summary>
        /// 获取当前数据数量
        /// </summary>
        /// <returns>数据数量</returns>
        public int GetDataCount()
        {
            return itemDataList.Count;
        }
        
        /// <summary>
        /// 获取指定索引的数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>数据对象</returns>
        public SimpleItemData GetItemData(int index)
        {
            if (index >= 0 && index < itemDataList.Count)
            {
                return itemDataList[index];
            }
            return null;
        }
        
        #endregion
    }
}