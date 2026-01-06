using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 재사용 가능한 제네릭 Object Pool 시스템
    /// UI 리스트 아이템 등 반복적으로 생성/삭제되는 GameObject 관리에 사용
    /// </summary>
    /// <typeparam name="T">풀링할 컴포넌트 타입 (MonoBehaviour 상속 필요)</typeparam>
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> pool = new Queue<T>();
        private readonly List<T> activeObjects = new List<T>();

        public ObjectPool(T prefab, Transform parent, int initialSize = 0)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            T obj = GameObject.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            return obj;
        }

        /// <summary>
        /// 풀에서 객체를 가져옵니다. 풀이 비어있으면 새로 생성합니다.
        /// </summary>
        public T Get()
        {
            T obj;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// 객체를 풀에 반환합니다.
        /// </summary>
        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            activeObjects.Remove(obj);
            pool.Enqueue(obj);
        }

        /// <summary>
        /// 모든 활성 객체를 풀에 반환합니다.
        /// </summary>
        public void ReturnAll()
        {
            while (activeObjects.Count > 0)
            {
                Return(activeObjects[0]);
            }
        }

        /// <summary>
        /// 활성 객체 개수
        /// </summary>
        public int ActiveCount => activeObjects.Count;

        /// <summary>
        /// 풀에 대기 중인 객체 개수
        /// </summary>
        public int PoolCount => pool.Count;
    }
}
