using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSheetData : MonoBehaviour
{
    public void Init()
    {
        BasicStartPosition();
        GunnerStartPosition();
        RunnerStartPosition();
        WinCheckStartPosition();
        PlayerValue();
        ItemValue();
        BulletValue();
    }
    
    //*===================================================================================================================================
    #region Players Position

    //* Basic(Lobby)
    public Vector3 Red1BasicStartPos;
    public Vector3 Red2BasicStartPos;
    public Vector3 Blue1BasicStartPos;
    public Vector3 Blue2BasicStartPos;
    public void BasicStartPosition()
    {
        Red1BasicStartPos = new Vector3(Define.Players["Player1"].StartPosX, Define.Players["Player1"].StartPosY, Define.Players["Player1"].StartPosZ);
        Red2BasicStartPos = new Vector3(Define.Players["Player2"].StartPosX, Define.Players["Player2"].StartPosY, Define.Players["Player2"].StartPosZ);
        Blue1BasicStartPos = new Vector3(Define.Players["Player3"].StartPosX, Define.Players["Player3"].StartPosY, Define.Players["Player3"].StartPosZ);
        Blue2BasicStartPos = new Vector3(Define.Players["Player4"].StartPosX, Define.Players["Player4"].StartPosY, Define.Players["Player4"].StartPosZ);
    }

    //*===================================================================================================================================
    //* Gunner
    public Vector3 RedGunnerStartPos;
    public Vector3 BlueGunnerStartPos;
    public void GunnerStartPosition()
    {
        RedGunnerStartPos = new Vector3(Define.Gunners["Red_Gunner"].StartPosX, Define.Gunners["Red_Gunner"].StartPosY, Define.Gunners["Red_Gunner"].StartPosZ);
        BlueGunnerStartPos = new Vector3(Define.Gunners["Blue_Gunner"].StartPosX, Define.Gunners["Blue_Gunner"].StartPosY, Define.Gunners["Blue_Gunner"].StartPosZ);
    }
    
    //*===================================================================================================================================
    //* Runner
    public Vector3 RedRunnerStartPos;
    public Vector3 BlueRunnerStartPos;
    public void RunnerStartPosition()
    {
        RedRunnerStartPos = new Vector3(Define.Runners["Red_Runner"].StartPosX, Define.Runners["Red_Runner"].StartPosY, Define.Runners["Red_Runner"].StartPosZ);
        BlueRunnerStartPos = new Vector3(Define.Runners["Blue_Runner"].StartPosX, Define.Runners["Blue_Runner"].StartPosY, Define.Runners["Blue_Runner"].StartPosZ);
    }
    
    //*===================================================================================================================================
    //* WinCheck(result)
    public Vector3 Red1WinCheckStartPos;
    public Vector3 Red2WinCheckStartPos;
    public Vector3 Blue1WinCheckStartPos;
    public Vector3 Blue2WinCheckStartPos;
    public void WinCheckStartPosition()
    {
        Red1BasicStartPos = new Vector3(Define.WinChecks["Player1"].StartPosX, Define.WinChecks["Player1"].StartPosY, Define.WinChecks["Player1"].StartPosZ);
        Red2BasicStartPos = new Vector3(Define.WinChecks["Player2"].StartPosX, Define.WinChecks["Player2"].StartPosY, Define.WinChecks["Player2"].StartPosZ);
        Blue1BasicStartPos = new Vector3(Define.WinChecks["Player3"].StartPosX, Define.WinChecks["Player3"].StartPosY, Define.WinChecks["Player3"].StartPosZ);
        Blue2BasicStartPos = new Vector3(Define.WinChecks["Player4"].StartPosX, Define.WinChecks["Player4"].StartPosY, Define.WinChecks["Player4"].StartPosZ);
    }
    #endregion




    //*===================================================================================================================================
    #region Player Value
    public float BasicMoveSpeed;
    public float BasicReadyPressTime;

    public float GunnerMoveSpeed;
    public float GunnerDashSpeed;
    public float GunnerFireCoolTime;

    public float RunnerMoveSpeed;
    public float RunnerDashForce;
    public float RunnerDashCoolTime;
    public void PlayerValue()
    {
        BasicMoveSpeed = Define.Players["Player1"].MoveSpeed;
        BasicReadyPressTime = Define.Players["Player1"].ReadyPressTime;

        GunnerMoveSpeed = Define.Gunners["Red_Gunner"].MoveSpeed;
        GunnerDashSpeed = Define.Gunners["Red_Gunner"].DashSpeed;
        GunnerFireCoolTime = Define.Gunners["Red_Gunner"].FiringCoolTime;

        RunnerMoveSpeed = Define.Runners["Red_Runner"].MoveSpeed;
        RunnerDashForce = Define.Runners["Red_Runner"].DashForce;
        RunnerDashCoolTime = Define.Runners["Red_Runner"].DashCoolTime;
    }
    
    #endregion




    //*===================================================================================================================================
    #region Item(Boom, Coin)
    public struct ItemInfo
    {
        int createCoolTime;
        int getPoint;
        int maxVal;
        public ItemInfo(int time, int point, int Value)
        {
            createCoolTime = time;
            getPoint = point;
            maxVal = Value;
        }
    }
    
    public ItemInfo Boom;
    public ItemInfo Coin;
    public void ItemValue()
    {
        Boom = new ItemInfo((int)Define.Items["Boom"].CreateCoolTime, Define.Items["Boom"].GetPoint, Define.Items["Boom"].MaxVal);
        Coin = new ItemInfo((int)Define.Items["Coin"].CreateCoolTime, Define.Items["Coin"].GetPoint, Define.Items["Coin"].MaxVal);
    }
    #endregion




    //*===================================================================================================================================
    #region Bullet
    public Vector3 BulletScale;
    public float BulletSpeed;
    public int BulletGetPoint;
    public void BulletValue()
    {
        BulletScale = new Vector3(Define.Bullets["Bullet"].ScaleX, Define.Bullets["Bullet"].ScaleY, Define.Bullets["Bullet"].ScaleZ);
        BulletSpeed = Define.Bullets["Bullet"].BulletSpeed;
        BulletGetPoint = Define.Bullets["Bullet"].GetPoint;
    }
    
    #endregion
}
