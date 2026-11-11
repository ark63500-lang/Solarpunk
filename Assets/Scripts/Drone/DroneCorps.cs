// DroneCorps : calcule la boîte englobante du drone.
// Sert aux autres scripts pour savoir où est le centre et le bas du drone.

using UnityEngine;

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
