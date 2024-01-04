using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class LerpToTargetFunction
{
    public static IEnumerator LerpToPosition(this Transform transform, Vector3 position, float lerpDuration, Action reachedTarget = null)
    {
        float lerpTimeElasped = 0;
        Vector3 startLerpPosition = transform.position;

        while (lerpTimeElasped < lerpDuration)
        {
            float t = lerpTimeElasped / lerpDuration;
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startLerpPosition, position, t);
            lerpTimeElasped += Time.deltaTime;
            yield return null;
        }
        if (reachedTarget != null)
        {
            reachedTarget();
        }
    }
    public static IEnumerator LerpToTarget(this Transform transform, Transform target, float lerpDuration, Action reachedTarget =null, bool flattenYToZero= false)
    {
        float lerpTimeElasped=0;
        Vector3 startLerpPosition = transform.position;
        
        while (lerpTimeElasped < lerpDuration)
        {
            if (target == null)
                yield break;
            Vector3 targetPos = flattenYToZero ? new Vector3(target.position.x,0,target.position.z) : target.position;
            float t = lerpTimeElasped / lerpDuration;
            t = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startLerpPosition, targetPos, t);
            lerpTimeElasped += Time.deltaTime;
            yield return null;
        }
        if (reachedTarget != null)
        {
            reachedTarget();
        }
    }
    public static IEnumerator FollowTarget(this Transform transform, Transform target, bool flattenYToZero = false)
    {
        while(target!=null) 
        {
            Vector3 targetPos = flattenYToZero ? new Vector3(target.position.x, 0, target.position.z) : target.position;
            transform.position = targetPos;
            yield return null;
        }
    }
}
public class MoveToUIElement : MonoBehaviour
{
    public event Action OnReachedTarget;
    public Transform Target;
    
    [SerializeField] private  float _lerpDuration = 1.5f;
    
    
    

    private void Start()
    {
        OnReachedTarget += ReachedTarget;
        StartCoroutine(transform.LerpToTarget(Target,_lerpDuration,()=>OnReachedTarget.Invoke()));
    }
    private void ReachedTarget()
    {
        Destroy(this.gameObject);

        
    }
    
}
