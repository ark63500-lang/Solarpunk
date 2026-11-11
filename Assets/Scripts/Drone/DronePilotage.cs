// DronePilotage : déplace le drone selon les commandes, à hauteur constante.
// Les directions suivent l'orientation de la caméra et le modèle s'incline dans le sens du mouvement.

using UnityEngine;

[RequireComponent(typeof(DroneCommandes))]
public class DronePilotage : MonoBehaviour
{
    [Header("Déplacement")]
    public float vitesse = 8f;
    public float lissage = 6f;

    [Header("Inclinaison")]
    public Transform modele;
    public float inclinaisonMax = 15f;

    [Header("Caméra")]
    public Transform cameraRef;

    [Header("Limites du potager (X / Z monde)")]
    public bool limiterAuPotager = true;
    public Vector2 limiteMin = new Vector2(-18f, -20f);
    public Vector2 limiteMax = new Vector2(17f, 7f);

    Vector3 velocite;
    Vector3 axeAvant, axeDroite;
    DroneCommandes commandes;

    void Start()
    {
        commandes = GetComponent<DroneCommandes>();
        Transform reference = cameraRef != null ? cameraRef : transform;
        axeAvant = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
        axeDroite = Vector3.Cross(Vector3.up, axeAvant).normalized;
    }

    public void Stopper() => velocite = Vector3.zero;

    void Update()
    {
        Vector2 entree = commandes.Deplacement;
        Vector3 direction = Vector3.ClampMagnitude(axeAvant * entree.y + axeDroite * entree.x, 1f);

        velocite = Vector3.Lerp(velocite, direction * vitesse, lissage * Time.deltaTime);
        transform.position += velocite * Time.deltaTime;

        // le drone ne peut pas quitter le potager
        if (limiterAuPotager)
        {
            Vector3 position = transform.position;
            float positionX = Mathf.Clamp(position.x, limiteMin.x, limiteMax.x);
            float positionZ = Mathf.Clamp(position.z, limiteMin.y, limiteMax.y);
            if (positionX != position.x) velocite.x = 0f;
            if (positionZ != position.z) velocite.z = 0f;
            transform.position = new Vector3(positionX, position.y, positionZ);
        }

        // petite inclinaison du modèle dans le sens du mouvement
        if (modele != null)
        {
            Vector3 avantDrone = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 droiteDrone = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            float tangage = Vector3.Dot(direction, avantDrone);
            float roulis = Vector3.Dot(direction, droiteDrone);
            Quaternion inclinaison = Quaternion.Euler(tangage * inclinaisonMax, 0f, -roulis * inclinaisonMax);
            modele.localRotation = Quaternion.Slerp(modele.localRotation, inclinaison, 5f * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!limiterAuPotager) return;
        Gizmos.color = Color.cyan;
        Vector3 centre = new Vector3((limiteMin.x + limiteMax.x) * 0.5f, transform.position.y, (limiteMin.y + limiteMax.y) * 0.5f);
        Gizmos.DrawWireCube(centre, new Vector3(limiteMax.x - limiteMin.x, 0.1f, limiteMax.y - limiteMin.y));
    }
}
