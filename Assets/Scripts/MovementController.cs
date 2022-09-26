using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Checkers.grid;

namespace Checkers.controller
{
    public class MovementController : MonoBehaviour
    {
        //GridManager gridManager;

        [SerializeField] private GameObject _potentialMoveHighlight;

        public List<GameObject> _highlightList;

        bool confirmMove = false;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RemovePreviouslyAllowedMoves(); // removes the valid moves and the highlights displaying for them when de-selecting a piece or selecting a new piece
                Clicked();
            }

            if (Input.GetMouseButtonDown(1) && confirmMove)
            {
                
                Debug.Log("Moved");
                getMoveSpot();
                RemovePreviouslyAllowedMoves();
            }
        }

        GameObject _selectedPiece = null;
        GameObject _selectedPieceHighLight = null;
        

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
            if(_selectedMoveLocation.name == "potentialMoveHighlight(Clone)")
            {
                _selectedPiece.transform.position = new Vector2(newX, newY);
                _selectedPieceHighLight.SetActive(false);
            }

        }

        void Clicked()
        {
            try
            {
                _selectedPieceHighLight.SetActive(false); // attempts to removes previous selection highlight
                confirmMove = false;
            }
            catch (System.Exception)
            {
                // if nothing was previously selected
            }

            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider.name == "playerPiece(Clone)")
            {
                _selectedPiece = hit.collider.transform.gameObject;
                _selectedPieceHighLight = hit.collider.transform.GetChild(0).gameObject;
                _selectedPieceHighLight.SetActive(true);
                confirmMove = true;
                CalculateAllowedMoves(_selectedPiece, true);

            }
            else if (hit.collider.name == "enemyPiece(Clone)")
            {
                _selectedPiece = hit.collider.transform.gameObject;
                _selectedPieceHighLight = hit.collider.transform.GetChild(0).gameObject;
                _selectedPieceHighLight.SetActive(true);
                confirmMove = true;
                CalculateAllowedMoves(_selectedPiece, false);
            }
            
        }

        private bool CheckifPieceisPresent(Vector2 attemptedMove) // check each piece position on board to validate potential move
        {
            RaycastHit2D hit;

            hit = Physics2D.Raycast(attemptedMove, Vector2.down);

            try
            {
                if (hit.transform.name == "playerPiece(Clone)" || hit.transform.name == "enemyPiece(Clone)") // remove enemypiece check from here maybe to do further move calculations
                {
                    return false;
                }
            }
            catch (System.Exception) // if caught, move was off of board, making it invalid
            {

                return false;
            }

            return true;
        }

        private void CalculateAllowedMoves(GameObject piece, bool isPlayerPiece)
        {
            //Debug.Log(GridManager._pieceList);
            var _allowedMoveList = new List<Vector2>();
            Vector2 currentPiecePos = piece.transform.position;

            int counter = 0;
            for(int i = -1; i < 2; i ++)
            {
                if(counter % 2 == 0) // skips invalid straight moves up one 1 value, not a valid checkers move
                {
                    Vector2 move = piece.transform.position;
                    if(isPlayerPiece)
                    {
                        move.y = currentPiecePos.y + 1;
                        move.x = move.x + i;
                    }
                    else
                    {
                        move.y = currentPiecePos.y - 1;
                        move.x = move.x + i;
                    }
                    if (CheckifPieceisPresent(move))
                    {
                        _allowedMoveList.Add(move);
                        GameObject potentialMoveHighlight = Instantiate(_potentialMoveHighlight, new Vector2(move.x, move.y), Quaternion.identity);
                        _highlightList.Add(potentialMoveHighlight);
                    }

                }
                counter += 1;
            }
            //Debug.Log("allowed move:" + _allowedMoveList[0] + _allowedMoveList.Count);
        }

        private void RemovePreviouslyAllowedMoves()
        {
            for (int i = 0; i < _highlightList.Count; i++)
            {
                Destroy(_highlightList[i]);

            }
            _highlightList.RemoveAll(x => x);
        }

    }

}
