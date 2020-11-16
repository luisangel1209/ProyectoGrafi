using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class script_zombie : MonoBehaviour
{

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(40.rb.velocity.y);
    }
}
