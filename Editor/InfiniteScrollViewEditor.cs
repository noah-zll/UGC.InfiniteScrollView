using UnityEngine;
using UnityEditor;
using UGC.InfiniteScrollView;

namespace UGC.InfiniteScrollView.Editor
{
    /// <summary>
    /// InfiniteScrollView 自定义编辑器
    /// 提供更好的Inspector界面和编辑体验
    /// </summary>
    [CustomEditor(typeof(InfiniteScrollView))]
    public class InfiniteScrollViewEditor : UnityEditor.Editor
    {
        #region 序列化属性

        // 基础配置
        private SerializedProperty itemPrefabProp;
        private SerializedProperty extraPrefabsProp;
        private SerializedProperty poolSizeProp;

        // 布局设置
        private SerializedProperty layoutTypeProp;
        private SerializedProperty constraintCountProp;
        private SerializedProperty spacingProp;
        private SerializedProperty paddingProp;
        private SerializedProperty cellSizeProp;
        private SerializedProperty cellSpacingProp;

        // 性能优化
        private SerializedProperty enableVirtualizationProp;
        private SerializedProperty preloadDistanceProp;
        private SerializedProperty maxVisibleItemsProp;

        // 交互状态
        private SerializedProperty enableHoverStateProp;
        private SerializedProperty enableSelectionStateProp;
        private SerializedProperty allowMultipleSelectionProp;

        // 滚动设置
        private SerializedProperty enableInertialScrollProp;
        private SerializedProperty scrollSensitivityProp;
        private SerializedProperty snapToItemProp;

        // 动画设置
        private SerializedProperty enableAnimationProp;

        #endregion

        #region 折叠状态

        private bool showBasicSettings = true;
        private bool showLayoutSettings = true;
        private bool showPerformanceSettings = true;
        private bool showInteractionSettings = true;
        private bool showScrollSettings = true;
        private bool showAnimationSettings = true;

        #endregion

        #region Unity生命周期

        private void OnEnable()
        {
            // 基础配置
            itemPrefabProp = serializedObject.FindProperty("itemPrefab");
            extraPrefabsProp = serializedObject.FindProperty("extraPrefabs");
            poolSizeProp = serializedObject.FindProperty("poolSize");

            // 布局设置
            layoutTypeProp = serializedObject.FindProperty("layoutType");
            constraintCountProp = serializedObject.FindProperty("constraintCount");
            spacingProp = serializedObject.FindProperty("itemSpacing");
            paddingProp = serializedObject.FindProperty("padding");
            cellSizeProp = serializedObject.FindProperty("cellSize");
            cellSpacingProp = serializedObject.FindProperty("cellSpacing");

            // 性能优化
            enableVirtualizationProp = serializedObject.FindProperty("enableVirtualization");
            preloadDistanceProp = serializedObject.FindProperty("preloadDistance");
            maxVisibleItemsProp = serializedObject.FindProperty("maxVisibleItems");

            // 交互状态
            enableHoverStateProp = serializedObject.FindProperty("enableHoverState");
            enableSelectionStateProp = serializedObject.FindProperty("enableSelectionState");
            allowMultipleSelectionProp = serializedObject.FindProperty("allowMultipleSelection");

            // 滚动设置
            enableInertialScrollProp = serializedObject.FindProperty("enableInertia");
            scrollSensitivityProp = serializedObject.FindProperty("decelerationRate");
            snapToItemProp = serializedObject.FindProperty("enableBounce");

            // 动画设置
            enableAnimationProp = serializedObject.FindProperty("enableAnimation");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();

            EditorGUILayout.Space(10);

            DrawBasicSettings();
            DrawLayoutSettings();
            DrawPerformanceSettings();
            DrawInteractionSettings();
            DrawScrollSettings();
            DrawAnimationSettings();
            // DrawEvents(); // Event properties not implemented

            EditorGUILayout.Space(10);

            DrawUtilityButtons();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region 绘制方法

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");

            GUILayout.Label("Infinite Scroll View", EditorStyles.boldLabel);

            var scrollView = target as InfiniteScrollView;
            if (scrollView != null)
            {
                EditorGUILayout.LabelField("Data Count", scrollView.DataCount.ToString());
                EditorGUILayout.LabelField("Visible Items", scrollView.VisibleItemCount.ToString());
                EditorGUILayout.LabelField("Pool Size", scrollView.CurrentPoolSize.ToString());
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBasicSettings()
        {
            showBasicSettings = EditorGUILayout.Foldout(showBasicSettings, "Basic Settings", true);
            if (showBasicSettings)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(itemPrefabProp, new GUIContent("Item Prefab", "预制体模板，必须实现IScrollViewItem接口"));

                if (itemPrefabProp.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Item Prefab is required and must implement IScrollViewItem interface.", MessageType.Error);
                }

                EditorGUILayout.PropertyField(extraPrefabsProp, new GUIContent("Extra Prefabs", "额外的列表项预制体，用于多样式支持（如分组标题）"), true);

                EditorGUILayout.PropertyField(poolSizeProp, new GUIContent("Pool Size", "对象池大小，建议设置为可见项数量的1.5-2倍"));

                if (poolSizeProp.intValue < 10)
                {
                    EditorGUILayout.HelpBox("Pool size should be at least 10 for optimal performance.", MessageType.Warning);
                }

                // Content and ScrollRect are automatically managed by the component

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawLayoutSettings()
        {
            showLayoutSettings = EditorGUILayout.Foldout(showLayoutSettings, "Layout Settings", true);
            if (showLayoutSettings)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(layoutTypeProp, new GUIContent("Layout Type", "布局类型"));

                var layoutType = (LayoutType)layoutTypeProp.enumValueIndex;
                if (layoutType == LayoutType.Grid)
                {
                    EditorGUILayout.PropertyField(constraintCountProp, new GUIContent("Constraint Count", "网格布局的列数或行数"));

                    if (constraintCountProp.intValue < 1)
                    {
                        constraintCountProp.intValue = 1;
                    }

                    EditorGUILayout.PropertyField(cellSizeProp, new GUIContent("Cell Size", "网格单元格大小"));
                    EditorGUILayout.PropertyField(cellSpacingProp, new GUIContent("Cell Spacing", "网格单元格间距"));
                }

                EditorGUILayout.PropertyField(spacingProp, new GUIContent("Spacing", "项之间的间距"));
                EditorGUILayout.PropertyField(paddingProp, new GUIContent("Padding", "内容区域的内边距"));

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawPerformanceSettings()
        {
            showPerformanceSettings = EditorGUILayout.Foldout(showPerformanceSettings, "Performance Settings", true);
            if (showPerformanceSettings)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(enableVirtualizationProp, new GUIContent("Enable Virtualization", "启用虚拟化以提高大数据集性能"));

                if (enableVirtualizationProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(preloadDistanceProp, new GUIContent("Preload Distance", "预加载距离（像素）"));
                    EditorGUI.indentLevel--;

                    if (preloadDistanceProp.floatValue < 0)
                    {
                        preloadDistanceProp.floatValue = 0;
                    }
                }

                EditorGUILayout.PropertyField(maxVisibleItemsProp, new GUIContent("Max Visible Items", "最大可见项目数量，用于性能控制"));

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawInteractionSettings()
        {
            showInteractionSettings = EditorGUILayout.Foldout(showInteractionSettings, "Interaction Settings", true);
            if (showInteractionSettings)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(enableHoverStateProp, new GUIContent("Enable Hover State", "启用鼠标悬停状态"));
                EditorGUILayout.PropertyField(enableSelectionStateProp, new GUIContent("Enable Selection State", "启用选中状态"));

                if (enableSelectionStateProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(allowMultipleSelectionProp, new GUIContent("Allow Multiple Selection", "允许多选"));
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawScrollSettings()
        {
            showScrollSettings = EditorGUILayout.Foldout(showScrollSettings, "Scroll Settings", true);
            if (showScrollSettings)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(enableInertialScrollProp, new GUIContent("Enable Inertia", "启用惯性滚动"));
                EditorGUILayout.PropertyField(scrollSensitivityProp, new GUIContent("Deceleration Rate", "减速率"));
                EditorGUILayout.PropertyField(snapToItemProp, new GUIContent("Enable Bounce", "启用边界回弹效果"));

                if (scrollSensitivityProp.floatValue <= 0)
                {
                    scrollSensitivityProp.floatValue = 0.135f;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawAnimationSettings()
        {
            showAnimationSettings = EditorGUILayout.Foldout(showAnimationSettings, "Animation Settings", true);
            if (showAnimationSettings)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.PropertyField(enableAnimationProp, new GUIContent("Enable Animation", "启用过渡动画（展开/收起等）"));

                EditorGUILayout.EndVertical();
            }
        }


        private void DrawUtilityButtons()
        {
            EditorGUILayout.BeginVertical("box");

            GUILayout.Label("Utilities", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Auto Setup"))
            {
                AutoSetup();
            }

            if (GUILayout.Button("Clear Pool"))
            {
                ClearPool();
            }

            if (GUILayout.Button("Refresh Layout"))
            {
                RefreshLayout();
            }

            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Scroll To Top"))
                {
                    ScrollToTop();
                }

                if (GUILayout.Button("Scroll To Bottom"))
                {
                    ScrollToBottom();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 工具方法

        private void AutoSetup()
        {
            var scrollView = target as InfiniteScrollView;
            if (scrollView == null) return;

            // 设置默认值
            if (poolSizeProp.intValue <= 0)
            {
                poolSizeProp.intValue = 20;
            }

            if (scrollSensitivityProp.floatValue <= 0)
            {
                scrollSensitivityProp.floatValue = 1f;
            }

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(target);
        }

        private void ClearPool()
        {
            var scrollView = target as InfiniteScrollView;
            if (scrollView != null && Application.isPlaying)
            {
                scrollView.ClearPool();
            }
        }

        private void RefreshLayout()
        {
            var scrollView = target as InfiniteScrollView;
            if (scrollView != null)
            {
                if (Application.isPlaying)
                {
                    scrollView.RefreshLayout();
                }
                else
                {
                    // 在编辑模式下，只能标记组件为脏状态，运行时才能完全刷新
                    EditorUtility.SetDirty(scrollView);
                    Debug.LogWarning("[InfiniteScrollViewEditor] 编辑模式下无法完全刷新布局，请在运行时使用此功能。\n" +
                                   "编辑模式下的布局更改会在下次运行时自动应用。");
                }
            }
        }

        private void ScrollToTop()
        {
            var scrollView = target as InfiniteScrollView;
            if (scrollView != null && Application.isPlaying)
            {
                scrollView.ScrollToTop(true);
            }
        }

        private void ScrollToBottom()
        {
            var scrollView = target as InfiniteScrollView;
            if (scrollView != null && Application.isPlaying)
            {
                scrollView.ScrollToBottom(true);
            }
        }

        #endregion
    }
}