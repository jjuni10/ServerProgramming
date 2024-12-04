using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThirdSceneUIController : MonoBehaviour
{
    public GameObject RedWinText;
    public GameObject BlueWinText;
    public GameObject NoneWinText;

    private Player[] player = new Player[4];
    public GameObject[] platform = new GameObject[4];

    private ETeam winTeam;

    public float HeightOffset = 20;
    private float rateValue;
    private float pointHeight;

    private int redGun = -1;
    private int redRun = -1;
    private int blueGun = -1;
    private int blueRun = -1;

    
    private bool co_Done = true;

    void Start()
    {
        winTeam = GameManager.Instance.WinTeam;
        //SetPointHeight();
    }

    public void SetPointHeight()
    {
        float maxPoint = GetPoint(true);
        rateValue = 20 / maxPoint;

        for (int i = 0; i < GameManager.Instance.PlayerCount; i++)
        {
            player[i] = GameManager.Instance.GetPlayer(i);
            player[i]._playerComponents.rigidbody.isKinematic = true;

            pointHeight = player[i]._currentValue.point * rateValue;
            if (player[i].Team == ETeam.Red){
                if (player[i].Role == ERole.Gunner) redGun = i;
                if (player[i].Role == ERole.Runner) redRun = i;
            }
            else{
                if (player[i].Role == ERole.Gunner) blueGun = i;
                if (player[i].Role == ERole.Runner) blueRun = i;
            }
            //StartCoroutine(UpdatePlayerHeight(i)); 

        }
        //SetWinTeamUI();
        StartCoroutine(ViewPoints());
    }

    private IEnumerator ViewPoints()
    {
        //* Gunner -> Runner -> SetWinTeamUI()

        if (redGun >= 0)
            StartCoroutine(UpdatePlayerHeight(redGun)); 
        yield return new WaitUntil(() => co_Done);  // co_Done이 true가 될때까지

        if (blueGun >= 0)
            StartCoroutine(UpdatePlayerHeight(blueGun)); 
        yield return new WaitUntil(() => co_Done);  // co_Done이 true가 될때까지

        if (redRun >= 0)
            StartCoroutine(UpdatePlayerHeight(redRun)); 
        yield return new WaitUntil(() => co_Done);  // co_Done이 true가 될때까지

        if (blueRun >= 0)
            StartCoroutine(UpdatePlayerHeight(blueRun)); 
        yield return new WaitUntil(() => co_Done);  // co_Done이 true가 될때까지

        SetWinTeamUI();
        for (int p = 0; p < GameManager.Instance.PlayerCount; p++)
        {
            player[p].SetAnimVicDef(winTeam);
        }

        yield return null;
    }

    private IEnumerator UpdatePlayerHeight(int playerIndex)
    {
        co_Done = false;

        pointHeight = player[playerIndex]._currentValue.point * rateValue;
        Vector3 playerPos = player[playerIndex].transform.position;
        Vector3 formSca = platform[playerIndex].transform.localScale;
        //Vector3 formPos = platform[playerIndex].transform.position;
        Vector3 playerPos_ = playerPos + new Vector3(0, pointHeight, 0);
        Vector3 formSca_ = formSca + new Vector3(0, pointHeight, 0);
        //Vector3 formPos_ = formPos + new Vector3(0, pointHeight/2, 0);

        do
        {
            float lerpSpeed = 0.1f * Time.deltaTime;
            
            // player[playerIndex].transform.position = Vector3.Lerp(playerPos, playerPos_, lerpSpeed);
            // platform[playerIndex].transform.localScale = Vector3.Lerp(formSca, formSca_, lerpSpeed);
            // platform[playerIndex].transform.position = Vector3.Lerp(formPos, formPos_, lerpSpeed);

            Vector3 pP = playerPos_ * lerpSpeed;
            Vector3 fS = formSca_ * lerpSpeed;
            //Vector3 fP = formPos_ * lerpSpeed;
            pP.x = 0; fS.x = 0; //fP.x = 0;
            pP.z = 0; fS.z = 0; //fP.z = 0;
            
            player[playerIndex].transform.position += pP;
            platform[playerIndex].transform.localScale += fS;
            //platform[playerIndex].transform.position += fP;

            yield return null; 
        }while (Vector3.Distance(player[playerIndex].transform.position, playerPos_) > 0.1f);
        
        player[playerIndex].transform.position = playerPos_;
        platform[playerIndex].transform.localScale = formSca_;
        //platform[playerIndex].transform.position = formPos_;
        player[playerIndex].SetPlayerPoint();

        co_Done = true;
    }

    public void SetWinTeamUI()
    {
        if (winTeam == ETeam.Red)
        {
            RedWinText.SetActive(true);
        }
        else if (winTeam == ETeam.Blue)
        {
            BlueWinText.SetActive(true);
        }
        else
        {
            NoneWinText.SetActive(true);
        }
    }

    public float GetPoint(bool isBig)
    {
        int result_Max = GameManager.Instance.points[0];
        int result_Min = GameManager.Instance.points[0];
        // foreach (var item in GameManager.Instance.points)
        // {
        //     if (result_Max < item) result_Max = item;
        //     else if (item < result_Min) result_Min = item;
        // }
        for (int i = 0; i < GameManager.Instance.PlayerCount; i++)
        {
            if (result_Max < GameManager.Instance.points[i]) result_Max = GameManager.Instance.points[i];
            else if (GameManager.Instance.points[i] < result_Min) result_Min = GameManager.Instance.points[i];
        }
        if (isBig)
        {
            return result_Max;
        }
        else
        {
            return result_Min;
        }
        
    }
    public float GetSumPoint()
    {
        float sum = 0;
        foreach (var item in GameManager.Instance.points)
        {
            sum += item;
        }
        return sum;
    }
}
