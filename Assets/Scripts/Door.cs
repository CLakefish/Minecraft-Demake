using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractObj
{
    public void onInteract();
}

public class Door : MonoBehaviour, IInteractObj
{
    [SerializeField] GameObject openDoor;
    [SerializeField] GameObject closedDoor;
    bool isOpen = true;

    public void onInteract()
    {
        isOpen = !isOpen;

        openDoor.SetActive(isOpen);
        closedDoor.SetActive(!isOpen);
    }

    // Start is called before the first frame update
    void Start()
    {
        onInteract();
    }
}
