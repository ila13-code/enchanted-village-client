using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleInformation
{

    [JsonProperty("enemyEmail")]
    public string enemyEmail { get; set; }

    [JsonProperty("percentage_destroyed")]
    public int percentage_destroyed { get; set; }

    [JsonProperty("elixir_stolen")]
    public int elixir_stolen { get; set; }

    [JsonProperty("gold_stolen")]
    public int gold_stolen { get; set; }

    [JsonProperty("reward_exp")]
    public int reward_exp { get; set; }


    public BattleInformation(
        string enemyEmail,
        int percentage_destroyed,
        int elixir_stolen,
        int gold_stolen,
        int reward_exp
    )
    {
        this.enemyEmail = enemyEmail;
        this.percentage_destroyed = percentage_destroyed;
        this.elixir_stolen = elixir_stolen;
        this.gold_stolen = gold_stolen;
        this.reward_exp = reward_exp;
    }



}

