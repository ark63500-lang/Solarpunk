using UnityEngine;

// fait passer le drone de temps en temps dans le décor du menu
public class DronePassageMenu : MonoBehaviour
{
    public Transform drone;
    public Vector2 attenteEntrePassages = new Vector2(4f, 9f);
    public float premierPassage = 1.5f;
    public float vitesse = 9f;
    public float distanceX = 40f;
    public Vector2 profondeur = new Vector2(14f, 24f);
    public Vector2 hauteur = new Vector2(4f, 8f);
    public float ondulation = 0.5f;
    public float inclinaison = 12f;
    [Tooltip("Rotation à ajouter si le modèle ne regarde pas vers +Z")]
    public float angleModele = 0f;

    float attente, tempsVol, duree;
    bool enVol;
    Vector3 depart, arrivee;

    void Start()
    {
        if (drone != null) drone.gameObject.SetActive(false);
        attente = premierPassage;
    }

    void Update()
    {
        if (drone == null) return;

        if (!enVol)
        {
            attente -= Time.deltaTime;
            if (attente <= 0f) Lancer();
            return;
        }

        tempsVol += Time.deltaTime;
        float progression = Mathf.Clamp01(tempsVol / duree);
        Vector3 position = Vector3.Lerp(depart, arrivee, progression);
        position.y += Mathf.Sin(tempsVol * 2.2f) * ondulation;
        drone.position = position;

        Vector3 direction = arrivee - depart;
        direction.y = 0f;
        drone.rotation = Quaternion.LookRotation(direction.normalized)
            * Quaternion.Euler(inclinaison, 0f, Mathf.Sin(tempsVol * 1.3f) * 3f)
            * Quaternion.Euler(0f, angleModele, 0f);

        if (progression >= 1f)
        {
            enVol = false;
            drone.gameObject.SetActive(false);
            attente = Random.Range(attenteEntrePassages.x, attenteEntrePassages.y);
        }
    }

    void Lancer()
    {
        float sens = Random.value < 0.5f ? -1f : 1f;
        float profondeurPassage = Random.Range(profondeur.x, profondeur.y);
        float hauteurPassage = Random.Range(hauteur.x, hauteur.y);
        depart = transform.position + new Vector3(sens * distanceX, hauteurPassage, profondeurPassage);
        arrivee = transform.position + new Vector3(-sens * distanceX, hauteurPassage + Random.Range(-1.5f, 1.5f), profondeurPassage + Random.Range(-4f, 4f));
        duree = Vector3.Distance(depart, arrivee) / vitesse;
        tempsVol = 0f;
        enVol = true;
        drone.position = depart;
        drone.gameObject.SetActive(true);
    }
}
