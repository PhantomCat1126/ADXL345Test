using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Timers;
public class fly_bird_ctrl : MonoBehaviour {
    private static System.Timers.Timer aTimer;
   private  GameObject fly_chicken;
    private  List<GameObject> myNodes;
    private  GameObject egg;
    private static bool is_gen_egg = false;
    private float rnd_x, rnd_y;
    // Use this for initialization
    void Start () {
        //fly_chicken = GameObject.Find("fly_chicken");
        //rnd_x = UnityEngine.Random.Range(-5.0f, 0.0f);
        //rnd_y = UnityEngine.Random.Range(10.0f, 12.0f);
        //myNodes = new List<GameObject>();
        ////set a timer
        //if (aTimer != null)
        //{
        //    aTimer.Stop();
        //    aTimer.Dispose();
        //}

        //SetTimer();
      

    }

    // Update is called once per frame
    void Update () {
        //生蛋
        if (is_gen_egg)
        {
            create_a_egg();
            is_gen_egg = false;
            //雞飛axis
            rnd_x = UnityEngine.Random.Range(-5.0f, 0.0f);
            rnd_y = UnityEngine.Random.Range(10.0f, 12.0f);
        }
        //雞飛
        fly_chicken.transform.position = Vector3.Lerp(fly_chicken.transform.position,
            new Vector3(rnd_x, rnd_y , fly_chicken.transform.position.z), 0.1F);
        //刪掉雞蛋
        for (int i = myNodes.Count-1; i >=0; i--)
        {
            if (myNodes[i].transform.position.y < -10f)
            {
                Destroy(myNodes[i]);
                myNodes.RemoveAt(i);
            }
        }

    }
    private void create_a_egg()
    {
        egg = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        egg.transform.position = Vector3.MoveTowards(egg.transform.position,
        new Vector3(fly_chicken.transform.position.x+3.7F, fly_chicken.transform.position.y - 3, fly_chicken.transform.position.z), 10);
        Rigidbody egg_rigibody = egg.AddComponent<Rigidbody>(); // Add the rigidbody.
        egg_rigibody.mass = 1; // Set the GO's mass to 1 via the Rigidbody.
        egg_rigibody.useGravity = true;
        egg_rigibody.drag = 0.5F;
        egg_rigibody.constraints = RigidbodyConstraints.FreezePositionZ;
        PhysicMaterial dydy_ball = new PhysicMaterial();
        dydy_ball.bounciness = 0F;
        dydy_ball.bounceCombine = PhysicMaterialCombine.Minimum;
        dydy_ball.staticFriction = 0F;
        dydy_ball.dynamicFriction = 0F;
        egg.GetComponent<SphereCollider>().material = dydy_ball;
        myNodes.Add(egg);
    }
    private static void SetTimer()
    {
        // Create a timer with a two second interval.
        aTimer = new System.Timers.Timer(2000);
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        is_gen_egg = true;

    }

}
