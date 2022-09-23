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
        //[SerializeField] private GameObject _selectedPieceHighLight;

        bool confirmMove = false;
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Clicked();
            }

            if (Input.GetMouseButtonDown(1) && confirmMove)
            {
                Debug.Log("Moved");
                getMoveSpot();
            }
        }

        GameObject _selectedPiece = null;
        GameObject _selectedPieceHighLight = null;
        

        void getMoveSpot()
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            var _selectedMoveLocation = hit.collider.transform;
            Debug.Log(_selectedMoveLocation.name);

            int newX, newY;

            newX = _selectedMoveLocation.name[5] - '0';
            newY = _selectedMoveLocation.name[7] - '0';

            // if move is valid, execute move, add check here

            _selectedPiece.transform.position = new Vector2(newX, newY);
            _selectedPieceHighLight.SetActive(false);
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
            Debug.Log(hit);

            if (hit.collider.name == "playerPiece(Clone)")
            {
                _selectedPiece = hit.collider.transform.gameObject;
                _selectedPieceHighLight = hit.collider.transform.GetChild(0).gameObject;
                _selectedPieceHighLight.SetActive(true);
                confirmMove = true;

            }
            else if (hit.collider.name == "enemyPiece(Clone)")
            {
                _selectedPieceHighLight = hit.collider.transform.GetChild(0).gameObject;
                _selectedPieceHighLight.SetActive(true);
            }
            //CalculateAllowedMoves();
        }

        private void CheckifPieceisPresent(Vector2 currentPiecePos)// check each piece position on board to validate potential move
        {
            for(int i = 0; i < GridManager._pieceList.Count; i++) 
            {

            }
        }

        private void CalculateAllowedMoves(GameObject piece, bool isPlayerPiece)
        {
            //Debug.Log(GridManager._pieceList);

            Vector2 currentPiecePos = piece.transform.position;
            for(int i = 0; i< 3; i ++)
            {
                CheckifPieceisPresent(currentPiecePos);
            }
        }

    }

}
