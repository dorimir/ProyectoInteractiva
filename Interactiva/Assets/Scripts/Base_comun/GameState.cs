namespace MazeAI
{
    public class GameState
    {
        public Maze maze = null;
        public int row = 0;
        public int column = 0;
        public int iteration_number = 0;
        public int max_iterations = 100;

        public void Reset(Maze maze, int max_iterations)
        {
            Pos start_position = maze.GetStartPosition();
            this.maze = maze.Clone();
            row = start_position.row;
            column = start_position.column;
            iteration_number = 0;
            this.max_iterations = max_iterations;
        }

        public int GetIterationNumber() => iteration_number;

        public void IncrementIterationNumber() => iteration_number += 1;

        public int GetMazeColumns() => maze.number_columns;

        public int GetMazeRows() => maze.number_rows;

        public Pos GetPosition() => new Pos(row, column);

        public void SetPosition(int row, int column)
        {
            this.row = row;
            this.column = column;
        }

        public bool IsTerminal() =>
            maze.IsGoal(row, column) || maze.IsHole(row, column) || iteration_number >= max_iterations;

        public bool ReachedGoal() => maze.IsGoal(row, column);

        public bool IsWall(int row, int column) => maze.IsWall(row, column);

        public GameState Clone()
        {
            var new_game_state = new GameState();
            new_game_state.row = row;
            new_game_state.column = column;
            new_game_state.maze = maze.Clone();
            new_game_state.iteration_number = iteration_number;
            new_game_state.max_iterations = max_iterations;
            return new_game_state;
        }

        public Observation GetObservation()
        {
            var new_observation = new Observation();
            new_observation.Set(this);
            return new_observation;
        }

        public bool IsIntoMaze(int row, int column) => maze.IsIntoMaze(row, column);

        public int GetNumberStates() => maze.number_rows * maze.number_columns;

        public override string ToString() => "Position: [" + row + ", " + column + "]\n" + maze;
    }
}
