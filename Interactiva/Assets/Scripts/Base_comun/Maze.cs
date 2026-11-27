namespace MazeAI
{
    public struct Pos
    {
        public int row, column;
        public Pos(int row, int column) { this.row = row; this.column = column; }
        public static bool operator ==(Pos a, Pos b) => a.row == b.row && a.column == b.column;
        public static bool operator !=(Pos a, Pos b) => !(a == b);
        public override bool Equals(object o) => o is Pos p && p == this;
        public override int GetHashCode() => row * 1000 + column;
        public override string ToString() => "[" + row + ", " + column + "]";
    }

    public class Maze
    {
        public int number_rows = 0;
        public int number_columns = 0;
        public int[,] cells = new int[0, 0];

        public int GetCell(int row, int column) => cells[row, column];
        public bool IsEmpty(int row, int column) => cells[row, column] == 0;
        public bool IsStart(int row, int column) => cells[row, column] == 1;
        public bool IsHole(int row, int column) => cells[row, column] == 2;
        public bool IsWall(int row, int column) => cells[row, column] == 3;
        public bool IsGoal(int row, int column) => cells[row, column] == 4;

        public virtual Maze Clone() => null;

        public bool IsIntoMaze(int row, int column) =>
            0 <= row && row < number_rows && 0 <= column && column < number_columns;

        public void PrintMaze(int row, int column)
        {
            string s = "";
            for (int i = 0; i < number_rows; i++)
            {
                for (int j = 0; j < number_columns; j++)
                {
                    if (i == row && j == column) s += "*";
                    else if (cells[i, j] == 0) s += "_";
                    else if (cells[i, j] == 1) s += "S";
                    else if (cells[i, j] == 2) s += "X";
                    else if (cells[i, j] == 3) s += "#";
                    else if (cells[i, j] == 4) s += "E";
                }
                s += "\n";
            }
            UnityEngine.Debug.Log(s);
        }

        public Pos GetStartPosition()
        {
            for (int r = 0; r < number_rows; r++)
                for (int c = 0; c < number_columns; c++)
                    if (IsStart(r, c)) return new Pos(r, c);
            return new Pos(0, 0);
        }

        public Pos? GetGoalPosition()
        {
            for (int r = 0; r < number_rows; r++)
                for (int c = 0; c < number_columns; c++)
                    if (IsGoal(r, c)) return new Pos(r, c);
            return null;
        }

        public override string ToString() => "Maze";
    }
}
