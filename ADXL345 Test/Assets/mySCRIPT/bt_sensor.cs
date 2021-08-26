using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Data.SqlClient;
using System;
using System.Timers;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Text;

public class bt_sensor : MonoBehaviour
{
    private static System.Timers.Timer sTimer;
    private DataTable dt_bt_sensor;
    private DataTable dt_game_settings;
    private int game_time_ct;
    private bool is_gameover = false;
    private Scene now_scene;
    const float SHOULDER_FACTOR = 1;
    const float HEAD_FACTOR =1;
    Db_ctrl db_Ctrl = new Db_ctrl(@"Data Source=127.0.0.1;Initial Catalog=cgmh_rh;User ID=sdc;Password=sdc@04792546");
    private static int is_trigger = 0;
    GameObject sphere1, cube1, gameover_wall;
    // Use this for initialization
    void Start()
    {
        now_scene = SceneManager.GetActiveScene();

        //get rigibody
        sphere1 = GameObject.Find("sphere1");
        cube1 = GameObject.Find("cube1");
        gameover_wall = GameObject.Find("gameover_wall");
        //set a timer
        if (sTimer != null)
        {
            sTimer.Stop();
            sTimer.Dispose();
        }
        SetTimer();
        //get game_setting
        get_game_settings();
        game_time_ct = 0;
        is_gameover = false;
        //set sphere mass 設定難度
        if (now_scene.name == "G1_scene")
        {
            //因為不轉動，純滑動，所以MASS調小
            sphere1.GetComponent<Rigidbody>().mass = (11 - Convert.ToSingle(dt_game_settings.Rows[0]["game_level"])) *1;
        }
        else if (now_scene.name == "G2_scene")
        {
            sphere1.GetComponent<Rigidbody>().mass = (11 - Convert.ToSingle(dt_game_settings.Rows[0]["game_level"])) * 10;
        }
   


    }
    private static void SetTimer()
    {
        // Create a timer with a two second interval.
        sTimer = new System.Timers.Timer(1000);
        // Hook up the Elapsed event for the timer. 
        sTimer.Elapsed += OnTimedEvent;
        sTimer.AutoReset = true;
        sTimer.Enabled = true;
    }

    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        is_trigger = 1;

    }
    // Update is called once per frame
    void Update()
    {
        float cube_base_y = cube1.transform.position.y - (cube1.transform.lossyScale.x * 0.5F * Math.Abs(Convert.ToSingle(Math.Sin(Math.PI * cube1.transform.eulerAngles.z / 180F))));
        if (this.name == "sphere1")
        {
            //處理按下ESC關閉
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            //控制上面的指示條
            GameObject Cube_balance_indicator = GameObject.Find("Cube_balance_indicator");
            GameObject Cube_balance_bar = GameObject.Find("Cube_balance_bar");
            dt_bt_sensor = db_Ctrl.dt_query("select * from bt_sensor");

            float sl = Convert.ToSingle(dt_bt_sensor.Rows[0]["L_shoulder"]);
            float sr = Convert.ToSingle(dt_bt_sensor.Rows[0]["R_shoulder"]);
            float hl = Convert.ToSingle(dt_bt_sensor.Rows[0]["L_head"]);

            float right_force = 0;
            if (now_scene.name == "G1_scene")//頭頸
            {
                right_force = (-1F) * HEAD_FACTOR * hl;
            }
            else if (now_scene.name == "G2_scene")//肩膀
            {
                //肩膀 使用兩邊SENSOR 平均值
                right_force = (((-1F) * SHOULDER_FACTOR * sl) + ((1F) * SHOULDER_FACTOR * sr)) / 2.0F;
            }
            if (Math.Abs(right_force) > 3)//3內當作ERROR不處理，
            {
                //給力
                sphere1.GetComponent<Rigidbody>().AddForce(Vector3.right * right_force);
                //指針位置
                float x_indicator_pos = (right_force / 125.0F) * 1F;
                Cube_balance_indicator.transform.position = Vector3.MoveTowards(Cube_balance_indicator.transform.position,
                 new Vector3(x_indicator_pos, Cube_balance_indicator.transform.position.y, Cube_balance_indicator.transform.position.z), 1);
                //改顏色
                float color_indicator = (right_force / 125.0F) * 255F;
                float green_color = (255F - Math.Abs(right_force) * 2) / 255F;
                if (green_color < 0F) green_color = 0F;
                float blue_color = (44F - Math.Abs(right_force) * 2) / 255F;
                if (blue_color < 0F) blue_color = 0F;
                Cube_balance_bar.GetComponent<Renderer>().material.color = new Color(Math.Abs(right_force * 3) / 255F, green_color, blue_color);
            }
            Debug.Log(right_force);
            //處理GAMEOVER
            if (sphere1.transform.position.y < (cube_base_y) && is_gameover == false)
            {
                is_gameover = true;
                change_text("gameover_text", "GAME OVER");
                change_text("count_down_text", "");
                update_game_settings(0, game_time_ct);
                if (sTimer != null)
                {
                    sTimer.Stop();
                    sTimer.Dispose();
                }

            }
        }
        else if (this.name == "target_and_score_text")
        {
            change_text("target_and_score_text", dt_game_settings.Rows[0]["score"] + "/" + dt_game_settings.Rows[0]["target_score"]);
        }
        //處理TIMER的事情(讀秒)
        if (is_trigger == 1 && this.name == "count_down_text")
        {
            //count down

            is_trigger = 0;
            int show_text = Convert.ToInt16(dt_game_settings.Rows[0]["balance_time"]) - game_time_ct;
            if (show_text < 0) show_text = 0;
            change_text("count_down_text", show_text.ToString());
            //使用時間顯示
            change_text("used_time_text", covert_to_min_format(game_time_ct + Convert.ToInt16(dt_game_settings.Rows[0]["usage_time"])));
            //得1分

            if (show_text == 0 && sphere1.transform.position.y > (cube_base_y))
            {
                update_game_settings(1, game_time_ct);
                change_text("gamewin_text", "得分!");
                sphere1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
                change_text("count_down_text", "");
                if (sTimer != null)
                {
                    sTimer.Stop();
                    sTimer.Dispose();
                }

            }
            game_time_ct++;
        }


    }
    private string covert_to_min_format(int totalSeconds)
    {
        int seconds = totalSeconds % 60;
        int minutes = totalSeconds / 60;
        return minutes.ToString().PadLeft(2, '0') + ":" + seconds.ToString().PadLeft(2, '0');

    }
    private void change_text(string gameobject_name, string t)
    {
        GameObject gn = GameObject.Find(gameobject_name);
        TextMesh tm = (TextMesh)gn.GetComponent(typeof(TextMesh));
        tm.text = t;
    }
    private void update_game_settings(int add_score, int add_use_t)
    {
        db_Ctrl.effrow_query("update game_setting set score=" + (add_score + Convert.ToInt16(dt_game_settings.Rows[0]["score"])).ToString() +
                    ",usage_time=" + (Convert.ToInt16(dt_game_settings.Rows[0]["usage_time"]) + add_use_t).ToString());
    }
    private void get_game_settings()
    {
        dt_game_settings = db_Ctrl.dt_query("select * from game_setting");
        string gt = dt_game_settings.Rows[0]["game_type"].ToString();


    }
    private void OnDestroy()
    {

    }
}
class Db_ctrl
{
    public string CONNECT_STR = @"Data Source=localhost\sqlexpress;Initial Catalog=cgmh_rh;User ID=sdc;Password=sdc@04792546";
    public Db_ctrl(string connect_str)
    {
        CONNECT_STR = connect_str;
    }

    public DataTable dt_query(string strSQL)
    {
        //escape string
        strSQL = strSQL.Replace("--", " ");
        strSQL = strSQL.Replace("\r\n", " ");
        strSQL = strSQL.Replace("'", "\'");
        //執行select
        string strConn = CONNECT_STR;
        //建立連接
        SqlConnection myConn = new SqlConnection(strConn);
        //打開連接
        myConn.Open();
        SqlCommand cmd = new SqlCommand(strSQL, myConn);
        //得到Data結果集
        var dt = new DataTable();
        try
        {
            SqlDataReader myDataReader = cmd.ExecuteReader();
            dt.Load(myDataReader);
        }
        catch { }
        cmd.Dispose();
        myConn.Close();
        myConn.Dispose();
        return dt;
    }

    public int effrow_query(string strSQL)
    {
        //escape string
        strSQL = strSQL.Replace("--", " ");
        strSQL = strSQL.Replace("\r\n", " ");
        strSQL = strSQL.Replace("'", "\'");
        //執行select
        string strConn = CONNECT_STR;
        //建立連接
        SqlConnection myConn = new SqlConnection(strConn);
        //打開連接
        myConn.Open();
        SqlCommand cmd = new SqlCommand(strSQL, myConn);
        //得到Data結果集
        int effrows = 0;
        try
        {
            //影響幾列
            effrows = cmd.ExecuteNonQuery();
        }
        catch
        {
        }
        cmd.Dispose();
        myConn.Close();
        myConn.Dispose();
        return effrows;
    }
    public DataTable dt_query_without_connect(string strSQL, SqlConnection myConn)
    {
        //escape string
        strSQL = strSQL.Replace("--", " ");
        strSQL = strSQL.Replace("\r\n", " ");
        strSQL = strSQL.Replace("'", "\'");
        SqlCommand cmd = new SqlCommand(strSQL, myConn);

        //得到Data結果集
        DataTable dt = new DataTable();
	
        try
        {
            SqlDataReader myDataReader = cmd.ExecuteReader();
            dt.Load(myDataReader);
        }
        catch { }
        cmd.Dispose();
        return dt;
    }
    public int effrow_query_without_connect(string strSQL, SqlConnection myConn)
    {
        //escape string
        strSQL = strSQL.Replace("--", " ");
        strSQL = strSQL.Replace("\r\n", " ");
        strSQL = strSQL.Replace("'", "\'");
        SqlCommand cmd = new SqlCommand(strSQL, myConn);
        //得到Data結果集
        int effrows = 0;
        try
        {
            //影響幾列
            effrows = cmd.ExecuteNonQuery();
        }
        catch
        {
        }
        cmd.Dispose();
        return effrows;
    }
    private List<Dictionary<string, object>> data_table_to_rows(DataTable dt)
    {
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
        Dictionary<string, object> row;
        foreach (DataRow dr in dt.Rows)
        {
            row = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                if (dr[col].GetType() == typeof(DateTime))
                {
                    row.Add(col.ColumnName, Convert.ToDateTime(dr[col].ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    row.Add(col.ColumnName, dr[col]);
                }
            }
            rows.Add(row);
        }
        return rows;
        //
    }
}
