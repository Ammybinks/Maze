using System;

namespace Maze
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Maze game = new Maze())
            {
                game.Run();
            }
        }
    }
}

