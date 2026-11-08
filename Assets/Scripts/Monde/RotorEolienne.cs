using UnityEngine;

// rotation des pales des éoliennes
public class RotorEolienne : MonoBehaviour
{
    public float vitesse = 90f;
    public float variation = 15f;
    public bool sensInverse = false;
    public Vector3 axe = Vector3.up;

    private float vitesseReelle;

    void Start()
    {
        vitesseReelle = vitesse + Random.Range(-variation, variation);
        transform.Rotate(axe, Random.Range(0f, 360f), Space.Self);
    }

    void Update()
    {
        transform.Rotate(axe, vitesseReelle * (sensInverse ? -1f : 1f) * Time.deltaTime, Space.Self);
    }
}
