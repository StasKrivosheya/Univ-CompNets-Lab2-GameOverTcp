using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isQueen;

    public bool IsForceToMove(Piece[,] board, int x, int y)
    {
        if (isWhite || isQueen)
        {
            // top left
            if (x >= 2 && y <= 5)
            {
                Piece p = board[x - 1, y + 1];
                // if there is a Piece of another color
                if (p != null && p.isWhite != isWhite)
                {
                    // check if it's possible to land
                    if (board[x - 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
            // top right
            if (x <= 5 && y <= 5)
            {
                Piece p = board[x + 1, y + 1];
                // if there is a Piece of another color
                if (p != null && p.isWhite != isWhite)
                {
                    // check if it's possible to land
                    if (board[x + 2, y + 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        if (!isWhite || isQueen)
        {
            // bottom left
            if (x >= 2 && y >= 2)
            {
                Piece p = board[x - 1, y - 1];
                // if there is a Piece of another color
                if (p != null && p.isWhite != isWhite)
                {
                    // check if it's possible to land
                    if (board[x - 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
            // bottom right
            if (x <= 5 && y >= 2)
            {
                Piece p = board[x + 1, y - 1];
                // if there is a Piece of another color
                if (p != null && p.isWhite != isWhite)
                {
                    // check if it's possible to land
                    if (board[x + 2, y - 2] == null)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool ValidMove(Piece[,] board, int x1, int y1, int x2, int y2)
    {
        // Move on the top of another piece - illegal
        if (board[x2, y2] != null)
        {
            return false;
        }

        int deltaMoveX = Mathf.Abs(x2 - x1);
        int deltaMoveY = y2 - y1;

        if (isWhite || isQueen)
        {
            if (deltaMoveX == 1)
            {
                if (deltaMoveY == 1)
                {
                    return true;
                }
            }
            else if (deltaMoveX == 2)
            {
                if (deltaMoveY == 2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];

                    if (p != null && p.isWhite != isWhite)
                    {
                        return true;
                    }
                }
            }
        }

        if (!isWhite || isQueen)
        {
            if (deltaMoveX == 1)
            {
                if (deltaMoveY == -1)
                {
                    return true;
                }
            }
            else if (deltaMoveX == 2)
            {
                if (deltaMoveY == -2)
                {
                    Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];

                    if (p != null && p.isWhite != isWhite)
                    {
                        return true;
                    }
                }
            }
        }

        // making able to kill in a backward direction
        deltaMoveY = Mathf.Abs(deltaMoveY);
        if (deltaMoveX == 2 && deltaMoveY == 2)
        {
            Piece p = board[(x1 + x2) / 2, (y1 + y2) / 2];

            if (p != null && p.isWhite != isWhite)
            {
                return true;
            }
        }

        // making able to move on diagonal for Queen
        if (isQueen)
        {
            // |deltaMove| = [3, 7] (other ways have already been handled)
            deltaMoveX = x2 - x1;
            deltaMoveY = y2 - y1;

            // if moving not on a diagonal
            if (Mathf.Abs(deltaMoveX) != Mathf.Abs(deltaMoveY))
            {
                return false;
            }

            // check whether we have just met an enemy (on the previous iteration)
            bool enemyEncountered = false;

            while (x1 != x2)
            {
                x1 = deltaMoveX > 0 ? ++x1 : --x1;
                y1 = deltaMoveY > 0 ? ++y1 : --y1;

                Piece p = board[x1, y1];

                if (p != null && p.isWhite == isWhite)
                {
                    return false;
                }

                // check for 2 enemies in a row
                if (enemyEncountered)
                {
                    if (p != null && p.isWhite != isWhite)
                    {
                        return false;
                    }

                    enemyEncountered = false;
                }

                if (p != null && p.isWhite != isWhite)
                {
                    enemyEncountered = true;
                }
            }

            return true;
        }

        return false;
    }
}
