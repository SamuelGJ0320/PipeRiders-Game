using UnityEngine;
using System.Collections.Generic;

public class LanePlayerControllerCurvo : MonoBehaviour
{
    [Header("Carriles")]
    public int numCarriles = 8;
    public float radioTunel = 5f;
    public float velocidadBase = 50f;
    public float velocidadMaxima = 150f;
    public float aceleracion = 40f;
    public float desaceleracion = 20f;
    public float suavizadoCambioCarril = 0.15f;
    public TunnelGenerator tunnelGenerator;
    [Header("Colision con Obstaculos")]
    public float multiplicadorVelocidadChoque = 0.55f;
    public float tiempoRecuperacionChoque = 1.2f;
    public float tiempoInvulnerableTrasChoque = 0.25f;
    [Header("Arranque")]
    public float velocidadInicialFactor = 0.25f;
    public float duracionAceleracionInicial = 2.5f;
    [Header("Cronometro")]
    public bool mostrarCronometro = true;
    public Vector2 margenCronometro = new Vector2(20f, 20f);
    public int tamanoFuenteCronometro = 38;
    [Header("Objetivo del Nivel")]
    [Tooltip("Formato mm:ss:cc. Ejemplo: 01:02:55")]
    public string tiempoObjetivoTexto = "01:02:55";
    public bool exigirCeroChoquesParaGanar = true;
    [Header("Cuenta Regresiva")]
    public float duracionCuentaRegresiva = 3f;
    public float duracionTextoGo = 0.7f;

    private int carrilActual = 0;
    private float carrilInterpolado = 0f;
    private float avanceCurva = 0f;
    private float velocidadActual;
    private List<Vector3> puntosCurva;
    private List<Vector3> tangentesCurva;

    [Header("Cámara Pipe Riders")]
    public Transform camaraPipeRiders;
    public float suavidadCamara = 0.15f;

    private int idxCamara;
    private float tCamara;
    private Vector3 posBaseCamara;
    private float tiempoFinPenalizacion = 0f;
    private float tiempoProximoChoqueValido = 0f;
    private float tiempoInicioNivel = 0f;
    private float velocidadInicialNivel = 0f;
    private float tiempoTranscurrido = 0f;
    private float longitudTotalCurva = 0f;
    private bool cronometroDetenido = false;
    private int choquesTotales = 0;
    private bool nivelIniciado = false;
    private bool nivelFinalizado = false;
    private bool jugadorGano = false;
    private float tiempoPreinicio = 0f;
    private float tiempoObjetivoSegundos = 0f;
    private GUIStyle estiloCronometro;
    private GUIStyle estiloVelocidad;
    private GUIStyle estiloCentro;

    void Start()
    {
        carrilActual = numCarriles / 2;
        carrilInterpolado = carrilActual;
        velocidadInicialNivel = Mathf.Max(2f, velocidadBase * Mathf.Clamp01(velocidadInicialFactor));
        velocidadActual = 0f;
        avanceCurva = 0f;
        tiempoInicioNivel = Time.time;
        tiempoPreinicio = Mathf.Max(0f, duracionCuentaRegresiva) + Mathf.Max(0f, duracionTextoGo);

        // Objetivo fijo del nivel solicitado.
        tiempoObjetivoTexto = "01:02:55";

        tiempoObjetivoSegundos = ParsearTiempo(tiempoObjetivoTexto);

        puntosCurva = tunnelGenerator.GetPuntosCurva();
        tangentesCurva = tunnelGenerator.GetTangentesCurva();
        longitudTotalCurva = CalcularLongitudCurva(puntosCurva);

        if (tunnelGenerator != null && tunnelGenerator.tunnelPrefab != null)
        {
            float escala = tunnelGenerator.tunnelPrefab.transform.localScale.x;
            radioTunel = escala * 0.5f;
        }

        // Posicion inicial consistente para evitar salto/angulo raro durante 3-2-1-GO.
        ActualizarPosicion();

        // La colision con obstaculos requiere Rigidbody + Collider en el player.
    }

    void Update()
    {
        if (!nivelIniciado)
        {
            tiempoPreinicio -= Time.deltaTime;
            if (tiempoPreinicio <= 0f)
            {
                nivelIniciado = true;
                tiempoInicioNivel = Time.time;
                velocidadActual = velocidadInicialNivel;
            }
            return;
        }

        if (nivelFinalizado)
            return;

        if (!cronometroDetenido)
        {
            tiempoTranscurrido += Time.deltaTime;
        }

        ManejarAceleracion();
        ManejarCambioCarril();
        ActualizarPosicion();

        if (!cronometroDetenido && longitudTotalCurva > 0.001f && avanceCurva >= longitudTotalCurva)
        {
            cronometroDetenido = true;
            nivelFinalizado = true;
            bool cumpleTiempo = tiempoTranscurrido <= tiempoObjetivoSegundos;
            bool cumpleChoques = !exigirCeroChoquesParaGanar || choquesTotales == 0;
            jugadorGano = cumpleTiempo && cumpleChoques;
            velocidadActual = 0f;
            Debug.Log($"Cronometro detenido al final del tunel: {FormatearTiempo(tiempoTranscurrido)}");
        }
    }

    void OnGUI()
    {
        if (!mostrarCronometro)
            return;

        if (estiloCronometro == null)
        {
            estiloCronometro = new GUIStyle(GUI.skin.label);
            estiloCronometro.fontSize = Mathf.Max(12, tamanoFuenteCronometro - 6);
            estiloCronometro.fontStyle = FontStyle.Bold;
            estiloCronometro.normal.textColor = Color.white;
            estiloCronometro.alignment = TextAnchor.UpperRight;
        }

        if (estiloVelocidad == null)
        {
            estiloVelocidad = new GUIStyle(GUI.skin.label);
            estiloVelocidad.fontSize = Mathf.Max(12, tamanoFuenteCronometro - 8);
            estiloVelocidad.fontStyle = FontStyle.Bold;
            estiloVelocidad.normal.textColor = Color.white;
            estiloVelocidad.alignment = TextAnchor.LowerRight;
        }

        if (estiloCentro == null)
        {
            estiloCentro = new GUIStyle(GUI.skin.label);
            estiloCentro.fontSize = Mathf.Max(28, tamanoFuenteCronometro + 10);
            estiloCentro.fontStyle = FontStyle.Bold;
            estiloCentro.normal.textColor = Color.white;
            estiloCentro.alignment = TextAnchor.MiddleCenter;
        }

        float bloqueAncho = 360f;
        float xDerecha = Screen.width - margenCronometro.x - bloqueAncho;
        float ySuperior = margenCronometro.y;

        GUI.Label(new Rect(xDerecha, ySuperior, bloqueAncho, 40f), $"Objetivo: {FormatearTiempo(tiempoObjetivoSegundos)}", estiloCronometro);
        GUI.Label(new Rect(xDerecha, ySuperior + 34f, bloqueAncho, 40f), $"Actual:   {FormatearTiempo(tiempoTranscurrido)}", estiloCronometro);

        float velocidadKmh = velocidadActual * 3.6f;
        GUI.Label(
            new Rect(Screen.width - margenCronometro.x - 260f, Screen.height - margenCronometro.y - 32f, 260f, 32f),
            $"Velocidad: {velocidadKmh:000.0} km/h",
            estiloVelocidad);

        if (!nivelIniciado)
        {
            string textoCentro;
            if (tiempoPreinicio > duracionTextoGo)
            {
                float restanteNumeros = Mathf.Max(0f, tiempoPreinicio - duracionTextoGo);
                textoCentro = Mathf.CeilToInt(restanteNumeros).ToString();
            }
            else
            {
                textoCentro = "GO";
            }

            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), textoCentro, estiloCentro);
        }
        else if (nivelFinalizado)
        {
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), jugadorGano ? "You Win!" : "You Lose!", estiloCentro);
        }
    }

    string FormatearTiempo(float segundosTotales)
    {
        int totalCentesimas = Mathf.Max(0, Mathf.RoundToInt(segundosTotales * 100f));
        int minutos = totalCentesimas / 6000;
        int segundos = (totalCentesimas % 6000) / 100;
        int centesimas = totalCentesimas % 100;
        return $"{minutos:00}:{segundos:00}:{centesimas:00}";
    }

    void LateUpdate()
    {
        if (camaraPipeRiders == null) return;
        Vector3 tangente = transform.forward;
        float distanciaCamaraAjustada = radioTunel * 1.08f;
        float alturaCamaraAjustada = radioTunel * 0.32f;
        Vector3 centroTunel = posBaseCamara;
        Vector3 upCamara = (-transform.up).normalized;
        if (upCamara.sqrMagnitude < 0.0001f)
            upCamara = (centroTunel - transform.position).normalized;
        Vector3 posicionObjetivo = transform.position
            - tangente * distanciaCamaraAjustada
            + upCamara * alturaCamaraAjustada;
        Vector3 direccionDesdeCentro = (posicionObjetivo - centroTunel).normalized;
        posicionObjetivo = centroTunel + direccionDesdeCentro * (radioTunel - transform.localScale.x * 0.5f);
        if (!nivelIniciado)
        {
            camaraPipeRiders.position = posicionObjetivo;
        }
        else
        {
            camaraPipeRiders.position = Vector3.Lerp(
                camaraPipeRiders.position,
                posicionObjetivo,
                suavidadCamara);
        }
        Quaternion rotacionObjetivo = Quaternion.LookRotation(tangente, upCamara);
        camaraPipeRiders.rotation = rotacionObjetivo;
    }

    void ManejarAceleracion()
    {
        float velocidadObjetivo;

        bool acelerar =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow);

        float tiempoDesdeInicio = Time.time - tiempoInicioNivel;
        float tInicio = Mathf.Clamp01(tiempoDesdeInicio / Mathf.Max(0.01f, duracionAceleracionInicial));
        bool enAceleracionInicial = tInicio < 1f;

        if (Time.time < tiempoFinPenalizacion)
        {
            velocidadObjetivo = velocidadBase * multiplicadorVelocidadChoque;
            velocidadActual = Mathf.MoveTowards(
                velocidadActual,
                velocidadObjetivo,
                desaceleracion * 2f * Time.deltaTime);
            return;
        }

        if (enAceleracionInicial)
        {
            float objetivoInicio = Mathf.Lerp(velocidadInicialNivel, velocidadBase, tInicio);
            velocidadActual = Mathf.MoveTowards(
                velocidadActual,
                objetivoInicio,
                aceleracion * 0.75f * Time.deltaTime);

            if (!acelerar)
                return;
        }

        if (acelerar)
        {
            velocidadObjetivo = velocidadMaxima;

            velocidadActual = Mathf.MoveTowards(
                velocidadActual,
                velocidadObjetivo,
                aceleracion * Time.deltaTime);
        }
        else
        {
            velocidadObjetivo = velocidadBase;

            velocidadActual = Mathf.MoveTowards(
                velocidadActual,
                velocidadObjetivo,
                desaceleracion * Time.deltaTime);
        }
    }

    void ManejarCambioCarril()
    {
        int direccion = 0;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            direccion = -1;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            direccion = 1;

        if (direccion != 0)
        {
            int carrilAnterior = carrilActual;

            carrilActual = (carrilActual + direccion + numCarriles) % numCarriles;

            if (Mathf.Abs(carrilActual - carrilAnterior) > numCarriles / 2)
            {
                if (direccion > 0 && carrilActual < carrilAnterior)
                    carrilInterpolado += numCarriles;

                else if (direccion < 0 && carrilActual > carrilAnterior)
                    carrilInterpolado -= numCarriles;
            }
        }

        carrilInterpolado = Mathf.Lerp(
            carrilInterpolado,
            carrilActual,
            suavizadoCambioCarril);
    }

    void ActualizarPosicion()
    {
        if (puntosCurva == null || tangentesCurva == null || puntosCurva.Count < 2 || tangentesCurva.Count < 2)
            return;

        avanceCurva += velocidadActual * Time.deltaTime;

        float distanciaAcumulada = 0f;
        int idx = 0;

        for (int i = 0; i < puntosCurva.Count - 1; i++)
        {
            float tramo = Vector3.Distance(puntosCurva[i], puntosCurva[i + 1]);

            if (distanciaAcumulada + tramo >= avanceCurva)
            {
                idx = i;
                break;
            }

            distanciaAcumulada += tramo;
        }

        float t;

        if (idx >= puntosCurva.Count - 1)
        {
            idx = puntosCurva.Count - 2;
            t = 1f;
        }
        else
        {
            t =
                (avanceCurva - distanciaAcumulada) /
                Vector3.Distance(puntosCurva[idx], puntosCurva[idx + 1]);
        }

        Vector3 posBase = Vector3.Lerp(puntosCurva[idx], puntosCurva[idx + 1], t);

        Vector3 tangente = Vector3.Lerp(
            tangentesCurva[idx],
            tangentesCurva[idx + 1],
            t).normalized;

        Vector3 normal = Vector3.Cross(tangente, Vector3.right).normalized;
        if (normal.magnitude < 0.1f)
            normal = Vector3.Cross(tangente, Vector3.up).normalized;

        float angulo =
            (carrilInterpolado / numCarriles) *
            2f * Mathf.PI;

        Vector3 offsetCarril =
            Quaternion.AngleAxis(Mathf.Rad2Deg * angulo, tangente)
            * normal * radioTunel;

        Vector3 nuevaPos = posBase + offsetCarril;

        Vector3 centroTunel = posBase;

        Vector3 direccionCentro =
            (nuevaPos - centroTunel).normalized;

        nuevaPos =
            centroTunel +
            direccionCentro *
            (radioTunel - transform.localScale.x * 0.5f);

        transform.position = Vector3.Lerp(
            transform.position,
            nuevaPos,
            0.25f);

        transform.rotation =
            Quaternion.LookRotation(
                tangente,
                offsetCarril.normalized);

        idxCamara = idx;
        tCamara = t;
        posBaseCamara = posBase;
    }

    float CalcularLongitudCurva(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2)
            return 0f;

        float longitud = 0f;
        for (int i = 0; i < puntos.Count - 1; i++)
        {
            longitud += Vector3.Distance(puntos[i], puntos[i + 1]);
        }

        return longitud;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Obstacle"))
            return;

        AplicarPenalizacionPorChoque();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Obstacle"))
            return;

        AplicarPenalizacionPorChoque();
    }

    void AplicarPenalizacionPorChoque()
    {
        if (Time.time < tiempoProximoChoqueValido)
            return;

        choquesTotales++;
        tiempoFinPenalizacion = Time.time + tiempoRecuperacionChoque;
        tiempoProximoChoqueValido = Time.time + tiempoInvulnerableTrasChoque;
        velocidadActual = Mathf.Min(velocidadActual, velocidadBase * multiplicadorVelocidadChoque);
    }

    float ParsearTiempo(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return 0f;

        string[] partes = texto.Split(':');
        if (partes.Length != 3)
            return 0f;

        int minutos;
        int segundos;
        int centesimas;
        if (!int.TryParse(partes[0], out minutos) ||
            !int.TryParse(partes[1], out segundos) ||
            !int.TryParse(partes[2], out centesimas))
        {
            return 0f;
        }

        minutos = Mathf.Max(0, minutos);
        segundos = Mathf.Clamp(segundos, 0, 59);
        centesimas = Mathf.Clamp(centesimas, 0, 99);

        return (minutos * 60f) + segundos + (centesimas / 100f);
    }
}