using UnityEngine;
using System.Collections;


[CreateAssetMenu(menuName = "PluggableAI/State")]
public class State : ScriptableObject
{

    public Action[] actions;
    public Transition[] transitions;
    public Color SceneGizmoColor = Color.grey;
  


    public void UpdateState(StateController controller)
    {
        DoActions(controller);
        CheckTrasitions(controller);
    }
    
    public void DoActions(StateController controller)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(controller);
        }
    }

    private void CheckTrasitions(StateController controller)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            bool transitionSucceeded = transitions[i].decision.Decide(controller);

            if(transitionSucceeded)
            {
                controller.TransitionToState(transitions[i].trueState);
            }
            else
            {
                controller.TransitionToState(transitions[i].falseState);
            }
        }
    }
}
