using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using System;
using System.Globalization;

public class MainScript : MonoBehaviour
{
    public DataTable table;
    public static double[][] param;
    public double lat;
    public double lon;
    public double AltGPS;
    public double HDG;
    public double roll;
    public double pitch;
    public double IAS;
    public double RPM;
    public int str_num = 0;
    public float MaxTime;
    public float ZeroTime;

    public GameObject HUD;

    void Start()
    {
        HUD = GameObject.Find("HUD");
        HUD.SetActive(false);
        if (DataHolder.Opened == true)
        {
            Constructor();
            HUD.SetActive(true);
            GameObject.Find("TimeLine").GetComponent<Slider>().maxValue = (MaxTime - ZeroTime);
        }
    }

    void Constructor()
    {
        table = DataHolder.data;
        ZeroTime = TimeConv(table.Rows[0]["Lcl Time"].ToString());
        MaxTime = TimeConv(table.Rows[table.Rows.Count - 1]["Lcl Time"].ToString());
        param = new double[(int)MaxTime][];

        NumberFormatInfo nfi = new CultureInfo( "en-US", false ).NumberFormat;
        nfi.NumberDecimalSeparator = ".";

        /*
        Param array structure
        index - programm time
        0 - Latitude
        1 - Longitude
        2 - AltGPS
        3 - HDG
        4 - roll
        5 - pitch
        6 - IAS
        7 - RPM
        */
        
        str_num = 0;

        for (int i = 0; i < MaxTime; i++)
        {
            if (str_num >= table.Rows.Count)
            {
                str_num = table.Rows.Count - 1;
                Debug.Log("Need to fix");
            }
            float seconds = TimeConv(table.Rows[str_num]["Lcl Time"].ToString());

            if (i > (seconds - ZeroTime))
            {
                str_num++;
            }
            
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

                str_num++;
            }
            
            param[i] = new double[8] {lat, lon, AltGPS, HDG, roll, pitch, IAS, RPM};
        }
        DataHolder.started = true;

    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
            DataHolder.started = false;
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