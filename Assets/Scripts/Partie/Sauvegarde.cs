using UnityEngine;

// Sauvegarde du meilleur score et de la dernière partie avec PlayerPrefs
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
