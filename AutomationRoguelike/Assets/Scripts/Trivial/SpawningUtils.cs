using System.Runtime.CompilerServices;
using UnityEngine;

namespace Trivial
{
    public enum LockAxis : byte
    {
        None = 0x00,
        X = 0x01,
        Y = 0x02,
        Z = 0x04,
        XY = 0x03,
        YZ = 0x06,
        XZ = 0x05, 
        All = 0x07
    }
    public static class LockAxisHelper 
    {
        public static bool IsLockedOn(this LockAxis thisLockaxis, LockAxis axis)
        {
			return (thisLockaxis & axis) == axis;
		}
    }

    public static class SpawningUtils
    {
        // https://discussions.unity.com/t/instantiate-inactive-object/39830/3

        /// <summary>
        /// Will instantiate an object disabled preventing it from calling Awake/OnEnable.
        /// </summary>
        public static T InstantiateDisabled<T>(this T original, Transform parent = null, bool worldPositionStays = false) where T : Object
        {
            if (!GetActiveState(original))
            {
                return Object.Instantiate(original, parent, worldPositionStays);
            }

            (GameObject coreObject, Transform coreObjectTransform) = CreateDisabledCoreObject(parent);
            T instance = Object.Instantiate(original, coreObjectTransform, worldPositionStays);
            SetActiveState(instance, false);
            SetParent(instance, parent, worldPositionStays);
            Object.Destroy(coreObject);
            return instance;
        }

        /// <summary>
        /// Will instantiate an object disabled preventing it from calling Awake/OnEnable.
        /// </summary>
        public static T InstantiateDisabled<T>(this T original, Vector3 position, Quaternion rotation, Transform parent = null) where T : Object
        {
            if (!GetActiveState(original))
            {
                return Object.Instantiate(original, position, rotation, parent);
            }

            (GameObject coreObject, Transform coreObjectTransform) = CreateDisabledCoreObject(parent);
            T instance = Object.Instantiate(original, position, rotation, coreObjectTransform);
            SetActiveState(instance, false);
            SetParent(instance, parent, false);
            Object.Destroy(coreObject);
            return instance;
        }

        private static (GameObject coreObject, Transform coreObjectTransform) CreateDisabledCoreObject(Transform parent = null)
        {
            GameObject coreObject = new GameObject(string.Empty);
            coreObject.SetActive(false);
            Transform coreObjectTransform = coreObject.transform;
            coreObjectTransform.SetParent(parent);

            return (coreObject, coreObjectTransform);
        }

        private static bool GetActiveState<T>(T obj) where T : Object
        {
            switch (obj)
            {
                case GameObject gameObject:
                    {
                        return gameObject.activeSelf;
                    }
                case Component component:
                    {
                        return component.gameObject.activeSelf;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        private static void SetActiveState<T>(T obj, bool state) where T : Object
        {
            switch (obj)
            {
                case GameObject gameObject:
                    {
                        gameObject.SetActive(state);

                        break;
                    }
                case Component component:
                    {
                        component.gameObject.SetActive(state);

                        break;
                    }
            }
        }

        private static void SetParent<T>(T obj, Transform parent, bool worldPositionStays) where T : Object
        {
            switch (obj)
            {
                case GameObject gameObject:
                    {
                        gameObject.transform.SetParent(parent, worldPositionStays);
                        break;
                    }
                case Component component:
                    {
                        component.transform.SetParent(parent, worldPositionStays);
                        break;
                    }
            }
        }

        /// <summary>
        /// Makes the rotation random. The axis you choose to lock will be unnaffected
        /// </summary>
        /// <param name="quaternion"></param>
        /// <param name="lockAxis">The axis you choose to lock will be unnaffected</param>
        /// <returns>this quaternion</returns>
        public static Quaternion MakeRandom(this Quaternion quaternion, LockAxis lockAxis = LockAxis.None)
        {
            

            // Actual Code
            if (lockAxis == LockAxis.All) return quaternion;

            quaternion = Quaternion.Euler(
                lockAxis.IsLockedOn(LockAxis.X) ? quaternion.eulerAngles.x : Random.Range(0, 360.0f),
                lockAxis.IsLockedOn(LockAxis.Y) ? quaternion.eulerAngles.y : Random.Range(0, 360.0f),
                lockAxis.IsLockedOn(LockAxis.Z) ? quaternion.eulerAngles.z : Random.Range(0, 360.0f)
                );
            return quaternion;
        }
    }
}
