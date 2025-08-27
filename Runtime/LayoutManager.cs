using System.Collections.Generic;
using UnityEngine;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 布局管理器基类
    /// 负责计算列表项的位置和大小
    /// </summary>
    public abstract class LayoutManager
    {
        #region 保护字段
        
        protected InfiniteScrollView scrollView;
        protected RectTransform content;
        protected RectTransform viewport;
        protected List<ScrollViewItemData> dataSource;
        protected float itemSpacing;
        protected RectOffset padding;
        
        // 缓存的布局信息
        protected List<ItemLayoutInfo> layoutInfos = new List<ItemLayoutInfo>();
        protected Vector2 contentSize;
        protected bool layoutDirty = true;
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 内容区域大小
        /// </summary>
        public Vector2 ContentSize => contentSize;
        
        /// <summary>
        /// 布局是否需要更新
        /// </summary>
        public bool LayoutDirty => layoutDirty;
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化布局管理器
        /// </summary>
        /// <param name="scrollView">滚动视图</param>
        /// <param name="content">内容区域</param>
        /// <param name="viewport">视口区域</param>
        public virtual void Initialize(InfiniteScrollView scrollView, RectTransform content, RectTransform viewport)
        {
            this.scrollView = scrollView;
            this.content = content;
            this.viewport = viewport;
        }
        
        /// <summary>
        /// 计算布局
        /// </summary>
        /// <param name="dataSource">数据源</param>
        /// <param name="itemSpacing">项间距</param>
        /// <param name="padding">内边距</param>
        public virtual void CalculateLayout(List<ScrollViewItemData> dataSource, float itemSpacing, RectOffset padding)
        {
            this.dataSource = dataSource;
            this.itemSpacing = itemSpacing;
            this.padding = padding;
            
            if (dataSource == null || dataSource.Count == 0)
            {
                layoutInfos.Clear();
                contentSize = Vector2.zero;
                UpdateContentSize();
                layoutDirty = false;
                return;
            }
            
            CalculateItemLayouts();
            CalculateContentSize();
            UpdateContentSize();
            layoutDirty = false;
        }
        
        /// <summary>
        /// 获取指定索引项的位置
        /// </summary>
        /// <param name="index">项索引</param>
        /// <returns>项位置</returns>
        public virtual Vector2 GetItemPosition(int index)
        {
            if (index < 0 || index >= layoutInfos.Count)
                return Vector2.zero;
            
            return layoutInfos[index].position;
        }
        
        /// <summary>
        /// 获取指定索引项的大小
        /// </summary>
        /// <param name="index">项索引</param>
        /// <returns>项大小</returns>
        public virtual Vector2 GetItemSize(int index)
        {
            if (index < 0 || index >= layoutInfos.Count)
                return Vector2.zero;
            
            return layoutInfos[index].size;
        }
        
        /// <summary>
        /// 获取可见范围内的项索引
        /// </summary>
        /// <param name="contentPosition">内容位置</param>
        /// <param name="preloadDistance">预加载距离</param>
        /// <returns>可见范围</returns>
        public virtual (int firstIndex, int lastIndex) GetVisibleRange(Vector2 contentPosition, float preloadDistance)
        {
            if (layoutInfos.Count == 0)
                return (-1, -1);
            
            Rect viewportRect = GetViewportRect(contentPosition, preloadDistance);
            
            int firstIndex = -1;
            int lastIndex = -1;
            
            for (int i = 0; i < layoutInfos.Count; i++)
            {
                Rect itemRect = new Rect(layoutInfos[i].position, layoutInfos[i].size);
                
                if (viewportRect.Overlaps(itemRect))
                {
                    if (firstIndex == -1)
                        firstIndex = i;
                    lastIndex = i;
                }
                else if (firstIndex != -1)
                {
                    // 已经找到可见范围，且当前项不可见，可以提前退出
                    break;
                }
            }
            
            return (firstIndex, lastIndex);
        }
        
        /// <summary>
        /// 标记布局为脏状态
        /// </summary>
        public virtual void MarkLayoutDirty()
        {
            layoutDirty = true;
        }
        
        #endregion
        
        #region 抽象方法
        
        /// <summary>
        /// 计算各项的布局信息
        /// </summary>
        protected abstract void CalculateItemLayouts();
        
        /// <summary>
        /// 计算内容区域大小
        /// </summary>
        protected abstract void CalculateContentSize();
        
        /// <summary>
        /// 获取视口矩形（包含预加载区域）
        /// </summary>
        /// <param name="contentPosition">内容位置</param>
        /// <param name="preloadDistance">预加载距离</param>
        /// <returns>视口矩形</returns>
        protected abstract Rect GetViewportRect(Vector2 contentPosition, float preloadDistance);
        
        #endregion
        
        #region 保护方法
        
        /// <summary>
        /// 获取项的首选大小
        /// </summary>
        /// <param name="index">项索引</param>
        /// <returns>首选大小</returns>
        protected virtual Vector2 GetItemPreferredSize(int index)
        {
            // 尝试从itemPrefab获取首选大小
            if (scrollView != null && scrollView.ItemPrefab != null)
            {
                // 首先尝试从预制体的RectTransform获取原始大小
                var prefabRect = scrollView.ItemPrefab.GetComponent<RectTransform>();
                if (prefabRect != null && prefabRect.sizeDelta != Vector2.zero)
                {
                    return prefabRect.sizeDelta;
                }
                
                // 如果预制体大小为零，再尝试从IScrollViewItem接口获取
                var itemComponent = scrollView.ItemPrefab.GetComponent<IScrollViewItem>();
                if (itemComponent != null)
                {
                    Vector2 preferredSize = itemComponent.GetPreferredSize();
                    if (preferredSize != Vector2.zero)
                    {
                        return preferredSize;
                    }
                }
            }
            
            // 默认返回固定大小，子类可以重写以支持动态大小
            return new Vector2(100, 50);
        }
        
        /// <summary>
        /// 更新内容区域大小
        /// </summary>
        protected virtual void UpdateContentSize()
        {
            if (content != null)
            {
                content.sizeDelta = contentSize;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 垂直布局管理器
    /// </summary>
    public class VerticalLayoutManager : LayoutManager
    {
        protected override void CalculateItemLayouts()
        {
            layoutInfos.Clear();
            
            float currentY = -padding.top;
            
            for (int i = 0; i < dataSource.Count; i++)
            {
                Vector2 itemSize = GetItemPreferredSize(i);
                // 由于锚点设置为左上角，位置就是当前Y坐标
                Vector2 position = new Vector2(padding.left, currentY);
                
                layoutInfos.Add(new ItemLayoutInfo
                {
                    index = i,
                    position = position,
                    size = itemSize
                });
                
                currentY -= itemSize.y + itemSpacing;
            }
        }
        
        protected override void CalculateContentSize()
        {
            if (layoutInfos.Count == 0)
            {
                contentSize = Vector2.zero;
                return;
            }
            
            // 计算实际需要的宽度，而不是强制使用viewport宽度
            float maxItemWidth = 0;
            float height = padding.top + padding.bottom;
            
            for (int i = 0; i < layoutInfos.Count; i++)
            {
                // 找到最宽的项目
                maxItemWidth = Mathf.Max(maxItemWidth, layoutInfos[i].size.x);
                
                height += layoutInfos[i].size.y;
                if (i < layoutInfos.Count - 1)
                    height += itemSpacing;
            }
            
            // 总宽度 = 左边距 + 最大项目宽度 + 右边距
            float totalWidth = padding.left + maxItemWidth + padding.right;
            
            // 使用实际计算的宽度，不强制拉伸到viewport宽度
            contentSize = new Vector2(totalWidth, height);
        }
        
        protected override Rect GetViewportRect(Vector2 contentPosition, float preloadDistance)
        {
            Vector2 viewportSize = viewport.rect.size;
            Vector2 position = new Vector2(
                -contentPosition.x - preloadDistance,
                -contentPosition.y - viewportSize.y - preloadDistance
            );
            Vector2 size = new Vector2(
                viewportSize.x + preloadDistance * 2,
                viewportSize.y + preloadDistance * 2
            );
            
            return new Rect(position, size);
        }
    }
    
    /// <summary>
    /// 水平布局管理器
    /// </summary>
    public class HorizontalLayoutManager : LayoutManager
    {
        protected override void CalculateItemLayouts()
        {
            layoutInfos.Clear();
            
            float currentX = padding.left;
            
            for (int i = 0; i < dataSource.Count; i++)
            {
                Vector2 itemSize = GetItemPreferredSize(i);
                Vector2 position = new Vector2(currentX, -padding.top);
                
                layoutInfos.Add(new ItemLayoutInfo
                {
                    index = i,
                    position = position,
                    size = itemSize
                });
                
                currentX += itemSize.x + itemSpacing;
            }
        }
        
        protected override void CalculateContentSize()
        {
            if (layoutInfos.Count == 0)
            {
                contentSize = Vector2.zero;
                return;
            }
            
            float width = padding.left + padding.right;
            // 计算实际需要的高度，而不是强制使用viewport高度
            float maxHeight = 0;
            
            for (int i = 0; i < layoutInfos.Count; i++)
            {
                width += layoutInfos[i].size.x;
                if (i < layoutInfos.Count - 1)
                    width += itemSpacing;
                
                // 找到最高的项目
                maxHeight = Mathf.Max(maxHeight, layoutInfos[i].size.y);
            }
            
            // 总高度 = 上边距 + 最大项目高度 + 下边距
            float totalHeight = padding.top + maxHeight + padding.bottom;
            
            // 使用实际计算的高度，不强制拉伸到viewport高度
            contentSize = new Vector2(width, totalHeight);
        }
        
        protected override Rect GetViewportRect(Vector2 contentPosition, float preloadDistance)
        {
            Vector2 viewportSize = viewport.rect.size;
            Vector2 position = new Vector2(
                -contentPosition.x - preloadDistance,
                -contentPosition.y - viewportSize.y - preloadDistance
            );
            Vector2 size = new Vector2(
                viewportSize.x + preloadDistance * 2,
                viewportSize.y + preloadDistance * 2
            );
            
            return new Rect(position, size);
        }
    }
    
    /// <summary>
    /// 网格布局管理器
    /// </summary>
    public class GridLayoutManager : LayoutManager
    {
        #region 私有字段
        
        private int constraintCount;
        private Vector2 cellSize = new Vector2(100, 100);
        private Vector2 cellSpacing = new Vector2(5, 5);
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 约束数量（列数或行数）
        /// </summary>
        public int ConstraintCount
        {
            get => constraintCount;
            set
            {
                constraintCount = Mathf.Max(1, value);
                MarkLayoutDirty();
            }
        }
        
        /// <summary>
        /// 网格单元格大小
        /// </summary>
        public Vector2 CellSize
        {
            get => cellSize;
            set
            {
                cellSize = value;
                MarkLayoutDirty();
            }
        }
        
        /// <summary>
        /// 网格单元格间距
        /// </summary>
        public Vector2 CellSpacing
        {
            get => cellSpacing;
            set
            {
                cellSpacing = value;
                MarkLayoutDirty();
            }
        }
        
        #endregion
        
        #region 构造函数
        
        public GridLayoutManager(int constraintCount = 1)
        {
            this.constraintCount = Mathf.Max(1, constraintCount);
        }
        
        #endregion
        
        protected override void CalculateItemLayouts()
        {
            layoutInfos.Clear();
            
            int columns = constraintCount;
            int rows = Mathf.CeilToInt((float)dataSource.Count / columns);
            
            for (int i = 0; i < dataSource.Count; i++)
            {
                int row = i / columns;
                int col = i % columns;
                
                Vector2 position = new Vector2(
                    padding.left + col * (cellSize.x + cellSpacing.x),
                    -padding.top - row * (cellSize.y + cellSpacing.y)
                );
                
                layoutInfos.Add(new ItemLayoutInfo
                {
                    index = i,
                    position = position,
                    size = cellSize
                });
            }
        }
        
        protected override void CalculateContentSize()
        {
            if (layoutInfos.Count == 0)
            {
                contentSize = Vector2.zero;
                return;
            }
            
            int columns = constraintCount;
            int rows = Mathf.CeilToInt((float)dataSource.Count / columns);
            
            float width = padding.left + padding.right + columns * cellSize.x + (columns - 1) * cellSpacing.x;
            float height = padding.top + padding.bottom + rows * cellSize.y + (rows - 1) * cellSpacing.y;
            
            contentSize = new Vector2(width, height);
        }
        
        protected override Rect GetViewportRect(Vector2 contentPosition, float preloadDistance)
        {
            Vector2 viewportSize = viewport.rect.size;
            Vector2 position = new Vector2(
                -contentPosition.x - preloadDistance,
                -contentPosition.y - viewportSize.y - preloadDistance
            );
            Vector2 size = new Vector2(
                viewportSize.x + preloadDistance * 2,
                viewportSize.y + preloadDistance * 2
            );
            
            return new Rect(position, size);
        }
        
        protected override Vector2 GetItemPreferredSize(int index)
        {
            return cellSize;
        }
    }
    
    /// <summary>
    /// 项布局信息
    /// </summary>
    [System.Serializable]
    public class ItemLayoutInfo
    {
        public int index;
        public Vector2 position;
        public Vector2 size;
        public Rect bounds => new Rect(position, size);
    }
}