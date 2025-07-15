using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtAttractor : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Attractor.POS);
    }
}
