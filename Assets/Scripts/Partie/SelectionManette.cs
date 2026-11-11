// SelectionManette : resélectionne un bouton quand la souris l'a désélectionné.
// Écoute les actions du module UI de l'Input System : clavier et manette passent par le même code.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class SelectionManette : MonoBehaviour
{
    public Selectable parDefaut;

    void Update()
    {
        var systemeEvenements = EventSystem.current;
        if (systemeEvenements == null || parDefaut == null || systemeEvenements.currentSelectedGameObject != null) return;

        var moduleInterface = systemeEvenements.currentInputModule as InputSystemUIInputModule;
        if (moduleInterface == null) return;

        bool navigue = moduleInterface.move != null && moduleInterface.move.action.ReadValue<Vector2>().sqrMagnitude > 0.25f;
        bool valide = moduleInterface.submit != null && moduleInterface.submit.action.WasPressedThisFrame();

        if (navigue || valide) parDefaut.Select();
    }
}
