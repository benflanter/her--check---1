using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;


namespace ChineseCheckers.Model
{
    public class ComputerPlayer : Player
    {
        Graph graph;
        private Piece scannedPiece;

        public ComputerPlayer(bool side, Board board) : base(side, board)
        {
            //graph =  new Graph(this.board);

        }

        public void CreateGraph()
        {
            graph.CreateGraph();
        }
        private Move ChooseMove()
        {
            List<Move> moves = GetMoves();
            int index = Heuristic(moves);
            return moves[index];
        }

        private int Heuristic(List<Move> moves)
        {
            int index = -1;
            Dictionary<int, Piece> Destinations = GetDestinations();
            int count = CountOutsidePieces();
            if (count == 1)
            {
                Piece OriginPiece = scannedPiece;
                Piece DestinationPiece = null;
                foreach (KeyValuePair<int, Piece> dest in Destinations)
                {
                    if (dest.Value != null)
                    {
                        DestinationPiece = dest.Value;
                        break;
                    }
                }

                moves = GetMovesForPiece(OriginPiece);
                index = GetShortestPath(moves, DestinationPiece);
                return index;
            }
            else
            {
                scannedPiece = null;
                if ((index = GetSMoves(moves)) != -1)
                    return index;

                else if (getPiece(13, 9) != null || getPiece(13, 15) != null)
                {
                    index = -1;
                    foreach (var move in moves)
                    {
                        index++;
                        Piece piece = move.GetOrigin();
                        if ((piece.row == 13 && piece.col == 9) || (piece.row == 13 && piece.col == 15))
                            return index;
                    }
                }
                else if ((index = LongestJump(moves)) != -1)
                    return index;
                else
                {
                    index = -1;
                    foreach (var move in moves)
                    {
                        index++;
                        Piece piece = move.GetOrigin();
                        int key = move.GetRow() * Board.WIDTH + move.GetCol();
                        if (Destinations.ContainsKey(key))
                        {
                            if(getPiece(move.GetRow(), move.GetCol()) == null)
                            {
                                if (Board.initmat[piece.row, piece.col] != 2)
                                    return index;
                            }
                        }
                        if (IsForwardAndCentralMove(piece.row, piece.col, move.GetRow(), move.GetCol()))
                            return index;
                        else if (piece.row > move.GetRow() && ((piece.col >= 9 || piece.col <= 15) || move.GetCol() >= 9 || move.GetCol() <= 15))
                        {
                            return index;
                        }
                        else if (move.GetCol() >= 9 || move.GetCol() <= 15)
                        {
                            return index;
                        }
                        else if (piece.row < move.GetRow())
                        {
                            return index;
                        }
                    }

                }
                return -1;
            }
        }

        private int CountOutsidePieces()
        {
            int count = 0;
            foreach (KeyValuePair<int, Piece> piece in pieces)
            {
                scannedPiece = piece.Value;
                if (Board.initmat[piece.Value.row, piece.Value.col] != 2)
                {
                    scannedPiece = piece.Value;
                    count++;
                }
            }
            return count;
        }

        int GetShortestPath(List<Move> moves, Piece destination)
        {
            int shortestPath = int.MaxValue;
            int shortestPathIndex = -1;
            for (int i = 0; i < moves.Count; i++)
            {
                int pathLength = Math.Abs(moves[i].GetOrigin().row - destination.row) + Math.Abs(moves[i].GetOrigin().col - destination.col);
                if (pathLength < shortestPath)
                {
                    shortestPath = pathLength;
                    shortestPathIndex = i;
                }
            }
            return shortestPathIndex;
        }


        //private int GetShortestPath(List<Move> moves, Piece piece)
        //{
        //    int index = -1;
        //    int currentIndex = -1;
        //    int min = -1;
        //    foreach (var move in moves)
        //    {
        //        currentIndex++;
        //        if (board.MoveAble(piece, move.GetRow(), move.GetCol()))
        //            return currentIndex;
        //        board.clearHelpMat();
        //        scannedPiece = piece;
        //        int PathCount = 0;
        //        GetPathCount(move.GetOrigin(), PathCount);
        //        if (min == -1 || PathCount < min)
        //        {
        //            min = PathCount;
        //            index = currentIndex;
        //        }
        //    }

        //    return index;
        //}



        //private bool GetPathCount(Piece piece, int num)
        //{

        //    if (piece.row == scannedPiece.row && piece.col == scannedPiece.col)
        //        return true;
        //    if (!Islegal(piece.row, piece.col) || Board.initmat[piece.row, piece.col] == 0 || board.helpmat[piece.row, piece.col] == 1)
        //        return false; ;
        //    board.helpmat[piece.row, piece.col] = 1;
        //    for (int i = 0; i < board.directions.Length / 2; i++)
        //    {
        //        int row = piece.row + board.directions[i, 0];
        //        int col = piece.col + board.directions[i, 1];
        //        if (Islegal(row, col) && board.getPiece(row, col) == null)
        //        {
        //            if (GetPathCount(new Piece(row, col), 1 + num))
        //                return true;
        //        }
        //    }
        //    return false;

        //}


        private Dictionary<int, Piece> GetDestinations()
        {
            Dictionary<int, Piece> Destinations = new Dictionary<int, Piece>();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 9; j < 16; j++)
                {
                    if (Board.initmat[i, j] == 2)
                    {
                        if (getPiece(i, j) == null)
                            Destinations.Add(i * Board.WIDTH + j, new Piece(i, j));
                        else
                            Destinations.Add(i * Board.WIDTH + j, null);
                    }
                }
            }
            return Destinations;
        }

        private int GetSMoves(List<Move> moves)
        {
            int index = -1;
            foreach (var move in moves)
            {
                index++;
                Piece piece = move.GetOrigin();
                if (piece.row - 4 == move.GetRow() && piece.col == move.GetCol())
                    return index;
            }
            return -1;
        }


        private int LongestJump(List<Move> moves)
        {
            double longest = 0;
            int currentIndex = -1;
            int index = -1;
            foreach (var move in moves)
            {
                currentIndex++;
                Piece piece = move.GetOrigin();
                int x = (int)Math.Pow(Math.Abs(piece.row - move.GetRow()), 2);
                int y = (int)Math.Pow(Math.Abs(piece.col - move.GetCol()), 2);
                double length = Math.Sqrt(x + y);
                if (longest == 0)
                {
                    longest = length;
                    index = currentIndex;
                }
                else if (piece.row > move.GetRow())
                {
                    if (length > longest)
                    {
                        longest = length;
                        index = currentIndex;
                    }
                }
            }
            return index;
        }


        public bool IsForwardAndCentralMove(int row, int col, int rowDest, int colDest)
        {
            if (rowDest < row)
            {
                int center = Board.WIDTH / 2;
                if (Math.Abs(center - colDest) <= Math.Abs(center - col))
                    return true;
            }
            return false;
        }



        public void MakeMove()
        {
            Move move = ChooseMove();
            if (move != null)
            {
                removePiece(move.GetOrigin());
                addPiece(move.GetRow(), move.GetCol(), this.side);
            }
        }
    }
}
