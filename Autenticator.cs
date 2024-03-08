using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Autenticator : MonoBehaviour
{
    public TMP_Text puntajesText;
    string url = "https://sid-restapi.onrender.com";
    string Token { get; set; }
    string Username { get; set; }

    void Start()
    {
        Token = PlayerPrefs.GetString("token");

        if (string.IsNullOrEmpty(Token))
        {
            Debug.Log("No Tokens");
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            StartCoroutine(GetProfile());
        }
    }

    public void enviarRegistro()
    {
        string username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        StartCoroutine(Registro(JsonUtility.ToJson(new AuthenticationData { username = username, password = password })));
    }

    public void enviarLogin()
    {
        string username = GameObject.Find("InputFieldUsername").GetComponent<TMP_InputField>().text;
        string password = GameObject.Find("InputFieldPassword").GetComponent<TMP_InputField>().text;

        StartCoroutine(Login(JsonUtility.ToJson(new AuthenticationData { username = username, password = password })));
    }

    IEnumerator Registro(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", json);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                StartCoroutine(Login(json));
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }
    IEnumerator Login(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/auth/login", json);
        request.method = UnityWebRequest.kHttpVerbPOST;
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error de conexión: " + request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                AuthenticationData data = JsonUtility.FromJson<AuthenticationData>(request.downloadHandler.text);

                if (data != null && !string.IsNullOrEmpty(data.token))
                {
                    Token = data.token;
                    Username = data.username;
                    PlayerPrefs.SetString("token", Token);
                    PlayerPrefs.SetString("username", Username);
                    Debug.Log("Inicio de sesión exitoso.");
                }
                else
                {
                    Debug.LogError("Respuesta de inicio de sesión inválida.");
                }
            }
            else
            {
                Debug.LogError("Error en la solicitud de inicio de sesión: " + request.responseCode);
            }
        }
    }


    IEnumerator GetProfile()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios/" + Username);
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                string puntajesInfo = "";

                foreach (var usuario in response.usuarios)
                {
                    puntajesInfo += "El usuario " + usuario.username + " tiene un puntaje de " + usuario.data.score + "\n";
                }

                puntajesText.text = puntajesInfo;
            }
            else
            {
                Debug.Log("El usuario no está autenticado");
            }
        }
    }

    [System.Serializable]
    public class AuthResponse
    {
        public List<UsuarioJson> usuarios;
    }

    [System.Serializable]
    public class AuthenticationData
    {
        public string username;
        public string password;
        public string token; 
        public DataUser data;
    }

    [System.Serializable]
    public class UsuarioJson
    {
        public string _id;
        public string username;
        public DataUser data;
    }

    [System.Serializable]
    public class DataUser
    {
        public int score;
    }
}
