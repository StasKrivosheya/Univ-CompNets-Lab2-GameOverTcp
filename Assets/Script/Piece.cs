using UnityEngine;

public class Piece : MonoBehaviour
{
    public bool isWhite;
    public bool isQueen;

    public bool IsForceToMove(Piece[,] board, int x, int y)
    {
        // top left
        if (x >= 2 && y <= 5)
        {
            if (board[x, y].isQueen)
            {
                int i = x - 1, j = y + 1;
                while (i != 0 && j != 7)
                {
                    if (board[i, j] != null && board[i, j].isWhite == isWhite)
                    {
                        break;
                    }

                    // if we've met enemy and can land
                    if (board[i, j] != null &&
                        board[i, j].isWhite != isWhite)
                    {
                        if (board[i - 1, j + 1] == null)
                        {
                            return true;
                        }

                        break;
                    }
                    --i;
                    ++j;
                }
            }

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
            if (board[x, y].isQueen)
            {
                int i = x + 1, j = y + 1;
                while (i != 7 && j != 7)
                {
                    if (board[i, j] != null && board[i, j].isWhite == isWhite)
                    {
                        break;
                    }

                    // if we've met enemy and can land
                    if (board[i, j] != null &&
                        board[i, j].isWhite != isWhite)
                    {
                        if (board[i + 1, j + 1] == null)
                        {
                            return true;
                        }

                        break;
                    }
                    ++i;
                    ++j;
                }
            }

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

        // bottom left
        if (x >= 2 && y >= 2)
        {
            if (board[x, y].isQueen)
            {
                int i = x - 1, j = y - 1;
                while (i != 0 && j != 0)
                {
                    if (board[i, j] != null && board[i, j].isWhite == isWhite)
                    {
                        break;
                    }

                    // if we've met enemy and can land
                    if (board[i, j] != null &&
                        board[i, j].isWhite != isWhite)
                    {
                        if (board[i - 1, j - 1] == null)
                        {
                            return true;
                        }

                        break;
                    }
                    --i;
                    --j;
                }
            }

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
            if (board[x, y].isQueen)
            {
                int i = x + 1, j = y - 1;
                while (i != 7 && j != 0)
                {
                    if (board[i, j] != null && board[i, j].isWhite == isWhite)
                    {
                        break;
                    }

                    // if we've met enemy and can land
                    if (board[i, j] != null &&
                        board[i, j].isWhite != isWhite)
                    {
                        if (board[i + 1, j - 1] == null)
                        {
                            return true;
                        }

                        break;
                    }
                    ++i;
                    --j;
                }
            }

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

        if (deltaMoveX == 1)
        {
            if (deltaMoveY == 1 && isWhite)
            {
                return true;
            }

            if (deltaMoveY == -1 && !isWhite)
            {
                return true;
            }
        }
        else if (deltaMoveX == 2 && Mathf.Abs(deltaMoveY) == 2)
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

            // if moving not on a diagonal - invalid move
            if (Mathf.Abs(deltaMoveX) == Mathf.Abs(deltaMoveY))
            {
                bool enemyEncountered = false;

                while (x1 != x2)
                {
                    x1 = deltaMoveX > 0 ? ++x1 : --x1;
                    y1 = deltaMoveY > 0 ? ++y1 : --y1;

                    Piece p = board[x1, y1];

                    // we are trying to kill our Piece - illegal
                    if (p != null && p.isWhite == isWhite)
                    {
                        return false;
                    }

                    // check if we can land after jumping over enemy and if it was our desired move
                    if (enemyEncountered)
                    {
                        if (p == null && x1 == x2)
                        {
                            return true;
                        }

                        return false;
                    }

                    if (p != null && p.isWhite != isWhite)
                    {
                        enemyEncountered = true;
                    }
                }

                // means we haven't met any Piece and can land
                return true;
            }
        }

        return false;
    }
}
