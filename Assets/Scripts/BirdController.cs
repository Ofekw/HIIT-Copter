﻿using UnityEngine;
using System.Collections;
using System;

public class BirdController : MonoBehaviour {

	public GameObject ringCollider;

	public float forwardMovement = 2f;
	public int upAngle=45, downAngle=280; //-80 degrees
    public float forceMultiplier = 10f;

    public int workoutPhase = 0; //0 menu screen, 1 warmup, 2 intervals

	public Vector3 startingPosition;
	[Range (-90,90)] public int zRotation;
	public Quaternion startingRotation;

	private bool waitingForPlayerToStart, scoreboard;

	private Engine engine;
    private int fallCount = 0;
    private int scoreShowingCount = 0;
	private float rotationAmount;

    public uint warmupDistance;
    public uint warmupPowerSum = 0;
    public int warmupCount = 0;
    public float warmupAverage = 0;

    public float timeBetweenlogging = 1.0f;
    private float time;


    void Awake(){
		engine = GameObject.Find("GameObjectSpawner").GetComponent<Engine>();
	}

	// Use this for initialization
	void Start () {
		transform.position = startingPosition;
		startingRotation = transform.rotation;

        GetComponent<Rigidbody>().useGravity = false;
		waitingForPlayerToStart = true;
        GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
        time = timeBetweenlogging;

    }

    void Update() {
        //track time for polling logs every second
        time -= Time.deltaTime;

        if (time <= 0)
        { 
            LogData();
            time = timeBetweenlogging;
        }
        //Recenter oculus when F12 pressed
        if (Input.GetKey(KeyCode.F12))
        {
            UnityEngine.VR.InputTracking.Recenter();
        }

        if (waitingForPlayerToStart){
            //Reset game variables when space pressed at start of game
			if(Input.GetKeyDown(KeyCode.Space)){
				scoreShowingCount = 0;

                engine.StartGame();

				waitingForPlayerToStart = false;

                //GetComponent<Rigidbody>().freezeRotation = false;
				GetComponent<Rigidbody>().useGravity = true;
			}

		}else{
            //Remove this if block, only for testing using space bar
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    gameObject.GetComponent<RowingMachineController>().waitingRow = false;
            //    uint rowBoost = GetComponent<RowingMachineController>().waitingDistance;


            //    if (GetComponent<Rigidbody>().velocity.y < 0)
            //    {
            //        GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
            //    }
            //    GetComponent<Rigidbody>().AddForce(Vector3.up * 30.0f / 5.0f, ForceMode.Impulse);
            //    if (transform.rotation.eulerAngles.z < upAngle)
            //    {
            //        rotationAmount = upAngle - transform.rotation.eulerAngles.z;
            //        transform.RotateAround(transform.position, Vector3.forward, rotationAmount * .5f);
            //    }
            //    else if (transform.rotation.eulerAngles.z > 180)
            //    {
            //        rotationAmount = 360 - (transform.rotation.eulerAngles.z - upAngle);
            //        transform.RotateAround(transform.position, Vector3.forward, rotationAmount * .5f);
            //    }
            //    engine.AddToCurrentScore(50);
            //    fallCount = 0;
            //}
            //else 
            if (engine.isWarmingUp)
            {
                //Warmup period, time configured witin the GameObjectSpawner
                if (gameObject.GetComponent<RowingMachineController>().waitingRow)
                {
                    warmupPowerSum += GetComponent<RowingMachineController>().currentForce;
                    warmupCount++;

                    gameObject.GetComponent<RowingMachineController>().waitingRow = false;

                    //Adjust height based on rowing machine force
                    GetComponent<Rigidbody>().velocity = new Vector3(0, (GetComponent<RowingMachineController>().currentForce) * forceMultiplier);
                    fallCount = 0;
                    Debug.Log(GetComponent<RowingMachineController>().currentForce);
                    Debug.Log("warming up");

                }
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                gameObject.GetComponent<RowingMachineController>().waitingRow = false;

				if(GetComponent<Rigidbody>().velocity.y<0){
					GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x,0,0);
				}
                GetComponent<Rigidbody>().AddForce(Vector3.up * (GetComponent<RowingMachineController>().currentForce / warmupAverage) * forceMultiplier, ForceMode.Impulse);

                engine.AddToCurrentScore(50);
				fallCount = 0;
            }
            else if (gameObject.GetComponent<RowingMachineController>().waitingRow)
            {
                gameObject.GetComponent<RowingMachineController>().waitingRow = false;

                if (GetComponent<Rigidbody>().velocity.y < 0)
                {
                    GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, 0);
                }
                GetComponent<Rigidbody>().AddForce(Vector3.up * (GetComponent<RowingMachineController>().currentForce / warmupAverage) * forceMultiplier, ForceMode.Impulse);

                Debug.Log(GetComponent<RowingMachineController>().currentForce / warmupAverage);

                engine.AddToCurrentScore(50);
                fallCount = 0;
            }

        }

        if (engine.isWarmingUp)
        {
            if (GetComponent<Rigidbody>().velocity.y < 0)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

            }
            else if (GetComponent<Rigidbody>().velocity.y > 3.0f)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0, 3.0f, 0);
            }
            else
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);

            }
        }
        else
        {
            if (GetComponent<Rigidbody>().velocity.y < -3.0f)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, -3.0f, 0);
            }
            if (GetComponent<Rigidbody>().velocity.y > 10.0f)
            {
                Debug.Log("Max y reached");
                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 10.0f, 0);
            }
        }
	}

	void FixedUpdate(){
        //move player foward at constant rate
		if(!waitingForPlayerToStart){
            uint rowDistance = GameObject.FindGameObjectWithTag("RowingMachine").GetComponent<Rower>().rowDistance;

            if (rowDistance > warmupDistance)
            {
                transform.position += Vector3.right * Time.fixedDeltaTime * forwardMovement;
               
            }
		}
	}

	void BirdReset(){
		GetComponent<Rigidbody>().velocity=Vector3.zero;
		transform.position = startingPosition;
		GetComponent<Rigidbody>().rotation = startingRotation;
		GetComponent<Rigidbody>().freezeRotation = true;
	}

	void OnTriggerEnter(Collider scorebox){
        //On collision with ring, create new ring and increment score by 500, remove ring
        GameObject.FindGameObjectWithTag("pipecreator").GetComponent<RingGenerator>().NewRing();

        engine.AddToCurrentScore(500);
		Destroy (scorebox.gameObject);
	}

    void LogData()
    {
        //Logging system for force, distance and heartrate.
        Power force = new Power(Time.time.ToString(), GetComponent<RowingMachineController>().currentForce, GameObject.FindGameObjectWithTag("pipecreator").GetComponent<RingGenerator>().isHighIntensity);
        Distance distance = new Distance(Time.time.ToString(), GetComponent<RowingMachineController>().distanceTravelled);
        HeartRate heartRate = new HeartRate( Time.time.ToString(), GameObject.FindGameObjectWithTag("HRMonitor").GetComponent<HeartRateService>().heartRate);

        var logger = GetComponent<LoggerService>();
        logger.heartRate.Enqueue(heartRate);
        logger.distance.Enqueue(distance);
        logger.power.Enqueue(force);

        logger.Log();
    }
}

[Serializable]
public class HeartRate
{
    public String time;
    public double heartrate;

    public HeartRate(String time, double data)
    {
        this.time = time;
        this.heartrate = data;
    }

    public override string ToString()
    {
        return this.time + "," + this.heartrate;
    }
}

[Serializable]
public class Power
{
    public String time;
    public bool intervalType;
    public double power;

    public Power(String time, double data, bool interval)
    {
        this.time = time;
        this.power = data;
        this.intervalType = interval;
    }

    public override string ToString()
    {
        return this.time + "," + this.power + "," + this.intervalType;
    }
}
[Serializable]
public class Distance
{
    public String time;
    public double distance;
    public Distance(String time, double data)
    {
        this.time = time;
        this.distance = data;
    }

    public override string ToString()
    {
        return this.time + "," + this.distance;
    }
}
