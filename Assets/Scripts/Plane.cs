﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class Plane : MonoBehaviour
{
    public float now_time;
    public DateTime human_time = new DateTime(0);
    public bool paused = false;
    public static double[] ZeroPoint;
    Quaternion rotate;
    public GameObject terrain;
    public Texture2D map;

    void Start()
    {
        if (DataHolder.Opened)
        {   
            foreach (double[] row in MainScript.param)
            {
                if ((row[0] != 0) && (row[1] != 0) && (row[2] != 0))
                {
                    ZeroPoint = new double[3] {row[0], row[2], row[1]}; //ZeroPoint (lat, alt, lon)
                    terrain = GameObject.Find("Plane");
                    terrain.transform.position = new Vector3(0, (float)(ZeroPoint[1] * 0.3048 - 1.1), 0);
                    transform.position = new Vector3(0, (float)(ZeroPoint[1] * 0.3048), 0);
                    runway_spawn();
                    return;
                }
            }
        }
    }

    void Update()
    {
        if (DataHolder.started == true && now_time < MainScript.param.Length - 1)
        {
            Slider speed_slider = GameObject.Find("Speed_slider").GetComponent<Slider>();
            float speed = speed_slider.value;

            if (paused != true)
            {
                now_time += Time.deltaTime * speed;
                GameObject.Find("TimeLine").GetComponent<Slider>().value = now_time;
            }

            //Move control
            double lat = MainScript.param[(int)Math.Floor(now_time)][0];
            double lon = MainScript.param[(int)Math.Floor(now_time)][1];
            double alt = (double)(MainScript.param[(int)Math.Floor(now_time)][alt_ind()]);
            float heading = (float)MainScript.param[(int)Math.Floor(now_time)][4] + 12;
            float roll = (float)MainScript.param[(int)Math.Floor(now_time)][5];
            float pitch = (float)MainScript.param[(int)Math.Floor(now_time)][6];

            double next_lat = MainScript.param[(int)Math.Floor(now_time + 1)][0];
            double next_lon = MainScript.param[(int)Math.Floor(now_time + 1)][1];
            double next_alt = (double)(MainScript.param[(int)Math.Floor(now_time + 1)][alt_ind()]);
            float next_heading = (float)MainScript.param[(int)Math.Floor(now_time + 1)][4] + 12;
            float next_roll = (float)MainScript.param[(int)Math.Floor(now_time + 1)][5];
            float next_pitch = (float)MainScript.param[(int)Math.Floor(now_time + 1)][6];

            //terrain loading
            // if (Math.Abs(transform.position.x - terrain.transform.position.x) > 50 || Math.Abs(transform.position.z - terrain.transform.position.z) > 50)
            // {
            //     StartCoroutine(map_loader(lat.ToString(), lon.ToString(), 0));
            //     terrain.transform.position = new Vector3(transform.position.x, terrain.transform.position.y, transform.position.z);
            //     loading = false;
            //     map[0] = map[0];
            // }
            // terrain.GetComponent<Renderer>().material.mainTexture = map[0];

            if (next_heading - heading > 180) next_heading = next_heading - 360;
            if (heading - next_heading > 180) heading = heading - 360;

            if (lat != 0)
            {
                lat = lat - ZeroPoint[0];
                lon = lon - ZeroPoint[2];
                next_lat = next_lat - ZeroPoint[0];
                next_lon = next_lon - ZeroPoint[2];
            }

            //tmp del
            //if (alt < ZeroPoint[1]) alt = ZeroPoint[1];
            //if (next_alt < ZeroPoint[1]) next_alt = ZeroPoint[1];

            Vector3 now_position = earth_calc(lat, alt, lon);
            Vector3 next_position = earth_calc(next_lat, next_alt, next_lon);

            Vector3 now_rotation = new Vector3(roll, heading, pitch);
            Vector3 next_rotation = new Vector3(next_roll, next_heading, next_pitch);

            transform.position = Vector3.Lerp(now_position, next_position, now_time - (int)Math.Floor(now_time));
            transform.eulerAngles = Vector3.Lerp(now_rotation, next_rotation, now_time - (int)Math.Floor(now_time));


            //Other
            float IAS = (float)MainScript.param[(int)Math.Floor(now_time)][7];
            float RPM = (float)MainScript.param[(int)Math.Floor(now_time)][8];

            GameObject.Find("helice").transform.Rotate(-RPM / 60, 0, 0);

            //Info
            string alt_sourse;
            if (alt_ind() == 2) alt_sourse = "GPS";
            else alt_sourse = "Baro";
            GameObject.Find("Params").GetComponent<Text>().text = ("Time = " + human_time.AddSeconds(now_time).ToLongTimeString() +
            "/" + human_time.AddSeconds(MainScript.param.Length).ToLongTimeString() + " (" + Math.Floor(now_time) +
                                                                    ")\nIAS = " + IAS +
                                                                    "\nAlt = " + alt * 0.3048 +
                                                                    "\nHDG = " + heading +
                                                                    "\nRPM = " + RPM +
                                                                    "\nAlt sourse: " + alt_sourse);
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

    public static Vector3 earth_calc(double lat, double alt, double lon)
    {
        double R = 6371000;
        double lat_m = 2 * Math.PI * R / 360 * lat;
        double lon_m = 2 * Math.PI * R / 360 * Math.Cos(ZeroPoint[0] + lat) * lon;
        double alt_m;
        // if (alt_ind() == 2) alt_m = alt * 0.3048;
        // else alt_m = (alt + ZeroPoint[1] - 1.1) * 0.3048;
        alt_m = alt * 0.3048;

        return new Vector3((float)lat_m, (float)alt_m, (float)lon_m);
    }

    private int alt_ind()
    {
        if (GameObject.Find("AltToggle").GetComponent<Toggle>().isOn == true) return 3;
        else return 2;
    }
    
    IEnumerator map_loader(string lat, string lon)
    {
        string url = "https://maps.google.com/maps/api/staticmap?center=" + lat + "," + lon + "&zoom=19&scale=2&size=640x640&format=jpg&maptype=satellite&key=AIzaSyDl-GxzE5IkJYXkYuenjcXCDYDD7HCjeIA";
        using (WWW www = new WWW(url))
        {
            yield return www;
            map = www.texture;
            terrain.transform.position = new Vector3(transform.position.x, terrain.transform.position.y, transform.position.z);
            terrain.GetComponent<Renderer>().material.mainTexture = map;
        }
    }

    void runway_spawn()
    {
        bool spawn = true;
        float rw_alt, rw_heading, rw_long;
        Vector2 rw_1, rw_2, rw_center;
        Vector2[] rw_coords = new Vector2[MainScript.filtered_sectors.Length];
        for (int i = 0; i < MainScript.filtered_sectors.Length; i++)
        {
            spawn = true;
            if (MainScript.filtered_sectors[i].Length != 2) return;
            rw_alt = Math.Min((float)MainScript.param[MainScript.filtered_sectors[i][0]][2], (float)MainScript.param[MainScript.filtered_sectors[i][1]][2]);
            Vector3 vector = earth_calc((float)(MainScript.param[MainScript.filtered_sectors[i][0]][0] - ZeroPoint[0]), 0, (float)(MainScript.param[MainScript.filtered_sectors[i][0]][1]- ZeroPoint[2]));
            rw_1 = new Vector2(vector.x, vector.z);
            vector = earth_calc((float)(MainScript.param[MainScript.filtered_sectors[i][1]][0]- ZeroPoint[0]), 0, (float)(MainScript.param[MainScript.filtered_sectors[i][1]][1]- ZeroPoint[2]));
            rw_2 = new Vector2(vector.x, vector.z);
            rw_heading = -(float)(Math.Atan((rw_2.y - rw_1.y) / (rw_2.x - rw_1.x)) * 180 / Math.PI);
            rw_long = Vector2.Distance(rw_1, rw_2);
            rw_center = Vector2.Lerp(rw_1, rw_2, 0.5f);
            float rw_delta_alt = Math.Abs((float)MainScript.param[MainScript.filtered_sectors[i][0]][2] - (float)MainScript.param[MainScript.filtered_sectors[i][1]][2]);
            if (i != 0)
            {
                foreach (Vector2 coords in rw_coords)
                {
                    if (Vector2.Distance(coords, rw_center) < 800) spawn = false;
                }
            }
            if (spawn == true)
            {    
                rw_coords[i] = rw_center;
                GameObject runway = new GameObject("runway" + i.ToString(), typeof(MeshRenderer), typeof(MeshFilter));
                runway.transform.position = new Vector3(rw_center.x, (float)(rw_alt * 0.3048), rw_center.y);
                runway.transform.eulerAngles = new Vector3(0, rw_heading, 0); //(float)(Math.Atan(rw_delta_alt / rw_long) * 180 / Math.PI));
                runway.transform.localScale = new Vector3(rw_long / 2, 1, 7);
                runway.GetComponent<MeshFilter>().mesh = terrain.GetComponent<MeshFilter>().mesh;
                runway.GetComponent<MeshRenderer>().material = Resources.Load("Materials/asphalt", typeof(Material)) as Material;
            }
        }
    }
}
