using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamTest : MonoBehaviour {


	public Texture Texture;
	public RawImage background;

	private bool isOK;
	private WebCamTexture webcamTexture;


	void Start()
	{
		webcamTexture  = new WebCamTexture();
		isOK = true;
		Test();
		Go();
	}

	public void Go()
	{

		//如果有后置摄像头，调用后置摄像头  
		for (int i = 0; i < WebCamTexture.devices.Length; i++)
		{
			//如果是前置摄像机
			if (WebCamTexture.devices[i].isFrontFacing && isOK)
			{
				webcamTexture.deviceName = WebCamTexture.devices[i].name;
				break;
			}
			//如果是后置摄像机
			else if (!WebCamTexture.devices[i].isFrontFacing && !isOK)
			{
				webcamTexture.deviceName = WebCamTexture.devices[i].name;
				break;
			}
		}
		background.texture = webcamTexture;
		webcamTexture.Play();
	}

	public void Test()
	{
		if (webcamTexture.isPlaying)
		{
			webcamTexture.Stop();
		}
	}
/*
	void OnGUI()
	{
		if (GUI.Button(new Rect(0,0,200,200),"Front"))
		{
			isOK = true;
			Test();
			Go();
		}
		if (GUI.Button(new Rect(0, 200, 200, 200), "Back"))
		{
			isOK = false;
			Test();
			Go();
		}
	}
	*/	
}
