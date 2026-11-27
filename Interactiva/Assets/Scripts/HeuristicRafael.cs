using System;

namespace MazeAI
{
    public class HeuristicRafael : Heuristic
    {
        public override double GetScore(Observation observation)
        {
            if (observation.IsInAHole())
                return -1000;
            else if (observation.ReachedGoal())
                return 1000;
            else
                return Math.Sqrt(Math.Pow(observation.GetStartPosition().row - observation.GetPosition().row, 2) +
                                 Math.Pow(observation.GetStartPosition().column - observation.GetPosition().column, 2));
        }
    }
}
