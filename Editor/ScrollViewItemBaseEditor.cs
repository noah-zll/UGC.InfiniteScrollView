using UnityEngine;
using UnityEditor;
using UGC.InfiniteScrollView;

namespace UGC.InfiniteScrollView.Editor
{
    /// <summary>
    /// ScrollViewItemBase 自定义编辑器
    /// 确保 stateStyle 字段在 Inspector 中正确显示
    /// </summary>
    [CustomEditor(typeof(ScrollViewItemBase), true)]
    public class ScrollViewItemBaseEditor : UnityEditor.Editor
    {
        private SerializedProperty stateStyleProp;
        private SerializedProperty backgroundImageProp;
        private SerializedProperty canvasGroupProp;
        
        private void OnEnable()
        {
            stateStyleProp = serializedObject.FindProperty("stateStyle");
            backgroundImageProp = serializedObject.FindProperty("backgroundImage");
            canvasGroupProp = serializedObject.FindProperty("canvasGroup");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            
            // 视觉状态配置
            EditorGUILayout.LabelField("视觉状态配置", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            // StateStyle 字段 - 使用 ObjectField 来正确显示 ScriptableObject
            EditorGUILayout.PropertyField(stateStyleProp, new GUIContent("State Style", "状态样式配置，可以创建 ItemStateStyle 资源或使用默认样式"));
            
            // 如果没有设置 stateStyle，显示提示信息
            if (stateStyleProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("未设置状态样式。运行时将自动创建默认样式，或者您可以创建 ItemStateStyle 资源文件。", MessageType.Info);
                
                if (GUILayout.Button("创建默认状态样式资源"))
                {
                    CreateDefaultStateStyleAsset();
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 组件引用
            EditorGUILayout.LabelField("组件引用", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.PropertyField(backgroundImageProp, new GUIContent("Background Image", "背景图片组件，用于显示状态颜色"));
            EditorGUILayout.PropertyField(canvasGroupProp, new GUIContent("Canvas Group", "画布组组件，用于控制透明度"));
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // 显示其他属性
            DrawPropertiesExcluding(serializedObject, "m_Script", "stateStyle", "backgroundImage", "canvasGroup");
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void CreateDefaultStateStyleAsset()
        {
            // 创建默认的 ItemStateStyle 资源
            var stateStyle = ScriptableObject.CreateInstance<ItemStateStyle>();
            
            // 设置默认值
            stateStyle.normalState = new ItemVisualState
            {
                useBackgroundColor = true,
                backgroundColor = Color.white,
                useAlpha = true,
                alpha = 1f,
                useScale = false,
                scale = 1f
            };
            
            stateStyle.hoverState = new ItemVisualState
            {
                useBackgroundColor = true,
                backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1f),
                useAlpha = true,
                alpha = 1f,
                useScale = true,
                scale = 1.05f
            };
            
            stateStyle.selectedState = new ItemVisualState
            {
                useBackgroundColor = true,
                backgroundColor = new Color(0.2f, 0.6f, 1f, 1f),
                useAlpha = true,
                alpha = 1f,
                useScale = false,
                scale = 1f
            };
            
            // 保存资源文件
            string path = EditorUtility.SaveFilePanelInProject(
                "保存状态样式",
                "ItemStateStyle",
                "asset",
                "选择保存位置");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(stateStyle, path);
                AssetDatabase.SaveAssets();
                
                // 设置到当前对象
                stateStyleProp.objectReferenceValue = stateStyle;
                serializedObject.ApplyModifiedProperties();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = stateStyle;
            }
        }
    }
}