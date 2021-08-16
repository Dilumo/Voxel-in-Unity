using System.IO;
using UnityEngine;

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string Version = "0.0.01";

    [Header("Performace")]
    public int LoadDistance = 3;
    public int ViewDistanceInChunks = 3;
    public CloudStyle cloudStyle = CloudStyle.Fast;

    [Header ("Controls")]
    [Range (0.5f, 3.5f)]
    public float MouseSensitivity = 2f;

    //[Header ("World Generation Values")]
    //public int Seed = 20161991;

}