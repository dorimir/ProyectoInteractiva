namespace MazeAI
{
    public class OSLAPlayer : Player
    {
        readonly ForwardModel forward_model = new ForwardModel();
        readonly DistanceHeuristic heuristic = new DistanceHeuristic();
        Pos? previous_position = null;

        public override void Reset() => previous_position = null;

        Pos? FindGoal(Observation observation)
        {
            Maze maze = observation.maze;
            for (int row = 0; row < maze.number_rows; row++)
                for (int column = 0; column < maze.number_columns; column++)
                    if (maze.IsGoal(row, column))
                        return new Pos(row, column);
            return null;
        }

        public override Action Think(Observation observation, float budget)
        {
            var list_actions = observation.GetListActions();
            Action best_action = null;
            double? best_score = null;
            Pos current_position = observation.GetPosition();
            Pos? goal = FindGoal(observation);

            foreach (var action in list_actions)
            {
                Observation next_observation = observation.Clone();
                forward_model.Test(next_observation, action);
                Pos next_position = next_observation.GetPosition();

                if (next_observation.ReachedGoal())
                {
                    previous_position = current_position;
                    return action;
                }

                if (next_observation.IsInAHole() ||
                    next_position == current_position ||
                    next_position == previous_position)
                    continue;

                double score = heuristic.GetScore(next_observation, next_position, goal);

                if (best_score == null || score > best_score)
                {
                    best_score = score;
                    best_action = action;
                }
            }

            if (best_action != null)
            {
                previous_position = current_position;
                return best_action;
            }

            return null;
        }

        public override string ToString() => "OSLAPlayer";
    }
}
