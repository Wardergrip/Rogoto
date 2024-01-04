using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivial
{
    public static partial class MonobehaviorExtensions
    {
        /// <summary>
        /// if null, tries to get component, if still null, adds component. Returns component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mb"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static T FindOrAddComponent<T>(this MonoBehaviour mb, T comp) where T : Component
        {
            if (comp != null)
            {
                return comp;
            }
            comp = mb.GetComponent<T>();
            if (comp != null)
            {
                return comp;
            }
            return mb.gameObject.AddComponent<T>();
        }
		#region Active
		public static void SetActiveAll<T>(this List<T> list, bool state) where T: MonoBehaviour
        {
            foreach (T item in list)
            {
                item.gameObject.SetActive(state);
            }
        }
        public static void SetActiveAll<T>(this T[] array, bool state) where T : MonoBehaviour
        {
            foreach (T item in array)
            {
                item.gameObject.SetActive(state);
            }
        }
        public static void SetActiveAll(this List<GameObject> list, bool state) 
        {
            foreach (GameObject item in list)
            {
                item.SetActive(state);
            }
        }
        public static void SetActiveAll(this GameObject[] array, bool state) 
        {
            foreach (GameObject item in array)
            {
                item.gameObject.SetActive(state);
            }
        }
        public static void SetActiveAll(this List<Transform> list, bool state)
        {
            foreach (Transform item in list)
            {
                item.gameObject.SetActive(state);
            }
        }
        public static void SetActiveAll(this Transform[] array, bool state)
        {
            foreach (Transform item in array)
            {
                item.gameObject.SetActive(state);
            }
        }
		public static void EnableScript<T>(this T obj) where T : MonoBehaviour
		{
			obj.enabled = true;
		}
		public static void DisableScript<T>(this T obj) where T : MonoBehaviour
		{
			obj.enabled = false;
		}
		#endregion Active
		#region Children
		public static void ForEachChild(this Transform transform, Action<Transform, int> action)
		{
			for (int i = 0; i < transform.childCount; ++i)
			{
				action(transform.GetChild(i), i);
			}
		}
		public static void ForEachChild(this MonoBehaviour mb, Action<Transform,int> action)
        {
            ForEachChild(mb.transform, action);
        }
        public static void ForEachChild(this GameObject go, Action<Transform,int> action)
        {
            ForEachChild(go.transform, action);
        }
		public static List<Transform> AllChildren(this Transform transform)
		{
			List<Transform> list = new();
			foreach (Transform t in transform)
			{
				list.Add(t);
			}
			return list;
		}
		#endregion Childrens
		#region Lerp
		public static IEnumerator<Transform> LerpRotationToTargetPosition(this Transform transform,
	Vector3 targetPosition, float lerpSpeed, Action endLerpAction = null)
		{
			float elapsedTime = 0;
			Vector3 direction = (targetPosition - transform.position).normalized;
			while (Vector3.Dot(transform.forward, direction) < 0.999f)
			{
				Quaternion toRotation = Quaternion.LookRotation(direction);
				transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, lerpSpeed * elapsedTime);
				yield return null;
				elapsedTime += Time.deltaTime;
			}
			if (endLerpAction != null)
				endLerpAction();

		}
		public static Vector3 Lerp(this List<Vector3> path, float t)
        {
			Debug.Assert(path != null,"LerpList: Path is null");
			Debug.Assert(path.Count > 0, "LerpList: Path is empty");

			// Clamp t between 0 and 1
			t = Mathf.Clamp01(t);

			// Calculate the index of the first and second points for interpolation
			int index1 = Mathf.FloorToInt(t * (path.Count - 1));
			int index2 = Mathf.CeilToInt(t * (path.Count - 1));

			// Ensure the indices are within bounds
			index1 = Mathf.Clamp(index1, 0, path.Count - 1);
			index2 = Mathf.Clamp(index2, 0, path.Count - 1);

			// Perform linear interpolation between the two points
			Vector3 point1 = path[index1];
			Vector3 point2 = path[index2];
			return Vector3.Lerp(point1, point2, t - index1);
		}

		public static void Lerp(this List<Transform> path, float t, Transform outVal)
		{
			Debug.Assert(path != null, "LerpList: Path is null");
			Debug.Assert(path.Count > 0, "LerpList: Path is empty");

			// Clamp t between 0 and 1
			t = Mathf.Clamp01(t);

			// Calculate the index of the first and second points for interpolation
			int index1 = Mathf.FloorToInt(t * (path.Count - 1));
			int index2 = Mathf.CeilToInt(t * (path.Count - 1));

			// Ensure the indices are within bounds
			index1 = Mathf.Clamp(index1, 0, path.Count - 1);
			index2 = Mathf.Clamp(index2, 0, path.Count - 1);

			float segmentT = t * (path.Count - 1) - index1;

			// Perform linear interpolation between the two points
			Transform point1 = path[index1];
			Transform point2 = path[index2];
			outVal.SetPositionAndRotation(
                Vector3.Lerp(point1.position, point2.position, segmentT), 
                Quaternion.Lerp(point1.rotation, point2.rotation, segmentT)
                );
			outVal.localScale = Vector3.Lerp(point1.localScale, point2.localScale, segmentT);
        }

        public static Vector3 LerpPositions(this List<Transform> path, float t)
        {
            Debug.Assert(path != null, "LerpList: Path is null");
            Debug.Assert(path.Count > 0, "LerpList: Path is empty");

            // Clamp t between 0 and 1
            t = Mathf.Clamp01(t);

            // Calculate the index of the first and second points for interpolation
            int index1 = Mathf.FloorToInt(t * (path.Count - 1));
            int index2 = Mathf.CeilToInt(t * (path.Count - 1));

            // Ensure the indices are within bounds
            index1 = Mathf.Clamp(index1, 0, path.Count - 1);
            index2 = Mathf.Clamp(index2, 0, path.Count - 1);

            // Perform linear interpolation between the two points
            Vector3 point1 = path[index1].position;
            Vector3 point2 = path[index2].position;
            return Vector3.Lerp(point1, point2, t - index1);
        }
		#endregion Lerp
	}
}