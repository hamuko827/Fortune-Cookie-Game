using UnityEngine;

//manages the database of the fortunes
//has the method to get a random fortune from the good, mediocre, or bad categories
public class FortuneDatabase : MonoBehaviour
{
    public string[] goodFortunes;
    public string[] mediocreFortunes;
    public string[] badFortunes;

    public string GetRandomFortune()
    {
        int category = Random.Range(0, 3); // 0 = good, 1 = mediocre, 2 = bad

        if (category == 0)
            return goodFortunes[Random.Range(0, goodFortunes.Length)];
        else if (category == 1)
            return mediocreFortunes[Random.Range(0, mediocreFortunes.Length)];
        else
            return badFortunes[Random.Range(0, badFortunes.Length)];
    }
}