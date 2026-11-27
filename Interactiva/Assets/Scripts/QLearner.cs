using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MazeAI
{
    public class QLearner : Learner
    {
        public double[,] qtable = null;
        public double alpha = 0.8;
        public double gamma = 0.9;
        public double epsilon = 0.1;
        public Heuristic heuristic = new HeuristicRafael();
        public int max_play_steps = 100;
        public Pos? previous_pos = null;
        public string qtable_filename = null;

        public int first_goal_episode = -1;

        readonly System.Random random = new System.Random();

        public void SetModel(double[,] model) => qtable = (double[,])model.Clone();

        public string GetQtableFilename() => qtable_filename;

        public void SetQtableFilename(string qt_filename) => qtable_filename = qt_filename;

        public override void SetHeuristic(Heuristic heuristic) => this.heuristic = heuristic;

        public void AdjustQlParams()
        {
            alpha -= 0.0006;
            epsilon += 0.0008;
        }

        public Pos? GetRandomValidPosition(GameState game_state)
        {
            int max_attempts = game_state.GetNumberStates();
            bool pos_found = false;
            Pos? random_pos = null;
            int random_row = 0, random_col = 0;

            int attempts = 0;
            while (!pos_found && attempts < max_attempts)
            {
                random_row = random.Next(game_state.GetMazeRows());
                random_col = random.Next(game_state.GetMazeColumns());
                pos_found = !(game_state.IsWall(random_row, random_col)
                              || game_state.maze.IsHole(random_row, random_col)
                              || game_state.maze.IsGoal(random_row, random_col));
                attempts += 1;
            }
            if (pos_found)
                random_pos = new Pos(random_row, random_col);
            return random_pos;
        }

        public Action GetRandomValidAction(Observation obs)
        {
            Action final_action = null;
            List<Action> actions = obs.GetListActions();
            var test_forward = new ForwardModel();
            while (final_action == null)
            {
                Observation test_obs = obs.Clone();
                Action action = actions[random.Next(actions.Count)];
                test_forward.Test(test_obs, action);
                Pos new_pos = test_obs.GetPosition();
                if (new_pos != obs.GetPosition() && new_pos != previous_pos)
                    final_action = action;
                else
                {
                    actions.Remove(action);
                    if (actions.Count == 0)
                        break;
                }
            }
            return final_action;
        }

        public int GetBestActionState(Observation observation, Pos position)
        {
            int idx_state = position.row * observation.GetMazeColumns() + position.column;
            return ArgMax(qtable, idx_state);
        }

        public void UpdateQtable(Observation observation, Pos previous_pos, Pos actual_pos, int action_idx, double reward)
        {
            int maze_n_cols = observation.GetMazeColumns();
            int idx_previous_state = previous_pos.row * maze_n_cols + previous_pos.column;
            int idx_actual_state = actual_pos.row * maze_n_cols + actual_pos.column;

            if (idx_previous_state != idx_actual_state)
            {
                double actual_value = qtable[idx_previous_state, action_idx];
                double best_future_value = qtable[idx_actual_state, ArgMax(qtable, idx_actual_state)];
                double temporary_difference = reward + gamma * best_future_value - actual_value;
                double new_value = actual_value + alpha * temporary_difference;
                qtable[idx_previous_state, action_idx] = new_value;
            }
        }

        public override double[,] Learn(GameState state, int n_episodes)
        {
            GameState initial_state = state.Clone();
            Observation obs = initial_state.GetObservation();
            int n_states = initial_state.GetNumberStates();
            int n_actions = obs.GetNumberActions();
            qtable = new double[n_states, n_actions];
            first_goal_episode = -1;

            for (int episode = 0; episode < n_episodes; episode++)
            {
                initial_state.Reset(state.maze, state.max_iterations);

                Pos? random_pos = GetRandomValidPosition(initial_state);
                if (random_pos != null)
                    initial_state.SetPosition(random_pos.Value.row, random_pos.Value.column);
                Play(initial_state);

                if (first_goal_episode < 0 && initial_state.ReachedGoal())
                    first_goal_episode = episode + 1;

                AdjustQlParams();
            }
            return qtable;
        }

        public void Play(GameState state)
        {
            int step = 1;
            var fm = new ForwardModel();
            int action_idx = 0;
            while (!state.IsTerminal() && step < max_play_steps)
            {
                Observation obs = state.GetObservation();
                Pos actual_pos = obs.GetPosition();
                Action action;
                if (random.NextDouble() < epsilon)
                {
                    action_idx = GetBestActionState(obs, actual_pos);
                    action = obs.GetListActions()[action_idx];
                }
                else
                {
                    action = GetRandomValidAction(obs);
                    if (action != null)
                        action_idx = action.GetAction() - 1;
                }

                if (action != null)
                {
                    fm.Play(state, action);
                    double reward = heuristic.GetScore(state.GetObservation());
                    UpdateQtable(obs, actual_pos, state.GetPosition(), action_idx, reward);
                    previous_pos = actual_pos;
                }

                step += 1;
            }
        }

        public static int ArgMax(double[,] table, int row)
        {
            int best = 0;
            for (int a = 1; a < table.GetLength(1); a++)
                if (table[row, a] > table[row, best]) best = a;
            return best;
        }

        public static void WriteQtableToFile(double[,] ql_table, string filename)
        {
            var sb = new StringBuilder();
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            for (int s = 0; s < ql_table.GetLength(0); s++)
            {
                for (int a = 0; a < ql_table.GetLength(1); a++)
                {
                    if (a > 0) sb.Append(',');
                    sb.Append(ql_table[s, a].ToString(inv));
                }
                sb.Append('\n');
            }
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            File.WriteAllText(filename, sb.ToString());
        }

        public override string ToString() => "QLearner";
    }
}
