using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoveSquare : MonoBehaviour
{
    private ItemSquare item;
    private bool isMoving = false; // 防止重复移动
    private void Awake()
    {
        item = GetComponent<ItemSquare>();
    }

    
    /// <summary>
    /// 移动方块到目标位置
    /// </summary>
    public void Move(int newX, int newY, float dropTime, Action callBack)
    {
        // 如果正在移动且目标没变，不重复移动
        if (isMoving && item.X == newX && item.Y == newY) return;

        // 杀掉当前 Tween，并立即完成（确保回调触发）
        if (DOTween.IsTweening(item))
        {
            DOTween.Kill(item, complete: true);
        }

        // 开始新 Tween
        MoveTo(newX, newY, dropTime, callBack);
    } 
    private void MoveTo(int newX, int newY, float dropTime, Action callBack)
    {
        isMoving = true;

        Vector3 endPos = BoardManager.Instance.CalculatePosition(newX, newY);

        // Tween 动画
        item.transform.DOMove(endPos, dropTime)
            .SetEase(Ease.Linear)
            .SetId(item) // 用方块对象作为 ID，防止互相干扰
            .OnComplete(() =>
            {
                // 动画完成后再更新逻辑坐标
                item.X = newX;
                item.Y = newY;

                // 确保位置对齐
                item.transform.position = endPos;

                isMoving = false;

                // 回调通知 Board
                callBack?.Invoke();
            });
    }
    
    // public void Move(int newX, int newY, float droptime, Action callBack)
    // {
    //     if (DOTween.IsTweening($"moveItem{newX}_{newY}"))
    //     {
    //         DOTween.Kill($"moveItem{newX}_{newY}");
    //     }
    //
    //     MoveTo(newX, newY, droptime, callBack);
    // }
    //
    // private void MoveTo(int newX, int newY, float dropTime, Action callBack)
    // {
    //     item.X = newX;
    //     item.Y = newY;
    //     Vector3 endPos = BoardManager.Instance.CalculatePosition(newX, newY);
    //     item.transform.DOMove(endPos, dropTime).SetEase(Ease.Linear)
    //         .OnComplete(() =>
    //         {
    //             item.transform.position = endPos;
    //             callBack?.Invoke();
    //         }).SetId($"moveItem{newX}_{newY}"
    //         );
    // }
}