using UnityEngine;

//script that manages resetting the game when the player presses the try again button
public class TryAgainManager : MonoBehaviour
{
    //resets all of the cookies and the fortune system
    public void TryAgain()
    {
        //resets the cookie selection so the player can choose another cookie
        BowlCookie.cookieAlreadyChosen = false;

        //finds all of the cookies in the scene
        BowlCookie[] allCookies = FindObjectsOfType<BowlCookie>();

        //resets every cookie in the bowl
        foreach (BowlCookie cookie in allCookies)
        {
            cookie.ResetCookie();
        }

        //finds all of the cookie drag systems in the scene
        CookieDrag[] allCookieDrags = FindObjectsOfType<CookieDrag>();

        //resets every opened cookie
        foreach (CookieDrag cookieDrag in allCookieDrags)
        {
            cookieDrag.ResetCookie();
        }
    }
}

