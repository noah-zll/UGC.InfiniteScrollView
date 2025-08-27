using UnityEngine;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 滚动视图列表项接口
    /// 所有列表项组件都应该实现此接口，以支持虚拟化渲染、数据绑定和交互状态管理。
    /// </summary>
    /// <remarks>
    /// 实现此接口的组件将能够：
    /// - 接收和显示数据
    /// - 响应用户交互（点击、悬停、选中）
    /// - 参与对象池的复用机制
    /// - 支持动态尺寸计算
    /// - 处理状态变化和动画效果
    /// </remarks>
    /// <example>
    /// 基本实现示例：
    /// <code>
    /// public class MyListItem : MonoBehaviour, IScrollViewItem
    /// {
    ///     public int Index { get; private set; }
    ///     public bool IsSelected { get; private set; }
    ///     public bool IsHovered { get; private set; }
    ///     public RectTransform RectTransform => transform as RectTransform;
    ///     public InfiniteScrollView ScrollView { get; private set; }
    ///     
    ///     public void SetData(object data, int index)
    ///     {
    ///         Index = index;
    ///         // 更新UI显示
    ///     }
    ///     
    ///     // 实现其他接口方法...
    /// }
    /// </code>
    /// </example>
    public interface IScrollViewItem
    {
        /// <summary>
        /// 列表项索引
        /// </summary>
        int Index { get; }
        
        /// <summary>
        /// 是否被选中
        /// </summary>
        bool IsSelected { get; }
        
        /// <summary>
        /// 是否处于悬停状态
        /// </summary>
        bool IsHovered { get; }
        
        /// <summary>
        /// 列表项的RectTransform组件
        /// </summary>
        RectTransform RectTransform { get; }
        
        /// <summary>
        /// 所属的滚动视图
        /// </summary>
        InfiniteScrollView ScrollView { get; }
        
        /// <summary>
        /// 设置数据和索引
        /// </summary>
        /// <param name="data">数据对象</param>
        /// <param name="index">列表项索引</param>
        void SetData(object data, int index);
        
        /// <summary>
        /// 设置所属的滚动视图
        /// </summary>
        /// <param name="scrollView">滚动视图实例</param>
        void SetScrollView(InfiniteScrollView scrollView);
        
        /// <summary>
        /// 当列表项被选中时调用
        /// </summary>
        /// <param name="selected">是否选中</param>
        void OnItemSelected(bool selected);
        
        /// <summary>
        /// 当列表项悬停状态改变时调用
        /// </summary>
        /// <param name="hovered">是否悬停</param>
        void OnItemHovered(bool hovered);
        
        /// <summary>
        /// 当列表项被点击时调用
        /// </summary>
        void OnItemClicked();
        
        /// <summary>
        /// 刷新列表项显示
        /// </summary>
        void RefreshDisplay();
        
        /// <summary>
        /// 获取列表项的首选大小
        /// </summary>
        /// <returns>首选大小</returns>
        Vector2 GetPreferredSize();
        
        /// <summary>
        /// 重置列表项状态（回收到对象池时调用）
        /// </summary>
        void ResetItem();
    }
}