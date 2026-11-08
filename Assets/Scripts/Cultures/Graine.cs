using UnityEngine;
using UnityEngine.Serialization;

// la graine lâchée par le drone, si elle tombe sur un champ ça fait une plante
[RequireComponent(typeof(Rigidbody))]
public class Graine : MonoBehaviour
{
    [SerializeField] Plante prefabPlante;
    [SerializeField] GameObject effetPlantation;
    [SerializeField] Renderer rendu;
    [SerializeField] TrailRenderer trainee;
    [FormerlySerializedAs("col")] [SerializeField] Collider zoneCollision;

    public Collider ZoneCollision => zoneCollision;

    TypePlante type;
    Partie partie;
    Transform conteneurPlantes;
    LayerMask calquesSol;
    float espacementMin;
    bool posee;

    public void Initialiser(TypePlante typePlante, Partie partieEnCours, Transform conteneur, LayerMask sol, float espacement, float taille, float dureeVie = 10f)
    {
        type = typePlante;
        partie = partieEnCours;
        conteneurPlantes = conteneur;
        calquesSol = sol;
        espacementMin = espacement;
        transform.localScale = Vector3.one * taille;

        var proprietes = new MaterialPropertyBlock();
        proprietes.SetColor("_BaseColor", type.couleurGraine);
        proprietes.SetColor("_Color", type.couleurGraine);
        rendu.SetPropertyBlock(proprietes);

        trainee.startWidth = taille * 0.8f;
        trainee.startColor = type.couleurGraine;
        trainee.endColor = new Color(type.couleurGraine.r, type.couleurGraine.g, type.couleurGraine.b, 0f);

        Destroy(gameObject, dureeVie);
    }

    // quand la graine touche quelque chose
    void OnCollisionEnter(Collision collision)
    {
        if (posee || type == null) return;
        if (collision.gameObject.GetComponent<Graine>() != null) return;

        posee = true;

        // hors champ, la graine est perdue
        if ((calquesSol.value & (1 << collision.gameObject.layer)) == 0)
        {
            Destroy(gameObject, 0.3f);
            return;
        }

        Vector3 pointImpact = collision.GetContact(0).point;
        // pas de nouvelle plante si y'en a déjà une trop proche
        if (partie != null)
        {
            foreach (var plante in partie.Plantes)
            {
                if (Vector3.Distance(plante.transform.position, pointImpact) < espacementMin)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        var rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        // chaque légume peut avoir sa propre variante du prefab Plante
        Plante modelePlante = type.prefabPlante != null ? type.prefabPlante : prefabPlante;
        var nouvellePlante = Instantiate(modelePlante, pointImpact, rotation, conteneurPlantes);
        nouvellePlante.transform.localScale = Vector3.one * type.echelle;
        nouvellePlante.Initialiser(type, partie);
        if (effetPlantation != null) Instantiate(effetPlantation, pointImpact, Quaternion.identity);

        Destroy(gameObject);
    }
}
