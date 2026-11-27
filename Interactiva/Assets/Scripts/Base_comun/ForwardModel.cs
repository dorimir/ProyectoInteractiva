namespace MazeAI
{
    public class ForwardModel
    {
        public void Play(GameState game_state, Action action)
        {
            Pos p = game_state.GetPosition();
            int row = p.row, column = p.column;

            if (action.IsUp() && game_state.IsIntoMaze(row - 1, column) && !game_state.IsWall(row - 1, column))
                game_state.SetPosition(row - 1, column);
            else if (action.IsRight() && game_state.IsIntoMaze(row, column + 1) && !game_state.IsWall(row, column + 1))
                game_state.SetPosition(row, column + 1);
            else if (action.IsDown() && game_state.IsIntoMaze(row + 1, column) && !game_state.IsWall(row + 1, column))
                game_state.SetPosition(row + 1, column);
            else if (action.IsLeft() && game_state.IsIntoMaze(row, column - 1) && !game_state.IsWall(row, column - 1))
                game_state.SetPosition(row, column - 1);

            game_state.IncrementIterationNumber();
        }

        public void Test(Observation observation, Action action)
        {
            Pos p = observation.GetPosition();
            int row = p.row, column = p.column;

            if (action.IsUp() && observation.IsIntoMaze(row - 1, column) && !observation.IsWall(row - 1, column))
                observation.SetPosition(row - 1, column);
            else if (action.IsRight() && observation.IsIntoMaze(row, column + 1) && !observation.IsWall(row, column + 1))
                observation.SetPosition(row, column + 1);
            else if (action.IsDown() && observation.IsIntoMaze(row + 1, column) && !observation.IsWall(row + 1, column))
                observation.SetPosition(row + 1, column);
            else if (action.IsLeft() && observation.IsIntoMaze(row, column - 1) && !observation.IsWall(row, column - 1))
                observation.SetPosition(row, column - 1);
        }
    }
}
