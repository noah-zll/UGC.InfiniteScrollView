using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 无限滚动列表视图组件
    /// 基于Unity UI ScrollView实现的高性能无限滚动列表，支持虚拟化渲染、对象池管理、多种布局模式和丰富的交互功能。
    /// </summary>
    /// <remarks>
    /// 主要特性：
    /// - 虚拟化渲染：只渲染可见区域的项目，支持大数据集
    /// - 对象池管理：自动复用UI对象，减少GC压力
    /// - 多种布局：支持垂直、水平、网格布局
    /// - 交互状态：支持悬停、选中、点击等状态
    /// - 平滑动画：内置缓动动画和过渡效果
    /// - 数据绑定：支持动态数据更新和事件通知
    /// </remarks>
    /// <example>
    /// 基础使用示例：
    /// <code>
    /// // 设置数据
    /// var data = new List&lt;string&gt; { "Item 1", "Item 2", "Item 3" };
    /// scrollView.SetData(data);
    /// 
    /// // 监听事件
    /// scrollView.OnItemClicked.AddListener(index => Debug.Log($"Clicked: {index}"));
    /// 
    /// // 滚动到指定位置
    /// scrollView.ScrollToIndex(10);
    /// </code>
    /// </example>
    [RequireComponent(typeof(ScrollRect))]
    public class InfiniteScrollView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region 序列化字段

        [Header("基础配置")]
        /// <summary>
        /// 列表项预制体，必须包含实现了IScrollViewItem接口的组件
        /// </summary>
        [SerializeField] private GameObject itemPrefab;

        /// <summary>
        /// 额外的列表项预制体，用于多样式支持（如分组标题）
        /// 索引对应 OnGetItemType 返回的类型值减 1
        /// </summary>
        [SerializeField] private List<GameObject> extraPrefabs = new List<GameObject>();

        /// <summary>
        /// 对象池大小，建议设置为可见项数的1.5-2倍
        /// </summary>
        [SerializeField] private int poolSize = 20;

        /// <summary>
        /// 列表项之间的间距（像素）
        /// </summary>
        [SerializeField] private float itemSpacing = 5f;

        /// <summary>
        /// 内容区域的内边距
        /// </summary>
        [SerializeField] private RectOffset padding = new RectOffset();

        [Header("布局设置")]
        /// <summary>
        /// 布局类型：垂直、水平或网格
        /// </summary>
        [SerializeField] private LayoutType layoutType = LayoutType.Vertical;

        /// <summary>
        /// 网格布局的约束数量（每行/列的项目数）
        /// </summary>
        [SerializeField] private int constraintCount = 1;

        /// <summary>
        /// 网格布局的单元格大小
        /// </summary>
        [SerializeField] private Vector2 cellSize = new Vector2(100, 100);

        /// <summary>
        /// 网格布局的单元格间距
        /// </summary>
        [SerializeField] private Vector2 cellSpacing = new Vector2(5, 5);

        [Header("性能优化")]
        /// <summary>
        /// 是否启用虚拟化渲染，大数据集时建议启用
        /// </summary>
        [SerializeField] private bool enableVirtualization = true;

        /// <summary>
        /// 预加载距离，超出可视区域多远开始预加载项目
        /// </summary>
        [SerializeField] private float preloadDistance = 200f;

        /// <summary>
        /// 最大可见项目数量，用于性能控制
        /// </summary>
        [SerializeField] private int maxVisibleItems = 50;

        [Header("交互状态")]
        /// <summary>
        /// 是否启用悬停状态效果
        /// </summary>
        [SerializeField] private bool enableHoverState = true;

        /// <summary>
        /// 是否启用选中状态功能
        /// </summary>
        [SerializeField] private bool enableSelectionState = true;

        /// <summary>
        /// 是否允许多选
        /// </summary>
        [SerializeField] private bool allowMultipleSelection = false;

        /// <summary>
        /// 是否启用选中状态功能
        /// </summary>
        public bool EnableSelectionState
        {
            get => enableSelectionState;
            set
            {
                enableSelectionState = value;
                if (stateManager != null)
                {
                    stateManager.EnableSelectionState = value;
                }
            }
        }

        /// <summary>
        /// 是否允许多选
        /// </summary>
        public bool AllowMultipleSelection
        {
            get => allowMultipleSelection;
            set
            {
                allowMultipleSelection = value;
                if (stateManager != null)
                {
                    stateManager.AllowMultipleSelection = value;
                }
            }
        }

        [Header("滚动设置")]
        /// <summary>
        /// 是否启用惯性滚动
        /// </summary>
        [SerializeField] private bool enableInertia = true;

        /// <summary>
        /// 减速率，控制惯性滚动的减速速度
        /// </summary>
        [SerializeField] private float decelerationRate = 0.135f;

        /// <summary>
        /// 是否启用边界回弹效果
        /// </summary>
        [SerializeField] private bool enableBounce = true;

        #endregion

        #region 私有字段

        private ScrollRect scrollRect;
        private RectTransform content;
        private RectTransform viewport;

        private Dictionary<int, ObjectPool<GameObject>> itemPools;
        private List<ScrollViewItemData> dataSource = new List<ScrollViewItemData>();
        internal Dictionary<int, GameObject> activeItems = new Dictionary<int, GameObject>();

        /// <summary>
        /// 获取指定索引的列表项类型回调
        /// 返回值：0为默认itemPrefab，1+为extraPrefabs[index-1]
        /// </summary>
        public Func<int, int> OnGetItemType;

        private LayoutManager layoutManager;
        private ItemStateManager stateManager;

        private bool isDragging;
        private int firstVisibleIndex = -1;
        private int lastVisibleIndex = -1;

        #endregion

        #region 公共属性

        /// <summary>
        /// 数据源总数量
        /// </summary>
        public int DataCount => dataSource.Count;

        /// <summary>
        /// 当前选中的索引列表
        /// </summary>
        public List<int> SelectedIndices => stateManager?.SelectedIndices ?? new List<int>();

        /// <summary>
        /// 当前悬停的索引
        /// </summary>
        public int HoveredIndex => stateManager?.HoveredIndex ?? -1;

        /// <summary>
        /// 是否启用虚拟化
        /// </summary>
        public bool EnableVirtualization
        {
            get => enableVirtualization;
            set
            {
                enableVirtualization = value;
                RefreshLayout();
            }
        }

        /// <summary>
        /// 布局类型
        /// </summary>
        public LayoutType LayoutType
        {
            get => layoutType;
            set
            {
                layoutType = value;
                InitializeLayoutManager();
                RefreshLayout();
            }
        }

        /// <summary>
        /// 当前可见项目数量
        /// </summary>
        public int VisibleItemCount => activeItems.Count;

        /// <summary>
        /// 当前对象池大小
        /// </summary>
        public int CurrentPoolSize
        {
            get
            {
                int total = 0;
                if (itemPools != null)
                {
                    foreach (var pool in itemPools.Values)
                    {
                        total += pool.Count;
                    }
                }
                return total;
            }
        }

        /// <summary>
        /// 获取列表项预制体
        /// </summary>
        public GameObject ItemPrefab => itemPrefab;

        /// <summary>
        /// 根据类型获取预制体
        /// </summary>
        /// <param name="type">预制体类型</param>
        /// <returns>预制体对象</returns>
        public GameObject GetPrefab(int type)
        {
            if (type == 0) return itemPrefab;
            int extraIndex = type - 1;
            if (extraIndex >= 0 && extraIndex < extraPrefabs.Count)
            {
                return extraPrefabs[extraIndex];
            }
            return itemPrefab;
        }

        /// <summary>
        /// 网格布局的单元格大小
        /// </summary>
        public Vector2 CellSize
        {
            get => cellSize;
            set
            {
                cellSize = value;
                if (layoutManager is GridLayoutManager gridLayoutManager)
                {
                    gridLayoutManager.CellSize = value;
                    RefreshLayout();
                }
            }
        }

        /// <summary>
        /// 网格布局的单元格间距
        /// </summary>
        public Vector2 CellSpacing
        {
            get => cellSpacing;
            set
            {
                cellSpacing = value;
                if (layoutManager is GridLayoutManager gridLayoutManager)
                {
                    gridLayoutManager.CellSpacing = value;
                    RefreshLayout();
                }
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 列表项被选中时触发
        /// </summary>
        public event Action<int, IScrollViewItem> OnItemSelected;

        /// <summary>
        /// 列表项被取消选中时触发
        /// </summary>
        public event Action<int, IScrollViewItem> OnItemDeselected;

        /// <summary>
        /// 鼠标进入列表项时触发
        /// </summary>
        public event Action<int, IScrollViewItem> OnItemHoverEnter;

        /// <summary>
        /// 鼠标离开列表项时触发
        /// </summary>
        public event Action<int, IScrollViewItem> OnItemHoverExit;

        /// <summary>
        /// 列表项被点击时触发
        /// </summary>
        public event Action<int, IScrollViewItem> OnItemClicked;

        /// <summary>
        /// 滚动位置改变时触发
        /// </summary>
        public event Action<Vector2> OnScrollPositionChanged;

        /// <summary>
        /// 可见范围改变时触发
        /// </summary>
        public event Action<int, int> OnVisibleRangeChanged;

        /// <summary>
        /// 自定义操作事件
        /// </summary>
        public event Action<string, object> OnCustomAction;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            InitializeComponents();
            InitializeLayoutManager();
            InitializeStateManager();
            InitializeObjectPool();
        }

        private void Start()
        {
            SetupScrollRect();
            RefreshLayout();
        }

        private void Update()
        {
            if (enableVirtualization)
            {
                UpdateVirtualization();
            }
        }

        #endregion

        #region 初始化方法

        private void InitializeComponents()
        {
            scrollRect = GetComponent<ScrollRect>();
            content = scrollRect.content;
            viewport = scrollRect.viewport;

            if (content == null)
            {
                Debug.LogError("ScrollRect content is null!");
                return;
            }
        }

        private void InitializeLayoutManager()
        {
            switch (layoutType)
            {
                case LayoutType.Vertical:
                    layoutManager = new VerticalLayoutManager();
                    break;
                case LayoutType.Horizontal:
                    layoutManager = new HorizontalLayoutManager();
                    break;
                case LayoutType.Grid:
                    var gridLayoutManager = new GridLayoutManager(constraintCount);
                    gridLayoutManager.CellSize = cellSize;
                    gridLayoutManager.CellSpacing = cellSpacing;
                    layoutManager = gridLayoutManager;
                    break;
                default:
                    layoutManager = new VerticalLayoutManager();
                    break;
            }

            layoutManager.Initialize(this, content, viewport);
        }

        private void InitializeStateManager()
        {
            stateManager = new ItemStateManager(this);
            stateManager.EnableHoverState = enableHoverState;
            stateManager.EnableSelectionState = enableSelectionState;
            stateManager.AllowMultipleSelection = allowMultipleSelection;
        }

        private void InitializeObjectPool()
        {
            if (itemPrefab == null)
            {
                Debug.LogError("Item prefab is not assigned!");
                return;
            }

            itemPools = new Dictionary<int, ObjectPool<GameObject>>();

            // 初始化默认对象池 (Type 0)
            itemPools[0] = CreatePoolForType(0, itemPrefab);

            // 初始化额外对象池 (Type 1..)
            for (int i = 0; i < extraPrefabs.Count; i++)
            {
                if (extraPrefabs[i] != null)
                {
                    itemPools[i + 1] = CreatePoolForType(i + 1, extraPrefabs[i]);
                }
            }
        }

        private ObjectPool<GameObject> CreatePoolForType(int type, GameObject prefab)
        {
            return new ObjectPool<GameObject>(
                () => CreatePoolItem(type, prefab),
                item => OnItemReturned(item),
                item => OnItemBorrowed(item),
                poolSize
            );
        }

        private void SetupScrollRect()
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            scrollRect.inertia = enableInertia;
            scrollRect.decelerationRate = decelerationRate;
            scrollRect.elasticity = enableBounce ? 0.1f : 0f;
        }

        #endregion

        #region 公共API方法

        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="data">数据列表</param>
        public void SetData<T>(List<T> data)
        {
            // 清理旧数据和状态，确保视图完全刷新
            ClearAllItems();

            dataSource.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                dataSource.Add(new ScrollViewItemData
                {
                    Index = i,
                    Data = data[i]
                });
            }

            RefreshLayout();
        }

        /// <summary>
        /// 添加数据项
        /// </summary>
        /// <param name="data">数据对象</param>
        public void AddItem<T>(T data)
        {
            dataSource.Add(new ScrollViewItemData
            {
                Index = dataSource.Count,
                Data = data
            });

            RefreshLayout();
        }

        /// <summary>
        /// 插入数据项
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="data">数据对象</param>
        public void InsertItem<T>(int index, T data)
        {
            if (index < 0 || index > dataSource.Count)
                return;

            dataSource.Insert(index, new ScrollViewItemData
            {
                Index = index,
                Data = data
            });

            // 更新后续项的索引
            for (int i = index + 1; i < dataSource.Count; i++)
            {
                dataSource[i].Index = i;
            }

            RefreshLayout();
        }

        /// <summary>
        /// 批量插入数据项
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="items">数据列表</param>
        public void InsertItems<T>(int index, IEnumerable<T> items)
        {
            if (index < 0 || index > dataSource.Count || items == null)
                return;

            int count = 0;
            foreach (var item in items)
            {
                dataSource.Insert(index + count, new ScrollViewItemData
                {
                    Index = index + count,
                    Data = item
                });
                count++;
            }

            if (count > 0)
            {
                // 标记开始批量处理
                isBatchProcessing = true;
                batchInsertIndex = index;
                batchInsertCount = count;

                // 调整 activeItems 的 Key 以匹配新索引
                // 从后往前遍历，避免覆盖
                var keysToMove = activeItems.Keys.Where(k => k >= index).OrderByDescending(k => k).ToList();
                foreach (var oldKey in keysToMove)
                {
                    var item = activeItems[oldKey];
                    activeItems.Remove(oldKey);
                    activeItems.Add(oldKey + count, item);

                    // 更新 Item 组件的索引
                    var scrollItem = item.GetComponent<ScrollViewItemBase>();
                    if (scrollItem != null)
                    {
                        // 获取对应的数据对象
                        // 注意：dataSource 已经包含了插入的项，所以原有的项已经向后移动了 count 位
                        // 例如：原 index=0 (A), 插入 1 个 at 0 -> [New, A]
                        // activeItems[0] (A) -> move to 1. oldKey=0, count=1. target=1.
                        // dataSource[1] is A.
                        if (oldKey + count < dataSource.Count)
                        {
                            var data = dataSource[oldKey + count].Data;
                            scrollItem.SetData(data, oldKey + count);
                        }
                    }
                }

                // 更新后续项的索引
                for (int i = index + count; i < dataSource.Count; i++)
                {
                    dataSource[i].Index = i;
                }

                RefreshLayout();

                // 结束批量处理
                isBatchProcessing = false;
                batchInsertIndex = -1;
                batchInsertCount = 0;
            }
        }

        /// <summary>
        /// 批量移除数据项
        /// </summary>
        /// <param name="index">移除起始位置</param>
        /// <param name="count">移除数量</param>
        public void RemoveItems(int index, int count)
        {
            if (index < 0 || count <= 0 || index + count > dataSource.Count)
                return;

            // 标记开始批量处理
            isBatchProcessing = true;

            // 回收被移除范围内的项
            // 计算收起动画的目标位置（Header 底部）
            Vector2 targetPos = Vector2.zero;
            if (enableAnimation && count > 0)
            {
                targetPos = GetGroupOrigin(index);
            }

            for (int i = 0; i < count; i++)
            {
                int removeIndex = index + i;
                if (activeItems.TryGetValue(removeIndex, out GameObject item))
                {
                    // 只有在启用动画时才播放移除动画
                    if (enableAnimation)
                    {
                        activeItems.Remove(removeIndex);
                        StartCoroutine(AnimateRemove(item, targetPos));
                    }
                    else
                    {
                        ReturnItemToPool(removeIndex);
                    }
                }
            }

            dataSource.RemoveRange(index, count);

            // 调整 activeItems 的 Key 以匹配新索引
            // 从前往后遍历
            var keysToMove = activeItems.Keys.Where(k => k >= index + count).OrderBy(k => k).ToList();
            foreach (var oldKey in keysToMove)
            {
                var item = activeItems[oldKey];
                activeItems.Remove(oldKey);
                activeItems.Add(oldKey - count, item);

                // 更新 Item 组件的索引
                var scrollItem = item.GetComponent<ScrollViewItemBase>();
                if (scrollItem != null)
                {
                    if (oldKey - count >= 0 && oldKey - count < dataSource.Count)
                    {
                        var data = dataSource[oldKey - count].Data;
                        scrollItem.SetData(data, oldKey - count);
                    }
                }
            }

            // 更新后续项的索引
            for (int i = index; i < dataSource.Count; i++)
            {
                dataSource[i].Index = i;
            }

            RefreshLayout();

            // 结束批量处理
            isBatchProcessing = false;
        }

        /// <summary>
        /// 移除数据项
        /// </summary>
        /// <param name="index">移除位置</param>
        public void RemoveItem(int index)
        {
            if (index < 0 || index >= dataSource.Count)
                return;

            dataSource.RemoveAt(index);

            // 更新后续项的索引
            for (int i = index; i < dataSource.Count; i++)
            {
                dataSource[i].Index = i;
            }

            RefreshLayout();
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void ClearData()
        {
            dataSource.Clear();
            ClearAllItems();
            RefreshLayout();
        }

        /// <summary>
        /// 滚动到指定索引
        /// </summary>
        /// <param name="index">目标索引</param>
        /// <param name="animated">是否使用动画</param>
        public void ScrollToIndex(int index, bool animated = true)
        {
            if (index < 0 || index >= dataSource.Count)
                return;

            Vector2 targetPosition = CalculateScrollPosition(index);

            if (animated)
            {
                StartCoroutine(AnimateScrollTo(targetPosition));
            }
            else
            {
                content.anchoredPosition = targetPosition;
            }
        }

        /// <summary>
        /// 计算滑动到指定索引的正确位置
        /// </summary>
        /// <param name="index">目标索引</param>
        /// <returns>滑动位置</returns>
        private Vector2 CalculateScrollPosition(int index)
        {
            Vector2 itemPosition = layoutManager.GetItemPosition(index);
            Vector2 itemSize = layoutManager.GetItemSize(index);

            // 获取视口大小
            RectTransform viewport = scrollRect.viewport;
            Vector2 viewportSize = viewport.rect.size;

            // 获取内容总大小
            Vector2 contentSize = content.sizeDelta;

            Vector2 targetPosition = itemPosition;

            // 垂直滑动处理
            if (scrollRect.vertical)
            {
                // 如果是最后几个项目，确保不会滑动超出底部
                float maxScrollY = Mathf.Max(0, contentSize.y - viewportSize.y);

                // 计算项目底部位置
                float itemBottom = itemPosition.y - itemSize.y;

                // 如果滑动到底部，使用最大滑动位置
                if (index >= dataSource.Count - 1 || itemBottom <= -maxScrollY)
                {
                    targetPosition.y = maxScrollY;
                }
                else
                {
                    // 确保项目完全可见
                    // Unity ScrollRect的content.anchoredPosition.y：正值向下滚动，负值向上滚动
                    // itemPosition.y是负值（从-padding.top开始向下递减）
                    // 所以滚动位置应该是-itemPosition.y（转换为正值）
                    targetPosition.y = Mathf.Clamp(-itemPosition.y, 0, maxScrollY);
                }
            }

            // 水平滑动处理
            if (scrollRect.horizontal)
            {
                // 如果是最后几个项目，确保不会滑动超出右边
                float maxScrollX = Mathf.Max(0, contentSize.x - viewportSize.x);

                // 计算项目右边位置
                float itemRight = itemPosition.x + itemSize.x;

                // 如果滑动到最右边，使用最大滑动位置
                if (index >= dataSource.Count - 1 || itemRight >= maxScrollX)
                {
                    targetPosition.x = maxScrollX;
                }
                else
                {
                    // 确保项目完全可见
                    targetPosition.x = Mathf.Clamp(itemPosition.x, 0, maxScrollX);
                }
            }

            return targetPosition;
        }

        /// <summary>
        /// 滑动到顶部
        /// </summary>
        /// <param name="animated">是否使用动画</param>
        public void ScrollToTop(bool animated = true)
        {
            if (dataSource.Count > 0)
            {
                ScrollToIndex(0, animated);
            }
        }

        /// <summary>
        /// 滑动到底部
        /// </summary>
        /// <param name="animated">是否使用动画</param>
        public void ScrollToBottom(bool animated = true)
        {
            if (dataSource.Count > 0)
            {
                ScrollToIndex(dataSource.Count - 1, animated);
            }
        }

        /// <summary>
        /// 选中指定索引的项
        /// </summary>
        /// <param name="indices">要选中的索引列表</param>
        public void SelectItems(params int[] indices)
        {
            stateManager?.SelectItems(indices);
        }

        /// <summary>
        /// 取消选中指定索引的项
        /// </summary>
        /// <param name="index">要取消选中的索引</param>
        public void DeselectItem(int index)
        {
            stateManager?.DeselectItem(index);
        }

        /// <summary>
        /// 清空所有选中项
        /// </summary>
        public void ClearSelection()
        {
            stateManager?.ClearSelection();
        }

        /// <summary>
        /// 检查指定索引是否被选中
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>是否被选中</returns>
        public bool IsItemSelected(int index)
        {
            return stateManager?.IsItemSelected(index) ?? false;
        }

        public void OnAction(string action, object data)
        {
            OnCustomAction?.Invoke(action, data);
        }

        /// <summary>
        /// 刷新布局
        /// </summary>
        public void RefreshLayout()
        {
            if (layoutManager == null)
                return;

            int requiredCount = dataSource.Count;
            if (requiredCount == 0)
            {
                ClearAllItems();
                return;
            }

            // 计算布局
            layoutManager.CalculateLayout(dataSource, itemSpacing, padding);

            // 更新内容区域大小
            Vector2 contentSize = layoutManager.ContentSize;
            content.sizeDelta = contentSize;

            // 确保对象池足够大
            EnsurePoolCapacity(requiredCount);

            // 强制重置可见索引，确保 UpdateVirtualization 总是触发 UpdateVisibleItems
            firstVisibleIndex = -1;
            lastVisibleIndex = -1;

            if (enableVirtualization)
            {
                UpdateVirtualization();
            }
            else
            {
                CreateAllItems();
            }
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void ClearPool()
        {
            ClearAllItems();
            if (itemPools != null)
            {
                foreach (var pool in itemPools.Values)
                {
                    pool.Clear();
                }
            }
        }

        #endregion

        #region 私有方法

        private GameObject CreatePoolItem(int type, GameObject prefab)
        {
            GameObject item = Instantiate(prefab, content);
            item.SetActive(false);

            // 设置锚点为左上角，确保位置计算正确
            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.pivot = new Vector2(0, 1);     // 轴心点也设为左上角

                // 保持预制体的原始大小
                RectTransform prefabRect = prefab.GetComponent<RectTransform>();
                if (prefabRect != null)
                {
                    itemRect.sizeDelta = prefabRect.sizeDelta;
                }
            }

            // 设置类型
            var scrollItem = item.GetComponent<ScrollViewItemBase>();
            if (scrollItem != null)
            {
                scrollItem.ItemType = type;
            }

            return item;
        }

        private void OnItemReturned(GameObject item)
        {
            // 重置列表项状态
            IScrollViewItem scrollItem = item.GetComponent<IScrollViewItem>();
            if (scrollItem != null)
            {
                // 只重置视觉状态，不清除ItemStateManager中的选中状态
                // 因为选中状态应该与数据索引绑定，而不是与GameObject绑定
                scrollItem.ResetItem();
            }

            item.SetActive(false);
            item.transform.SetParent(content);
        }

        private void OnItemBorrowed(GameObject item)
        {
            item.SetActive(true);
        }

        private void UpdateVirtualization()
        {
            if (layoutManager == null || dataSource.Count == 0)
                return;

            var visibleRange = layoutManager.GetVisibleRange(content.anchoredPosition, preloadDistance);
            int newFirstVisible = visibleRange.firstIndex;
            int newLastVisible = visibleRange.lastIndex;

            if (newFirstVisible != firstVisibleIndex || newLastVisible != lastVisibleIndex)
            {
                UpdateVisibleItems(newFirstVisible, newLastVisible);
                firstVisibleIndex = newFirstVisible;
                lastVisibleIndex = newLastVisible;

                OnVisibleRangeChanged?.Invoke(firstVisibleIndex, lastVisibleIndex);
            }
        }

        private void UpdateVisibleItems(int firstIndex, int lastIndex)
        {
            // 记录旧的位置信息用于动画
            var oldPositions = new Dictionary<int, Vector2>();
            if (enableAnimation)
            {
                foreach (var kvp in activeItems)
                {
                    oldPositions[kvp.Key] = kvp.Value.GetComponent<RectTransform>().anchoredPosition;
                }
            }

            // 回收不再可见的项
            var itemsToRemove = new List<int>();
            foreach (var kvp in activeItems)
            {
                if (kvp.Key < firstIndex || kvp.Key > lastIndex)
                {
                    itemsToRemove.Add(kvp.Key);
                }
            }

            foreach (int index in itemsToRemove)
            {
                ReturnItemToPool(index);
            }

            // 创建新的可见项
            for (int i = firstIndex; i <= lastIndex; i++)
            {
                if (i >= 0 && i < dataSource.Count && !activeItems.ContainsKey(i))
                {
                    CreateItemAtIndex(i);

                    // 新创建的项如果有动画需求
                    if (enableAnimation && isBatchProcessing)
                    {
                        var item = activeItems[i];
                        if (item != null)
                        {
                            // 检查这是否是一个新插入的项（即不在旧位置记录中）
                            if (!oldPositions.ContainsKey(i))
                            {
                                // 检查是否在当前批量插入范围内
                                if (i >= batchInsertIndex && i < batchInsertIndex + batchInsertCount)
                                {
                                    // 从 Header 底部滑出
                                    Vector2 origin = GetGroupOrigin(batchInsertIndex);
                                    Vector2 target = layoutManager.GetItemPosition(i);
                                    StartCoroutine(AnimateInsert(item.GetComponent<RectTransform>(), origin, target));
                                }
                                else
                                {
                                    // 其他新项（可能是预加载出来的），使用默认淡入
                                    // StartCoroutine(AnimateFadeIn(item)); 
                                }
                            }
                        }
                    }
                }
                else if (activeItems.ContainsKey(i))
                {
                    // 已存在的项，更新位置
                    var item = activeItems[i];
                    var rect = item.GetComponent<RectTransform>();
                    Vector2 newPos = layoutManager.GetItemPosition(i);

                    if (enableAnimation)
                    {
                        if (oldPositions.TryGetValue(i, out Vector2 oldPos))
                        {
                            if (Vector2.Distance(oldPos, newPos) > 1f)
                            {
                                // 暂时恢复旧位置，然后动画移动到新位置
                                rect.anchoredPosition = oldPos;
                                StartCoroutine(AnimateItemMove(rect, newPos));
                            }
                            else
                            {
                                rect.anchoredPosition = newPos;
                            }
                        }
                        else
                        {
                            // 理论上不会发生，除非 oldPositions 没记录到
                            rect.anchoredPosition = newPos;
                        }
                    }
                    else
                    {
                        rect.anchoredPosition = newPos;
                    }
                }
            }

            // 保持兄弟顺序与索引一致，避免显示顺序异常
            EnsureStableSiblingOrder(firstIndex, lastIndex);
        }

        private System.Collections.IEnumerator AnimateItemMove(RectTransform itemRect, Vector2 targetPos)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector2 startPos = itemRect.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t);

                if (itemRect != null)
                {
                    itemRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                }
                yield return null;
            }

            if (itemRect != null)
            {
                itemRect.anchoredPosition = targetPos;
            }
        }

        private System.Collections.IEnumerator AnimateInsert(RectTransform itemRect, Vector2 originPos, Vector2 targetPos)
        {
            if (itemRect == null) yield break;

            float duration = 0.3f;
            float elapsed = 0f;

            // 初始状态
            itemRect.anchoredPosition = originPos;

            CanvasGroup canvasGroup = itemRect.GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // 缓动
                t = t * t * (3f - 2f * t);

                if (itemRect != null)
                {
                    itemRect.anchoredPosition = Vector2.Lerp(originPos, targetPos, t);
                    if (canvasGroup != null) canvasGroup.alpha = t;
                }
                yield return null;
            }

            if (itemRect != null)
            {
                itemRect.anchoredPosition = targetPos;
                if (canvasGroup != null) canvasGroup.alpha = 1f;
            }
        }

        private System.Collections.IEnumerator AnimateRemove(GameObject item, Vector2 targetPos)
        {
            if (item == null) yield break;

            RectTransform itemRect = item.GetComponent<RectTransform>();
            if (itemRect == null)
            {
                RecycleItem(item);
                yield break;
            }

            float duration = 0.25f; // 移除稍快一点
            float elapsed = 0f;
            Vector2 startPos = itemRect.anchoredPosition;

            CanvasGroup canvasGroup = item.GetComponent<CanvasGroup>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (item != null)
                {
                    itemRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                    if (canvasGroup != null) canvasGroup.alpha = 1f - t;
                }
                yield return null;
            }

            if (item != null)
            {
                // 恢复状态以便下次使用
                if (canvasGroup != null) canvasGroup.alpha = 1f;

                RecycleItem(item);
            }
        }

        [Header("动画设置")]
        [SerializeField] private bool enableAnimation = true;

        private bool isBatchProcessing = false;
        private int batchInsertIndex = -1;
        private int batchInsertCount = 0;

        private Vector2 GetGroupOrigin(int firstItemIndex)
        {
            if (firstItemIndex <= 0) return Vector2.zero;

            // 获取前一个项（Header）的位置和大小
            // 注意：此时 LayoutManager 已经更新，但我们想要的是Header的位置
            // Header 的索引是 firstItemIndex - 1
            int headerIndex = firstItemIndex - 1;

            // 简单处理：获取Header的底部位置作为起点
            // 这里假设是垂直布局，向下增长
            Vector2 pos = layoutManager.GetItemPosition(headerIndex);
            Vector2 size = layoutManager.GetItemSize(headerIndex);

            if (layoutType == LayoutType.Vertical)
            {
                return pos + new Vector2(0, -size.y);
            }
            else if (layoutType == LayoutType.Horizontal)
            {
                return pos + new Vector2(size.x, 0);
            }

            // 网格或其他布局，暂时返回 Header 位置
            return pos;
        }

        private void CreateItemAtIndex(int index)
        {
            if (index < 0 || index >= dataSource.Count)
                return;

            // 获取项类型
            int itemType = OnGetItemType?.Invoke(index) ?? 0;

            if (!itemPools.TryGetValue(itemType, out var pool))
            {
                // 如果没有找到对应类型的池，使用默认池
                if (!itemPools.TryGetValue(0, out pool))
                    return;
            }

            GameObject item = pool.Borrow();
            if (item == null)
                return;

            // 设置位置和数据
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchoredPosition = layoutManager.GetItemPosition(index);

            // 只有在网格布局或需要强制设置大小时才修改sizeDelta
            // 对于垂直和水平布局，保持预制体的原始大小
            if (layoutType == LayoutType.Grid)
            {
                itemRect.sizeDelta = layoutManager.GetItemSize(index);
            }

            // 绑定数据
            IScrollViewItem scrollItem = item.GetComponent<IScrollViewItem>();
            if (scrollItem != null)
            {
                // 先重置状态，确保 GameObject 重用时从干净状态开始
                scrollItem.ResetItem();

                // 设置新的数据和索引
                scrollItem.SetScrollView(this);
                scrollItem.SetData(dataSource[index].Data, index);

                // 根据状态管理器恢复该索引位置应有的状态
                if (stateManager != null)
                {
                    bool shouldBeSelected = stateManager.IsItemSelected(index);
                    bool shouldBeHovered = stateManager.HoveredIndex == index;

                    if (shouldBeSelected)
                    {
                        scrollItem.OnItemSelected(true);
                    }
                    if (shouldBeHovered)
                    {
                        scrollItem.OnItemHovered(true);
                    }
                }
            }

            activeItems[index] = item;
        }

        private void RecycleItem(GameObject item)
        {
            int itemType = 0;
            var scrollItem = item.GetComponent<ScrollViewItemBase>();
            if (scrollItem != null)
            {
                itemType = scrollItem.ItemType;
            }

            if (itemPools != null && itemPools.TryGetValue(itemType, out var pool))
            {
                pool.Return(item);
            }
            else if (itemPools != null && itemPools.TryGetValue(0, out var defaultPool))
            {
                defaultPool.Return(item);
            }
            else
            {
                Destroy(item);
            }
        }

        private void ReturnItemToPool(int index)
        {
            if (activeItems.TryGetValue(index, out GameObject item))
            {
                activeItems.Remove(index);
                RecycleItem(item);
            }
        }

        private void CreateAllItems()
        {
            ClearAllItems();

            for (int i = 0; i < dataSource.Count; i++)
            {
                CreateItemAtIndex(i);
            }

            // 非虚拟化模式也保持兄弟顺序稳定
            EnsureStableSiblingOrder(0, dataSource.Count - 1);
        }

        private void ClearAllItems()
        {
            foreach (var kvp in activeItems)
            {
                GameObject item = kvp.Value;
                int itemType = 0;
                var scrollItem = item.GetComponent<ScrollViewItemBase>();
                if (scrollItem != null)
                {
                    itemType = scrollItem.ItemType;
                }

                if (itemPools != null && itemPools.TryGetValue(itemType, out var pool))
                {
                    pool.Return(item);
                }
                else if (itemPools != null && itemPools.TryGetValue(0, out var defaultPool))
                {
                    defaultPool.Return(item);
                }
                else
                {
                    Destroy(item);
                }
            }
            activeItems.Clear();

            // 重置可见索引，确保下次虚拟化检查能正常工作
            firstVisibleIndex = -1;
            lastVisibleIndex = -1;

            // 清除选中状态
            stateManager?.ClearSelection();
        }

        private System.Collections.IEnumerator AnimateScrollTo(Vector2 targetPosition)
        {
            Vector2 startPosition = content.anchoredPosition;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t);

                content.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            content.anchoredPosition = targetPosition;
        }

        #endregion

        #region 事件处理

        private void OnScrollValueChanged(Vector2 position)
        {
            OnScrollPositionChanged?.Invoke(position);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 拖拽处理逻辑
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        /// <summary>
        /// 内部方法：处理项点击事件
        /// </summary>
        internal void HandleItemClick(int index, IScrollViewItem item)
        {
            OnItemClicked?.Invoke(index, item);

            // 调用项目的点击方法
            if (item is ScrollViewItemBase itemBase)
            {
                itemBase.OnItemClicked();
            }

            if (enableSelectionState)
            {
                stateManager?.HandleItemClick(index, item);
            }
        }

        /// <summary>
        /// 内部方法：处理项悬停进入事件
        /// </summary>
        internal void HandleItemHoverEnter(int index, IScrollViewItem item)
        {
            if (enableHoverState)
            {
                stateManager?.HandleItemHoverEnter(index, item);
                OnItemHoverEnter?.Invoke(index, item);
            }
        }

        /// <summary>
        /// 内部方法：处理项悬停离开事件
        /// </summary>
        internal void HandleItemHoverExit(int index, IScrollViewItem item)
        {
            if (enableHoverState)
            {
                stateManager?.HandleItemHoverExit(index, item);
                OnItemHoverExit?.Invoke(index, item);
            }
        }

        private void EnsurePoolCapacity(int requiredCount)
        {
            if (itemPools == null)
                return;

            // 期望容量：所需数量 + 小缓冲
            int desired = Mathf.Max(poolSize, requiredCount + 4);

            // 检查是否需要扩容
            bool needResize = false;
            foreach (var pool in itemPools.Values)
            {
                if (pool.MaxSize < desired)
                {
                    needResize = true;
                    break;
                }
            }

            if (needResize)
            {
                // 扩容：清理现有项并重建对象池
                ClearAllItems();

                // 清理旧池
                foreach (var pool in itemPools.Values)
                {
                    pool.Clear();
                }

                poolSize = desired;

                // 重新初始化
                InitializeObjectPool();
            }
        }

        private void EnsureStableSiblingOrder(int firstIndex, int lastIndex)
        {
            if (firstIndex < 0 || lastIndex < firstIndex)
                return;

            int sibling = 0;
            for (int i = firstIndex; i <= lastIndex; i++)
            {
                if (activeItems.TryGetValue(i, out GameObject go))
                {
                    go.transform.SetSiblingIndex(sibling++);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 布局类型枚举
    /// </summary>
    public enum LayoutType
    {
        Vertical,
        Horizontal,
        Grid
    }

    /// <summary>
    /// 滚动视图项数据
    /// </summary>
    [System.Serializable]
    public class ScrollViewItemData
    {
        public int Index;
        public object Data;
    }
}