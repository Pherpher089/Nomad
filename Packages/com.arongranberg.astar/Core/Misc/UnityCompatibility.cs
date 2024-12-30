using UnityEngine;

namespace Pathfinding.Util {
    /// <summary>Compatibility class for Unity APIs that are not available in all Unity versions</summary>
    public static class UnityCompatibility {
        public static T[] FindObjectsByTypeSorted<T>() where T : Object {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsSortMode.InstanceID);
#elif UNITY_2021_3_OR_NEWER
            return Object.FindObjectsOfType<T>();
#else
            return (T[])Object.FindObjectsOfType(typeof(T));
#endif
        }

        public static T[] FindObjectsByTypeUnsorted<T>() where T : Object {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#elif UNITY_2021_3_OR_NEWER
            return Object.FindObjectsOfType<T>();
#else
            return (T[])Object.FindObjectsOfType(typeof(T));
#endif
        }

        public static T[] FindObjectsByTypeUnsortedWithInactive<T>() where T : Object {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#elif UNITY_2021_3_OR_NEWER
            return Object.FindObjectsOfType<T>(true);
#else
            // Include inactive objects not supported in older versions
            return (T[])Object.FindObjectsOfType(typeof(T));
#endif
        }

        public static T FindAnyObjectByType<T>() where T : Object {
#if UNITY_2023_1_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#elif UNITY_2021_3_OR_NEWER
            return Object.FindObjectOfType<T>();
#else
            return (T)Object.FindObjectOfType(typeof(T));
#endif
        }
    }
}

#if !UNITY_2022_3_OR_NEWER
namespace Pathfinding {
    public class IgnoredByDeepProfilerAttribute : System.Attribute {
    }
}
#endif
