using UnityEngine;

// boîte englobante du drone, sert à savoir où est le bas du drone
public class DroneCorps : MonoBehaviour
{
    Renderer[] rendus;

    public Bounds Limites
    {
        get
        {
            if (rendus == null || rendus.Length == 0) rendus = GetComponentsInChildren<Renderer>();

            Bounds limites = new Bounds(transform.position, Vector3.zero);
            bool premier = true;
            foreach (var rendu in rendus)
            {
                if (rendu == null || rendu is ParticleSystemRenderer || rendu is TrailRenderer) continue;
                if (premier)
                {
                    limites = rendu.bounds;
                    premier = false;
                }
                else limites.Encapsulate(rendu.bounds);
            }
            return limites;
        }
    }

    public Vector3 Centre => Limites.center;
    public float Bas => Limites.min.y;
}
