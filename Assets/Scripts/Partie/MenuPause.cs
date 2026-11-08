using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// menu pause en cours de partie : reprendre, recommencer, revenir au menu ou quitter
public class MenuPause : MonoBehaviour
{
    [Header("Scènes")]
    public string sceneJeu = "Jeu";
    public string sceneMenu = "Menu";

    [Header("Interface")]
    public GameObject panneau;
    public Button boutonReprendre;
    public Button boutonRecommencer;
    public Button boutonMenu;
    public Button boutonQuitter;

    [Header("Références")]
    public DroneCommandes commandes;

    public bool EnPause { get; private set; }

    void Start()
    {
        boutonReprendre.onClick.AddListener(Reprendre);
        boutonRecommencer.onClick.AddListener(() => Charger(sceneJeu));
        boutonMenu.onClick.AddListener(() => Charger(sceneMenu));
        if (boutonQuitter != null)
        {
            // pas de bouton quitter dans le navigateur
            boutonQuitter.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
            boutonQuitter.onClick.AddListener(Application.Quit);
        }
        panneau.SetActive(false);
    }

    public void Basculer()
    {
        if (EnPause) Reprendre();
        else MettreEnPause();
    }

    void MettreEnPause()
    {
        EnPause = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        if (commandes != null) commandes.BloquerJeu(true);
        panneau.SetActive(true);
        boutonReprendre.Select();
    }

    void Reprendre()
    {
        EnPause = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (commandes != null) commandes.BloquerJeu(false);
        panneau.SetActive(false);
    }

    void Charger(string scene)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(scene);
    }

    // sécurité : on ne laisse jamais le jeu figé en quittant la scène
    void OnDestroy()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
