// Partie : état d'une partie (score, chrono, plantes au champ, récoltes).
// Pas de singleton ni de static : les objets qui en ont besoin reçoivent une référence à la Partie (injection).
// Prévient le reste du jeu par des événements (points, récolte, explosion) et lance l'écran de fin.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Partie : MonoBehaviour
{
    public event Action<int, Vector3> QuandPoints;
    public event Action<TypePlante, Vector3> QuandRecolte;
    public event Action<TypePlante, Vector3> QuandExplosion;

    [Header("Règles")]
    public float duree = 180f;
    public float delaiAvantFin = 1.5f;

    [Header("Scènes")]
    public string sceneFin = "Fin";

    [Header("Références")]
    public DroneCommandes commandes;
    public MenuPause menuPause;

    public int Score { get; private set; }
    public float TempsRestant { get; private set; }
    public bool Terminee { get; private set; }
    public int TotalRecoltes { get; private set; }
    public IReadOnlyList<Plante> Plantes => plantes;

    readonly List<Plante> plantes = new List<Plante>();
    readonly Dictionary<TypePlante, int> recoltesParType = new Dictionary<TypePlante, int>();

    void Awake()
    {
        TempsRestant = duree;
    }

    void Update()
    {
        if (Terminee) return;

        // Échap / Start ouvre ou ferme le menu pause
        if (commandes != null && commandes.MenuPresse && menuPause != null) menuPause.Basculer();

        TempsRestant -= Time.deltaTime;
        if (TempsRestant <= 0f) Terminer();
    }

    // les plantes s'inscrivent à leur naissance et se retirent quand elles disparaissent
    public void AjouterPlante(Plante plante)
    {
        if (!plantes.Contains(plante)) plantes.Add(plante);
    }

    public void RetirerPlante(Plante plante) => plantes.Remove(plante);

    // appelé par le drone quand il récolte un légume mûr
    public void EnregistrerRecolte(TypePlante type, Vector3 position)
    {
        if (Terminee || type == null) return;
        recoltesParType.TryGetValue(type, out int nombre);
        recoltesParType[type] = nombre + 1;
        TotalRecoltes++;
        AjouterPoints(type.points, position);
        QuandRecolte?.Invoke(type, position);
    }

    // appelé par une plante trop mûre qui éclate
    public void SignalerExplosion(TypePlante type, Vector3 position)
    {
        if (Terminee || type == null) return;
        AjouterPoints(-type.penaliteExplosion, position);
        QuandExplosion?.Invoke(type, position);
    }

    public int NombreRecoltes(TypePlante type) => type != null && recoltesParType.TryGetValue(type, out int nombre) ? nombre : 0;

    // le score ne descend pas sous 0
    void AjouterPoints(int points, Vector3 position)
    {
        Score = Mathf.Max(0, Score + points);
        QuandPoints?.Invoke(points, position);
    }

    // fin du chrono : on sauvegarde et on passe à l'écran de fin
    void Terminer()
    {
        Terminee = true;
        TempsRestant = 0f;
        Sauvegarde.EnregistrerPartie(Score, TotalRecoltes);
        Invoke(nameof(ChargerFin), delaiAvantFin);
    }

    void ChargerFin() => SceneManager.LoadScene(sceneFin);
}
