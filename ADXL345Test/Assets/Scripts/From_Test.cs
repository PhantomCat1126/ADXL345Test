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

public class From_Test : MonoBehaviour {

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

		sp.ReadTimeout = 500;
		sp.Open();
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
		// Update is called once per frame
	void Update () {
		
	}

	public Dropdown DR;
	private void Form1_Load(object sender, EventArgs e)
	{
		//combo box 列出所有可能的COM PORT
//		for (int i = 0; i < SerialPort.GetPortNames().Length; i++){
//			DR.options.Add(SerialPort.GetPortNames()[i].ToString());		}

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
}
