using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInformation
{
    public class BattleDestroyed
    {
        public string uniqueId { get; set; }

        public BattleDestroyed(string uniqueId)
        {
            this.uniqueId = uniqueId;
        }
    }


    [JsonProperty("enemyEmail")]
    public string enemyEmail { get; set; }

    [JsonProperty("percentageDestroyed")]
    public int percentage_destroyed { get; set; }

    [JsonProperty("elixirStolen")]
    public int elixir_stolen { get; set; }

    [JsonProperty("goldStolen")]
    public int gold_stolen { get; set; }

    [JsonProperty("rewardExp")]
    public int reward_exp { get; set; }

    [JsonProperty("battleDestroyed")]
    public List<BattleDestroyed> battle_destroyeds { get; set; }

    public BattleInformation(
        string enemyEmail,
        int percentage_destroyed,
        int elixir_stolen,
        int gold_stolen,
        int reward_exp,
        List<BattleDestroyed> battle_destroyeds
    )
    {
        this.enemyEmail = enemyEmail;
        this.percentage_destroyed = percentage_destroyed;
        this.elixir_stolen = elixir_stolen;
        this.gold_stolen = gold_stolen;
        this.reward_exp = reward_exp;
        this.battle_destroyeds = battle_destroyeds;
    }



}

