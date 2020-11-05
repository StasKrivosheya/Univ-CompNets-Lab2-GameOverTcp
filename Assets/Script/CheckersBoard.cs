using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheckersBoard : MonoBehaviour
{
    public static CheckersBoard Instance { get; private set; }

    public Piece[,] pieces = new Piece[8, 8];

    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    public CanvasGroup alertCanvas;
    private float lastAlert;
    private bool alertActive;

    private bool gameIsOver;
    private float winTime;

    private readonly Vector3 boardOffset = new Vector3(-4.0f, 0, -4.0f);
    private readonly Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);
    
    // who am I as a player
    public bool isWhite;
    // who's turn
    private bool isWhiteTurn;
    private bool hasKilled;

    private Piece selectedPiece;
    private List<Piece> forcedPieces;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    private Client client;

    private void Start()
    {
        Instance = this;

        client = FindObjectOfType<Client>();

        isWhite = client.isHost;
        Alert(client.players[0].name + " VS " + client.players[1].name);
        isWhiteTurn = true;

        GenerateBoard();

        forcedPieces = new List<Piece>();
    }

    private void Update()
    {
        if (gameIsOver)
        {
            if (Time.time - winTime > 3.0f)
            {
                Server server = FindObjectOfType<Server>();
                Client client = FindObjectOfType<Client>();

                if (server)
                {
                    Destroy(server.gameObject);
                }

                if (client)
                {
                    Destroy(client.gameObject);
                }

                SceneManager.LoadScene("Menu");
            }

            return;
        }

        if (GameManager.Instance.gameInterrupted)
        {
            Alert("Your opponent lost connection");
            winTime = Time.time;
            gameIsOver = true;
            return;
        }

        UpdateAlert();
        UpdateMouseOver();

        if (isWhite ? isWhiteTurn : !isWhiteTurn)
        {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;

            if (selectedPiece != null)
            {
                UpdatePieceDrag(selectedPiece);
            }

            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece(x, y);
            }

            if (Input.GetMouseButtonUp(0))
            {
                TryMove((int)startDrag.x, (int)startDrag.y, x, y);
            }
        }
        
    }

    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
            out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }

    }

    private void UpdatePieceDrag(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),
            out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }

    private void SelectPiece(int x, int y)
    {
        // Out of bounds
        if (x < 0 || x >= pieces.GetLength(0) ||
            y < 0 || y >= pieces.GetLength(1))
        {
            return;
        }

        Piece p = pieces[x, y];
        if (p != null && p.isWhite == isWhite)
        {
            if (forcedPieces.Count == 0)
            {
                selectedPiece = p;
                startDrag = mouseOver;
            }
            else
            {
                // look for the piece under our forced pieces list
                if (forcedPieces.Find(fp => fp == p) == null)
                {
                    return;
                }

                selectedPiece = p;
                startDrag = mouseOver;
            }
        }
    }

    public void TryMove(int x1, int y1, int x2, int y2)
    {
        forcedPieces = ScanForPossibleMove();

        // Multiplayer support
        startDrag = new Vector2(x1, y1);
        endDrag = new Vector2(x2, y2);
        selectedPiece = pieces[x1, y1];

        // Out of bounds
        if (x2 < 0 || x2 >= pieces.GetLength(0) ||
            y2 < 0 || y2 >= pieces.GetLength(1))
        {
            if (selectedPiece != null)
            {
                MovePiece(selectedPiece, x1, y1);
            }

            startDrag = Vector2.zero;
            selectedPiece = null;
            return;
        }

        if (selectedPiece != null)
        {
            if (startDrag == endDrag)
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }

            // if it's a valid move
            if (selectedPiece.ValidMove(pieces, x1, y1, x2, y2))
            {
                // if we killed anything
                if (Mathf.Abs(x2 - x1) == 2)
                {
                    Piece p = pieces[(x1 + x2) / 2, (y1 + y2) / 2];

                    if (p != null)
                    {
                        pieces[(x1 + x2) / 2, (y1 + y2) / 2] = null;
                        Destroy(p.gameObject);
                        hasKilled = true;
                    }
                }
                else if (pieces[x1, y1].isQueen && Mathf.Abs(x2 - x1) > 2) // if Queen killed anything
                {
                    int enemyX = x2 > x1 ? x2 - 1 : x2 + 1;
                    int enemyY = y2 > y1 ? y2 - 1 : y2 + 1;

                    Piece p = pieces[enemyX, enemyY];
                    if (p != null)
                    {
                        pieces[enemyX, enemyY] = null;
                        Destroy(p.gameObject);

                        hasKilled = true;
                    }
                }

                // were we supposed to kill anything
                if (forcedPieces.Count != 0 && !hasKilled)
                {
                    MovePiece(selectedPiece, x1, y1);
                    startDrag = Vector2.zero;
                    selectedPiece = null;
                    return;
                }

                pieces[x2, y2] = selectedPiece;
                pieces[x1, y1] = null;

                MovePiece(selectedPiece, x2, y2);

                EndTurn();
            }
            else
            {
                MovePiece(selectedPiece, x1, y1);
                startDrag = Vector2.zero;
                selectedPiece = null;
                return;
            }
        }
    }

    private List<Piece> ScanForPossibleMove(Piece p, int x, int y)
    {
        forcedPieces = new List<Piece>();

        if (pieces[x, y].IsForceToMove(pieces, x, y))
        {
            forcedPieces.Add(pieces[x, y]);
        }

        return forcedPieces;
    }
    private List<Piece> ScanForPossibleMove()
    {
        forcedPieces = new List<Piece>();

        // check all the pieces
        for (int i = 0; i < pieces.GetLength(0); i++)
        {
            for (int j = 0; j < pieces.GetLength(1); j++)
            {
                // if there's a piece and it's your turn
                if (pieces[i, j] != null && pieces[i, j].isWhite == isWhiteTurn)
                {
                    if (pieces[i, j].IsForceToMove(pieces, i, j))
                    {
                        forcedPieces.Add(pieces[i, j]);
                    }
                }
            }
        }

        return forcedPieces;
    }

    private void EndTurn()
    {
        int x = (int)endDrag.x;
        int y = (int)endDrag.y;

        // Promotions
        if (selectedPiece != null)
        {
            // if the Piece has to become a Queen
            if (selectedPiece.isWhite && !selectedPiece.isQueen && y == 7)
            {
                selectedPiece.isQueen = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
            else if (!selectedPiece.isWhite && !selectedPiece.isQueen && y == 0)
            {
                selectedPiece.isQueen = true;
                selectedPiece.transform.Rotate(Vector3.right * 180);
            }
        }

        // client moved
        string msg = "CMOV|";
        msg += startDrag.x.ToString() + "|";
        msg += startDrag.y.ToString() + "|";
        msg += endDrag.x.ToString() + "|";
        msg += endDrag.y.ToString();

        client.Send(msg);

        selectedPiece = null;
        startDrag = Vector2.zero;

        // to prevent Queen from illegal loop killing
        bool wasQueen = pieces[x, y].isQueen;
        pieces[x, y].isQueen = false;

        if (ScanForPossibleMove(selectedPiece, x, y).Count != 0 && hasKilled)
        {
            // turn status back
            pieces[x, y].isQueen = wasQueen;
            return;
        }
        // turn status back
        pieces[x, y].isQueen = wasQueen;

        isWhiteTurn = !isWhiteTurn;

        // good for single player, bad for multi
        // isWhite = !isWhite;

        hasKilled = false;

        CheckVictory();

        if (!gameIsOver)
        {
            if (isWhiteTurn)
            {
                Alert("Your turn, " + client.players[0].name);
            }
            else
            {
                Alert("Your turn, " + client.players[1].name);
            }
        }
    }

    private void CheckVictory()
    {
        bool hasWhite = false, hasBlack = false;

        foreach (var p in pieces)
        {
            if (p != null)
            {
                if (p.isWhite)
                {
                    hasWhite = true;
                }
                else
                {
                    hasBlack = true;
                }
            }
        }

        if (!hasWhite)
        {
            Victory(false);
        }
        if (!hasBlack)
        {
            Victory(true);
        }
    }

    private void Victory(bool isWhite)
    {
        winTime = Time.time;

        Alert(isWhite ? "White team has won!" : "Black team has won!");

        gameIsOver = true;
    }

    private void GenerateBoard()
    {
        // Generate White team
        for (int y = 0; y < 3; y++)
        {
            bool isRowOdd = y % 2 == 0;
            for (int x = 0; x < 8; x+=2)
            {
                GeneratePiece(isRowOdd ? x : x + 1, y);
            }
        }

        // Generate Black team
        for (int y = 7; y > 4; y--)
        {
            bool isRowOdd = y % 2 == 0;
            for (int x = 0; x < 8; x += 2)
            {
                GeneratePiece(isRowOdd ? x : x + 1, y);
            }
        }
    }

    private void GeneratePiece(int x, int y)
    {
        bool shouldPieceBeWhite = y < 4;
        GameObject gameObject = Instantiate(shouldPieceBeWhite ? whitePiecePrefab : blackPiecePrefab) as GameObject;
        gameObject.transform.SetParent(transform);

        Piece p = gameObject.GetComponent<Piece>();
        pieces[x, y] = p;

        MovePiece(p, x, y);
    }

    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
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
