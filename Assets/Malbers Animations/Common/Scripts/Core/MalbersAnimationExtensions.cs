using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    public static class MalbersAnimationsExtensions
    {
        #region Types
        public static bool IsSubclassDeep(this Type type, Type parenType)
        {
            while (type != null)
            {
                if (type.IsSubclassOf(parenType))
                    return true;
                type = type.BaseType;
            }

            return false;
        }

        public static bool TryGetGenericTypeOfDefinition(this Type type, Type genericTypeDefinition,
            out Type generictype)
        {
            generictype = null;
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    generictype = type;
                    return true;
                }
                type = type.BaseType;
            }

            return false;
        }

        public static bool IsSubclassOfGenericTypeDefinition(this Type t, Type genericTypeDefinition)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new Exception("genericTypeDefinition parameter isn't generic type definition");
            }
            if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                return true;
            }
            else
            {
                t = t.BaseType;
                while (t != null)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == genericTypeDefinition)
                        return true;

                    t = t.BaseType;
                }
            }

            return false;
        }
        #endregion

        #region NullCheck
        /// <summary> Use this to check if a Unity Object is null or not. Usage:  [ a=b.Ref() ?? c; ] </summary>
        public static T Ref<T>(this T o) where T : UnityEngine.Object
        {
            return o == null ? null : o;
        }
        #endregion

        #region Float Int
        public static bool CompareFloat(this float current, float newValue, ComparerInt comparer)
        {
            switch (comparer)
            {
                case ComparerInt.Equal:
                    return (current == newValue);
                case ComparerInt.Greater:
                    return (current > newValue);
                case ComparerInt.Less:
                    return (current < newValue);
                case ComparerInt.NotEqual:
                    return (current != newValue);
                default:
                    return false;
            }
        }
        public static bool CompareInt(this int current, int newValue, ComparerInt comparer)
        {
            switch (comparer)
            {
                case ComparerInt.Equal:
                    return (current == newValue);
                case ComparerInt.Greater:
                    return (current > newValue);
                case ComparerInt.Less:
                    return (current < newValue);
                case ComparerInt.NotEqual:
                    return (current != newValue);
                default:
                    return false;
            }
        }
        public static bool InRange(this float current, float min, float max) => current >= min && current <= max;
        public static bool InRange(this int current, float min, float max) => current >= min && current <= max;

        #endregion

        /// <summary> Same as StartCoroutine but it also stores the coroytine in an IEnumerator </summary>
        public static void StartCoroutine(this MonoBehaviour Mono, out IEnumerator Cor, IEnumerator newCoro)
        {
            Cor = null;
            if (Mono.gameObject.activeInHierarchy)
            {
                Cor = newCoro;
                Mono.StartCoroutine(Cor);
            }
        }

        #region Vector3
        /// <summary>A useful Epsilon</summary>
        public const float Epsilon = 0.0001f;

        /// <summary>Round Decimal Places on a Vector</summary>
        public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
            {
                multiplier *= 10f;
            }
            return new Vector3(
                Mathf.Round(vector3.x * multiplier) / multiplier,
                Mathf.Round(vector3.y * multiplier) / multiplier,
                Mathf.Round(vector3.z * multiplier) / multiplier);
        }

        /// <summary>Checks if a vector is close to Vector3.zero</summary>
        public static bool CloseToZero(this Vector3 v, float threshold = 0.0001f) => v.sqrMagnitude < threshold * threshold;

        /// <summary> Get the closest point on a line segment. </summary>
        /// <param name="p">A point in space</param>
        /// <param name="s0">Start of line segment</param>
        /// <param name="s1">End of line segment</param>
        /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
        public static Vector3 ClosestPointOnLine(this Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 aB = b - a;
            Vector3 aP = point - a;
            float sqrLenAB = aB.sqrMagnitude;

            if (sqrLenAB < Epsilon) return a;

            float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
            return a + (aB * t);
        }


        /// <summary>Get the closest point (0-1) on a line segment</summary>
        /// <param name="p">A point in space</param>
        /// <param name="s0">Start of line segment</param>
        /// <param name="s1">End of line segment</param>
        /// <returns>The interpolation parameter representing the point on the segment, with 0==s0, and 1==s1</returns>
        public static float ClosestTimeOnSegment(this Vector3 p, Vector3 s0, Vector3 s1)
        {
            Vector3 s = s1 - s0;
            float len2 = Vector3.SqrMagnitude(s);
            if (len2 < Epsilon)
                return 0; // degenrate segment
            return Mathf.Clamp01(Vector3.Dot(p - s0, s) / len2);
        }


        /// <summary> Calculate the Direction from an Origin to a Target or Destination  </summary>
        public static Vector3 DirectionTo(this Vector3 origin, Vector3 destination) => Vector3.Normalize(destination - origin);

        /// <summary>returns the delta position from a rotation.</summary>
        public static Vector3 DeltaPositionFromRotate(this Transform transform, Vector3 point, Vector3 axis, float deltaAngle)
        {
            var pos = transform.position;
            var direction = pos - point;
            var rotation = Quaternion.AngleAxis(deltaAngle, axis);
            direction = rotation * direction;

            pos = point + direction - pos;
            pos.y = 0;                                                      //the Y is handled by the Fix Position method

            return pos;
        }

        /// <summary>returns the delta position from a rotation.</summary>
        public static Vector3 DeltaPositionFromRotate(this Transform transform, Vector3 platform, Quaternion deltaRotation)
        {
            var pos = transform.position;

            var direction = pos - platform;
            var directionAfterRotation = deltaRotation * direction;

            var NewPoint = platform + directionAfterRotation;


            pos = NewPoint - transform.position;

            return pos;
        }

        /// <summary>  Returns if a point is inside a Sphere Radius </summary>
        /// <param name="point">Point you want to find inside a sphere</param>
        public static bool PointInsideSphere(this Vector3 point, Vector3 sphereCenter, float sphereRadius)
        {
            Vector3 direction = point - sphereCenter;
            float distanceSquared = direction.sqrMagnitude;
            return (distanceSquared <= sphereRadius * sphereRadius);
        }


        #endregion

        #region Transforms
        /// <summary> Find the first transform grandchild with this name inside this transform</summary>
        public static Transform FindGrandChild(this Transform aParent, string aName)
        {
            var result = aParent.ChildContainsName(aName);

            if (result != null) return result;

            foreach (Transform child in aParent)
            {
                result = child.FindGrandChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>  Returns the Real Transform Core   </summary> 
        public static Transform FindObjectCore(this Transform transf)
        {
            var core = transf;
            var IsObjectCore = core.FindInterface<IObjectCore>();
            if (IsObjectCore != null) return IsObjectCore.transform;

            return core;
        }


        /// <summary> Find the if a Transform is in the same hierachy of a another. Returns true also if the Child = Parent</summary>
        public static bool SameHierarchy(this Transform child, Transform Parent)
        {
            if (child == Parent) return true; //Include yourself!! IMPORTANT
            if (child.parent == null) return false;
            if (child.parent == Parent) return true;

            return SameHierarchy(child.parent, Parent);
        }

        [Obsolete("Use [SameHierarchy] Instead")]
        public static bool IsGrandchild(this Transform child, Transform Parent) => SameHierarchy(child, Parent);


        /// <summary> Calculate the Direction from an Origin to a Target or Destination  </summary>
        public static Vector3 DirectionTo(this Transform origin, Transform destination) => DirectionTo(origin.position, destination.position);
        /// <summary> Calculate the Direction from an Origin to a Target or Destination  </summary>
        public static Vector3 DirectionTo(this Transform origin, Vector3 destination) => DirectionTo(origin.position, destination);


        /// <summary> Find the closest transform from the origin </summary>
        public static Transform NearestTransform(this Transform origin, params Transform[] transforms)
        {
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = origin.position;
            foreach (Transform potentialTarget in transforms)
            {
                Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }

            return bestTarget;
        }


        /// <summary> Find the farest transform from the origin </summary>
        public static Transform FarestTransform(this Transform t, params Transform[] transforms)
        {
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = t.position;
            foreach (Transform potentialTarget in transforms)
            {
                Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget > closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }

            return bestTarget;
        }

        public static Transform ChildContainsName(this Transform aParent, string aName)
        {
            foreach (Transform child in aParent)
            {
                if (child.name.Contains(aName))
                    return child;
            }
            return null;
        }

        /// <summary>Resets the Local Position and rotation of a transform</summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>Resets the Local Position and rotation of a transform</summary>
        public static void SetLocalTransform(this Transform transform, Vector3 LocalPos, Vector3 LocalRot, Vector3 localScale)
        {
            transform.localPosition = LocalPos;
            transform.localEulerAngles = LocalRot;
            transform.localScale = localScale;
        }

        /// <summary>Resets the Local Position and rotation of a transform</summary>
        public static void SetLocalTransform(this Transform transform, TransformOffset offset)
        {
            transform.localPosition = offset.Position;
            transform.localEulerAngles = offset.Rotation;
            transform.localScale = offset.Scale;
        }

        /// <summary>Parent a transform to another Transform, and Solves the Scale problem in case the Parent has a deformed scale  </summary>
        /// <param name="parent">Transform to be the parent</param>
        /// <param name="Position">Relative position to the Parent (World Position)</param>
        public static Transform SetParentScaleFixer(this Transform transform, Transform parent, Vector3 Position)
        {
            if (parent.lossyScale.x == parent.lossyScale.y && parent.lossyScale.x == parent.lossyScale.z) //Check if the Scale is Uniform
            {
                transform.SetParent(parent, true);
                transform.position = Position;
                return null;
            }

            Vector3 NewScale = parent.transform.lossyScale;
            NewScale.x = 1f / Mathf.Max(NewScale.x, Epsilon);
            NewScale.y = 1f / Mathf.Max(NewScale.y, Epsilon);
            NewScale.z = 1f / Mathf.Max(NewScale.z, Epsilon);

            GameObject Hlper = new GameObject { name = transform.name + "Link" };

            //  Debug.Log("Hlper = " + Hlper);

            Hlper.transform.SetParent(parent);
            Hlper.transform.localScale = NewScale;
            Hlper.transform.position = Position;
            Hlper.transform.localRotation = Quaternion.identity;

            transform.SetParent(Hlper.transform);
            transform.localPosition = Vector3.zero;
            return Hlper.transform;
        }

        #endregion

        #region String
        public static string RemoveSpecialCharacters(this string str)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>  Resize a List </summary>
        public static void Resize<T>(this List<T> list, int size, T element = default(T))
        {
            int count = list.Count;

            if (size < count)
            {
                list.RemoveRange(size, count - size);
            }
            else if (size > count)
            {
                if (size > list.Capacity)   // Optimization
                    list.Capacity = size;

                list.AddRange(Enumerable.Repeat(element, size - count));
            }
        }


        /// <summary>  Resize a Listener Number from a Unity Event Base </summary>
        public static int GetListenerNumber(this UnityEventBase unityEvent)
        {
            var field = typeof(UnityEventBase).GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var invokeCallList = field.GetValue(unityEvent);
            var property = invokeCallList.GetType().GetProperty("Count");
            return (int)property.GetValue(invokeCallList);
        }


        #region GameObjects
        /// <summary>The GameObject is a prefab, Meaning in not in any scene</summary>
        public static bool IsPrefab(this GameObject go) => !go.scene.IsValid();

        #endregion


        #region Delay Action

        /// <summary>Do an action the next frame</summary>
        public static IEnumerator Delay_Action(this MonoBehaviour mono, Action action) => Delay_Action(mono, (int)1, action);

        /// <summary>Do an action the next given frames</summary>
        public static IEnumerator Delay_Action(this MonoBehaviour mono, int frames, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(frames, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        /// <summary>If the action is active stop it!</summary>
        public static void Stop_Action(this MonoBehaviour mono, IEnumerator action)
        {
            if (action != null) mono.StopCoroutine(action);
        }

   

        /// <summary>Do an action after certain time</summary>
        public static IEnumerator Delay_Action(this MonoBehaviour mono, float time, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(time, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        /// <summary>Do an action after certain time and stop an oldone</summary>
        public static void Delay_Action(this MonoBehaviour mono, ref IEnumerator oldAction, float time, Action action)
        {
            if (oldAction != null) mono.StopCoroutine(oldAction);
            oldAction = Delay_Action(mono, time, action);
        }

        public static IEnumerator Delay_Action(this MonoBehaviour mono, Func<bool> Condition, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(Condition, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        public static IEnumerator Delay_Action(this MonoBehaviour mono, WaitForSeconds time, Action action)
        {
            if (mono.enabled && mono.gameObject.activeInHierarchy)
            {
                var coro = DelayedAction(time, action);
                mono.StartCoroutine(coro);

                return coro;
            }
            return null;
        }

        private static IEnumerator DelayedAction(int frame, Action action)
        {
            for (int i = 0; i < frame; i++)
                yield return null;

            action.Invoke();
        }


        private static IEnumerator DelayedAction(Func<bool> Condition, Action action)
        {
            yield return new WaitWhile(Condition);
            action.Invoke();
        }


        private static IEnumerator DelayedAction(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action.Invoke();
        }

        private static IEnumerator DelayedAction(WaitForSeconds time, Action action)
        {
            yield return time;
            action.Invoke();
        }

        #endregion

        #region Find Components/Interfaces
        public static T CopyComponent<T>(this T original, GameObject destination) where T : Component
        {
            Type type = original.GetType();

            Component copy = destination.AddComponent(type);

            var fields = type.GetFields();

            foreach (System.Reflection.FieldInfo field in fields)
                field.SetValue(copy, field.GetValue(original));

            return copy as T;
        }

        public static T FindComponent<T>(this GameObject c) where T : Component
        {
            T Ttt = c.GetComponent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInParent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }

     

        public static Component FindComponent(this GameObject c, Type t)
        {
            var Ttt = c.GetComponent(t);
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInParent(t);
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren(t, true);
            if (Ttt != null) return Ttt;

            return default;
        }

        public static T[] FindComponents<T>(this GameObject c) where T : Component
        {
            T[] Ttt = c.GetComponents<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentsInParent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentsInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }

        /// <summary>Search for the Component in the root of the Object </summary>
        public static T FindComponentInRoot<T>(this GameObject c) where T : Component
        {
            var root = c.transform.root;

            if (root.TryGetComponent<T>(out var Ttt)) return Ttt;

            Ttt = c.GetComponentInParent<T>();
            if (Ttt != null) return Ttt;

            Ttt = root.GetComponentInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }


        public static T FindInterface<T>(this GameObject c)
        {
            T Ttt = c.GetComponent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInParent<T>(true);
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren<T>(true);
            if (Ttt != null) return Ttt;

            return default;
        }

        public static T FindInterface<T>(this GameObject c, bool includeInactive)
        {
            T Ttt = c.GetComponent<T>();
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInParent<T>(includeInactive);
            if (Ttt != null) return Ttt;

            Ttt = c.GetComponentInChildren<T>(includeInactive);
            if (Ttt != null) return Ttt;

            return default;
        }

        public static T[] FindInterfaces<T>(this GameObject c)
        {
            T[] Ttt = c.GetComponents<T>();
            if (Ttt != null && Ttt.Length > 0) return Ttt;

            Ttt = c.GetComponentsInParent<T>();
            if (Ttt != null && Ttt.Length > 0) return Ttt;

            Ttt = c.GetComponentsInChildren<T>(true);
            if (Ttt != null && Ttt.Length > 0) return Ttt;

            return default;
        }

        /// <summary>Search for the Component in the hierarchy Up or Down</summary>
        public static T FindComponent<T>(this Component c) where T : Component => c.gameObject.FindComponent<T>();

        public static T FindInterface<T>(this Component c) => c.gameObject.FindInterface<T>();
        public static T FindInterface<T>(this Component c, bool includeInactive) => c.gameObject.FindInterface<T>(includeInactive);
        public static T[] FindInterfaces<T>(this Component c) => c.gameObject.FindInterfaces<T>();

        /// <summary>Search for the Component in the root of the Object </summary>
        public static T FindComponentInRoot<T>(this Component c) where T : Component => c.gameObject.FindComponentInRoot<T>();


        /// <summary> Uses Getcomponent in childern but with a string</summary>
        public static Component GetComponentInChildren(this Component owner, string classtype)
        {
            var sender = owner.GetComponent(classtype);
            if (sender) return sender;
            else
            {
                foreach (Transform item in owner.transform)
                {
                    var found = item.GetComponentInChildren(classtype);
                    if (found) return found;
                }
            }

            return null;
        }

        /// <summary> Uses GetComponent in Parent but with a string</summary>
        public static Component GetComponentInParent(this Component owner, string classtype)
        {
            var sender = owner.GetComponent(classtype);

            if (sender != null)
            {
                return sender;
            }
            else
            {
                if (owner.transform.parent == null)
                {
                    return null;
                }
                else
                {
                    return owner.transform.parent.GetComponentInParent(classtype);
                }
            }
        }

        /// <summary> Gets a real copy of a component / </summary>
        private static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                    catch { } 
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }


        public static T AddCopyComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }


        /// <summary>  Reset the delta RootMotion of the Animator  </summary>
        public static IDeltaRootMotion TryDeltaRootMotion(this Component c)
        {
            if (c.TryGetComponent(out IDeltaRootMotion target))
            {
                target.ResetDeltaRootMotion();
                return target;
            }
            return null;
        }

        #endregion

        /// <summary>  Checks if a GameObject has been destroyed. </summary>
        /// <param name="gameObject">GameObject reference to check for destructedness</param>
        /// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
        public static bool IsDestroyed(this GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }


        #region Reflections

        public static UnityAction<T> CreateDelegate<T>(object target, MethodInfo method)
        {
            var del = (UnityAction<T>)Delegate.CreateDelegate(typeof(UnityAction<T>), target, method);
            return del;
        }

        /// <summary>Converts a Method Info into a Unity Action</summary>
        public static UnityAction CreateDelegate(object target, MethodInfo method)
        {
            var del = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method);
            return del;
        }

        /// <summary> Returns a Unity Action from a component and a method. Used to connect methods in the inspector </summary>
        public static UnityAction GetUnityAction(this Component c, string component, string method)
        {
            var sender = c.GetComponent(component);
            if (sender == null) sender = c.GetComponentInParent(Type.GetType(component));
            if (sender == null) sender = c.GetComponentInChildren(Type.GetType(component));

            MethodInfo methodPtr;

            //Debug.Log("sender = " + sender);

            if (sender != null)
            {
                methodPtr = sender.GetType().GetMethod(method, new Type[0]);
            }
            else return null;

            if (methodPtr != null)
            {
                // Debug.Log("methodPtr = " + methodPtr.Name);
                var action = CreateDelegate(sender, methodPtr);
                return (action);
            }

            return null;
        }

        public static Type FindType(string qualifiedTypeName)
        {
            Type t = Type.GetType(qualifiedTypeName);

            if (t != null)
            {
                return t;
            }
            else
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetType(qualifiedTypeName);
                    if (t != null)
                        return t;
                }
                return null;
            }
        }

        public static UnityAction<T> GetUnityAction<T>(this Component c, string component, string method)
        {
            if (string.IsNullOrEmpty(component)) return null;

            var sender = c.GetComponent(component);
            if (sender == null) sender = c.GetComponentInParent(component);
            if (sender == null) sender = c.GetComponentInChildren(component);
            if (sender == null) return null;

            var methodPtr = sender.GetType().GetMethod(method, new Type[] { typeof(T) });

            if (methodPtr != null)
            {
                var action = CreateDelegate<T>(sender, methodPtr);
                return (action);
            }

            PropertyInfo property = sender.GetType().GetProperty(method);

            if (property != null)
            {
                var action = CreateDelegate<T>(sender, property.SetMethod);
                return (action);
            }

            return null;
        }


        #endregion


        public static T GetFieldClass<T>(this Component owner, string component, string field) where T : class
        {
            var sender = owner.GetComponent(component);

            if (sender != null)
            {
                FieldInfo methodPtr = sender.GetType().GetField(field, BindingFlags.Public | BindingFlags.Instance);

                if (methodPtr != null)
                {
                    return methodPtr.GetValue(sender) as T;
                }
            }
            return null;
        }


        /// <summary> Invoke with Parameters </summary>
        public static bool InvokeWithParams(this MonoBehaviour sender, string method, object args)
        {
            Type argType = null;

            if (args != null) argType = args.GetType();

            MethodInfo methodPtr;

            if (argType != null)
            {
                methodPtr = sender.GetType().GetMethod(method, new Type[] { argType });
            }
            else
            {
                try
                {
                    methodPtr = sender.GetType().GetMethod(method);
                }
                catch (Exception)
                {
                    //methodPtr = sender.GetType().GetMethods().First
                    //(m => m.Name == method && m.GetParameters().Count() == 0);

                    //Debug.Log("OTHER");

                    throw;
                }

            }

            if (methodPtr != null)
            {
                if (args != null)
                {
                    var arguments = new object[1] { args };
                    methodPtr.Invoke(sender, arguments);
                    return true;
                }
                else
                {
                    methodPtr.Invoke(sender, null);
                    return true;
                }
            }

            PropertyInfo property = sender.GetType().GetProperty(method);

            if (property != null)
            {
                property.SetValue(sender, args, null);
                return true;

            }
            return false;
        }


        /// <summary>Invoke with Parameters and Delay </summary>
        public static void InvokeDelay(this MonoBehaviour behaviour, string method, object options, YieldInstruction wait)
        {
            behaviour.StartCoroutine(_invoke(behaviour, method, wait, options));
        }

        private static IEnumerator _invoke(this MonoBehaviour behaviour, string method, YieldInstruction wait, object options)
        {
            yield return wait;

            Type instance = behaviour.GetType();
            MethodInfo mthd = instance.GetMethod(method);
            mthd.Invoke(behaviour, new object[] { options });

            yield return null;
        }


        /// <summary>Invoke with Parameters for Scriptable objects</summary>
        public static void Invoke(this ScriptableObject sender, string method, object args)
        {
            var methodPtr = sender.GetType().GetMethod(method);

            if (methodPtr != null)
            {
                if (args != null)
                {
                    var arguments = new object[1] { args };
                    methodPtr.Invoke(sender, arguments);
                }
                else
                {
                    methodPtr.Invoke(sender, null);
                }
            }
        }


        #region Layers and Colliders
        /// <summary> Changes the Layer of a GameObject and its children.  </summary>
        public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true)
        {
            parent.layer = layer;
            if (includeChildren)
            {
                foreach (var trans in parent.transform.GetComponentsInChildren<Transform>(true))
                    trans.gameObject.layer = layer;
            }
        }

        #endregion

        #region SetEnable
        /// <summary>Enable disable the Mono</summary>
        public static void SetEnable(this MonoBehaviour c, bool enable) => c.enabled = enable;
        /// <summary>Enable disable the Mono</summary>
        public static void SetEnable(this Collider c, bool enable) => c.enabled = enable;
        #endregion
    }
}