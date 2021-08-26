using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine.Windows;
using UnityEngine.UI;

public class From : MonoBehaviour {

	private Queue<byte> inputBuffer;
	private DataTable dt_bt_sensor;
	//for math
	private double[] l_shoulder_vector = { 0, 0, 0, 0 };//第4個是NORM
	private double[] r_shoulder_vector = { 0, 0, 0, 0 };//第4個是NORM
	private double[] l_head_vector = { 0, 0, 0, 0 };//第4個是NORM

	//connect db
	Db_ctrl db_ctrl = new Db_ctrl(@"Data Source=.\sqlexpress;Initial Catalog=cgmh_rh;User ID=sdc;Password=sdc@04792546");
	SqlConnection myConn;

	private SerialPort sp;
	private Thread receiveThread;  //用于接收消息的线程
	private Thread sendThread;     //用于发送消息的线程

	// Use this for initialization
	void Start () {
		sp = new SerialPort("COM4", 9600);  //这里的"COM4"是我的设备，可以在设备管理器查看。
		//print();
		sp.ReadTimeout = 500;
		sp.Open();
	}

	// Update is called once per frame
	void Update () {

	}

	//启动接受和发送数据的线程，根据调试的需要注释掉一部分即可。
	private void startThread() {
		receiveThread = new Thread(ReceiveThread);
		receiveThread.IsBackground = true;
		receiveThread.Start();
		sendThread = new Thread(SendThread);
		sendThread.IsBackground = true;
		sendThread.Start();
	}
	private void SendThread()
	{
		int i=0;
		while (true)
		{
			Thread.Sleep(20);
			this.sp.DiscardInBuffer();
			if (i > 255)
				i = 0;
			sp.WriteLine(i++.ToString());
			print(i++.ToString());
		}
	}
	private void ReceiveThread()
	{
		while (true)
		{
			if(this.sp != null && this.sp.IsOpen)
			{
				try
				{
					String strRec = sp.ReadLine();            //SerialPort读取数据有多种方法，我这里根据需要使用了ReadLine()
					print("Receive From Serial: " +strRec);
				}
				catch
				{
					//continue;
				}
			}
		}
	}



	public Dropdown DR;
	private void Form1_Load(object sender, EventArgs e)
	{  //combo box 列出所有可能的COM PORT
//		for (int i = 0; i < SerialPort.GetPortNames().Length; i++){
//			DR.options.Add(SerialPort.GetPortNames()[i].ToString());
//		}

		//serial port ini
		inputBuffer = new Queue<byte>();
		//ini
		dt_bt_sensor = new DataTable();
		string[] columns = { "XR", "YR", "ZR", "XL", "YL", "ZL", "XH", "YH", "ZH",
			"XR_bias", "YR_bias", "ZR_bias", "XL_bias", "YL_bias", "ZL_bias", "XH_bias", "YH_bias", "ZH_bias",
			"XR_comp", "YR_comp", "ZR_comp", "XL_comp", "YL_comp", "ZL_comp", "XH_comp", "YH_comp", "ZH_comp"
			,"L_shoulder","R_shoulder"
			,"L_head"};
		for (int i = 0; i < columns.Length; i++)
		{
			System.Data.DataColumn newColumn = new System.Data.DataColumn(columns[i], typeof(System.Double));
			newColumn.DefaultValue = 0;
			dt_bt_sensor.Columns.Add(newColumn);
		}
		dt_bt_sensor.Rows.Add();
		//db CONN
		//打開連接
		myConn = new SqlConnection(db_ctrl.CONNECT_STR);
		myConn.Open();

	}

	private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
	{
		try
		{
			string PortData = "";
			byte signal = 0;
			while (sp.BytesToRead > 0)
			{
				try
				{
					signal = (byte)sp.ReadByte();
					inputBuffer.Enqueue(signal);
					if (signal == Convert.ToChar(','))
					{
						byte[] receive_data = inputBuffer.ToArray();
						PortData = (Encoding.ASCII.GetString(inputBuffer.ToArray()));
						PortData = PortData.Replace(",", "");
						inputBuffer.Clear();
						if (PortData.Split(':').Length > 0)
						{
							if (PortData != "")
							{
								if (PortData.Substring(0, 2) == "ZH")
								{
									PortData = PortData.Replace("XR", "");
									dt_bt_sensor.Rows[0]["ZH"] = Convert.ToDouble(PortData.Split(':')[1]);
									dt_bt_sensor.Rows[0]["XR"] = Convert.ToDouble(PortData.Split(':')[2]);
									//compensate data writing
									dt_bt_sensor.Rows[0]["ZH_comp"] = Convert.ToDouble(dt_bt_sensor.Rows[0]["ZH"]) + Convert.ToDouble(dt_bt_sensor.Rows[0]["ZH_bias"]);
									dt_bt_sensor.Rows[0]["XR_comp"] = Convert.ToDouble(dt_bt_sensor.Rows[0]["XR"]) + Convert.ToDouble(dt_bt_sensor.Rows[0]["XR_bias"]);
									//write db
									db_ctrl.effrow_query_without_connect("update bt_sensor set ZH=" + dt_bt_sensor.Rows[0]["ZH"] +
										",XR=" + dt_bt_sensor.Rows[0]["XR"] +
										",ZH_comp=" + dt_bt_sensor.Rows[0]["ZH_comp"] + ", XR_comp = " + dt_bt_sensor.Rows[0]["XR_comp"], myConn);
								}
								else
								{
									string col_name = PortData.Split(':')[0].ToString();
									string col_name_comp = col_name + "_comp";
									string col_name_bias = col_name + "_bias";
									dt_bt_sensor.Rows[0][col_name] = Convert.ToDouble(PortData.Split(':')[1]);
									dt_bt_sensor.Rows[0][col_name_comp] = Convert.ToDouble(dt_bt_sensor.Rows[0][col_name]) + Convert.ToDouble(dt_bt_sensor.Rows[0][col_name_bias]);
									db_ctrl.effrow_query_without_connect("update bt_sensor set " + col_name + "=" + dt_bt_sensor.Rows[0][col_name] +
										"," + col_name_comp + "=" + dt_bt_sensor.Rows[0][col_name_comp], myConn);
								}
								//=====計算各向量的投影量
								if (l_shoulder_vector[3] != 0)
								{
									double L_shoulder_project_value = (
										l_shoulder_vector[0] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["XL_comp"])) +
										l_shoulder_vector[1] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["YL_comp"])) +
										l_shoulder_vector[2] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["ZL_comp"]))) / l_shoulder_vector[3];
									dt_bt_sensor.Rows[0]["L_shoulder"] = L_shoulder_project_value;
									db_ctrl.effrow_query_without_connect("update bt_sensor set L_shoulder=" + L_shoulder_project_value, myConn);
								}
								if (r_shoulder_vector[3] != 0)
								{
									double R_shoulder_project_value = (
										r_shoulder_vector[0] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["XR_comp"])) +
										r_shoulder_vector[1] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["YR_comp"])) +
										r_shoulder_vector[2] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["ZR_comp"]))) / r_shoulder_vector[3];
									dt_bt_sensor.Rows[0]["R_shoulder"] = R_shoulder_project_value;
									db_ctrl.effrow_query_without_connect("update bt_sensor set R_shoulder=" + R_shoulder_project_value, myConn);
								}
								if (l_head_vector[3] != 0)
								{
									double L_head_project_value = (
										l_head_vector[0] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["XH_comp"])) +
										l_head_vector[1] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["YH_comp"])) +
										l_head_vector[2] * (Convert.ToDouble(dt_bt_sensor.Rows[0]["ZH_comp"]))) / l_head_vector[3];
									dt_bt_sensor.Rows[0]["L_head"] = L_head_project_value;
									db_ctrl.effrow_query_without_connect("update bt_sensor set L_head=" + L_head_project_value, myConn);
								}
								//=====監測要不要OPEN GAME
								DataTable dt_open_game = db_ctrl.dt_query_without_connect("select open_game from game_setting", myConn);
/*								if (Convert.ToInt16(dt_open_game.Rows[0]["open_game"]) == 1)
								{
									Thread open_game_TH = new Thread(new ThreadStart(open_game));
									open_game_TH.Priority = ThreadPriority.BelowNormal;
									open_game_TH.IsBackground = true;
									open_game_TH.Start();

									db_ctrl.effrow_query_without_connect("update game_setting set open_game=0", myConn);
								}*/
								Console.WriteLine("rec:" + PortData);
							}

						}

					}
				}
				catch (Exception)
				{
					Console.WriteLine("write datatable occur problem");

				}

			}
		}
		catch (TimeoutException)
		{
			return; // Data not ready yet
		}
	}
	//確認連接按鈕
	private void button1_Click(object sender, EventArgs e)
	{
		try
		{
			if (sp.IsOpen)
			{
				sp.Close();
			}
//			sp.PortName = Dropdown.SelectedItem.ToString();

			sp.Open();
			Console.WriteLine(sp.PortName + " connected..");
		}
		catch (Exception)
		{
			Console.WriteLine("connect wrong");
		}

	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		try
		{
			//dataGridView1.DataSource = dt_bt_sensor;
			//橫
			DataTable dtNew = new DataTable();
			for (int i = 0; i < dt_bt_sensor.Rows.Count; i++)
			{
				dtNew.Columns.Add("值");
			}

			for (int i = 0; i < dt_bt_sensor.Columns.Count; i++)
			{
				DataRow dr = dtNew.NewRow();
				dtNew.Rows.Add(dr);
			}

			for (int i = 0; i < dt_bt_sensor.Rows.Count; i++)
			{
				for (int j = 0; j < dt_bt_sensor.Columns.Count; j++)
				{
					dtNew.Rows[j][i] = dt_bt_sensor.Rows[i][j].ToString();
				}
			}
			dtNew.Columns.Add("名稱");
			for (int i = 0; i < dtNew.Rows.Count; i++)
			{
				dtNew.Rows[i]["名稱"] = dt_bt_sensor.Columns[i].ColumnName;
			}
//			Scrollbar.DataSource = dtNew;

		}
		catch (Exception err)
		{
			Console.WriteLine(err.ToString());
		}


	}

	//歸零
	private void button2_Click(object sender, EventArgs e)
	{
		dt_bt_sensor.Rows[0]["XR_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["XR"]);
		dt_bt_sensor.Rows[0]["YR_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["YR"]);
		dt_bt_sensor.Rows[0]["ZR_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["ZR"]);

		dt_bt_sensor.Rows[0]["XL_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["XL"]);
		dt_bt_sensor.Rows[0]["YL_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["YL"]);
		dt_bt_sensor.Rows[0]["ZL_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["ZL"]);

		dt_bt_sensor.Rows[0]["XH_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["XH"]);
		dt_bt_sensor.Rows[0]["YH_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["YH"]);
		dt_bt_sensor.Rows[0]["ZH_bias"] = -1.0 * Convert.ToDouble(dt_bt_sensor.Rows[0]["ZH"]);
	}
	//定義肩膀向左
	private void button3_Click(object sender, EventArgs e)
	{
		double x = Convert.ToDouble(dt_bt_sensor.Rows[0]["XL_comp"]);
		double y = Convert.ToDouble(dt_bt_sensor.Rows[0]["YL_comp"]);
		double z = Convert.ToDouble(dt_bt_sensor.Rows[0]["ZL_comp"]);
		l_shoulder_vector[0] = x;
		l_shoulder_vector[1] = y;
		l_shoulder_vector[2] = z;
		l_shoulder_vector[3] = Math.Sqrt(x * x + y * y + z * z);
		string text = "肩膀左：\r\nunit vector = (" + Math.Round((x / l_shoulder_vector[3]), 2) +
			"," + Math.Round((y / l_shoulder_vector[3]), 2) +
			"," + Math.Round((z / l_shoulder_vector[3]), 2) + ")";


	}
	//定義肩膀向右
	private void button4_Click(object sender, EventArgs e)
	{
		double x = Convert.ToDouble(dt_bt_sensor.Rows[0]["XR_comp"]);
		double y = Convert.ToDouble(dt_bt_sensor.Rows[0]["YR_comp"]);
		double z = Convert.ToDouble(dt_bt_sensor.Rows[0]["ZR_comp"]);
		r_shoulder_vector[0] = x;
		r_shoulder_vector[1] = y;
		r_shoulder_vector[2] = z;
		r_shoulder_vector[3] = Math.Sqrt(x * x + y * y + z * z);
		string text = "肩膀右：\r\nunit vector = (" + Math.Round((x / l_shoulder_vector[3]), 2) +
			"," + Math.Round((y / l_shoulder_vector[3]), 2) +
			"," + Math.Round((z / l_shoulder_vector[3]), 2) + ")";

	}
	//定義頭頸向左
	private void button5_Click(object sender, EventArgs e)
	{
		double x = Convert.ToDouble(dt_bt_sensor.Rows[0]["XH_comp"]);
		double y = Convert.ToDouble(dt_bt_sensor.Rows[0]["YH_comp"]);
		double z = Convert.ToDouble(dt_bt_sensor.Rows[0]["ZH_comp"]);
		l_head_vector[0] = x;
		l_head_vector[1] = y;
		l_head_vector[2] = z;
		l_head_vector[3] = Math.Sqrt(x * x + y * y + z * z);
		string text = "頭頸左：\r\nunit vector = (" + Math.Round((x / l_shoulder_vector[3]), 2) +
			"," + Math.Round((y / l_shoulder_vector[3]), 2) +
			"," + Math.Round((z / l_shoulder_vector[3]), 2) + ")";
	}
}
