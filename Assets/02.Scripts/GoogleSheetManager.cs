using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class GoogleSheetsManager : MonoBehaviour
{
    private const string apiKey = "AIzaSyC6WXitd6NEl4xdmBwrJAEj5yGZgx3D61A"; // API 키
    private const string spreadsheetId = "1ehWQEYsIBCIgznBSH6Y41rMxcOfiM1okkJ_LVDoq_Og"; // 스프레드시트 ID
    private const string range = "Sheet1!A1:B6"; // 데이터가 있는 범위 지정

    private static readonly HttpClient client = new HttpClient();

    // Google Sheets에서 데이터를 가져옴
    public async Task<Dictionary<string, string>> LoadDataFromGoogleSheets()
    {
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}?key={apiKey}";
        var response = await client.GetStringAsync(url);
        var json = JObject.Parse(response);

        Dictionary<string, string> data = new Dictionary<string, string>();
        foreach (var row in json["values"])
        {
            string key = row[0].ToString();
            string value = row[1].ToString();
            data[key] = value;
        }

        return data;
    }

    // 데이터를 Define 클래스의 상수에 반영
    public async Task ApplyData()
    {
        var data = await LoadDataFromGoogleSheets();

        // Define.cs에서 주석처리된부분 참고 public static float 로 변경해야함
        // Define.START_POSITION_OFFSET = float.Parse(data["START_POSITION_OFFSET"]);
        // Define.START_DISTANCE_OFFSET = float.Parse(data["START_DISTANCE_OFFSET"]);
        // Define.FIRE_COOL_TIME = float.Parse(data["FIRE_COOL_TIME"]);
        // Define.GAME_GUNNER_POSITION_OFFSET = float.Parse(data["GAME_GUNNER_POSITION_OFFSET"]);
        // Define.GAME_RUNNER_POSITION_OFFSET = float.Parse(data["GAME_RUNNER_POSITION_OFFSET"]);
        // Define.READY_TIME = float.Parse(data["READY_TIME"]);

        Debug.Log("Define constants updated from Google Sheets.");
        Debug.Log(Define.START_POSITION_OFFSET);
    }

    private async void Start()
    {
        await ApplyData(); // 시작할 때 데이터 적용.
    }
}