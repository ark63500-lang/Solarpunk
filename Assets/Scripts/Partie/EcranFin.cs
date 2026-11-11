// EcranFin : écran de fin de partie.
// Affiche le score, le meilleur score et le nombre de légumes récoltés, et gère les boutons Rejouer et Menu.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EcranFin : MonoBehaviour
{
    public string sceneJeu = "Jeu";
    public string sceneMenu = "Menu";
    public Text txtScore;
    public Text txtMeilleur;
    public Text txtRecoltes;
    public GameObject nouveauRecord;
    public Button boutonRejouer;
    public Button boutonMenu;

    void Start()
    {
        txtScore.text = "Score : " + Sauvegarde.DernierScore;
        txtMeilleur.text = "Meilleur score : " + Sauvegarde.MeilleurScore;
        txtRecoltes.text = Sauvegarde.DernieresRecoltes + " légumes récoltés";
        if (nouveauRecord != null) nouveauRecord.SetActive(Sauvegarde.NouveauRecord);

        boutonRejouer.onClick.AddListener(() => SceneManager.LoadScene(sceneJeu));
        boutonMenu.onClick.AddListener(() => SceneManager.LoadScene(sceneMenu));
        boutonRejouer.Select();
    }
}
