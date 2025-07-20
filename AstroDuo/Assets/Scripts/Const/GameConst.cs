using UnityEngine;

public static class GameConst 
{
    public const int MAX_SCORE = 5;
    public const string SETTING_SCENE = "SettingScene";
    public const string MAIN_SCENE = "MainScene";
    public const string SELECT_SKIN_SCENE = "SelectSkinScene";
    public const string SAMPLE_SCENE = "SampleScene";
    public const string SCORE_SCENE = "ScoreScene";
    public const string MAP_1 = "Map1";
    public const string MAP_2 = "Map2";
    public const string MAP_4 = "Map4";
    public const string MAP_6 = "Map6";
    public const string WINNER_SCENE = "WinnerScene";
    
    // Array of all available maps for random selection
    public static readonly string[] AVAILABLE_MAPS = {
        SAMPLE_SCENE,
        MAP_1,
        MAP_2,
        MAP_4,
        MAP_6
    };
}
