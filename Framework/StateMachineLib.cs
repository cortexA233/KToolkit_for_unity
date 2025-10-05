using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using KToolkit;


public abstract class BaseState
{
    public abstract void EnterState();
    public abstract void HandleFixedUpdate();
    public abstract void HandleUpdate();
    public abstract void ExitState();
    public abstract void HandleCollide2D(Collision2D collision);
    public abstract void HandleTrigger2D(Collider2D collider);
    // public abstract void HandleCollide(Collision collision);
    // public abstract void HandleTrigger(Collider collider);
}


public abstract class BaseFSM : KObserverNoMono
{
    public BaseState currentState { protected set; get; }
    // protected StateMachineEventKObserver eventKObserver = new StateMachineEventKObserver();

    public void TransitState(BaseState newState)
    {
        currentState.ExitState();
        currentState = null;
        currentState = newState;
    }
}


public abstract class KBaseState<TOwner> where TOwner : MonoBehaviour
{
    public TOwner owner;
    public KBaseState(TOwner owner)
    {
        this.owner = owner;
    }
    public abstract void EnterState();
    public abstract void HandleFixedUpdate();
    public abstract void HandleUpdate();
    public abstract void ExitState();
    public abstract void HandleCollide2D(Collision2D collision);
    public abstract void HandleTrigger2D(Collider2D collider);
}


public abstract class KBaseFSM<TOwner> : KObserverNoMono where TOwner : MonoBehaviour
{
    public TOwner owner { protected set; get; }
    public KBaseState<TOwner> currentState { protected set; get; }
    public void TransitState(KBaseState<TOwner> newState)
    {
        currentState.ExitState();
        currentState = null;
        currentState = newState;
    }
}


// public class StateMachineEventKObserver : KObserverNoMono
// {
//     public 
// }

