using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
    public static class GameObjectExtension
    {
        /// <summary>
        /// 查找一个游戏物体
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static GameObject Find(this GameObject obj, string path)
        {
            return obj.transform.Find(path).gameObject;
        }

        /// <summary>
        /// 设置父物体
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="trans"></param>
        public static void SetParent(this GameObject obj, Transform trans)
        {
            obj.transform.SetParent(trans);
            obj.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 设置父物体
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="obj2"></param>
        public static void SetParent(this GameObject obj, GameObject obj2)
        {
            obj.transform.SetParent(obj2.transform);
            obj.transform.localScale = Vector3.one;
        }

        public static void ClearChildren(this GameObject obj)
        {
            Transform trans = obj.transform;

            int childCount = trans.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Object.Destroy(trans.GetChild(i).gameObject);
            }
        }

        public static GameObject AddPrefab(this GameObject obj, string path, string name)
        {
            GameObject prefab = AssetManager.Instance.LoadPrefab(path, name);
            GameObject obj2= Object.Instantiate(prefab);
            obj2.SetParent(obj);
            return obj2;
        }
    }
}