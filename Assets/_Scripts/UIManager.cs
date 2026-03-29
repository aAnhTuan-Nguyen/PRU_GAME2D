using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using System;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Login UI")]
    public GameObject panelLogin;
    public TMP_InputField inputEmailLogin;
    public TMP_InputField inputPasswordLogin;
    public TMP_Text messageLogin;

    [Header("Register UI")]
    public GameObject panelRegister;
    public TMP_InputField inputEmailRegister;
    public TMP_InputField inputPasswordRegister;
    public TMP_InputField inputPasswordConfirm;
    public Toggle toggleShowPassword;
    public TMP_Text messageRegister;

    [Header("Supabase Config")]
    public string projectUrl = "https://YOUR_PROJECT.supabase.co";
    public string anonKey = "YOUR_ANON_KEY";

    void Start()
    {
        ShowLoginPanel();
        // Ensure password fields are masked initially
        SetPasswordFieldVisibility(inputPasswordLogin, true);
        SetPasswordFieldVisibility(inputPasswordRegister, true);
        SetPasswordFieldVisibility(inputPasswordConfirm, true);

        // If toggle exists, set its initial state and hook up listener (optional)
        if (toggleShowPassword != null)
        {
            toggleShowPassword.isOn = false;
            toggleShowPassword.onValueChanged.RemoveAllListeners();
            toggleShowPassword.onValueChanged.AddListener(OnToggleShowPassword);
        }
    }

    // UI panel switching
    public void ShowLoginPanel()
    {
        if (panelLogin != null) panelLogin.SetActive(true);
        if (panelRegister != null) panelRegister.SetActive(false);
        if (messageLogin != null) messageLogin.text = "";
    }

    public void ShowRegisterPanel()
    {
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegister != null) panelRegister.SetActive(true);
        if (messageRegister != null) messageRegister.text = "";
    }

    // Toggle show/hide password for register panel
    public void OnToggleShowPassword(bool isOn)
    {
        SetPasswordFieldVisibility(inputPasswordRegister, !isOn);
        SetPasswordFieldVisibility(inputPasswordConfirm, !isOn);
    }

    void SetPasswordFieldVisibility(TMP_InputField field, bool hide)
    {
        if (field == null) return;

        field.contentType = hide ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
        field.ForceLabelUpdate();
    }

    // ---------------- LOGIN ----------------
    public void OnLoginButton()
    {
        if (messageLogin != null)
        {
            messageLogin.color = Color.black;
            messageLogin.text = "Đang đăng nhập...";
        }

        string email = inputEmailLogin != null ? inputEmailLogin.text.Trim() : "";
        string password = inputPasswordLogin != null ? inputPasswordLogin.text : "";
        StartCoroutine(LoginCoroutine(email, password));
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (messageLogin != null)
            {
                messageLogin.color = Color.red;
                messageLogin.text = "Vui lòng nhập email và mật khẩu.";
            }
            yield break;
        }

        string url = projectUrl + "/auth/v1/token?grant_type=password";
        var body = new LoginRequest { email = email, password = password };
        string jsonBody = JsonUtility.ToJson(body);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", anonKey);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    LoginResponse res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
                    if (!string.IsNullOrEmpty(res.access_token))
                    {
                        PlayerPrefs.SetString("access_token", res.access_token);
                        PlayerPrefs.SetString("refresh_token", res.refresh_token ?? "");
                        PlayerPrefs.Save();

                        if (messageLogin != null)
                        {
                            messageLogin.color = Color.green;
                            messageLogin.text = "Đăng nhập thành công!";
                        }

                        // Load Scene *******************************************
                        SceneManager.LoadScene("MainScene");
                    }
                    else
                    {
                        if (messageLogin != null)
                        {
                            messageLogin.color = Color.red;
                            messageLogin.text = "Đăng nhập thất bại. Vui lòng thử lại.";
                        }
                    }
                }
                catch (Exception)
                {
                    if (messageLogin != null)
                    {
                        messageLogin.color = Color.red;
                        messageLogin.text = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                    }
                }
            }
            else
            {
                if (messageLogin != null)
                {
                    messageLogin.color = Color.red;
                    messageLogin.text = GetFriendlyErrorMessage(req.responseCode, req.downloadHandler?.text);
                }
            }
        }
    }

    // ---------------- REGISTER ----------------
    public void OnRegisterButton()
    {
        if (messageRegister != null)
        {
            messageRegister.color = Color.black;
            messageRegister.text = "Đang đăng ký...";
        }

        string email = inputEmailRegister != null ? inputEmailRegister.text.Trim() : "";
        string password = inputPasswordRegister != null ? inputPasswordRegister.text : "";
        string passwordConfirm = inputPasswordConfirm != null ? inputPasswordConfirm.text : "";

        StartCoroutine(RegisterCoroutine(email, password, passwordConfirm));
    }

    IEnumerator RegisterCoroutine(string email, string password, string passwordConfirm)
    {
        // Basic client-side validation
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            if (messageRegister != null)
            {
                messageRegister.color = Color.red;
                messageRegister.text = "Vui lòng nhập email và mật khẩu.";
            }
            yield break;
        }

        if (!IsValidEmail(email))
        {
            if (messageRegister != null)
            {
                messageRegister.color = Color.red;
                messageRegister.text = "Email không hợp lệ.";
            }
            yield break;
        }

        if (password.Length < 6)
        {
            if (messageRegister != null)
            {
                messageRegister.color = Color.red;
                messageRegister.text = "Mật khẩu phải có ít nhất 6 ký tự.";
            }
            yield break;
        }

        if (password != passwordConfirm)
        {
            if (messageRegister != null)
            {
                messageRegister.color = Color.red;
                messageRegister.text = "Mật khẩu xác nhận không khớp.";
            }
            yield break;
        }

        string url = projectUrl + "/auth/v1/signup";
        var body = new LoginRequest { email = email, password = password };
        string jsonBody = JsonUtility.ToJson(body);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", anonKey);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                if (messageRegister != null)
                {
                    messageRegister.color = Color.green;
                    messageRegister.text = "Đăng ký thành công";
                }

                // Optionally auto-fill login email and switch to login panel
                if (inputEmailLogin != null) inputEmailLogin.text = email;
                if (inputPasswordLogin != null) inputPasswordLogin.text = "";

                yield return new WaitForSeconds(1.2f);
                ShowLoginPanel();
            }
            else
            {
                if (messageRegister != null)
                {
                    messageRegister.color = Color.red;
                    messageRegister.text = GetFriendlyErrorMessage(req.responseCode, req.downloadHandler?.text);
                }
            }
        }
    }

    // Simple email check
    bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // Helper method to convert HTTP errors to friendly messages
    string GetFriendlyErrorMessage(long responseCode, string responseText)
    {
        switch (responseCode)
        {
            case 400:
                // Try to parse error message from Supabase
                if (!string.IsNullOrEmpty(responseText) && responseText.Contains("User already registered"))
                {
                    return "Email này đã được đăng ký.";
                }
                if (!string.IsNullOrEmpty(responseText) && responseText.Contains("Invalid login credentials"))
                {
                    return "Email hoặc mật khẩu không đúng.";
                }
                return "Thông tin không hợp lệ. Vui lòng kiểm tra lại.";

            case 401:
                return "Email hoặc mật khẩu không đúng.";

            case 404:
                return "Không tìm thấy dịch vụ. Vui lòng kiểm tra cấu hình.";

            case 422:
                return "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";

            case 429:
                return "Bạn đã thử quá nhiều lần. Vui lòng chờ và thử lại.";

            case 500:
            case 502:
            case 503:
            case 504:
                return "Lỗi máy chủ. Vui lòng thử lại sau.";

            case 0:
                return "Không thể kết nối. Vui lòng kiểm tra kết nối mạng.";

            default:
                return "Có lỗi xảy ra. Vui lòng thử lại sau.";
        }
    }

    // ---------------- RESET PASSWORD ----------------
    public void OnResetPasswordButton()
    {
        // Implement reset password flow if needed (call /auth/v1/recover)

    }

    // Data classes
    [Serializable] public class LoginRequest { public string email; public string password; }
    [Serializable] public class ResetRequest { public string email; }
    [Serializable]
    public class LoginResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public User user;
    }
    [Serializable] public class User { public string id; public string email; public bool email_confirmed_at; }
}