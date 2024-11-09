using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class TroopsData
    {
        [JsonProperty("_x")]
        public float _x { get; set; }
        [JsonProperty("_y")]
        public float _y { get; set; }
        [JsonProperty("_z")]
        public float _z { get; set; }
        [JsonProperty("_type")]
        public int _type { get; set; }

        public TroopsData(float x, float y, float z, int type)
        {
            _x = x;
            _y = y;
            _z = z;
            _type = type;
        }

        public TroopsData Clone()
        {
            return new TroopsData(_x, _y, _z, _type);
        }
    

        public float getX()
        {
            return _x;
        }
        public float getY()
        {
            return _y;
        }
        public float getZ()
        {
            return _z;
        }
        public int getType()
        {
            return _type;
        }
        public void setX(float x)
        {
            this._x = x;
        }
        public void setY(float y)
        {
            this._y = y;
        }
        public void setZ(float z)
        {
            this._z = z;
        }
        public void setType(int type)
        {
            this._type = type;
        }
    }
}