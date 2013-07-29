using System;
using System.Windows;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for EditSession.xaml
  /// </summary>
  public partial class EditSession
  {
    public PhotoSession Session { get; set; }

    public EditSession(PhotoSession session)
    {
      Session = session;
      Session.BeginEdit();
      InitializeComponent();
      DataContext = Session;
      ServiceProvider.Settings.ApplyTheme(this);
    }

    private void btn_browse_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new System.Windows.Forms.FolderBrowserDialog();
      dialog.SelectedPath = Session.Folder;
      if(dialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
      {
        Session.Folder = dialog.SelectedPath;
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        iud_LengthMax.ValueChanged += on_iud_LengthMax_Change;
        iud_LengthMin.ValueChanged += on_iud_LengthMin_Change;
        txt_SettingsPassword.Password = ServiceProvider.Settings.SettingsPassword;
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      Session.EndEdit();
      DialogResult = true;
      Close();
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      Session.CancelEdit();
      Close();
    }

    private void btn_add_tag_Click(object sender, RoutedEventArgs e)
    {
      TagItem item = new TagItem();
      EditTagWnd wnd = new EditTagWnd(item);
      if (wnd.ShowDialog() == true)
      {
        Session.Tags.Add(item);
      }
    }

    private void btn_del_tag_Click(object sender, RoutedEventArgs e)
    {
      TagItem item = lst_tags.SelectedItem as TagItem;
      if (item != null)
      {
        Session.Tags.Remove(item);
      }
    }

    private void btn_edit_tag_Click(object sender, RoutedEventArgs e)
    {
      TagItem item = lst_tags.SelectedItem as TagItem;
      if (item != null)
      {
        EditTagWnd wnd = new EditTagWnd(item);
        wnd.ShowDialog();
      }
    }

    private void btn_help_Click(object sender, RoutedEventArgs e)
    {
      HelpProvider.Run(HelpSections.Session);
    }

    public void LoadDefaults(bool Force = false)
    {
        if ((!Force) && ServiceProvider.Settings.DefaultsWereLoaded)
            return;

        Session.Name = "Default";
        Session.FileNameTemplate = "$B$-$Qt$UTime";
        Session.UseOriginalFilename = false;
        Session.BarcodeRegex = @"^[a-zA-Z0-9\(\)_\-\.]*$";
        Session.BarcodeDelimiter = "-";
        Session.BarcodeLengthMax = 32;
        Session.BarcodeLengthMin = 8;
        Session.QuickTagOption = QuickTagOptions.Set_From_Main;

        string SaveFolder = "C:\\Proshots\\Dropfolder";
        try
        {
            XmlTextReader reader = new XmlTextReader("C:\\Proshots\\config.xml");
            while (reader.Read())
                if (reader.Name == "dropfolder" && reader.NodeType == XmlNodeType.Element)
                {
                    reader.Read();
                    SaveFolder = reader.Value;
                    break;
                }
        }
        catch (Exception ex)
        {
            CameraControl.Devices.Log.Debug("Error loading Proshots config: " + ex.Message);
        }
        Session.Folder = SaveFolder;

        ServiceProvider.Settings.Save(Session);

        //if (String.IsNullOrEmpty(ServiceProvider.Settings.SettingsPassword))
        //{
        //    StringBuilder builder = new StringBuilder();
        //    var random = new Random();
        //    for (int i = 0; i < 8; i++)
        //        builder.Append(Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))));
        //    ServiceProvider.Settings.SettingsPassword = builder.ToString();
        //}
        ServiceProvider.Settings.DefaultsWereLoaded = true;
        ServiceProvider.Settings.Save();

        ServiceProvider.Settings.DefaultsWereLoaded = true;
    }

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd, object param)
    {
        switch (cmd)
        {
            case WindowsCmdConsts.EditSessionWnd_Show:
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    Show();
                    Activate();
                    Topmost = true;
                    Topmost = false;
                    Focus();
                }));
                break;
            case WindowsCmdConsts.EditSessionWnd_Hide:
                Dispatcher.Invoke(new Action(delegate
                {   
                    Hide();
                }));
                break;
            case CmdConsts.All_Close:
                Dispatcher.Invoke(new Action(delegate
                {
                    Hide();
                    Close();
                }));
                break;
            default: break;
        }
    }

    #endregion

    private void on_btn_Defaults(object sender, RoutedEventArgs e)
    {
        LoadDefaults(true);
    }

    private void on_iud_LengthMin_Change(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if ((iud_LengthMax.Value ?? int.MaxValue) < (iud_LengthMin.Value ?? 0))
            iud_LengthMax.Value = (iud_LengthMin.Value ?? 0);
    }

    private void on_iud_LengthMax_Change(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if ((iud_LengthMin.Value ?? 0) > (iud_LengthMax.Value ?? int.MaxValue))
            iud_LengthMin.Value = (iud_LengthMax.Value ?? int.MaxValue);
    }

    private void on_txt_QuickTag_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var QTRegex = @"[^a-zA-Z]{1}";
        Session.QuickTag = txt_QuickTag.Text;    // Expedite update
        var pos = txt_QuickTag.CaretIndex;
        var matchesBefore = Regex.Matches(Session.QuickTag.Substring(0, pos), QTRegex).Count;
        Session.QuickTag = Regex.Replace(Session.QuickTag, QTRegex, "");
        txt_QuickTag.Text = Session.QuickTag;
        txt_QuickTag.CaretIndex = pos - matchesBefore;
    }

    private void on_txt_SettingsPassword_Changed(object sender, RoutedEventArgs e)
    {
        ServiceProvider.Settings.SettingsPassword = txt_SettingsPassword.Password;
    }

  }
}
