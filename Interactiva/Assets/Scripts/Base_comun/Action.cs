namespace MazeAI
{
    public class Action
    {
        public int action = 0;

        public int GetAction() => action;

        public bool IsUp() => action == 1;
        public bool IsRight() => action == 2;
        public bool IsDown() => action == 3;
        public bool IsLeft() => action == 4;

        public void SetUp() => action = 1;
        public void SetRight() => action = 2;
        public void SetDown() => action = 3;
        public void SetLeft() => action = 4;

        public void ReverseAction()
        {
            if (action == 1) action = 3;
            else if (action == 2) action = 4;
            else if (action == 3) action = 1;
            else if (action == 4) action = 2;
        }

        public override string ToString()
        {
            string s = "";
            if (action == 1) s += "UP";
            else if (action == 2) s += "RIGHT";
            else if (action == 3) s += "DOWN";
            else if (action == 4) s += "LEFT";
            return s;
        }
    }
}
