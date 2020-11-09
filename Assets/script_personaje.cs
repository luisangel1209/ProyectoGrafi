using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class script_personaje : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    public float fuerzaSalto;
    public bool enPiso;
    public Transform refPie;
    public float velX = 70f;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float movX;
        movX = Input.GetAxis("Horizontal");
        anim.SetFloat("absMovX", Mathf.Abs(movX));
        rb.velocity = new Vector2(velX * movX, rb.velocity.y);

        enPiso = Physics2D.OverlapCircle(refPie.position,1f,1<<8);
        anim.SetBool("enPiso", enPiso);

        if (Input.GetButtonDown("Jump") && enPiso){
            rb.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse);
        }
        if (movX < 0) transform.localScale = new Vector3(-1, 1, 1);
        if (movX > 0) transform.localScale = new Vector3(1, 1, 1);

    }
}
