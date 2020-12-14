using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class script_zombie : MonoBehaviour
{

    Rigidbody2D rb;
    float limiteCaminataIzq;
    float limiteCaminataDer;

    public float velCaminata = 5f;
    int direccion = 1;
    Vector3 escalaOriginal;

    public float umbralVelocidad;

    public GameObject prefabMuerto;

    public float magnitudVueloCabeza = 200f;

    enum tipoComportamientoZombie { pasivo, persecución, ataque}

    tipoComportamientoZombie comportamiento = tipoComportamientoZombie.pasivo;

    float entradaZonaPersecución = 60f;
    float salidaZonaPersecución = 100f;
    float distanciaAtaque = 7f;

    float distanciaConPersonaje;
    public Transform Personaje;

    Animator anim;

    bool mordidaEsValida = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        limiteCaminataDer = transform.position.x + GetComponent<CircleCollider2D>().radius;
        limiteCaminataIzq = transform.position.x - GetComponent<CircleCollider2D>().radius;

        escalaOriginal = transform.localScale;

        anim = transform.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        distanciaConPersonaje = Mathf.Abs(Personaje.position.x - transform.position.x);
        switch (comportamiento)
        {
            case tipoComportamientoZombie.pasivo:
                //pasivo
                if (rb.velocity.magnitude < umbralVelocidad)
                {
                    //desplazarse caminando
                    rb.velocity = new Vector2(velCaminata * direccion, rb.velocity.y);
                    //girarse 
                    if (transform.position.x < limiteCaminataIzq) direccion = 1;
                    if (transform.position.x > limiteCaminataDer) direccion = -1;

                    anim.speed = 1f;

                    if (distanciaConPersonaje < entradaZonaPersecución) comportamiento = tipoComportamientoZombie.persecución;
                }
                break;

            case tipoComportamientoZombie.persecución:
                //persecucion
                if (rb.velocity.magnitude < umbralVelocidad)
                {
                    //desplazarse corriendo
                    rb.velocity = new Vector2(velCaminata * 1.5f * direccion, rb.velocity.y);
                    //girarse 
                    if (Personaje.position.x > transform.position.x) direccion = 1;
                    if (Personaje.position.x < transform.position.x) direccion = -1;

                    anim.speed = 1.5f;

                    if (distanciaConPersonaje > salidaZonaPersecución) comportamiento = tipoComportamientoZombie.pasivo;

                    if (distanciaConPersonaje < distanciaAtaque) comportamiento = tipoComportamientoZombie.ataque;
                }
                break;

            case tipoComportamientoZombie.ataque:
                //ataque
                if (rb.velocity.magnitude < umbralVelocidad)
                {
                    anim.SetTrigger("Atacar");
                    //girarse posicion personaje
                    if (Personaje.position.x > transform.position.x) direccion = 1;
                    if (Personaje.position.x < transform.position.x) direccion = -1;

                    anim.speed = 1f;
                    
                    //volver zona persecucion
                    if (distanciaConPersonaje > distanciaAtaque) {
                    comportamiento = tipoComportamientoZombie.persecución;
                    anim.ResetTrigger("Atacar");
                }
                }
                break;
        }
        transform.localScale = new Vector3(escalaOriginal.x * direccion, escalaOriginal.y, escalaOriginal.z);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && mordidaEsValida)
        {
            mordidaEsValida = false;
            Personaje.GetComponent<script_personaje>().RecibirMordida(collision.contacts[0].point);
        }
    }

    public void muere(Vector3 direccion)
    {
        GameObject instMuerto = Instantiate(prefabMuerto, transform.position, transform.rotation);

        instMuerto.transform.GetChild(0).GetComponent<Rigidbody2D>().AddForce(direccion * magnitudVueloCabeza, ForceMode2D.Impulse);
        instMuerto.transform.GetChild(1).GetComponent<Rigidbody2D>().AddForce(direccion * magnitudVueloCabeza/2, ForceMode2D.Impulse);
        instMuerto.transform.GetChild(0).GetComponent<Rigidbody2D>().AddTorque(10f, ForceMode2D.Impulse);
        instMuerto.transform.GetChild(1).GetComponent<Rigidbody2D>().AddTorque(-10f, ForceMode2D.Impulse);

        Destroy(gameObject);
    }

    public void mordidaValida_inicio(){
        mordidaEsValida = true;
    }

    public void mordidaValida_fin(){
        mordidaEsValida = false;
    }
}
