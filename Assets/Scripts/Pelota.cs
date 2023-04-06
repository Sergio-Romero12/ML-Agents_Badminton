using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pelota : MonoBehaviour
{
    public GameObject entorno; // Identifica el script de C# de control del entorno
    ControladorJuego controlador; // Variable para guardar el objeto script
    public Rigidbody PelotaRb; //Componentes fisicos de la pelota
    public float fuerza_caida = 1f; //Variable para dotar de fuerza la caida de la pelota
    public bool funcion_caida = true;

    // Start se lanza antes de la actualización del primer frame
    void Start()
    {
        controlador = entorno.GetComponent<ControladorJuego>(); //Controlador representa el script con el control del entorno
    }

    //Update se lanza en cada frame del juego
    void Update()
    {
        if (PelotaRb.velocity.y < 0 && funcion_caida == true) //Permite controlar la velocidad de caida de la pelota, para evitar el efectod de que flota.
        {
            PelotaRb.velocity += fuerza_caida * Physics.gravity.y * Time.deltaTime * Vector3.up;
        }
    }


    void OnTriggerEnter(Collider collision) //Metodo que comprueba las distintas colisiones que se pueden producir entre el entorno y la pelota
    {
        if (collision.gameObject.CompareTag("pared")) //La pelota sale del campo
        {
            controlador.Colisiones(DistintosCasos.LimitesPared); 
        }
        else if (collision.gameObject.CompareTag("objetivo_rojo")) //La pelota alcanza el campo Azul
        {
            controlador.Colisiones(DistintosCasos.ObjetivoRojo); 
        }
        else if (collision.gameObject.CompareTag("objetivo_azul")) //La pelota alcanza el campo Rojo
        {
            controlador.Colisiones(DistintosCasos.ObjetivoAzul); 
        }
        else if (collision.gameObject.CompareTag("pasa_red_rojo")) //El agente rojo consigue pasar la pelota por encima de la red
        {
            controlador.Colisiones(DistintosCasos.PasaRedRojo);
        }
        else if (collision.gameObject.CompareTag("pasa_red_azul")) //El agente azul consigue pasar la pelota por encima de la red
        {
            controlador.Colisiones(DistintosCasos.PasaRedAzul);
        }

    }
    
}
