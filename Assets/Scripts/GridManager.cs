using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows;
using System;


namespace Checkers.grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int _width, _height;

        [SerializeField] private Tile _tilePrefab;

        [SerializeField] private GameObject _playerPiece, _enemyPiece;

        [SerializeField] private Transform _camera;

        public static List<GameObject> _pieceList;


        void Start()
        {
            generateGrid();
        }

        public void placePieces()
        {
            string fenString = "P1P1P1P1/1P1P1P1P/P1P1P1P1///1p1p1p1p/p1p1p1p1/1p1p1p1p"; // determines position of pieces on the board

            _pieceList = new List<GameObject>();

            int currentfenPos = 0;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (currentfenPos < fenString.Length) // checks for out of bounds fen string
                    {
                        if (fenString[currentfenPos] == '/') // skips line if '/' is present
                        {
                            y += 1;
                            x = -1;
                        }
                        else if (char.IsDigit(fenString[currentfenPos]))
                        {
                            int amountToSkip = fenString[currentfenPos] - '0';
                            if (amountToSkip > 1)
                            {
                                x += amountToSkip - 1;
                            }
                        }
                        else if (char.IsUpper(fenString[currentfenPos])) // upper case corresponds to player piece
                        {
                            var playerPiece = Instantiate(_playerPiece, new Vector3(x, y), Quaternion.identity);
                            _pieceList.Add(playerPiece);
                        }
                        else // must be an enemy piece
                        {
                            var enemyPiece = Instantiate(_enemyPiece, new Vector3(x, y), Quaternion.identity);
                            _pieceList.Add(enemyPiece);
                        }

                        if (x == _width - 1)
                        {
                            y -= 1;
                        }
                        currentfenPos += 1;
                    }
                }

            }

        }



        void generateGrid()
        {
            for (int x = 0; x < _width; x++) // width
            {
                for (int y = 0; y < _height; y++) // height
                {
                    var createdTile = Instantiate(_tilePrefab, new Vector3(x, y, 2), Quaternion.identity);
                    createdTile.name = $"Tile {x} {y}";


                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    createdTile.Init(isOffset); // colors grid tiles accordingly

                }
            }
            _camera.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10); // recenters camera to center of board

            placePieces();

        }
    }
}
