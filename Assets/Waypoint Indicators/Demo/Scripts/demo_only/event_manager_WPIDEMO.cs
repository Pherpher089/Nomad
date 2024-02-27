using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class event_manager_WPIDEMO : MonoBehaviour
{
    //Delegates that take no arguements
    public delegate void MyVoidGameDelegate(); //Define method signature
    public static MyVoidGameDelegate onStartLevel;
    public static MyVoidGameDelegate onResetToTitleScreen;
    public static MyVoidGameDelegate onOpenOptionScreen;
    public static MyVoidGameDelegate onCloseOptionScreen;
    public static MyVoidGameDelegate onCloseOptionScreenWithX;



    //Delegates that take ints
    public delegate void MyIntDelegate(int intAmnt); //Define method signature
    public static MyIntDelegate onLoadScene;



    //Delegates that take floats
    public delegate void MyFloatDelegate(float fltAmnt); //Define method signature
    public static MyFloatDelegate onDamagePlayerShield;

    //Delegates that take bools
    public delegate void MyBoolDelegate(bool state); //Define method signature
    public static MyBoolDelegate onPlayerMoving;

    //Delegates that take strings
    public delegate void MyStringDelegate(string message); //Define method signature
    public static MyStringDelegate onShowAlertBig;



    // VOID DELEGATES -------------------------------------

    public static void StartLevel()
    {
        if (onStartLevel != null)
        {
            onStartLevel();
        }
    }

    public static void ResetToTitleScreen()
    {
        if (onResetToTitleScreen != null)
        {
            onResetToTitleScreen();
        }
    }

    public static void OpenOptionScreen()
    {
        if (onOpenOptionScreen != null)
        {
            onOpenOptionScreen();
        }
    }

    public static void CloseOptionScreen()
    {
        if (onCloseOptionScreen != null)
        {
            onCloseOptionScreen();
        }
    }

    
    public static void CloseOptionScreenWithX()
    {
        if (onCloseOptionScreenWithX != null)
        {
            onCloseOptionScreenWithX();
        }
    }


    // INT DELEGATES -------------------------------------

    //Fire Score Points delegate event
    public static void LoadScene(int sceneNum)
    {
        if (onLoadScene != null)
        {
            onLoadScene(sceneNum);
        }
    }




    // FLOAT DELEGATES -------------------------------------

    //Fire Damage Shield delegate event
    public static void DamagePlayerShield(float dmgAmount)
    {
        if (onDamagePlayerShield != null)
        {
            onDamagePlayerShield(dmgAmount);
        }
    }




    // BOOL DELEGATES -------------------------------------

    //Fire BOOL delegate event
    public static void PlayerMoving(bool state)
    {
        if (onPlayerMoving != null)
        {
            onPlayerMoving(state);
        }
    }



    // STRING DELEGATES -------------------------------------

    //Fire STRING delegate event
    public static void ShowAlertBig(string message) //(Animated Floating Up Message)
    {
        if (onShowAlertBig != null)
        {
            onShowAlertBig(message);
        }
    }


}
