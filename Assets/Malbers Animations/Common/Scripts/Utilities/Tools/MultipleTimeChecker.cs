using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations.Utilities
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/utilities/multiple-time-checker")]
    [AddComponentMenu("Malbers/Utilities/Multiple Time Checker")]
    public class MultipleTimeChecker : MonoBehaviour
    {
        [Tooltip("Amount of taps/click/checks you need to ")]
        [Min(1)] public int MaxChecks = 2;
        [Min(0.1f)] public float interval = 0.3f;

        public int CurrentCheck { get; private set; }
        public float CurrentTime { get; private set; }

        public IntEvent CheckStep = new();
        public UnityEvent CheckSuccessful = new();


        public bool debug;



        public void Check()
        {
            //Means that is the first Tap
            if (CurrentTime != 0)
            {
                if (!MTools.ElapsedTime(CurrentTime, interval))
                {
                    //Check if we have made the multiple Clicks/Taps/checks
                    if (CurrentCheck == MaxChecks)
                    {
                        if (debug) Debug.Log("Max Checks Successful!");
                        CheckSuccessful.Invoke();
                        ResetCheck();
                    }
                    else
                    {
                        CheckAdd();
                    }
                }
                else //Is not in the interval so reset the Checker
                {
                    ResetCheck();
                    CheckAdd();
                }
            }
            else
            {
                CheckAdd();
            }
        }

        void ResetCheck()
        {
            CurrentCheck = 1;
            CurrentTime = 0;
        }

        void CheckAdd()
        {
            CurrentCheck++;
            if (debug) Debug.Log($"Check [{CurrentCheck}]");
            CurrentTime = Time.time;
            CheckStep.Invoke(CurrentCheck);
        }
    }
}
