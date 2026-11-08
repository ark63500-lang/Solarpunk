using System.Collections;
using UnityEngine;

// portée radio de l'antenne : trop loin le drone tombe et réapparaît au départ
[RequireComponent(typeof(DroneCorps))]
public class DroneSignal : MonoBehaviour
{
    public enum Etat { Connecte, Chute, Reconnexion }

    [Header("Antenne")]
    public Transform antenne;
    public float rayonSur = 32f;
    public float rayonMax = 42f;

    [Header("Alerte")]
    public float amplitudeTremblement = 0.12f;

    [Header("Chute")]
    public float gravite = 12f;
    public float vitesseRotationChute = 300f;
    public float hauteurSol = 0.5f;
    public float dureeMaxChute = 3f;

    [Header("Réapparition")]
    public float dureeFondu = 0.6f;
    public float pauseNoir = 0.9f;

    public float Signal { get; private set; } = 1f;
    public Etat EtatActuel { get; private set; } = Etat.Connecte;
    public float Fondu { get; private set; }
    public bool EnAlerte => EtatActuel == Etat.Connecte && Signal < 1f;
    public bool AuSol { get; private set; }

    Vector3 positionDepart;
    Quaternion rotationDepart;
    DronePilotage pilotage;
    LanceurGraines lanceur;
    DroneAspirateur aspirateur;
    DroneArrosage arrosage;
    DroneCorps corps;
    Helice[] helices;
    float[] vitessesHelices;
    Vector3 dernierTremblement;

    void Awake()
    {
        pilotage = GetComponent<DronePilotage>();
        lanceur = GetComponent<LanceurGraines>();
        aspirateur = GetComponent<DroneAspirateur>();
        arrosage = GetComponent<DroneArrosage>();
        corps = GetComponent<DroneCorps>();
        helices = GetComponentsInChildren<Helice>();
        vitessesHelices = new float[helices.Length];
        for (int i = 0; i < helices.Length; i++) vitessesHelices[i] = helices[i].vitesse;
    }

    void Start()
    {
        positionDepart = transform.position;
        rotationDepart = transform.rotation;
    }

    void LateUpdate()
    {
        if (EtatActuel != Etat.Connecte || antenne == null) return;

        Vector3 ecart = corps.Centre - antenne.position;
        ecart.y = 0f;
        Signal = 1f - Mathf.InverseLerp(rayonSur, rayonMax, ecart.magnitude);

        // le drone tremble de plus en plus quand le signal baisse
        Vector3 tremblement = Vector3.zero;
        if (Signal < 1f)
        {
            float amplitude = amplitudeTremblement * (1f - Signal) * 2f;
            float temps = Time.time * 18f;
            tremblement = new Vector3(
                Mathf.PerlinNoise(temps, 0f) - 0.5f,
                (Mathf.PerlinNoise(0f, temps) - 0.5f) * 0.6f,
                Mathf.PerlinNoise(temps, temps) - 0.5f) * amplitude;
        }
        transform.position += tremblement - dernierTremblement;
        dernierTremblement = tremblement;

        if (Signal <= 0f) StartCoroutine(PerteSignal());
    }

    // chute du drone, écran noir puis retour au point de départ
    IEnumerator PerteSignal()
    {
        EtatActuel = Etat.Chute;
        AuSol = false;
        transform.position -= dernierTremblement;
        dernierTremblement = Vector3.zero;
        Activer(false);

        float vitesseChute = 0f;
        float tempsChute = 0f;
        Vector3 axe = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
        while (tempsChute < dureeMaxChute && corps.Bas > hauteurSol)
        {
            vitesseChute -= gravite * Time.deltaTime;
            transform.position += Vector3.up * vitesseChute * Time.deltaTime;
            transform.Rotate(axe, vitesseRotationChute * Time.deltaTime, Space.World);
            VitesseHelices(1f - tempsChute);
            tempsChute += Time.deltaTime;
            yield return null;
        }
        AuSol = true;
        VitesseHelices(0f);
        yield return new WaitForSeconds(0.5f);

        EtatActuel = Etat.Reconnexion;
        yield return TransitionFondu(0f, 1f);
        transform.SetPositionAndRotation(positionDepart, rotationDepart);
        if (pilotage != null) pilotage.Stopper();
        Signal = 1f;
        yield return new WaitForSeconds(pauseNoir);
        VitesseHelices(1f);
        Activer(true);
        EtatActuel = Etat.Connecte;
        yield return TransitionFondu(1f, 0f);
    }

    IEnumerator TransitionFondu(float de, float vers)
    {
        for (float tempsEcoule = 0f; tempsEcoule < dureeFondu; tempsEcoule += Time.deltaTime)
        {
            Fondu = Mathf.Lerp(de, vers, tempsEcoule / dureeFondu);
            yield return null;
        }
        Fondu = vers;
    }

    void Activer(bool actif)
    {
        if (pilotage != null) pilotage.enabled = actif;
        if (lanceur != null) lanceur.enabled = actif;
        if (aspirateur != null) aspirateur.enabled = actif;
        if (arrosage != null) arrosage.enabled = actif;
    }

    void VitesseHelices(float facteur)
    {
        for (int i = 0; i < helices.Length; i++)
            if (helices[i] != null) helices[i].vitesse = vitessesHelices[i] * Mathf.Clamp01(facteur);
    }

    void OnDrawGizmosSelected()
    {
        if (antenne == null) return;
        Gizmos.color = Color.green;
        DessinerCercle(antenne.position, rayonSur);
        Gizmos.color = Color.red;
        DessinerCercle(antenne.position, rayonMax);
    }

    static void DessinerCercle(Vector3 centre, float rayon)
    {
        Vector3 precedent = centre + new Vector3(rayon, 0.5f, 0f);
        for (int i = 1; i <= 64; i++)
        {
            float angle = i * Mathf.PI * 2f / 64f;
            Vector3 point = centre + new Vector3(Mathf.Cos(angle) * rayon, 0.5f, Mathf.Sin(angle) * rayon);
            Gizmos.DrawLine(precedent, point);
            precedent = point;
        }
    }
}
