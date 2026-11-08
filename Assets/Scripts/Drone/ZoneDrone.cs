using System.Collections.Generic;
using UnityEngine;

// zone de détection (Trigger) sous le drone : garde la liste des plantes qui sont dessous
[RequireComponent(typeof(Rigidbody))]
public class ZoneDrone : MonoBehaviour
{
    readonly HashSet<Plante> plantes = new HashSet<Plante>();
    readonly List<Plante> tampon = new List<Plante>();

    void OnTriggerEnter(Collider autre)
    {
        var plante = autre.GetComponentInParent<Plante>();
        if (plante != null) plantes.Add(plante);
    }

    void OnTriggerExit(Collider autre)
    {
        var plante = autre.GetComponentInParent<Plante>();
        if (plante != null) plantes.Remove(plante);
    }

    // renvoie toujours la même liste pour éviter des allocations à chaque image
    public List<Plante> Plantes()
    {
        tampon.Clear();
        foreach (var plante in plantes)
            if (plante != null) tampon.Add(plante);
        if (tampon.Count != plantes.Count) plantes.RemoveWhere(plante => plante == null);
        return tampon;
    }
}
