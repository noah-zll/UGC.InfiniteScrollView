using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGC.InfiniteScrollView
{
    /// <summary>
    /// 通用对象池
    /// 用于高效管理对象的创建、借用和归还，减少频繁的内存分配和垃圾回收。
    /// </summary>
    /// <typeparam name="T">对象类型，必须是引用类型</typeparam>
    /// <remarks>
    /// 对象池的主要优势：
    /// - 减少GC压力：复用对象而不是频繁创建销毁
    /// - 提高性能：避免重复的对象初始化开销
    /// - 内存管理：控制内存使用量，防止内存泄漏
    /// - 线程安全：支持多线程环境下的安全操作
    /// 
    /// 使用场景：
    /// - UI对象池：复用列表项、按钮等UI元素
    /// - 游戏对象池：复用子弹、特效、敌人等游戏对象
    /// - 数据对象池：复用临时数据结构和缓存对象
    /// </remarks>
    /// <example>
    /// 基本使用示例：
    /// <code>
    /// // 创建GameObject对象池
    /// var pool = new ObjectPool&lt;GameObject&gt;(
    ///     createFunc: () => Instantiate(prefab),
    ///     onReturn: obj => obj.SetActive(false),
    ///     onBorrow: obj => obj.SetActive(true),
    ///     maxSize: 50
    /// );
    /// 
    /// // 借用对象
    /// var obj = pool.Borrow();
    /// 
    /// // 使用完毕后归还
    /// pool.Return(obj);
    /// </code>
    /// </example>
    public class ObjectPool<T> where T : class
    {
        #region 私有字段
        
        private readonly Queue<T> pool = new Queue<T>();
        private readonly Func<T> createFunc;
        private readonly Action<T> onReturn;
        private readonly Action<T> onBorrow;
        private readonly int maxSize;
        private int currentSize;
        
        #endregion
        
        #region 公共属性
        
        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int AvailableCount => pool.Count;
        
        /// <summary>
        /// 池的最大容量
        /// </summary>
        public int MaxSize => maxSize;
        
        /// <summary>
        /// 当前池的总大小（包括借出的对象）
        /// </summary>
        public int CurrentSize => currentSize;
        
        /// <summary>
        /// 池的总大小（CurrentSize的别名）
        /// </summary>
        public int Count => currentSize;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="createFunc">创建对象的函数</param>
        /// <param name="onReturn">对象归还时的回调</param>
        /// <param name="onBorrow">对象借出时的回调</param>
        /// <param name="maxSize">池的最大容量</param>
        /// <param name="preloadCount">预加载对象数量</param>
        public ObjectPool(Func<T> createFunc, Action<T> onReturn = null, Action<T> onBorrow = null, int maxSize = 100, int preloadCount = 0)
        {
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            this.onReturn = onReturn;
            this.onBorrow = onBorrow;
            this.maxSize = Mathf.Max(1, maxSize);
            
            // 预加载对象
            for (int i = 0; i < preloadCount && i < maxSize; i++)
            {
                T obj = createFunc();
                if (obj != null)
                {
                    onReturn?.Invoke(obj);
                    pool.Enqueue(obj);
                    currentSize++;
                }
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 从池中借用一个对象
        /// </summary>
        /// <returns>借用的对象，如果池为空且无法创建新对象则返回null</returns>
        public T Borrow()
        {
            T obj;
            
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else if (currentSize < maxSize)
            {
                obj = createFunc();
                if (obj != null)
                {
                    currentSize++;
                }
            }
            else
            {
                // 池已满且没有可用对象
                Debug.LogWarning($"ObjectPool<{typeof(T).Name}> is full and no objects available!");
                return null;
            }
            
            if (obj != null)
            {
                onBorrow?.Invoke(obj);
            }
            
            return obj;
        }
        
        /// <summary>
        /// 将对象归还到池中
        /// </summary>
        /// <param name="obj">要归还的对象</param>
        /// <returns>是否成功归还</returns>
        public bool Return(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("Trying to return null object to pool!");
                return false;
            }
            
            if (pool.Count >= maxSize)
            {
                // 池已满，销毁对象
                DestroyObject(obj);
                currentSize--;
                return false;
            }
            
            onReturn?.Invoke(obj);
            pool.Enqueue(obj);
            return true;
        }
        
        /// <summary>
        /// 清空池中所有对象
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                T obj = pool.Dequeue();
                DestroyObject(obj);
            }
            currentSize = 0;
        }
        
        /// <summary>
        /// 预热池（预先创建指定数量的对象）
        /// </summary>
        /// <param name="count">预创建的对象数量</param>
        public void Warmup(int count)
        {
            count = Mathf.Min(count, maxSize - currentSize);
            
            for (int i = 0; i < count; i++)
            {
                T obj = createFunc();
                if (obj != null)
                {
                    onReturn?.Invoke(obj);
                    pool.Enqueue(obj);
                    currentSize++;
                }
            }
        }
        
        /// <summary>
        /// 收缩池（移除多余的对象）
        /// </summary>
        /// <param name="targetCount">目标对象数量</param>
        public void Shrink(int targetCount)
        {
            targetCount = Mathf.Max(0, targetCount);
            
            while (pool.Count > targetCount)
            {
                T obj = pool.Dequeue();
                DestroyObject(obj);
                currentSize--;
            }
        }
        
        #endregion
        
        #region 私有方法
        
        private void DestroyObject(T obj)
        {
            if (obj is UnityEngine.Object unityObj)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(unityObj);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(unityObj);
                }
            }
            else if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// GameObject专用对象池
    /// 提供针对GameObject的优化实现
    /// </summary>
    public class GameObjectPool : ObjectPool<GameObject>
    {
        #region 私有字段
        
        private readonly GameObject prefab;
        private readonly Transform parent;
        private readonly bool worldPositionStays;
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 创建GameObject对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父级Transform</param>
        /// <param name="maxSize">池的最大容量</param>
        /// <param name="preloadCount">预加载数量</param>
        /// <param name="worldPositionStays">是否保持世界坐标</param>
        public GameObjectPool(GameObject prefab, Transform parent = null, int maxSize = 100, int preloadCount = 0, bool worldPositionStays = false)
            : base(
                () => CreateGameObject(prefab, parent, worldPositionStays),
                obj => OnGameObjectReturn(obj),
                obj => OnGameObjectBorrow(obj),
                maxSize,
                preloadCount
            )
        {
            this.prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            this.parent = parent;
            this.worldPositionStays = worldPositionStays;
        }
        
        #endregion
        
        #region 静态方法
        
        private static GameObject CreateGameObject(GameObject prefab, Transform parent, bool worldPositionStays)
        {
            GameObject obj = UnityEngine.Object.Instantiate(prefab, parent, worldPositionStays);
            obj.SetActive(false);
            return obj;
        }
        
        private static void OnGameObjectReturn(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        private static void OnGameObjectBorrow(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 借用GameObject并设置父级
        /// </summary>
        /// <param name="parent">新的父级Transform</param>
        /// <param name="worldPositionStays">是否保持世界坐标</param>
        /// <returns>借用的GameObject</returns>
        public GameObject Borrow(Transform parent, bool worldPositionStays = false)
        {
            GameObject obj = Borrow();
            if (obj != null && parent != null)
            {
                obj.transform.SetParent(parent, worldPositionStays);
            }
            return obj;
        }
        
        /// <summary>
        /// 借用GameObject并设置位置和旋转
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父级Transform</param>
        /// <returns>借用的GameObject</returns>
        public GameObject Borrow(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject obj = Borrow();
            if (obj != null)
            {
                if (parent != null)
                {
                    obj.transform.SetParent(parent, worldPositionStays);
                }
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 对象池管理器
    /// 用于管理多个对象池
    /// </summary>
    public static class ObjectPoolManager
    {
        #region 私有字段
        
        private static readonly Dictionary<string, object> pools = new Dictionary<string, object>();
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 获取或创建对象池
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">池的唯一标识</param>
        /// <param name="createFunc">创建对象的函数</param>
        /// <param name="onReturn">对象归还时的回调</param>
        /// <param name="onBorrow">对象借出时的回调</param>
        /// <param name="maxSize">池的最大容量</param>
        /// <returns>对象池实例</returns>
        public static ObjectPool<T> GetOrCreatePool<T>(string key, Func<T> createFunc, Action<T> onReturn = null, Action<T> onBorrow = null, int maxSize = 100) where T : class
        {
            if (pools.TryGetValue(key, out object pool))
            {
                return pool as ObjectPool<T>;
            }
            
            var newPool = new ObjectPool<T>(createFunc, onReturn, onBorrow, maxSize);
            pools[key] = newPool;
            return newPool;
        }
        
        /// <summary>
        /// 获取GameObject对象池
        /// </summary>
        /// <param name="key">池的唯一标识</param>
        /// <param name="prefab">预制体</param>
        /// <param name="parent">父级Transform</param>
        /// <param name="maxSize">池的最大容量</param>
        /// <param name="preloadCount">预加载数量</param>
        /// <returns>GameObject对象池</returns>
        public static GameObjectPool GetOrCreateGameObjectPool(string key, GameObject prefab, Transform parent = null, int maxSize = 100, int preloadCount = 0)
        {
            if (pools.TryGetValue(key, out object pool))
            {
                return pool as GameObjectPool;
            }
            
            var newPool = new GameObjectPool(prefab, parent, maxSize, preloadCount);
            pools[key] = newPool;
            return newPool;
        }
        
        /// <summary>
        /// 移除对象池
        /// </summary>
        /// <param name="key">池的唯一标识</param>
        /// <returns>是否成功移除</returns>
        public static bool RemovePool(string key)
        {
            if (pools.TryGetValue(key, out object pool))
            {
                if (pool is ObjectPool<object> objPool)
                {
                    objPool.Clear();
                }
                pools.Remove(key);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public static void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                if (pool is ObjectPool<object> objPool)
                {
                    objPool.Clear();
                }
            }
            pools.Clear();
        }
        
        /// <summary>
        /// 获取池的统计信息
        /// </summary>
        /// <returns>池的统计信息字典</returns>
        public static Dictionary<string, (int available, int total, int maxSize)> GetPoolStats()
        {
            var stats = new Dictionary<string, (int available, int total, int maxSize)>();
            
            foreach (var kvp in pools)
            {
                if (kvp.Value is ObjectPool<object> pool)
                {
                    stats[kvp.Key] = (pool.AvailableCount, pool.CurrentSize, pool.MaxSize);
                }
            }
            
            return stats;
        }
        
        #endregion
    }
}