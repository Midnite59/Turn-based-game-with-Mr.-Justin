using System.Collections.Generic;
using UnityEngine;

public class Interacter : MonoBehaviour
{
    public List<IInteractable> interactablesNearby;
    public int interactablesCount;
    public new Collider collider;
    int ind = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactablesNearby = new List<IInteractable>();
    }

    // Update is called once per frame
    void Update()
    {
        interactablesCount = interactablesNearby.Count;
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            interactablesNearby[ind].Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable iinteractable = other.GetComponent<IInteractable>() ?? other.GetComponentInChildren<IInteractable>() ?? other.GetComponentInParent<IInteractable>();
        if (iinteractable != null) 
        {
            if (!interactablesNearby.Contains(iinteractable) && iinteractable.IsActive()) 
            {
                interactablesNearby.Add(iinteractable);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        IInteractable iinteractable = other.GetComponent<IInteractable>() ?? other.GetComponentInChildren<IInteractable>() ?? other.GetComponentInParent<IInteractable>();
        if (iinteractable != null)
        {
            if (interactablesNearby.Contains(iinteractable) && !iinteractable.IsActive())
            {
                interactablesNearby.Remove(iinteractable);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        IInteractable iinteractable = other.GetComponent<IInteractable>() ?? other.GetComponentInChildren<IInteractable>() ?? other.GetComponentInParent<IInteractable>();
        if (iinteractable != null)
        {
            if (interactablesNearby.Contains(iinteractable))
            {
                interactablesNearby.Remove(iinteractable);
            }
        }
    }
}
