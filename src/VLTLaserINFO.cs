using System.Collections.Generic;

namespace VLTLaserControllerNET
{
    public class VLTLaserINFO
    {
        //IP_0=192 & IP_1=168 & IP_2=6 & IP_3=200
        //port=5011 & 
        //gate_ip_1=192 & gate_ip_2=168 & gate_ip_3=6 & gate_ip_4 = 1
        //mask_1 = 255 & mask_2 = 255 & mask_3 = 255 & mask_4 = 0
        //mac_1 = 0x00 & mac_2 = 0x08 & mac_3 = 0xDC & mac_4=0x22 & mac_5=0x4D & mac_6=0xC8
        //size_X=10708 & size_Y=10708 &
        //pos_X=-2785 & pos_Y=11779 &
        //rota=0 &
        //flip_X=0 & flip_Y=1 &
        //scanrate_chek=1 & scanrate=3855 & fps=30 & avto_colorshift=0 & colorshift=-3 & blanking_off=0 & 
        //max_green=65535 & max_blue=0 & max_red=0 &
        //fist_points=0 & last_points=0 & after_blank_points = 0 & 
        //before_blank_points = 0 &
        //angle_points = 0 &
        //time_hours = 14 & time_minutes = 12 &
        //auto_off = 0 & 
        //stop_work=11 &
        //laser_ID=1105 &
        //firmware_ver=1_1_v_5 &
        //stabl=1 &
        //laser_on=0 &
        //temp_galvo_fist=00 &
        //temp_galvo_last = 0 & 
        //temp_laser_fist = 00 &
        //temp_laser_last = 0 &
        //web_server = 1 &
        //lx = 0 & cr = 0 &
        //end=0

        public void Update(string message)
        {
            
        }

        private Dictionary<string, string> GetDictionary(string message)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string line in message.Split('&'))
            {
                string[] item = line.Split('=');
                dic.Add(item[0], item[1]);
            }
            return dic;
        }
    }
}
