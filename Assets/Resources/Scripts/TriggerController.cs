using UnityEngine;
using System.Collections.Generic;

public class TriggerController : MonoBehaviour
{
    //тут прописывать все взаимодействия игрока с помощью тригеров и у меня стоит пробел как interact тут

    [SerializeField] private GameManager gameManager;

    [SerializeField] private List<Plant> CurrPlants = new();
    private bool NearBed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FieldCell"))
        {
            CurrPlants.Add(other.GetComponent<Plant>());
        }
        else if (other.CompareTag("Bed"))
        {
            NearBed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("FieldCell"))
        {
            CurrPlants.Remove(other.GetComponent<Plant>());
        }
        else if (other.CompareTag("Bed"))
        {
            NearBed = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CurrPlants.Count > 0)
            {
                if (CurrPlants[0].stage != 6)
                {
                    if (!CurrPlants[0].planted)
                    {
                        CurrPlants[0].PutDown();
                    }
                }
                else if (CurrPlants[0].stage == 6)
                {
                    CurrPlants[0].PickUp();
                }
            }

            if (NearBed)
            {
                gameManager.NextDay();
            }
        }
    }
}
