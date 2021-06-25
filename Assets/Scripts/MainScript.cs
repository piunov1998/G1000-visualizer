using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using System;
using System.Globalization;
using System.Collections.Generic;

public class MainScript : MonoBehaviour
{
    public DataTable table;
    public static double[][] param;
    public double lat, lon;
    public double AltGPS, AltBaro;
    public double HDG, roll, pitch;
    public double IAS, RPM, PWR, MagVar;
    public int str_num = 0;
    public float MaxTime;
    public float ZeroTime;

    public GameObject HUD;

    public float timer;
    public static int[][] filtered_sectors;

    void Start()
    {
        HUD = GameObject.Find("HUD");
        HUD.SetActive(false);
        if (DataHolder.Opened == true)
        {
            Constructor();
            HUD.SetActive(true);
            GameObject.Find("TimeLine").GetComponent<Slider>().maxValue = (MaxTime - ZeroTime);
            analysis();
            marker();
        }
    }

    void Constructor()
    {
        table = DataHolder.data;
        ZeroTime = TimeConv(table.Rows[0]["Lcl Time"].ToString());
        MaxTime = TimeConv(table.Rows[table.Rows.Count - 1]["Lcl Time"].ToString());
        param = new double[(int)MaxTime - (int)ZeroTime][];

        NumberFormatInfo nfi = new CultureInfo( "en-US", false ).NumberFormat;
        nfi.NumberDecimalSeparator = ".";

        /*
        Param array structure
        index - programm time
        0  - Latitude
        1  - Longitude
        2  - AltGPS
        3  - AltBaro
        4  - HDG
        5  - roll
        6  - pitch
        7  - IAS
        8  - RPM
        9  - PWR
        10 - MagVar
        */
        
        str_num = 0;

        for (int i = 0; i < param.Length; i++)
        {
            if (str_num >= table.Rows.Count)
            {
                str_num = table.Rows.Count - 1;
                //Debug.Log("Need to fix");
            }
            float seconds = TimeConv(table.Rows[str_num]["Lcl Time"].ToString());

            if (i > (seconds - ZeroTime)) str_num++;
            
            if (i == (seconds - ZeroTime))
            {            
                //Latitude
                if (table.Rows[str_num]["Latitude"].ToString() == String.Empty) lat = 0;
                else lat = double.Parse(table.Rows[str_num]["Latitude"].ToString(), nfi);


                //Longitude
                if (table.Rows[str_num]["Longitude"].ToString() == String.Empty) lon = 0;
                else lon = double.Parse(table.Rows[str_num]["Longitude"].ToString(), nfi);

                //AltGPS
                if (table.Rows[str_num]["AltGPS"].ToString() == String.Empty) AltGPS = 0;
                else AltGPS = double.Parse(table.Rows[str_num]["AltGPS"].ToString(), nfi);

                //AltBaro
                if (table.Rows[str_num]["AltB"].ToString() == String.Empty) AltBaro = 0;
                else AltBaro = double.Parse(table.Rows[str_num]["AltB"].ToString(), nfi);

                //HDG
                if (table.Rows[str_num]["HDG"].ToString() == String.Empty) HDG = 0;
                else HDG = double.Parse(table.Rows[str_num]["HDG"].ToString(), nfi);

                //Roll
                if (table.Rows[str_num]["Roll"].ToString() == String.Empty) roll = 0;
                else roll = -double.Parse(table.Rows[str_num]["Roll"].ToString(), nfi);

                //Pitch
                if (table.Rows[str_num]["Pitch"].ToString() == String.Empty) pitch = 0;
                else pitch = double.Parse(table.Rows[str_num]["Pitch"].ToString(), nfi);

                //IAS
                if (table.Rows[str_num]["IAS"].ToString() == String.Empty) IAS = 0;
                else IAS = double.Parse(table.Rows[str_num]["IAS"].ToString(), nfi);

                //RPM
                if (table.Rows[str_num]["E1 RPM"].ToString() == String.Empty) RPM = 0;
                else RPM = double.Parse(table.Rows[str_num]["E1 RPM"].ToString(), nfi);

                //PWR
                if (table.Rows[str_num]["E1 %Pwr"].ToString() == String.Empty) PWR = 0;
                else PWR = double.Parse(table.Rows[str_num]["E1 %Pwr"].ToString(), nfi);

                //MagVar
                if (table.Rows[str_num]["MagVar"].ToString() == String.Empty) MagVar = 0;
                else MagVar = double.Parse(table.Rows[str_num]["MagVar"].ToString(), nfi);

                str_num++;
            }
            
            param[i] = new double[10] {lat, lon, AltGPS, AltBaro, HDG, roll, pitch, IAS, RPM, PWR};
        }
        DataHolder.started = true;

    }

    void analysis()
    {
        List<int> data = new List<int>();
        for (int i = 0; i < param.Length; i++)
        {
            if (param[i][7] > 20 && param[i][7] < 60) data.Add(i);
        }
        filtered_sectors = new int[64][];
        int start_point = data[0];
        int middle_point = data[0];
        int k = 0;
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i] - middle_point > 2)
            {
                filtered_sectors[k] = new int[2] {start_point, data[i - 1]};
                k++;
                start_point = middle_point = data[i];
            }
            middle_point = data[i];
        }
        filtered_sectors[k] = new int[3] {0, 0, 0};
    }

    void marker()
    {
        GameObject time_slider = GameObject.Find("TimeLine");
        float slider_scale = time_slider.GetComponent<RectTransform>().sizeDelta.x / time_slider.GetComponent<Slider>().maxValue;

        for (int i = 0; i < filtered_sectors.Length; i++)
        {
            if (filtered_sectors[i].Length == 2)
            {
                GameObject sector = new GameObject("sector" + i.ToString(), typeof(Image));
                sector.GetComponent<RectTransform>().sizeDelta = new Vector2((filtered_sectors[i][1] - filtered_sectors[i][0]) * slider_scale, 15);
                sector.GetComponent<Image>().color = new Color(1, 0, 0, 0.8f);
                sector.transform.SetParent(time_slider.transform);
                sector.transform.localPosition = new Vector3((filtered_sectors[i][0] * slider_scale - time_slider.GetComponent<RectTransform>().sizeDelta.x / 2 + sector.GetComponent<RectTransform>().sizeDelta.x / 2), 0, 0);
            }
            else return;
        }
    }


    void Update()
    {
        if (DataHolder.started == true && timer > 5) GameObject.Find("Hint").GetComponent<Text>().enabled = false;
        else timer += Time.deltaTime;
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
            DataHolder.started = false;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            
            if (GameObject.Find("Pilot").GetComponent<Camera>().enabled == true) 
            {
                GameObject.Find("Pilot").GetComponent<Camera>().enabled = false;
                GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
            }
            else 
            {
                GameObject.Find("Pilot").GetComponent<Camera>().enabled = true;
                GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;
            }
        }
    }

    float TimeConv(string locTime)
    {
        float hours = float.Parse(locTime.Split(':')[0]);
        float minutes = float.Parse(locTime.Split(':')[1]);
        float secs = float.Parse(locTime.Split(':')[2]);
        float time = hours * 3600 + minutes * 60 + secs;
        return(time);
    }
}