using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingData
{
    [JsonProperty]
    private int _prefabIndex;
    [JsonProperty]
    private int _x;
    [JsonProperty]
    private int _y;

    public BuildingData(int prefabIndex, int x, int y)
    {
        _prefabIndex = prefabIndex;
        _x = x;
        _y = y;
    }

    public int getPrefabIndex()
    {
        return _prefabIndex;
    }
    public int getX()
    {
        return _x;
    }

    public int getY()
    {
        return _y;
    }

    public void setPrefabIndex(int prefabIndex)
    {
        _prefabIndex = prefabIndex;
    }

    public void setX(int x)
    {
        _x = x;
    }

    public void setY(int y)
    {
        _y = y;
    }
}
