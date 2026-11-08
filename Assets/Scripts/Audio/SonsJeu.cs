using UnityEngine;
using UnityEngine.Serialization;

// tous les sons du jeu au même endroit, déclenchés par les événements de la Partie et du drone
public class SonsJeu : MonoBehaviour
{
    [Header("Références")]
    public Transform drone;
    public Partie partie;

    [Header("Volumes")]
    [Range(0, 1)] public float volumeGeneral = 0.8f;
    [Range(0, 1)] public float volumeMoteur = 0.35f;
    [Range(0, 1)] public float volumeTornade = 0.6f;
    [Range(0, 1)] public float volumeArrosage = 0.5f;
    [Range(0, 1)] public float volumeParasites = 0.5f;
    [Range(0, 1)] public float volumeEffets = 0.8f;
    [Range(0, 1)] public float volumeMusique = 0.4f;

    [Header("Boucles")]
    public AudioClip moteur;
    public AudioClip tornadeVent;
    public AudioClip arrosage;
    public AudioClip parasites;
    public AudioClip musique;

    [Header("Effets")]
    public AudioClip graineLacher;
    public AudioClip recolte;
    [FormerlySerializedAs("uiSelection")] public AudioClip interfaceSelection;
    public AudioClip alerteBip;
    public AudioClip signalPerdu;
    public AudioClip chuteImpact;
    public AudioClip reconnexion;
    public AudioClip explosion;

    AudioSource sourceMoteur, sourceTornade, sourceArrosage, sourceParasites, sourceMusique;
    AudioSource[] sourcesEffets;
    int prochaineSource;
    DroneAspirateur aspirateur;
    DroneArrosage arroseur;
    LanceurGraines lanceur;
    DroneSignal signal;
    Vector3 dernierePosition;
    float vitesseLissee, prochainBip;
    int derniereSelection = -1;
    DroneSignal.Etat dernierEtat = DroneSignal.Etat.Connecte;
    bool impactJoue;

    void Awake()
    {
        sourceMoteur = Boucle(moteur);
        sourceTornade = Boucle(tornadeVent);
        sourceArrosage = Boucle(arrosage);
        sourceParasites = Boucle(parasites);
        sourceMusique = Boucle(musique);
        sourceMusique.volume = volumeMusique * volumeGeneral;
        sourcesEffets = new AudioSource[6];
        for (int i = 0; i < sourcesEffets.Length; i++)
        {
            sourcesEffets[i] = gameObject.AddComponent<AudioSource>();
            sourcesEffets[i].playOnAwake = false;
        }
    }

    void Start()
    {
        if (partie != null)
        {
            partie.QuandRecolte += SurRecolte;
            partie.QuandExplosion += SurExplosion;
        }
        if (drone == null) return;

        aspirateur = drone.GetComponent<DroneAspirateur>();
        arroseur = drone.GetComponent<DroneArrosage>();
        lanceur = drone.GetComponent<LanceurGraines>();
        signal = drone.GetComponent<DroneSignal>();
        dernierePosition = drone.position;
        if (lanceur != null)
        {
            derniereSelection = lanceur.selection;
            lanceur.QuandLacher += SurLacher;
        }
    }

    void OnDestroy()
    {
        if (partie != null)
        {
            partie.QuandRecolte -= SurRecolte;
            partie.QuandExplosion -= SurExplosion;
        }
        if (lanceur != null) lanceur.QuandLacher -= SurLacher;
    }

    void SurLacher() => Jouer(graineLacher, 0.7f, 0.1f);
    void SurRecolte(TypePlante type, Vector3 position) => Jouer(recolte, 0.8f, 0.12f);
    void SurExplosion(TypePlante type, Vector3 position) => Jouer(explosion, 1f, 0.1f);

    AudioSource Boucle(AudioClip son)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.clip = son;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        if (son != null) source.Play();
        return source;
    }

    // joue un son avec une petite variation de hauteur
    void Jouer(AudioClip son, float volume = 1f, float variation = 0.08f)
    {
        if (son == null) return;
        var source = sourcesEffets[prochaineSource];
        prochaineSource = (prochaineSource + 1) % sourcesEffets.Length;
        source.pitch = 1f + Random.Range(-variation, variation);
        source.PlayOneShot(son, volume * volumeEffets * volumeGeneral);
    }

    void Update()
    {
        if (drone == null) return;
        float deltaTemps = Mathf.Max(Time.deltaTime, 0.0001f);
        var etat = signal != null ? signal.EtatActuel : DroneSignal.Etat.Connecte;

        // moteur
        Vector3 positionDrone = drone.position;
        Vector3 deplacement = positionDrone - dernierePosition;
        deplacement.y = 0;
        dernierePosition = positionDrone;
        vitesseLissee = Mathf.Lerp(vitesseLissee, deplacement.magnitude / deltaTemps, 5f * deltaTemps);

        float hauteurSonCible = 0.9f + 0.35f * Mathf.Clamp01(vitesseLissee / 8f);
        float volumeCible = volumeMoteur;
        if (etat == DroneSignal.Etat.Chute)
        {
            hauteurSonCible = 0.35f;
            volumeCible = volumeMoteur * 0.5f;
        }
        else if (etat == DroneSignal.Etat.Reconnexion)
        {
            hauteurSonCible = 0.3f;
            volumeCible = 0f;
        }
        sourceMoteur.pitch = Mathf.Lerp(sourceMoteur.pitch, hauteurSonCible, (etat == DroneSignal.Etat.Connecte ? 6f : 1.5f) * deltaTemps);
        sourceMoteur.volume = Mathf.Lerp(sourceMoteur.volume, volumeCible * volumeGeneral, 4f * deltaTemps);

        // tornade
        bool actif = aspirateur != null && aspirateur.enabled && aspirateur.Actif;
        sourceTornade.volume = Mathf.MoveTowards(sourceTornade.volume, actif ? volumeTornade * volumeGeneral : 0f, 2.5f * deltaTemps);
        sourceTornade.pitch = actif ? 1f + 0.05f * Mathf.Sin(Time.time * 2f) : 1f;

        // arrosage : boucle d'eau tant que le jet est actif
        bool arrose = arroseur != null && arroseur.enabled && arroseur.Actif;
        sourceArrosage.volume = Mathf.MoveTowards(sourceArrosage.volume, arrose ? volumeArrosage * volumeGeneral : 0f, 4f * deltaTemps);

        // parasites radio
        float niveauSignal = signal != null ? signal.Signal : 1f;
        float niveauParasites = etat == DroneSignal.Etat.Chute ? 1f : etat == DroneSignal.Etat.Reconnexion ? 0.3f : (1f - niveauSignal);
        sourceParasites.volume = Mathf.Lerp(sourceParasites.volume, niveauParasites * volumeParasites * volumeGeneral, 6f * deltaTemps);

        // bips d'alerte, de plus en plus rapides
        if (etat == DroneSignal.Etat.Connecte && niveauSignal < 1f && Time.time >= prochainBip)
        {
            Jouer(alerteBip, 0.7f, 0f);
            prochainBip = Time.time + Mathf.Lerp(0.18f, 0.9f, niveauSignal);
        }

        if (etat != dernierEtat)
        {
            if (etat == DroneSignal.Etat.Chute)
            {
                Jouer(signalPerdu, 1f, 0f);
                impactJoue = false;
            }
            if (etat == DroneSignal.Etat.Connecte && dernierEtat == DroneSignal.Etat.Reconnexion)
                Jouer(reconnexion, 0.9f, 0f);
            dernierEtat = etat;
        }
        if (etat == DroneSignal.Etat.Chute && !impactJoue && signal.AuSol)
        {
            Jouer(chuteImpact, 1f, 0.05f);
            impactJoue = true;
        }

        if (lanceur != null && lanceur.selection != derniereSelection)
        {
            derniereSelection = lanceur.selection;
            Jouer(interfaceSelection, 0.8f, 0.02f);
        }
    }
}
