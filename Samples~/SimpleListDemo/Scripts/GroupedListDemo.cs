using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UGC.InfiniteScrollView;

namespace UGC.InfiniteScrollView.Samples
{
    /// <summary>
    /// 分组列表演示
    /// </summary>
    public class GroupedListDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InfiniteScrollView scrollView;
        [SerializeField] private Button expandAllButton;
        [SerializeField] private Button collapseAllButton;
        [SerializeField] private Button refreshButton;

        [Header("Settings")]
        [SerializeField] private int groupCount = 5;
        [SerializeField] private int minItemsPerGroup = 3;
        [SerializeField] private int maxItemsPerGroup = 10;

        private GroupedDataHelper<SimpleItemData> groupHelper;

        private void Start()
        {
            InitializeDemo();
        }

        private void InitializeDemo()
        {
            if (scrollView == null)
            {
                Debug.LogError("ScrollView reference is missing!");
                return;
            }

            // 1. 初始化 Helper
            groupHelper = new GroupedDataHelper<SimpleItemData>(scrollView);

            // 2. 绑定按钮事件
            if (expandAllButton != null) expandAllButton.onClick.AddListener(() => groupHelper.ExpandAll());
            if (collapseAllButton != null) collapseAllButton.onClick.AddListener(() => groupHelper.CollapseAll());
            if (refreshButton != null) refreshButton.onClick.AddListener(GenerateData);

            // 3. 绑定 ScrollView 点击事件
            scrollView.OnItemClicked += HandleItemClick;

            // 4. 生成初始数据
            GenerateData();
        }

        private void HandleItemClick(int index, IScrollViewItem item)
        {
            // 检查点击的是否是分组标题
            if (groupHelper.IsHeader(index))
            {
                var displayData = groupHelper.GetDisplayData(index);
                if (displayData != null)
                {
                    // 切换分组展开/收起状态
                    groupHelper.ToggleGroup(displayData.GroupIndex);
                    
                    // 可选：播放点击音效或动画
                    Debug.Log($"Toggled group {displayData.GroupIndex}");
                }
            }
            else
            {
                // 处理普通项点击
                var displayData = groupHelper.GetDisplayData(index);
                if (displayData != null && displayData.Data is SimpleItemData itemData)
                {
                    Debug.Log($"Clicked item: {itemData.Title}");
                }
            }
        }

        private void GenerateData()
        {
            var groups = new List<GroupedDataHelper<SimpleItemData>.GroupData>();

            for (int i = 0; i < groupCount; i++)
            {
                var group = new GroupedDataHelper<SimpleItemData>.GroupData
                {
                    Title = $"Group {i + 1}",
                    Expanded = true,
                    Items = new List<SimpleItemData>()
                };

                int itemCount = Random.Range(minItemsPerGroup, maxItemsPerGroup + 1);
                for (int j = 0; j < itemCount; j++)
                {
                    group.Items.Add(new SimpleItemData
                    {
                        Id = $"{i}-{j}",
                        Title = $"Item {i}-{j}",
                        Description = $"Description for item {j} in group {i}",
                        IconIndex = Random.Range(0, 3)
                    });
                }

                groups.Add(group);
            }

            groupHelper.SetGroups(groups);
        }

        private void OnDestroy()
        {
            if (scrollView != null)
            {
                scrollView.OnItemClicked -= HandleItemClick;
            }
        }
    }
}
