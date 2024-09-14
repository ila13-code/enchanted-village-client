using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[System.Serializable]
public class BuildingData
{
    [JsonProperty]
    private string _uniqueId;
    [JsonProperty]
    private int _prefabIndex;
    [JsonProperty]
    private int _x;
    [JsonProperty]
    private int _y;
    [JsonProperty]
    private List<TroopsData> _troopsData;

    public BuildingData(String id,int prefabIndex, int x, int y)
    {
        _uniqueId = id;
        _prefabIndex = prefabIndex;
        _x = x;
        _y = y;
        if(prefabIndex == 4) //sto salvando un campo di truppe
        {
            _troopsData = new List<TroopsData>();
        }
        else
        {
            _troopsData = null;
        }
    }
    public string GetUniqueId()
    {
        return _uniqueId;
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

    public List<TroopsData> getTroopsData()
    {
        return _troopsData;
    }

    public int getTroopsCount()
    {
        return _troopsData.Count;
    }

    public void setTroopsData(List<TroopsData> troopsData)
    {
        _troopsData = troopsData;
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
