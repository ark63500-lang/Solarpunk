using System;
using UnityEngine;
using UnityEngine.InputSystem;

// lit les actions de l'input system (clavier + manette)
// chaque action est une référence glissée dans l'Inspector, aucun nom d'action écrit en dur
[DefaultExecutionOrder(-100)]
public class DroneCommandes : MonoBehaviour
{
    [Header("Actions (DroneControls)")]
    public InputActionReference deplacer;
    public InputActionReference lacher;
    public InputActionReference aspirer;
    public InputActionReference arroser;
    public InputActionReference menu;
    public InputActionReference graine1;
    public InputActionReference graine2;
    public InputActionReference graine3;
    public InputActionReference graineSuivante;
    public InputActionReference grainePrecedente;

    public bool ManetteActive { get; private set; }
    public event Action<bool> QuandPeripheriqueChange;

    InputActionMap groupeActions;
    bool jeuBloque;

    public Vector2 Deplacement => Actif(deplacer) ? Vector2.ClampMagnitude(deplacer.action.ReadValue<Vector2>(), 1f) : Vector2.zero;
    public bool LacherPresse => Actif(lacher) && lacher.action.WasPressedThisFrame();
    public bool AspirerMaintenu => Actif(aspirer) && aspirer.action.IsPressed();
    public bool ArroserMaintenu => Actif(arroser) && arroser.action.IsPressed();
    public bool MenuPresse => menu != null && menu.action.WasPressedThisFrame();

    public int ChangementGraine
    {
        get
        {
            int changement = 0;
            if (Actif(graineSuivante) && graineSuivante.action.WasPressedThisFrame()) changement++;
            if (Actif(grainePrecedente) && grainePrecedente.action.WasPressedThisFrame()) changement--;
            return changement;
        }
    }

    public int GraineDirecte
    {
        get
        {
            if (Actif(graine1) && graine1.action.WasPressedThisFrame()) return 0;
            if (Actif(graine2) && graine2.action.WasPressedThisFrame()) return 1;
            if (Actif(graine3) && graine3.action.WasPressedThisFrame()) return 2;
            return -1;
        }
    }

    // pendant la pause, seule l'action Menu reste écoutée
    public void BloquerJeu(bool bloque) => jeuBloque = bloque;

    bool Actif(InputActionReference reference) => reference != null && !jeuBloque;

    void Awake()
    {
        if (deplacer == null)
        {
            Debug.LogError("DroneCommandes : actions non assignées dans l'Inspector", this);
            enabled = false;
            return;
        }
        groupeActions = deplacer.action.actionMap;
        ManetteActive = Gamepad.current != null;
    }

    void OnEnable()
    {
        if (groupeActions == null) return;
        groupeActions.actionTriggered += SurAction;
        groupeActions.Enable();
    }

    void OnDisable()
    {
        if (groupeActions == null) return;
        groupeActions.actionTriggered -= SurAction;
        groupeActions.Disable();
    }

    // pour savoir si le joueur est sur manette ou clavier (aide à l'écran)
    void SurAction(InputAction.CallbackContext contexte)
    {
        if (contexte.control == null) return;
        bool manette = contexte.control.device is Gamepad;
        if (manette == ManetteActive) return;
        ManetteActive = manette;
        QuandPeripheriqueChange?.Invoke(manette);
    }
}
