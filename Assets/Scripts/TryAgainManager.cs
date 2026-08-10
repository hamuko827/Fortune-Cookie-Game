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
        //(this is a static field, so it survives scene reloads on its own -
        //has to be cleared explicitly here)
        BowlCookie.cookieAlreadyChosen = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}