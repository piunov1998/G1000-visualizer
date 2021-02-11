//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Plane : MonoBehaviour
{
    public float now_time;
    public float past_time;
    public DateTime human_time = new DateTime(0);
    public bool paused = false;
    public double[] ZeroPoint;
    public Vector3 world_position = new Vector3(0, 0, 0);
    Quaternion rotate;

    void Start()
    {
        if (DataHolder.Opened)
        {    foreach (double[] row in MainScript.param)
            {
                if ((row[0] != 0) && (row[0] != 0))
                {
                    ZeroPoint = new double[2] {row[0], row[1]};
                    //GameObject.Find("Plane").transform.position = new Vector3(0, row[2] * 0.3048f, 0);
                    return;
                }
            }
        }
    }

    void Update()
    {
        if (DataHolder.started == true)
        {    
            Slider speed_slider = GameObject.Find("Speed_slider").GetComponent<Slider>();
            float speed = speed_slider.value;

            if (paused != true)
            {
                now_time += Time.deltaTime * speed;
                GameObject.Find("TimeLine").GetComponent<Slider>().value = now_time;
            }
            
            double lat = MainScript.param[(int)Math.Floor(now_time)][0];
            double lon = MainScript.param[(int)Math.Floor(now_time)][1];
            double alt = (double)(MainScript.param[(int)Math.Floor(now_time)][2] * 0.3048);
            float heading = (float)MainScript.param[(int)Math.Floor(now_time)][3];
            float roll = (float)MainScript.param[(int)Math.Floor(now_time)][4];
            float pitch = (float)MainScript.param[(int)Math.Floor(now_time)][5];

            past_time = (int)Math.Floor(now_time) - 1;
            if (past_time < 0) past_time = 0;

            double past_lat = MainScript.param[(int)Math.Floor(past_time)][0];
            double past_lon = MainScript.param[(int)Math.Floor(past_time)][1];
            double past_alt = (double)(MainScript.param[(int)Math.Floor(past_time)][2] * 0.3048);
            float past_heading = (float)MainScript.param[(int)Math.Floor(past_time)][3];
            float past_roll = (float)MainScript.param[(int)Math.Floor(past_time)][4];
            float past_pitch = (float)MainScript.param[(int)Math.Floor(past_time)][5];

            if (lat != 0)
            {
                lat = lat - ZeroPoint[0];
                lon = lon - ZeroPoint[1];
            }

            rotate.eulerAngles = new Vector3(roll, heading, pitch);

            Vector3 past_position = new Vector3((float)past_lat, (float)past_alt, (float)past_lon);
            Vector3 new_position = new Vector3((float)(lat*10000), (float)alt, (float)(lon*10000));

            world_position += (new_position - past_position) * Time.deltaTime;

            transform.position = new_position;
            transform.localEulerAngles = new Vector3 (roll, heading + 180, pitch);

            human_time.AddSeconds(Math.Floor(now_time)); 

            GameObject.Find("Params").GetComponent<Text>().text = ( "Time = " + human_time.ToShortTimeString() + "(" + Math.Floor(now_time) +
                                                                    ")\nLatitude = " + lat + 
                                                                    "\nLongitude = " + lon +
                                                                    "\nAlt = " + alt +
                                                                    "\nHDG = " + heading);
            GameObject.Find("Speed_ind").GetComponent<Text>().text = ("x" + Math.Floor(speed));
        }
    }
    
    public void OnDrag()
    {
        paused = true;
        now_time = GameObject.Find("TimeLine").GetComponent<Slider>().value;
        GameObject.Find("Trail").GetComponent<TrailRenderer>().Clear();
    }
    public void OnEndDrag()
    {
        paused = false;
        GameObject.Find("Trail").GetComponent<TrailRenderer>().Clear();
    }

    private Vector3 CoordsConv(float lat, float lon, float alt)
    {
        double a = 6378137;
        double b = 6356752.3;
        double N = Math.Pow(a, 2)/Math.Pow(a*a * Math.Pow(Math.Cos(lat), 2) + b*b * Math.Pow(Math.Sin(lat), 2), 0.5);

        double x = (N + alt) * Math.Cos(lat) * Math.Cos(lon);
        double z = (N + alt) * Math.Cos(lat) * Math.Sin(lon);
        double y = ((Math.Pow(a, 2)/Math.Pow(b, 2)) * N + alt) * Math.Sin(lat);

        return new Vector3((float)x, (float)y, (float)z);
    }
}
