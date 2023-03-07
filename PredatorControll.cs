using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorControll : MonoBehaviour
{

   
    int maxPopulation;
    bool risePredatorPopulation;
    int automatons;
    int currentRound;
    int creationRound = 0;


    private void Start()
    {
        currentRound = SimulationMenager.Instance.round;
        risePredatorPopulation = SimulationMenager.Instance.risePredatorPopulation;
    }

    private void Update()
    {
        SimulationMenager.Instance.risePredatorPopulation = risePredatorPopulation;
        currentRound = SimulationMenager.Instance.round;

        PredatorMaxCountControll();
        PopulationControll();

    }

    


    void PopulationControll()
    {
        automatons = GameObject.FindGameObjectsWithTag("Automaton").Length;
        maxPopulation = SimulationMenager.Instance.maxAutomatonCount;

        if (automatons >= maxPopulation * 0.75) risePredatorPopulation = true;
        if (automatons <= maxPopulation * 0.6 && currentRound != 0) risePredatorPopulation = false;

        if(risePredatorPopulation && creationRound == currentRound)
        {
            Debug.Log("Populacja łowców wzrasta");
            SimulationMenager.Instance.predatorChance += automatons /5 ;
            creationRound++;
        }

        if (!risePredatorPopulation && creationRound == currentRound)
        {
            Debug.Log("Populacja łowców spada");
            SimulationMenager.Instance.predatorChance -= (automatons / 10) * 3;
            creationRound++;
        }

    }

    void PredatorMaxCountControll()
    {
        if(SimulationMenager.Instance.predatorChance + SimulationMenager.Instance.predatorChanceBonus > 100)
        {
            SimulationMenager.Instance.predatorChance--;
            
        }
        if (SimulationMenager.Instance.predatorChance < 30 )
        {
            SimulationMenager.Instance.predatorChance++;
          
        }
    }


}
