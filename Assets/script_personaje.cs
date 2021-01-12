


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
    bool miraValida;

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

    //energia
    int energiaMax = 5;
    public int energiaActual;
    public TMPro.TextMeshProUGUI TextoVida;

    //muerte
    public UnityEngine.UI.Image telaNegra;
    float valorAlfaDeseadoTelaNegra;

    public TMPro.TextMeshProUGUI textoContBalas;
    int cantBalas = 0;

    //cambiuo de escenas 
    int cuantosZombiesQuedan;
    public GameObject textoFinal;
    float momInicioFadeOut = float.MaxValue;
    int escenaACargarDespuesDelFadeOut;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        energiaActual = energiaMax;

        //fade in incial 
        telaNegra.color = new Color(0, 0, 0, 1); //negro
        valorAlfaDeseadoTelaNegra = 0; //Transparente

        if(infoPartida.hayPartidaGuardada) cargarPartida();

        //leemos cuantos zoombies hay
        cuantosZombiesQuedan = GameObject.Find("zombies").transform.childCount;
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

            Vector3 distancia = transform.position - mira.position; //calculo distancia
            miraValida = (distancia.magnitude > 10f);

            mira.gameObject.SetActive(miraValida);

            if (Input.GetButtonDown("Fire1") && miraValida) {
                if (cantBalas > 0) disparar();
                else {
                    //avisar que no tiene balas
                    textoContBalas.color = Color.red;
                    textoContBalas.fontSize = 50;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) SceneManager.LoadScene(escenaActual());
        if (Input.GetKeyDown(KeyCode.O)) SceneManager.LoadScene(escenaActual() + 1);

        //cantidad de balas
        textoContBalas.text = cantBalas.ToString();

        //chequear si es hora de pasar de nivel
        if (Time.time > momInicioFadeOut)
        {
            iniciarFadeOut();
            momInicioFadeOut = float.MaxValue;
        }
          
    }

    void guardarPartida(){
        infoPartida.infoPersonaje.cantBalas = cantBalas;
        infoPartida.infoPersonaje.energiaActual = energiaActual;
        infoPartida.infoPersonaje.posicion = transform.position;

        infoPartida.hayPartidaGuardada = true;

        //guardar el estado de cada paquete de balas en la lista llamada infoPaqueteBalas
        infoPartida.infoPaqueteBalas.Clear();
        Transform todosLosPaquetes = GameObject.Find("paquetes balas").transform;
        foreach (Transform paq in todosLosPaquetes){
            infoPartida.TipoInfoPaqueteBalas itemPaq = new infoPartida.TipoInfoPaqueteBalas{
                activo = paq.gameObject.activeSelf
            };
            infoPartida.infoPaqueteBalas.Add(itemPaq);
        }

        //guardar el estado de cada zombie
        infoPartida.infoZombies.Clear();
        Transform todosLosZombies = GameObject.Find("Zombies").transform;
        foreach (Transform zombie in todosLosZombies){
            infoPartida.TipoInfoZombies itemZombie = new infoPartida.TipoInfoZombies{
                activo = zombie.gameObject.activeSelf,
                posicion = zombie.position
            };
            infoPartida.infoZombies.Add(itemZombie);
        }
    }
    
    void cargarPartida(){
        cantBalas = infoPartida.infoPersonaje.cantBalas;
        energiaActual = infoPartida.infoPersonaje.energiaActual;
        transform.position = infoPartida.infoPersonaje.posicion;

        //cargar el estado de cada paquete de balas en la lista llamada infoPaqueteBalas
        Transform todosLosPaquetes = GameObject.Find("paquetes balas").transform;
        int i = 0;
        foreach (Transform paq in todosLosPaquetes){
            paq.gameObject.SetActive(infoPartida.infoPaqueteBalas[i++].activo);
        }
        //cargar el estado de cada zombie
        Transform todosLosZombies = GameObject.Find("Zombies").transform;
        i = 0;
        foreach (Transform zombie in todosLosZombies){
            //zombie.gameObject.SetActive(infoPartida.infoZombies[i++].activo);

            zombie.GetComponent<SpriteRenderer>().enabled = infoPartida.infoZombies[i].activo;
            zombie.GetComponent<script_zombie>().vivo = infoPartida.infoZombies[i].activo;

            zombie.position = infoPartida.infoZombies[i].posicion;
            i++;
        }
    }

    private void LateUpdate(){
        if (energiaActual <= 0) return;
        if (tieneArma && miraValida){
            //gire cabeza para mirar el mouse
            cabeza.up = RefOjos.position - mira.position;

            //arma mire al mouse
            contArma.up = contArma.position - mira.position;

            RefManoArma.position = mira.position;
        }
    }

    void disparar()
    {
        Vector3 direccion = (mira.position - contArma.position).normalized;

        //arma patea
        rb.AddForce(magnitudPateoArma * -direccion, ForceMode2D.Impulse);

        //particulas
        Instantiate(particulasArma, refPuntaArma.position, Quaternion.identity);

        //Sacudir cámara
        sacudirCamara(.5f);

        RaycastHit2D hit = Physics2D.Raycast(contArma.position, direccion, 10000f, ~(1 << 10));
        if (hit.collider != null)
        {   
            //le dio a algo
            if (hit.collider.gameObject.CompareTag("zombie"))
            {
                //Destroy(hit.collider.gameObject);
                //le dio al cuerpo de un zombie
                hit.rigidbody.AddForce(magnitudReaccionDisparo * direccion, ForceMode2D.Impulse);

                //particulas sangre
                Instantiate(particulasSangreVerde, hit.point, Quaternion.identity);
            }
            if (hit.collider.gameObject.CompareTag("cabezazombie"))
            {
                if(hit.transform.GetComponent<script_zombie>().vivo){
                    //le dio en la cabeza a un zombie
                    hit.transform.GetComponent<script_zombie>().muere(direccion);
                    Instantiate(particulasMuchaSangreVerde, hit.point, Quaternion.identity);

                    cuantosZombiesQuedan--;
                    if(cuantosZombiesQuedan == 0)
                    {
                        Debug.Log("mataste todos los zombies");
                        //mostrar el cartel
                        textoFinal.SetActive(true);
                        momInicioFadeOut = Time.time + 3f;
                        escenaACargarDespuesDelFadeOut = escenaActual() + 1;
                        
                    }
                }
            }
        }
        //restar municion
        cantBalas -= 1;
    }

    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("arma"))
        {
            tieneArma = true;
            Destroy(collision.gameObject);
            contArma.gameObject.SetActive(true);
            cantBalas +=5;
            textoContBalas.color = Color.green;
            textoContBalas.fontSize = 50;
        }
        if(collision.gameObject.CompareTag("balas")){
            //Destroy(collision.gameObject);
            collision.gameObject.SetActive(false);
            cantBalas +=5;
            textoContBalas.color = Color.green;
            textoContBalas.fontSize = 50;
        }
        if(collision.gameObject.CompareTag("checkpoint")){
            guardarPartida();
            Destroy(collision.gameObject);
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

        //Reinicar escena cuando se complete el fadeout
        if (valorAlfa > 0.9f && valorAlfaDeseadoTelaNegra == 1)
        {
            SceneManager.LoadScene(escenaACargarDespuesDelFadeOut);
        }

        //vuelve el contador de balas a su estilo normal
        textoContBalas.color = Color.Lerp(textoContBalas.color, Color.white, .1f);
        textoContBalas.fontSize = Mathf.Lerp(textoContBalas.fontSize, 36, .1f);
    }

    int escenaActual()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    void actualizarDisplay(){
        //mascara roja
        float valorAlfa = 1 / (float) energiaMax * (energiaMax - energiaActual);
        mascaraDaño.color = new Color(1, 1, 1, valorAlfa);


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
            escenaACargarDespuesDelFadeOut = escenaActual();
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
