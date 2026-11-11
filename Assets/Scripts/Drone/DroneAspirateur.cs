// DroneAspirateur : récolte les légumes mûrs sous le drone avec une tornade.
// Le légume est aspiré en spirale vers le drone, puis la Partie compte la récolte et les points.
// Les légumes pourris ne se récoltent pas : ils finissent par éclater.

using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(DroneCommandes), typeof(DroneCorps))]
public class DroneAspirateur : MonoBehaviour
{
    [Header("Aspiration")]
    public float rayon = 2.5f;
    public float dureeAspiration = 0.9f;
    [FormerlySerializedAs("hauteurSol")] public float hauteurChamps = 0.77f;
    public float rayonDepart = 0.3f;
    public ZoneDrone zone;

    [Header("Récolte")]
    public GameObject effetRecolte;
    public Partie partie;

    [Header("Tornade")]
    public Material materiauTornade;
    public Color couleurTornade = new Color(0.85f, 0.9f, 1f, 0.55f);

    public bool Actif { get; private set; }

    ParticleSystem tornade, poussiere;
    DroneCommandes commandes;
    DroneCorps corps;

    void Awake()
    {
        commandes = GetComponent<DroneCommandes>();
        corps = GetComponent<DroneCorps>();
        if (zone == null) zone = GetComponentInChildren<ZoneDrone>();

        tornade = CreerParticules("Tornade", 220, 0.28f, 0.55f, 0.9f);
        var formeTornade = tornade.shape;
        formeTornade.radius = rayonDepart;
        var tailleTornade = tornade.sizeOverLifetime;
        tailleTornade.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.3f));

        poussiere = CreerParticules("Poussiere", 45, 0.35f, 0.7f, 0.6f);
        var vitessePoussiere = poussiere.velocityOverLifetime;
        vitessePoussiere.y = 0.6f;
        vitessePoussiere.radial = 0.9f;
        var reglagesPoussiere = poussiere.main;
        reglagesPoussiere.startColor = new Color(0.55f, 0.42f, 0.3f, 0.45f);
    }

    void Update()
    {
        Actif = commandes.AspirerMaintenu;

        Bounds limitesDrone = corps.Limites;
        Vector3 centreDrone = limitesDrone.center;
        float basDrone = limitesDrone.min.y;
        tornade.transform.position = new Vector3(centreDrone.x, basDrone, centreDrone.z);
        poussiere.transform.position = new Vector3(centreDrone.x, hauteurChamps, centreDrone.z);

        float hauteurTornade = Mathf.Max(0.5f, basDrone - hauteurChamps);
        float dureeVie = tornade.main.startLifetime.constant;
        var vitesseTornade = tornade.velocityOverLifetime;
        vitesseTornade.y = -hauteurTornade / dureeVie;
        vitesseTornade.radial = (rayon - rayonDepart) / dureeVie;

        Emettre(tornade, Actif);
        Emettre(poussiere, Actif);

        if (!Actif) return;

        if (zone == null) return;
        foreach (var plante in zone.Plantes())
        {
            if (plante.Aspiree) continue;
            // seul un légume mûr est récolté
            if (plante.EstMure) StartCoroutine(Aspirer(plante));
        }
    }

    void OnDisable()
    {
        Actif = false;
        if (tornade != null) Emettre(tornade, false);
        if (poussiere != null) Emettre(poussiere, false);
    }

    // la plante tourne en spirale vers le drone puis disparait
    IEnumerator Aspirer(Plante plante)
    {
        TypePlante type = plante.Type;
        plante.CommencerAspiration();

        Transform objetPlante = plante.transform;
        Vector3 depart = objetPlante.position;
        Vector3 ecart = depart - corps.Centre;
        ecart.y = 0;
        float rayonInitial = ecart.magnitude;
        float angleInitial = Mathf.Atan2(ecart.z, ecart.x);
        Vector3 echelleInitiale = objetPlante.localScale;

        for (float tempsEcoule = 0; tempsEcoule < dureeAspiration; tempsEcoule += Time.deltaTime)
        {
            if (objetPlante == null) yield break;

            float progression = tempsEcoule / dureeAspiration;
            float acceleration = progression * progression;
            Bounds limitesDrone = corps.Limites;
            float angle = angleInitial + progression * 4f * Mathf.PI;
            float rayonActuel = Mathf.Lerp(rayonInitial, 0f, acceleration);
            float hauteurActuelle = Mathf.Lerp(depart.y, limitesDrone.min.y, acceleration);

            objetPlante.position = new Vector3(
                limitesDrone.center.x + Mathf.Cos(angle) * rayonActuel,
                hauteurActuelle,
                limitesDrone.center.z + Mathf.Sin(angle) * rayonActuel);
            objetPlante.Rotate(Vector3.up, 720f * Time.deltaTime, Space.World);
            objetPlante.localScale = echelleInitiale * Mathf.Lerp(1f, 0.15f, acceleration);
            yield return null;
        }

        if (objetPlante == null) yield break;

        // la Partie compte la récolte et donne les points du légume
        if (partie != null) partie.EnregistrerRecolte(type, objetPlante.position);
        if (effetRecolte != null) Instantiate(effetRecolte, objetPlante.position, Quaternion.identity);
        Destroy(objetPlante.gameObject);
    }

    static void Emettre(ParticleSystem systeme, bool actif)
    {
        if (actif && !systeme.isEmitting) systeme.Play();
        else if (!actif && systeme.isEmitting) systeme.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    // particules de la tornade et de la poussière faites par code
    ParticleSystem CreerParticules(string nom, float taux, float tailleMin, float tailleMax, float dureeVie)
    {
        var objet = new GameObject(nom);
        var systeme = objet.AddComponent<ParticleSystem>();
        systeme.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var reglages = systeme.main;
        reglages.loop = true;
        reglages.startLifetime = dureeVie;
        reglages.startSpeed = 0f;
        reglages.startSize = new ParticleSystem.MinMaxCurve(tailleMin, tailleMax);
        reglages.startColor = couleurTornade;
        reglages.simulationSpace = ParticleSystemSimulationSpace.Local;
        reglages.maxParticles = 600;
        reglages.playOnAwake = false;

        var emission = systeme.emission;
        emission.rateOverTime = taux;

        var forme = systeme.shape;
        forme.shapeType = ParticleSystemShapeType.Circle;
        forme.radius = rayon;
        forme.rotation = new Vector3(90f, 0f, 0f);

        var vitesse = systeme.velocityOverLifetime;
        vitesse.enabled = true;
        vitesse.space = ParticleSystemSimulationSpace.Local;
        vitesse.orbitalX = 0f;
        vitesse.orbitalY = 7f;
        vitesse.orbitalZ = 0f;
        vitesse.radial = -2.4f;
        vitesse.x = 0f;
        vitesse.y = 3f;
        vitesse.z = 0f;

        var couleur = systeme.colorOverLifetime;
        couleur.enabled = true;
        var degrade = new Gradient();
        degrade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0.8f, 0.7f), new GradientAlphaKey(0f, 1f) });
        couleur.color = degrade;

        var taille = systeme.sizeOverLifetime;
        taille.enabled = true;
        taille.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.4f));

        var rendu = objet.GetComponent<ParticleSystemRenderer>();
        rendu.sharedMaterial = materiauTornade;
        rendu.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return systeme;
    }
}
