using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class ErrorManager : MonoBehaviour
{
    public static ErrorManager Instance;
    public string LastDisconnectError = "";

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicate instances
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string GetDisconnectMessage(DisconnectCause cause)
    {
        switch (cause)
        {
            case DisconnectCause.None:
                return "No connection issues detected.";

            case DisconnectCause.ExceptionOnConnect:
                return "Exception On Connect: Could not connect to the game. Please check your internet and try again.";

            case DisconnectCause.DnsExceptionOnConnect:
                return "Dns Exception On Connect: Connection issue detected. Try restarting your internet or checking your network settings.";

            case DisconnectCause.ServerAddressInvalid:
                return "Server Address Invalid: Something went wrong while connecting. Please try again later.";

            case DisconnectCause.Exception:
                return "Exception: An unexpected error occurred. Please restart the game and try again.";

            case DisconnectCause.ServerTimeout:
                return "Server Timeout: Lost connection to the server. Please check your internet or try again later.";

            case DisconnectCause.ClientTimeout:
                return "Client Timeout: Your connection to the game was lost. Try reconnecting.";

            case DisconnectCause.DisconnectByServerLogic:
                return "Disconnect By Server Logic: You have been removed from the game. If this was unexpected, try rejoining.";

            case DisconnectCause.DisconnectByServerReasonUnknown:
                return "Disconnect By Server ReasonUnknown: Disconnected from the server. Please try again.";

            case DisconnectCause.InvalidAuthentication:
                return "Invalid Authentication: Unable to join the game. Please restart and try again.";

            case DisconnectCause.CustomAuthenticationFailed:
                return "Custom Authentication Failed: Could not sign in. Please try again later.";

            case DisconnectCause.AuthenticationTicketExpired:
                return "Authentication Ticket Expired: Session expired. Please restart the game.";

            case DisconnectCause.MaxCcuReached:
                return "Max Ccu Reached: The game is full. Try again later.";

            case DisconnectCause.InvalidRegion:
                return "Invalid Region: You are unable to connect. Try switching to a different server region.";

            case DisconnectCause.OperationNotAllowedInCurrentState:
                return "Operation Not Allowed In Current State: Action not allowed right now. Try again in a moment.";

            case DisconnectCause.DisconnectByClientLogic:
                return "Disconnect By Client Logic: Disconnected from the game. Try reconnecting.";

            case DisconnectCause.DisconnectByOperationLimit:
                return "Disconnect By Operation Limit: You were disconnected for sending too many requests. Try again in a moment.";

            case DisconnectCause.DisconnectByDisconnectMessage:
                return "Disconnect By Disconnect Message: The game has ended or you were removed from the session.";

            case DisconnectCause.ApplicationQuit:
                return "Application Quit: You left the game.";

            default:
                return "Disconnected from the game. Please try again.";
        }
    }

}
