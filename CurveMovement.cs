using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveMovement : MonoBehaviour
{
    [SerializeField] Transform[] rutes;

    int choosenRute;
    float tParam;
    Vector3 objectPosition;
    public float speed = .5f;
    public bool loop = false;
    public bool lookAtMouse = false;
    bool corutineAllowed;
    


    private void Start()
    {
        choosenRute = 0;
        tParam = 0f;
        corutineAllowed = true;
    }

    private void Update()
    {
    
            if (corutineAllowed && choosenRute < rutes.Length) StartCoroutine(GoByTheRute(choosenRute));

        Vector3 mousePos = Input.mousePosition;

        if(lookAtMouse) transform.LookAt(mousePos);

    }

    private IEnumerator GoByTheRute(int ruteNumber)
    {
        corutineAllowed = false;

        Vector3 p0 = rutes[ruteNumber].GetChild(0).position;
        Vector3 p1 = rutes[ruteNumber].GetChild(1).position;
        Vector3 p2 = rutes[ruteNumber].GetChild(2).position;
        Vector3 p3 = rutes[ruteNumber].GetChild(3).position;

        while (tParam < 1) 
        {
            tParam += Time.deltaTime * speed;

            objectPosition = Mathf.Pow(1 - tParam, 3) * p0 +
                3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 +
                 3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 +
                 Mathf.Pow(tParam, 3) * p3;

            transform.position = objectPosition;
            yield return new WaitForEndOfFrame();
        
        }

        tParam = 0f;
        choosenRute++;

        if (loop && choosenRute > rutes.Length - 1) choosenRute = 0;


        corutineAllowed = true;
    }
}
