using System.Collections;
using System.Collections.Generic;
using System.Net;
using System;
using UnityEngine;
using Checkers.grid;
using Unity.VisualScripting;
using System.Linq;
using static System.Diagnostics.Stopwatch;

namespace Checkers.controller
{
    public class MovementController : MonoBehaviour
    {
        //GridManager gridManager;

        [SerializeField] private GameObject _potentialMoveHighlight;

        public List<GameObject> _highlightList;
        
        bool confirmMove = false;

        public List<(GameObject obj, int path, int move, GameObject removedPiece)> _pathlist;

        private int pathnum = 0;

        GameObject _selectedPiece = null;
        GameObject _selectedPieceHighLight = null;

        bool isBlackTurn;


        void Start()
        {
            isBlackTurn = true;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RemovePreviouslyAllowedMoves(); // removes the valid moves and the highlights displaying for them when de-selecting a piece or selecting a new piece
                _pathlist = new List<(GameObject, int, int, GameObject)>();
                Clicked();
            }

            if (Input.GetMouseButtonDown(1) && confirmMove)
            {
                Debug.Log("Moved");
                getMoveSpot();
                RemovePreviouslyAllowedMoves();
                _pathlist = new List<(GameObject, int, int, GameObject)>();
            }
        }

        void swapTurns() // for turn based functionality
        {
            if(_selectedPiece.transform.name == "playerPiece(Clone)")
            {
                isBlackTurn = false;
            } else if(_selectedPiece.transform.name == "enemyPiece(Clone)")
            {
                isBlackTurn = true;
            } else
            {
                return;
            }
        }

        void getMoveSpot()
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            var _selectedMoveLocation = hit.collider.transform;
            Debug.Log("move:" + _selectedMoveLocation.name);

            float newX, newY;

            newX = _selectedMoveLocation.position.x;
            newY = _selectedMoveLocation.position.y;

            // if move is valid, execute move, add check here
            if (_selectedMoveLocation.name.Contains("potentialMoveHighlight"))
            {
                _selectedPiece.transform.position = new Vector2(newX, newY);
                _selectedPieceHighLight.SetActive(false);
            }

            swapTurns();
            removeEnemyPieces(_selectedMoveLocation);

        }

        void Clicked()
        {
            try
            {
                _selectedPieceHighLight.SetActive(false); // attempts to removes previous selection highlight
                confirmMove = false;
            }
            catch (Exception)
            {
                // if nothing was previously selected
            }

            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider.name == "playerPiece(Clone)" && isBlackTurn)
            {
                _selectedPiece = hit.collider.transform.gameObject;
                _selectedPieceHighLight = hit.collider.transform.GetChild(0).gameObject;
                _selectedPieceHighLight.SetActive(true);

                confirmMove = true;
                pathnum = 0;
                CalculateAllowedMoves(_selectedPiece, true, false, _selectedPiece.transform.position.x, _selectedPiece.transform.position.y);

            }
            else if (hit.collider.name == "enemyPiece(Clone)" && !isBlackTurn)
            {
                _selectedPiece = hit.collider.transform.gameObject;
                _selectedPieceHighLight = hit.collider.transform.GetChild(0).gameObject;
                _selectedPieceHighLight.SetActive(true);

                confirmMove = true;
                pathnum = 0;
                CalculateAllowedMovesForEnemyPiece(_selectedPiece, false, false, _selectedPiece.transform.position.x, _selectedPiece.transform.position.y);
            }
            
        }

        private (bool, GameObject, bool) CheckForJump(Vector2 attemptedMove)
        {
            if(attemptedMove.x >= 8 || attemptedMove.y >= 8)
            {
                return (false, null, false);
            }

            RaycastHit2D hit;

            hit = Physics2D.Raycast(attemptedMove, Vector2.down);

            try
            {
                if (hit.transform.name == "playerPiece(Clone)")
                {
                    return (false, hit.transform.gameObject, false);

                } else if (hit.transform.name == "enemyPiece(Clone)") {

                    return (true, hit.transform.gameObject, false);

                }
            }
            catch (System.Exception) // if caught, move was off the board, making it invalid
            {
                return (false, null, false);
            }

            return (false, hit.transform.gameObject, true);
        }

        int counter = 0;
        private void CalculateAllowedMoves(GameObject piece, bool isPlayerPiece, bool hitEnemyPiece, float currentX, float currentY)
        {
            //Debug.Log(GridManager._pieceList);
            var _allowedMoveList = new List<Vector2>();
            Vector2 currentPiecePos = piece.transform.position;
            

            for(int i = -1; i < 2; i ++)
            {
                Vector2 move = piece.transform.position;
                if(isPlayerPiece) // maybe make seperate function for calculating enemy allowed moves??
                {
                    move.y = currentPiecePos.y + 1;
                    move.x = move.x + i;
                }
                else
                {
                    move.y = currentPiecePos.y - 1;
                    move.x = move.x + i;
                }

                // checks if move is actually on a diagonal before considering it a valid move
                if((Math.Abs(move.x - currentX)) !=  Math.Abs(move.y - currentY))
                {
                    continue;
                }

                // checks original position compared to current to determine if position is on the same paths on the board
                if (currentPiecePos.x == _selectedPiece.transform.position.x && currentPiecePos.y == _selectedPiece.transform.position.y) 
                {
                    pathnum++;
                }

                var jumpSpots = CheckForJump(move);

                if (jumpSpots.Item1 && !hitEnemyPiece)
                {
                    /* Recursion is used here because in checkers, one could theoretically jump over just about every single enemy piece.
                        * making it necessary to call this function over and over in order to find each path out of multiple potential jump paths
                        * the user could decide on. */
                    CalculateAllowedMoves(jumpSpots.Item2, true, true, currentX, currentY); 

                }
                else if(jumpSpots.Item3 && !piece.transform.name.Contains("potentialMoveHighlight"))
                {

                    _allowedMoveList.Add(move);
                    GameObject potentialMoveHighlight = Instantiate(_potentialMoveHighlight, new Vector2(move.x, move.y), Quaternion.identity);
                    potentialMoveHighlight.name = "potentialMoveHighlight" + counter.ToString();
                    counter += 1;
                    _highlightList.Add(potentialMoveHighlight);

                    if(hitEnemyPiece)
                    {
                        _pathlist.Add((potentialMoveHighlight, pathnum, counter, piece));
                        CalculateAllowedMoves(potentialMoveHighlight, true, false, potentialMoveHighlight.transform.position.x, potentialMoveHighlight.transform.position.y);
                    }
                }
            }
        }



        private (bool, GameObject, bool) CheckForJumpForEnemyPiece(Vector2 attemptedMove)
        {
            if (attemptedMove.x >= 8 || attemptedMove.y >= 8)
            {
                return (false, null, false);
            }

            RaycastHit2D hit;

            hit = Physics2D.Raycast(attemptedMove, Vector2.down);

            try
            {
                if (hit.transform.name == "enemyPiece(Clone)")
                {
                    return (false, hit.transform.gameObject, false);

                }
                else if (hit.transform.name == "playerPiece(Clone)")
                {

                    return (true, hit.transform.gameObject, false);

                }
            }
            catch (System.Exception) // if caught, move was off the board, making it invalid
            {
                return (false, null, false);
            }

            return (false, hit.transform.gameObject, true);
        }


        private void CalculateAllowedMovesForEnemyPiece(GameObject piece, bool isPlayerPiece, bool hitEnemyPiece, float currentX, float currentY)
        {
            //Debug.Log(GridManager._pieceList);
            var _allowedMoveList = new List<Vector2>();
            Vector2 currentPiecePos = piece.transform.position;


            for (int i = -1; i < 2; i++)
            {
                Vector2 move = piece.transform.position;
                if (isPlayerPiece) // maybe make seperate function for calculating enemy allowed moves??
                {
                    move.y = currentPiecePos.y + 1;
                    move.x = move.x + i;
                }
                else
                {
                    move.y = currentPiecePos.y - 1;
                    move.x = move.x + i;
                }

                // checks if move is actually on a diagonal before considering it a valid move
                if ((Math.Abs(move.x - currentX)) != Math.Abs(move.y - currentY))
                {
                    continue;
                }

                // checks original position compared to current to determine if position is on the same paths on the board
                if (currentPiecePos.x == _selectedPiece.transform.position.x && currentPiecePos.y == _selectedPiece.transform.position.y)
                {
                    pathnum++;
                }

                var jumpSpots = CheckForJumpForEnemyPiece(move);

                if (jumpSpots.Item1 && !hitEnemyPiece)
                {
                    /* Recursion is used here because in checkers, one could theoretically jump over just about every single enemy piece.
                        * making it necessary to call this function over and over in order to find each path out of multiple potential jump paths
                        * the user could decide on. */
                    CalculateAllowedMovesForEnemyPiece(jumpSpots.Item2, false, true, currentX, currentY);

                }
                else if (jumpSpots.Item3 && !piece.transform.name.Contains("potentialMoveHighlight"))
                {

                    _allowedMoveList.Add(move);
                    GameObject potentialMoveHighlight = Instantiate(_potentialMoveHighlight, new Vector2(move.x, move.y), Quaternion.identity);
                    potentialMoveHighlight.name = "potentialMoveHighlight" + counter.ToString();
                    counter += 1;
                    _highlightList.Add(potentialMoveHighlight);

                    if (hitEnemyPiece)
                    {
                        _pathlist.Add((potentialMoveHighlight, pathnum, counter, piece));
                        CalculateAllowedMovesForEnemyPiece(potentialMoveHighlight, false, false, potentialMoveHighlight.transform.position.x, potentialMoveHighlight.transform.position.y);
                    }
                }
            }
        }


        private void RemovePreviouslyAllowedMoves() // removes the highlights representing a valid move when the user deselects a piece or selects a new piece
        {
            for (int i = 0; i < _highlightList.Count; i++)
            {
                Destroy(_highlightList[i]);

            }
            _highlightList.RemoveAll(x => x);
        }


        private void removeEnemyPieces(Transform moveName)
        {

            // improvements need to be made to the time complexity of this function, for now it is okay

            for (int i = 0; i < _pathlist.Count; i++)
            {
                if (moveName.name == _pathlist[i].obj.name)
                {
                    var chosenMove = _pathlist[i];
                    for (int j = 0; j < _pathlist.Count; j++)
                    {
                        if (_pathlist[j].path == chosenMove.path && chosenMove.move >= _pathlist[j].move) // checks if piece is on same path before removing
                        {
                            try
                            {
                                // if y values are equal choose piece closer to position of move, gross solution but only thing I could come up with for this weird outlier case
                                if (_pathlist[j + 1].removedPiece.transform.position.y == _pathlist[j].removedPiece.transform.position.y)
                                {
                                    if (Math.Abs(_pathlist[j].removedPiece.transform.position.x - moveName.position.x) != 1)
                                    {
                                        Destroy(_pathlist[j + 1].removedPiece);
                                        continue;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                Destroy(_pathlist[j].removedPiece);
                            }

                            Destroy(_pathlist[j].removedPiece);
                        }
                    }
                }
            }

            /*for (int j = 0; j < _pathlist.Count; j++)
            {
                try
                {
                    if (moveName.name == _pathlist[j].obj.name)
                    {
                        try
                        {
                            // if y values are equal choose piece closer to position of move, gross solution but only thing I could come up with for this weird outlier case
                            if (_pathlist[j + 1].removedPiece.transform.position.y == _pathlist[j].removedPiece.transform.position.y)
                            {
                                if (Math.Abs(_pathlist[j].removedPiece.transform.position.x - moveName.position.x) != 1)
                                {
                                    Destroy(_pathlist[j + 1].removedPiece);
                                    continue;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            Destroy(_pathlist[j].removedPiece);
                        }
                    }
                    else if (_pathlist[j + 1].removedPiece.transform.position.y != _pathlist[j].removedPiece.transform.position.y)
                    {
                        Destroy(_pathlist[j].removedPiece);
                    }
                }
                catch (Exception)
                {
                    Destroy(_pathlist[j - 1].removedPiece);
                }
            }*/

            /*for (int j = 0; j < _pathlist.Count; j++)
            {
                try
                {
                    bool sameY = _pathlist[j + 1].removedPiece.transform.position.y == _pathlist[j].removedPiece.transform.position.y;

                    *//*if (moveName.name == _pathlist[j].obj.name)
                    {*//*
                        // if y values are equal choose piece closer to position of move, gross solution but only thing I could come up with for this weird outlier case
                        if (sameY)
                        {
                            if (Math.Abs(_pathlist[j].removedPiece.transform.position.x - moveName.position.x) != 1)
                            {
                                Destroy(_pathlist[j+1].removedPiece);
                                continue;
                            }
                        
                    }
                    else if (!sameY)
                    {
                        Destroy(_pathlist[j].removedPiece);
                    }
                }
                catch (Exception)
                {
                    Destroy(_pathlist[j].removedPiece);
                }

            }*/


        }
    }
}
