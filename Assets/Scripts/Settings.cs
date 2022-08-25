using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Settings
{
    [FormerlySerializedAs("Version")] [Header("Game Data")]
    public string version = "0.0.01";

    [FormerlySerializedAs("LoadDistance")] [Header("Performance")]
    public int loadDistance = 3;
    [FormerlySerializedAs("ViewDistanceInChunks")] public int viewDistanceInChunks = 3;
    public CloudStyle cloudStyle = CloudStyle.Fast;

    [FormerlySerializedAs("MouseSensitivity")]
    [Header ("Controls")]
    [Range (0.5f, 3.5f)]
    public float mouseSensitivity = 2f;

    //[Header ("World Generation Values")]
    //public int Seed = 20161991;

}