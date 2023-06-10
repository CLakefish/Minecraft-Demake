using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractObj
{
    [SerializeField] GameObject canvas;
    PlayerMovement p;
    bool isOpen = true;

    public void onInteract()
    {
        canvas.SetActive(isOpen);

        isOpen = !isOpen;
        //throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        onInteract();

        p = FindObjectOfType<PlayerMovement>();
    }
}
