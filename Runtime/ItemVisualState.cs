using UnityEngine;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 列表项视觉状态
    /// </summary>
    [System.Serializable]
    public class ItemVisualState
    {
        [Header("背景颜色")]
        public bool useBackgroundColor = true;
        public Color backgroundColor = Color.white;
        
        [Header("透明度")]
        public bool useAlpha = true;
        [Range(0f, 1f)]
        public float alpha = 1f;
        
        [Header("缩放")]
        public bool useScale = false;
        [Range(0.1f, 2f)]
        public float scale = 1f;
        
        [Header("动画")]
        public bool useAnimation = true;
        public float animationDuration = 0.2f;
        public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}