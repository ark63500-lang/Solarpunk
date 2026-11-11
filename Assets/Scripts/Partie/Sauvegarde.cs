// Sauvegarde : enregistre le meilleur score et la dernière partie avec PlayerPrefs.
// Les données restent sur l'ordinateur même après avoir fermé le jeu.

using UnityEngine;

public static class Sauvegarde
{
    const string CleMeilleur = "MeilleurScore";
    const string CleDernier = "DernierScore";
    const string CleRecoltes = "DernieresRecoltes";

    public static int MeilleurScore => PlayerPrefs.GetInt(CleMeilleur, 0);
    public static int DernierScore => PlayerPrefs.GetInt(CleDernier, 0);
    public static int DernieresRecoltes => PlayerPrefs.GetInt(CleRecoltes, 0);
    public static bool NouveauRecord { get; private set; }

    public static void EnregistrerPartie(int score, int recoltes)
    {
        NouveauRecord = score > MeilleurScore;
        if (NouveauRecord) PlayerPrefs.SetInt(CleMeilleur, score);
        PlayerPrefs.SetInt(CleDernier, score);
        PlayerPrefs.SetInt(CleRecoltes, recoltes);
        PlayerPrefs.Save();
    }
}
