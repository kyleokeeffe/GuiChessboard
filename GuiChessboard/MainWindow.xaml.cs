﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GuiChessboard.Models;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

//version 
//need to erase label contents as piece position chagnes
    //either redraw all labesl as empty, or 
    //set previous position label to empty <-
//1.piece class should have a position property
//then current square is derived from this,
//then current label is derived from current square 


//2. make overloaded direction method with distance limitation for pawn/king 
namespace GuiChessboard
{
    public partial class MainWindow : Window
    {
       
     
        public delegate void EventCreator(object obj, MouseButtonEventArgs e);
        
        ArrayList emptyBoardColors = new ArrayList();
        static List<Piece> piecesList = new List<Piece>();
        protected Dictionary<System.Windows.Controls.Border, Piece> availableEmptySquares;
        protected Dictionary<System.Windows.Controls.Border, PieceTake> availableOccupiedSquares;
        

     
        public MainWindow()
        {
            InitializeComponent();
       
            emptyBoardColors =  SaveEmptyBoardColors();

     

            Piece bishop1 = new Piece(PieceColour.Black, PieceType.Bishop, cell43);
            Piece bishop2 = new Piece(PieceColour.White, PieceType.Bishop, cell65);
            Piece rook1 = new Piece(PieceColour.Black, PieceType.Rook, cell55);
            Piece queen1 = new Piece(PieceColour.Black, PieceType.Queen, cell33);

            piecesList.Add(bishop1);
            piecesList.Add(bishop2);
            piecesList.Add(rook1);
            piecesList.Add(queen1);
          

       
            
            grdBoard.Loaded += PaintPieces;
          
            
            grdBoard.MouseDown += IdentifyClick;
          
           
         
            
            
        }

       

        public void IdentifyClick(object obj, MouseButtonEventArgs e)
        {
            var pieceClicked = piecesList.Find(piece => piece.CurrentLocation == (System.Windows.Controls.Border)e.Source);

            if (pieceClicked != null)
                PaintPieceMovePattern(pieceClicked);
            else if (availableEmptySquares == null)
            {
                PaintEmptyBoardColors();
                PaintPieces();
            }
            else if (availableEmptySquares.ContainsKey((System.Windows.Controls.Border)e.Source) == false)
            {
                foreach (System.Windows.Controls.Border emptySquare in availableEmptySquares.Keys)
                    emptySquare.MouseLeftButtonDown -= EmptyMoveSquareClicked;

                foreach (var pieceSquare in availableOccupiedSquares.Keys)
                    pieceSquare.MouseLeftButtonDown -= OccupiedMoveSquareClicked;

                PaintEmptyBoardColors();
                PaintPieces();
            }
        }

        public void PaintPieceMovePattern(Piece pieceClicked)
        {
            PaintEmptyBoardColors();
            PaintPieces();
            
            int directionModifier = (int)pieceClicked.Color;
            int x = pieceClicked.XPos;
            int y = pieceClicked.YPos;

            //if the new click is on another piece, remove the event listeners for the previous piece clicked
            if (availableEmptySquares != null)
            {
                foreach (System.Windows.Controls.Border emptySquare in availableEmptySquares.Keys)
                    emptySquare.MouseLeftButtonDown -= EmptyMoveSquareClicked;
            }
            
            if (availableOccupiedSquares != null)
            {
                foreach (var pieceSquare in availableOccupiedSquares.Keys)
                    pieceSquare.MouseLeftButtonDown -= OccupiedMoveSquareClicked;
            }


            //if this method has not been run before, initialize move square arrays
            availableEmptySquares = new Dictionary<System.Windows.Controls.Border, Piece>();
            availableOccupiedSquares = new Dictionary<System.Windows.Controls.Border, PieceTake>();

            ArrayList patternSquares= MovementPattern.CreatePattern(grdBoard,pieceClicked);
            availableEmptySquares = (Dictionary<System.Windows.Controls.Border, Piece>)patternSquares[0];
            availableOccupiedSquares = (Dictionary<System.Windows.Controls.Border, PieceTake>)patternSquares[1];

            //paint squares
            if (availableEmptySquares != null)
            {
                foreach (System.Windows.Controls.Border emptySquare in availableEmptySquares.Keys)
                {
                    emptySquare.Background = System.Windows.Media.Brushes.Green;
                    emptySquare.MouseLeftButtonDown += EmptyMoveSquareClicked;  
                }
            }

            if (availableOccupiedSquares != null)
            {
                 foreach (var pieceSquare in availableOccupiedSquares.Keys)
                    {
                        pieceSquare.Background = System.Windows.Media.Brushes.Orange;
                        pieceSquare.MouseLeftButtonDown += OccupiedMoveSquareClicked;
                    }
            }
        }

        public void EmptySquareClicked(object obj, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Border thisSquare = (System.Windows.Controls.Border)obj;
            Piece originatingPiece = availableEmptySquares.GetValueOrDefault(thisSquare);

            originatingPiece.CurrentLocation = thisSquare;

            PaintEmptyBoardColors();
            PaintPieces();
        }

        public void EmptyMoveSquareClicked(object obj, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Border thisSquare = (System.Windows.Controls.Border)obj;
            Piece originatingPiece = availableEmptySquares.GetValueOrDefault(thisSquare);
          
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////need reference to child of current location if its a label(if it has content property)


            originatingPiece.CurrentLocation = thisSquare;

            PaintEmptyBoardColors();
            PaintPieces();
        }

        public void OccupiedMoveSquareClicked(object obj, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Border thisSquare = (System.Windows.Controls.Border)obj;
            PieceTake thisPiecetake = availableOccupiedSquares.GetValueOrDefault(thisSquare);
           
            thisPiecetake.OriginatingPiece.CurrentLocation = thisSquare;

            piecesList.Remove(thisPiecetake.DestinationPiece);
            PaintEmptyBoardColors();
            PaintPieces();
         
            System.Windows.MessageBox.Show($"{thisPiecetake.OriginatingPiece.Color} {thisPiecetake.OriginatingPiece.Name} takes {thisPiecetake.DestinationPiece.Color} {thisPiecetake.DestinationPiece.Name}");
        }

        public static Piece CheckSquareForPiece(Position position)
        {
            //check if theres a piece on it
            Piece thisPiece;
            try
            {
                thisPiece = piecesList.Find(x => x.XPos == position.X && x.YPos == position.Y);
            }
            catch (Exception)
            {
                thisPiece = null;
            }
            return thisPiece;
        }
        
        public void PaintPieces(Object obj, EventArgs e)
        {
            for(int i = 0; i< piecesList.Count; i++)
            {
                var thisPiece = piecesList[i];
                var thisPosition = new Position(thisPiece.XPos, thisPiece.YPos);
                var thisLabel = Position.GetPositionLabel(grdBoard, thisPosition);
                var thisColor = piecesList[i].Color.ToString();
                SolidColorBrush colorConvertor = (SolidColorBrush)new BrushConverter().ConvertFromString(thisColor);

                thisPiece.CurrentLocation.Background = colorConvertor;
                thisLabel.Content = thisPiece.Name.ToString();
                
             
            }

          
        }
    
     


   
        

         public void PaintPieces() //why a parameterless? so it runs first round without needing to click on somethign first 
         {
            
             for (int i = 0; i < piecesList.Count; i++)
             {
                var thisPiece = piecesList[i];
                var thisPosition = new Position(thisPiece.XPos, thisPiece.YPos);
                var thisLabel = Position.GetPositionLabel(grdBoard, thisPosition);

                var thisColor = piecesList[i].Color.ToString();
                 SolidColorBrush colorConvertor = (SolidColorBrush)new BrushConverter().ConvertFromString(thisColor);

                 piecesList[i].CurrentLocation.Background = colorConvertor;
                thisLabel.Content = thisPiece.Name.ToString();

            }
        }



        public ArrayList SaveEmptyBoardColors()
        {
            ArrayList emptyBoardColors = new ArrayList();
            foreach (var thing in grdBoard.Children)
            {
                if (thing.GetType() == typeof(System.Windows.Controls.Border))
                {
                    System.Windows.Controls.Border thisThing = (System.Windows.Controls.Border)thing;

                    //if(thisThing.Background!=null)
                        emptyBoardColors.Add(thisThing.Background);
                }
                else
                    emptyBoardColors.Add(thing);
            }
            return emptyBoardColors;
        }
            
        public void PaintEmptyBoardColors()
        {
            for(int i =0;i< grdBoard.Children.Count; i++)
            {
                var previousColor = emptyBoardColors[i];

               
                if (previousColor.GetType() == typeof(System.Windows.Media.SolidColorBrush))
                {
                    System.Windows.Controls.Border thing = (System.Windows.Controls.Border)grdBoard.Children[i];
                    thing.Background = (System.Windows.Media.Brush)previousColor;
                }
            }
        }
    }
} 
