using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Junkyard : MonoBehaviour, IDice
{



    int[] itemsTofind = new int[5];

    #region Metody publiczne

    public int Search(int option)
    {

        itemsTofind = GenerateItemsToFind();


        return itemsTofind[option];
    }

    public int itemsToFindLength() => itemsTofind.Length;

    #endregion

    #region Metody prywatne



    int[] GenerateItemsToFind()
    {      

        for (int i = 0; i < itemsTofind.Length; i++)
        {
            itemsTofind[i] = DiceRoll(1, SimulationMenager.Instance.maxItemValue);
        }

        Array.Sort(itemsTofind);

        return itemsTofind;

    }

    public bool IsPredatorThere()
    {
        bool isPredatorThere = false;

        if (DiceRoll() < SimulationMenager.Instance.predatorChance + SimulationMenager.Instance.predatorChanceBonus) isPredatorThere = true;

        return isPredatorThere;
    }

    public int DiceRoll() => UnityEngine.Random.Range(1, 100);
    public int DiceRoll(int min, int max) => UnityEngine.Random.Range(min, max);
    #endregion


}
