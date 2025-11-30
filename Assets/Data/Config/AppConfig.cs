using UnityEngine;

[CreateAssetMenu(fileName = "AppConfig", menuName = "Amethral/AppConfig")]
public class AppConfig : ScriptableObject
{
    [Header("API Settings")]
    public string DevelopmentApiUrl = "http://localhost:5298/api/auth";
    public string ProductionApiUrl = "https://api.amethral.com/api/auth";
    
    [Header("Website Settings")] 
    // Add the URL where users register (Localhost for dev, Real site for prod)
    public string DevelopmentRegisterUrl = "http://localhost:3000/register"; 
    public string ProductionRegisterUrl = "https://amethral.com/register";

    [Header("Game Server Settings")]
    public bool IsProductionBuild = false;

    /// <summary>
    /// Returns the API URL based on Build Mode or Command Line Args
    /// </summary>
    public string GetApiUrl()
    {
        // 1. Priorité aux Arguments de ligne de commande
        string cmdLineUrl = GetArg("-apiurl");
        if (!string.IsNullOrEmpty(cmdLineUrl)) return cmdLineUrl;

        // 2. Sinon config standard
        return IsProductionBuild ? ProductionApiUrl : DevelopmentApiUrl;
    }

    /// <summary>
    /// Returns the Registration URL based on Build Mode
    /// </summary>
    public string GetRegisterUrl()
    {
        return IsProductionBuild ? ProductionRegisterUrl : DevelopmentRegisterUrl;
    }

    // Helper pour lire les arguments de lancement
    private string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}