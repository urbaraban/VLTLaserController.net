using System;
using System.Collections.Generic;
using System.Net;

namespace VLTLaserControllerNET
{
    public class VLTLaserINFO
    {
        public IPAddress IPAddress { get; }
        public int Port { get; }
        public IPAddress GateIPAddress { get; }
        public IPAddress Mask { get; }
        string MacAddress { get; } = string.Empty;
        public short Pos_X { get; } = 0;
        public short Pos_Y { get; } = 0;
        //rota=0 &
        //flip_X=0 & flip_Y=1 &
        //scanrate_chek=1 & scanrate=3855 & fps=30 & avto_colorshift=0 & colorshift=-3 & blanking_off=0 & 
        //max_green=65535 & max_blue=0 & max_red=0 &
        //fist_points=0 & last_points=0 & after_blank_points = 0 & 
        //before_blank_points = 0 &
        //angle_points = 0 &
        //time_hours = 14 &
        //time_minutes = 12 &
        //auto_off = 0 & 
        public bool AutoOff = false;
        //stop_work=11 &
        //laser_ID=1105 &
        //firmware_ver=1_1_v_5 &
        public string Firmware_ver = string.Empty;
        //stabl=1 &
        //laser_on=0 &
        public bool LaserOn = false;
        //temp_galvo_fist=00 &
        //temp_galvo_last = 0 & 
        //temp_laser_fist = 00 &
        //temp_laser_last = 0 &
        public bool WebServer { get; set; } = false;
        //web_server = 1 &
        //lx = 0 & cr = 0 &
        //end=0

        public VLTLaserINFO(string message)
        {
            Dictionary<string, string> keys = GetDictionary(message);

            if (keys != null && keys.Count > 0 )
            {
                IPAddress = new IPAddress(new byte[4]
                {
                    byte.Parse(keys["IP_0"]),
                    byte.Parse(keys["IP_1"]),
                    byte.Parse(keys["IP_2"]),
                    byte.Parse(keys["IP_3"])
                });

                Port = int.Parse(keys["port"]);

                GateIPAddress = new IPAddress(new byte[4]
                {
                byte.Parse(keys["gate_ip_1"]),
                byte.Parse(keys["gate_ip_2"]),
                byte.Parse(keys["gate_ip_3"]),
                byte.Parse(keys["gate_ip_4"])
                });

                GateIPAddress = new IPAddress(new byte[4]
                {
                byte.Parse(keys["mask_1"]),
                byte.Parse(keys["mask_2"]),
                byte.Parse(keys["mask_3"]),
                byte.Parse(keys["mask_4"])
                });
                MacAddress = $"{keys["mac_1"]}:{keys["mac_2"]}:{keys["mac_3"]}:{keys["mac_4"]}:{keys["mac_5"]}:{keys["mac_6"]}";
                AutoOff = Convert.ToBoolean(byte.Parse(keys["auto_off"]));

                Pos_X = short.Parse(keys["pos_X"]);
                Pos_Y = short.Parse(keys["pos_Y"]);

                Firmware_ver = keys["firmware_ver"];

                LaserOn = Convert.ToBoolean(byte.Parse(keys["laser_on"]));
                WebServer = Convert.ToBoolean(byte.Parse(keys["web_server"]) > 0);
            }
        }

        private Dictionary<string, string> GetDictionary(string message)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string line in message.Split('&'))
            {
                string[] item = line.Split('=');
                dic.Add(item[0], item[1]);
            }

            if (dic.Count > 0)
            {
                return dic;
            }
            return null;
        }
    }
}
