using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TileGrid : MonoBehaviour
{
    public GameObject tilePrefab;  //Тайл(1 Клетка)
    public int numberOfTiles = 16;  //Колличество тайлов
    public float distanceBetweenTiles = 1.3f;  //Дистанция мд тайлами
    public int tilesInRow = 4;  //Тайлов в строке

    public int numberOfMines = 4;

    public List<GameObject> allTiles, tilesMined , tilesUnmined;

    public enum State { ingame, lose, win};
    public State gamemode = State.ingame;

    public  int minesMarked = 0, tilesUncovered = 0, minesRemaining = 0;


    public GameObject WinScreen, LoseScreen;

    private void Update()
    {
        if(gamemode == State.ingame)
        {
            if(minesRemaining ==0 && minesMarked == numberOfMines)
            {
                FinishGame();
            }
        }

        UI();
    }
    public void FinishGame()
    {
        gamemode = State.win;

        foreach (GameObject curTile in allTiles)
            if (curTile.GetComponent<TileMainScript>().state == TileMainScript.State.idle && !curTile.GetComponent<TileMainScript>().isMined)
                curTile.GetComponent<TileMainScript>().UncoverTileExternal();

        foreach (GameObject curTile in tilesMined)
            if (curTile.GetComponent<TileMainScript>().state == TileMainScript.State.flagged)
                curTile.GetComponent<TileMainScript>().SetFlag();
    }


    public void UI()
    {
        if (gamemode == State.lose)
            LoseScreen.SetActive(true);

        if (gamemode == State.win)
            WinScreen.SetActive(true);
    }


    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    void Awake()
    {
        allTiles = new List<GameObject>(numberOfTiles);
        tilesMined = new List<GameObject>();
        tilesUnmined = new List<GameObject>();

        CreateTiles();  //Запускаем метод спавна тайлов 

        gamemode = State.ingame;

        minesRemaining = numberOfMines;
        minesMarked = 0;
        tilesUncovered = 0;
    }
    public void CreateTiles()
    {
        float xoffset = 0f;  //Смещение по х
        float yoffset = 0f;  //Смещение по у

        for (int tilesCreated = 0; tilesCreated < numberOfTiles; tilesCreated++)  //Спавн тайлов
        {
            xoffset += distanceBetweenTiles;  //Смещение по икс

           if(tilesCreated % tilesInRow == 0)   //Если в строке 4 тайла, то переход на некст строку на позицию уоффсет
            {
                yoffset += distanceBetweenTiles;  //Смещение по у
                xoffset = 0;    
            }
            GameObject createdTiles = Instantiate(tilePrefab, new Vector2(transform.position.x + xoffset, transform.position.y + yoffset), transform.rotation); //Создание тайлов
            createdTiles.GetComponent<TileMainScript>().ID = tilesCreated;
            createdTiles.GetComponent<TileMainScript>().tilesPerRow = tilesInRow;
            allTiles.Add(createdTiles);
            tilesUnmined.Add(createdTiles);
        }

        AssignMines();
    }


    public void AssignMines()
    {     
        for(int minesAssigned = 0; minesAssigned < numberOfMines; minesAssigned++)
        {
            GameObject currentTile = tilesUnmined[Random.Range(0, tilesUnmined.Count)];
            tilesMined.Add(currentTile);
            tilesUnmined.Remove(currentTile);        
            currentTile.GetComponent<TileMainScript>().isMined = true;
        }
    }

   




}
