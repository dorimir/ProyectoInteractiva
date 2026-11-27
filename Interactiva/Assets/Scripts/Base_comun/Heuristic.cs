namespace MazeAI
{
    public class Heuristic
    {
        public virtual double GetScore(Observation observation) => 0;
    }
}
