using UnityEngine;
using System.Collections.Generic;

public class TunnelGenerator : MonoBehaviour
{
    [Header("Configuracion del Tunel")]
    public GameObject tunnelPrefab;
    [Tooltip("Longitud de cada segmento de tunel. Si tu prefab tiene Y=10, pon 10.")]
    public float longitudSegmento = 10f;
    [Tooltip("El pivote del prefab esta en el centro")]
    public bool pivoteEnCentro = true;
    [Tooltip("Mayor densidad = curvas mas suaves")]
    [Range(1, 10)]
    public int densidadCurva = 3;
    [Tooltip("Ignora el roll de los spawn points")]
    public bool ignorarRoll = true;

    [Header("Spawn Points")]
    public List<TunnelSpawnPoint> spawnPoints = new List<TunnelSpawnPoint>();
    public bool autoEncontrarSpawnPoints = true;
    public bool mostrarLineaDeRuta = true;
    [Range(4, 120)]
    public int cantidadSpawnPointsAleatorios = 30;
    public Vector3 inicioTunel = Vector3.zero;
    public Vector3 referenciaFinTunel = new Vector3(2125.382f, -1754.525f, 3860.203f);
    public float variacionYawMaxSP = 22f;
    public float variacionPitchMaxSP = 14f;

    [Header("Optimizacion de Rendimiento")]
    [Tooltip("Radio donde los colliders del tunel estan activos")]
    public float radioActivacionColliders = 100f;
    public bool optimizarColliders = true;

    [Header("Info (Solo lectura)")]
    public int totalSegmentosGenerados = 0;
    public int collidersActivos = 0;

    [Header("Obstaculos Procedurales")]
    public GameObject obstacleBarPrefab;
    public GameObject obstacleBarrier40Prefab;
    public GameObject obstacleBarrier60Prefab;
    public GameObject obstacleCubePrefab;
    [Range(1, 5)]
    public int nivelActual = 1;
    public bool generarObstaculosAlIniciar = true;
    [Tooltip("Semilla para generacion reproducible. Con el mismo valor, se obtiene el mismo resultado.")]
    public int semilla = 0;

    [HideInInspector]
    public List<GameObject> obstaculosGenerados = new List<GameObject>();

    private List<GameObject> segmentosGenerados = new List<GameObject>();
    private GameObject jugador;
    [HideInInspector]
    public List<Vector3> puntosAltaResolucion = new List<Vector3>();
    [HideInInspector]
    public List<Vector3> tangentesAltaResolucion = new List<Vector3>();
    [HideInInspector]
    public List<float> rollsAltaResolucion = new List<float>();

    void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (generarObstaculosAlIniciar)
        {
            GenerarNivelProcedural();
        }
        else
        {
            GenerarTunel();
        }
    }

    void Update()
    {
        if (!Application.isPlaying || !optimizarColliders)
        {
            return;
        }

        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador == null)
            {
                return;
            }
        }

        Vector3 posJugador = jugador.transform.position;
        int collidersActivosTemp = 0;

        foreach (GameObject segmento in segmentosGenerados)
        {
            if (segmento == null)
            {
                continue;
            }

            float distancia = Vector3.Distance(segmento.transform.position, posJugador);
            MeshCollider col = segmento.GetComponent<MeshCollider>();
            if (col == null)
            {
                continue;
            }

            bool debeEstarActivo = distancia < radioActivacionColliders;
            col.enabled = debeEstarActivo;
            if (debeEstarActivo)
            {
                collidersActivosTemp++;
            }
        }

        collidersActivos = collidersActivosTemp;
    }

    [ContextMenu("Generar Nivel Procedural")]
    public void GenerarNivelProcedural()
    {
        GenerarTunel();
        LimpiarObstaculos();

        LevelObstacleConfig config = ObtenerConfigNivel(nivelActual);
        if (config.total <= 0)
        {
            return;
        }

        int seed = semilla + nivelActual * 97;
        System.Random rand = new System.Random(seed);
        HashSet<int> usados = new HashSet<int>();

        ColocarBarreras(obstacleBarrier40Prefab, config.barrier40, 0.40f, rand, usados, config.minSeparacion);
        ColocarBarreras(obstacleBarrier60Prefab, config.barrier60, 0.60f, rand, usados, config.minSeparacion);
        ColocarBarras(config.bars, rand, usados, config.minSeparacion);
        ColocarCubos(config.cubes, rand, usados, Mathf.Max(1, config.minSeparacion / 2));
    }

    public void LimpiarObstaculos()
    {
        for (int i = 0; i < obstaculosGenerados.Count; i++)
        {
            GameObject obj = obstaculosGenerados[i];
            if (obj == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        obstaculosGenerados.Clear();
    }

    public void GenerarTunel()
    {
        LimpiarTunel();

        if (autoEncontrarSpawnPoints)
        {
            EncontrarSpawnPoints();
        }

        if (spawnPoints.Count == 0)
        {
            GenerarTunelRecto();
            return;
        }

        GenerarTunelConCurvas();
    }

    public void LimpiarTunel()
    {
        LimpiarObstaculos();

        foreach (GameObject segmento in segmentosGenerados)
        {
            if (segmento == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(segmento);
            }
            else
            {
                DestroyImmediate(segmento);
            }
        }

        segmentosGenerados.Clear();
        totalSegmentosGenerados = 0;

        puntosAltaResolucion.Clear();
        tangentesAltaResolucion.Clear();
        rollsAltaResolucion.Clear();
    }

    void EncontrarSpawnPoints()
    {
        spawnPoints.Clear();
        TunnelSpawnPoint[] puntosEncontrados = Object.FindObjectsByType<TunnelSpawnPoint>(FindObjectsSortMode.None);
        System.Array.Sort(puntosEncontrados, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));
        spawnPoints.AddRange(puntosEncontrados);
    }

    public void GenerarSpawnPointsAleatoriosSolo()
    {
        EliminarSpawnPointsSolo();
        GenerarSpawnPointsAleatoriosInterno();
    }

    public void EliminarSpawnPointsSolo()
    {
        TunnelSpawnPoint[] existentes = Object.FindObjectsByType<TunnelSpawnPoint>(FindObjectsSortMode.None);
        for (int i = 0; i < existentes.Length; i++)
        {
            if (existentes[i] == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(existentes[i].gameObject);
            }
            else
            {
                DestroyImmediate(existentes[i].gameObject);
            }
        }

        spawnPoints.Clear();
    }

    void GenerarSpawnPointsAleatoriosInterno()
    {
        int cantidad = Mathf.Max(4, cantidadSpawnPointsAleatorios);
        float longitudObjetivo = Vector3.Distance(inicioTunel, referenciaFinTunel);
        if (longitudObjetivo < 1f)
        {
            longitudObjetivo = (cantidad - 1) * Mathf.Max(30f, longitudSegmento * 2f);
        }

        int seed = semilla + 7919;
        System.Random rand = new System.Random(seed);

        List<Vector3> puntos = new List<Vector3>(cantidad);
        Vector3 pos = inicioTunel;
        Vector3 dirBase = referenciaFinTunel - inicioTunel;
        Vector3 forward = dirBase.sqrMagnitude > 0.0001f ? dirBase.normalized : Vector3.forward;
        puntos.Add(pos);

        float pasoBase = longitudObjetivo / Mathf.Max(1, cantidad - 1);
        for (int i = 1; i < cantidad; i++)
        {
            float yaw = Mathf.Lerp(-variacionYawMaxSP, variacionYawMaxSP, (float)rand.NextDouble());
            float pitch = Mathf.Lerp(-variacionPitchMaxSP, variacionPitchMaxSP, (float)rand.NextDouble());
            Vector3 candidato = (Quaternion.Euler(pitch, yaw, 0f) * forward).normalized;
            forward = Vector3.Slerp(forward, candidato, 0.6f).normalized;

            float paso = pasoBase * Mathf.Lerp(0.8f, 1.2f, (float)rand.NextDouble());
            pos += forward * paso;
            puntos.Add(pos);
        }

        float largoActual = CalcularLongitudCurva(puntos);
        if (largoActual > 0.001f)
        {
            float factor = longitudObjetivo / largoActual;
            for (int i = 0; i < puntos.Count; i++)
            {
                Vector3 local = puntos[i] - inicioTunel;
                puntos[i] = inicioTunel + local * factor;
            }
        }

        for (int i = 0; i < puntos.Count; i++)
        {
            GameObject go = new GameObject($"SP_Auto_{i + 1:00}");
            go.transform.SetParent(transform);
            go.transform.position = puntos[i];

            Vector3 dir;
            if (i < puntos.Count - 1)
            {
                dir = (puntos[i + 1] - puntos[i]).normalized;
            }
            else
            {
                dir = (puntos[i] - puntos[i - 1]).normalized;
            }

            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = Vector3.forward;
            }

            go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            TunnelSpawnPoint sp = go.AddComponent<TunnelSpawnPoint>();
            sp.ordenEnRuta = i;
            spawnPoints.Add(sp);
        }
    }

    void GenerarTunelRecto()
    {
        Vector3 posicion = transform.position;
        Quaternion rotacion = Quaternion.Euler(90f, 0f, 0f);

        for (int i = 0; i < 10; i++)
        {
            GameObject segmento = Instantiate(tunnelPrefab, posicion, rotacion);
            segmento.transform.SetParent(transform);
            segmentosGenerados.Add(segmento);

            posicion += Vector3.forward * longitudSegmento;
            totalSegmentosGenerados++;
        }
    }

    void GenerarTunelConCurvas()
    {
        if (spawnPoints.Count < 2)
        {
            Debug.LogWarning("Se necesitan al menos 2 spawn points para generar curvas");
            GenerarTunelRecto();
            return;
        }

        for (int i = 0; i < spawnPoints.Count - 1; i++)
        {
            Vector3 puntoInicio = spawnPoints[i].transform.position;
            Vector3 puntoFin = spawnPoints[i + 1].transform.position;

            Vector3 tangenteInicio;
            Vector3 tangenteFin;

            if (i > 0)
            {
                Vector3 prevPos = spawnPoints[i - 1].transform.position;
                tangenteInicio = (puntoFin - prevPos).normalized;
            }
            else
            {
                tangenteInicio = (puntoFin - puntoInicio).normalized;
            }

            if (i < spawnPoints.Count - 2)
            {
                Vector3 nextPos = spawnPoints[i + 2].transform.position;
                tangenteFin = (nextPos - puntoInicio).normalized;
            }
            else
            {
                tangenteFin = (puntoFin - puntoInicio).normalized;
            }

            float rollInicio = spawnPoints[i].transform.eulerAngles.z;
            float rollFin = spawnPoints[i + 1].transform.eulerAngles.z;

            float distancia = Vector3.Distance(puntoInicio, puntoFin);
            float factorControl = distancia * 0.33f;

            Vector3 p0 = puntoInicio;
            Vector3 p1 = puntoInicio + tangenteInicio * factorControl;
            Vector3 p2 = puntoFin - tangenteFin * factorControl;
            Vector3 p3 = puntoFin;

            int pasosPorSegmento = Mathf.Max(10, Mathf.RoundToInt(distancia / Mathf.Max(0.1f, longitudSegmento) * densidadCurva));

            for (int j = 0; j < pasosPorSegmento; j++)
            {
                float t = (float)j / pasosPorSegmento;
                Vector3 punto = CalcularPuntoBezier(p0, p1, p2, p3, t);
                Vector3 tangente = CalcularTangenteBezier(p0, p1, p2, p3, t);
                float roll = Mathf.Lerp(rollInicio, rollFin, t);

                puntosAltaResolucion.Add(punto);
                tangentesAltaResolucion.Add(tangente.normalized);
                rollsAltaResolucion.Add(roll);
            }
        }

        puntosAltaResolucion.Add(spawnPoints[spawnPoints.Count - 1].transform.position);

        if (puntosAltaResolucion.Count >= 2)
        {
            Vector3 ultimaTangente = (puntosAltaResolucion[puntosAltaResolucion.Count - 1] - puntosAltaResolucion[puntosAltaResolucion.Count - 2]).normalized;
            tangentesAltaResolucion.Add(ultimaTangente);
        }
        else
        {
            tangentesAltaResolucion.Add(Vector3.forward);
        }

        rollsAltaResolucion.Add(spawnPoints[spawnPoints.Count - 1].transform.eulerAngles.z);

        float longitudTotalCurva = CalcularLongitudCurva(puntosAltaResolucion);
        int numSegmentosNecesarios = Mathf.Max(1, Mathf.RoundToInt(longitudTotalCurva / Mathf.Max(0.1f, longitudSegmento)));
        float offset = pivoteEnCentro ? (longitudSegmento * 0.5f) : 0f;

        for (int segIndex = 0; segIndex < numSegmentosNecesarios; segIndex++)
        {
            float distanciaObjetivo = (segIndex * longitudSegmento) + offset;

            float distanciaAcumulada = 0f;
            Vector3 posicionSegmento = puntosAltaResolucion[0];
            Vector3 direccionSegmento = tangentesAltaResolucion[0];
            float rollSegmento = rollsAltaResolucion[0];
            bool encontrado = false;

            for (int i = 0; i < puntosAltaResolucion.Count - 1; i++)
            {
                float distanciaTramo = Vector3.Distance(puntosAltaResolucion[i], puntosAltaResolucion[i + 1]);
                if (distanciaAcumulada + distanciaTramo >= distanciaObjetivo)
                {
                    float distanciaEnTramo = distanciaObjetivo - distanciaAcumulada;
                    float t = distanciaTramo > 0.001f ? (distanciaEnTramo / distanciaTramo) : 0f;

                    posicionSegmento = Vector3.Lerp(puntosAltaResolucion[i], puntosAltaResolucion[i + 1], t);
                    rollSegmento = Mathf.Lerp(rollsAltaResolucion[i], rollsAltaResolucion[i + 1], t);
                    direccionSegmento = Vector3.Slerp(tangentesAltaResolucion[i], tangentesAltaResolucion[i + 1], t).normalized;
                    encontrado = true;
                    break;
                }

                distanciaAcumulada += distanciaTramo;
            }

            if (!encontrado || distanciaObjetivo >= longitudTotalCurva)
            {
                posicionSegmento = puntosAltaResolucion[puntosAltaResolucion.Count - 1];
                rollSegmento = rollsAltaResolucion[rollsAltaResolucion.Count - 1];
                direccionSegmento = tangentesAltaResolucion[tangentesAltaResolucion.Count - 1];
            }

            if (direccionSegmento.sqrMagnitude < 0.0001f)
            {
                direccionSegmento = Vector3.forward;
            }

            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(direccionSegmento, Vector3.up)) > 0.95f)
            {
                up = Vector3.forward;
            }

            Quaternion rotacionBase = Quaternion.LookRotation(direccionSegmento, up);
            float rollFinal = ignorarRoll ? 0f : rollSegmento;
            Quaternion rotacionFinal = rotacionBase * Quaternion.Euler(90f, 0f, rollFinal);

            GameObject segmento = Instantiate(tunnelPrefab, posicionSegmento, rotacionFinal);
            segmento.transform.SetParent(transform);
            segmentosGenerados.Add(segmento);
            totalSegmentosGenerados++;
        }

        Debug.Log($"Tunel generado con {totalSegmentosGenerados} segmentos.");
    }

    void OnDrawGizmos()
    {
        if (!mostrarLineaDeRuta)
        {
            return;
        }

        if (autoEncontrarSpawnPoints && Application.isEditor && !Application.isPlaying)
        {
            TunnelSpawnPoint[] puntos = Object.FindObjectsByType<TunnelSpawnPoint>(FindObjectsSortMode.None);
            if (puntos.Length <= 1)
            {
                return;
            }

            System.Array.Sort(puntos, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

            Gizmos.color = Color.magenta;
            for (int i = 0; i < puntos.Length - 1; i++)
            {
                Gizmos.DrawLine(puntos[i].transform.position, puntos[i + 1].transform.position);

                #if UNITY_EDITOR
                UnityEditor.Handles.Label(puntos[i].transform.position + Vector3.up * 3f, "Punto " + (i + 1));
                #endif
            }

            #if UNITY_EDITOR
            UnityEditor.Handles.Label(puntos[puntos.Length - 1].transform.position + Vector3.up * 3f, "Punto " + puntos.Length);
            #endif
        }
    }

    Vector3 CalcularPuntoBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        Vector3 punto = uuu * p0;
        punto += 3f * uu * t * p1;
        punto += 3f * u * tt * p2;
        punto += ttt * p3;
        return punto;
    }

    Vector3 CalcularTangenteBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float uu = u * u;
        float tt = t * t;

        Vector3 tangente = -3f * uu * p0;
        tangente += 3f * uu * p1 - 6f * u * t * p1;
        tangente += 6f * u * t * p2 - 3f * tt * p2;
        tangente += 3f * tt * p3;
        return tangente;
    }

    float CalcularLongitudCurva(List<Vector3> puntos)
    {
        if (puntos == null || puntos.Count < 2)
        {
            return 0f;
        }

        float longitud = 0f;
        for (int i = 0; i < puntos.Count - 1; i++)
        {
            longitud += Vector3.Distance(puntos[i], puntos[i + 1]);
        }

        return longitud;
    }

    public List<Vector3> GetPuntosCurva()
    {
        return puntosAltaResolucion;
    }

    public List<Vector3> GetTangentesCurva()
    {
        return tangentesAltaResolucion;
    }

    float ObtenerRadioTunelAproximado()
    {
        if (tunnelPrefab == null)
        {
            return 5f;
        }

        return Mathf.Max(0.5f, tunnelPrefab.transform.localScale.x * 0.5f);
    }

    int ObtenerIndiceDisponible(System.Random rand, HashSet<int> usados, int minSeparacion)
    {
        int total = puntosAltaResolucion.Count;
        if (total < 10)
        {
            return -1;
        }

        for (int intento = 0; intento < 64; intento++)
        {
            int idx = rand.Next(5, total - 5);
            bool libre = true;
            foreach (int usado in usados)
            {
                if (Mathf.Abs(usado - idx) < minSeparacion)
                {
                    libre = false;
                    break;
                }
            }

            if (libre)
            {
                usados.Add(idx);
                return idx;
            }
        }

        return -1;
    }

    void ObtenerFrameTunel(int idx, out Vector3 pos, out Vector3 forward, out Vector3 up, out Vector3 right)
    {
        pos = puntosAltaResolucion[idx];
        forward = tangentesAltaResolucion[idx].normalized;

        right = Vector3.Cross(forward, Vector3.up).normalized;
        if (right.sqrMagnitude < 0.01f)
        {
            right = Vector3.Cross(forward, Vector3.right).normalized;
        }

        up = Vector3.Cross(right, forward).normalized;
    }

    void ConfigurarTagYCollider(GameObject obj, bool forzarTrigger)
    {
        obj.tag = "Obstacle";

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        if (colliders == null || colliders.Length == 0)
        {
            // Fallback si el prefab no trae collider funcional.
            Collider fallback = CrearColliderFallback(obj);
            if (fallback != null)
            {
                fallback.isTrigger = forzarTrigger;
                colliders = new Collider[] { fallback };
            }
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
            {
                continue;
            }

            CorregirMeshColliderSiHaceFalta(colliders[i]);

            colliders[i].isTrigger = forzarTrigger;
        }
    }

    void CorregirMeshColliderSiHaceFalta(Collider col)
    {
        MeshCollider meshCol = col as MeshCollider;
        if (meshCol == null || meshCol.sharedMesh != null)
        {
            return;
        }

        MeshFilter mf = meshCol.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            meshCol.sharedMesh = mf.sharedMesh;
            return;
        }

        MeshFilter mfEnHijo = meshCol.GetComponentInChildren<MeshFilter>();
        if (mfEnHijo != null && mfEnHijo.sharedMesh != null)
        {
            meshCol.sharedMesh = mfEnHijo.sharedMesh;
        }
    }

    Collider CrearColliderFallback(GameObject obj)
    {
        MeshFilter mf = obj.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            MeshCollider mc = obj.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
            return mc;
        }

        // Ultimo recurso: al menos un box collider para no dejar obstaculo sin colision.
        return obj.AddComponent<BoxCollider>();
    }

    void ColocarBarras(int cantidad, System.Random rand, HashSet<int> usados, int minSeparacion)
    {
        if (obstacleBarPrefab == null)
        {
            return;
        }

        for (int i = 0; i < cantidad; i++)
        {
            int idx = ObtenerIndiceDisponible(rand, usados, minSeparacion);
            if (idx < 0)
            {
                break;
            }

            ObtenerFrameTunel(idx, out Vector3 pos, out Vector3 forward, out Vector3 up, out _);
            Quaternion baseRot = Quaternion.LookRotation(forward, up);
            int[] angulos = new int[] { 0, 45, 90, 135 };
            Quaternion rot = baseRot * Quaternion.Euler(0f, 0f, angulos[rand.Next(0, angulos.Length)]);

            GameObject obj = Instantiate(obstacleBarPrefab, pos, rot, transform);
            ConfigurarTagYCollider(obj, true);
            obstaculosGenerados.Add(obj);
        }
    }

    void ColocarBarreras(GameObject prefab, int cantidad, float cobertura, System.Random rand, HashSet<int> usados, int minSeparacion)
    {
        if (prefab == null)
        {
            return;
        }

        float radio = ObtenerRadioTunelAproximado();
        float diametro = radio * 2f;
        float offset = diametro * (1f - cobertura);

        for (int i = 0; i < cantidad; i++)
        {
            int idx = ObtenerIndiceDisponible(rand, usados, minSeparacion);
            if (idx < 0)
            {
                break;
            }

            ObtenerFrameTunel(idx, out Vector3 pos, out Vector3 forward, out Vector3 up, out Vector3 right);
            Quaternion baseRot = Quaternion.LookRotation(forward, up);

            int lado = rand.Next(0, 4);
            Vector3 side = lado == 0 ? up : lado == 1 ? -up : lado == 2 ? right : -right;
            Vector3 finalPos = pos + side * offset;
            Quaternion rot = baseRot * Quaternion.Euler(0f, 0f, lado * 90f);

            GameObject obj = Instantiate(prefab, finalPos, rot, transform);
            ConfigurarTagYCollider(obj, true);
            obstaculosGenerados.Add(obj);
        }
    }

    void ColocarCubos(int cantidad, System.Random rand, HashSet<int> usados, int minSeparacion)
    {
        if (obstacleCubePrefab == null)
        {
            return;
        }

        float radio = ObtenerRadioTunelAproximado();
        float half = 1f;
        float distancia = Mathf.Max(0.5f, radio - half - 0.05f);

        for (int i = 0; i < cantidad; i++)
        {
            int idx = ObtenerIndiceDisponible(rand, usados, minSeparacion);
            if (idx < 0)
            {
                break;
            }

            ObtenerFrameTunel(idx, out Vector3 pos, out Vector3 forward, out Vector3 up, out Vector3 right);
            Quaternion baseRot = Quaternion.LookRotation(forward, up);

            int paso = rand.Next(0, 8);
            float angulo = paso * 45f;
            Vector3 radial = Quaternion.AngleAxis(angulo, forward) * right;
            Vector3 finalPos = pos + radial * distancia;

            GameObject obj = Instantiate(obstacleCubePrefab, finalPos, baseRot, transform);
            ConfigurarTagYCollider(obj, true);
            obstaculosGenerados.Add(obj);
        }
    }

    LevelObstacleConfig ObtenerConfigNivel(int nivel)
    {
        switch (Mathf.Clamp(nivel, 1, 5))
        {
            case 1:
                return new LevelObstacleConfig(4, 2, 8, 16, 6);
            case 2:
                return new LevelObstacleConfig(5, 3, 10, 20, 5);
            case 3:
                return new LevelObstacleConfig(6, 4, 12, 24, 5);
            case 4:
                return new LevelObstacleConfig(7, 5, 14, 28, 4);
            default:
                return new LevelObstacleConfig(8, 6, 16, 32, 4);
        }
    }

    struct LevelObstacleConfig
    {
        public int barrier40;
        public int barrier60;
        public int bars;
        public int cubes;
        public int minSeparacion;

        public int total => barrier40 + barrier60 + bars + cubes;

        public LevelObstacleConfig(int barrier40, int barrier60, int bars, int cubes, int minSeparacion)
        {
            this.barrier40 = barrier40;
            this.barrier60 = barrier60;
            this.bars = bars;
            this.cubes = cubes;
            this.minSeparacion = minSeparacion;
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(TunnelGenerator))]
public class TunnelGeneratorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TunnelGenerator gen = (TunnelGenerator)target;
        GUILayout.Space(10);
        GUILayout.Label("Generacion Procedural de Niveles", UnityEditor.EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Nivel 1")) { gen.nivelActual = 1; gen.GenerarNivelProcedural(); }
        if (GUILayout.Button("Nivel 2")) { gen.nivelActual = 2; gen.GenerarNivelProcedural(); }
        if (GUILayout.Button("Nivel 3")) { gen.nivelActual = 3; gen.GenerarNivelProcedural(); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Nivel 4")) { gen.nivelActual = 4; gen.GenerarNivelProcedural(); }
        if (GUILayout.Button("Nivel 5")) { gen.nivelActual = 5; gen.GenerarNivelProcedural(); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generar Tunel")) { gen.GenerarTunel(); }
        if (GUILayout.Button("Limpiar Tunel")) { gen.LimpiarTunel(); }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generar SP Aleatorios")) { gen.GenerarSpawnPointsAleatoriosSolo(); }
        if (GUILayout.Button("Eliminar SP")) { gen.EliminarSpawnPointsSolo(); }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Limpiar Obstaculos"))
        {
            gen.LimpiarObstaculos();
        }

        if (GUI.changed)
        {
            UnityEditor.EditorUtility.SetDirty(gen);
        }
    }
}
#endif
