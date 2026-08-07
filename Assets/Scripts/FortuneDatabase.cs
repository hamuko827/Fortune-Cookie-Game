using UnityEngine;

//manages the database of the fortunes
//has the method to get a random fortune from the good, mediocre, or bad categories
public class FortuneDatabase : MonoBehaviour
{
    //makes arrays that stores all the fortunes
    public string[] goodFortunes;
    public string[] mediocreFortunes;
    public string[] badFortunes;

    //main script that generates a random fortune based on a 0-2 range
    public string GetRandomFortune()
    {
        //0 = good, 1 = mediocre, 2 = bad
        int category = Random.Range(0, 3); 

        if (category == 0)
            return goodFortunes[Random.Range(0, goodFortunes.Length)];
        else if (category == 1)
            return mediocreFortunes[Random.Range(0, mediocreFortunes.Length)];
        else
            return badFortunes[Random.Range(0, badFortunes.Length)];
    }
}