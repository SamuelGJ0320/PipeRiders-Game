using UnityEngine;

public class TunnelSpawnPoint : MonoBehaviour
{
    public Color gizmoColor = Color.yellow;
    public float gizmoRadius = 2f;
    public int ordenEnRuta = 0; // Para saber el orden en la ruta

    // Dibuja el punto en la escena para que lo puedas ver
    void OnDrawGizmos()
    {
        // Esfera principal más grande
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawSphere(transform.position, gizmoRadius * 0.3f);
        
        // Flecha grande para mostrar la dirección
        Gizmos.color = Color.cyan;
        Vector3 direccion = transform.forward * 5f;
        Gizmos.DrawLine(transform.position, transform.position + direccion);
        Gizmos.DrawLine(transform.position + direccion, transform.position + direccion * 0.8f + transform.right * 0.5f);
        Gizmos.DrawLine(transform.position + direccion, transform.position + direccion * 0.8f - transform.right * 0.5f);
        
        // Mostrar ejes para facilitar la rotación
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 2f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Cuando está seleccionado, mostrar un círculo más grande
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius * 1.5f);
    }
}
