using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TileMainScript : MonoBehaviour
{
    public TextMesh displayText;
    public bool isMined = false;

    public Material materialIdle, materialLightUp, materialUncovered, detonatedMaterial;

    public int ID, tilesPerRow;

    public GameObject tileUpper, tileLower, tileLeft, tileRight, tileUpperRight, tileUpperLeft, tileLowerRight, tileLowerLeft;

    TileGrid tg;

    public List<GameObject> adjacentTiles;


    public int adjacentMines;

    public enum State { idle, flagged, uncovered, detonated};
    public State state = State.idle;


    public GameObject displayFlag;


    private void Awake()
    {
        tg = FindObjectOfType<TileGrid>();
    }


    private void Start()
    {
        displayFlag.SetActive(false);
        displayText.GetComponent<Renderer>().enabled = false;


        if (IsBounds(tg.allTiles, ID + tilesPerRow))
                tileUpper = tg.allTiles[ID + tilesPerRow];

        if (IsBounds(tg.allTiles, ID - tilesPerRow))
                tileLower = tg.allTiles[ID - tilesPerRow];
        
        if (IsBounds(tg.allTiles, ID-1) && ID % tilesPerRow != 0)
                tileLeft = tg.allTiles[ID-1];
        
       if (IsBounds(tg.allTiles, ID+1) && (ID + 1) % tilesPerRow != 0)
                tileRight = tg.allTiles[ID+1];
        
       if (IsBounds(tg.allTiles, ID + tilesPerRow+1) && (ID + 1) % tilesPerRow != 0)
                tileUpperRight = tg.allTiles[ID + tilesPerRow + 1];

       if (IsBounds(tg.allTiles, ID + tilesPerRow-1) && ID % tilesPerRow != 0)
                tileUpperLeft = tg.allTiles[ID + tilesPerRow - 1];

       if (IsBounds(tg.allTiles, ID - tilesPerRow+1) && (ID + 1) % tilesPerRow != 0)
                tileLowerRight = tg.allTiles[ID - tilesPerRow + 1];

       if (IsBounds(tg.allTiles, ID - tilesPerRow-1) && ID % tilesPerRow != 0)
                tileLowerLeft = tg.allTiles[ID - tilesPerRow - 1];


        if (tileUpper) 
            adjacentTiles.Add(tileUpper);
        if (tileLower) 
            adjacentTiles.Add(tileLower);
        if (tileLeft) 
            adjacentTiles.Add(tileLeft);
        if (tileRight) 
            adjacentTiles.Add(tileRight);
        if (tileUpperRight) 
            adjacentTiles.Add(tileUpperRight);
        if (tileUpperLeft) 
            adjacentTiles.Add(tileUpperLeft);
        if (tileLowerRight) 
            adjacentTiles.Add(tileLowerRight);
        if (tileLowerLeft) 
            adjacentTiles.Add(tileLowerLeft);

        CountMines();
    }


    public void SetFlag()
    {
        if(state == State.idle)
        {
            state = State.flagged;
            displayFlag.SetActive(true);
            tg.GetComponent<TileGrid>().minesRemaining = tg.GetComponent<TileGrid>().minesRemaining - 1;
            if (isMined)
                tg.GetComponent<TileGrid>().minesMarked = tg.GetComponent<TileGrid>().minesMarked + 1;

        }
        else if (state == State.flagged)
        {
            state = State.idle;
            displayFlag.SetActive(false);
            tg.GetComponent<TileGrid>().minesRemaining = tg.GetComponent<TileGrid>().minesRemaining + 1;
            if (isMined)
                tg.GetComponent<TileGrid>().minesMarked = tg.GetComponent<TileGrid>().minesMarked - 1;
        }
    }


    public bool IsBounds(List<GameObject> massive, int targetID)  //Проверка есть ли сосед
    {
        if (targetID < 0 || targetID >= massive.Count)
            return false;
        else
            return true;
    }

    public void CountMines()  //Подсчет мин вокруг
    {
        adjacentMines = 0;

        foreach (GameObject curTile in adjacentTiles)
            if (curTile.GetComponent<TileMainScript>().isMined)
                adjacentMines += 1;

        displayText.text = adjacentMines.ToString();

        if (adjacentMines <= 0)
            displayText.text = "";
    }

    public void Explode()
    {
        state = State.detonated;
        GetComponent<SpriteRenderer>().material = detonatedMaterial;

        foreach (GameObject curTile in tg.tilesMined)
            curTile.GetComponent<TileMainScript>().ExploadExternal();

        tg.GetComponent<TileGrid>().gamemode = TileGrid.State.lose;
    }
    public void ExploadExternal()
    {
        state = State.detonated;
        GetComponent<SpriteRenderer>().material = detonatedMaterial;
    }
    public void UncoverTile()
    {
        if (!isMined)
        {
            state = State.uncovered;
            displayText.GetComponent<Renderer>().enabled = true;
            GetComponent<SpriteRenderer>().material = materialUncovered;

            if (adjacentMines == 0)
                UncoverAdjacentTiles();
        }
        else
            Explode();
    }
    private void OnMouseOver()
    {
        if (tg.gamemode == TileGrid.State.ingame)
        {

            if (state == State.idle)
            {
                GetComponent<SpriteRenderer>().material = materialLightUp;  //Изменение материала при наведении

                if (Input.GetMouseButtonDown(1))
                    SetFlag();

                if (Input.GetMouseButtonDown(0))
                    UncoverTile();
            }
            else if (state == State.flagged)
            {
                GetComponent<SpriteRenderer>().material = materialLightUp;  //Изменение материала при наведении

                if (Input.GetMouseButtonDown(1))
                    SetFlag();
            }
        }
    }

    private void OnMouseExit()
    {
        if (tg.gamemode == TileGrid.State.ingame)
        {
            if (state == State.idle || state == State.flagged)
                GetComponent<SpriteRenderer>().material = materialIdle; //Возвращение на начальный материал
        }
    }

    public void UncoverAdjacentTiles()
    {

        foreach(GameObject curTile in adjacentTiles)
        {
            //Если сосед без мины, то открываем его
            if (!curTile.GetComponent<TileMainScript>().isMined && curTile.GetComponent<TileMainScript>().state == State.idle && curTile.GetComponent<TileMainScript>().adjacentMines == 0)
                curTile.GetComponent<TileMainScript>().UncoverTile();
            //Если мина есть, то раскрываем его
            else if (!curTile.GetComponent<TileMainScript>().isMined && curTile.GetComponent<TileMainScript>().state == State.idle && curTile.GetComponent<TileMainScript>().adjacentMines > 0)
                curTile.GetComponent<TileMainScript>().UncoverTileExternal();
        }

    }

    public void UncoverTileExternal()
    {
        state = State.uncovered;
        displayText.GetComponent<Renderer>().enabled = true;
        GetComponent<SpriteRenderer>().material = materialUncovered;
        tg.GetComponent<TileGrid>().tilesUncovered = tg.GetComponent<TileGrid>().tilesUncovered + 1;


    }
}
