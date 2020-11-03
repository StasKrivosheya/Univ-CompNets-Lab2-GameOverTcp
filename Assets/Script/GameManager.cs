using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public GameObject mainMenu;
    public GameObject connectMenu;
    public GameObject serverMenu;

    private void Start()
    {
        Instance = this;

        serverMenu.SetActive(false);
        connectMenu.SetActive(false);

        DontDestroyOnLoad(gameObject);
    }

    public void ConnectButton()
    {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }
    public void HostButton()
    {
        mainMenu.SetActive(false);
        serverMenu.SetActive(true);


    }

    public void ConnectToServerButton()
    {

    }

    public void BackButton()
    {
        mainMenu.SetActive(true);
        connectMenu.SetActive(false);
        serverMenu.SetActive(false);
    }
}
