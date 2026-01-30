using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 分组数据辅助类
    /// 用于管理分组数据的展开/收起，并生成InfiniteScrollView所需的扁平列表
    /// </summary>
    /// <typeparam name="T">列表项数据类型</typeparam>
    public class GroupedDataHelper<T>
    {
        #region 数据定义

        public class GroupData
        {
            public string Title;
            public bool Expanded = true;
            public List<T> Items = new List<T>();
            public object UserData;
        }

        public class DisplayData
        {
            public bool IsHeader;
            public int GroupIndex;
            public int ItemIndex;
            public object Data; // 指向 GroupData 或 T
        }

        #endregion

        private InfiniteScrollView scrollView;
        private List<GroupData> groups = new List<GroupData>();
        private List<DisplayData> flattenedList = new List<DisplayData>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="scrollView">关联的InfiniteScrollView组件</param>
        public GroupedDataHelper(InfiniteScrollView scrollView)
        {
            this.scrollView = scrollView;
            // 注册类型回调：Header返回1，普通Item返回0
            this.scrollView.OnGetItemType = GetItemType;
        }

        /// <summary>
        /// 设置分组数据
        /// </summary>
        /// <param name="newGroups">分组列表</param>
        public void SetGroups(List<GroupData> newGroups)
        {
            this.groups = newGroups ?? new List<GroupData>();
            Refresh();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            FlattenData();
            scrollView.SetData(flattenedList);
        }

        /// <summary>
        /// 切换分组展开/收起状态
        /// </summary>
        /// <param name="groupIndex">分组索引</param>
        public void ToggleGroup(int groupIndex)
        {
            if (groupIndex >= 0 && groupIndex < groups.Count)
            {
                var group = groups[groupIndex];
                bool isExpanded = !group.Expanded;
                group.Expanded = isExpanded;

                // 查找该分组在扁平列表中的起始位置（Header位置）
                int headerIndex = -1;
                for (int i = 0; i < flattenedList.Count; i++)
                {
                    if (flattenedList[i].IsHeader && flattenedList[i].GroupIndex == groupIndex)
                    {
                        headerIndex = i;
                        break;
                    }
                }

                if (headerIndex == -1)
                {
                    // 如果找不到Header，说明数据不一致，完全刷新
                    Refresh();
                    return;
                }

                if (isExpanded)
                {
                    // 展开：在Header后面插入数据
                    if (group.Items != null && group.Items.Count > 0)
                    {
                        var newItems = new List<DisplayData>();
                        for (int j = 0; j < group.Items.Count; j++)
                        {
                            newItems.Add(new DisplayData
                            {
                                IsHeader = false,
                                GroupIndex = groupIndex,
                                ItemIndex = j,
                                Data = group.Items[j]
                            });
                        }

                        // 更新本地扁平数据
                        flattenedList.InsertRange(headerIndex + 1, newItems);

                        // 调用ScrollView的批量插入接口
                        scrollView.InsertItems(headerIndex + 1, newItems);
                    }
                }
                else
                {
                    // 收起：从Header后面移除数据
                    if (group.Items != null && group.Items.Count > 0)
                    {
                        int removeCount = group.Items.Count;

                        // 验证要移除的数量是否超出范围
                        if (headerIndex + 1 + removeCount <= flattenedList.Count)
                        {
                            // 更新本地扁平数据
                            flattenedList.RemoveRange(headerIndex + 1, removeCount);

                            // 调用ScrollView的批量移除接口
                            scrollView.RemoveItems(headerIndex + 1, removeCount);
                        }
                        else
                        {
                            // 数据不一致，回退到全量刷新
                            Refresh();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 展开所有分组
        /// </summary>
        public void ExpandAll()
        {
            foreach (var g in groups) g.Expanded = true;
            Refresh();
        }

        /// <summary>
        /// 收起所有分组
        /// </summary>
        public void CollapseAll()
        {
            foreach (var g in groups) g.Expanded = false;
            Refresh();
        }

        /// <summary>
        /// 检查显示索引是否为Header
        /// </summary>
        public bool IsHeader(int displayIndex)
        {
            if (displayIndex < 0 || displayIndex >= flattenedList.Count) return false;
            return flattenedList[displayIndex].IsHeader;
        }

        /// <summary>
        /// 获取显示数据
        /// </summary>
        public DisplayData GetDisplayData(int index)
        {
            if (index < 0 || index >= flattenedList.Count) return null;
            return flattenedList[index];
        }

        private void FlattenData()
        {
            flattenedList.Clear();
            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];

                // Add Header
                flattenedList.Add(new DisplayData
                {
                    IsHeader = true,
                    GroupIndex = i,
                    ItemIndex = -1,
                    Data = group
                });

                // Add Items if expanded
                if (group.Expanded && group.Items != null)
                {
                    for (int j = 0; j < group.Items.Count; j++)
                    {
                        flattenedList.Add(new DisplayData
                        {
                            IsHeader = false,
                            GroupIndex = i,
                            ItemIndex = j,
                            Data = group.Items[j]
                        });
                    }
                }
            }
        }

        private int GetItemType(int index)
        {
            if (index < 0 || index >= flattenedList.Count) return 0;
            // Header使用ExtraPrefabs[0] (Type=1), Item使用ItemPrefab (Type=0)
            return flattenedList[index].IsHeader ? 1 : 0;
        }
    }
}
