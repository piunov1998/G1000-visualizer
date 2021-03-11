//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Plane : MonoBehaviour
{
    public float now_time;
    public DateTime human_time = new DateTime(0);
    public bool paused = false;
    public double[] ZeroPoint;
    Quaternion rotate;

    void Start()
    {
        if (DataHolder.Opened)
        {   
            foreach (double[] row in MainScript.param)
            {
                if ((row[0] != 0) && (row[1] != 0) && (row[2] != 0))
                {
                    ZeroPoint = new double[3] {row[0], row[2], row[1]};
                    GameObject.Find("Plane").transform.position = new Vector3(0, (float)(ZeroPoint[1] * 0.3048 - 1.1), 0);
                    transform.position = new Vector3(0, (float)(ZeroPoint[1] * 0.3048 - 1.1), 0);
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
            double alt = (double)(MainScript.param[(int)Math.Floor(now_time)][2]);
            float heading = (float)MainScript.param[(int)Math.Floor(now_time)][3] + 12;
            float roll = (float)MainScript.param[(int)Math.Floor(now_time)][4];
            float pitch = (float)MainScript.param[(int)Math.Floor(now_time)][5];

            double next_lat = MainScript.param[(int)Math.Floor(now_time + 1)][0];
            double next_lon = MainScript.param[(int)Math.Floor(now_time + 1)][1];
            double next_alt = (double)(MainScript.param[(int)Math.Floor(now_time + 1)][2]);
            float next_heading = (float)MainScript.param[(int)Math.Floor(now_time + 1)][3] + 12;
            float next_roll = (float)MainScript.param[(int)Math.Floor(now_time + 1)][4];
            float next_pitch = (float)MainScript.param[(int)Math.Floor(now_time + 1)][5];

            if (lat != 0)
            {
                lat = lat - ZeroPoint[0];
                lon = lon - ZeroPoint[2];
                next_lat = next_lat - ZeroPoint[0];
                next_lon = next_lon - ZeroPoint[2];
            }

            if (alt < ZeroPoint[1] - 1.1) alt = ZeroPoint[1] - 1.1;
            if (next_alt < ZeroPoint[1] - 1.1) next_alt = ZeroPoint[1] - 1.1;

            rotate.eulerAngles = new Vector3(roll, heading, pitch);

            Vector3 now_position = earth_calc(lat, alt, lon);
            Vector3 next_position = earth_calc(next_lat, next_alt, next_lon);

            Vector3 now_rotation = new Vector3(roll, heading, pitch);
            Vector3 next_rotation = new Vector3(next_roll, next_heading, next_pitch);
            
            transform.position = Vector3.Lerp(now_position, next_position, now_time - (int)Math.Floor(now_time));
            transform.localEulerAngles = Vector3.Lerp(now_rotation, next_rotation, now_time - (int)Math.Floor(now_time));

            human_time.AddSeconds(Math.Floor(now_time)); 

            GameObject.Find("Params").GetComponent<Text>().text = ( "Time = " + human_time.ToShortTimeString() + "(" + Math.Floor(now_time) +
                                                                    ")\nLatitude = " + lat + 
                                                                    "\nLongitude = " + lon +
                                                                    "\nAlt = " + alt +
                                                                    "\nHDG = " + heading +
                                                                    "\nVector = " + earth_calc(lat, alt, lon).ToString());
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

    private Vector3 CoordsConv(double lat, double alt, double lon)
    {
        double a = 6378137;
        double b = 6356752.3;
        double N = Math.Pow(a, 2)/Math.Pow(a*a * Math.Pow(Math.Cos(lat), 2) + b*b * Math.Pow(Math.Sin(lat), 2), 0.5);

        double x = (N + alt) * Math.Cos(lat) * Math.Cos(lon);
        double z = (N + alt) * Math.Cos(lat) * Math.Sin(lon);
        double y = ((Math.Pow(a, 2)/Math.Pow(b, 2)) * N + alt) * Math.Sin(lat);

        return new Vector3((float)x, (float)y, (float)z);
    }

    private Vector3 earth_calc(double lat, double alt, double lon)
    {
        double R = 6371000;
        double lat_m = 2 * Math.PI * R / 360 * lat;
        double lon_m = 2 * Math.PI * R / 360 * Math.Cos(ZeroPoint[0]) * lon;
        double alt_m = alt * 0.3048;

        return new Vector3((float)lat_m, (float)alt_m, (float)lon_m);

    }
}
