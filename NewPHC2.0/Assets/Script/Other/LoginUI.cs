using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private string nextScene;

    private void Awake()
    {
        usernameInput.text = PlayerPrefs.GetString("username", "");
        passwordInput.text = PlayerPrefs.GetString("password", "");
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("accessToken"))
        {
            usernameInput.gameObject.SetActive(false);
            passwordInput.gameObject.SetActive(false);
            StartCoroutine(TokenManager.Instance.RefreshToken((success, statusCode, error) =>
            {
                if (success)
                    GoToOverworld();
                else if (statusCode == 401)
                    RestartScene();
                else
                    RestartScene();
            }));
        }
        else
        {
            usernameInput.gameObject.SetActive(true);
            passwordInput.gameObject.SetActive(true);
            loginButton.onClick.AddListener(Login);
        }
    }

    private void Login()
    {
        if (!usernameInput.gameObject.activeSelf || !passwordInput.gameObject.activeSelf)
            return;

        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return;

        StartCoroutine(DatabaseManager.Instance.Login(usernameInput.text, passwordInput.text, (success, error) =>
        {
            if (success)
            {
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("password", password);
                PlayerPrefs.Save();
                Debug.Log("Login successful.");
                GoToOverworld();
            }
            else
            {
                PlayerPrefs.DeleteKey("username");
                PlayerPrefs.DeleteKey("password");
                PlayerPrefs.Save();
                Debug.LogError("Login failed: " + error);
                Awake();
            }
        }));
    }

    private void GoToOverworld()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("Next scene is not set.");
            return;
        }

        SceneManager.LoadScene(nextScene);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
