using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;

public class GoogleSheetsManager : MonoBehaviour
{
    private const string apiKey = "AIzaSyAcIfE5pZTLtUCbVxaFoHG1hougAWCfVKw"; // API 키
    private const string spreadsheetId = "1j2kEidWX2-9yBsBqeKDA7uWYAhIt6ULLkY4XLgLuwLA"; // 스프레드시트 ID
    private static readonly HttpClient client = new HttpClient();

    // 특정 시트에서 데이터를 로드
    public async Task<List<Dictionary<string, string>>> LoadDataFromSheet(string sheetName, string range)
    {
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{sheetName}!{range}?key={apiKey}";
        var response = await client.GetStringAsync(url);
        var json = JObject.Parse(response);

        var headers = json["values"][0].ToObject<List<string>>(); // 첫 번째 행(헤더)을 키로 사용
        List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

        foreach (var row in json["values"].Skip(1)) // 데이터 행 처리
        {
            var rowData = row.ToObject<List<string>>();
            Dictionary<string, string> rowDict = new Dictionary<string, string>();

            for (int i = 0; i < headers.Count; i++)
            {
                rowDict[headers[i]] = rowData[i];
            }

            rows.Add(rowDict);
        }

        return rows;
    }

    // 데이터를 Define 클래스에 반영
    public async Task ApplyData()
    {

        var sheetNames = new List<string> { "PlayerBasic", "Gunner", "Runner", "WinCheck", "Item", "Bullet", "Origin" };

        foreach (var sheetName in sheetNames)
        {
            var data = await LoadDataFromSheet(sheetName, "A1:Z100"); // 데이터 범위 확장

            Debug.Log($"Applying data from sheet: {sheetName}");

            switch (sheetName)
            {
                case "PlayerBasic":
                    foreach (var row in data)
                    {
                        var playerId = row["id"];
                        Define.PlayerBasicData playerData = new Define.PlayerBasicData
                        {
                            StartPosX = float.Parse(row["startPosX"]),
                            StartPosY = float.Parse(row["startPosY"]),
                            StartPosZ = float.Parse(row["startPosZ"]),
                            ScaleX = float.Parse(row["scaleX"]),
                            ScaleY = float.Parse(row["scaleY"]),
                            ScaleZ = float.Parse(row["scaleZ"]),
                            MoveSpeed = float.Parse(row["moveSpeed"]),
                            ReadyPressTime = float.Parse(row["readyPressTime"]),
                            ReadyStats = bool.Parse(row["readyStats"]),
                            TeamStats = bool.Parse(row["teamStats"]),
                            RoleStats = bool.Parse(row["roleStats"]),
                        };
                        Define.Players[playerId] = playerData;
                    }
                    break;

                case "Origin":
                    var originData = data[0]; // Origin 시트의 첫 번째 데이터만 사용
                    Define.START_POSITION_OFFSET = float.Parse(originData["START_POSITION_OFFSET"]);
                    Define.START_DISTANCE_OFFSET = float.Parse(originData["START_DISTANCE_OFFSET"]);
                    Define.FIRE_COOL_TIME = float.Parse(originData["FIRE_COOL_TIME"]);
                    Define.GAME_GUNNER_POSITION_OFFSET = float.Parse(originData["GAME_GUNNER_POSITION_OFFSET"]);
                    Define.GAME_RUNNER_POSITION_OFFSET = float.Parse(originData["GAME_RUNNER_POSITION_OFFSET"]);
                    Define.READY_TIME = float.Parse(originData["READY_TIME"]);

                    Debug.Log("Origin data applied to Define constants:");
                    break;

                case "Gunner":
                    foreach (var row in data)
                    {
                        var gunnerId = row["id"];
                        Define.GunnerData gunnerData = new Define.GunnerData
                        {
                            StartPosX = float.Parse(row["startPosX"]),
                            StartPosY = float.Parse(row["startPosY"]),
                            StartPosZ = float.Parse(row["startPosZ"]),
                            PosX = float.Parse(row["posX"]),
                            PosY = float.Parse(row["posY"]),
                            PosZ = float.Parse(row["posZ"]),
                            ScaleX = float.Parse(row["scaleX"]),
                            ScaleY = float.Parse(row["scaleY"]),
                            ScaleZ = float.Parse(row["scaleZ"]),
                            MoveSpeed = float.Parse(row["moveSpeed"]),
                            DashSpeed = float.Parse(row["dashSpeed"]),
                            FiringCoolTime = float.Parse(row["firingCoolTime"]),
                            GetPoint = int.Parse(row["getPoint"]),
                            IsClicked = bool.Parse(row["isClicked"]),
                            TeamStats = bool.Parse(row["teamStats"]),
                        };
                        Define.Gunners[gunnerId] = gunnerData;
                    }
                    break;

                case "Runner":
                    foreach (var row in data)
                    {
                        var runnerId = row["id"];
                        Define.RunnerData runnerData = new Define.RunnerData
                        {
                            StartPosX = float.Parse(row["startPosX"]),
                            StartPosY = float.Parse(row["startPosY"]),
                            StartPosZ = float.Parse(row["startPosZ"]),
                            PosX = float.Parse(row["posX"]),
                            PosY = float.Parse(row["posY"]),
                            PosZ = float.Parse(row["posZ"]),
                            ScaleX = float.Parse(row["scaleX"]),
                            ScaleY = float.Parse(row["scaleY"]),
                            ScaleZ = float.Parse(row["scaleZ"]),
                            MoveSpeed = float.Parse(row["moveSpeed"]),
                            DashForce = float.Parse(row["dashForce"]),
                            DashCoolTime = float.Parse(row["dashCoolTime"]),
                            GetPoint = int.Parse(row["getPoint"]),
                            IsClicked = bool.Parse(row["isClicked"]),
                            TeamStats = bool.Parse(row["teamStats"]),
                        };
                        Define.Runners[runnerId] = runnerData;
                    }
                    break;

                case "WinCheck":
                    foreach (var row in data)
                    {
                        var winCheckId = row["id"];
                        Define.WinCheckData winCheckData = new Define.WinCheckData
                        {
                            StartPosX = float.Parse(row["startPosX"]),
                            StartPosY = float.Parse(row["startPosY"]),
                            StartPosZ = float.Parse(row["startPosZ"]),
                            TeamStats = bool.Parse(row["teamStats"]),
                            EndPoint = float.Parse(row["endPoint"]),
                            BoxPosY = float.Parse(row["boxPosY"]),
                            PlayerPosY = float.Parse(row["playerPosY"]),
                        };
                        Define.WinChecks[winCheckId] = winCheckData;
                    }
                    break;

                case "Item":
                    foreach (var row in data)
                    {
                        var itemId = row["id"];
                        Define.ItemData itemData = new Define.ItemData
                        {
                            PosX = float.Parse(row["posX"]),
                            PosY = float.Parse(row["posY"]),
                            PosZ = float.Parse(row["posZ"]),
                            ScaleX = float.Parse(row["scaleX"]),
                            ScaleY = float.Parse(row["scaleY"]),
                            ScaleZ = float.Parse(row["scaleZ"]),
                            CreateCoolTime = float.Parse(row["createCoolTime"]),
                            GetPoint = int.Parse(row["getPoint"]),
                            MaxVal = int.Parse(row["maxVal"]),
                        };
                        Define.Items[itemId] = itemData;
                    }
                    break;

                case "Bullet":
                    foreach (var row in data)
                    {
                        var bulletId = row["id"];
                        Define.BulletData bulletData = new Define.BulletData
                        {
                            ScaleX = float.Parse(row["scaleX"]),
                            ScaleY = float.Parse(row["scaleY"]),
                            ScaleZ = float.Parse(row["scaleZ"]),
                            BulletSpeed = float.Parse(row["bulletSpeed"]),
                            GetPoint = int.Parse(row["getPoint"]),
                        };
                        Define.Bullets[bulletId] = bulletData;
                    }
                    break;
            }
        }

        Debug.Log("All data applied from multiple sheets.");
    }

    private async void Awake()
    {
        await ApplyData();
        Debug.Log($"Player1 exists after ApplyData: {Define.Players.ContainsKey("Player1")}");
        
        var playerSheetData = FindObjectOfType<PlayerSheetData>();
        if (playerSheetData != null)
        {
            playerSheetData.Init();
            Debug.Log($"Red1BasicStartPos: {playerSheetData.Red1BasicStartPos}");
        }

        Debug.Log("All data has been applied and initialized.");
    }


}
