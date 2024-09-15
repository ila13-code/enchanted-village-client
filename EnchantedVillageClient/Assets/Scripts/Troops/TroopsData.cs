using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unical.Demacs.EnchantedVillage
{
    public class TroopsData
    {
        [JsonProperty]
        private int _x;
        [JsonProperty]
        private int _y;
        [JsonProperty]
        private int _z;
        [JsonProperty]
        private int type;

        public TroopsData(int x, int y, int z, int type)
        {
            _x = x;
            _y = y;
            _z = z;
            this.type = type;
        }

        public TroopsData()
        {
        }

        public int getX()
        {
            return _x;
        }
        public int getY()
        {
            return _y;
        }
        public int getZ()
        {
            return _z;
        }
        public int getType()
        {
            return type;
        }
        public void setX(int x)
        {
            this._x = x;
        }
        public void setY(int y)
        {
            this._y = y;
        }
        public void setZ(int z)
        {
            this._z = z;
        }
        public void setType(int type)
        {
            this.type = type;
        }
    }
}