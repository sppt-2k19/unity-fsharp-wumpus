using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WumpusFS;

public class fscomp_agent : MonoBehaviour
{
    private Agent agent;
    
    void Awake()
    {
        agent = new Agent(gameObject);
        
    }
    
    void Start()
    {
        //agent.Start();
    }
    
    void Update()
    {
        agent.Update();
        
    }
}
