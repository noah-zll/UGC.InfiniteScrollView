using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 列表项状态管理器
    /// 负责管理列表项的选中状态和悬停状态
    /// </summary>
    public class ItemStateManager
    {
        #region 私有字段
        
        private readonly InfiniteScrollView scrollView;
        private readonly HashSet<int> selectedIndices = new HashSet<int>();
        private int hoveredIndex = -1;
        
        private bool enableHoverState = true;
        private bool enableSelectionState = true;
        private bool allowMultipleSelection = false;
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 当前选中的索引列表
        /// </summary>
        public List<int> SelectedIndices => new List<int>(selectedIndices);
        
        /// <summary>
        /// 当前悬停的索引
        /// </summary>
        public int HoveredIndex => hoveredIndex;
        
        /// <summary>
        /// 是否启用悬停状态
        /// </summary>
        public bool EnableHoverState
        {
            get => enableHoverState;
            set
            {
                enableHoverState = value;
                if (!value && hoveredIndex != -1)
                {
                    ClearHover();
                }
            }
        }
        
        /// <summary>
        /// 是否启用选中状态
        /// </summary>
        public bool EnableSelectionState
        {
            get => enableSelectionState;
            set
            {
                enableSelectionState = value;
                if (!value)
                {
                    ClearSelection();
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
                if (!value && selectedIndices.Count > 1)
                {
                    // 只保留第一个选中项
                    var firstSelected = selectedIndices.Count > 0 ? new List<int>(selectedIndices)[0] : -1;
                    ClearSelection();
                    if (firstSelected != -1)
                    {
                        SelectItem(firstSelected);
                    }
                }
            }
        }
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 选中状态改变时触发
        /// </summary>
        public event Action<int, bool> OnSelectionChanged;
        
        /// <summary>
        /// 悬停状态改变时触发
        /// </summary>
        public event Action<int, bool> OnHoverChanged;
        
        #endregion
        
        #region 构造函数
        
        public ItemStateManager(InfiniteScrollView scrollView)
        {
            this.scrollView = scrollView ?? throw new ArgumentNullException(nameof(scrollView));
        }
        
        #endregion
        
        #region 选中状态管理
        
        /// <summary>
        /// 选中指定索引的项
        /// </summary>
        /// <param name="indices">要选中的索引数组</param>
        public void SelectItems(params int[] indices)
        {
            if (!enableSelectionState || indices == null)
                return;
            
            if (!allowMultipleSelection)
            {
                ClearSelection();
                if (indices.Length > 0)
                {
                    SelectItem(indices[0]);
                }
                return;
            }
            
            foreach (int index in indices)
            {
                SelectItem(index);
            }
        }
        
        /// <summary>
        /// 选中单个项
        /// </summary>
        /// <param name="index">项索引</param>
        public void SelectItem(int index)
        {
            if (!enableSelectionState || index < 0 || index >= scrollView.DataCount)
                return;
            
            if (selectedIndices.Contains(index))
                return;
            
            if (!allowMultipleSelection && selectedIndices.Count > 0)
            {
                ClearSelection();
            }
            
            selectedIndices.Add(index);
            NotifyItemSelectionChanged(index, true);
            OnSelectionChanged?.Invoke(index, true);
        }
        
        /// <summary>
        /// 取消选中指定项
        /// </summary>
        /// <param name="index">项索引</param>
        public void DeselectItem(int index)
        {
            if (!enableSelectionState || !selectedIndices.Contains(index))
                return;
            
            selectedIndices.Remove(index);
            NotifyItemSelectionChanged(index, false);
            OnSelectionChanged?.Invoke(index, false);
        }
        
        /// <summary>
        /// 切换项的选中状态
        /// </summary>
        /// <param name="index">项索引</param>
        public void ToggleSelection(int index)
        {
            if (IsItemSelected(index))
            {
                DeselectItem(index);
            }
            else
            {
                SelectItem(index);
            }
        }
        
        /// <summary>
        /// 清空所有选中项
        /// </summary>
        public void ClearSelection()
        {
            if (selectedIndices.Count == 0)
                return;
            
            var indicesToClear = new List<int>(selectedIndices);
            selectedIndices.Clear();
            
            foreach (int index in indicesToClear)
            {
                NotifyItemSelectionChanged(index, false);
                OnSelectionChanged?.Invoke(index, false);
            }
        }
        
        /// <summary>
        /// 检查指定索引是否被选中
        /// </summary>
        /// <param name="index">项索引</param>
        /// <returns>是否被选中</returns>
        public bool IsItemSelected(int index)
        {
            return selectedIndices.Contains(index);
        }
        
        #endregion
        
        #region 悬停状态管理
        
        /// <summary>
        /// 设置悬停项
        /// </summary>
        /// <param name="index">项索引</param>
        public void SetHoveredItem(int index)
        {
            if (!enableHoverState)
                return;
            
            if (hoveredIndex == index)
                return;
            
            // 清除之前的悬停状态
            if (hoveredIndex != -1)
            {
                NotifyItemHoverChanged(hoveredIndex, false);
                OnHoverChanged?.Invoke(hoveredIndex, false);
            }
            
            // 设置新的悬停状态
            hoveredIndex = index;
            if (hoveredIndex != -1 && hoveredIndex < scrollView.DataCount)
            {
                NotifyItemHoverChanged(hoveredIndex, true);
                OnHoverChanged?.Invoke(hoveredIndex, true);
            }
        }
        
        /// <summary>
        /// 清除悬停状态
        /// </summary>
        public void ClearHover()
        {
            SetHoveredItem(-1);
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 处理项点击事件
        /// </summary>
        /// <param name="index">项索引</param>
        /// <param name="item">项实例</param>
        public void HandleItemClick(int index, IScrollViewItem item)
        {
            if (!enableSelectionState)
                return;
            
            bool isCtrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            
            if (allowMultipleSelection && isCtrlPressed)
            {
                // Ctrl+点击：切换选中状态
                ToggleSelection(index);
            }
            else if (allowMultipleSelection && isShiftPressed && selectedIndices.Count > 0)
            {
                // Shift+点击：范围选择
                SelectRange(GetLastSelectedIndex(), index);
            }
            else
            {
                // 普通点击：单选
                if (!IsItemSelected(index) || selectedIndices.Count > 1)
                {
                    ClearSelection();
                    SelectItem(index);
                }
            }
        }
        
        /// <summary>
        /// 处理项悬停进入事件
        /// </summary>
        /// <param name="index">项索引</param>
        /// <param name="item">项实例</param>
        public void HandleItemHoverEnter(int index, IScrollViewItem item)
        {
            SetHoveredItem(index);
        }
        
        /// <summary>
        /// 处理项悬停离开事件
        /// </summary>
        /// <param name="index">项索引</param>
        /// <param name="item">项实例</param>
        public void HandleItemHoverExit(int index, IScrollViewItem item)
        {
            if (hoveredIndex == index)
            {
                ClearHover();
            }
        }
        
        #endregion
        
        #region 键盘输入处理
        
        /// <summary>
        /// 处理键盘输入
        /// </summary>
        public void HandleKeyboardInput()
        {
            if (!enableSelectionState)
                return;
            
            if (Input.GetKeyDown(KeyCode.A) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                // Ctrl+A：全选
                SelectAll();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Esc：清空选择
                ClearSelection();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // 上箭头：选择上一项
                NavigateSelection(-1);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // 下箭头：选择下一项
                NavigateSelection(1);
            }
        }
        
        /// <summary>
        /// 全选
        /// </summary>
        public void SelectAll()
        {
            if (!enableSelectionState || !allowMultipleSelection)
                return;
            
            for (int i = 0; i < scrollView.DataCount; i++)
            {
                SelectItem(i);
            }
        }
        
        /// <summary>
        /// 导航选择（键盘导航）
        /// </summary>
        /// <param name="direction">方向（-1向上，1向下）</param>
        private void NavigateSelection(int direction)
        {
            int currentIndex = GetLastSelectedIndex();
            int newIndex = currentIndex + direction;
            
            if (newIndex >= 0 && newIndex < scrollView.DataCount)
            {
                bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                
                if (allowMultipleSelection && isShiftPressed)
                {
                    // Shift+方向键：扩展选择
                    SelectItem(newIndex);
                }
                else
                {
                    // 普通方向键：单选
                    ClearSelection();
                    SelectItem(newIndex);
                }
                
                // 滚动到可见区域
                scrollView.ScrollToIndex(newIndex, true);
            }
        }
        
        #endregion
        
        #region 私有方法
        
        private void NotifyItemSelectionChanged(int index, bool selected)
        {
            if (scrollView != null && scrollView.activeItems.TryGetValue(index, out GameObject itemObject))
            {
                var item = itemObject.GetComponent<IScrollViewItem>();
                item?.OnItemSelected(selected);
            }
        }
        
        private void NotifyItemHoverChanged(int index, bool hovered)
        {
            if (scrollView != null && scrollView.activeItems.TryGetValue(index, out GameObject itemObject))
            {
                var item = itemObject.GetComponent<IScrollViewItem>();
                item?.OnItemHovered(hovered);
            }
        }
        
        private int GetLastSelectedIndex()
        {
            if (selectedIndices.Count == 0)
                return -1;
            
            int lastIndex = -1;
            foreach (int index in selectedIndices)
            {
                if (index > lastIndex)
                    lastIndex = index;
            }
            return lastIndex;
        }
        
        private void SelectRange(int startIndex, int endIndex)
        {
            if (startIndex == -1)
            {
                SelectItem(endIndex);
                return;
            }
            
            int min = Mathf.Min(startIndex, endIndex);
            int max = Mathf.Max(startIndex, endIndex);
            
            for (int i = min; i <= max; i++)
            {
                SelectItem(i);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 输入处理器
    /// 处理滚动视图的输入事件
    /// </summary>
    public class InputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
    {
        #region 私有字段
        
        private InfiniteScrollView scrollView;
        private ItemStateManager stateManager;
        
        private bool isDragging;
        private Vector2 lastPointerPosition;
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 初始化输入处理器
        /// </summary>
        /// <param name="scrollView">滚动视图</param>
        /// <param name="stateManager">状态管理器</param>
        public void Initialize(InfiniteScrollView scrollView, ItemStateManager stateManager)
        {
            this.scrollView = scrollView;
            this.stateManager = stateManager;
        }
        
        #endregion
        
        #region Unity生命周期
        
        private void Update()
        {
            if (stateManager != null)
            {
                stateManager.HandleKeyboardInput();
            }
        }
        
        #endregion
        
        #region 事件处理
        
        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = false;
            lastPointerPosition = eventData.position;
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDragging)
            {
                // 这是一个点击事件，不是拖拽
                HandleClick(eventData);
            }
            isDragging = false;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            float dragDistance = Vector2.Distance(eventData.position, lastPointerPosition);
            if (dragDistance > 10f) // 拖拽阈值
            {
                isDragging = true;
            }
        }
        
        public void OnScroll(PointerEventData eventData)
        {
            // 滚轮事件处理
        }
        
        #endregion
        
        #region 私有方法
        
        private void HandleClick(PointerEventData eventData)
        {
            // 检查点击是否在空白区域
            if (IsClickInEmptyArea(eventData))
            {
                // 点击空白区域，清空选择
                stateManager?.ClearSelection();
            }
        }
        
        private bool IsClickInEmptyArea(PointerEventData eventData)
        {
            // 检查点击位置是否在任何列表项上
            // 这里需要根据具体实现来判断
            return true; // 简化实现
        }
        
        #endregion
    }
}