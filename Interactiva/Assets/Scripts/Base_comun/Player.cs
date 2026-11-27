namespace MazeAI
{
    public abstract class Player
    {
        public abstract Action Think(Observation observation, float budget);

        public virtual void Reset() { }

        public virtual string ConfigInfoStr() => null;
    }
}
