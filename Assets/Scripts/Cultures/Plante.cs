using System.Collections;
using UnityEngine;

// une plante dans le champ, elle passe par 4 stades puis explose si on la récolte pas
public class Plante : MonoBehaviour
{
    [SerializeField] Transform visuel;
    [SerializeField] Animator animateur;
    [SerializeField] GameObject effetExplosion;

    public TypePlante Type { get; private set; }
    public int Stade { get; private set; } = -1;
    public bool EstMure => Stade == 2;
    public bool EstPourrie => Stade == 3;
    public bool Aspiree { get; private set; }
    public bool EstArrosee => Time.time < finArrosage;

    GameObject modele;
    float finArrosage;
    float multiplicateur = 1f;
    Partie partie;

    // la plante est créée pendant la partie : on lui donne la Partie à sa naissance
    public void Initialiser(TypePlante type, Partie partieEnCours)
    {
        Type = type;
        partie = partieEnCours;
        if (partie != null) partie.AjouterPlante(this);
        StartCoroutine(Cycle());
    }

    void OnDestroy()
    {
        if (partie != null) partie.RetirerPlante(this);
    }

    // cycle de vie : plantée -> pousse -> mûre -> pourrie -> boom
    IEnumerator Cycle()
    {
        ChangerStade(0);
        yield return Attendre(Type.tempsPousse, true);
        ChangerStade(1);
        yield return Attendre(Type.tempsMaturite, true);
        ChangerStade(2);
        yield return Attendre(Type.tempsAvantPourriture, false);
        ChangerStade(3);
        yield return Attendre(Type.tempsAvantExplosion, false);
        Exploser();
    }

    // l'arrosage accélère seulement la croissance, pas le pourrissement
    IEnumerator Attendre(float duree, bool accelerable)
    {
        float tempsEcoule = 0f;
        while (tempsEcoule < duree)
        {
            float vitesse = accelerable && EstArrosee ? multiplicateur : 1f;
            tempsEcoule += Time.deltaTime * vitesse;
            yield return null;
        }
    }

    // appelé par le drone quand il arrose au dessus
    public void Arroser(float duree, float multiplicateurCroissance)
    {
        if (Stade >= 2) return;
        finArrosage = Time.time + duree;
        multiplicateur = multiplicateurCroissance;
    }

    // on remplace le modèle 3D par celui du nouveau stade
    void ChangerStade(int nouveauStade)
    {
        if (Type.stades == null || nouveauStade >= Type.stades.Length || Type.stades[nouveauStade] == null)
        {
            Debug.LogWarning($"{Type.nom} : modèle manquant pour le stade {nouveauStade}");
            return;
        }
        if (modele != null) Destroy(modele);
        modele = Instantiate(Type.stades[nouveauStade], visuel != null ? visuel : transform);
        modele.transform.localPosition = Vector3.zero;
        Stade = nouveauStade;

        if (animateur != null)
        {
            animateur.SetTrigger("Apparition");
            animateur.SetBool("Pourrie", EstPourrie);
        }
    }

    // trop mûre : effet + on prévient la Partie (pénalité et son)
    void Exploser()
    {
        if (effetExplosion != null) Instantiate(effetExplosion, transform.position + Vector3.up * 0.3f, Quaternion.identity);
        if (partie != null) partie.SignalerExplosion(Type, transform.position);
        Destroy(gameObject);
    }

    public void CommencerAspiration()
    {
        Aspiree = true;
        StopAllCoroutines();
        if (partie != null) partie.RetirerPlante(this);

        var zoneCollision = GetComponent<Collider>();
        if (zoneCollision != null) zoneCollision.enabled = false;
        if (animateur != null) animateur.enabled = false;
    }
}
