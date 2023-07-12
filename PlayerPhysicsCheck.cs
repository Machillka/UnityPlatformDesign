using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsCheck : MonoBehaviour
{
    [Header("GroundCheck")]
    public Vector2 groundPoint;
    public Vector2 groundRange;
    public LayerMask groundLayer;
    public bool isGround;

    [Header("WallCheck")]
    public Vector2 wallPoint;
    public Vector2 wallRange;
    public bool isWall;
    public bool isLeftWall;
    public bool isRightWall;
    public enum WallDirection
    {
        LeftWall = -1,
        RightWall = 1,
        NoneWall = 0
    };
    public WallDirection wallDirection;

    void Update()
    {
        CheckGround();
        CheckWall();
    }

    public void CheckGround()
    {
        isGround = Physics2D.OverlapBox((Vector2)transform.position + groundPoint, groundRange, 0f, groundLayer);
    }

    public void CheckWall()
    {
        isRightWall = Physics2D.OverlapBox((Vector2)transform.position + wallPoint, wallRange, 0f, groundLayer);
        Vector2 leftPos = new Vector2(transform.position.x - wallPoint.x, transform.position.y + wallPoint.y);
        isLeftWall = Physics2D.OverlapBox(leftPos, wallRange, 0f, groundLayer);
    
        isWall = isRightWall || isLeftWall;

        if (isLeftWall)
            wallDirection = WallDirection.LeftWall;
        else if (isRightWall)
            wallDirection = WallDirection.RightWall;
        else
            wallDirection = WallDirection.NoneWall;
    }

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + groundPoint, (Vector3)groundRange);
        
        Gizmos.color = Color.blue;

        Gizmos.DrawWireCube((Vector2)transform.position + wallPoint, wallRange);
        Vector2 leftPos = new Vector2(transform.position.x - wallPoint.x, transform.position.y + wallPoint.y);
        
        Gizmos.DrawWireCube(leftPos, wallRange);
    }
}
