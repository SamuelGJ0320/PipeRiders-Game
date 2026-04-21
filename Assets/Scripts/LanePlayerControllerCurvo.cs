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
    [Header("Niveles")]
    public int totalNiveles = 5;
    public bool mostrarSelectorNiveles = true;
    [Tooltip("Tiempo objetivo por nivel en formato mm:ss:cc")]
    public string[] tiemposObjetivoPorNivel = new string[] { "01:02:55", "01:02:55", "01:02:55", "01:02:55", "01:02:55" };
    [Header("Menu de Inicio (Code UI)")]
    public bool mostrarPantallaInicio = true;
    public string tituloJuego = "PIPE RIDERS";
    [TextArea(2, 4)]
    public string subtituloJuego = "Atraviesa el tunel, evita obstaculos y vence el tiempo objetivo.";
    public Color colorNeonMenu = new Color(0.64f, 0.94f, 1f, 1f);
    [Range(0.7f, 1.5f)]
    public float escalaTituloMenu = 1f;
    [Header("Cuenta Regresiva")]
    public float duracionCuentaRegresiva = 3f;
    public float duracionTextoGo = 0.7f;
    [Header("Feedback de Choque")]
    public bool oscurecerPantallaAlChocar = true;
    [Range(0f, 2f)]
    public float intensidadOscurecimientoChoque = 1f;
    public float duracionOscurecimientoChoque = 2f;
    public AudioClip sonidoChoque;
    [Range(0f, 1f)]
    public float volumenChoque = 0.8f;
    [Header("Particulas de Choque")]
    public bool usarParticulasChoque = true;
    public ParticleSystem prefabParticulasChoque;
    [Tooltip("Si esta activo, el script fuerza color/vida/velocidad sobre el prefab. Si esta apagado, respeta lo que configures en el prefab.")]
    public bool sobrescribirParametrosPrefabParticulas = false;
    [Range(4, 80)]
    public int cantidadParticulasChoque = 24;
    public float vidaParticulasChoque = 0.35f;
    public float velocidadParticulasChoque = 8f;
    public Color colorParticulasChoque = new Color(1f, 0.85f, 0.2f, 1f);
    [Header("Musica")]
    public AudioClip musicaFondo;
    [Tooltip("Musica por nivel. Si un nivel no tiene clip asignado aqui, usa musicaFondo.")]
    public AudioClip[] musicaPorNivel;
    [Range(0f, 1f)]
    public float volumenMusica = 0.45f;

    private int carrilActual = 0;
    private float carrilInterpolado = 0f;
    private float carrilObjetivoInterpolado = 0f;
    private float avanceCurva = 0f;
    private float velocidadActual;
    private List<Vector3> puntosCurva;
    private List<Vector3> tangentesCurva;

    [Header("Cámara Pipe Riders")]
    public Transform camaraPipeRiders;
    public float suavidadCamara = 0.15f;
    [Range(0.7f, 1.3f)]
    public float multiplicadorDistanciaCamara = 0.92f;
    [Header("Visual Moto")]
    [Tooltip("Arrastra aqui el child visual de la moto para mantenerla derecha (sin voltearse).")]
    public Transform visualMoto;
    [Tooltip("Ajuste fino de orientacion de la moto visual.")]
    public Vector3 visualMotoOffsetEuler = Vector3.zero;
    [Tooltip("Si la moto queda mirando hacia atras en Play, activa este check.")]
    public bool invertirFrenteVisualMoto = false;
    [Tooltip("Offset local del modelo visual respecto al jugador.")]
    public Vector3 visualMotoOffsetPosLocal = Vector3.zero;
    [Header("Animacion Visual Moto")]
    public bool animarVisualMoto = true;
    public float inclinacionMaximaVisual = 16f;
    public float suavizadoInclinacionVisual = 8f;
    public float fuerzaImpulsoInclinacionVisual = 1.35f;
    public float velocidadCaidaImpulsoVisual = 3.2f;
    public float amplitudCabeceoVisual = 1.8f;
    public float frecuenciaCabeceoVisual = 8f;

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
    private bool enPantallaInicio = false;
    private float tiempoPreinicio = 0f;
    private float tiempoObjetivoSegundos = 0f;
    private int tiempoObjetivoCentesimas = 0;
    private int nivelActualJuego = 1;
    private bool enSelectorNivel = false;
    private bool enPausa = false;
    private float oscurecimientoActual = 0f;
    private float tiempoOscurecimientoRestante = 0f;
    private Quaternion visualMotoRotInicialLocal = Quaternion.identity;
    private Vector3 visualMotoPosInicialLocal = Vector3.zero;
    private bool visualMotoInicialCapturada = false;
    private float inclinacionVisualActual = 0f;
    private float impulsoInclinacionVisual = 0f;
    private float tiempoAnimVisual = 0f;
    private Texture2D texturaNegra;
    private AudioSource audioSource;
    private AudioSource audioMusicaSource;
    private ParticleSystem particulasChoqueRuntime;
    private GUIStyle estiloCronometro;
    private GUIStyle estiloVelocidad;
    private GUIStyle estiloCentro;
    private GUIStyle estiloNivelSuperior;
    private GUIStyle estiloMenuWelcome;
    private GUIStyle estiloMenuTitulo;
    private GUIStyle estiloMenuTituloGlow;
    private GUIStyle estiloMenuSubtitulo;
    private GUIStyle estiloMenuBoton;
    private GUIStyle estiloMenuBotonSeleccionado;
    private GUIStyle estiloMenuAyuda;
    private Texture2D texturaBlancaUI;
    private int opcionMenuInicioSeleccionada = 0;
    private bool enMenuAjustes = false;
    private int opcionMenuAjustesSeleccionada = 0;
    private int indicePresetFpsSeleccionado = 0;
    private static readonly int[] presetsFps = new int[] { 60, 120, 144, 165, 240 };

    void OnValidate()
    {
        totalNiveles = Mathf.Max(1, totalNiveles);
        numCarriles = Mathf.Max(2, numCarriles);
        volumenChoque = Mathf.Clamp01(volumenChoque);
        volumenMusica = Mathf.Clamp01(volumenMusica);
        cantidadParticulasChoque = Mathf.Max(1, cantidadParticulasChoque);
        vidaParticulasChoque = Mathf.Max(0.05f, vidaParticulasChoque);
        velocidadParticulasChoque = Mathf.Max(0.1f, velocidadParticulasChoque);
        inclinacionMaximaVisual = Mathf.Clamp(inclinacionMaximaVisual, 0f, 45f);
        suavizadoInclinacionVisual = Mathf.Max(0.1f, suavizadoInclinacionVisual);
        amplitudCabeceoVisual = Mathf.Clamp(amplitudCabeceoVisual, 0f, 12f);
        frecuenciaCabeceoVisual = Mathf.Max(0f, frecuenciaCabeceoVisual);
        fuerzaImpulsoInclinacionVisual = Mathf.Clamp(fuerzaImpulsoInclinacionVisual, 0f, 3f);
        velocidadCaidaImpulsoVisual = Mathf.Max(0.1f, velocidadCaidaImpulsoVisual);
        escalaTituloMenu = Mathf.Clamp(escalaTituloMenu, 0.7f, 1.5f);
        multiplicadorDistanciaCamara = Mathf.Clamp(multiplicadorDistanciaCamara, 0.7f, 1.3f);

        AsegurarTiemposObjetivo();
        AsegurarMusicaPorNivel();

        if (audioMusicaSource != null)
        {
            audioMusicaSource.volume = volumenMusica;
        }
    }

    void Start()
    {
        if (tunnelGenerator == null)
        {
            Debug.LogError("Falta referencia a TunnelGenerator en LanePlayerControllerCurvo");
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
        audioSource.volume = 1f;
        audioSource.ignoreListenerPause = true;

        audioMusicaSource = gameObject.AddComponent<AudioSource>();
        audioMusicaSource.playOnAwake = false;
        audioMusicaSource.loop = true;
        audioMusicaSource.spatialBlend = 0f;
        audioMusicaSource.ignoreListenerPause = true;
        audioMusicaSource.volume = Mathf.Clamp01(volumenMusica);
        audioMusicaSource.clip = null;

        if (sonidoChoque == null)
        {
            Debug.LogWarning("No hay AudioClip asignado en 'sonidoChoque'. Importa un .wav/.mp3 y asignalo en el Inspector del player.");
        }

        carrilActual = numCarriles / 2;
        carrilInterpolado = carrilActual;
        carrilObjetivoInterpolado = carrilActual;
        velocidadInicialNivel = Mathf.Max(2f, velocidadBase * Mathf.Clamp01(velocidadInicialFactor));
        velocidadActual = 0f;
        avanceCurva = 0f;
        tiempoInicioNivel = Time.time;
        tiempoPreinicio = Mathf.Max(0f, duracionCuentaRegresiva) + Mathf.Max(0f, duracionTextoGo);

        AsegurarTiemposObjetivo();
        AsegurarMusicaPorNivel();

        if (tunnelGenerator != null && tunnelGenerator.tunnelPrefab != null)
        {
            float escala = tunnelGenerator.tunnelPrefab.transform.localScale.x;
            radioTunel = escala * 0.5f;
        }

        // Posicion inicial consistente para evitar salto/angulo raro durante 3-2-1-GO.
        if (mostrarPantallaInicio)
        {
            enPantallaInicio = true;
            enSelectorNivel = false;
            nivelIniciado = false;
            nivelFinalizado = false;
            cronometroDetenido = false;
            velocidadActual = 0f;

            AudioClip clipInicio = ObtenerMusicaContextoActual();
            if (audioMusicaSource != null)
            {
                audioMusicaSource.clip = clipInicio;
                if (clipInicio != null)
                {
                    audioMusicaSource.Play();
                }
            }
        }
        else if (mostrarSelectorNiveles)
        {
            enSelectorNivel = true;
            PrepararSelectorNivel();
        }
        else
        {
            IniciarNivel(1);
        }

        if (visualMoto == null && transform.childCount > 0)
        {
            visualMoto = transform.GetChild(0);
            Debug.LogWarning("'visualMoto' no estaba asignado. Se uso automaticamente el primer child del player para animacion visual.");
        }

        if (visualMoto != null)
        {
            visualMotoRotInicialLocal = visualMoto.localRotation;
            visualMotoPosInicialLocal = visualMoto.localPosition;
            visualMotoInicialCapturada = true;
        }

        indicePresetFpsSeleccionado = ObtenerIndicePresetMasCercano(RuntimeFramePacing.ObtenerFpsConfigurado());

        // La colision con obstaculos requiere Rigidbody + Collider en el player.
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (!enPantallaInicio && !enSelectorNivel && !nivelFinalizado)
            {
                if (enPausa)
                {
                    ReanudarJuego();
                }
                else
                {
                    PausarJuego();
                }
            }
        }

        if (tiempoOscurecimientoRestante > 0f)
        {
            tiempoOscurecimientoRestante = Mathf.Max(0f, tiempoOscurecimientoRestante - Time.deltaTime);
            float t = duracionOscurecimientoChoque > 0.001f ? (tiempoOscurecimientoRestante / duracionOscurecimientoChoque) : 0f;
            oscurecimientoActual = intensidadOscurecimientoChoque * Mathf.Clamp01(t);
        }
        else
        {
            oscurecimientoActual = 0f;
        }

        if (enPantallaInicio)
        {
            ManejarInputPantallaInicio();
            return;
        }

        if (enSelectorNivel)
            return;

        if (enPausa)
            return;

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
            int tiempoActualCentesimas = ConvertirACentesimas(tiempoTranscurrido);
            bool cumpleTiempo = tiempoActualCentesimas <= tiempoObjetivoCentesimas;
            bool cumpleChoques = !exigirCeroChoquesParaGanar || choquesTotales == 0;
            jugadorGano = cumpleTiempo && cumpleChoques;
            velocidadActual = 0f;
            Debug.Log($"Cronometro detenido al final del tunel: {FormatearTiempo(tiempoTranscurrido)}");
        }
    }

    void OnGUI()
    {
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

        if (estiloNivelSuperior == null)
        {
            estiloNivelSuperior = new GUIStyle(GUI.skin.label);
            estiloNivelSuperior.fontSize = Mathf.Max(16, tamanoFuenteCronometro - 10);
            estiloNivelSuperior.fontStyle = FontStyle.Bold;
            estiloNivelSuperior.normal.textColor = Color.white;
            estiloNivelSuperior.alignment = TextAnchor.UpperCenter;
        }

        if (enSelectorNivel)
        {
            DibujarSelectorNiveles();
            DibujarOscurecimiento();
            return;
        }

        if (enPantallaInicio)
        {
            if (enMenuAjustes)
            {
                DibujarMenuAjustes();
            }
            else
            {
                DibujarMenuInicioProfesional();
            }
            return;
        }

        if (!nivelFinalizado)
        {
            string txtPausa = enPausa ? "Reanudar" : "Pausa";
            if (GUI.Button(new Rect(20f, 20f, 110f, 34f), txtPausa))
            {
                if (enPausa)
                {
                    ReanudarJuego();
                }
                else
                {
                    PausarJuego();
                }
            }
        }

        GUI.Label(
            new Rect(0f, 14f, Screen.width, 34f),
            $"Nivel {nivelActualJuego}",
            estiloNivelSuperior);

        if (mostrarCronometro)
        {
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
        }

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

            float btnW = 220f;
            float btnH = 42f;
            float x = (Screen.width - btnW) * 0.5f;
            float y = (Screen.height * 0.5f) + 70f;
            float ySiguiente = y + 50f;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "Reintentar Nivel"))
            {
                IniciarNivel(nivelActualJuego);
            }

            if (jugadorGano && nivelActualJuego < totalNiveles)
            {
                if (GUI.Button(new Rect(x, ySiguiente, btnW, btnH), "Continuar"))
                {
                    IniciarNivel(nivelActualJuego + 1);
                }
                ySiguiente += 50f;
            }

            if (mostrarSelectorNiveles)
            {
                if (GUI.Button(new Rect(x, ySiguiente, btnW, btnH), "Seleccionar Nivel"))
                {
                    enSelectorNivel = true;
                    PrepararSelectorNivel();
                }
                ySiguiente += 50f;
            }

            if (nivelActualJuego >= totalNiveles)
            {
                if (GUI.Button(new Rect(x, ySiguiente, btnW, btnH), "Menu Principal"))
                {
                    IrAMenuPrincipal();
                }
            }
        }

        if (enPausa)
        {
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "PAUSA", estiloCentro);

            float btnW = 240f;
            float btnH = 44f;
            float x = (Screen.width - btnW) * 0.5f;
            float y = (Screen.height * 0.5f) + 40f;

            if (GUI.Button(new Rect(x, y, btnW, btnH), "Reanudar"))
            {
                ReanudarJuego();
            }

            if (GUI.Button(new Rect(x, y + 52f, btnW, btnH), "Reiniciar Nivel"))
            {
                ReanudarJuego();
                IniciarNivel(nivelActualJuego);
            }

            if (GUI.Button(new Rect(x, y + 104f, btnW, btnH), "Volver al Selector"))
            {
                ReanudarJuego();
                enSelectorNivel = true;
                PrepararSelectorNivel();
            }
        }

        DibujarOscurecimiento();
    }

    void DibujarSelectorNiveles()
    {
        float panelW = 440f;
        float panelH = 320f;
        float px = (Screen.width - panelW) * 0.5f;
        float py = (Screen.height - panelH) * 0.5f;

        GUI.Box(new Rect(px, py, panelW, panelH), "Selecciona Nivel");

        float btnW = 120f;
        float btnH = 44f;
        float startX = px + 30f;
        float startY = py + 60f;
        float gapX = 20f;
        float gapY = 20f;

        int columnas = 3;
        for (int i = 1; i <= totalNiveles; i++)
        {
            int idx = i - 1;
            int col = idx % columnas;
            int row = idx / columnas;

            float x = startX + col * (btnW + gapX);
            float y = startY + row * (btnH + gapY);
            string txt = $"Nivel {i}\n{ObtenerTiempoObjetivo(i)}";

            if (GUI.Button(new Rect(x, y, btnW, btnH), txt))
            {
                IniciarNivel(i);
            }
        }

        float btnMenuW = 220f;
        float btnMenuH = 38f;
        float btnMenuX = px + (panelW - btnMenuW) * 0.5f;
        float btnMenuY = py + panelH - btnMenuH - 16f;
        if (GUI.Button(new Rect(btnMenuX, btnMenuY, btnMenuW, btnMenuH), "Menu Principal"))
        {
            IrAMenuPrincipal();
        }
    }

    void IrAMenuPrincipal()
    {
        ReanudarJuego();

        enPantallaInicio = true;
        enSelectorNivel = false;
        nivelIniciado = false;
        nivelFinalizado = false;
        cronometroDetenido = false;
        jugadorGano = false;
        velocidadActual = 0f;
        tiempoTranscurrido = 0f;
        opcionMenuInicioSeleccionada = 0;
        enMenuAjustes = false;
        opcionMenuAjustesSeleccionada = 0;
        indicePresetFpsSeleccionado = ObtenerIndicePresetMasCercano(RuntimeFramePacing.ObtenerFpsConfigurado());

        if (audioMusicaSource != null)
        {
            AudioClip clipInicio = ObtenerMusicaContextoActual();
            audioMusicaSource.clip = clipInicio;
            if (clipInicio != null)
            {
                audioMusicaSource.Play();
            }
            else
            {
                audioMusicaSource.Stop();
            }
        }
    }

    void ManejarInputPantallaInicio()
    {
        if (enMenuAjustes)
        {
            ManejarInputMenuAjustes();
            return;
        }

        int totalOpciones = mostrarSelectorNiveles ? 4 : 3;
        opcionMenuInicioSeleccionada = Mathf.Clamp(opcionMenuInicioSeleccionada, 0, totalOpciones - 1);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            opcionMenuInicioSeleccionada = (opcionMenuInicioSeleccionada - 1 + totalOpciones) % totalOpciones;
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            opcionMenuInicioSeleccionada = (opcionMenuInicioSeleccionada + 1) % totalOpciones;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            EjecutarAccionMenuInicio(opcionMenuInicioSeleccionada);
        }
    }

    void EjecutarAccionMenuInicio(int opcion)
    {
        if (opcion <= 0)
        {
            enPantallaInicio = false;
            IniciarNivel(1);
            return;
        }

        int idx = 1;

        if (mostrarSelectorNiveles && opcion == idx)
        {
            enPantallaInicio = false;
            enSelectorNivel = true;
            PrepararSelectorNivel();
            return;
        }

        if (mostrarSelectorNiveles)
            idx++;

        if (opcion == idx)
        {
            enMenuAjustes = true;
            opcionMenuAjustesSeleccionada = 0;
            indicePresetFpsSeleccionado = ObtenerIndicePresetMasCercano(RuntimeFramePacing.ObtenerFpsConfigurado());
            return;
        }

        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("Salir presionado (en editor no se cierra la aplicacion).");
#endif
    }

    void ManejarInputMenuAjustes()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            enMenuAjustes = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            opcionMenuAjustesSeleccionada = (opcionMenuAjustesSeleccionada - 1 + 2) % 2;
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            opcionMenuAjustesSeleccionada = (opcionMenuAjustesSeleccionada + 1) % 2;
        }

        if (opcionMenuAjustesSeleccionada == 0)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                CambiarPresetFps(-1);
            }

            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                CambiarPresetFps(1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            if (opcionMenuAjustesSeleccionada == 1)
            {
                enMenuAjustes = false;
            }
        }
    }

    int ObtenerIndicePresetMasCercano(int fpsActual)
    {
        int indice = 0;
        int mejorDelta = int.MaxValue;
        for (int i = 0; i < presetsFps.Length; i++)
        {
            int delta = Mathf.Abs(presetsFps[i] - fpsActual);
            if (delta < mejorDelta)
            {
                mejorDelta = delta;
                indice = i;
            }
        }

        return indice;
    }

    void CambiarPresetFps(int direccion)
    {
        indicePresetFpsSeleccionado = (indicePresetFpsSeleccionado + direccion + presetsFps.Length) % presetsFps.Length;
        RuntimeFramePacing.ConfigurarFps(presetsFps[indicePresetFpsSeleccionado]);
    }

    void DibujarMenuInicioProfesional()
    {
        if (texturaBlancaUI == null)
        {
            texturaBlancaUI = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texturaBlancaUI.SetPixel(0, 0, Color.white);
            texturaBlancaUI.Apply();
        }

        if (estiloMenuWelcome == null)
        {
            estiloMenuWelcome = new GUIStyle(GUI.skin.label);
            estiloMenuWelcome.alignment = TextAnchor.MiddleCenter;
            estiloMenuWelcome.fontStyle = FontStyle.Normal;
            estiloMenuWelcome.fontSize = Mathf.RoundToInt(44f * escalaTituloMenu);
        }

        if (estiloMenuTitulo == null)
        {
            estiloMenuTitulo = new GUIStyle(GUI.skin.label);
            estiloMenuTitulo.alignment = TextAnchor.MiddleCenter;
            estiloMenuTitulo.fontStyle = FontStyle.BoldAndItalic;
            estiloMenuTitulo.fontSize = Mathf.RoundToInt(102f * escalaTituloMenu);
        }

        if (estiloMenuTituloGlow == null)
        {
            estiloMenuTituloGlow = new GUIStyle(estiloMenuTitulo);
        }

        if (estiloMenuSubtitulo == null)
        {
            estiloMenuSubtitulo = new GUIStyle(GUI.skin.label);
            estiloMenuSubtitulo.alignment = TextAnchor.MiddleCenter;
            estiloMenuSubtitulo.fontSize = Mathf.RoundToInt(24f * escalaTituloMenu);
            estiloMenuSubtitulo.wordWrap = true;
        }

        if (estiloMenuBoton == null)
        {
            estiloMenuBoton = new GUIStyle(GUI.skin.button);
            estiloMenuBoton.alignment = TextAnchor.MiddleCenter;
            estiloMenuBoton.fontStyle = FontStyle.Bold;
            estiloMenuBoton.fontSize = Mathf.RoundToInt(24f * escalaTituloMenu);
            estiloMenuBoton.normal.textColor = new Color(0.95f, 0.97f, 1f, 1f);
            estiloMenuBoton.hover.textColor = Color.white;
            estiloMenuBoton.active.textColor = Color.white;
        }

        if (estiloMenuBotonSeleccionado == null)
        {
            estiloMenuBotonSeleccionado = new GUIStyle(estiloMenuBoton);
        }

        if (estiloMenuAyuda == null)
        {
            estiloMenuAyuda = new GUIStyle(GUI.skin.label);
            estiloMenuAyuda.alignment = TextAnchor.MiddleCenter;
            estiloMenuAyuda.fontSize = Mathf.RoundToInt(16f * escalaTituloMenu);
        }

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.1f);
        Color colorGlow = Color.Lerp(colorNeonMenu * 0.4f, colorNeonMenu, pulse);

        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), texturaBlancaUI);

        float hazeAlpha = Mathf.Lerp(0.07f, 0.18f, pulse);
        GUI.color = new Color(colorNeonMenu.r, colorNeonMenu.g, colorNeonMenu.b, hazeAlpha);
        GUI.DrawTexture(new Rect(0f, Screen.height * 0.27f, Screen.width, Screen.height * 0.05f), texturaBlancaUI);

        float centerX = Screen.width * 0.5f;
        float welcomeY = Screen.height * 0.22f;
        float titleY = Screen.height * 0.30f;
        float subtitleY = Screen.height * 0.42f;

        estiloMenuWelcome.normal.textColor = new Color(colorGlow.r, colorGlow.g, colorGlow.b, 0.95f);
        GUI.Label(new Rect(centerX - 360f, welcomeY, 720f, 60f), "WELCOME TO", estiloMenuWelcome);

        string titulo = string.IsNullOrWhiteSpace(tituloJuego) ? "PIPE RIDERS" : tituloJuego.ToUpperInvariant();
        estiloMenuTituloGlow.normal.textColor = new Color(colorGlow.r, colorGlow.g, colorGlow.b, 0.35f);
        GUI.Label(new Rect(centerX - 560f + 3f, titleY - 2f, 1120f, 120f), titulo, estiloMenuTituloGlow);
        GUI.Label(new Rect(centerX - 560f - 3f, titleY + 2f, 1120f, 120f), titulo, estiloMenuTituloGlow);

        estiloMenuTitulo.normal.textColor = new Color(0.95f, 0.97f, 1f, 1f);
        GUI.Label(new Rect(centerX - 560f, titleY, 1120f, 120f), titulo, estiloMenuTitulo);

        estiloMenuSubtitulo.normal.textColor = new Color(0.78f, 0.87f, 0.93f, 0.92f);
        GUI.Label(new Rect(centerX - 430f, subtitleY, 860f, 80f), subtituloJuego, estiloMenuSubtitulo);

        float btnW = 350f;
        float btnH = 56f;
        float btnX = centerX - (btnW * 0.5f);
        float btnY = Screen.height * 0.57f;
        float btnGap = 16f;

        string[] labels = mostrarSelectorNiveles
            ? new string[] { "START", "LEVEL SELECT", "SETTINGS", "QUIT" }
            : new string[] { "START", "SETTINGS", "QUIT" };

        for (int i = 0; i < labels.Length; i++)
        {
            Rect r = new Rect(btnX, btnY + i * (btnH + btnGap), btnW, btnH);
            bool hover = r.Contains(Event.current.mousePosition);
            bool selected = opcionMenuInicioSeleccionada == i || hover;

            if (hover)
                opcionMenuInicioSeleccionada = i;

            Color borde = selected
                ? new Color(colorNeonMenu.r, colorNeonMenu.g, colorNeonMenu.b, 1f)
                : new Color(0.80f, 0.88f, 0.95f, 0.85f);
            Color fondo = selected
                ? new Color(0.06f, 0.20f, 0.30f, 0.92f)
                : new Color(0.04f, 0.09f, 0.14f, 0.88f);

            GUI.color = new Color(borde.r, borde.g, borde.b, selected ? 0.34f : 0.18f);
            GUI.DrawTexture(new Rect(r.x - 4f, r.y - 4f, r.width + 8f, r.height + 8f), texturaBlancaUI);

            GUI.color = borde;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 2f), texturaBlancaUI);
            GUI.DrawTexture(new Rect(r.x, r.yMax - 2f, r.width, 2f), texturaBlancaUI);
            GUI.DrawTexture(new Rect(r.x, r.y, 2f, r.height), texturaBlancaUI);
            GUI.DrawTexture(new Rect(r.xMax - 2f, r.y, 2f, r.height), texturaBlancaUI);

            GUI.color = fondo;
            GUI.DrawTexture(new Rect(r.x + 2f, r.y + 2f, r.width - 4f, r.height - 4f), texturaBlancaUI);

            GUI.color = Color.white;
            if (GUI.Button(r, labels[i], selected ? estiloMenuBotonSeleccionado : estiloMenuBoton))
            {
                opcionMenuInicioSeleccionada = i;
                EjecutarAccionMenuInicio(i);
            }
        }

        estiloMenuAyuda.normal.textColor = new Color(0.72f, 0.80f, 0.88f, 0.92f);
        GUI.Label(
            new Rect(0f, Screen.height - 48f, Screen.width, 32f),
            "Use W/S para navegar, ENTER para confirmar, ESC para volver",
            estiloMenuAyuda);

        GUI.color = Color.white;
    }

    void DibujarMenuAjustes()
    {
        if (texturaBlancaUI == null)
        {
            texturaBlancaUI = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texturaBlancaUI.SetPixel(0, 0, Color.white);
            texturaBlancaUI.Apply();
        }

        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), texturaBlancaUI);

        float centerX = Screen.width * 0.5f;
        float panelW = 640f;
        float panelH = 340f;
        Rect panel = new Rect(centerX - panelW * 0.5f, Screen.height * 0.5f - panelH * 0.5f, panelW, panelH);

        GUI.color = new Color(0.08f, 0.14f, 0.20f, 0.92f);
        GUI.DrawTexture(panel, texturaBlancaUI);

        GUI.color = new Color(colorNeonMenu.r, colorNeonMenu.g, colorNeonMenu.b, 0.9f);
        GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 2f), texturaBlancaUI);
        GUI.DrawTexture(new Rect(panel.x, panel.yMax - 2f, panel.width, 2f), texturaBlancaUI);

        GUI.color = Color.white;
        GUIStyle titulo = new GUIStyle(GUI.skin.label);
        titulo.alignment = TextAnchor.MiddleCenter;
        titulo.fontStyle = FontStyle.Bold;
        titulo.fontSize = 44;
        titulo.normal.textColor = new Color(0.90f, 0.95f, 1f, 1f);
        GUI.Label(new Rect(panel.x, panel.y + 20f, panel.width, 48f), "SETTINGS", titulo);

        GUIStyle subtitulo = new GUIStyle(GUI.skin.label);
        subtitulo.alignment = TextAnchor.MiddleCenter;
        subtitulo.fontSize = 22;
        subtitulo.normal.textColor = new Color(0.75f, 0.86f, 0.94f, 1f);
        GUI.Label(new Rect(panel.x, panel.y + 86f, panel.width, 36f), "FPS LIMIT", subtitulo);

        int fpsActual = RuntimeFramePacing.ObtenerFpsConfigurado();
        indicePresetFpsSeleccionado = ObtenerIndicePresetMasCercano(fpsActual);

        float rowY = panel.y + 140f;
        float btnSmallW = 72f;
        float btnSmallH = 48f;
        float valorW = 190f;
        float totalW = btnSmallW + 18f + valorW + 18f + btnSmallW;
        float startX = centerX - totalW * 0.5f;

        bool filaSeleccionada = opcionMenuAjustesSeleccionada == 0;
        if (GUI.Button(new Rect(startX, rowY, btnSmallW, btnSmallH), "<"))
        {
            CambiarPresetFps(-1);
        }

        Color valorColor = filaSeleccionada
            ? new Color(0.10f, 0.28f, 0.40f, 0.95f)
            : new Color(0.06f, 0.16f, 0.24f, 0.90f);
        GUI.color = valorColor;
        GUI.DrawTexture(new Rect(startX + btnSmallW + 18f, rowY, valorW, btnSmallH), texturaBlancaUI);
        GUI.color = Color.white;

        GUIStyle valor = new GUIStyle(GUI.skin.label);
        valor.alignment = TextAnchor.MiddleCenter;
        valor.fontStyle = FontStyle.Bold;
        valor.fontSize = 24;
        valor.normal.textColor = Color.white;
        GUI.Label(new Rect(startX + btnSmallW + 18f, rowY, valorW, btnSmallH), fpsActual + " FPS", valor);

        if (GUI.Button(new Rect(startX + btnSmallW + 18f + valorW + 18f, rowY, btnSmallW, btnSmallH), ">"))
        {
            CambiarPresetFps(1);
        }

        float chipW = 88f;
        float chipH = 36f;
        float chipGap = 10f;
        float chipsTotal = presetsFps.Length * chipW + (presetsFps.Length - 1) * chipGap;
        float chipsX = centerX - chipsTotal * 0.5f;
        float chipsY = rowY + 66f;

        for (int i = 0; i < presetsFps.Length; i++)
        {
            bool selected = i == indicePresetFpsSeleccionado;
            GUI.color = selected
                ? new Color(colorNeonMenu.r, colorNeonMenu.g, colorNeonMenu.b, 0.35f)
                : Color.white;

            if (GUI.Button(new Rect(chipsX + i * (chipW + chipGap), chipsY, chipW, chipH), presetsFps[i].ToString()))
            {
                indicePresetFpsSeleccionado = i;
                RuntimeFramePacing.ConfigurarFps(presetsFps[i]);
            }
        }

        GUI.color = Color.white;

        float volverW = 220f;
        float volverH = 46f;
        Rect btnVolver = new Rect(centerX - volverW * 0.5f, panel.yMax - volverH - 24f, volverW, volverH);
        if (opcionMenuAjustesSeleccionada == 1)
        {
            GUI.color = new Color(colorNeonMenu.r, colorNeonMenu.g, colorNeonMenu.b, 0.30f);
            GUI.DrawTexture(new Rect(btnVolver.x - 3f, btnVolver.y - 3f, btnVolver.width + 6f, btnVolver.height + 6f), texturaBlancaUI);
            GUI.color = Color.white;
        }
        if (GUI.Button(btnVolver, "BACK"))
        {
            enMenuAjustes = false;
            opcionMenuAjustesSeleccionada = 0;
        }

        GUIStyle ayuda = new GUIStyle(GUI.skin.label);
        ayuda.alignment = TextAnchor.MiddleCenter;
        ayuda.fontSize = 15;
        ayuda.normal.textColor = new Color(0.73f, 0.82f, 0.90f, 0.95f);
        GUI.Label(
            new Rect(0f, Screen.height - 44f, Screen.width, 30f),
            "A/D o Flechas para cambiar FPS, W/S para navegar, ENTER para volver",
            ayuda);
    }

    void DibujarOscurecimiento()
    {
        if (!oscurecerPantallaAlChocar || oscurecimientoActual <= 0.001f)
            return;

        if (texturaNegra == null)
        {
            texturaNegra = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texturaNegra.SetPixel(0, 0, Color.black);
            texturaNegra.Apply();
        }

        Color anterior = GUI.color;
        float alpha = Mathf.Clamp01(oscurecimientoActual * 1.35f);
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), texturaNegra);
        GUI.color = anterior;
    }

    string FormatearTiempo(float segundosTotales)
    {
        int totalCentesimas = ConvertirACentesimas(segundosTotales);
        int minutos = totalCentesimas / 6000;
        int segundos = (totalCentesimas % 6000) / 100;
        int centesimas = totalCentesimas % 100;
        return $"{minutos:00}:{segundos:00}:{centesimas:00}";
    }

    int ConvertirACentesimas(float segundosTotales)
    {
        return Mathf.Max(0, Mathf.RoundToInt(segundosTotales * 100f));
    }

    void LateUpdate()
    {
        if (camaraPipeRiders == null) return;

        Vector3 tangente = transform.forward;
        float distanciaCamaraAjustada = radioTunel * 1.08f * multiplicadorDistanciaCamara;
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

        ActualizarVisualMoto();
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
            impulsoInclinacionVisual = Mathf.Clamp(
                impulsoInclinacionVisual + (-direccion * fuerzaImpulsoInclinacionVisual),
                -2.5f,
                2.5f);

            carrilActual = (carrilActual + direccion + numCarriles) % numCarriles;
            carrilObjetivoInterpolado += direccion;
        }

        carrilInterpolado = Mathf.Lerp(
            carrilInterpolado,
            carrilObjetivoInterpolado,
            suavizadoCambioCarril);

        impulsoInclinacionVisual = Mathf.MoveTowards(
            impulsoInclinacionVisual,
            0f,
            velocidadCaidaImpulsoVisual * Time.deltaTime);
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

        posBaseCamara = posBase;
    }

    void IniciarNivel(int nivel)
    {
        ReanudarJuego();

        nivelActualJuego = Mathf.Clamp(nivel, 1, Mathf.Max(1, totalNiveles));
        enSelectorNivel = false;
        nivelIniciado = false;
        nivelFinalizado = false;
        jugadorGano = false;
        cronometroDetenido = false;
        choquesTotales = 0;
        tiempoTranscurrido = 0f;
        tiempoFinPenalizacion = 0f;
        tiempoProximoChoqueValido = 0f;
        oscurecimientoActual = 0f;
        tiempoOscurecimientoRestante = 0f;

        carrilActual = numCarriles / 2;
        carrilInterpolado = carrilActual;
        carrilObjetivoInterpolado = carrilActual;
        avanceCurva = 0f;
        velocidadActual = 0f;
        tiempoPreinicio = Mathf.Max(0f, duracionCuentaRegresiva) + Mathf.Max(0f, duracionTextoGo);

        tunnelGenerator.nivelActual = nivelActualJuego;
        tunnelGenerator.GenerarNivelProcedural();

        puntosCurva = tunnelGenerator.GetPuntosCurva();
        tangentesCurva = tunnelGenerator.GetTangentesCurva();
        longitudTotalCurva = CalcularLongitudCurva(puntosCurva);

        tiempoObjetivoTexto = ObtenerTiempoObjetivo(nivelActualJuego);
        tiempoObjetivoSegundos = ParsearTiempo(tiempoObjetivoTexto);
        tiempoObjetivoCentesimas = ConvertirACentesimas(tiempoObjetivoSegundos);

        ReproducirMusicaNivel(nivelActualJuego, true);

        ActualizarPosicion();
    }

    void PrepararSelectorNivel()
    {
        ReanudarJuego();
        nivelIniciado = false;
        nivelFinalizado = false;
        cronometroDetenido = false;
        velocidadActual = 0f;
    }

    void PausarJuego()
    {
        enPausa = true;
        Time.timeScale = 0f;
        if (audioMusicaSource != null && audioMusicaSource.isPlaying)
        {
            audioMusicaSource.Pause();
        }
    }

    void ReanudarJuego()
    {
        enPausa = false;
        Time.timeScale = 1f;
        if (audioMusicaSource != null)
        {
            AudioClip clipNivel = ObtenerMusicaContextoActual();

            if (audioMusicaSource.clip != clipNivel)
            {
                audioMusicaSource.clip = clipNivel;
                if (clipNivel != null)
                {
                    audioMusicaSource.Play();
                }
                else
                {
                    audioMusicaSource.Stop();
                }
                return;
            }

            if (clipNivel != null && !audioMusicaSource.isPlaying)
            {
                audioMusicaSource.UnPause();
                if (!audioMusicaSource.isPlaying)
                {
                    audioMusicaSource.Play();
                }
            }
        }
    }

    void AsegurarTiemposObjetivo()
    {
        if (totalNiveles < 1)
            totalNiveles = 1;

        if (tiemposObjetivoPorNivel == null || tiemposObjetivoPorNivel.Length != totalNiveles)
        {
            string[] porDefecto = new string[] { "01:02:55", "01:02:55", "01:02:55", "01:02:55", "01:02:55" };
            string[] nuevo = new string[totalNiveles];
            for (int i = 0; i < totalNiveles; i++)
            {
                if (i < porDefecto.Length)
                    nuevo[i] = porDefecto[i];
                else
                    nuevo[i] = porDefecto[porDefecto.Length - 1];
            }
            tiemposObjetivoPorNivel = nuevo;
        }

        for (int i = 0; i < tiemposObjetivoPorNivel.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(tiemposObjetivoPorNivel[i]))
            {
                tiemposObjetivoPorNivel[i] = "01:02:55";
            }
        }
    }

    void AsegurarMusicaPorNivel()
    {
        if (totalNiveles < 1)
            totalNiveles = 1;

        if (musicaPorNivel == null || musicaPorNivel.Length != totalNiveles)
        {
            AudioClip[] previo = musicaPorNivel;
            AudioClip[] nuevo = new AudioClip[totalNiveles];
            if (previo != null)
            {
                int copiar = Mathf.Min(previo.Length, nuevo.Length);
                for (int i = 0; i < copiar; i++)
                {
                    nuevo[i] = previo[i];
                }
            }
            musicaPorNivel = nuevo;
        }
    }

    AudioClip ObtenerMusicaNivel(int nivel)
    {
        AsegurarMusicaPorNivel();
        int idx = Mathf.Clamp(nivel - 1, 0, musicaPorNivel.Length - 1);
        AudioClip clipNivel = musicaPorNivel[idx];
        return clipNivel != null ? clipNivel : musicaFondo;
    }

    AudioClip ObtenerMusicaContextoActual()
    {
        if (enPantallaInicio || enSelectorNivel)
            return musicaFondo;

        return ObtenerMusicaNivel(nivelActualJuego);
    }

    void ReproducirMusicaNivel(int nivel, bool reiniciar)
    {
        if (audioMusicaSource == null)
            return;

        audioMusicaSource.volume = Mathf.Clamp01(volumenMusica);
        AudioClip clip = ObtenerMusicaNivel(nivel);

        if (clip == null)
        {
            audioMusicaSource.Stop();
            audioMusicaSource.clip = null;
            return;
        }

        bool cambioClip = audioMusicaSource.clip != clip;
        audioMusicaSource.clip = clip;
        if (reiniciar || cambioClip || !audioMusicaSource.isPlaying)
        {
            audioMusicaSource.Play();
        }
    }

    string ObtenerTiempoObjetivo(int nivel)
    {
        AsegurarTiemposObjetivo();
        int idx = Mathf.Clamp(nivel - 1, 0, tiemposObjetivoPorNivel.Length - 1);
        string t = tiemposObjetivoPorNivel[idx];
        if (string.IsNullOrWhiteSpace(t))
            return "01:02:55";
        return t;
    }

    void ActualizarVisualMoto()
    {
        if (visualMoto == null)
            return;

        if (!visualMotoInicialCapturada)
        {
            visualMotoRotInicialLocal = visualMoto.localRotation;
            visualMotoPosInicialLocal = visualMoto.localPosition;
            visualMotoInicialCapturada = true;
        }

        Vector3 eulerAnim = Vector3.zero;
        if (animarVisualMoto)
        {
            float errorCarril = Mathf.Clamp(carrilObjetivoInterpolado - carrilInterpolado, -1f, 1f);
            float inclinacionObjetivo = Mathf.Clamp(
                (errorCarril * 0.55f + impulsoInclinacionVisual) * inclinacionMaximaVisual,
                -inclinacionMaximaVisual,
                inclinacionMaximaVisual);
            float t = 1f - Mathf.Exp(-suavizadoInclinacionVisual * Time.deltaTime);
            inclinacionVisualActual = Mathf.Lerp(inclinacionVisualActual, inclinacionObjetivo, t);

            float factorVel = Mathf.Clamp01(velocidadActual / Mathf.Max(1f, velocidadMaxima));
            float actividadLateral = Mathf.Max(
                Mathf.Abs(errorCarril),
                Mathf.Max(Mathf.Abs(impulsoInclinacionVisual), Mathf.Abs(inclinacionVisualActual) / Mathf.Max(1f, inclinacionMaximaVisual)));

            float cabeceo = 0f;
            if (actividadLateral > 0.03f)
            {
                tiempoAnimVisual += Time.deltaTime * Mathf.Lerp(0.35f, 1.6f, factorVel);
                float factorActividad = Mathf.Clamp01(actividadLateral);
                cabeceo = Mathf.Sin(tiempoAnimVisual * frecuenciaCabeceoVisual)
                    * amplitudCabeceoVisual
                    * factorVel
                    * factorActividad;
            }
            else
            {
                tiempoAnimVisual = 0f;
            }

            eulerAnim = new Vector3(cabeceo, 0f, inclinacionVisualActual);
        }

        float flipYaw = invertirFrenteVisualMoto ? 180f : 0f;
        Vector3 eulerFinal = visualMotoOffsetEuler + eulerAnim + new Vector3(0f, flipYaw, 0f);

        // Aplicar en espacio local garantiza que la inclinacion sea visible sobre el modelo.
        visualMoto.localPosition = visualMotoPosInicialLocal + visualMotoOffsetPosLocal;
        visualMoto.localRotation = visualMotoRotInicialLocal * Quaternion.Euler(eulerFinal);
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
        if (!EsColisionConObstaculo(other))
            return;

        Vector3 punto = other != null ? other.ClosestPoint(transform.position) : transform.position;
        Vector3 normal = (transform.position - punto).sqrMagnitude > 0.0001f
            ? (transform.position - punto).normalized
            : transform.up;
        AplicarPenalizacionPorChoque(punto, normal);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!EsColisionConObstaculo(collision.collider))
            return;

        Vector3 punto = transform.position;
        Vector3 normal = transform.up;
        if (collision.contactCount > 0)
        {
            punto = collision.GetContact(0).point;
            normal = collision.GetContact(0).normal;
        }
        else if (collision.collider != null)
        {
            punto = collision.collider.ClosestPoint(transform.position);
            if ((transform.position - punto).sqrMagnitude > 0.0001f)
            {
                normal = (transform.position - punto).normalized;
            }
        }

        AplicarPenalizacionPorChoque(punto, normal);
    }

    bool EsColisionConObstaculo(Collider col)
    {
        if (col == null)
            return false;

        if (col.CompareTag("Obstacle"))
            return true;

        Transform t = col.transform;
        while (t != null)
        {
            if (t.CompareTag("Obstacle"))
                return true;

            t = t.parent;
        }

        return false;
    }

    void AplicarPenalizacionPorChoque(Vector3 puntoChoque, Vector3 normalChoque)
    {
        if (Time.time < tiempoProximoChoqueValido)
            return;

        choquesTotales++;
        if (oscurecerPantallaAlChocar)
        {
            duracionOscurecimientoChoque = Mathf.Max(0.1f, duracionOscurecimientoChoque);
            tiempoOscurecimientoRestante = duracionOscurecimientoChoque;
            oscurecimientoActual = Mathf.Clamp(intensidadOscurecimientoChoque + 0.25f, 0f, 2f);
        }

        if (audioSource != null && sonidoChoque != null)
        {
            audioSource.PlayOneShot(sonidoChoque, volumenChoque);
        }

        ReproducirParticulasChoque(puntoChoque, normalChoque);

        tiempoFinPenalizacion = Time.time + tiempoRecuperacionChoque;
        tiempoProximoChoqueValido = Time.time + tiempoInvulnerableTrasChoque;
        velocidadActual = Mathf.Min(velocidadActual, velocidadBase * multiplicadorVelocidadChoque);
    }

    void ReproducirParticulasChoque(Vector3 puntoChoque, Vector3 normalChoque)
    {
        if (!usarParticulasChoque)
            return;

        Vector3 normal = normalChoque.sqrMagnitude > 0.0001f ? normalChoque.normalized : transform.up;
        Vector3 posicionSpawn = puntoChoque + normal * 0.08f;
        Quaternion rotacionSpawn = Quaternion.LookRotation(normal, Vector3.up);

        if (prefabParticulasChoque != null)
        {
            ParticleSystem instancia = Instantiate(prefabParticulasChoque, posicionSpawn, rotacionSpawn);
            if (!instancia.gameObject.activeSelf)
            {
                instancia.gameObject.SetActive(true);
            }
            AplicarColorParticulasEnJerarquia(instancia, colorParticulasChoque);
            if (sobrescribirParametrosPrefabParticulas)
            {
                var mainPrefab = instancia.main;
                mainPrefab.startLifetime = Mathf.Max(0.05f, vidaParticulasChoque);
                mainPrefab.startSpeed = Mathf.Max(0.1f, velocidadParticulasChoque);
            }
            int emisionGarantizada = sobrescribirParametrosPrefabParticulas
                ? Mathf.Max(1, cantidadParticulasChoque)
                : Mathf.Max(1, Mathf.RoundToInt(cantidadParticulasChoque * 0.35f));
            ReproducirJerarquiaParticulas(instancia, emisionGarantizada);
            float vidaDestruccion = Mathf.Max(0.9f, CalcularDuracionParticulasEnJerarquia(instancia) + 0.4f);
            Destroy(instancia.gameObject, vidaDestruccion);
            return;
        }

        if (particulasChoqueRuntime == null)
        {
            GameObject go = new GameObject("ChoqueParticles_Runtime");
            go.transform.SetParent(transform, false);
            particulasChoqueRuntime = go.AddComponent<ParticleSystem>();
            var main = particulasChoqueRuntime.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = Mathf.Max(0.05f, vidaParticulasChoque);
            main.startSpeed = Mathf.Max(0.1f, velocidadParticulasChoque);
            main.startSize = 0.09f;
            main.maxParticles = 200;
            main.startColor = colorParticulasChoque;

            var emission = particulasChoqueRuntime.emission;
            emission.enabled = false;

            var shape = particulasChoqueRuntime.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;
        }

        var mainRuntime = particulasChoqueRuntime.main;
        mainRuntime.startLifetime = Mathf.Max(0.05f, vidaParticulasChoque);
        mainRuntime.startSpeed = Mathf.Max(0.1f, velocidadParticulasChoque);
        mainRuntime.startColor = colorParticulasChoque;

        particulasChoqueRuntime.transform.position = posicionSpawn;
        particulasChoqueRuntime.Emit(Mathf.Max(1, cantidadParticulasChoque));
    }

    void AplicarColorParticulasEnJerarquia(ParticleSystem raiz, Color color)
    {
        if (raiz == null)
            return;

        ParticleSystem[] sistemas = raiz.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < sistemas.Length; i++)
        {
            ParticleSystem ps = sistemas[i];
            if (ps == null)
                continue;

            var main = ps.main;
            main.startColor = color;

            var col = ps.colorOverLifetime;
            if (col.enabled)
            {
                ParticleSystem.MinMaxGradient grad = col.color;
                grad.mode = ParticleSystemGradientMode.Color;
                grad.color = color;
                col.color = grad;
            }
        }
    }

    void ReproducirJerarquiaParticulas(ParticleSystem raiz, int emisionGarantizada)
    {
        if (raiz == null)
            return;

        ParticleSystem[] sistemas = raiz.GetComponentsInChildren<ParticleSystem>(true);
        if (sistemas == null || sistemas.Length == 0)
            return;

        int porSistema = Mathf.Max(1, Mathf.CeilToInt((float)Mathf.Max(1, emisionGarantizada) / sistemas.Length));
        for (int i = 0; i < sistemas.Length; i++)
        {
            ParticleSystem ps = sistemas[i];
            if (ps == null)
                continue;

            if (!ps.gameObject.activeSelf)
            {
                ps.gameObject.SetActive(true);
            }

            ps.Clear(true);
            ps.Play(true);

            // Garantiza que haya feedback visual inmediato incluso con prefabs sin burst.
            if (porSistema > 0)
            {
                ps.Emit(porSistema);
            }
        }
    }

    float CalcularDuracionParticulasEnJerarquia(ParticleSystem raiz)
    {
        if (raiz == null)
            return 0.8f;

        float duracion = 0.8f;
        ParticleSystem[] sistemas = raiz.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < sistemas.Length; i++)
        {
            ParticleSystem ps = sistemas[i];
            if (ps == null)
                continue;

            var main = ps.main;
            float vida = main.startLifetime.constantMax;
            if (vida <= 0.01f)
            {
                vida = main.startLifetime.constant;
            }
            vida = Mathf.Max(0.1f, vida);
            duracion = Mathf.Max(duracion, main.duration + vida);
        }

        return duracion;
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

    void OnDestroy()
    {
        Time.timeScale = 1f;
        if (audioMusicaSource != null)
        {
            audioMusicaSource.Stop();
        }
    }
}