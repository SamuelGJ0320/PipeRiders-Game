using UnityEngine;

public class RuntimeFramePacing : MonoBehaviour
{
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
        AplicarConfiguracion();
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
}