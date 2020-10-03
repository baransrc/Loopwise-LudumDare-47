using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFlip : MonoBehaviour
{
    public bool FacingRight { get; private set; }

    private void Awake()
    {
        FacingRight = transform.localScale.x > 0;
    }

    private void Flip()
    {
        var direction = Input.GetAxisRaw("Horizontal");

        if (direction == 0f) return;
        
        if (FacingRight != (direction > 0f))
        {
            FacingRight = (direction > 0f);
            transform.localScale = new Vector3(-1f * transform.localScale.x,  transform.localScale.y, transform.localScale.z);
        }
    }

    void Update()
    {
        Flip();
    }
}