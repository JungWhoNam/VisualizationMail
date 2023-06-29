using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VMail.Utils.MessageFile;

namespace VMail.Utils.Web
{
    public class VMailData
    {
        public int ID { get; private set; }
        public string name { get; private set; }
        public DateTime lastModifiedDesktop;
        public DateTime lastModifiedMobile;
        public DateTime lastModifiedServer;

        public VMailData(int ID, string name, string lastModifiedDesktop, string lastModifiedMobile, string lastModifiedServer)
        {
            this.ID = ID;
            this.name = name;
            this.lastModifiedDesktop = DateTime.Parse(lastModifiedDesktop);
            this.lastModifiedMobile = DateTime.Parse(lastModifiedMobile);
            this.lastModifiedServer = DateTime.Parse(lastModifiedServer);

            // string dateTimeStr = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public VMailData(int ID, string name)
        {
            this.ID = ID;
            this.name = name;
            this.lastModifiedDesktop = DateTime.UtcNow;
            this.lastModifiedMobile = DateTime.UtcNow;
            this.lastModifiedServer = DateTime.UtcNow;
        }

        public override string ToString()
        {
            string str = ID + " " + name + " ";
            str += lastModifiedDesktop.ToString("yyyy-MM-dd HH:mm:ss") + " " + lastModifiedMobile.ToString("yyyy-MM-dd HH:mm:ss") + " " + lastModifiedServer.ToString("yyyy-MM-dd HH:mm:ss") + " ";

            return str;
        }

        public DateTime GetLatestModified()
        {
            DateTime latest = this.lastModifiedDesktop;

            if (latest < this.lastModifiedMobile)
            {
                latest = this.lastModifiedMobile;
            }

            if (latest < this.lastModifiedServer)
            {
                latest = this.lastModifiedServer;
            }

            return latest;
        }

        public string GetDirectoryURL()
        {
            return VMailWebManager.ServerDir + this.ID + "/";
        }

        public string GetVideoURL()
        {
            return VMailWebManager.ServerDir + this.ID + "/" + VideoFileGenerator.VideoFileName;
        }

    }
}