using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DistintosCasos //Para identificar los distintos eventos de colisiones que se pueden producir
{
    LimitesPared = 0,
    PasaRedAzul = 1,
    PasaRedRojo = 2,
    ObjetivoAzul = 3,
    ObjetivoRojo = 4
}

public enum ColorAgente //Para identificar que agente fue el último en tocar la pelota
{
    Azul = 0,
    Rojo = 1,
    Ninguno = 2,
    Previo = 3
}

public class ControladorJuego : MonoBehaviour
{
    public ML_Bot AgenteAzul; //Representa al agente de color azul
    public ML_Bot AgenteRojo; //Representa al agente de color rojo

    public GameObject pelota; //Representa a la pelota
    Rigidbody PelotaRb; //Componentes físicos de la pelota

    public GameObject ObjetivoAzul; //Representa la zona donde debe atacar el agente azul
    public GameObject ObjetivoRojo; //Representa la zona donde debe atacar el agente rojo

    private int contador; //Variable para medir el número de steps actuales por episodio
    public int StepsEntrenamientoMax; //Variable para indicar el número máximo de steps por episodio
    private int AuxSpawnPelota; //Variable auxiliar para la inicialización random de en que campo aparace la pelota al comenzar
    private int SpawnPelota; // Variable que se emplea para que la pelota vaya apareciendo en ambos campos alternativamente

    public ColorAgente UltimoEnTocar; //Variable que identifica el último agente que tocó la pelota
    public ColorAgente PrevioEnTocar; //Variable que identifica el penúltimo agente que tocó la pelota

    public int NumeroToques = 0;

    // Start se lanza antes de la actualización del primer frame
    void Start()
    {   
        PelotaRb = pelota.GetComponent<Rigidbody>();

        // Para que la pelota aparezca aleatoriamente en un campo u otro al inicializar el entrenamiento
        
        AuxSpawnPelota = Random.Range(0, 2);
        if (AuxSpawnPelota == 0)
        {
            SpawnPelota = 1; // Identifica un campo
        }
        else
        {
            SpawnPelota = -1; // Identifica el otro campo
        }

        ReseteoEntorno();
    }

    // Update se lanza una vez por frame
    void Update() 
    {
        // En el update de unity se comprueba el número máximo de steps por episodio
        // Se codifica asi para evitar que el entrenemaiento se quede pillado con alguna prueba
        contador += 1;
        if (contador >= StepsEntrenamientoMax && StepsEntrenamientoMax > 0)
        {
            AgenteAzul.EpisodeInterrupted();
            AgenteRojo.EpisodeInterrupted();
            ReseteoEntorno();
        }
    }

    public void Colisiones(DistintosCasos triggerEvent)
    {
        switch (triggerEvent)
        {
            case DistintosCasos.LimitesPared:
                //Aprendizaje competitivo
                /*if (UltimoEnTocar == ColorAgente.Azul)
                {
                    AgenteAzul.AddReward(-1f);
                    AgenteRojo.AddReward(1f);
                }
                else if (UltimoEnTocar == ColorAgente.Rojo)
                {
                    AgenteAzul.AddReward(1f);
                    AgenteRojo.AddReward(-1f);
                }*/
                

                //Aprendizaje cooperativo

                if (UltimoEnTocar == ColorAgente.Azul)
                {
                    AgenteAzul.AddReward(-0.1f);
                }
                else if (UltimoEnTocar == ColorAgente.Rojo)
                {
                    AgenteRojo.AddReward(-0.1f);
                }

                AgenteAzul.EndEpisode();
                AgenteRojo.EndEpisode();
                ReseteoEntorno();
                break;
            
            case DistintosCasos.PasaRedAzul:
                if (UltimoEnTocar == ColorAgente.Azul)
                {
                    AgenteAzul.AddReward(1f);
                }
                break;

                // No se termina el episodio, ni se resetea el entorno porque se puede producir algun otro caso

            case DistintosCasos.PasaRedRojo:
                if (UltimoEnTocar == ColorAgente.Rojo)
                {
                    AgenteRojo.AddReward(1f);
                }
                break;

                // No se termina el episodio, ni se resetea el entorno porque se puede producir algun otro caso

            case DistintosCasos.ObjetivoAzul: //Para aprendizaje competitivo (self-play)
                // azul consigue un punto en el juego de badminton
                /*AgenteAzul.AddReward(1f);
                AgenteRojo.AddReward(-1f);*/

                AgenteAzul.EndEpisode();
                AgenteRojo.EndEpisode();
                ReseteoEntorno();
                break;

            case DistintosCasos.ObjetivoRojo: //Para aprendizaje competitivo (self-play)
                // rojo consigue un punto en el juego de badminton
                /*AgenteRojo.AddReward(1f);
                AgenteAzul.AddReward(-1f);*/

                AgenteAzul.EndEpisode();
                AgenteRojo.EndEpisode();
                ReseteoEntorno();
                break;
        }
    }

    public void ReseteoEntorno() //Para generar una nueva prueba de entrenamiento
    {
        contador = 0; //Se resetea el contador de steps

        UltimoEnTocar = ColorAgente.Ninguno; // Ultimo en tocar pasa a ser ninguno de los dos agentes
        PrevioEnTocar = ColorAgente.Previo; //Se resetea también el valor de previo a ninguno

        NumeroToques = 0; //Varible para contar toques sucesivos entre agentes. Se resetea el valor a 0.

        // Se randomizan los valores de la posición y de la rotación del agente Azul
        var AgenteAzul_posicionX = Random.Range(-3f, 3f);
        var AgenteAzul_posicionY = Random.Range(1f, 2f); 
        var AgenteAzul_posicionZ = Random.Range(-3f, 3f);
        var AgenteAzul_rotacion = Random.Range(-45f, 45f);

        AgenteAzul.transform.localPosition = new Vector3(AgenteAzul_posicionX, AgenteAzul_posicionY, AgenteAzul_posicionZ);
        AgenteAzul.transform.eulerAngles = new Vector3(0, AgenteAzul_rotacion, 0);

        AgenteAzul.GetComponent<Rigidbody>().velocity = default(Vector3);

        // Se randomizan los valores de la posición y de la rotación del agente Rojo
        var AgenteRojo_posicionX = Random.Range(-3f, 3f);
        var AgenteRojo_posicionY = Random.Range(1f, 2f);
        var AgenteRojo_posicionZ = Random.Range(-3f, 3f);
        var AgenteRojo_rotacion = Random.Range(-45f, 45f);

        AgenteRojo.transform.localPosition = new Vector3(AgenteRojo_posicionX, AgenteRojo_posicionY, AgenteRojo_posicionZ);
        AgenteRojo.transform.eulerAngles = new Vector3(0, AgenteRojo_rotacion, 0);

        AgenteRojo.GetComponent<Rigidbody>().velocity = default(Vector3);
        
        // A continuacion se resetea la pelota 

        var pelota_posicionX = Random.Range(-2f, 2f);
        var pelota_posicionY = Random.Range(6f, 8f);
        var pelota_posicionZ = Random.Range(6f, 10f);
        
        SpawnPelota = -1 * SpawnPelota;  //Se multiplica por -1 para indicar que aparezca en el otro campo (SpawnPelota toma valores 1 o -1)

        if (SpawnPelota == -1) //Campo agente rojo
        {
            pelota.transform.localPosition = new Vector3(pelota_posicionX, pelota_posicionY, pelota_posicionZ);
            //pelota.transform.localPosition = new Vector3(0f, 7f, 3f); //Rango corto
            //pelota.transform.localPosition = new Vector3(0f, 7f, 6f); //Rango medio
            //pelota.transform.localPosition = new Vector3(0f, 7f, 13f); //Rango largo
        }
        else if (SpawnPelota == 1) //Campo agente azul
        {
            pelota.transform.localPosition = new Vector3(pelota_posicionX, pelota_posicionY, -1 * pelota_posicionZ);
            //pelota.transform.localPosition = new Vector3(0f, 7f, -3f); //Rango corto
            //pelota.transform.localPosition = new Vector3(0f, 7f, -6f); //Rango medio
            //pelota.transform.localPosition = new Vector3(0f, 7f, -13f); //Rango largo
        }

        PelotaRb.angularVelocity = Vector3.zero;
        PelotaRb.velocity = Vector3.zero;
    }
}
