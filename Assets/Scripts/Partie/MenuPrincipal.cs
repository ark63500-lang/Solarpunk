// MenuPrincipal : écran titre du jeu.
// Affiche le meilleur score et gère les boutons Jouer et Quitter.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    public string sceneJeu = "Jeu";
    public Text txtMeilleurScore;
    public Button boutonJouer;
    public Button boutonQuitter;

    void Start()
    {
        if (txtMeilleurScore != null) txtMeilleurScore.text = "Meilleur score : " + Sauvegarde.MeilleurScore;

        boutonJouer.onClick.AddListener(() => SceneManager.LoadScene(sceneJeu));
        if (boutonQuitter != null)
        {
            // pas de bouton quitter dans le navigateur
            boutonQuitter.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
            boutonQuitter.onClick.AddListener(Application.Quit);
        }
        boutonJouer.Select();
    }
}
