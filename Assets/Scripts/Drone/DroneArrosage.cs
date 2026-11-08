using UnityEngine;

// arrose les plantes sous le drone pour qu'elles poussent plus vite
[RequireComponent(typeof(DroneCommandes))]
public class DroneArrosage : MonoBehaviour
{
    public ZoneDrone zone;
    public ParticleSystem eau;
    public float dureeEffet = 4f;
    public float multiplicateurCroissance = 2.5f;

    public bool Actif { get; private set; }

    DroneCommandes commandes;

    void Awake()
    {
        commandes = GetComponent<DroneCommandes>();
        if (zone == null) zone = GetComponentInChildren<ZoneDrone>();
    }

    void Update()
    {
        Actif = commandes.ArroserMaintenu;

        if (eau != null)
        {
            if (Actif && !eau.isEmitting) eau.Play();
            else if (!Actif && eau.isEmitting) eau.Stop();
        }
        if (!Actif || zone == null) return;

        foreach (var plante in zone.Plantes())
            plante.Arroser(dureeEffet, multiplicateurCroissance);
    }

    void OnDisable()
    {
        Actif = false;
        if (eau != null) eau.Stop();
    }
}
