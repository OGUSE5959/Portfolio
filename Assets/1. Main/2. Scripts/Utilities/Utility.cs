using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
// using System;

namespace AT.SerializableDictionary
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();
        [SerializeField]
        private List<TValue> values = new List<TValue>();
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }
        public void OnAfterDeserialize()
        {
            this.Clear();
            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }
}

public static class Utility
{
    static Dictionary<float, WaitForSeconds> m_waitForSecondsTable = new Dictionary<float, WaitForSeconds>();
    public static Quaternion QI { get { return Quaternion.identity; } }
    public static WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!m_waitForSecondsTable.ContainsKey(seconds))
            m_waitForSecondsTable.Add(seconds, new WaitForSeconds(seconds));
        return m_waitForSecondsTable[seconds];
    }
    public static T StringToEnum<T>(string str)
    {
        return (T)Enum.Parse(typeof(T), str);
    }
    public static bool TryStringToEnum<T>(string str, out T result)
    {
        if(Enum.TryParse(typeof(T), str, out object rs))
        {
            result =  StringToEnum<T>(str);
            return true;

        }
        result = default(T);
        return false;
    }
    public static Vector3 GetNormalizedDir(Vector3 to, Vector3 from)
    {
        var dir = GetDirection(to, from);
        return dir.normalized;
    }
    public static Vector3 GetDirection(Vector3 to, Vector3 from)
    {
        var dir = (to - from);
        return dir;
    }
    public static float GetDistance(Vector3 a, Vector3 b)
    {
        var sqr = Vector3.SqrMagnitude(a - b);
        return Mathf.Sqrt(sqr);
    }
    public static void ChangeLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, layer);
        }
    }
    public static Transform FindObjectInChildrenWithTag(Transform founder, string tag, bool includeInactive = false)
    {
        var objs = founder.GetComponentsInChildren<Transform>(includeInactive);
        foreach (var @object in objs)
            if (@object.tag == tag)
            {
                // Debug.Log(@object.name);
                return @object;
            }
        return null;
    }
    public static T GetComponentInOnlyChildren<T>(Transform founder)
    {
        foreach(Transform child in founder)
        {
            if (child == founder) continue;
            return child.GetComponentInChildren<T>();
        }
        return default(T);
    }
    public static void ChangeLayerRecursively(Transform obj, int layer)
    {
        obj.gameObject.layer = layer;

        foreach (Transform child in obj)
        {
            ChangeLayerRecursively(child.gameObject, layer);
        }
    }
    public static List<T> Shuffle<T>(List<T> values)
    {
        for(int i = 0; i < values.Count; i++)
        {
            int rand = UnityEngine.Random.Range(0, i);

            T temp = values[i];
            values[i] = values[rand];
            values[rand] = temp;
        }

        return values;
    }
    public static Queue<T> Shuffle<T>(Queue<T> values)
    {
        List<T> list = new List<T>();
        foreach (T t in values)
            list.Add(t);
        list = Shuffle<T>(list);
        values.Clear();
        foreach (T t in list)
            values.Enqueue(t);
        return values;
    }
    public static T[] Shuffle<T>(T[] values)
    {
        var list = values.ToList<T>();
        list = Shuffle<T>(list);
        for (int i = 0; i < values.Length; i++)
            values[i] = list[i];
        return values;
    }
    public static bool ScreenToCanvasPoint(RectTransform rectTransform, Vector2 screenPoint, Camera camera, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, camera, out localPoint);
    }
    public static List<T> CopyList<T>(List<T> point, List<T> source)
    {
        point.Clear();
        foreach (T t in source)
            point.Add(t);
        return point;
    }
    public static T GetClosestOne<T>(Vector3 standard, List<T> list, float maxDistance = 1000f) where T : Component
    {
        if (list.Count == 0) return null;
        if (list.Count == 1) return list[0];

        float closestDist = maxDistance;
        T closestOne = null;
        foreach (T t in list)
        {
            float dist = Vector3.Distance(standard, t.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestOne = t;
            }
        }
        return closestOne;
    }
    public static bool ArrayContains<T>(T[] array, T sample)
    {
        foreach (T item in array)
            if (item != null && item.Equals(sample))
                return true;
        return false;
    }
    public static Color HexColor(string hexCode)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hexCode, out color))
        {
            return color;
        }

        Debug.LogError("[UnityExtension::HexColor]invalid hex code - " + hexCode);
        return Color.white;
    }
    public static string ToRGBHex(Color c)
    {
        return string.Format("{0:X2}{1:X2}{2:X2}", ToByte(c.r), ToByte(c.g), ToByte(c.b));
    }
    public static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
    /*public static T FlipPairEnum<T>(T value) 
    {
        return;
    }*/
    public static float NormalizeAnglePos(float angle) => (angle + 360f) % 360f;
    public static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
        return angle;
    }
    public static float GetAngleBetweenAngles(float angleA, float angleB)
    {
        float clockwiseAngle = NormalizeAngle(angleB - angleA);
        float counterClockwiseAngle = NormalizeAngle(angleA - angleB);

        if (Mathf.Abs(clockwiseAngle) < Mathf.Abs(counterClockwiseAngle))
        {
            return clockwiseAngle;
        }

        return counterClockwiseAngle;
    }
    public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    public static Vector3 NormalizeEulerAnglesXY(Vector3 eulerAngles)
    {
        eulerAngles.x = NormalizeAngle(eulerAngles.x);
        eulerAngles.y = NormalizeAngle(eulerAngles.y);
        //eulerAngles.z = NormalizeAngle(eulerAngles.z);
        return eulerAngles;
    }
    public static string GetResourcesPath(GameObject prefab)
    {
#if UNITY_EDITOR
        string raw = AssetDatabase.GetAssetPath(prefab);       
        return GetResourcesPath(raw);
#else
        return "";
#endif
    }
    public static string GetResourcesPath(string rawPath)
    {
        var path = new List<string>();
        path.AddRange(rawPath.Split('/'));
        path.RemoveAt(0); path.RemoveAt(0);
        string last = path[path.Count - 1];
        string[] lastSplit = last.Split('.');
        string lastTotal = null;
        for (int i = 0; i < lastSplit.Length; i++)
        {
            if (i < lastSplit.Length - 1)
            {
                lastTotal += lastSplit[i];
                if (i < lastSplit.Length - 2)
                    lastTotal += ".";
            }
        }
        path[path.Count - 1] = lastTotal;
        string total = null;
        for (int i = 0; i < path.Count; i++)
        {
            total += path[i];
            if (i < path.Count - 1)
                total += "/";
        }
        return total;
    }
    public static Vector3 ScreenCenter => new Vector3(Screen.width / 2f, Screen.height / 2f);
}
