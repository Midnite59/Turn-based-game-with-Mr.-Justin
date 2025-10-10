using UnityEngine;

[RequireComponent (typeof(Collider))]
[RequireComponent(typeof(EncountersOhNo))]
public class EncounterTrigger : MonoBehaviour
{
    Collider collider;
    EncountersOhNo encounter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        collider = GetComponent<Collider>();
        encounter = GetComponent<EncountersOhNo>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<OverworldMovement>() != null) 
        {
            GameManager.instance.StartBattle(encounter);
        }
    }
}
