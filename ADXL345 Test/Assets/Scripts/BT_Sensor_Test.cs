using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Data.SqlClient;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BT_Sensor_Test : MonoBehaviour
{
    private DataTable dt_bt_sensor;
    private DataTable dt_game_settings;
    const float SHOULDER_FACTOR = 1;
    const float HEAD_FACTOR =1;
    Db_ctrl db_Ctrl = new Db_ctrl(@"Data Source=127.0.0.1;Initial Catalog=cgmh_rh;User ID=sdc;Password=sdc@04792546");

	public GameObject R_cube, H_cube, L_cube;
	public Text[] Table;


	// Use this for initialization
    void Start()
    {

    }
	private int game_time_ct;

    // Update is called once per frame
    void Update()
	{
		//處理按下ESC關閉
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

//		float cube_base_y = cube.transform.position.y - (cube.transform.lossyScale.x * 0.5F * Math.Abs (Convert.ToSingle (Math.Sin (Math.PI * cube.transform.eulerAngles.z / 180F))));

		//控制上面的指示條
    
		dt_bt_sensor = db_Ctrl.dt_query ("select * from bt_sensor");

//		print ("db_Ctrl.CONNECT_STR : "+db_Ctrl.CONNECT_STR);
//		print ("dt_bt_sensor.Rows : "+dt_bt_sensor.Rows.ToString());

//		Table [0].text = dt_bt_sensor.Rows [0] ["XL"].ToString ();
//		Table [1].text = dt_bt_sensor.Rows [0] ["XH"].ToString ();
//		Table [2].text = dt_bt_sensor.Rows [0] ["XR"].ToString ();
//		Table [3].text = dt_bt_sensor.Rows [0] ["YL"].ToString ();
//		Table [4].text = dt_bt_sensor.Rows [0] ["YH"].ToString ();
//		Table [5].text = dt_bt_sensor.Rows [0] ["YR"].ToString ();
//		Table [6].text = dt_bt_sensor.Rows [0] ["ZL"].ToString ();
//		Table [7].text = dt_bt_sensor.Rows [0] ["ZH"].ToString ();
//		Table [8].text = dt_bt_sensor.Rows [0] ["ZR"].ToString ();

//		float sl = Convert.ToSingle (dt_bt_sensor.Rows [0] ["L_shoulder"]);
//		float sr = Convert.ToSingle (dt_bt_sensor.Rows [0] ["R_shoulder"]);
//		float hl = Convert.ToSingle (dt_bt_sensor.Rows [0] ["L_head"]);
		for (int i = 0; i < Table.Length; i++) {
//			print ("i : " + i + "," + Table [i].name);
			Table [i].text = dt_bt_sensor.Rows [0] [Table [i].name].ToString ();
		}

		R_cube.transform.rotation = Quaternion.Euler (Convert.ToSingle (dt_bt_sensor.Rows [0] ["XR"]), Convert.ToSingle (dt_bt_sensor.Rows [0] ["YR"]), Convert.ToSingle (dt_bt_sensor.Rows [0] ["ZR"]));
		H_cube.transform.rotation = Quaternion.Euler (Convert.ToSingle (dt_bt_sensor.Rows [0] ["XH"]), Convert.ToSingle (dt_bt_sensor.Rows [0] ["YH"]), Convert.ToSingle (dt_bt_sensor.Rows [0] ["ZH"]));
		L_cube.transform.rotation = Quaternion.Euler (Convert.ToSingle (dt_bt_sensor.Rows [0] ["XL"]), Convert.ToSingle (dt_bt_sensor.Rows [0] ["YL"]), Convert.ToSingle (dt_bt_sensor.Rows [0] ["ZL"]));

		float right_force = 0;
           
		Debug.Log (right_force);

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
}
