using System.Collections.Generic;

namespace MazeAI
{
    public class Observation
    {
        public int row = 0;
        public int column = 0;
        public Maze maze = null;
        public int iteration_number = 0;
        public int max_iterations = 0;

        public void Set(GameState game_state)
        {
            row = game_state.row;
            column = game_state.column;
            maze = game_state.maze.Clone();
            iteration_number = game_state.iteration_number;
            max_iterations = game_state.max_iterations;
        }

        public Pos GetPosition() => new Pos(row, column);

        public int GetMazeColumns() => maze.number_columns;

        public int GetMazeRows() => maze.number_rows;

        public void SetPosition(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        public Observation Clone()
        {
            var new_observation = new Observation();
            new_observation.row = row;
            new_observation.column = column;
            new_observation.maze = maze.Clone();
            new_observation.iteration_number = iteration_number;
            new_observation.max_iterations = max_iterations;
            return new_observation;
        }

        public List<Action> GetListActions()
        {
            var a_up = new Action();
            var a_right = new Action();
            var a_down = new Action();
            var a_left = new Action();

            a_up.SetUp();
            a_right.SetRight();
            a_down.SetDown();
            a_left.SetLeft();

            return new List<Action> { a_up, a_right, a_down, a_left };
        }

        public int GetNumberActions() => GetListActions().Count;

        public void PrintMaze() => maze.PrintMaze(row, column);

        public bool ReachedGoal() => maze.IsGoal(row, column);

        public bool IsInAHole() => maze.IsHole(row, column);

        public bool IsWall(int row, int column) => maze.IsWall(row, column);

        public bool IsIntoMaze(int row, int column) => maze.IsIntoMaze(row, column);

        public Pos GetStartPosition() => maze.GetStartPosition();

        public Pos? GetGoalPosition() => maze.GetGoalPosition();

        public bool IsTerminal() =>
            maze.IsGoal(row, column) || maze.IsHole(row, column) || iteration_number >= max_iterations;

        public override string ToString() => "Position: [" + row + ", " + column + "]\n" + maze;
    }
}
