using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private string nextScene;

    private bool loggingIn = false;

    private void Awake()
    {
        //startGameButton.onClick.RemoveAllListeners();
        //startGameButton.onClick.AddListener(GoToOverworld);
        //loginButton.onClick.RemoveAllListeners();
        //loginButton.onClick.AddListener(Login);

        /////////////////////// ชั่วคราว ///////////////////////
        PlayerPrefs.SetString("myUserId", string.Join("", System.Guid.NewGuid().ToByteArray()));
        PlayerPrefs.Save();
        //////////////////////////////////////////////////////
    }

    private void Start()
    {
        //if (PlayerPrefs.HasKey("accessToken"))
        //{
        //    HideAll();
        //    StartCoroutine(TokenManager.Instance.RefreshToken((success, statusCode, error) =>
        //    {
        //        if (success)
        //            ShowStartGameButton();
        //        else if (statusCode == 401)
        //            RestartScene();
        //        else
        //            RestartScene();
        //    }));
        //}
        //else
        //{
        //    ShowLoginPanel();
        //}
    }

    /////////////////////// ชั่วคราว ///////////////////////
    public void Python()
    {
        DatabaseManager.world = DatabaseManager.World.Python;
        Debug.Log(DatabaseManager.world);
        GoToOverworld();
    }

    public void Unity()
    {
        DatabaseManager.world = DatabaseManager.World.Unity;
        Debug.Log(DatabaseManager.world);
        GoToOverworld();
    }

    public void Blender()
    {
        DatabaseManager.world = DatabaseManager.World.Blender;
        Debug.Log(DatabaseManager.world);
        GoToOverworld();
    }

    public void Website()
    {
        DatabaseManager.world = DatabaseManager.World.Website;
        Debug.Log(DatabaseManager.world);
        GoToOverworld();
    }
    //////////////////////////////////////////////////////
    
    private void ShowLoginPanel()
    {
        startGameButton.gameObject.SetActive(false);
        usernameInput.text = PlayerPrefs.GetString("username", "");
        passwordInput.text = PlayerPrefs.GetString("password", "");
        loginPanel.SetActive(true);
    }

    private void ShowStartGameButton()
    {
        loginPanel.SetActive(false);
        startGameButton.gameObject.SetActive(true);
    }

    private void HideAll()
    {
        startGameButton.gameObject.SetActive(false);
        loginPanel.SetActive(false);
    }

    private void Login()
    {
        if (!usernameInput.gameObject.activeSelf || !passwordInput.gameObject.activeSelf)
            return;

        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return;

        if (loggingIn)
            return;

        loggingIn = true;

        StartCoroutine(DatabaseManager.Instance.Login(usernameInput.text, passwordInput.text, (success, error) =>
        {
            if (success)
            {
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("password", password);
                PlayerPrefs.Save();
                Debug.Log("Login successful.");
                ShowStartGameButton();
            }
            else
            {
                PlayerPrefs.DeleteKey("username");
                PlayerPrefs.DeleteKey("password");
                PlayerPrefs.Save();
                Debug.LogError("Login failed: " + error);
                ShowLoginPanel();
            }

            loggingIn = false;
        }));
    }

    private void GoToOverworld() => StartCoroutine(GoToOverworldIE());

    private IEnumerator GoToOverworldIE()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("Next scene is not set.");
            yield break;
        }

        yield return new WaitForSeconds(1);

        string loginScene = SceneManager.GetActiveScene().name;
        DatabaseManager.LoginScene = loginScene;

        SceneManager.LoadScene(nextScene);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
