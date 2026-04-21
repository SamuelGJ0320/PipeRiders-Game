using UnityEngine;

public class RuntimeFramePacing : MonoBehaviour
{
    private const string PrefKeyTargetFps = "PipeRiders.TargetFps";
    private static RuntimeFramePacing instancia;

    [Header("Frame Pacing")]
    [SerializeField] private bool usarVSync = false;
    [SerializeField, Range(0, 4)] private int vSyncCount = 1;
    [SerializeField] private int targetFpsSinVSync = 240;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCrear()
    {
        if (FindFirstObjectByType<RuntimeFramePacing>() != null)
        {
            return;
        }

        GameObject go = new GameObject("RuntimeFramePacing");
        DontDestroyOnLoad(go);
        go.AddComponent<RuntimeFramePacing>();
    }

    private void Awake()
    {
        instancia = this;
        CargarPreferencias();
        AplicarConfiguracion();
    }

    private void CargarPreferencias()
    {
        if (PlayerPrefs.HasKey(PrefKeyTargetFps))
        {
            targetFpsSinVSync = Mathf.Clamp(PlayerPrefs.GetInt(PrefKeyTargetFps, targetFpsSinVSync), 30, 360);
        }
    }

    private void AplicarConfiguracion()
    {
        if (usarVSync)
        {
            QualitySettings.vSyncCount = Mathf.Clamp(vSyncCount, 0, 4);
            Application.targetFrameRate = -1;
            return;
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Mathf.Max(30, targetFpsSinVSync);
    }

    public static int ObtenerFpsConfigurado()
    {
        RuntimeFramePacing runtime = ObtenerInstancia();
        if (runtime == null)
        {
            return Mathf.Max(30, Application.targetFrameRate);
        }

        return runtime.usarVSync ? -1 : runtime.targetFpsSinVSync;
    }

    public static void ConfigurarFps(int fps)
    {
        RuntimeFramePacing runtime = ObtenerInstancia();
        if (runtime == null)
        {
            return;
        }

        runtime.usarVSync = false;
        runtime.targetFpsSinVSync = Mathf.Clamp(fps, 30, 360);
        PlayerPrefs.SetInt(PrefKeyTargetFps, runtime.targetFpsSinVSync);
        PlayerPrefs.Save();
        runtime.AplicarConfiguracion();
    }

    private static RuntimeFramePacing ObtenerInstancia()
    {
        if (instancia != null)
        {
            return instancia;
        }

        instancia = FindFirstObjectByType<RuntimeFramePacing>();
        return instancia;
    }
}