using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UGC.InfiniteScrollView;

namespace UGC.InfiniteScrollView.Samples
{
    /// <summary>
    /// 分组标题列表项
    /// </summary>
    public class SimpleGroupHeader : ScrollViewItemBase
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image expandIcon;

        [Header("Settings")]
        [SerializeField] private Sprite expandSprite;
        [SerializeField] private Sprite collapseSprite;
        [SerializeField] private Color expandedColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color collapsedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private GroupedDataHelper<SimpleItemData>.GroupData groupData;

        protected override void OnDataChanged(object data, int index)
        {
            // 注意：这里的数据类型是 GroupedDataHelper<T>.GroupData
            var displayData = data as GroupedDataHelper<SimpleItemData>.DisplayData;
            groupData = displayData.Data as GroupedDataHelper<SimpleItemData>.GroupData;

            if (groupData != null)
            {
                if (titleText != null)
                    titleText.text = groupData.Title;

                if (countText != null)
                    countText.text = $"{groupData.Items.Count} Items";

                UpdateExpandState();
            }
        }

        public override void ResetItem()
        {
            base.ResetItem();
            groupData = null;
        }

        private void UpdateExpandState()
        {
            if (groupData == null) return;

            if (expandIcon != null)
            {
                expandIcon.transform.localRotation = Quaternion.Euler(0, 0, groupData.Expanded ? 0 : -90);
                if (groupData.Expanded && expandSprite != null) expandIcon.sprite = expandSprite;
                else if (!groupData.Expanded && collapseSprite != null) expandIcon.sprite = collapseSprite;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = groupData.Expanded ? expandedColor : collapsedColor;
            }
        }
    }
}
