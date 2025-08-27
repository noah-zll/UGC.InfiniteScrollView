# UGC Infinite Scroll View API 文档

## 概述

UGC Infinite Scroll View 是一个高性能的无限滚动列表组件，专为 Unity UI 设计。它支持虚拟化、对象池、交互状态管理和数据绑定等高级功能。

## 核心组件

### InfiniteScrollView

主要的滚动视图组件，继承自 MonoBehaviour。

#### 属性

| 属性名 | 类型 | 描述 |
|--------|------|------|
| `itemPrefab` | GameObject | 列表项的预制体，必须包含 IScrollViewItem 组件 |
| `poolSize` | int | 对象池大小，建议设置为可见区域能容纳的项数的 1.5-2 倍 |
| `layoutType` | LayoutType | 布局类型：Vertical（垂直）、Horizontal（水平）、Grid（网格） |
| `constraintCount` | int | 网格布局时的约束数量（列数或行数） |
| `spacing` | Vector2 | 项之间的间距 |
| `padding` | RectOffset | 内容区域的内边距 |
| `enableVirtualization` | bool | 是否启用虚拟化（推荐开启以提高性能） |
| `preloadDistance` | float | 预加载距离，超出可见区域多远开始预加载项 |
| `enableHoverState` | bool | 是否启用悬停状态 |
| `enableSelectionState` | bool | 是否启用选中状态 |
| `allowMultipleSelection` | bool | 是否允许多选 |
| `ItemCount` | int | 当前数据项总数（只读） |

#### 方法

##### SetData<T>(IList<T> data)
设置滚动视图的数据源。

**参数：**
- `data`: 数据列表，可以为 null（清空数据）

**示例：**
```csharp
var items = new List<MyItemData>();
scrollView.SetData(items);
```

##### AddItem<T>(T item)
添加单个数据项到列表末尾。

**参数：**
- `item`: 要添加的数据项

##### InsertItem<T>(int index, T item)
在指定位置插入数据项。

**参数：**
- `index`: 插入位置
- `item`: 要插入的数据项

##### RemoveItem(int index)
移除指定索引的数据项。

**参数：**
- `index`: 要移除的项的索引

##### ClearData()
清空所有数据。

##### ScrollToIndex(int index, bool animated = true)
滚动到指定索引的项。

**参数：**
- `index`: 目标索引
- `animated`: 是否使用动画

##### ScrollToTop(bool animated = true)
滚动到顶部。

##### ScrollToBottom(bool animated = true)
滚动到底部。

##### SelectItems(params int[] indices)
选中指定索引的项（多选模式）。

##### SelectItem(int index)
选中单个项。

##### DeselectItem(int index)
取消选中指定项。

##### ClearSelection()
清空所有选中状态。

#### 事件

| 事件名 | 类型 | 描述 |
|--------|------|------|
| `OnItemClicked` | UnityEvent<int> | 项被点击时触发 |
| `OnItemSelected` | UnityEvent<int> | 项被选中时触发 |
| `OnItemDeselected` | UnityEvent<int> | 项被取消选中时触发 |
| `OnItemHovered` | UnityEvent<int> | 项被悬停时触发 |
| `OnScrollValueChanged` | UnityEvent<Vector2> | 滚动值改变时触发 |

### IScrollViewItem

列表项必须实现的接口。

#### 属性

| 属性名 | 类型 | 描述 |
|--------|------|------|
| `Index` | int | 项在列表中的索引 |
| `IsSelected` | bool | 是否被选中 |
| `IsHovered` | bool | 是否被悬停 |
| `RectTransform` | RectTransform | 项的 RectTransform 组件 |
| `ScrollView` | InfiniteScrollView | 所属的滚动视图 |

#### 方法

##### SetData<T>(T data)
设置项的数据。

##### SetScrollView(InfiniteScrollView scrollView)
设置所属的滚动视图。

##### OnItemSelected()
项被选中时调用。

##### OnItemDeselected()
项被取消选中时调用。

##### OnItemHovered(bool isHovered)
项悬停状态改变时调用。

##### OnItemClicked()
项被点击时调用。

##### RefreshDisplay()
刷新项的显示。

##### GetPreferredSize()
获取项的首选大小。

**返回值：** Vector2 - 首选大小

##### ResetItem()
重置项到初始状态（用于对象池回收）。

### ScrollViewItemBase

IScrollViewItem 的基础实现类，提供了常用的功能。

#### 属性

| 属性名 | 类型 | 描述 |
|--------|------|------|
| `stateStyle` | ItemStateStyle | 视觉状态样式配置 |
| `backgroundImage` | Image | 背景图片组件 |
| `canvasGroup` | CanvasGroup | 用于透明度控制的 CanvasGroup |

#### 受保护的虚方法

这些方法可以在子类中重写以实现自定义行为：

- `OnDataSet<T>(T data)`: 数据设置时调用
- `OnSelectionChanged(bool isSelected)`: 选中状态改变时调用
- `OnHoverChanged(bool isHovered)`: 悬停状态改变时调用
- `OnClicked()`: 点击时调用

### LayoutManager

布局管理器的抽象基类。

#### 具体实现

- **VerticalLayoutManager**: 垂直布局
- **HorizontalLayoutManager**: 水平布局
- **GridLayoutManager**: 网格布局

#### 方法

##### Initialize(Vector2 containerSize, Vector2 spacing, RectOffset padding)
初始化布局管理器。

##### CalculateLayout(IList<Vector2> itemSizes)
计算所有项的布局信息。

**返回值：** List<ItemLayoutInfo> - 布局信息列表

##### GetItemPosition(int index)
获取指定项的位置。

##### GetItemSize(int index)
获取指定项的大小。

##### GetVisibleRange(Vector2 viewportPosition, Vector2 viewportSize)
获取可见范围内的项索引。

**返回值：** (int startIndex, int endIndex)

### ObjectPool<T>

通用对象池实现。

#### 构造函数

```csharp
ObjectPool<T>(
    Func<T> createFunc,
    Action<T> actionOnGet = null,
    Action<T> actionOnReturn = null,
    Action<T> actionOnDestroy = null,
    bool collectionCheck = true,
    int defaultCapacity = 10,
    int maxSize = 10000
)
```

#### 方法

##### Get()
从池中获取对象。

**返回值：** T - 池中的对象

##### Return(T item)
将对象返回到池中。

##### Clear()
清空池中所有对象。

##### Dispose()
释放池资源。

### DataBinding

数据绑定系统，支持单向和双向绑定。

#### ViewModelBase

ViewModel 的基类，实现了 INotifyPropertyChanged。

##### SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
设置属性值并触发变更通知。

#### ObservableCollection<T>

可观察的集合，支持集合变更通知。

#### DataBindingUtility

数据绑定工具类。

##### CreateOneWayBinding<T>(Func<T> source, Action<T> target)
创建单向绑定。

##### CreateTwoWayBinding<T>(Func<T> source, Action<T> target, Func<T> targetSource, Action<T> sourceTarget)
创建双向绑定。

## 使用示例

### 基本用法

```csharp
// 1. 创建数据
var items = new List<MyItemData>();
for (int i = 0; i < 1000; i++)
{
    items.Add(new MyItemData { title = $"Item {i}", description = $"Description {i}" });
}

// 2. 设置数据
scrollView.SetData(items);

// 3. 监听事件
scrollView.OnItemClicked.AddListener(index => {
    Debug.Log($"Clicked item {index}");
});
```

### 自定义列表项

```csharp
public class MyListItem : ScrollViewItemBase
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    
    protected override void OnDataSet<T>(T data)
    {
        if (data is MyItemData itemData)
        {
            titleText.text = itemData.title;
            descriptionText.text = itemData.description;
        }
    }
    
    public override Vector2 GetPreferredSize()
    {
        return new Vector2(380, 80);
    }
}
```

### 数据绑定示例

```csharp
public class MyViewModel : ViewModelBase
{
    private ObservableCollection<MyItemData> _items = new ObservableCollection<MyItemData>();
    
    public ObservableCollection<MyItemData> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
}

// 绑定到滚动视图
var binding = DataBindingUtility.CreateOneWayBinding(
    () => viewModel.Items,
    items => scrollView.SetData(items)
);
```

## 性能优化建议

1. **启用虚拟化**：对于大数据集，始终启用 `enableVirtualization`
2. **合理设置池大小**：`poolSize` 应为可见项数的 1.5-2 倍
3. **优化项大小计算**：在 `GetPreferredSize()` 中避免复杂计算
4. **使用对象池**：避免频繁创建和销毁 GameObject
5. **批量操作**：使用 `SetData()` 而不是多次调用 `AddItem()`

## 常见问题

### Q: 如何处理动态高度的列表项？
A: 重写 `GetPreferredSize()` 方法，根据内容计算实际需要的高度。

### Q: 如何实现无限滚动？
A: 监听 `OnScrollValueChanged` 事件，在接近底部时动态加载更多数据。

### Q: 如何优化大数据集的性能？
A: 启用虚拟化，合理设置预加载距离，使用对象池。

### Q: 如何自定义选中和悬停效果？
A: 配置 `ItemStateStyle` 或重写 `OnSelectionChanged()` 和 `OnHoverChanged()` 方法。

## 版本历史

- **1.0.0**: 初始版本，包含基础功能
  - 无限滚动和虚拟化
  - 多种布局支持
  - 交互状态管理
  - 数据绑定系统
  - 对象池优化
  - 完整的编辑器工具