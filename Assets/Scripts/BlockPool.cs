using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool  : MonoBehaviour
{
    public static BlockPool Instance; // 单例
    public GameObject blockPrefab;    // 方块预制体
    public int initialSize = 64;      // 初始数量

    private Queue<ItemSquare> pool = new Queue<ItemSquare>();

    private void Awake()
    {
        Instance = this;
        InitPool();
    }
    
    private void InitPool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewBlock();
        }
    }

    private void CreateNewBlock()
    {
        GameObject obj = Instantiate(blockPrefab);
        obj.SetActive(false);
        ItemSquare square = obj.GetComponent<ItemSquare>();
        pool.Enqueue(square);
    }
    
    public ItemSquare GetBlock()
    {
        ItemSquare square = null;
        if (pool.Count == 0)
        {
            CreateNewBlock();
        }
        square = pool.Dequeue();
        square.gameObject.SetActive(true);
        return square;
    }
    public void ReturnBlock(ItemSquare square)
    {
        square.gameObject.SetActive(false);
        pool.Enqueue(square);
    }
    
}
