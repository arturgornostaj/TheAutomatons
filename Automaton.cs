using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Junkyard))]


public class Automaton: Factory //NOTE Wzorzec maszyna stanów. Przenieść na maszyne stanów opartą o klasy celem późniejszej rozbuowy
{

    #region Zmienne

    public int aggresiveGene { get; private set; }
    public int bestGene { get; private set; }
    public int itemFound { get; private set; }
    public int fightSkill { get; private set; }
    public int livesCount { get; private set; }

    GameObject[] target; 
    GameObject[] factory;

    Junkyard junkyard;
    Rigidbody rb;

    /*WaitForSignal waitForSignal = new WaitForSignal();
    MoveToJunkyard moveToJunkyard = new MoveToJunkyard();
    MoveToFactory moveToFactory = new MoveToFactory();
    Fight fight = new Fight();
    Loot loot = new Loot();
    Scav scav = new Scav();*/

    public int searchCount { get; private set; }
    float dist { get;  set; } 
    [SerializeField] int currentMode { get; set; }
    public string myName { get; private set; }
    bool pointIsReached { get; set; }
    int randomTarget { get; set; }
    public float speed;
    public float movementNoiseX { get; set; }
    public float movementNoiseZ { get; set; }

    public bool TimeToScav
    {
        get { return timeToScav; }
    }

  

    //IAutomatonState automatonState;


    #endregion

    #region Metody Silnika Unity

    private void Awake()
    {
        aggresiveGene = aggresiveGenePool;
        bestGene = bestGenePool;
        fightSkill = SimulationMenager.Instance.fightSkill;

    }


    void Start()
    {
        Initialize();
       // automatonState = waitForSignal;
    }


    void Update()
    {
        
        GeneValueControll();
        Proceed();
        //automatonState = automatonState.DoState(this);



    }

    #endregion

    #region Metody sterujące pracą Automatona

    void Initialize()
    {

        Physics.IgnoreLayerCollision(8, 8);
        rb = GetComponent<Rigidbody>();
        junkyard = GetComponent<Junkyard>();
        Names name = GetComponent<Names>();
        myName = name.ChosenName();
        rb.isKinematic = true;

        target = GameObject.FindGameObjectsWithTag("Target");
        factory = GameObject.FindGameObjectsWithTag("Ramp");

        itemFound = 0;
        livesCount = 0;
        randomTarget = -1;
        speed = 5f + Random.Range(-1f,1f);
        currentMode = 1;
    }

    void Proceed()
    {


        switch (currentMode)
        {
            
            case 1: //NOTE Czekaj na sygnał

                if (TimeToScav) currentMode = 2;

                break;


            case 2: //NOTE Odebranie fantów

                searchCount = 0;
                itemFound = 0;
                currentMode = 3;
                Noise();

                break;

            case 3: //NOTE Wybór celu i ruch do niego

                MoveTo(target, 4);

                break;

            case 4: //NOTE Losowanie niebezpieczeństw

                if (junkyard.IsPredatorThere())
                {
                    if (DiceRoll() <= aggresiveGene) currentMode = 5; //Wykryto wroga. Uciekać czy walczyć
                    else currentMode = 7;
                }
                else
                {
                    currentMode = 6; //Jest bezpiecznie ( na razie )
                }


                break;

            case 5: //NOTE Walka

                if (FightResult()) currentMode = 6;
                else AutomatonIsDestroyed();

                break;

            case 6: //NOTE Przeszukiwanie

                itemFound = junkyard.Search(searchCount);

                if (DiceRoll() <= bestGene && itemFound < (SimulationMenager.Instance.maxItemValue * 0.85) && searchCount < junkyard.itemsToFindLength() - 1)
                {
                    searchCount++;
                    bestGene += SimulationMenager.Instance.geneIncrease;
                    currentMode = 3;
                }
                else currentMode = 7;

                break;

            case 7: //NOTE Czyszczenie i wprowadzanie szumu ruchu

                Noise();
                Lives();
                Ageing();
                currentMode = 8;

                break;

            case 8: //NOTE Wróć do bazy

                MoveTo(factory, 1);

                break;

        }
    }

        #endregion

    #region Metody pomocnicze (ruch, kontrola automatona)

        void MoveTo(GameObject[] poolOfTargets, int modeAfterPointIsReached)
        {

            if (randomTarget == -1)
            {
                randomTarget = Random.Range(0, poolOfTargets.Length);
            }

            Vector3 currentTarget;
            Vector3 noise = new Vector3(movementNoiseX, 0, movementNoiseZ);
           
            dist = Vector3.Distance(poolOfTargets[randomTarget].transform.position + noise, transform.position);

            currentTarget = poolOfTargets[randomTarget].transform.position;
            transform.LookAt(currentTarget);
            transform.position = Vector3.MoveTowards(transform.position, currentTarget + noise, Time.deltaTime * speed);


            if (dist < 0.5)
            {
                randomTarget = -1;
                currentMode = modeAfterPointIsReached;
            }
        }

        void Lives()
        {
  
            livesCount++;
            if (livesCount >= SimulationMenager.Instance.lives)
            {
                Debug.Log("Jednostaka zużyła całą energię");
                AutomatonIsDestroyed();
            }
        }

        void Noise()
        {
            int minOffset = -70;        //NOTE  Nie większy niż ustawiony w maksymalnym dystansie w metodzie Move()
            int maxOffset = 70;

            movementNoiseX = Random.Range(minOffset, maxOffset);
            movementNoiseX = movementNoiseX / 100;
            movementNoiseZ = Random.Range(minOffset, maxOffset);
            movementNoiseZ = movementNoiseZ / 100;

        }

        void GeneValueControll()
        {
            if (aggresiveGene >= 99) aggresiveGene = 99;
            if (aggresiveGene < 1) aggresiveGene = 1;
            if (bestGene >= 99) bestGene = 99;
            if (bestGene < 1) bestGene = 1;
    }

        #endregion

    #region Metody walki

        void AutomatonIsDestroyed()
        {
            Debug.Log("Straciliśmy jedną jednostkę");
            expectedUnitsInFactory--;
            Destroy(gameObject);

        }

        bool FightResult()
        {
                Debug.Log("Doszło do walki, automaton zwyciężył");

                bool won;

                aggresiveGene += SimulationMenager.Instance.geneIncrease;
                fightSkill += SimulationMenager.Instance.fightSkillIncrease;

                if (DiceRoll() <= SimulationMenager.Instance.fightSkill) won = true;
                else won = false;

                return won;
        }

        void Ageing()
        {
        aggresiveGene--;
        bestGene--;
        fightSkill--;
        }


    #endregion


}
