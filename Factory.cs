using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Factory : MonoBehaviour, IDice //NOTE Wzorzec maszyna stanów. Przenieść na maszyne stanów opartą o klasy celem późniejszej rozbuowy
{
    #region zmienne

    public static int aggresiveGenePool { get; set; }  
    public static int bestGenePool { get; set; }


    public GameObject automaton;
    GameObject[] automatons;

    GameObject mainRamp;
    GameObject spawn;
   

    public static bool timeToScav { get; set; }
    public static int expectedUnitsInFactory = 0;
    public static int unitsInFactory = 0;
    

    [SerializeField] int currentModeF = 0;
    int automatonsToSpawn = 1;
    int[] allFoundItems = new int[0];

    #endregion

    #region Metody Silnika Unity
    
    void Update()
    {
        Proceed();
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Automaton"))
        {
            unitsInFactory++;
        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Automaton"))
        {
            unitsInFactory--;
        }
    }


    #endregion

    #region Metody sterujące pracą fabryki



    void Initialize( int delay , int automatonsCount)   //NOTE Pierwsze uruchomienie fabryki
    {
        
        timeToScav = false;
        spawn = GameObject.FindGameObjectWithTag("Respawn");
        mainRamp = GameObject.FindGameObjectWithTag("Ramp");
        automatons = GameObject.FindGameObjectsWithTag("Automaton");
        


        aggresiveGenePool = SimulationMenager.Instance.startAggresiveGene;
        bestGenePool = SimulationMenager.Instance.startBestGene;
              
        StartCoroutine(CreateAutomatonsWithDelay(delay, automatonsCount));

    }

    void Proceed()      //NOTE Fabryka pracuje
    {



        switch (currentModeF)
        {
            case 0:

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Symulacja wystartowała");
                    Initialize(2, SimulationMenager.Instance.firstAutomatonsCount);
                    currentModeF = 1;
                }
                break;


            case 1: //NOTE Sygnał do przeszukieawnia


                if ((unitsInFactory == expectedUnitsInFactory) && unitsInFactory != 0) StartCoroutine(FactoryCoolDown(2,2));

                    break;

            case 2: //NOTE Czekanie aż wszystkie automatony opuszczą bazę


                timeToScav = true;

                if (unitsInFactory == 0)
                {
                    
                    timeToScav = false;
                    currentModeF = 3;
                }
                break;

            case 3: //NOTE Czekanie aż automatony wrócą



                if ((unitsInFactory == expectedUnitsInFactory) && unitsInFactory != 0) currentModeF = 4;

                    break;

            case 4: //NOTE Sortowanie automatonów


                SortAutomatons();
                SimulationMenager.Instance.round++;
                currentModeF = 5;
                
                break;

            case 5: //NOTE Tworzenie nowych automatonów


                CreateAutomaton(automatonsToSpawn);
                
                currentModeF = 6;

                break;

            case 6:

                
                StartCoroutine(FactoryCoolDown(1, 1));

                break;
                   
        }   
    }

    #endregion

    #region Metody pomocnicze (kontrola)

    void CreateAutomaton(int count)
    {

        if(automatons.Length == 0)
        {
            for (int i = 0; i < count; i++)
            {

                InstantiateAutomaton();

            }

            Debug.Log("Utworzono " + count + " Automanotnów");
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < automatons.Length; j++)
                {
                    
                    if (allFoundItems[automatons.Length - i - 1] == automatons[j].GetComponent<Automaton>().itemFound)
                    {
                        
                        aggresiveGenePool = automatons[j].GetComponent<Automaton>().aggresiveGene + DiceRoll(-1,1);
                        bestGenePool = automatons[j].GetComponent<Automaton>().bestGene + DiceRoll(-1, 1); 

                        Debug.Log("Automaton nr: " + j + " zebrał: " + automatons[j].GetComponent<Automaton>().itemFound + " zarejestrowano: " + (allFoundItems[automatons.Length - i - 1]) + " dzieli geny: " + automatons[j].GetComponent<Automaton>().aggresiveGene + automatons[j].GetComponent<Automaton>().bestGene);

                        InstantiateAutomaton();

                        break;

                    }

                }
                    
              
            }

            Debug.Log("Utworzono " + count + " Automanotnów");
        }
  
        
    }

    void InstantiateAutomaton()
    {
        float randomFloat = UnityEngine.Random.Range(-70, 70);
        randomFloat = randomFloat / 100;
        expectedUnitsInFactory++;

        Vector3 randomPositon = new Vector3(randomFloat, 0, randomFloat);

        Instantiate(automaton, spawn.transform.position + randomPositon, spawn.transform.rotation);
    }


    void SortAutomatons()
    {


        Debug.ClearDeveloperConsole();
        Debug.Log("Sortowanie");
        automatons = GameObject.FindGameObjectsWithTag("Automaton");
        int itemsSum = 0;
        allFoundItems = new int[automatons.Length];
        

        for (int i = 0; i < automatons.Length; i++)
        {
            allFoundItems[i] = automatons[i].GetComponent<Automaton>().itemFound;
            itemsSum += automatons[i].GetComponent<Automaton>().itemFound;            
        }

        Array.Sort(allFoundItems);

        if (itemsSum / SimulationMenager.Instance.itemsToForgeAutomaton > automatons.Length) automatonsToSpawn = automatons.Length;
        else automatonsToSpawn = itemsSum / SimulationMenager.Instance.itemsToForgeAutomaton;



        Debug.Log("Mamy zasoby na utworzenie " + automatonsToSpawn + " Automatonów");
    
    }

    #endregion

    #region Interfejsy

    IEnumerator CreateAutomatonsWithDelay(int delay, int count)
    {

        yield return new WaitForSeconds(delay);
        CreateAutomaton(count);

    }

    IEnumerator DelayedSignalToScav(int delay)
    {
        Debug.Log("Za " + delay + "sek roboty wyruszą na poszukiwania");
        yield return new WaitForSeconds(delay);
        timeToScav = true;
        currentModeF = 4;
       
        
    }

    IEnumerator FactoryCoolDown(int delay, int nextModeF)
    {
        
        yield return new WaitForSeconds(delay);
        currentModeF = nextModeF;

    }

    public int DiceRoll() => UnityEngine.Random.Range(1, 100);

    public int DiceRoll(int min, int max) => UnityEngine.Random.Range(min, max);

    #endregion
}
