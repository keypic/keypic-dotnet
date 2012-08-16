using System;
using System.Configuration;

namespace KeypicLib
{
    public class KeypicConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("host",IsRequired=true)]
        public String Host
        {
            get { return this["host"].ToString(); }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("debug", DefaultValue = false)]
        public Boolean Debug
        {
            get { return (Boolean)this["debug"]; }
            set { this["debug"] = value; }
        }

        [ConfigurationProperty("formID", DefaultValue="")]
        public String FormID
        {
            get { return this["formID"].ToString(); }
            set { this["formID"] = value; }
        }
    }
}
