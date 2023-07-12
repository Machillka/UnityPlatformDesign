using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowPrefab : MonoBehaviour
{
private Transform playerTransform;

    [Header("Time")]
    public float existTime;
    private float startTime;

    [Header("Sprite")]
    private SpriteRenderer shadowSprite;
    private SpriteRenderer playerSprite;
    public float alphaSet;
    public float multiplierAlpha;
    private float alpha;
    private Color color;

    private void OnEnable()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        shadowSprite = GetComponent< SpriteRenderer >();
        playerSprite = playerTransform.GetComponent< SpriteRenderer >();

        alpha = alphaSet;
        shadowSprite.sprite = playerSprite.sprite;

        transform.position = playerTransform.position;
        transform.localScale = playerTransform.localScale;
        transform.rotation = playerTransform.rotation;

        startTime = Time.time;
    }

    private void Update()
    {
        alpha *= multiplierAlpha;

        color = new Color(1, 1, 1, alpha);

        shadowSprite.color = color;

        if(Time.time > startTime + existTime)
        {
            ShadowPool.instance.ReturePool(this.gameObject);
        }
    }
}
