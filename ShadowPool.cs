using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPool : MonoBehaviour
{
 // 单例模式
    public static ShadowPool instance;

    public GameObject shadowPrefab;
    public int shadowCount;

    private Queue< GameObject > avaliableObjects = new Queue< GameObject >();

    private void Awake()
    {
        instance = this;

        // init
        FillPool();
    }

    public void FillPool()
    {
        for (int i = 0; i < shadowCount; i++)
        {
            var newShadow = Instantiate(shadowPrefab);
            newShadow.transform.SetParent(transform);

            // unable return pool
            ReturePool(newShadow);
        }
    }

    public void ReturePool(GameObject gameObject)
    {
        gameObject.SetActive(false);

        avaliableObjects.Enqueue(gameObject);
    }

    public GameObject GetFromPool()
    {
        if(avaliableObjects.Count == 0)
        {
            FillPool();
        }

        var outShadow = avaliableObjects.Dequeue();

        outShadow.SetActive(true);

        return outShadow;
    }
}
