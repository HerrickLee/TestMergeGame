using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 清除动画组件
/// </summary>
public class ClearSquare : MonoBehaviour
{
    public AnimationClip clearAnimation;
    private bool isClearing = false;

    public bool IsClearring
    {
        get
        {
            return isClearing;
        }
    }
    
    protected ItemSquare itemSquare;

    private void Awake()
    {
        itemSquare= GetComponent<ItemSquare>();
    }

    public virtual void Clear()
    {
        isClearing = true;
        
        
        
        StartCoroutine(ClearAnimation());
    }
    private IEnumerator ClearAnimation()
    {
        Animator animator= GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(clearAnimation.name);
            //玩家得分 音效
            GameManager.Instance.AddScore(1);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }

    public void UpDestory()
    {
        Destroy(gameObject);
    }
}
