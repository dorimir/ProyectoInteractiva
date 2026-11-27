using System;

namespace MazeAI
{
    public class DistanceHeuristic : Heuristic
    {
        public double GetScore(Observation observation, Pos next_position, Pos? goal)
        {
            double score;
            if (observation.IsInAHole())
                return -500;
            else if (observation.ReachedGoal())
                return 500;
            else
                score = 0;

            if (goal != null)
                score -= Math.Abs(next_position.row - goal.Value.row) + Math.Abs(next_position.column - goal.Value.column);
            return score;
        }
    }
}
