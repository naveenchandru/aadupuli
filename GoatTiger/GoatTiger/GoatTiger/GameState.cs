using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoatTiger
{
    class GameState
    {
        nodeState[,] grid;
        public List<nodeState[,]> positionslist = new List<nodeState[,]>();
        public List<int> mGoatsIntoBoardList = new List<int>();
        void fetchSavedStateFromStorage()
        {
            //logic to get saved state
        }
        void writeGameState()
        {
            //logic to write game state
        }

        

    }
}
