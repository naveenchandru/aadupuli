using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GoatTiger
{
    class BoardHistory
    {
        public static List<nodeState[,]> history = new List<nodeState[,]>();
        public static bool isBoardEqual(nodeState [,] data1,nodeState[,] data2)
        {
            if (data1.Rank == data2.Rank &&
                            Enumerable.Range(0, data1.Rank).All(dimension => data1.GetLength(dimension) == data2.GetLength(dimension)) && data1.Cast<nodeState>().SequenceEqual(data2.Cast<nodeState>()))
                return true;
            else return false;
        }
        public static bool isRepeated(nodeState[,] data1)
        {
            for (int i = 0; i < BoardHistory.history.Count; i++)
            {
                if ( isBoardEqual(data1,BoardHistory.history.ElementAt(i))){
                    return true;
                }
            }
            return false;
        }
    }
    
    class Board
    {
        public nodeState[,] mValues;
        public nodeState[,] prevMove;
        int mScore;
        public bool mTurnForPlayer;//true - tiger
        public int mGoatsIntoBoard;

        public int RecursiveScore
        {
            get;
            private set;
        }
        public bool GameOver
        {
            get;
            private set;
        }

        public Board(nodeState[,] values, bool turnForPlayerX, int goatsIntoBoard)
        {
            mTurnForPlayer = turnForPlayerX;
            mValues = values;
            mGoatsIntoBoard = goatsIntoBoard;
            ComputeScore();

        }

        void ComputeScore()
        {
            int score = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (mValues[i, j] == nodeState.goat)
                    {
                        score += 400 + getWeightForGoat(i, j);
                       
                    }

                }
                
            }
            score += getWeightForTigerBlocks();
            

            mScore = score;

           
        }

        int getWeightForTigerBlocks()
        {
            int surroundCount = 0;
            int possibleTigerMoves = 0;
            List<Point> tigerPoints = new List<Point>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (mValues[i, j] == nodeState.tiger)
                    {
                        int shortCount = 0;
                        int longCount = 0;
                        tigerPoints.Add(new Point(i, j));
            

                        foreach (var adjacentPoint in GetAdjacentPoints(new Point(i, j)))
                        {
                            if (mValues[adjacentPoint.X, adjacentPoint.Y] != nodeState.none)
                            {
                                surroundCount += 40;
                            }
                            else
                            {
                                possibleTigerMoves++;
                            }
                        }
                        foreach (var jumpingPoint in GetJumpingPoints(new Point(i, j)))
                        {
                            if (mValues[jumpingPoint.X, jumpingPoint.Y] != nodeState.none)
                            {
                                surroundCount += 40;
                            }
                            else
                            {
                                surroundCount -= 80;
                                possibleTigerMoves++;
                            }
                        }
                        
                    }
                    
                }
            }
            int p1, p2;
            if (tigerPoints.Count == 3)
            {
                for (int l = 0; l < 3; l++)
                {
                    p1 = l;
                    p2 = l + 1;
                    if (p2 == 3)
                    {
                        p2 = 0;
                    }
                    if (tigerPoints.ElementAt(p1).X - tigerPoints.ElementAt(p2).X == 1 || tigerPoints.ElementAt(p1).X - tigerPoints.ElementAt(p2).X == -1)
                    {
                        if (tigerPoints.ElementAt(p1).Y - tigerPoints.ElementAt(p2).Y == 1 || tigerPoints.ElementAt(p1).Y - tigerPoints.ElementAt(p2).Y == -1)
                        {
                            //System.Diagnostics.Debug.WriteLine("forting detected");
                            surroundCount += 300;
                        }
                    }

                }
            }
            
            
            if (possibleTigerMoves == 0)
            {
                surroundCount += 10000;
            }
            return surroundCount;
        }

        int getWeightForGoat(int i, int j)
        {

            switch (i)
            {
                case 0:
                    return 4;
                case 1:
                    switch (j)
                    {
                        case 0:
                        case 5:
                            return 1;
                        case 1:
                        case 4:
                            return 3;
                        case 2:
                        case 3:
                            return 4;
                        default:
                            return 1;
                    }
                case 2:
                    switch (j)
                    {
                        case 0:
                        case 5: return 1;
                        case 1:
                        case 4:
                            return 3;
                        case 2:
                        case 3: return 4;
                            
                        default: return 1;
                    }
                case 3:
                    switch (j)
                    {
                        case 0:
                        case 5: return 1;
                        case 1:
                        case 4: return 2;
                        case 2:
                        case 3: return 3;
                        default: return 1;
                    }
                case 4:
                    return 1;
                default:
                    return 1;
            }
        }

        public bool IsTerminalNode()
        {
            if (GameOver)
                return true;
            //if all entries are set, then it is a leaf node
            //foreach (nodeState v in mValues)
            //{
            //    if (v == nodeState.none)
            //        return false;
            //}
            return false;
        }

        public int MiniMax(int depth, bool needMax, out Board childWithMax)
        {
            childWithMax = this;
            String animal = "goat";
            if (mTurnForPlayer)
            {
                animal = "tiger";
            }
            //System.Diagnostics.Debug.Assert(mTurnForPlayer == needMax);
            if (depth == 0 || IsTerminalNode())
            {
                // System.Diagnostics.Debug.WriteLine("TERMINAL NODE REACHED");
                RecursiveScore = mScore;
                
               // System.Diagnostics.Debug.WriteLine("#" + animal+  "needmax" + needMax + "depth:" + depth + "recursive" + RecursiveScore);

                return mScore;
            }

            //System.Diagnostics.Debug.WriteLine("BEFORE GETTING CHILDREN");
            int countofchild = 0;
            int bestValue = 0;
            if (needMax)
            {
                bestValue = int.MinValue;
                foreach (Board cur in GetChildren())
                {
                    countofchild++;
                    //System.Diagnostics.Debug.WriteLine("CHILDREN" + ++countofchild);
                    Board dummy;
                    int score = cur.MiniMax(depth - 1, !needMax, out dummy);
                    
                    if (bestValue < score)
                    {
                        bestValue = score;
                        childWithMax = cur;
                    }
                    //System.Diagnostics.Debug.WriteLine("score:" + score + "needmax" + needMax + "depth:" + depth + "bestvalue" + bestValue);
                }
            }
            else
            {
                bestValue = int.MaxValue;
                foreach (Board cur in GetChildren())
                {
                    countofchild++;
                    //System.Diagnostics.Debug.WriteLine("CHILDREN" + ++countofchild);
                    Board dummy;
                    int score = cur.MiniMax(depth - 1, !needMax, out dummy);
                    
                    if (bestValue >= score)
                    {
                        bestValue = score;
                        childWithMax = cur;
                    }
                    
                }
            }

            if (countofchild == 0)
            {
                if (needMax)
                {
                    RecursiveScore = -mScore;
                }
                else
                {
                    RecursiveScore = 20000 + depth;
                    ////RecursiveScore = 1000;
                }
                //System.Diagnostics.Debug.WriteLine("NO CHILDREN FOR THIS BOARD");
                childWithMax = this;
                //System.Diagnostics.Debug.WriteLine(animal + "needmax" + needMax + "depth:" + depth + "recursive" + RecursiveScore);

                return RecursiveScore;
                
            }

            RecursiveScore = bestValue;

            //System.Diagnostics.Debug.WriteLine("CHILDREN FOR THIS BOARD");
            //System.Diagnostics.Debug.WriteLine(animal + "needmax" + needMax + "depth:" + depth + "recursive" + RecursiveScore);
            return RecursiveScore;
        }


        //public int MiniMax(int depth, bool needMax, int alpha, int beta, out Board childWithMax)
        //{
        //    childWithMax = null;
        //    //System.Diagnostics.Debug.Assert(mTurnForPlayer == needMax);
        //    if (depth == 0 || IsTerminalNode())
        //    {
        //       // System.Diagnostics.Debug.WriteLine("TERMINAL NODE REACHED");
        //        RecursiveScore = mScore;
        //        return mScore;
        //    }

        //    //System.Diagnostics.Debug.WriteLine("BEFORE GETTING CHILDREN");
        //    int countofchild = 0;
        //    foreach (Board cur in GetChildren())
        //    {
        //        //System.Diagnostics.Debug.WriteLine("CHILDREN" + ++countofchild);
        //        Board dummy;
        //        int score = cur.MiniMax(depth - 1, !needMax, alpha, beta, out dummy);
        //        System.Diagnostics.Debug.WriteLine("score:" + score+ "alpha:" + alpha + "beta" + beta + "needmax" + needMax + "depth:" + depth);
        //        if (!needMax)
        //        {
        //            if (beta > score)
        //            {
        //                beta = score;
        //                childWithMax = cur;
        //                if (alpha >= beta)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (alpha < score)
        //            {
        //                alpha = score;
        //                childWithMax = cur;
        //                if (alpha >= beta)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    if (countofchild == 0)
        //    {
        //        RecursiveScore = mScore;
        //        return RecursiveScore;
        //        //System.Diagnostics.Debug.WriteLine("NO CHILDREN FOR THIS BOARD");
        //    }

        //    RecursiveScore = needMax ? alpha : beta;
        //    return RecursiveScore;
        //}

        IEnumerable<Point> GetJumpingPoints(Point toBeMovedPuck)
        {
            switch (toBeMovedPuck.X)
            {
                case 0:
                    yield return (new Point(2,1));
                    yield return (new Point(2, 2));
                    yield return (new Point(2, 3));
                    yield return (new Point(2, 4));
                    
                    break;
                case 1:
                    switch (toBeMovedPuck.Y)
                    {
                        case 0:
                            yield return (new Point(1, 2));
                            yield return (new Point(3, 0));
                            break;
                        case 1:
                            
                            yield return (new Point(1, 3));
                            yield return (new Point(3, 1));
                            break;
                        case 2:
                        case 3:
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y - 2));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y + 2));
                            yield return (new Point(toBeMovedPuck.X + 2, toBeMovedPuck.Y));
                            break;
                        
                        case 4:
                            
                            yield return (new Point(1, 2));
                            yield return (new Point(3, 4));
                            break;
                        case 5:
                            yield return (new Point(1, 3));
                            yield return (new Point(3, 5));
                            break;
                    }
                    break;
                case 2:
                    switch (toBeMovedPuck.Y)
                    {
                        case 0:
                            yield return (new Point(2,2));
                            break;
                        case 1:
                            yield return (new Point(0, 0));
                            yield return (new Point(4, 1));
                            yield return (new Point(2, 3));
                            break;
                        case 2:
                        case 3:
                            yield return (new Point(0, 0));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y + 2));
                            yield return (new Point(toBeMovedPuck.X + 2, toBeMovedPuck.Y));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y-2));
                            break;
                        case 4:
                            yield return (new Point(0, 0));
                            yield return (new Point(2, 2));
                            yield return (new Point(4, 4));
                            break;
                        case 5:
                            yield return (new Point(2, 3));
                            break;
                    }
                    break;
                case 3:
                    switch (toBeMovedPuck.Y)
                    {
                        case 0:
                            yield return (new Point(3, 2));
                            yield return (new Point(1, 0));
                            break;
                        case 1:
                            yield return (new Point(1, 1));
                            yield return (new Point(3, 3));
                            break;
                        case 2:
                        case 3:
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y - 2));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y + 2));
                            yield return (new Point(toBeMovedPuck.X-2, toBeMovedPuck.Y ));
                            break;
                            
                        case 4:
                            yield return (new Point(3, 2));
                            yield return (new Point(1, 4));
                            break;
                        case 5:
                            yield return (new Point(1, 5));
                            yield return (new Point(3, 3));
                            break;

                    }
                    break;
                case 4:
                    switch (toBeMovedPuck.Y)
                    {
                        case 1:
                            yield return (new Point(2, 1));
                            yield return (new Point(4, 3));
                            break;
                        case 2:
                            yield return (new Point(2, 2));
                            yield return (new Point(4, 4));
                            break;
                        case 3:
                            yield return (new Point(2, 3));
                            yield return (new Point(4, 1));
                            break;
                        case 4:
                            yield return (new Point(2, 4));
                            yield return (new Point(4, 2));
                            break;
                    }
                    break;

            }
        }
        IEnumerable<Point> GetAdjacentPoints(Point toBeMovedPuck)
        {
            switch (toBeMovedPuck.X)
            {
                case 0:
                    yield return (new Point(1,1));
                    yield return (new Point(1, 2));
                    yield return (new Point(1, 3));
                    yield return (new Point(1, 4));
                    break;
                case 1:
                    switch (toBeMovedPuck.Y)
                    {
                        case 0:
                            yield return (new Point(1,1));
                            yield return (new Point(2,0));
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            yield return (new Point(0, 0));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y-1));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y + 1));
                            yield return (new Point(toBeMovedPuck.X+1, toBeMovedPuck.Y));
                            break;
                        case 5:
                            yield return (new Point(1,4));
                            yield return (new Point(2,5));
                            break;
                    }
                    break;
                case 2:
                    switch (toBeMovedPuck.Y)
                    {
                        case 0:
                            yield return (new Point(1, 0));
                            yield return (new Point(2, 1));
                            yield return (new Point(3, 0));
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y-1));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y + 1));
                            yield return (new Point(toBeMovedPuck.X+1, toBeMovedPuck.Y));
                            yield return (new Point(toBeMovedPuck.X - 1, toBeMovedPuck.Y));
                            break;
                        case 5:
                            yield return (new Point(1, 5));
                            yield return (new Point(2, 4));
                            yield return (new Point(3, 5));
                            break;
                    }
                    break;
                case 3:
                    switch (toBeMovedPuck.Y)
                    {
                        case 0:
                            yield return (new Point(2, 0));
                            yield return (new Point(3, 1));
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y-1));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y + 1));
                            yield return (new Point(toBeMovedPuck.X+1, toBeMovedPuck.Y));
                            yield return (new Point(toBeMovedPuck.X - 1, toBeMovedPuck.Y));
                            break;
                        case 5:
                            yield return (new Point(2, 5));
                            yield return (new Point(3, 4));
                            break;
                            
                    }
                    break;
                case 4:
                    switch (toBeMovedPuck.Y)
                    {
                        case 1:
                           yield return (new Point(3, 1));
                           yield return (new Point(4, 2));
                            break;
                        case 2:
                        case 3:
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y-1));
                            yield return (new Point(toBeMovedPuck.X, toBeMovedPuck.Y + 1));
                            yield return (new Point(toBeMovedPuck.X-1, toBeMovedPuck.Y));
                            break;
                        case 4:
                            yield return (new Point(3, 4));
                           yield return (new Point(4, 3));
                            break;
                    }
                    break;
                
            }
        }

        public IEnumerable<Point> GetMovesForGoatPuck(nodeState[,] values, Point toBeMovedPuck)
        {
            foreach (var adjacentPoint in GetAdjacentPoints(toBeMovedPuck))
            {
                if (values[adjacentPoint.X, adjacentPoint.Y] == nodeState.none)
                {
                    yield return adjacentPoint;
                }
            }
            

        }

        public IEnumerable<Point> GetShortMovesForTigerPuck(nodeState[,] values, Point toBeMovedPuck)
        {
            foreach (var adjacentPoint in GetAdjacentPoints(toBeMovedPuck))
            {
                if (values[adjacentPoint.X, adjacentPoint.Y] == nodeState.none)
                {
                    yield return adjacentPoint;
                }
            }
        }

        Point GetMiddlePoint(Point one, Point two)
        {
            if (one.X > two.X)
            {
                return new Point(one.X - 1, one.Y);
            }
            else if (one.X < two.X)
            {
                return new Point(two.X - 1, two.Y);
            }
            else
            {
                if ( one.Y > two.Y )
                {
                    return new Point(one.X, one.Y-1);
                }
                else{
                    return new Point(two.X, two.Y - 1);
                }
            }
        }

        public IEnumerable<Point> GetCaptureMovesForTigerPuck(nodeState[,] values, Point toBeMovedPuck)
        {
            foreach (var jumpingPoint in GetJumpingPoints(toBeMovedPuck))
            {
                if (values[jumpingPoint.X, jumpingPoint.Y] == nodeState.none)
                {
                    Point middlePoint = GetMiddlePoint(jumpingPoint, toBeMovedPuck);
                    if (values[middlePoint.X,middlePoint.Y] == nodeState.goat)
                    {
                        yield return jumpingPoint;
                    }
                    
                }
            }
        }

        public Board FindNextMove(int depth)
        {
            Board ret = null;
           
           
           // MiniMax(depth, !mTurnForPlayer, int.MinValue + 1, int.MaxValue - 1, out ret);
            MiniMax(depth, !mTurnForPlayer,  out ret);
            if (ret == null)
            {
               // System.Diagnostics.Debug.WriteLine("null at ret");
            }
            System.Diagnostics.Debug.WriteLine("Score:" + mScore);
            
            return ret;
        }

        public Board MakeMove(Point from,Point to)
        {
            
            if (!mTurnForPlayer)
            {//if turn for player goat, get all moves for goat
                if (mGoatsIntoBoard < 15)
                {
                    
                            if (mValues[to.X, to.Y] == nodeState.none)
                            {
                                nodeState[,] newValues = (nodeState[,])mValues.Clone();
                                newValues[to.X, to.Y] = nodeState.goat;
                                return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard + 1);
                            }
                    
                   
                }
                else
                {                   
                            if (mValues[from.X, from.Y] == nodeState.goat)
                            {
                                    nodeState[,] newValues = (nodeState[,])mValues.Clone();
                                    newValues[from.X, from.Y] = nodeState.none;
                                    newValues[to.X, to.Y] = nodeState.goat;
                                    return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard);
                            }  
                }
            }
            else
            {
                        if (mValues[from.X,from.Y] == nodeState.tiger)
                        {
                            

                            
                                nodeState[,] newValues = (nodeState[,])mValues.Clone();
                                newValues[from.X, from.Y] = nodeState.none;
                                newValues[to.X, to.Y] = nodeState.tiger;
                            
                            
                            foreach (var move in GetCaptureMovesForTigerPuck(mValues, from))
                            {
                                if (move.Equals(to))
                                {
                                    Point middlePoint = GetMiddlePoint(to, from);
                                    newValues[middlePoint.X, middlePoint.Y] = nodeState.none;
                                }
                            }
                            return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard);
                        }

            }
            return new Board(mValues, mTurnForPlayer, mGoatsIntoBoard);
        
        }


        public IEnumerable<Board> GetChildren()   
        {
            if (!mTurnForPlayer)
            {//if turn for player goat, get all moves for goat
                if (mGoatsIntoBoard < 15)
                {
                    for (int i = 1; i < 4; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            if (mValues[i, j] == nodeState.none)
                            {
                                nodeState[,] newValues = (nodeState[,])mValues.Clone();
                                newValues[i, j] = nodeState.goat;
                                
                                
                                        yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard + 1);
                                
                                
                                
                                
                            }
                        }
                    }
                    for (int j = 1; j < 5; j++)
                    {
                        if (mValues[4, j] == nodeState.none)
                        {
                            nodeState[,] newValues = (nodeState[,])mValues.Clone();
                            newValues[4, j] = nodeState.goat;
                            if (!newValues.Equals(prevMove))
                            {
                                yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard + 1);
                            }
                        }
                    }
                    if (mValues[0, 0] == nodeState.none)
                    {
                        nodeState[,] newValues = (nodeState[,])mValues.Clone();
                        newValues[0, 0] = nodeState.goat;
                        if (newValues.Equals(prevMove))
                        {
                            System.Diagnostics.Debug.WriteLine("prevmove");
                        }
                        //if (!BoardHistory.isBoardEqual(mValues,))
                        {
                            yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard + 1);
                        }
                    }

                }
                else
                {

                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 6; j++)
                        {
                            if (mValues[i, j] == nodeState.goat)
                            {


                                foreach (var move in GetMovesForGoatPuck(mValues, new Point(i, j)))
                                {
                                    nodeState[,] newValues = (nodeState[,])mValues.Clone();
                                    newValues[i, j] = nodeState.none;
                                    newValues[move.X, move.Y] = nodeState.goat;

                                    {
                                        
                                  
                                        
                                            yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard);
                                        
                                    }
                                }


                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (mValues[i, j] == nodeState.tiger)
                        {
                            
                            Point toBeMovedTigerPuck = new Point(i,j);
                            foreach (var move in GetShortMovesForTigerPuck(mValues, toBeMovedTigerPuck))
                            {
                                nodeState[,] newValues = (nodeState[,])mValues.Clone();
                                newValues[i, j] = nodeState.none;
                                newValues[move.X, move.Y] = nodeState.tiger;
                                if (mGoatsIntoBoard == 15)
                                {
                                    bool canYield = true;
                                    for (int k = BoardHistory.history.Count - 8; k < BoardHistory.history.Count; k++)
                                    {
                                        if (newValues.Equals(BoardHistory.history.ElementAt(k)))
                                        {
                                            canYield = false;
                                        }
                                    }
                                    if (canYield)
                                    {
                                        yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard);
                                    }
                                }
                                else
                                {
                                    yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard);
                                }
                                
                            }
                            foreach (var move in GetCaptureMovesForTigerPuck(mValues, toBeMovedTigerPuck))
                            {
                                
                                nodeState[,] newValues = (nodeState[,])mValues.Clone();
                                Point middlePoint = GetMiddlePoint(move, toBeMovedTigerPuck);
                                newValues[middlePoint.X, middlePoint.Y] = nodeState.none;
                                newValues[i, j] = nodeState.none;
                                newValues[move.X, move.Y] = nodeState.tiger;
                                
                                if (mGoatsIntoBoard == 15)
                                {
                                    bool canYield = true;
                                    for (int k = BoardHistory.history.Count - 8; k < BoardHistory.history.Count; k++)
                                    {
                                        if (newValues.Equals(BoardHistory.history.ElementAt(k)))
                                        {
                                            canYield = false;
                                        }
                                    }
                                    if (canYield)
                                    {
                                        yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard);
                                    }
                                }
                                else
                                {
                                    yield return new Board(newValues, !mTurnForPlayer, mGoatsIntoBoard);
                                }
                                
                            }

                        }
                    }
                }

            }
        }



    }
}
