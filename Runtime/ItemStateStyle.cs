using UnityEngine;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 列表项状态样式配置
    /// </summary>
    [CreateAssetMenu(fileName = "ItemStateStyle", menuName = "UGC/InfiniteScrollView/Item State Style")]
    public class ItemStateStyle : ScriptableObject
    {
        [Header("正常状态")]
        public ItemVisualState normalState = new ItemVisualState();
        
        [Header("悬停状态")]
        public ItemVisualState hoverState = new ItemVisualState();
        
        [Header("选中状态")]
        public ItemVisualState selectedState = new ItemVisualState();
    }
}