using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] int maxJumps;
    private int jumpCount;

    private void Awake() //Called Everytime script is loaded
    {
        body = GetComponent<Rigidbody2D>(); //Gets component from game object from inspector tab
        jumpCount = maxJumps;
    }

    private void Update() //Runs every frame
    {

        Debug.Log(jumpCount);
        float horizontalInput = Input.GetAxis("Horizontal"); //Store horizontal Input (-1, 0 ,1)

        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);

        //Flip player when changing direction
        if (horizontalInput > 0.01f)
            transform.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);
      
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount > 0)
                body.velocity = new Vector2(body.velocity.x, jumpPower);
                jumpCount -= 1;
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            jumpCount = maxJumps;
    }

}
