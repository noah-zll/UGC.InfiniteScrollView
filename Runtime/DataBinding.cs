using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 数据绑定接口
    /// 定义数据绑定的基本行为
    /// </summary>
    public interface IDataBinding
    {
        /// <summary>
        /// 绑定数据源
        /// </summary>
        /// <param name="dataSource">数据源</param>
        void BindData(object dataSource);
        
        /// <summary>
        /// 解绑数据源
        /// </summary>
        void UnbindData();
        
        /// <summary>
        /// 刷新数据显示
        /// </summary>
        void RefreshData();
    }
    
    /// <summary>
    /// 视图模型基类
    /// 实现INotifyPropertyChanged接口，支持属性变更通知
    /// </summary>
    [Serializable]
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region 事件
        
        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion
        
        #region 受保护方法
        
        /// <summary>
        /// 触发属性变更通知
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// 设置属性值并触发变更通知
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否发生了变更</returns>
        protected bool SetProperty<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 可观察集合
    /// 支持集合变更通知的泛型集合
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    [Serializable]
    public class ObservableCollection<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        #region 私有字段
        
        private readonly List<T> items = new List<T>();
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 集合变更事件
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        
        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 元素数量
        /// </summary>
        public int Count => items.Count;
        
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly => false;
        
        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>元素</returns>
        public T this[int index]
        {
            get => items[index];
            set
            {
                T oldValue = items[index];
                items[index] = value;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, index));
            }
        }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ObservableCollection()
        {
        }
        
        /// <summary>
        /// 从现有集合构造
        /// </summary>
        /// <param name="collection">现有集合</param>
        public ObservableCollection(IEnumerable<T> collection)
        {
            if (collection != null)
            {
                items.AddRange(collection);
            }
        }
        
        #endregion
        
        #region IList<T> 实现
        
        public void Add(T item)
        {
            items.Add(item);
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, items.Count - 1));
        }
        
        public void Insert(int index, T item)
        {
            items.Insert(index, item);
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }
        
        public bool Remove(T item)
        {
            int index = items.IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }
        
        public void RemoveAt(int index)
        {
            T item = items[index];
            items.RemoveAt(index);
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }
        
        public void Clear()
        {
            if (items.Count > 0)
            {
                items.Clear();
                OnPropertyChanged(nameof(Count));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        
        public bool Contains(T item)
        {
            return items.Contains(item);
        }
        
        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }
        
        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion
        
        #region 批量操作
        
        /// <summary>
        /// 添加多个元素
        /// </summary>
        /// <param name="collection">要添加的元素集合</param>
        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
                return;
            
            int startIndex = items.Count;
            var newItems = new List<T>(collection);
            
            if (newItems.Count > 0)
            {
                items.AddRange(newItems);
                OnPropertyChanged(nameof(Count));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, startIndex));
            }
        }
        
        /// <summary>
        /// 移除多个元素
        /// </summary>
        /// <param name="collection">要移除的元素集合</param>
        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null)
                return;
            
            var itemsToRemove = new List<T>(collection);
            bool hasChanges = false;
            
            foreach (var item in itemsToRemove)
            {
                if (items.Remove(item))
                {
                    hasChanges = true;
                }
            }
            
            if (hasChanges)
            {
                OnPropertyChanged(nameof(Count));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        
        /// <summary>
        /// 替换所有元素
        /// </summary>
        /// <param name="collection">新的元素集合</param>
        public void ReplaceAll(IEnumerable<T> collection)
        {
            items.Clear();
            if (collection != null)
            {
                items.AddRange(collection);
            }
            OnPropertyChanged(nameof(Count));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        
        #endregion
        
        #region 事件触发
        
        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        /// <summary>
        /// 触发集合变更事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 数据绑定上下文
    /// 管理数据绑定的上下文信息
    /// </summary>
    public class DataBindingContext
    {
        #region 私有字段
        
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();
        private readonly Dictionary<string, List<Action<object>>> propertyBindings = new Dictionary<string, List<Action<object>>>();
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 数据源
        /// </summary>
        public object DataSource { get; private set; }
        
        #endregion
        
        #region 事件
        
        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event Action<string, object> PropertyChanged;
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <param name="dataSource">数据源</param>
        public void SetDataSource(object dataSource)
        {
            if (DataSource is INotifyPropertyChanged oldNotifyPropertyChanged)
            {
                oldNotifyPropertyChanged.PropertyChanged -= OnDataSourcePropertyChanged;
            }
            
            DataSource = dataSource;
            
            if (DataSource is INotifyPropertyChanged newNotifyPropertyChanged)
            {
                newNotifyPropertyChanged.PropertyChanged += OnDataSourcePropertyChanged;
            }
            
            RefreshAllBindings();
        }
        
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <returns>属性值</returns>
        public object GetProperty(string propertyName)
        {
            if (properties.TryGetValue(propertyName, out object value))
            {
                return value;
            }
            
            if (DataSource != null)
            {
                var property = DataSource.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    value = property.GetValue(DataSource);
                    properties[propertyName] = value;
                    return value;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">属性值</param>
        public void SetProperty(string propertyName, object value)
        {
            properties[propertyName] = value;
            
            if (DataSource != null)
            {
                var property = DataSource.GetType().GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(DataSource, value);
                }
            }
            
            NotifyPropertyChanged(propertyName, value);
        }
        
        /// <summary>
        /// 绑定属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="callback">回调函数</param>
        public void BindProperty(string propertyName, Action<object> callback)
        {
            if (!propertyBindings.TryGetValue(propertyName, out List<Action<object>> callbacks))
            {
                callbacks = new List<Action<object>>();
                propertyBindings[propertyName] = callbacks;
            }
            
            callbacks.Add(callback);
            
            // 立即调用一次回调
            callback?.Invoke(GetProperty(propertyName));
        }
        
        /// <summary>
        /// 解绑属性
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="callback">回调函数</param>
        public void UnbindProperty(string propertyName, Action<object> callback)
        {
            if (propertyBindings.TryGetValue(propertyName, out List<Action<object>> callbacks))
            {
                callbacks.Remove(callback);
                if (callbacks.Count == 0)
                {
                    propertyBindings.Remove(propertyName);
                }
            }
        }
        
        /// <summary>
        /// 清空所有绑定
        /// </summary>
        public void ClearBindings()
        {
            propertyBindings.Clear();
            properties.Clear();
        }
        
        #endregion
        
        #region 私有方法
        
        private void OnDataSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            object value = GetProperty(propertyName);
            NotifyPropertyChanged(propertyName, value);
        }
        
        private void NotifyPropertyChanged(string propertyName, object value)
        {
            PropertyChanged?.Invoke(propertyName, value);
            
            if (propertyBindings.TryGetValue(propertyName, out List<Action<object>> callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback?.Invoke(value);
                }
            }
        }
        
        private void RefreshAllBindings()
        {
            properties.Clear();
            
            foreach (var kvp in propertyBindings)
            {
                string propertyName = kvp.Key;
                var callbacks = kvp.Value;
                object value = GetProperty(propertyName);
                
                foreach (var callback in callbacks)
                {
                    callback?.Invoke(value);
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 数据绑定工具类
    /// 提供数据绑定的辅助方法
    /// </summary>
    public static class DataBindingUtility
    {
        /// <summary>
        /// 创建双向绑定
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="sourceProperty">源属性名</param>
        /// <param name="target">目标对象</param>
        /// <param name="targetProperty">目标属性名</param>
        /// <returns>绑定对象</returns>
        public static TwoWayBinding CreateTwoWayBinding(object source, string sourceProperty, object target, string targetProperty)
        {
            return new TwoWayBinding(source, sourceProperty, target, targetProperty);
        }
        
        /// <summary>
        /// 创建单向绑定
        /// </summary>
        /// <param name="source">源对象</param>
        /// <param name="sourceProperty">源属性名</param>
        /// <param name="target">目标对象</param>
        /// <param name="targetProperty">目标属性名</param>
        /// <returns>绑定对象</returns>
        public static OneWayBinding CreateOneWayBinding(object source, string sourceProperty, object target, string targetProperty)
        {
            return new OneWayBinding(source, sourceProperty, target, targetProperty);
        }
    }
    
    /// <summary>
    /// 绑定基类
    /// </summary>
    public abstract class BindingBase : IDisposable
    {
        protected object source;
        protected string sourceProperty;
        protected object target;
        protected string targetProperty;
        protected bool disposed;
        
        protected BindingBase(object source, string sourceProperty, object target, string targetProperty)
        {
            this.source = source;
            this.sourceProperty = sourceProperty;
            this.target = target;
            this.targetProperty = targetProperty;
        }
        
        public abstract void Dispose();
    }
    
    /// <summary>
    /// 单向绑定
    /// </summary>
    public class OneWayBinding : BindingBase
    {
        public OneWayBinding(object source, string sourceProperty, object target, string targetProperty)
            : base(source, sourceProperty, target, targetProperty)
        {
            if (source is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnSourcePropertyChanged;
            }
            
            // 初始同步
            SyncValue();
        }
        
        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == sourceProperty)
            {
                SyncValue();
            }
        }
        
        private void SyncValue()
        {
            if (disposed) return;
            
            var sourcePropertyInfo = source.GetType().GetProperty(sourceProperty);
            var targetPropertyInfo = target.GetType().GetProperty(targetProperty);
            
            if (sourcePropertyInfo != null && targetPropertyInfo != null && targetPropertyInfo.CanWrite)
            {
                object value = sourcePropertyInfo.GetValue(source);
                targetPropertyInfo.SetValue(target, value);
            }
        }
        
        public override void Dispose()
        {
            if (!disposed)
            {
                if (source is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= OnSourcePropertyChanged;
                }
                disposed = true;
            }
        }
    }
    
    /// <summary>
    /// 双向绑定
    /// </summary>
    public class TwoWayBinding : BindingBase
    {
        private bool isUpdating;
        
        public TwoWayBinding(object source, string sourceProperty, object target, string targetProperty)
            : base(source, sourceProperty, target, targetProperty)
        {
            if (source is INotifyPropertyChanged sourceNotify)
            {
                sourceNotify.PropertyChanged += OnSourcePropertyChanged;
            }
            
            if (target is INotifyPropertyChanged targetNotify)
            {
                targetNotify.PropertyChanged += OnTargetPropertyChanged;
            }
            
            // 初始同步（从源到目标）
            SyncSourceToTarget();
        }
        
        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == sourceProperty && !isUpdating)
            {
                SyncSourceToTarget();
            }
        }
        
        private void OnTargetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == targetProperty && !isUpdating)
            {
                SyncTargetToSource();
            }
        }
        
        private void SyncSourceToTarget()
        {
            if (disposed || isUpdating) return;
            
            isUpdating = true;
            try
            {
                var sourcePropertyInfo = source.GetType().GetProperty(sourceProperty);
                var targetPropertyInfo = target.GetType().GetProperty(targetProperty);
                
                if (sourcePropertyInfo != null && targetPropertyInfo != null && targetPropertyInfo.CanWrite)
                {
                    object value = sourcePropertyInfo.GetValue(source);
                    targetPropertyInfo.SetValue(target, value);
                }
            }
            finally
            {
                isUpdating = false;
            }
        }
        
        private void SyncTargetToSource()
        {
            if (disposed || isUpdating) return;
            
            isUpdating = true;
            try
            {
                var sourcePropertyInfo = source.GetType().GetProperty(sourceProperty);
                var targetPropertyInfo = target.GetType().GetProperty(targetProperty);
                
                if (sourcePropertyInfo != null && sourcePropertyInfo.CanWrite && targetPropertyInfo != null)
                {
                    object value = targetPropertyInfo.GetValue(target);
                    sourcePropertyInfo.SetValue(source, value);
                }
            }
            finally
            {
                isUpdating = false;
            }
        }
        
        public override void Dispose()
        {
            if (!disposed)
            {
                if (source is INotifyPropertyChanged sourceNotify)
                {
                    sourceNotify.PropertyChanged -= OnSourcePropertyChanged;
                }
                
                if (target is INotifyPropertyChanged targetNotify)
                {
                    targetNotify.PropertyChanged -= OnTargetPropertyChanged;
                }
                
                disposed = true;
            }
        }
    }
}