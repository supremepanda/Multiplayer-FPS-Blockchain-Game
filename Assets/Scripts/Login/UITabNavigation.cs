using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// This class just controlling navigation with keyboard "TAB" key. With this class, we can navigate the UI with TAB key.
/// </summary>
public class UITabNavigation : MonoBehaviour
{
    EventSystem system;

    void Start()
    {
        system = EventSystem.current;

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

            if (next != null)
            {

                InputField inputfield = next.GetComponent<InputField>();
                if (inputfield != null)
                    inputfield.OnPointerClick(new PointerEventData(system));

                system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
            }

        }
    }
}
