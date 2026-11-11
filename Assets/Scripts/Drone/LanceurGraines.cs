// LanceurGraines : choix de la graine et largage sous le drone.
// Transmet la Partie et le conteneur des plantes à chaque graine créée.

using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(DroneCommandes), typeof(DroneCorps))]
public class LanceurGraines : MonoBehaviour
{
    [FormerlySerializedAs("plantTypes")] public TypePlante[] typesPlantes;
    [FormerlySerializedAs("selected")] public int selection = 0;

    [Header("Largage")]
    [FormerlySerializedAs("dropPoint")] public Transform pointLargage;
    [FormerlySerializedAs("dropOffset")] public float decalageLargage = 0.1f;
    [FormerlySerializedAs("cooldown")] public float delai = 0.3f;
    [FormerlySerializedAs("seedSize")] public float tailleGraine = 0.3f;
    public Graine prefabGraine;

    [Header("Plantation")]
    [FormerlySerializedAs("groundLayers")] public LayerMask calquesSol = ~0;
    [FormerlySerializedAs("minSpacing")] public float espacementMin = 0.8f;

    [Header("Références")]
    [Tooltip("Partie en cours, transmise aux graines puis aux plantes")]
    public Partie partie;
    [Tooltip("Parent des graines lâchées, pour garder la Hierarchy lisible")]
    public Transform conteneurGraines;
    [Tooltip("Parent des plantes qui poussent")]
    public Transform conteneurPlantes;

    public event Action QuandLacher;

    float prochainLargage;
    Collider[] collisionneursDrone;
    DroneCommandes commandes;
    DroneCorps corps;

    void Awake()
    {
        collisionneursDrone = GetComponentsInChildren<Collider>();
        commandes = GetComponent<DroneCommandes>();
        corps = GetComponent<DroneCorps>();
    }

    void Update()
    {
        if (typesPlantes == null || typesPlantes.Length == 0) return;

        int graineChoisie = commandes.GraineDirecte;
        if (graineChoisie >= 0 && graineChoisie < typesPlantes.Length) selection = graineChoisie;

        // LB/RB pour changer de graine, on boucle sur la liste
        int changement = commandes.ChangementGraine;
        if (changement != 0) selection = (selection + changement + typesPlantes.Length) % typesPlantes.Length;

        if (commandes.LacherPresse && Time.time >= prochainLargage)
        {
            prochainLargage = Time.time + delai;
            Lacher();
        }
    }

    // instancie la graine juste sous le drone
    void Lacher()
    {
        var type = typesPlantes[selection];
        if (type == null || prefabGraine == null) return;

        var graine = Instantiate(prefabGraine, PositionLargage(), Quaternion.identity, conteneurGraines);
        foreach (var collisionneur in collisionneursDrone)
            if (collisionneur != null) Physics.IgnoreCollision(graine.ZoneCollision, collisionneur);
        graine.Initialiser(type, partie, conteneurPlantes, calquesSol, espacementMin, tailleGraine);

        QuandLacher?.Invoke();
    }

    Vector3 PositionLargage()
    {
        if (pointLargage != null) return pointLargage.position;
        Bounds limitesDrone = corps.Limites;
        return new Vector3(limitesDrone.center.x, limitesDrone.min.y - tailleGraine - decalageLargage, limitesDrone.center.z);
    }
}
