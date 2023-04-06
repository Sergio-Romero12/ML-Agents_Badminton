using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class ML_Bot : Agent
{
    public GameObject entorno; // Identifica el script del controlador del entorno
    Rigidbody RbAgente; // Componentes físicos del agente
    public ColorAgente color_agente; // Identifica el color del agente que esta interactuando con el entorno
    public ML_Bot Agente_rival; //Identifica al agente rival

    public GameObject pelota; //Representa a la pelota
    Rigidbody PelotaRb; //Componentes físicos de la pelota

    public ControladorJuego controlador;

    public float VelocidadAgente = 0.75f;
    //public float FriccionAgente = 0.5f;

    float IdentificacionAgente; //Sirve para que la dirección de los vectores apunte en la dirección correcta dependiendo del agente
    float fuerza_lanzamiento; //Indica la fuerza con la que se lanza el tiro

    // Start se lanza antes de la actualización del primer frame
    void Start()
    {
        controlador = entorno.GetComponent<ControladorJuego>();
    }

    public override void Initialize()
    {
        RbAgente = GetComponent<Rigidbody>();
        PelotaRb = pelota.GetComponent<Rigidbody>();
        
        if (color_agente == ColorAgente.Azul) //Permite la simetría del juego
        {
            IdentificacionAgente = 1; // Identifica el agente azul
        }
        else
        {
            IdentificacionAgente = -1; // Identifica el agente rojo
        }
    }

    public override void CollectObservations(VectorSensor sensor) //Se definen las observaciones del agente
    {
        Vector3 vector_direccion_pelota = new Vector3((pelota.transform.position.x - transform.position.x) * IdentificacionAgente,
                                                      (pelota.transform.position.y - transform.position.y),
                                                      (pelota.transform.position.z - transform.position.z) * IdentificacionAgente);

        sensor.AddObservation(vector_direccion_pelota.normalized); // Vector direccion normalizado hacia la pelota desde la posicion del agente (3 floats)

        sensor.AddObservation(vector_direccion_pelota.magnitude); // Distancia del agente a la pelota (1 float)
                                                                  // Magnitud es igual a raiz cuadrada de (x*x+y*y+z*z) (Unity Documentation)

        Vector3 vector_velocidad_pelota = new Vector3(PelotaRb.velocity.x * IdentificacionAgente,
                                                      PelotaRb.velocity.y,
                                                      PelotaRb.velocity.z * IdentificacionAgente);

        sensor.AddObservation(vector_velocidad_pelota); // Vector velocidad pelota (3 floats)

        sensor.AddObservation(RbAgente.velocity); // Velocidad del agente (3 floats)

        sensor.AddObservation(transform.rotation.y); // Rotación actual del agente (1 float)

        sensor.AddObservation(Agente_rival.transform.position); // Posición del agente rival (3 floats)

    }

    public void MovimientoAgente(ActionSegment<int> act) //Método donde se especifican las acciones que se pueden realizar en función de las decisiones
    {
        var direccion_desplazamiento = Vector3.zero; // Vector auxiliar para el desplazamiento que va a realizar el agente
        var rotacion = Vector3.zero; //Vector auxiliar para la rotación que va a realizar el agente
        var moverse_adelante_atras = act[0]; // Posición 0: Moverse hacia delanto, hacia atras o no moverse en ese eje (Eje Z) 
        var moverse_drch_izq = act[1]; // Posición 1: Moverse hacia la derecha, hacia la izquierda o no moverse en ese eje (Eje X)
        var rotar = act[2]; //Posición 2: Rotar sentido agujas del reloj, sentido contrario o no rotar (En rotacion corresponde con el Eje Y)
        var lanzamiento = act[3]; //Posición 3: Indica la fuerza con la que golpear la pelota (Se mide en rangos debil, medio y fuerte)

        if (moverse_adelante_atras == 1) // Ir hacia delante
            direccion_desplazamiento = transform.forward * 1f;
        else if (moverse_adelante_atras == 2) // Ir hacia atras
            direccion_desplazamiento = transform.forward * -1f;

        if (moverse_drch_izq == 1) //Moverse derecha
            direccion_desplazamiento = transform.right * 1f;
        else if (moverse_drch_izq == 2) //Moverse izquierda
            direccion_desplazamiento = transform.right * -1f;

        if (rotar == 1) //Rotar derecha
            rotacion = transform.up * 1f;
        else if (rotar == 2) //Rotar izquierda
            rotacion = transform.up * -1f;

        if (lanzamiento == 0)
            fuerza_lanzamiento = 11f; //Lanzamiento débil
        else if (lanzamiento == 1)
            fuerza_lanzamiento = 14f; //Lanzamiento medio
        else if (lanzamiento == 2)
            fuerza_lanzamiento = 17f; //Lanzamiento fuerte

        //Una vez calculados los nuevos vectores se le aplica a los agentes
        transform.Rotate(rotacion, Time.fixedDeltaTime * 200f); //Se actualiza en función del tiempo dentro del juego de FixedUpdate
        RbAgente.AddForce(direccion_desplazamiento * VelocidadAgente * IdentificacionAgente, ForceMode.VelocityChange); //No es necesario delta time ya que se produce el cambio en segundos directamente
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MovimientoAgente(actionBuffers.DiscreteActions); //Recoge las decisiones tomadas por la red sobre que acciones realizar y lo pasa como argumento a la función de arriba
    }

    void OnCollisionEnter(Collision collision) //Método que actualiza quien es el último que toca la pelota y realiza el tiro del agente
    {
        if (collision.gameObject.CompareTag("ball"))
        {
            controlador.PrevioEnTocar = controlador.UltimoEnTocar; // Guarda quién toco previamente el balon
            controlador.UltimoEnTocar = color_agente; //Representa el número del color del agente, 0: Azul y 1: Rojo

            controlador.NumeroToques = controlador.NumeroToques + 1; //Para contar el número de toques consecutivo
            Debug.Log(" Numero de Toques: " + controlador.NumeroToques);

            //PelotaRb.velocity = direccion_lanzamiento;
            if (fuerza_lanzamiento == 11f)
            {
                Debug.Log(" Fuerza Lanzamiento: " + fuerza_lanzamiento);
                //PelotaRb.velocity = new Vector3(0f, fuerza_lanzamiento, 5f * IdentificacionAgente);
                PelotaRb.velocity = RbAgente.velocity.normalized * 5f + new Vector3(0f, fuerza_lanzamiento, 0f);
            }
            else if (fuerza_lanzamiento == 14f)
            {
                Debug.Log(" Fuerza Lanzamiento: " + fuerza_lanzamiento);
                //PelotaRb.velocity = new Vector3(0f, fuerza_lanzamiento, 8.5f * IdentificacionAgente);
                PelotaRb.velocity = RbAgente.velocity.normalized * 8.5f + new Vector3(0f, fuerza_lanzamiento, 0f);
            }
            else if (fuerza_lanzamiento == 17f)
            {
                Debug.Log(" Fuerza Lanzamiento: " + fuerza_lanzamiento);
                //PelotaRb.velocity = new Vector3(0f, fuerza_lanzamiento, 13.5f * IdentificacionAgente);
                PelotaRb.velocity = RbAgente.velocity.normalized * 13.5f + new Vector3(0f, fuerza_lanzamiento, 0f);
            }

            if (controlador.PrevioEnTocar == controlador.UltimoEnTocar) //Se penaliza dar dos o mas toques por el mismo agente
            {
                AddReward(-0.2f);
            }
            else
            {
                AddReward(0.1f);
            }
            //AddReward(0.1f);
        }

    }

    /*public override void Heuristic(in ActionBuffers actionsOut) //Permite controlar al agente mediante el teclado
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // moverse hacia delante
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // moverse hacia atras
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // moverse hacia la derecha
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // moverse hacia la izquierda
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.D))
        {
            // rotar hacia la derecha
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // rotar hacia la izquierda
            discreteActionsOut[2] = 2;
        }

        if (Input.GetKey(KeyCode.U))
        {
            // tiro debil
            discreteActionsOut[3] = 0;
        }
        if (Input.GetKey(KeyCode.I))
        {
            // tiro medio
            discreteActionsOut[3] = 1;
        }
        if (Input.GetKey(KeyCode.O))
        {
            // tiro fuerte
            discreteActionsOut[3] = 2;
        }
        //var aux_lanzamiento = Random.Range(0, 3);
        //discreteActionsOut[3] = aux_lanzamiento;
    }*/


}
