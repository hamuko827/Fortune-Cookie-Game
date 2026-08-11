using UnityEngine;
using UnityEngine.SceneManagement;

//script that manages resetting the game when the player presses the try again button
public class TryAgainManager : MonoBehaviour
{
    //reloads the current scene, which naturally resets every cookie,
    //fortune paper, and text mask back to however they're set up in the scene
    public void TryAgain()
    {
        //resets the cookie selection so the player can choose another cookie
        //(these are static fields, so they survive scene reloads on their
        //own - clears both cookieAlreadyChosen and the currently-selected
        //cookie reference in one go)
        BowlCookie.ClearCurrentSelection();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}