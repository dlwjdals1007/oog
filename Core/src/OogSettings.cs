using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Oog.Viewer;
using System.Text;
using System.ComponentModel;

namespace Oog {
  public class OogSettings {
    ThumbnailSettings thumbnailSettings;
    FullScreenViewerSettings fullScreenViewerSettings;
    Dictionary<string, string> jumpPath;
    Rectangle windowStartup;
    FormWindowState windowState;

    private OogSettings(ThumbnailSettings thumbnailSettings, FullScreenViewerSettings fullScreenViewerSettings, Rectangle windowStartup) {
      this.thumbnailSettings = thumbnailSettings;
      this.fullScreenViewerSettings = fullScreenViewerSettings;

      jumpPath = new Dictionary<string, string>();
      this.windowStartup = windowStartup;
    }

    //const string SETTING_FILE_NAME = "OogSettings.xml";

    static string settingFilePath = null;

    public static string GetSettingFilePath() {
      if (settingFilePath == null) {
        string aplDatDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string fileName = @"oog\OogSettings.xml";
        string filePath = Path.Combine(aplDatDir, fileName);
        return filePath;
      }
      return settingFilePath;
    }

    public static OogSettings Load() {
      string exePath = Application.ExecutablePath;
      string exeDir = Path.GetDirectoryName(exePath);
      string settingFilePath = Path.Combine(exeDir, OogSettings.GetSettingFilePath());

      OogSettings settings = Default;

      if (File.Exists(settingFilePath)) {
        XmlDocument document = new XmlDocument();
        try {
          document.Load(settingFilePath);
          
          settings.ThumbnailSettings = ThumbnailSettings.Load(document);
          settings.FullScreenViewerSettings = FullScreenViewerSettings.Load(document);

          TypeConverter rectConv = TypeDescriptor.GetConverter(typeof(Rectangle));
          XmlElement rectElem = (XmlElement)document.SelectSingleNode("settings/windowStartup");
          if (rectElem != null) {
            try {
              settings.WindowStartup = (Rectangle)rectConv.ConvertFromString(rectElem.GetAttribute("value"));
            }
            catch { }
          }

          TypeConverter wstateConv = TypeDescriptor.GetConverter(typeof(FormWindowState));
          XmlElement wstateElem = (XmlElement)document.SelectSingleNode("settings/windowState");
          if (wstateElem != null) {
            try {
              settings.WindowState = (FormWindowState)wstateConv.ConvertFromString(wstateElem.GetAttribute("value"));
            }
            catch { }
          }

          foreach (XmlElement itemElem in document.SelectNodes("settings/jumpPath/item")) {
            string itemName = itemElem.GetAttribute("name");
            string itemPath = itemElem.InnerText;
            if (!string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(itemPath) && !settings.JumpPath.ContainsKey(itemName)) {
              settings.JumpPath.Add(itemName, itemPath);
            }
          }
        }
        catch (XmlException) { }
      }

      return settings;
    }

    public void Save() {
      string settingFilePath = OogSettings.GetSettingFilePath();
      string settingFileDir = Path.GetDirectoryName(settingFilePath);

      if (!Directory.Exists(settingFileDir)) {
        Directory.CreateDirectory(settingFileDir);
      }

      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      using (FileStream fs = File.Create(settingFilePath))
      using (StreamWriter sw = new StreamWriter(fs, new UTF8Encoding()))
      using (XmlWriter writer = XmlWriter.Create(sw, settings)) {
        writer.WriteStartElement("settings");
        
        thumbnailSettings.Save(writer);
        
        fullScreenViewerSettings.Save(writer);

        writer.WriteStartElement("windowStartup");
        TypeConverter rectConv = TypeDescriptor.GetConverter(typeof(Rectangle));
        writer.WriteAttributeString("value", rectConv.ConvertToString(windowStartup));
        writer.WriteEndElement();
        
        writer.WriteStartElement("windowState");
        TypeConverter wstateConv = TypeDescriptor.GetConverter(typeof(FormWindowState));
        writer.WriteAttributeString("value", wstateConv.ConvertToString(windowState));
        writer.WriteEndElement();
        
        writer.WriteStartElement("jumpPath");
        foreach (string itemName in jumpPath.Keys) {
          string itemPath = jumpPath[itemName];
          writer.WriteStartElement("item");
          writer.WriteAttributeString("name", itemName);
          writer.WriteString(itemPath);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
        
        writer.WriteEndElement();
      }
    }

    [ReadOnly(true)]
    [Category("Settings")]
    [Description("Settings concerning thumbnail.")]
      public ThumbnailSettings ThumbnailSettings {
        get { return thumbnailSettings; }
        set { thumbnailSettings = value; }
      }

    [ReadOnly(true)]
    [Category("Settings")]
    [Description("Settings concerning viewer.")]
    public FullScreenViewerSettings FullScreenViewerSettings {
      get { return fullScreenViewerSettings; }
      set { fullScreenViewerSettings = value; }
    }

    [Browsable(false)]
    public Rectangle WindowStartup {
      get { return windowStartup; }
      set { windowStartup = value; }
    }

    [Browsable(false)]
    public FormWindowState WindowState {
      get { return windowState; }
      set { windowState = value; }
    }

    [Browsable(false)]
    public Dictionary<string, string> JumpPath {
      get { return jumpPath; }
    }

    [Browsable(false)]
    public static OogSettings Default {
      get { return new OogSettings(ThumbnailSettings.Default, FullScreenViewerSettings.Default, DefaultWindowStartup); }
    }

    [Browsable(false)]
    public static Rectangle DefaultWindowStartup {
      get { return new Rectangle(100, 100, 500, 500); }
    }

    [Browsable(false)]
    public static FormWindowState DefaultWindowState {
      get { return FormWindowState.Normal; }
    }
  }
}