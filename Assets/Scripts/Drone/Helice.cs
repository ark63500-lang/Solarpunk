// Helice : fait tourner une hélice du drone autour de son centre.

using UnityEngine;
using UnityEngine.Serialization;

public class Helice : MonoBehaviour
{
    public float vitesse = 1500f;
    public bool sensInverse = false;
    public Vector3 axe = Vector3.up;
    [FormerlySerializedAs("centrerSurLeMesh")] public bool centrerSurLeMaillage = true;

    private Vector3 centreLocal;

    void Start()
    {
        centreLocal = Vector3.zero;
        if (centrerSurLeMaillage)
        {
            Renderer rendu = GetComponent<Renderer>();
            if (rendu != null) centreLocal = transform.InverseTransformPoint(rendu.bounds.center);
        }
    }

    void Update()
    {
        float angle = vitesse * (sensInverse ? -1f : 1f) * Time.deltaTime;
        Vector3 pivot = transform.TransformPoint(centreLocal);
        Vector3 axeMonde = transform.TransformDirection(axe);
        transform.RotateAround(pivot, axeMonde, angle);
    }
}
