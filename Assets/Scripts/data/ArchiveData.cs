using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.data
{
    // save data.
    [Serializable]
    public class ArchiveData
    {
        // only use this JsonProperty can we use "JsonConvert" to serialize the private memeber variable correctly
        [JsonProperty("name")] 
        private string name;

        [JsonProperty("unitMoralDict")]
        private Dictionary<string, float> unitMoralDict;

        [JsonProperty("unitDisciplineDict")]
        private Dictionary<string, float> unitDisciplineDict;

        public PlayerData playerData;

        public ArchiveData(string _name)
        {
            name = _name;
            unitMoralDict = new Dictionary<string, float>();
            unitDisciplineDict = new Dictionary<string, float>();
            playerData = new PlayerData();
        }

        public void InitByDefault()
        {
            unitMoralDict = new Dictionary<string, float>();
            unitDisciplineDict = new Dictionary<string, float>();

            foreach(var item in Table_unit.data)
            {
                unitMoralDict[item.Key] = item.Value.basicMoral;
                unitDisciplineDict[item.Key] = item.Value.basicDiscipline;
            }
        }

        public string GetName() { return name; }

        public void SetMoral(string id, float value) { unitMoralDict[id] = value; }

        public void SetDiscipline(string id, float value) { unitDisciplineDict[id] = value; }

        public Dictionary<string, float> GetUnitMoralDictCopy() { return new Dictionary<string, float>(unitMoralDict); }

        public Dictionary<string, float> GetUnitDisciplineDictCopy() { return new Dictionary<string, float>(unitDisciplineDict); }
    }

}
