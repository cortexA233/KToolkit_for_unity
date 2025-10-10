using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using KToolkit;


// public abstract class BaseState
// {
//     public abstract void EnterState();
//     public abstract void HandleFixedUpdate();
//     public abstract void HandleUpdate();
//     public abstract void ExitState();
//     public abstract void HandleCollide2D(Collision2D collision);
//     public abstract void HandleTrigger2D(Collider2D collider);
//     // public abstract void HandleCollide(Collision collision);
//     // public abstract void HandleTrigger(Collider collider);
// }
//
//
// public abstract class BaseFSM : KObserverNoMono
// {
//     public BaseState currentState { protected set; get; }
//     // protected StateMachineEventKObserver eventKObserver = new StateMachineEventKObserver();
//
//     public void TransitState(BaseState newState)
//     {
//         currentState.ExitState();
//         currentState = null;
//         currentState = newState;
//     }
// }
namespace KToolkit
{
   
    public interface KIBaseState<in TOwner>
    {
        public void EnterState(TOwner owner, params object[] args);
        public void HandleFixedUpdate(TOwner owner);
        public void HandleUpdate(TOwner owner);
        public void ExitState(TOwner owner);
        public void HandleCollide2D(TOwner owner, Collision2D collision);
        public void HandleTrigger2D(TOwner owner, Collider2D collider);
    }


    public class KStateMachine<TOwner> : KObserverNoMono where TOwner : MonoBehaviour
    {
        public TOwner owner { protected set; get; }
        public KIBaseState<TOwner> currentState { protected set; get; }

        public KStateMachine(TOwner owner, KIBaseState<TOwner> initialState, params object[] args)
        {
            this.owner = owner;
            TransitState(initialState, args);
        }
        public void TransitState(KIBaseState<TOwner> newState, params object[] args)
        {
            if (currentState != null) 
            {
                currentState.ExitState(owner);
                currentState = null;
            }
            currentState = newState;
            newState.EnterState(owner, args);
        }
    } 
}
