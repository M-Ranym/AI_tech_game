using System.Collections.Generic;
using System.Collections;

public class Game_Over_2_LeaderBoardEntry
{
    public string name;
    public string surName;
    public int bestScore;
    public int exp;
    public string stars;
    public int avatarIndex;

    public Game_Over_2_LeaderBoardEntry(string name, string surName, int bestScore, int exp, int avatarIndex, string stars)
    {
        this.name = name;
        this.surName = surName;
        this.bestScore = bestScore;
        this.exp = exp;
        this.stars = stars;
        this.avatarIndex = avatarIndex;
    }

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();

        result[Game_Over_2_Constants.DB_NAME] = name;
        result[Game_Over_2_Constants.DB_SURNAME] = surName;
        result[Game_Over_2_Constants.DB_BEST_SCORE] = bestScore;
        result[Game_Over_2_Constants.DB_XP] = exp;
        result[Game_Over_2_Constants.DB_STARS] = stars;
        result[Game_Over_2_Constants.DB_AVATR_UPPER_ACCESSORIES] = avatarIndex;

        return result;
    }
}