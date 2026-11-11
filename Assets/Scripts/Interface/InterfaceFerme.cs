// InterfaceFerme : met à jour toute l'interface en jeu.
// Score, chrono, compteurs des plantes, barre de graines, aide des touches et signal radio.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InterfaceFerme : MonoBehaviour
{
    [Header("Références")]
    [FormerlySerializedAs("dropper")] public LanceurGraines lanceur;
    public Partie partie;
    public DroneSignal signal;
    public DroneCommandes commandes;

    [Header("Couleurs")]
    public Color couleurSelection = new Color(1f, 0.82f, 0.2f, 1f);
    public Color couleurBordure = new Color(1f, 1f, 1f, 0.18f);
    public Color couleurTexte = Color.white;
    public float echelleSelection = 1.06f;

    [Header("Panneau ferme")]
    public Text txtCroissance;
    public Text txtMures;
    public Text txtPourries;
    public Text txtRecoltees;
    public Text txtTotal;

    [Header("Partie")]
    public Text txtScore;
    public Text txtChrono;
    [FormerlySerializedAs("animScore")] public Animator animateurScore;

    [Header("Barre de graines")]
    [FormerlySerializedAs("slotBordures")] public Image[] caseBordures;
    [FormerlySerializedAs("slotBadges")] public Image[] casePastilles;
    [FormerlySerializedAs("slotNoms")] public Text[] caseNoms;
    [FormerlySerializedAs("slotCompteurs")] public Text[] caseCompteurs;
    public bool synchroniserAvecLesGraines = true;

    [Header("Aide")]
    public Text txtAide;
    [Tooltip("Écrit l'aide à partir des touches de l'asset Input Actions")]
    [FormerlySerializedAs("adapterTouchesAuClavier")] public bool lireTouchesDepuisLesActions = true;
    [Tooltip("Groupe de bindings du clavier dans l'asset Input Actions")]
    public string groupeClavier = "Keyboard";
    [Tooltip("Groupe de bindings de la manette dans l'asset Input Actions")]
    public string groupeManette = "Gamepad";

    [Header("Signal radio")]
    public Image[] barresSignal;
    public Text txtSignal;
    public Text txtAlerte;
    public RawImage parasites;
    public Image ecranNoir;
    public Text txtNoir;
    public Color couleurSignalBon = new Color(0.45f, 0.9f, 0.35f);
    public Color couleurSignalMoyen = new Color(1f, 0.65f, 0.15f);
    public Color couleurSignalFaible = new Color(1f, 0.25f, 0.2f);
    public Color couleurBarreVide = new Color(1f, 1f, 1f, 0.15f);
    [Range(0, 1)] public float intensiteParasites = 0.45f;

    int derniereSelection = -1;
    float prochaineMiseAJour;
    string aideOriginale;
    int dernierScore = -1, derniereSeconde = -1;

    void Start()
    {
        if (lanceur != null && synchroniserAvecLesGraines) SynchroniserGraines();

        if (txtAide != null) aideOriginale = txtAide.text;
        if (commandes != null) commandes.QuandPeripheriqueChange += MajAide;
        if (partie != null) partie.QuandPoints += SurPoints;
        MajAide(commandes != null && commandes.ManetteActive);
        MajStatistiques();
    }

    void OnDestroy()
    {
        if (commandes != null) commandes.QuandPeripheriqueChange -= MajAide;
        if (partie != null) partie.QuandPoints -= SurPoints;
    }

    void SurPoints(int points, Vector3 position)
    {
        if (animateurScore != null) animateurScore.SetTrigger("Pulsation");
    }

    void MajPartie()
    {
        if (partie == null) return;
        if (partie.Score != dernierScore)
        {
            dernierScore = partie.Score;
            Ecrire(txtScore, dernierScore.ToString());
        }
        int secondes = Mathf.CeilToInt(partie.TempsRestant);
        if (secondes != derniereSeconde)
        {
            derniereSeconde = secondes;
            Ecrire(txtChrono, $"{secondes / 60}:{secondes % 60:00}");
        }
    }

    // l'aide affiche les touches réellement assignées dans l'asset Input Actions,
    // donc elle suit automatiquement un changement de touche ou de disposition (AZERTY / QWERTY)
    void MajAide(bool manette)
    {
        if (txtAide == null) return;
        if (!lireTouchesDepuisLesActions || commandes == null)
        {
            txtAide.text = aideOriginale;
            return;
        }

        string groupe = manette ? groupeManette : groupeClavier;
        string choixGraine = manette
            ? $"{Touches(groupe, commandes.grainePrecedente, commandes.graineSuivante)} ou {Touches(groupe, commandes.graine1, commandes.graine2, commandes.graine3)}"
            : Touches(groupe, commandes.graine1, commandes.graine2, commandes.graine3);

        txtAide.text =
            $"<b>{Touches(groupe, commandes.deplacer)}</b>  se déplacer      " +
            $"<b>{choixGraine}</b>  choisir la graine      " +
            $"<b>{Touches(groupe, commandes.lacher)}</b>  lâcher      " +
            $"<b>{Touches(groupe, commandes.arroser)}</b>  arroser      " +
            $"<b>{Touches(groupe, commandes.aspirer)}</b>  aspirer";
    }

    // nom des touches d'une ou plusieurs actions pour un périphérique (premier binding du groupe seulement)
    static string Touches(string groupe, params InputActionReference[] actions)
    {
        var noms = new System.Collections.Generic.List<string>();
        foreach (var reference in actions)
        {
            if (reference == null) continue;
            string nom = Traduire(NomPremiereTouche(reference.action, groupe));
            if (!string.IsNullOrEmpty(nom) && !noms.Contains(nom)) noms.Add(nom);
        }
        return noms.Count > 0 ? string.Join(" ", noms) : "?";
    }

    static string NomPremiereTouche(InputAction action, string groupe)
    {
        int index = action.GetBindingIndex(group: groupe);
        if (index < 0) return "";

        // une partie de composite (ZQSD) : on affiche le composite entier
        if (action.bindings[index].isPartOfComposite)
            while (index > 0 && !action.bindings[index].isComposite) index--;

        // les touches chiffres : on affiche le chiffre plutôt que le caractère AZERTY (& é ")
        string chemin = action.bindings[index].effectivePath;
        string fin = chemin.Substring(chemin.LastIndexOf('/') + 1);
        if (chemin.StartsWith("<Keyboard>") && fin.Length == 1 && char.IsDigit(fin[0])) return fin;

        return action.GetBindingDisplayString(index, InputBinding.DisplayStringOptions.DontIncludeInteractions);
    }

    // noms de touches à rendre plus lisibles à l'écran
    static string Traduire(string nom)
    {
        if (string.IsNullOrEmpty(nom)) return nom;
        if (nom.StartsWith("D-Pad") || nom.StartsWith("Dpad")) return "CROIX";
        switch (nom)
        {
            case "LS": return "STICK G";
            case "RS": return "STICK D";
            default: return nom.Replace("/", " ").ToUpper();
        }
    }

    void SynchroniserGraines()
    {
        for (int i = 0; i < lanceur.typesPlantes.Length; i++)
        {
            var type = lanceur.typesPlantes[i];
            if (type == null) continue;
            if (caseNoms != null && i < caseNoms.Length && caseNoms[i] != null) caseNoms[i].text = type.nom;
            if (casePastilles != null && i < casePastilles.Length && casePastilles[i] != null) casePastilles[i].color = type.couleurGraine;
        }
    }

    void Update()
    {
        MajSignal();
        MajPartie();
        if (lanceur == null) return;

        if (lanceur.selection != derniereSelection && caseBordures != null)
        {
            derniereSelection = lanceur.selection;
            for (int i = 0; i < caseBordures.Length; i++)
            {
                if (caseBordures[i] == null) continue;
                bool estSelectionnee = i == derniereSelection;
                caseBordures[i].color = estSelectionnee ? couleurSelection : couleurBordure;
                caseBordures[i].rectTransform.localScale = Vector3.one * (estSelectionnee ? echelleSelection : 1f);
            }
        }

        if (Time.time >= prochaineMiseAJour)
        {
            prochaineMiseAJour = Time.time + 0.25f;
            MajStatistiques();
        }
    }

    // compte les plantes par état pour le panneau
    void MajStatistiques()
    {
        if (lanceur == null || partie == null) return;
        int croissance = 0, mures = 0, pourries = 0;
        int nombreCases = caseCompteurs != null ? Mathf.Min(caseCompteurs.Length, lanceur.typesPlantes.Length) : 0;
        int[] parType = new int[nombreCases];

        foreach (var plante in partie.Plantes)
        {
            if (plante.EstMure) mures++;
            else if (plante.EstPourrie) pourries++;
            else croissance++;

            for (int i = 0; i < nombreCases; i++)
                if (plante.Type == lanceur.typesPlantes[i]) parType[i]++;
        }

        Ecrire(txtCroissance, croissance.ToString());
        Ecrire(txtMures, mures.ToString());
        Ecrire(txtPourries, pourries.ToString());
        Ecrire(txtRecoltees, partie.TotalRecoltes.ToString());
        Ecrire(txtTotal, (croissance + mures + pourries) + " au champ");
        for (int i = 0; i < nombreCases; i++)
            Ecrire(caseCompteurs[i], $"stock {partie.NombreRecoltes(lanceur.typesPlantes[i])} | {parType[i]} au champ");
    }

    // barres de signal, parasites et écran noir
    void MajSignal()
    {
        float niveauSignal = signal != null ? signal.Signal : 1f;
        var etat = signal != null ? signal.EtatActuel : DroneSignal.Etat.Connecte;
        bool connecte = etat == DroneSignal.Etat.Connecte;

        if (barresSignal != null)
        {
            int barresActives = connecte ? Mathf.Clamp(Mathf.CeilToInt(niveauSignal * barresSignal.Length), 0, barresSignal.Length) : 0;
            Color couleurBarres = niveauSignal > 0.6f ? couleurSignalBon : niveauSignal > 0.3f ? couleurSignalMoyen : couleurSignalFaible;
            for (int i = 0; i < barresSignal.Length; i++)
                if (barresSignal[i] != null) barresSignal[i].color = i < barresActives ? couleurBarres : couleurBarreVide;
        }

        if (txtSignal != null)
        {
            txtSignal.text = connecte ? (niveauSignal >= 1f ? "SIGNAL" : "SIGNAL FAIBLE") : "AUCUN SIGNAL";
            txtSignal.color = connecte && niveauSignal >= 1f ? couleurTexte : couleurSignalFaible;
        }

        if (parasites != null)
        {
            float opacite = etat == DroneSignal.Etat.Chute ? 0.75f : etat == DroneSignal.Etat.Reconnexion ? 0.35f : (1f - niveauSignal) * intensiteParasites;
            parasites.color = new Color(1, 1, 1, opacite);
            if (opacite > 0.001f) parasites.uvRect = new Rect(Random.value, Random.value, 1f, 1f);
        }

        if (txtAlerte != null)
        {
            if (etat == DroneSignal.Etat.Chute)
            {
                txtAlerte.text = "SIGNAL PERDU !";
                txtAlerte.color = new Color(couleurSignalFaible.r, couleurSignalFaible.g, couleurSignalFaible.b, 1f);
            }
            else if (connecte && niveauSignal < 1f)
            {
                txtAlerte.text = "SIGNAL FAIBLE, FAIS DEMI-TOUR !";
                txtAlerte.color = new Color(1f, 0.35f, 0.2f, 0.35f + 0.65f * Mathf.PingPong(Time.time * 3f, 1f));
            }
            else txtAlerte.text = "";
        }

        float fondu = signal != null ? signal.Fondu : 0f;
        if (ecranNoir != null) ecranNoir.color = new Color(0, 0, 0, fondu);
        if (txtNoir != null) txtNoir.color = new Color(1, 1, 1, fondu);
    }

    static void Ecrire(Text texte, string contenu)
    {
        if (texte != null) texte.text = contenu;
    }
}
