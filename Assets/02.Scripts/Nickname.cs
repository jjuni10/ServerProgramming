using UnityEngine;
using TMPro; // Nếu bạn dùng TextMesh Pro

public class NicknameDisplay : MonoBehaviour
{
    public TextMeshProUGUI nicknameText; // Hoặc Text nếu không dùng TMP
    private string nickname = "Mouse";

    // Hàm này nhận nickname từ ngoài vào, ví dụ từ server
    public void SetNickname(string name)
    {
        nickname = name;
        UpdateNicknameText();
    }

    private void UpdateNicknameText()
    {
        if (nicknameText != null)
        {
            nicknameText.text = nickname;
        }
    }

    private void LateUpdate()
    {
        // Đảm bảo Text luôn nhìn về camera
        if (nicknameText != null)
        {
            nicknameText.transform.LookAt(Camera.main.transform);
            nicknameText.transform.Rotate(0, 180, 0); // Đảo ngược Text vì LookAt hướng ra sau
        }
    }
}
