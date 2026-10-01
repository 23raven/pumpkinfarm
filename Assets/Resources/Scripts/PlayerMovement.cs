using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //скрипт движения коментировать пожалуй не буду

    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (moveX != 0)
        {
            moveY = 0;
        }

        movement = new Vector2(moveX, moveY).normalized;

        if (movement != Vector2.zero)
        {
            transform.up = movement;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}