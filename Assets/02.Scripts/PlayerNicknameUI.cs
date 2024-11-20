using UnityEngine;
using UnityEngine.UI;

public class PlayerNicknameUI : MonoBehaviour
{
    public Text nicknameText;

    private void Start()
    {
        string playerID = GameManager.Instance.UserID;
        DisplayNickname(playerID);
    }

    private void DisplayNickname(string nickname)
    {
        nicknameText.text = nickname;
    }
}
