using UnityEngine;
using UnityEngine.Serialization;

// scriptable object pour régler chaque légume (carotte, fraise, maïs)
[CreateAssetMenu(fileName = "NouvellePlante", menuName = "Ferme/Type de plante")]
public class TypePlante : ScriptableObject
{
    [FormerlySerializedAs("plantName")] public string nom = "Plante";
    [FormerlySerializedAs("seedColor")] public Color couleurGraine = Color.white;

    [Tooltip("Variante du prefab Plante pour ce légume (sinon le prefab Plante de la graine)")]
    public Plante prefabPlante;

    [Tooltip("0 plantée, 1 pousse, 2 mûre, 3 pourrie")]
    [FormerlySerializedAs("stages")] public GameObject[] stades = new GameObject[4];

    [Header("Durées (secondes)")]
    [FormerlySerializedAs("timeToSprout")] public float tempsPousse = 5f;
    [FormerlySerializedAs("timeToRipe")] public float tempsMaturite = 10f;
    [FormerlySerializedAs("timeBeforeRot")] public float tempsAvantPourriture = 20f;
    public float tempsAvantExplosion = 8f;

    [Header("Score")]
    public int points = 10;
    public int penaliteExplosion = 5;

    [FormerlySerializedAs("scale")] public float echelle = 1f;
}
