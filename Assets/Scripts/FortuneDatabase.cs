using UnityEngine;

//manages the database of the fortunes
//has the method to get a random fortune from the good, mediocre, or bad categories
public class FortuneDatabase : MonoBehaviour
{
    //makes arrays that stores all the fortunes
    public string[] goodFortunes;
    public string[] mediocreFortunes;
    public string[] badFortunes;

    //fortune categories for the different reveal jingles
    public enum FortuneCategory
    {
        Good,
        Mediocre,
        Bad
    }

    //stores which category the current fortune belongs to
    [HideInInspector]
    public FortuneCategory currentFortuneCategory;


    //main script that generates a random fortune based on a 0-2 range
    public string GetRandomFortune()
    {
        //0 = good, 1 = mediocre, 2 = bad
        int category = Random.Range(0, 3);

        if (category == 0)
        {
            currentFortuneCategory = FortuneCategory.Good;

            return goodFortunes[
                Random.Range(
                    0,
                    goodFortunes.Length
                )
            ];
        }
        else if (category == 1)
        {
            currentFortuneCategory = FortuneCategory.Mediocre;

            return mediocreFortunes[
                Random.Range(
                    0,
                    mediocreFortunes.Length
                )
            ];
        }
        else
        {
            currentFortuneCategory = FortuneCategory.Bad;

            return badFortunes[
                Random.Range(
                    0,
                    badFortunes.Length
                )
            ];
        }
    }
}
