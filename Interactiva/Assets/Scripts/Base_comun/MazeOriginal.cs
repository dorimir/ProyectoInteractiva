namespace MazeAI
{
    public class MazeOriginal : Maze
    {
        public MazeOriginal()
        {
            number_rows = 8;
            number_columns = 8;

            cells = new int[,] { { 1, 0, 0, 0, 2, 0, 0, 0 },
                                 { 0, 0, 2, 0, 0, 0, 3, 0 },
                                 { 0, 0, 0, 3, 3, 0, 3, 0 },
                                 { 0, 2, 0, 0, 2, 0, 3, 0 },
                                 { 0, 0, 0, 0, 0, 0, 3, 0 },
                                 { 2, 3, 2, 0, 2, 0, 2, 0 },
                                 { 0, 0, 0, 0, 3, 0, 0, 0 },
                                 { 3, 3, 0, 2, 3, 2, 0, 4 } };
        }

        public override Maze Clone()
        {
            var new_maze = new MazeOriginal();
            new_maze.number_rows = number_rows;
            new_maze.number_columns = number_columns;
            new_maze.cells = (int[,])cells.Clone();
            return new_maze;
        }

        public override string ToString() => "MazeOriginal";
    }
}
