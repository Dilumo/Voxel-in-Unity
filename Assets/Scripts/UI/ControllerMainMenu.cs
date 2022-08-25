using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using Data;

public class ControllerMainMenu : MonoBehaviour
{
    public GameObject pnlMainMenu;
    public GameObject pnlSettings;

    [Header("Main Menu")]
    public TextMeshProUGUI inpSeed;

    [Header("Settings")]
    public Slider sldViewDistance;
    public TextMeshProUGUI txtViewDistance;
    public Slider sldMouseSensitivity;
    public TextMeshProUGUI txtMouseSensitivity;
    public TMP_Dropdown dpdClouds;


    Settings settings = new Settings();

    #region MAIN MENU
    public void StartGame()
    {
        if (!string.IsNullOrEmpty(inpSeed.text))
        {
            VoxelData.seed = Mathf.Abs(inpSeed.text.GetHashCode()) / VoxelData.WorldSizeInChunks;
            SceneManager.LoadScene("VoxelTest", LoadSceneMode.Single);
        }
        else
            Debug.Log("<color=red>SEED..</color>");
    }

    public void GameSettings()
    {
        sldViewDistance.value = settings.viewDistanceInChunks;
        UpdateViewDistanceSlider();
        sldMouseSensitivity.value = settings.mouseSensitivity;
        UpdateMouseSensitivitySlider();
        dpdClouds.value = (int)settings.cloudStyle;

        ManagementMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    private void Awake()
    {
        if (!File.Exists(Application.dataPath + "/settings.cfg"))
        {
            Debug.Log($"<color=yellow>File not found! Creating file...</color>");

            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);
        }
        else
        {
            Debug.Log($"<color=green>Settings file found! Loading settings...</color>");

            string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
            settings = JsonUtility.FromJson<Settings>(jsonImport);
        }
    }

    #region SETTINGS

    public void LeaveSettings()
    {
        settings.viewDistanceInChunks = (int)sldViewDistance.value;
        settings.mouseSensitivity = sldMouseSensitivity.value;
        settings.cloudStyle = (CloudStyle)dpdClouds.value;

        string jsonExport = JsonUtility.ToJson(settings);
        File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

        ManagementMenu();
    }

    public void UpdateViewDistanceSlider() => txtViewDistance.text = $"View Distance: {sldViewDistance.value}";
    public void UpdateMouseSensitivitySlider() => txtMouseSensitivity.text = $"Mouse Sensit.: {sldMouseSensitivity.value.ToString("F1")}";


    #endregion

    void ManagementMenu()
    {
        pnlMainMenu.SetActive(!pnlMainMenu.activeSelf);
        pnlSettings.SetActive(!pnlSettings.activeSelf);
    }

}
