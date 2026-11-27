using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MazeAI
{
    public class QLPlayer : Player
    {
        public string qtable_filename;
        Pos? previous_position = null;
        readonly double[,] qtable;
        readonly System.Random random = new System.Random();

        public QLPlayer(string qt_filename)
        {
            qtable_filename = qt_filename;
            string[] lines = File.ReadAllLines(qtable_filename);
            int cols = lines[0].Split(',').Length;
            qtable = new double[lines.Length, cols];
            for (int s = 0; s < lines.Length; s++)
            {
                string[] values = lines[s].Split(',');
                for (int a = 0; a < cols; a++)
                    qtable[s, a] = double.Parse(values[a], CultureInfo.InvariantCulture);
            }
        }

        public override void Reset() { }

        public string GetQtableFilename() => qtable_filename;

        public void SetQtableFilename(string qt_filename) => qtable_filename = qt_filename;

        bool IsValidAction(Observation observation, Action action)
        {
            Pos current_position = observation.GetPosition();
            var fm = new ForwardModel();
            Observation test_obs = observation.Clone();
            fm.Test(test_obs, action);
            Pos new_position = test_obs.GetPosition();

            bool moved = new_position != current_position;
            bool backwards = previous_position != null && new_position == previous_position.Value;
            return moved && !backwards;
        }

        Action GetRandomValidAction(Observation observation, float budget, List<Action> l_actions)
        {
            bool action_found = false;
            Action action = null;
            while (!action_found && l_actions.Count > 0)
            {
                action = l_actions[random.Next(l_actions.Count)];
                bool valid_position = IsValidAction(observation, action);

                if (valid_position)
                    action_found = true;
                else
                    l_actions.Remove(action);
            }

            if (!action_found)
                action = null;
            return action;
        }

        public override Action Think(Observation observation, float budget)
        {
            List<Action> l_actions = observation.GetListActions();
            Pos current_position = observation.GetPosition();
            int maze_n_cols = observation.GetMazeColumns();
            int idx_state = current_position.row * maze_n_cols + current_position.column;
            Action action;
            if (idx_state < qtable.GetLength(0))
            {
                int best_action_idx = QLearner.ArgMax(qtable, idx_state);
                action = l_actions[best_action_idx];

                bool valid = IsValidAction(observation, action);
                if (!valid)
                {
                    l_actions.Remove(action);
                    action = GetRandomValidAction(observation, budget, l_actions);
                }

                if (action != null)
                    previous_position = current_position;
            }
            else
                action = null;
            return action;
        }

        public override string ToString() => "QLPlayer";
    }
}
