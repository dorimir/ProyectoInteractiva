using UnityEngine;

namespace MazeAI
{
    public class Game
    {
        public GameState game_state = new GameState();
        public ForwardModel forward_model = new ForwardModel();

        public (bool reached_goal, int iterations) Run(Maze maze, Player player, float budget, int max_iterations, bool verbose)
        {
            game_state.Reset(maze, max_iterations);

            while (!game_state.IsTerminal())
            {
                if (verbose)
                    Debug.Log("Iteration: " + game_state.GetIterationNumber());
                Observation observation = game_state.GetObservation();
                if (verbose)
                    observation.PrintMaze();
                Action action = player.Think(observation, budget);
                if (action == null)
                    break;
                if (verbose)
                    Debug.Log("Player select: " + action + "\n");
                forward_model.Play(game_state, action);
            }

            bool reached_goal;
            if (game_state.ReachedGoal())
            {
                if (verbose) Debug.Log("Player reached the goal !!!");
                reached_goal = true;
            }
            else
            {
                if (verbose) Debug.Log("Ups!!! player could not find the exit");
                reached_goal = false;
            }

            return (reached_goal, game_state.GetIterationNumber());
        }
    }
}
