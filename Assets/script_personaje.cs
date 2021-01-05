using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class script_personaje : MonoBehaviour
{
    Animator anim;
    Rigidbody2D rb;
    public float fuerzaSalto;
    public bool enPiso;
    public Transform refPie;
    public float velX = 70f;
    // Start is called before the first frame update

    public Transform contArma;
    public bool tieneArma;
    public Transform mira;
    public Transform RefManoArma;

    public Transform RefOjos;
    public Transform cabeza;
    public float magnitudPateoArma = 100f;
    public Transform refPuntaArma;
    public GameObject particulasArma;

    //sacudir camara
    public Transform camaraASacudir;
    float magnitudSacudida;

    public float magnitudReaccionDisparo = 100f;
    public GameObject particulasSangreVerde;
    public GameObject particulasMuchaSangreVerde;

    public GameObject particulasSangrePersonaje;

    public UnityEngine.UI.Image mascaraDaño;
    public UnityEngine.UI.Image telaNegra;
    float valorAlfaDeseadoTelaNegra;

    //energia
    int energiaMax = 5;
    int energiaActual = 0;
    
    public TMPro.TextMeshProUGUI TextoVida;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        energiaActual = energiaMax;

        //fade in incial 
        telaNegra.color = new Color(0, 0, 0, 1); //negro
        valorAlfaDeseadoTelaNegra = 0; //Transparente
    }

    // Update is called once per frame
    void Update()
    {
        if (energiaActual <= 0) return;
        float movX;
        movX = Input.GetAxis("Horizontal");
        anim.SetFloat("absMovX", Mathf.Abs(movX));
        rb.velocity = new Vector2(velX * movX, rb.velocity.y);

        enPiso = Physics2D.OverlapCircle(refPie.position, 1f, 1 << 8);
        anim.SetBool("enPiso", enPiso);

        if (Input.GetButtonDown("Jump") && enPiso) {
            rb.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse);
        }

        if (tieneArma)
        {
            if (mira.transform.position.x < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
            if (mira.transform.position.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
        }
        else { 
            if (movX < 0) transform.localScale = new Vector3(-1, 1, 1);
            if (movX > 0) transform.localScale = new Vector3(1, 1, 1);
        }
        //Detecta Mouse y coloca mira
        if (tieneArma)
        {
            mira.position = Camera.main.ScreenToWorldPoint(new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y,
                -Camera.main.transform.position.z
                ));
            RefManoArma.position = mira.position;

            if (Input.GetButtonDown("Fire1")) disparar();

        }

        if (Input.GetKeyDown(KeyCode.P)) anim.SetTrigger("muere"); ;
        if (Input.GetKeyDown(KeyCode.O)) valorAlfaDeseadoTelaNegra = 0;

    }

    void disparar()
    {
        Vector3 direccion = (mira.position - contArma.position).normalized;

        //arma patea
        rb.AddForce(magnitudPateoArma * -direccion, ForceMode2D.Impulse);

        //particulas
        Instantiate(particulasArma, refPuntaArma.position, Quaternion.identity);

        //Sacudir cámara
        sacudirCamara(.3f);

        RaycastHit2D hit = Physics2D.Raycast(contArma.position, direccion, 10000f, ~(1 << 10));
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("zombie"))
            {
                //Destroy(hit.collider.gameObject);
                hit.rigidbody.AddForce(magnitudReaccionDisparo * direccion, ForceMode2D.Impulse);

                //particulas sangre
                Instantiate(particulasSangreVerde, hit.point, Quaternion.identity);
            }
            if (hit.collider.gameObject.CompareTag("cabezazombie"))
            {
                hit.transform.GetComponent<script_zombie>().muere(direccion);
                Instantiate(particulasMuchaSangreVerde, hit.point, Quaternion.identity);
            }
        }
    }

    private void LateUpdate()
    {
        if (energiaActual <= 0) return;
        if (tieneArma)
        {
            //gire cabeza para mirar el mouse
            cabeza.up = RefOjos.position - mira.position;

            //arma mire al mouse
            contArma.up = contArma.position - mira.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("arma"))
        {
            tieneArma = true;
            Destroy(collision.gameObject);
            contArma.gameObject.SetActive(true);
        }
    }

    void sacudirCamara(float maximo)
    {
        magnitudSacudida = maximo;
    }

    private void FixedUpdate()
    {
        if(magnitudSacudida > .01f)
        {
            //Sacudir Cámara
            camaraASacudir.rotation = Quaternion.Euler(
                Random.Range(-magnitudSacudida, magnitudSacudida),
                Random.Range(-magnitudSacudida, magnitudSacudida),
                Random.Range(-magnitudSacudida, magnitudSacudida)
                );
            magnitudSacudida *= .9f;
        }
        actualizarDisplay();

        //manejar la tela negra
        float valorAlfa = Mathf.Lerp(telaNegra.color.a, valorAlfaDeseadoTelaNegra, .05f);
        telaNegra.color = new Color(0, 0, 0, valorAlfa);
    }

    void actualizarDisplay(){
        //mascara roja
        float valorAlfa = 1 / (float) energiaMax * (energiaMax - energiaActual);
        mascaraDaño.color = new Color(1, 1, 1, valorAlfa);

        //Reinicar escena cuando se complete el fadeout
        if (valorAlfa > 0.9f && valorAlfaDeseadoTelaNegra == 1) SceneManager.LoadScene("SampleScene");

        //vida
        TextoVida.text = energiaActual.ToString();
    }

    public void RecibirMordida(Vector2 posicion){
        //reducir energia
        energiaActual -= 1;

        if(energiaActual <= 0){
            Debug.Log("muerto");
            actualizarDisplay();
            // Destroy(gameObject);

            //Comienza el proceso de muerte
            anim.SetTrigger("muere");
        }else{

        Debug.Log("auch! ahora tengo " + energiaActual + " de " + energiaMax);

        //Particulas de sangre
        Instantiate (particulasSangrePersonaje, posicion, Quaternion.identity);

        //disparar animacion
        anim.SetTrigger("auch");

        
        
        }

    }

    public void iniciarFadeOut()
    {
       valorAlfaDeseadoTelaNegra = 1; //fadeOut
    }
}
