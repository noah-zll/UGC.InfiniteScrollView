# UGF Infinite ScrollView

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
[![Package Version](https://img.shields.io/badge/Package-1.0.0-orange.svg)](package.json)

一个基于 Unity UI ScrollView 的高性能无限滚动列表组件，专为处理大量数据集合而设计。

## ✨ 核心特性

- 🚀 **高性能渲染** - 仅渲染可见区域内的列表项
- 💾 **内存优化** - 通过对象池复用列表项，避免频繁的内存分配
- 🎨 **灵活布局** - 支持垂直/水平滚动，支持网格和瀑布流布局
- 🔗 **数据绑定** - 提供灵活的数据绑定机制
- 📏 **动态尺寸** - 支持不同高度/宽度的列表项
- 🎯 **平滑滚动** - 支持惯性滚动和边界回弹
- 📱 **事件系统** - 完整的选择、点击、滚动事件支持

## 📦 安装

### 通过 Package Manager 安装

1. 打开 Unity Package Manager
2. 点击 "+" 按钮，选择 "Add package from git URL"
3. 输入：`https://github.com/unity-game-framework/UGC.InfiniteScrollView.git`

### 手动安装

1. 下载最新的 Release 包
2. 解压到项目的 `Packages` 目录下

## 🚀 快速开始

### 基础使用

```csharp
using UGC.InfiniteScrollView;
using UnityEngine;
using System.Collections.Generic;

public class ScrollViewExample : MonoBehaviour
{
    [SerializeField] private InfiniteScrollView scrollView;
    
    void Start()
    {
        // 准备数据
        var data = new List<string>();
        for (int i = 0; i < 10000; i++)
        {
            data.Add($"Item {i}");
        }
        
        // 设置数据
        scrollView.SetData(data);
        
        // 注册事件
        scrollView.OnItemClicked.AddListener(OnItemClicked);
    }
    
    private void OnItemClicked(int index)
    {
        Debug.Log($"点击了第 {index} 个项目");
    }
}
```

### 自定义列表项

```csharp
using UGC.InfiniteScrollView;
using UnityEngine;
using UnityEngine.UI;

public class CustomScrollItem : MonoBehaviour, IScrollViewItem
{
    [SerializeField] private Text titleText;
    [SerializeField] private Image iconImage;
    
    public RectTransform RectTransform => transform as RectTransform;
    public int Index { get; set; }
    public bool IsSelected { get; set; }
    
    public void BindData(object data)
    {
        if (data is string title)
        {
            titleText.text = title;
        }
    }
    
    public Vector2 CalculateSize(object data)
    {
        return new Vector2(300, 80);
    }
    
    public void OnItemRecycled() { }
    public void OnItemActivated() { }
}
```

## 📚 文档

- [设计文档](设计文档.md) - 详细的架构设计和实现说明
- [API 参考](Documentation~/API.md) - 完整的 API 文档
- [使用指南](Documentation~/UserGuide.md) - 详细的使用教程
- [性能优化](Documentation~/Performance.md) - 性能优化建议

## 🎮 示例

查看 `Samples~` 目录下的示例项目：

- **BasicExamples** - 基础使用示例
- **AdvancedExamples** - 高级功能示例
- **PerformanceDemo** - 性能演示

## 🔧 系统要求

- Unity 2021.3 或更高版本
- .NET Standard 2.1
- Unity UI (com.unity.ugui)

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE.md](LICENSE.md) 文件了解详情。

## 🙏 致谢

- Unity Technologies - 提供优秀的游戏引擎
- Unity Game Framework 团队 - 项目支持和维护

---

**Made with ❤️ by Unity Game Framework Team**