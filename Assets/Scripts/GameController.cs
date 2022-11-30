using UnityEngine;

public class GameController : MonoBehaviour
{
    //Logic
    private int width = 16;
    private int height = 16;
    private int mineCount = 40;

    private Board board;
    private Cell[,] state;
    private bool gameover = false;

    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            NewGame();
            gameover = false;
        }

        if (!gameover)
        {
            if(Input.GetMouseButtonDown(1))
            {
                Flags();
            } else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
        }
    }


    private void NewGame()
    {
        state = new Cell[width, height];

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        Camera.main.orthographicSize = height / 2f;
        board.Draw(state);
    }

    private void GenerateCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while(state[x, y].type == Cell.Type.Mine)
            {
                x++;

                if(x >= width)
                {
                    x = 0;
                    y++;

                    if (y >= height)
                    {
                        y = 0;
                    }
                }
            }

            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                
                if(cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x, y);
                
                if(cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                state[x, y] = cell;
            }
        } 
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if(i == 0 && j == 0)
                {
                    continue;
                }

                int x = cellX + i;
                int y = cellY + j;

                if(GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void Flags()
    {
        Vector3Int cellPosition = MousePositionToCell();
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
    }

    private void Reveal()
    {
        Vector3Int cellPosition = MousePositionToCell();
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                Flood(cell);
                WinCheck();
                break;
            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                WinCheck();
                break;
        }
        
        board.Draw(state);
    }

    private void Flood(Cell cell)
    {
        if(cell.revealed) return;
        if(cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
            Flood(GetCell(cell.position.x + 1, cell.position.y + 1));
            Flood(GetCell(cell.position.x - 1, cell.position.y + 1));
            Flood(GetCell(cell.position.x - 1, cell.position.y - 1));
            Flood(GetCell(cell.position.x + 1, cell.position.y - 1));
        }
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game Over!");
        gameover = true;

        cell.revealed = true;
        cell.exploded = true;
        state[cell.position.x, cell.position.y] = cell;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = state[x, y];
                if(cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private void WinCheck()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if(cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        Debug.Log("You won!");
        gameover = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if(cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }
    }

    private Vector3Int MousePositionToCell()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return board.tilemap.WorldToCell(worldPosition);
    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return state[x, y];
        } else
        {
            return new Cell();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
