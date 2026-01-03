using System;
using System.Collections;
using System.Collections.Generic;
using KToolkit;
using UnityEngine;

/// <summary>
/// Use example of tick system
/// </summary>
public class Ticktest : MonoBehaviour, IKTickable
{
    private void Awake()
    {

    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        if (KTickManager.instance != null)
            KTickManager.instance.Unregister(this);
    }

    public void OnTick(KTickContext context)
    {
        Debug.Log("Ticktest ticked " + Time.time);
        Debug.Log(
            $"Ticktest ticked | tick #{context.tickCount} | dt={context.tickDeltaTime:F3} | elapsed={context.elapsedTime:F3}");
    }
    

    // Start is called before the first frame update
    void Start()
    {
        if (KTickManager.instance != null)
            KTickManager.instance.Register(this);
        else
        {
            Debug.Log("no tickmanager instance");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
