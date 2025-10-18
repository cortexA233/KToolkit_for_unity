using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using KToolkit;


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
        public TOwner owner { private set; get; }
        public KIBaseState<TOwner> currentState { private set; get; }

        public KStateMachine(TOwner owner, KIBaseState<TOwner> initialState, params object[] args)
        {
            this.owner = owner;
            currentState = initialState;
            currentState.EnterState(owner, args);
        }

        public void TransitState<TState>(params object[] args) where TState : KIBaseState<TOwner>, new()
        {
            if (currentState != null) 
            {
                currentState.ExitState(owner);
                currentState = null;
            }
            currentState = new TState();
            currentState.EnterState(owner, args);
        }
    } 
}
