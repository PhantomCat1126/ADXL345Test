using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class change_scene : MonoBehaviour {
    Db_ctrl db_Ctrl = new Db_ctrl(@"Data Source=127.0.0.1;Initial Catalog=cgmh_rh;User ID=sdc;Password=sdc@04792546");
    // Use this for initialization
    void Start () {
       Scene now_scene = SceneManager.GetActiveScene();
        string gt = db_Ctrl.dt_query("select * from game_setting").Rows[0]["game_type"].ToString();
            if (gt == "g1" && now_scene.name != "G1_scene")
            {
              
                SceneManager.LoadScene("G1_scene", LoadSceneMode.Single);
            }
            else if (gt == "g2" && now_scene.name != "G2_scene")
            {
         
                SceneManager.LoadScene("G2_scene", LoadSceneMode.Single);
            }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
