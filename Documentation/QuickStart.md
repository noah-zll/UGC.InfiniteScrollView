# 快速入门指南

本指南将帮助您快速开始使用 UGC Infinite Scroll View 组件。

## 安装

### 通过 Unity Package Manager 安装

1. 打开 Unity Package Manager（Window > Package Manager）
2. 点击左上角的 "+" 按钮
3. 选择 "Add package from git URL"
4. 输入包的 Git URL 或本地路径
5. 点击 "Add" 完成安装

### 手动安装

1. 下载或克隆项目到本地
2. 将整个文件夹复制到您的 Unity 项目的 `Packages` 目录下
3. Unity 会自动识别并导入包

## 第一个无限滚动列表

### 步骤 1：创建基础 UI 结构

1. 在场景中创建一个 Canvas（如果还没有的话）
2. 在 Canvas 下创建一个空的 GameObject，命名为 "ScrollView"
3. 为 ScrollView 添加 `RectTransform` 组件
4. 添加 `InfiniteScrollView` 组件

### 步骤 2：创建列表项预制体

1. 创建一个新的 GameObject，命名为 "ListItem"
2. 添加 `RectTransform` 组件
3. 添加背景 Image 组件
4. 添加文本组件用于显示内容
5. 创建并添加自定义的列表项脚本：

```csharp
using UnityEngine;
using UnityEngine.UI;
using UGC.InfiniteScrollView;

public class SimpleItem : ScrollViewItemBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    
    protected override void OnDataSet<T>(T data)
    {
        if (data is ItemData itemData)
        {
            titleText.text = itemData.title;
            descriptionText.text = itemData.description;
        }
    }
    
    public override Vector2 GetPreferredSize()
    {
        return new Vector2(400, 80); // 固定大小
    }
}

[System.Serializable]
public class ItemData
{
    public string title;
    public string description;
}
```

6. 将 ListItem 保存为预制体

### 步骤 3：配置 InfiniteScrollView

1. 选中 ScrollView GameObject
2. 在 InfiniteScrollView 组件中：
   - 将创建的 ListItem 预制体拖拽到 `Item Prefab` 字段
   - 设置 `Pool Size` 为 20（根据需要调整）
   - 选择 `Layout Type` 为 Vertical
   - 启用 `Enable Virtualization`

### 步骤 4：创建控制脚本

创建一个脚本来管理数据和交互：

```csharp
using System.Collections.Generic;
using UnityEngine;
using UGC.InfiniteScrollView;

public class ScrollViewController : MonoBehaviour
{
    [SerializeField] private InfiniteScrollView scrollView;
    
    private List<ItemData> items = new List<ItemData>();
    
    void Start()
    {
        // 生成测试数据
        GenerateTestData();
        
        // 设置数据到滚动视图
        scrollView.SetData(items);
        
        // 监听事件
        scrollView.OnItemClicked.AddListener(OnItemClicked);
    }
    
    void GenerateTestData()
    {
        for (int i = 0; i < 1000; i++)
        {
            items.Add(new ItemData
            {
                title = $"Item {i + 1}",
                description = $"This is the description for item {i + 1}"
            });
        }
    }
    
    void OnItemClicked(int index)
    {
        Debug.Log($"Clicked item: {items[index].title}");
    }
}
```

### 步骤 5：运行和测试

1. 将控制脚本添加到场景中的某个 GameObject 上
2. 在控制脚本中引用 ScrollView
3. 运行场景，您应该能看到一个包含 1000 个项目的滚动列表

## 常用配置

### 垂直列表（默认）

```csharp
scrollView.layoutType = LayoutType.Vertical;
scrollView.spacing = new Vector2(0, 10); // 项之间的垂直间距
scrollView.padding = new RectOffset(10, 10, 10, 10); // 内边距
```

### 水平列表

```csharp
scrollView.layoutType = LayoutType.Horizontal;
scrollView.spacing = new Vector2(10, 0); // 项之间的水平间距
```

### 网格布局

```csharp
scrollView.layoutType = LayoutType.Grid;
scrollView.constraintCount = 3; // 每行 3 列
scrollView.spacing = new Vector2(10, 10); // 水平和垂直间距
```

### 启用交互状态

```csharp
// 启用悬停效果
scrollView.enableHoverState = true;

// 启用选中功能
scrollView.enableSelectionState = true;
scrollView.allowMultipleSelection = true; // 允许多选

// 监听选中事件
scrollView.OnItemSelected.AddListener(index => {
    Debug.Log($"Selected item {index}");
});
```

## 性能优化技巧

### 1. 合理设置池大小

```csharp
// 池大小应该是可见项数的 1.5-2 倍
// 例如：如果屏幕能显示 10 个项目，设置池大小为 15-20
scrollView.poolSize = 20;
```

### 2. 启用虚拟化

```csharp
// 对于大数据集，始终启用虚拟化
scrollView.enableVirtualization = true;
scrollView.preloadDistance = 200f; // 预加载距离
```

### 3. 优化项目大小计算

```csharp
public class OptimizedItem : ScrollViewItemBase
{
    private Vector2 cachedSize = new Vector2(400, 80);
    
    public override Vector2 GetPreferredSize()
    {
        // 返回缓存的大小，避免重复计算
        return cachedSize;
    }
}
```

## 常见使用场景

### 聊天消息列表

```csharp
public class ChatMessage
{
    public string sender;
    public string content;
    public System.DateTime timestamp;
    public bool isOwnMessage;
}

public class ChatItem : ScrollViewItemBase
{
    [SerializeField] private Text senderText;
    [SerializeField] private Text contentText;
    [SerializeField] private Text timeText;
    [SerializeField] private Image backgroundImage;
    
    protected override void OnDataSet<T>(T data)
    {
        if (data is ChatMessage message)
        {
            senderText.text = message.sender;
            contentText.text = message.content;
            timeText.text = message.timestamp.ToString("HH:mm");
            
            // 根据是否为自己的消息调整样式
            backgroundImage.color = message.isOwnMessage ? 
                Color.blue : Color.gray;
        }
    }
    
    public override Vector2 GetPreferredSize()
    {
        // 根据内容动态计算高度
        float height = 60f + (contentText.preferredHeight - 20f);
        return new Vector2(400, Mathf.Max(60f, height));
    }
}
```

### 商品列表

```csharp
public class Product
{
    public string name;
    public string description;
    public float price;
    public Sprite icon;
}

public class ProductItem : ScrollViewItemBase
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;
    
    protected override void OnDataSet<T>(T data)
    {
        if (data is Product product)
        {
            iconImage.sprite = product.icon;
            nameText.text = product.name;
            descriptionText.text = product.description;
            priceText.text = $"${product.price:F2}";
            
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => BuyProduct(product));
        }
    }
    
    private void BuyProduct(Product product)
    {
        Debug.Log($"Buying {product.name}");
    }
}
```

## 下一步

- 查看 [API 文档](API.md) 了解详细的 API 参考
- 探索 Samples 文件夹中的示例项目
- 学习高级功能如数据绑定和自定义布局

## 常见问题

**Q: 列表项没有显示？**
A: 检查是否正确设置了 itemPrefab，并确保预制体包含实现了 IScrollViewItem 的组件。

**Q: 滚动性能不佳？**
A: 确保启用了虚拟化，合理设置池大小，并优化列表项的 GetPreferredSize() 方法。

**Q: 如何实现下拉刷新？**
A: 监听滚动事件，当滚动到顶部时触发刷新逻辑。

**Q: 如何添加加载更多功能？**
A: 监听滚动事件，当接近底部时动态添加更多数据。