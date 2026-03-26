using System.Collections.Generic;
using UnityEngine;

namespace Extra
{
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance;

        private Dictionary<GameObject, Queue<GameObject>> pool = new();

        private void Awake()
        {
            Instance = this;
        }

        public GameObject Get(GameObject prefab)
        {
            if (!pool.ContainsKey(prefab))
            {
                pool[prefab] = new Queue<GameObject>();
            }

            var queue = pool[prefab];

            if (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            return Instantiate(prefab);
        }

        public void Return(GameObject prefab, GameObject obj)
        {
            obj.SetActive(false);

            if (!pool.ContainsKey(prefab))
            {
                pool[prefab] = new Queue<GameObject>();
            }

            pool[prefab].Enqueue(obj);
        }
    }
}