using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class script_zombie : MonoBehaviour
{

    Animator anim;
    Rigidbody2D rb;
    float limiteCaminataIzq;
    float limiteCaminataDer;

    public float velCaminata = 10f;
    int direccion = 1;
    Vector3 escalaOriginal;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        limiteCaminataDer = transform.position.x + GetComponent<CircleCollider2D>().radius;
        limiteCaminataIzq = transform.position.x - GetComponent<CircleCollider2D>().radius;

        escalaOriginal = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(velCaminata * direccion, rb.velocity.y);
        if (transform.position.x < limiteCaminataIzq) direccion = 1;
        if (transform.position.x > limiteCaminataDer) direccion = -1;
        transform.localScale = new Vector3(escalaOriginal.x * direccion, escalaOriginal.y, escalaOriginal.z);
    }
}
