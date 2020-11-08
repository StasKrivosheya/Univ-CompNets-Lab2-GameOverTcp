using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; }

    public bool isInTournamentMode;
    public Toggle tournamenToggle;

    public GameObject mainMenu;
    public GameObject connectMenu;
    public GameObject serverMenu;

    public GameObject serverPrefab;
    public GameObject clientPrefab;

    public InputField nameInput;

    public CanvasGroup alertCanvas;
    private float lastAlert;
    private bool alertActive;

    private Client c;
    public bool gameInterrupted;

    private void Start()
    {
        Instance = this;

        serverMenu.SetActive(false);
        connectMenu.SetActive(false);

        DontDestroyOnLoad(gameObject);

        Alert("Welcome to the Checkers!");
    }

    private void Update()
    {
        UpdateAlert();

        if (!gameInterrupted)
        {
            if (c != null && c.OpponentDisconnected)
            {
                gameInterrupted = true;
            }
        }

        if (tournamenToggle)
        {
            isInTournamentMode = tournamenToggle.isOn;
        }
    }

    public void ConnectButton()
    {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }
    public void HostButton()
    {
        try
        {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            try
            {
                s.Init();
            }
            catch (Exception e)
            {
                Debug.Log("Socket error: " + e.Message);
                SceneManager.LoadScene("Menu");
                mainMenu.SetActive(true);
                Destroy(s.gameObject);

                return;
            }

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = nameInput.text;
            c.isHost = true;

            if (c.clientName == "")
            {
                c.clientName = "Host";
            }

            c.ConnectToServer("127.0.0.1", 6321);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        mainMenu.SetActive(false);
        serverMenu.SetActive(true);
    }

    public void ConnectToServerButton()
    {
        string hostAddress = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (hostAddress == "")
        {
            hostAddress = "127.0.0.1";
        }

        try
        {
            //Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c = Instantiate(clientPrefab).GetComponent<Client>();

            c.clientName = nameInput.text;
            if (c.clientName == "")
            {
                c.clientName = "Client";
            }

            c.ConnectToServer(hostAddress, 6321);

            connectMenu.SetActive(false);
        }
        catch// (Exception e)
        {
            Alert("Wait for the host being ready!");
            // Debug.Log("GameManager ConnectToServerButton error: " + e.Message);
        }
    }

    public void BackButton()
    {
        mainMenu.SetActive(true);
        connectMenu.SetActive(false);
        serverMenu.SetActive(false);

        Server s = FindObjectOfType<Server>();
        if (s != null)
        {
            s.StopListener();
            Destroy(s.gameObject);
        }

        Client c = FindObjectOfType<Client>();
        if (c != null)
        {
            Destroy(c.gameObject);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void Alert(string text)
    {
        alertCanvas.GetComponentInChildren<Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        alertActive = true;
    }
    public void UpdateAlert()
    {
        if (alertActive)
        {
            if (Time.time - lastAlert > 1.5f)
            {
                alertCanvas.alpha = 1 - (Time.time - lastAlert - 1.5f);

                if (Time.time - lastAlert > 2.5f)
                {
                    alertActive = false;
                }
            }
        }
    }
}
