using System.Collections.Generic;
using UnityEngine;

namespace Trivial
{
    public static partial class ClosestExtensions
    {

        public static T FindClosest<T>(this List<T> list, Vector3 position) where T : MonoBehaviour
        {
            float closestDistance = float.MaxValue;
            T closestObj = null;

            foreach (T t in list)
            {
                if (t == null) continue;

                float distance = (t.transform.position - position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestObj = t;
                    closestDistance = distance;
                }
            }

            return closestObj;
        }
        public static Vector3 FindClosest(this Vector3[] array, Vector3 position)
        {
            float closestDistance = float.MaxValue;
            Vector3 closestPoint= Vector3.zero;

            foreach (Vector3 pos in array)
            {
                float distance = (pos - position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestPoint = pos;
                    closestDistance = distance;
                }
            }

            return closestPoint;
        }
        public static GameObject FindClosest(this List<GameObject> list, Vector3 position)
        {
            float closestDistance = float.MaxValue;
            GameObject closestObj = null;

            foreach (GameObject obj in list)
            {
                if (obj == null) continue;

                float distance = (obj.transform.position - position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestObj = obj;
                    closestDistance = distance;
                }
            }

            return closestObj;
        }
        public static Transform FindClosest(this List<Transform> list, Vector3 position)
        {
            float closestDistance = float.MaxValue;
            Transform closestObj = null;

            foreach (Transform obj in list)
            {
                if (obj == null) continue;

                float distance = (obj.transform.position - position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestObj = obj;
                    closestDistance = distance;
                }
            }

            return closestObj;
        }
        /// <summary>
        /// Sorts from closest to furthest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static List<T> SortByDistance<T>(this List<T> list, Vector3 targetPosition) where T : MonoBehaviour
        {
            List<T> sortedList = new(list);

            sortedList.Sort((a, b) =>
            {
                float distanceToA = Vector3.Distance(a.transform.position, targetPosition);
                float distanceToB = Vector3.Distance(b.transform.position, targetPosition);
                return distanceToA.CompareTo(distanceToB);
            });

            return sortedList;
        }
        /// <summary>
        /// Sorts from closest to furthest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static List<Transform> SortByDistance(this List<Transform> list, Vector3 targetPosition)
        {
            List<Transform> sortedList = new(list);

            sortedList.Sort((a, b) =>
            {
                float distanceToA = Vector3.Distance(a.position, targetPosition);
                float distanceToB = Vector3.Distance(b.position, targetPosition);
                return distanceToA.CompareTo(distanceToB);
            });

            return sortedList;
        }
        /// <summary>
        /// Sorts from closest to furthest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static List<GameObject> SortByDistance(this List<GameObject> list, Vector3 targetPosition)
        {
            List<GameObject> sortedList = new(list);

            sortedList.Sort((a, b) =>
            {
                float distanceToA = Vector3.Distance(a.transform.position, targetPosition);
                float distanceToB = Vector3.Distance(b.transform.position, targetPosition);
                return distanceToA.CompareTo(distanceToB);
            });

            return sortedList;
        }
    }
}