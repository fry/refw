using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace refw.BT.Utility {
    public class BasicConfig : ICloneable {
        [XmlIgnore]
        public string Filename { get; set; }

        public BasicConfig() {
        }

        public BasicConfig(string filename) {
            this.Filename = filename;
        }

        object ICloneable.Clone() {
            return this.Clone();
        }

        public BasicConfig Clone() {
            return (BasicConfig)this.MemberwiseClone();
        }

        public bool Save() {
            FileStream w = null;
            try {
                // Save file to different name then move it when successful
                string new_file = Filename + ".new";
                string backup = Filename + ".bak";

                XmlSerializer s = new XmlSerializer(GetType());
                w = new FileStream(new_file, FileMode.Create, FileAccess.Write);
                s.Serialize(w, this);
                w.Close();

                // Backup old file
                File.Delete(backup);
                if (File.Exists(Filename))
                    File.Move(Filename, backup);
                File.Move(new_file, Filename);
                return true;
            } catch (Exception e) {
                Trace.WriteLine("Failed to save config " + Filename);
                Trace.WriteLine(e.ToString());
                if (w != null)
                    w.Close();
                return false;
            }
        }

        public static bool Load<T>(string filename, out T config) where T : BasicConfig, new() {
            FileStream r = null;
            try {
                XmlSerializer s = new XmlSerializer(typeof(T));
                r = new FileStream(filename, FileMode.Open, FileAccess.Read);
                config = (T)s.Deserialize(r);
                r.Close();
                config.Filename = filename;
                config.OnLoad();
                return true;
            } catch (Exception e) {
                if (r != null)
                    r.Close();
                Trace.WriteLine("Failed to load config " + filename);
                //Trace.WriteLine(e.ToString());
                config = new T {
                    Filename = filename
                };
                return false;
            }
        }

        public virtual string GetSubConfigPath(string filename) {
            return Path.Combine(Path.GetDirectoryName(Filename), filename);
        }

        /// <summary>
        /// Dynamically load a config component. The function will look for a string property called "<name>ConfigFilename" and load the config with that name
        /// into the property of type T with "<name>Config". These properties will have to be manually created.
        /// </summary>
        /// <typeparam name="T">The type of the config to load</typeparam>
        /// <param name="name">The name of the config</param>
        /// <returns>Whether the loading process was successful</returns>
        public bool LoadComponent<T>(string name) where T : BasicConfig, new() {
            var type = this.GetType();

            // Read the config file name property
            var file_name_info = type.GetField(name + "ConfigFilename");
            if (file_name_info == null)
                return false;

            var file_name = (string)file_name_info.GetValue(this);

            var config_info = type.GetField(name + "Config");
            if (config_info == null)
                return false;

            // Load the config and store it into the config property
            T config;
            if (file_name == null || !BasicConfig.Load<T>(GetSubConfigPath(file_name), out config))
                return false;

            config_info.SetValue(this, config);

            return true;
        }

        protected virtual void OnLoad() {
        }
    }
}
