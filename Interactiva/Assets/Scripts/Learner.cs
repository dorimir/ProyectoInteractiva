namespace MazeAI
{
    public abstract class Learner
    {
        public abstract double[,] Learn(GameState state, int n_episodes);

        public virtual void SetHeuristic(Heuristic heuristic) { }
    }
}
