using Memory;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Media;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace NotYetHAX
{
    public partial class NotYetHAX : Form
    {
        private List<Control> GetAllControls(Control parent)
        {
            List<Control> controls = new List<Control>();
            controls.AddRange(parent.Controls.Cast<Control>());
            controls.AddRange(parent.Controls.Cast<Control>().SelectMany(GetAllControls));
            return controls;
        }

        private readonly Mem mem = new Mem();

        public NotYetHAX()
        {
            InitializeComponent();
        }

        public class Adapter
        {
            public ManagementObject adapter;
            public string adaptername;
            public string customname;
            public int devnum;

            public Adapter(ManagementObject a, string aname, string cname, int n)
            {
                adapter = a;
                adaptername = aname;
                customname = cname;
                devnum = n;
            }

            public Adapter(NetworkInterface i) : this(i.Description) { }

            public Adapter(string aname)
            {
                adaptername = aname;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from win32_networkadapter where Name='" + adaptername + "'");
                ManagementObjectCollection found = searcher.Get();
                adapter = found.Cast<ManagementObject>().FirstOrDefault();
                try
                {
                    Match match = Regex.Match(adapter.Path.RelativePath, "\\\"(\\d+)\\\"$");
                    devnum = int.Parse(match.Groups[1].Value);
                }
                catch
                {
                    return;
                }

                customname = NetworkInterface.GetAllNetworkInterfaces().Where(
                    i => i.Description == adaptername
                ).Select(
                    i => " (" + i.Name + ")"
                ).FirstOrDefault();
            }
            public NetworkInterface ManagedAdapter => NetworkInterface.GetAllNetworkInterfaces().Where(
                        nic => nic.Description == adaptername
                    ).FirstOrDefault();

            public string Mac
            {
                get
                {
                    try
                    {
                        return BitConverter.ToString(ManagedAdapter.GetPhysicalAddress().GetAddressBytes()).Replace("-", "").ToUpper();
                    }
                    catch { return null; }
                }
            }

            public string RegistryKey => string.Format(@"SYSTEM\ControlSet001\Control\Class\{{4D36E972-E325-11CE-BFC1-08002BE10318}}\{0:D4}", devnum);
            public string RegistryMac
            {
                get
                {
                    try
                    {
                        using (RegistryKey regkey = Registry.LocalMachine.OpenSubKey(RegistryKey, false))
                        {
                            return regkey.GetValue("NetworkAddress").ToString();
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            public bool SetRegistryMac(string value)
            {
                bool shouldReenable = false;

                try
                {
                    if (value.Length > 0 && !Adapter.IsValidMac(value, false))
                    {
                        throw new Exception(value + " is not a valid mac address");
                    }

                    using (RegistryKey regkey = Registry.LocalMachine.OpenSubKey(RegistryKey, true))
                    {
                        if (regkey == null)
                        {
                            throw new Exception("Failed to open the registry key");
                        }

                        if (regkey.GetValue("AdapterModel") as string != adaptername
                            && regkey.GetValue("DriverDesc") as string != adaptername)
                        {
                            throw new Exception("Adapter not found in registry");
                        }

                        uint result = (uint)adapter.InvokeMethod("Disable", null);
                        if (result != 0)
                        {
                            throw new Exception("Failed to disable network adapter.");
                        }

                        shouldReenable = true;

                        if (value.Length > 0)
                        {
                            regkey.SetValue("NetworkAddress", value, RegistryValueKind.String);
                        }
                        else
                        {
                            regkey.DeleteValue("NetworkAddress");
                        }

                        return true;
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return false;
                }

                finally
                {
                    if (shouldReenable)
                    {
                        uint result = (uint)adapter.InvokeMethod("Enable", null);
                        if (result != 0)
                        {
                            MessageBox.Show("Failed to re-enable network adapter.");
                        }
                    }
                }
            }
            public override string ToString()
            {
                return adaptername + customname;
            }

            public static string GetNewMac()
            {
                System.Random r = new System.Random();

                byte[] bytes = new byte[6];
                r.NextBytes(bytes);

                bytes[0] = (byte)(bytes[0] | 0x02);

                bytes[0] = (byte)(bytes[0] & 0xfe);

                return MacToString(bytes);
            }

            public static bool IsValidMac(string mac, bool actual)
            {
                if (mac.Length != 12)
                {
                    return false;
                }

                if (mac != mac.ToUpper())
                {
                    return false;
                }

                if (!Regex.IsMatch(mac, "^[0-9A-F]*$"))
                {
                    return false;
                }

                if (actual)
                {
                    return true;
                }

                char c = mac[1];
                return (c == '2' || c == '6' || c == 'A' || c == 'E');
            }
            public static bool IsValidMac(byte[] bytes, bool actual)
            {
                return IsValidMac(Adapter.MacToString(bytes), actual);
            }
            public static string MacToString(byte[] bytes)
            {
                return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
            }
        }
        private void UpdateAddresses()
        {
            Adapter a = AdaptersComboBox.SelectedItem as Adapter;
            CurrentMacTextBox.Text = a.RegistryMac;
            ActualMacLabel.Text = a.Mac;
        }

        private void playaudio()
        {
            SoundPlayer audio = new SoundPlayer(Properties.Resources.Notification);
            audio.Play();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            foreach (string subkeyname in Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").GetSubKeyNames())
            {
                if (subkeyname.StartsWith("1") || subkeyname.StartsWith("2") || subkeyname.StartsWith("3") || subkeyname.StartsWith("4") || subkeyname.StartsWith("5") || subkeyname.StartsWith("6") || subkeyname.StartsWith("7") || subkeyname.StartsWith("8") || subkeyname.StartsWith("9"))
                {
                    shortkey.Text = subkeyname;
                    break;
                }
            }
            foreach (string subkeyname2 in Registry.CurrentUser.GetSubKeyNames())
            {
                if (subkeyname2.StartsWith("1") || subkeyname2.StartsWith("2") || subkeyname2.StartsWith("3") || subkeyname2.StartsWith("4") || subkeyname2.StartsWith("5") || subkeyname2.StartsWith("6") || subkeyname2.StartsWith("7") || subkeyname2.StartsWith("8") || subkeyname2.StartsWith("9"))
                {
                    longkey.Text = subkeyname2;
                    break;
                }
            }
        }

        private void ButtonUnban_Click(object sender, EventArgs e)
        {
            FocusText.Focus();
            if (!Adapter.IsValidMac(CurrentMacTextBox.Text, false))
            {
                MessageBox.Show("Entered MAC-address is not valid; will not update.", "Invalid MAC-address specified", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SetRegistryMac(CurrentMacTextBox.Text);

            rtb.Text = rtb.Text + Environment.NewLine;
            rtb.Text = rtb.Text + "->Mac Adress Randomized And Changed!";
            Thread.Sleep(500);
            if (longkey.Text != "No Second Key Connect Growtopia To Fix It" && shortkey.Text != "No First Key Connect Growtopia To Fix It")
            {
                Registry.CurrentUser.DeleteSubKey(longkey.Text);
                rtb.Text = rtb.Text + Environment.NewLine;
                rtb.Text = rtb.Text + "->The Second Key " + longkey.Text + " is deleted!";
                string microsoftKey = @"Software\Microsoft\" + shortkey.Text;
                Registry.CurrentUser.DeleteSubKey(microsoftKey);
                rtb.Text = rtb.Text + Environment.NewLine;
                rtb.Text = rtb.Text + "->The First Key " + shortkey.Text + " is deleted!";
                string cryptographyKey = @"SOFTWARE\Microsoft\Cryptography";
                RegistryKey ckey = Registry.LocalMachine.OpenSubKey(cryptographyKey, true);
                ckey.DeleteValue("MachineGuid");
                rtb.Text = rtb.Text + Environment.NewLine;
                rtb.Text = rtb.Text + "->The MachineGuid key is deleted!";
                longkey.Text = "No Second Key Connect Growtopia To Fix It";
                shortkey.Text = "No First Key Connect Growtopia To Fix It";
                rtb.Text = rtb.Text + Environment.NewLine;
                rtb.Text = rtb.Text + "->Done Unbanning!";
            }
            else
            {
                rtb.Text = rtb.Text + Environment.NewLine;
                rtb.Text = rtb.Text + "->Can't UNBAN! Open Growtopia and click 'Connect'. Then Click 'REFRESH' (its inside the UNBANNER!)";
                string message = "                                                Can't UNBAN!                                                          Tip: Open Growtopia and click 'Connect'. Then Restart the Trainer!";
                string title = "NotYetHAX";
                MessageBox.Show(message, title);
            }
        }

        private void SetRegistryMac(string mac)
        {
            Adapter a = AdaptersComboBox.SelectedItem as Adapter;

            if (a.SetRegistryMac(mac))
            {
                System.Threading.Thread.Sleep(100);
                UpdateAddresses();
            }
        }

        private void AdaptersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAddresses();
        }

        private bool ProcOpen = false;
        private void NotYetHAX_Load(object sender, EventArgs e)
        {
            if (Environment.Is64BitOperatingSystem)
            {

            }
            else
            {
                MessageBox.Show("This Trainer Won't Work With 32 Bit Computers...");
            }

            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
            string str = File.ReadAllText(path);
            HostFileEditor.Text = str;
            BGWorker.RunWorkerAsync();

            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces().Where(
                    a => Adapter.IsValidMac(a.GetPhysicalAddress().GetAddressBytes(), true)
                ).OrderByDescending(a => a.Speed))
            {
                AdaptersComboBox.Items.Add(new Adapter(adapter));
            }

            AdaptersComboBox.SelectedIndex = 0;
            timer4.Start();
            foreach (string subkeyname2 in Registry.CurrentUser.GetSubKeyNames())
            {
                if (subkeyname2.StartsWith("1") || subkeyname2.StartsWith("2") || subkeyname2.StartsWith("3") || subkeyname2.StartsWith("4") || subkeyname2.StartsWith("5") || subkeyname2.StartsWith("6") || subkeyname2.StartsWith("7") || subkeyname2.StartsWith("8") || subkeyname2.StartsWith("9"))
                {
                    longkey.Text = subkeyname2;
                    rtb.Text = "->The Second Key " + longkey.Text + " is found!";
                    break;
                }
            }
            foreach (string subkeyname in Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").GetSubKeyNames())
            {
                if (subkeyname.StartsWith("1") || subkeyname.StartsWith("2") || subkeyname.StartsWith("3") || subkeyname.StartsWith("4") || subkeyname.StartsWith("5") || subkeyname.StartsWith("6") || subkeyname.StartsWith("7") || subkeyname.StartsWith("8") || subkeyname.StartsWith("9"))
                {
                    shortkey.Text = subkeyname;
                    rtb.Text = rtb.Text + Environment.NewLine;
                    rtb.Text = rtb.Text + "->The First Key " + shortkey.Text + " is found!";
                    break;
                }
            }
            if (longkey.Text == "No Long Key Connect Growtopia To Fix It")
            {
                rtb.Text = "->First Key Cannot be found!";
            }
            if (shortkey.Text == "No First Key Connect Growtopia To Fix It")
            {
                rtb.Text = rtb.Text + Environment.NewLine;
                rtb.Text = rtb.Text + "->First Key Cannot be found!";
            }
            rtb.Text = rtb.Text + Environment.NewLine;
            rtb.Text = rtb.Text + "->MachineGuid key is found!";
            KeyPreview = true;
            int PID = mem.GetProcIdFromName("Growtopia.exe");
            if (PID > 0)
            {
                mem.OpenProcess(PID);
            }
            foreach (Control c in GetAllControls(this))
            {
                CheckBox b = c as CheckBox;
                if (b != null)
                {
                    b.FlatAppearance.MouseOverBackColor = b.BackColor;
                    b.FlatAppearance.MouseDownBackColor = b.BackColor;
                    b.FlatStyle = FlatStyle.Flat;
                }
            }
        }

        private void label2_MouseDown(object sender, MouseEventArgs e)
        {
            playaudio();
            Confirmation openTest = new Confirmation();
            openTest.Show();
        }

        private void label3_MouseDown(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        private void About_MouseDown(object sender, MouseEventArgs e)
        {
            {
                if (HackerModeActivate.BackColor == Color.Black)
                {
                    HackerModePictureBox.Hide();
                    HackerModePictureBoxAbout.Hide();
                    HackerModePictureBoxCheatPage1.Hide();
                    HackerModePictureBoxCheatPage2.Hide();
                    HackerModePictureBoxCheatPage3.Hide();
                    HackerModePictureBoxCheatPage4.Hide();
                    HackerModePictureBoxVisuals.Hide();
                    HackerModePictureBoxChangers.Hide();
                    HackerModePictureBoxSettings.Hide();
                    HackerModePictureBoxUnbanner.Hide();
                    HackerModePictureBox.Enabled = false;
                    HackerModePictureBoxAbout.Enabled = false;
                    HackerModePictureBoxCheatPage1.Enabled = false;
                    HackerModePictureBoxCheatPage2.Enabled = false;
                    HackerModePictureBoxCheatPage3.Enabled = false;
                    HackerModePictureBoxCheatPage4.Enabled = false;
                    HackerModePictureBoxVisuals.Enabled = false;
                    HackerModePictureBoxChangers.Enabled = false;
                    HackerModePictureBoxSettings.Enabled = false;
                    HackerModePictureBoxUnbanner.Enabled = false;
                    HackerModePictureBoxSpammer.Hide();
                    HackerModePictureBoxSpammer.Enabled = false;
                }
                else
                {
                    HackerModePictureBox.Hide();
                    HackerModePictureBoxAbout.Show();
                    HackerModePictureBoxCheatPage1.Hide();
                    HackerModePictureBoxCheatPage2.Hide();
                    HackerModePictureBoxCheatPage3.Hide();
                    HackerModePictureBoxCheatPage4.Hide();
                    HackerModePictureBoxVisuals.Hide();
                    HackerModePictureBoxChangers.Hide();
                    HackerModePictureBoxSettings.Hide();
                    HackerModePictureBoxUnbanner.Hide();
                    HackerModePictureBox.Enabled = false;
                    HackerModePictureBoxAbout.Enabled = true;
                    HackerModePictureBoxCheatPage1.Enabled = false;
                    HackerModePictureBoxCheatPage2.Enabled = false;
                    HackerModePictureBoxCheatPage3.Enabled = false;
                    HackerModePictureBoxCheatPage4.Enabled = false;
                    HackerModePictureBoxVisuals.Enabled = false;
                    HackerModePictureBoxChangers.Enabled = false;
                    HackerModePictureBoxSettings.Enabled = false;
                    HackerModePictureBoxUnbanner.Enabled = false;
                    HackerModePictureBoxSpammer.Hide();
                    HackerModePictureBoxSpammer.Enabled = false;
                }
            }
            Changers2.Hide();
            panel9.BringToFront();
            AnimatedFire2.Enabled = true;
            AnimatedFire1.Enabled = true;
            HostsFileViewerTimer.Stop();
            RandomMacAdressTimer.Stop();
            timer1.Start();
            timer4.Stop();
            CheatPage1.Hide();
            CheatPage2.Hide();
            CheatPage3.Hide();
            CheatPage4.Hide();
            Spammer.Hide();
            Visuals.Hide();
            Changers.Hide();
            Settings.Hide();
            panel9.Show();
            Pages.Hide();
            Page1.Hide();
            Page2.Hide();
            Page3.Hide();
            Page4.Hide();
            Unbanner.Hide();
        }

        private int r = 0, g = 255, b = 0;

        private void Cheats_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Show();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = true;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            Changers2.Hide();
            AnimatedFire2.Enabled = false;
            AnimatedFire1.Enabled = false;
            HostsFileViewerTimer.Stop();
            RandomMacAdressTimer.Stop();
            timer1.Stop();
            timer4.Stop();
            CheatPage1.BringToFront();
            CheatPage1.Show();
            Visuals.Hide();
            Changers.Hide();
            Settings.Hide();
            Spammer.Hide();
            Pages.Show();
            Page1.Show();
            Page2.Show();
            Page3.Show();
            Page4.Show();
            Unbanner.Hide();
        }

        private void VisualPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Show();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = true;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            Changers2.Hide();
            AnimatedFire2.Enabled = false;
            AnimatedFire1.Enabled = false;
            HostsFileViewerTimer.Stop();
            RandomMacAdressTimer.Stop();
            timer1.Stop();
            timer4.Stop();
            Visuals.BringToFront();
            Visuals.Show();
            CheatPage1.Hide();
            Changers.Hide();
            Settings.Hide();
            Spammer.Hide();
            Pages.Hide();
            Page1.Hide();
            Page2.Hide();
            Page3.Hide();
            Page4.Hide();
            Unbanner.Hide();
        }

        private void Page1_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Show();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = true;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = true;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            CheatPage1.BringToFront();
            CheatPage1.Show();
            CheatPage2.Hide();
            CheatPage3.Hide();
            CheatPage4.Hide();
        }

        private void button5_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Show();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = true;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            CheatPage2.BringToFront();
            CheatPage1.Hide();
            CheatPage2.Show();
            CheatPage3.Hide();
            CheatPage4.Hide();
        }

        private void Page3_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Show();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = true;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            CheatPage3.BringToFront();
            CheatPage1.Hide();
            CheatPage2.Hide();
            CheatPage3.Show();
            CheatPage4.Hide();
        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            label2.BackColor = Color.Red;
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            label2.BackColor = Color.Black;
        }

        private void label3_MouseEnter(object sender, EventArgs e)
        {
            label3.BackColor = Color.Gray;
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            label3.BackColor = Color.Black;
        }

        private void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!mem.OpenProcess("Growtopia.exe"))
            {
                Thread.Sleep(1000);
                return;
            }

            ProcOpen = true;

            Thread.Sleep(1000);
            BGWorker.ReportProgress(0);
        }

        private void NotYetHAX_Shown(object sender, EventArgs e)
        {
            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
            string str = File.ReadAllText(path);
            HostFileEditor.Text = str;
            HostsFileViewerTimer.Stop();
            RandomMacAdressTimer.Stop();
            shortkey.UseSystemPasswordChar = true;
            longkey.UseSystemPasswordChar = true;
            ActualMacLabel.UseSystemPasswordChar = true;
            CurrentMacTextBox.UseSystemPasswordChar = true;
        }

        private void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProcOpen)
            {
                GrowtopiaStatus.ForeColor = Color.Lime;
                GrowtopiaStatus.Text = "GROWTOPIA: RUNNING";
                mem.WriteMemory("Growtopia.exe+30A163", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+3F4023", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+4000AE", "bytes", "E9 19 01 00 00");
                mem.WriteMemory("Growtopia.exe+5F6204", "float", "5");
                mem.WriteMemory("Growtopia.exe+20143", "bytes", "90 90 90 90 90 90");
                mem.WriteMemory("Growtopia.exe+5C2E40", "string", "\n \n`2N`7o`2t`7Y`2e`7t`2H`7A`2X'`7s `2G`7r`2o`7w`2t`7o`2p`7i`2a `7/ `2T`7r`2a`7i`2n`7e`2r `7V`23`7.`26`71 \n`2T`7r`2a`7i`2n`7e`2r`7: `2A`7C`2T`7I`2V`7E \n`2F`7p`2s`7:`7%d");
                mem.WriteMemory("Growtopia.exe+2FBD5F", "bytes", "90 90 90 90 90 90 90");
            }
            else
            {
                GrowtopiaStatus.ForeColor = Color.Red;
                GrowtopiaStatus.Text = "GROWTOPIA: NOT RUNNING";
            }
        }

        private void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BGWorker.RunWorkerAsync();
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            IEnumerable<Process> chromeDriverProcesses = Process.GetProcesses().
                Where(pr => pr.ProcessName == "Fixed Cheat Engine By NotYetHAX (7.1)");
            foreach (Process process in chromeDriverProcesses)
            {
                MessageBox.Show("Why did you even try to open CE??");
                process.Kill();
                Application.Exit();
            }
            IEnumerable<Process> chromeDriverProcesse = Process.GetProcesses().
                Where(pr => pr.ProcessName == "Fixed Cheat Engine By NotYetHAX (7.2)");
            foreach (Process process in chromeDriverProcesse)
            {
                MessageBox.Show("Why did you even try to open CE??");
                process.Kill();
                Application.Exit();
            }
            IEnumerable<Process> chromeDriverProcess = Process.GetProcesses().
               Where(pr => pr.ProcessName == "Fixed Engine");
            foreach (Process process in chromeDriverProcess)
            {
                MessageBox.Show("Why did you even try to open CE??");
                process.Kill();
                Application.Exit();
            }
            IEnumerable<Process> chromeDriverProces = Process.GetProcesses().
               Where(pr => pr.ProcessName == "draqxorengine-x86_64");
            foreach (Process process in chromeDriverProces)
            {
                MessageBox.Show("Why did you even try to open CE??");
                process.Kill();
                Application.Exit();
            }
            IEnumerable<Process> chromeDriverProce = Process.GetProcesses().
               Where(pr => pr.ProcessName == "Cheat Engine");
            foreach (Process process in chromeDriverProce)
            {
                MessageBox.Show("Why did you even try to open CE??");
                process.Kill();
                Application.Exit();
            }
            IEnumerable<Process> processkill = Process.GetProcesses().
               Where(pr => pr.ProcessName == "cheatengine-x86_64");
            foreach (Process process in processkill)
            {
                MessageBox.Show("Why did you even try to open CE??");
                process.Kill();
                Application.Exit();
            }
        }

        private void button3_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Show();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = true;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            Changers2.Hide();
            AnimatedFire2.Enabled = false;
            AnimatedFire1.Enabled = false;
            HostsFileViewerTimer.Stop();
            RandomMacAdressTimer.Stop();
            timer1.Stop();
            timer4.Stop();
            Changers.BringToFront();
            Visuals.Hide();
            CheatPage1.Hide();
            Changers.Show();
            Settings.Hide();
            Spammer.Hide();
            Pages.Hide();
            Page1.Hide();
            Page2.Hide();
            Page3.Hide();
            Page4.Hide();
            Unbanner.Hide();
        }

        private void button4_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Show();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = true;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
            string str = File.ReadAllText(path);
            HostFileViewer.Text = str;
            Changers2.Hide();
            AnimatedFire2.Enabled = false;
            AnimatedFire1.Enabled = false;
            HostsFileViewerTimer.Start();
            RandomMacAdressTimer.Stop();
            timer1.Stop();
            timer4.Stop();
            Settings.BringToFront();
            Visuals.Hide();
            CheatPage1.Hide();
            Changers.Hide();
            Settings.Show();
            Spammer.Hide();
            Pages.Hide();
            Page1.Hide();
            Page2.Hide();
            Page3.Hide();
            Page4.Hide();
            Unbanner.Hide();
        }

        private void Change1_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b4", "int", HatChanger.Text);
        }

        private void Change4_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c4", "int", NeckChanger.Text);
        }

        private void Change5_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", ShirtChanger.Text);
        }

        private void Change6_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b8", "int", PantChanger.Text);
        }

        private void Change7_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", ShoeChanger.Text);
        }

        private void Change8_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2be", "int", HandChanger.Text);
        }

        private void Change9_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c0", "int", WingChanger.Text);
        }

        private void Change11_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,1a4", "int", PunchEffectChanger.Text);
        }

        private void Reset1_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`7");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b4", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2bc", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c4", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c4", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b8", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2be", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c0", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,1a4", "int", "0");
        }

        private void Reset2_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`7");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b4", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2bc", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c4", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c4", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b8", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2be", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c0", "int", "0");
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,1a4", "int", "0");
        }

        private void GmodeTimer_Tick(object sender, EventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0)
            {
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 83 88 00 00 00");
            }
            else
            {
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 84 88 00 00 00");
            }
        }
        private void Change10_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,8c", "float", PunchSpeedChanger.Text);
        }
        private void ModFlyTimer_Tick(object sender, EventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.S) & KeyStates.Down) > 0)
            {
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
            }
            else
            {
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
            }
        }

        private void DiscordLink1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://discord.gg/rEAT4SKg4S");
        }

        private void button6_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Show();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Show();
                HackerModePictureBoxSpammer.Enabled = true;
            }
            Changers2.Hide();
            AnimatedFire2.Enabled = false;
            AnimatedFire1.Enabled = false;
            HostsFileViewerTimer.Stop();
            RandomMacAdressTimer.Stop();
            timer1.Stop();
            timer4.Stop();
            Spammer.BringToFront();
            Spammer.Show();
            Visuals.Hide();
            CheatPage1.Hide();
            Changers.Hide();
            Settings.Hide();
            Pages.Hide();
            Page1.Hide();
            Page2.Hide();
            Page3.Hide();
            Page4.Hide();
            Unbanner.Hide();
        }

        private void TextTimer_Tick(object sender, EventArgs e)
        {
            SendKeys.Send("{ENTER}");
            SendKeys.Send(TextHere.Text);
            SendKeys.Send("{ENTER}");
        }

        private void Start_Click(object sender, EventArgs e)
        {
            FocusText.Focus();
            Start.Enabled = true;
            TextTimer.Start();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            FocusText.Focus();
            TextTimer.Stop();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            TextTimer.Interval = 1000;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            TextTimer.Interval = 2000;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            TextTimer.Interval = 3000;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            TextTimer.Interval = 4000;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            TextTimer.Interval = 5000;
        }

        private void PosYButton_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,c", "float", PosY.Text);
        }

        private void PosXButton_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,8", "float", PosX.Text);
        }

        private void PosXTimer_Tick(object sender, EventArgs e)
        {

        }

        private void RandomCrystals_Tick(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "2242");
            Thread.Sleep(250);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "2246");
            Thread.Sleep(250);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "2244");
            Thread.Sleep(250);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "2248");
            Thread.Sleep(250);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "2250");
        }

        private void EggSpawnTimer_Tick(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "611");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "611");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "611");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "611");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "611");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "611");
        }

        private void RandomSeedTimer_Tick(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "307");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "309");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "1673");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "2555");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "4477");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "3435");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5737");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5739");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "6007");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5749");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5767");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5773");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5791");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5835");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5983");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "5999");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "6011");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "8323");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "8315");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "8347");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "8377");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "8405");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "8425");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "37");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "95");
            Thread.Sleep(25);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,248", "int", "121");
        }

        private void SpinBotTimer_Tick(object sender, EventArgs e)
        {
            Thread.Sleep(200);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,60", "int", "1");
            Thread.Sleep(200);
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,60", "int", "257");
            Thread.Sleep(200);
        }

        private void CloseGT_Click(object sender, EventArgs e)
        {
            IEnumerable<Process> processkill = Process.GetProcesses().
            Where(pr => pr.ProcessName == "Growtopia");
            foreach (Process process in processkill)
            {
                process.Kill();
            }
        }

        private void StartGT_Click(object sender, EventArgs e)
        {
            string sPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = @"\Growtopia";
            string path = @"\Growtopia\Growtopia.exe";
            if (Directory.Exists(sPath + dir))
            {
                Process.Start(sPath + path);
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            label1.ForeColor = Color.FromArgb(r, g, b);
            if (r > 0 && b == 0)
            {
                r--;
                g++;
            }
            if (g > 0 && r == 0)
            {
                g--;
                b++;
            }
            if (b > 0 && g == 0)
            {
                b--;
                r++;
            }
        }

        private void NotYetHAX_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

        }

        private void AlwaysOnTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (AlwaysOnTop.BackColor == Color.Black)
            {
                FocusText.Focus();
                NotYetHAX.ActiveForm.TopMost = true;
                AlwaysOnTop.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                NotYetHAX.ActiveForm.TopMost = false;
                AlwaysOnTop.BackColor = Color.Black;
            }
        }

        private void GiveawayModeV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (GiveawayModeV1.BackColor == Color.Black)
            {
                {
                    GmodeTimer.Enabled = true;
                }
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "73 05");
                mem.WriteMemory("Growtopia.exe+45BE12", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+45BE8D", "bytes", "74 90");
                GiveawayModeV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                GmodeTimer.Enabled = false;
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "75 0C");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "74 05");
                GiveawayModeV1.BackColor = Color.Black;
            }
        }

        private void GiveawayModeV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (GiveawayModeV2.BackColor == Color.Black)
            {
                {
                    GmodeTimer.Enabled = true;
                }
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "73 05");
                mem.WriteMemory("Growtopia.exe+45BE12", "bytes", "90 90"); // fast pickup
                mem.WriteMemory("Growtopia.exe+45BE8D", "bytes", "74 90"); // fast drop
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "90 90 90 90"); // antib
                GiveawayModeV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                GmodeTimer.Enabled = false;
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "75 0C");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "74 05");
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "41 0F 28 C2"); // antib
                GiveawayModeV2.BackColor = Color.Black;
            }

        }

        private void GiveawayModeV3_MouseDown(object sender, MouseEventArgs e)
        {
            if (GiveawayModeV3.BackColor == Color.Black)
            {
                {
                    GmodeTimer.Enabled = true;
                }
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "73 05");
                mem.WriteMemory("Growtopia.exe+45BE12", "bytes", "90 90"); // fast pickup
                mem.WriteMemory("Growtopia.exe+45BE8D", "bytes", "74 90"); // fast drop
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "90 90 90 90"); // antib
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "90 90 90 90"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "90 90 90 90 90"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "90 90 90 90 90"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "90 90 90 90 90 90"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "90 90"); // NoJumpAnimationServer
                //
                GiveawayModeV3.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                GmodeTimer.Enabled = false;
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "75 0C");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "74 05");
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "41 0F 28 C2"); // antib
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "F3 0F 11 11"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "F3 0F 11 41 04"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "F3 0F 11 53 20"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "88 87 48 1A 00 00"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "74 1D"); // NoJumpAnimationServer
                //
                GiveawayModeV3.BackColor = Color.Black;
            }
        }

        private void ModFlyV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (ModFlyV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
                ModFlyV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                ModFlyV1.BackColor = Color.Black;
            }
        }

        private void ModFlyV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (ModFlyV2.BackColor == Color.Black)
            {
                {
                    ModFlyTimer.Enabled = true;
                }
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
                ModFlyV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                ModFlyTimer.Enabled = false;
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                ModFlyV2.BackColor = Color.Black;
            }
        }

        private void ModFlyV3_MouseDown(object sender, MouseEventArgs e)
        {
            if (ModFlyV3.BackColor == Color.Black)
            {
                {
                    FocusText.Focus();
                    ModFlyTimer.Enabled = true;
                }
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "90 90 90 90"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "90 90 90 90 90"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "90 90 90 90 90"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "90 90 90 90 90 90"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "90 90"); // NoJumpAnimationServer
                //
                ModFlyV3.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                ModFlyTimer.Enabled = false;
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "F3 0F 11 11"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "F3 0F 11 41 04"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "F3 0F 11 53 20"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "88 87 48 1A 00 00"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "74 1D"); // NoJumpAnimationServer
                //
                ModFlyV3.BackColor = Color.Black;
            }
        }

        private void AntiBounceV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiBounceV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+460F63", "bytes", "74 10");
                AntiBounceV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+460F63", "bytes", "75 10");
                AntiBounceV1.BackColor = Color.Black;
            }
        }

        private void AntiBounceV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiBounceV2.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "90 90 90 90");
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "90 90 90 90");
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "90 90 90 90 90");
                mem.WriteMemory("Growtopia.exe+3F8824", "bytes", "90 90 90 90 90");
                AntiBounceV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "41 0F 28 C2");
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "F3 0F 11 11");
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "F3 0F 11 41 04");
                mem.WriteMemory("Growtopia.exe+3F8824", "bytes", "F3 0F 11 43 24");
                AntiBounceV2.BackColor = Color.Black;
            }
        }

        private void AntiBounceV3_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiBounceV3.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "90 90 90 90");
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "90 90 90 90"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "90 90 90 90 90"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "90 90 90 90 90"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "90 90 90 90 90 90"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "90 90"); // NoJumpAnimationServer
                //
                AntiBounceV3.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+460EDA", "bytes", "41 0F 28 C2");
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "F3 0F 11 11"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "F3 0F 11 41 04"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "F3 0F 11 53 20"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "88 87 48 1A 00 00"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "74 1D"); // NoJumpAnimationServer
                //
                AntiBounceV3.BackColor = Color.Black;
            }
        }

        private void AntiSlide_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiSlide.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+402892", "bytes", "90 90");
                AntiSlide.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+402892", "bytes", "75 03");
                AntiSlide.BackColor = Color.Black;
            }
        }

        private void SlideMode_MouseDown(object sender, MouseEventArgs e)
        {
            if (SlideMode.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+402884", "bytes", "74 0E");
                SlideMode.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+402884", "bytes", "75 0E");
                SlideMode.BackColor = Color.Black;
            }
        }

        private void Growz_MouseDown(object sender, MouseEventArgs e)
        {
            if (Growz.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+401861", "bytes", "90 90 90 90");
                Growz.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+401861", "bytes", "F3 0F 5C D1");
                Growz.BackColor = Color.Black;
            }
        }

        private void GhostV3_MouseDown(object sender, MouseEventArgs e)
        {
            if (GhostV3.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "73 05");
                GhostV3.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "74 05");
                GhostV3.BackColor = Color.Black;
            }
        }

        private void NoClip_MouseDown(object sender, MouseEventArgs e)
        {
            if (NoClip.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "90 90");
                NoClip.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "75 0C");
                NoClip.BackColor = Color.Black;
            }
        }

        private void FastFallV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (FastFallV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EE26", "bytes", "75 0F");
                FastFallV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EE26", "bytes", "74 0F");
                FastFallV1.BackColor = Color.Black;
            }
        }

        private void FastFallV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (FastFallV2.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D248", "bytes", "90 90");
                FastFallV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D248", "bytes", "74 08");
                FastFallV2.BackColor = Color.Black;
            }
        }

        private void FastFallV3_MouseDown(object sender, MouseEventArgs e)
        {
            if (FastFallV3.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 83 88 00 00 00");
                FastFallV3.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 84 88 00 00 00");
                FastFallV3.BackColor = Color.Black;
            }
        }

        private void Float_MouseDown(object sender, MouseEventArgs e)
        {
            if (Float.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EE26", "bytes", "90 90");
                Float.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EE26", "bytes", "74 0F");
                Float.BackColor = Color.Black;
            }
        }

        private void SystemSpeed_MouseDown(object sender, MouseEventArgs e)
        {
            if (SystemSpeed.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+9E5A1", "bytes", "90 90 90 90");
                mem.WriteMemory("Growtopia.exe+9E341", "bytes", "90 90 90 90");
                mem.WriteMemory("Growtopia.exe+9E626", "bytes", "90 90 90 90");
                SystemSpeed.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+9E5A1", "bytes", "89 54 24 6C");
                mem.WriteMemory("Growtopia.exe+9E341", "bytes", "89 54 24 6C");
                mem.WriteMemory("Growtopia.exe+9E626", "bytes", "48 8B 43 08");
                SystemSpeed.BackColor = Color.Black;
            }
        }

        private void AntiPlatformWaterfall_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiPlatformWaterfall.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4508B8", "bytes", "90 90");
                AntiPlatformWaterfall.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4508B8", "bytes", "74 0D");
                AntiPlatformWaterfall.BackColor = Color.Black;
            }
        }

        private void AntiCheckpoint_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiCheckpoint.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45087A", "bytes", "90 90 90 90 90");
                AntiCheckpoint.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45087A", "bytes", "83 7C 02 04 1B");
                AntiCheckpoint.BackColor = Color.Black;
            }
        }

        private void AntiGravityWell_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiGravityWell.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F3016", "bytes", "90 90 90 90 90");
                AntiGravityWell.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F3016", "bytes", "E8 25 01 00 00");
                AntiGravityWell.BackColor = Color.Black;
            }
        }

        private void AntiPunchV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiPunchV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EA68", "bytes", "90 90 90 90 90");
                AntiPunchV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EA68", "bytes", "F3 0F 10 49 0C");
                AntiPunchV1.BackColor = Color.Black;
            }
        }

        private void AntiPunchV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiPunchV2.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F9335", "bytes", "0F 85 F4 00 00 00");
                AntiPunchV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F9335", "bytes", "0F 84 F4 00 00 00");
                AntiPunchV2.BackColor = Color.Black;
            }
        }

        private void AntiKnockback_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiKnockback.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+408A59", "bytes", "E9 C1 00 00 00");
                mem.WriteMemory("Growtopia.exe+408C8A", "bytes", "E9 68 01 00 00");
                AntiKnockback.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+408A59", "bytes", "0F 85 C0 00 00 00");
                mem.WriteMemory("Growtopia.exe+408C8A", "bytes", "0F 85 67 01 00 00");
                AntiKnockback.BackColor = Color.Black;
            }
        }

        private void AntiState_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiState.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+30B8FC", "bytes", "0F 85 24 16 00 00");
                AntiState.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+30B8FC", "bytes", "0F 84 24 16 00 00");
                AntiState.BackColor = Color.Black;
            }
        }

        private void PickupRangeUP_MouseDown(object sender, MouseEventArgs e)
        {
            if (PickupRangeUP.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EA76", "bytes", "90 90 90 90 90");
                PickupRangeUP.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40EA76", "bytes", "F3 0F 11 4A 04");
                PickupRangeUP.BackColor = Color.Black;
            }
        }

        private void AntiPortal_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiPortal.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4037EC", "bytes", "90 90");
                AntiPortal.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4037EC", "bytes", "75 67");
                AntiPortal.BackColor = Color.Black;
            }
        }

        private void AntiLgridSpike_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiLgridSpike.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FEA6E", "bytes", "0F 84 67 05 00 00");
                AntiLgridSpike.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FEA6E", "bytes", "0F 85 67 05 00 00");
                AntiLgridSpike.BackColor = Color.Black;
            }
        }

        private void MoveWhileDead_MouseDown(object sender, MouseEventArgs e)
        {
            if (MoveWhileDead.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FD7C3", "bytes", "90 90");
                MoveWhileDead.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FD7C3", "bytes", "75 1F");
                MoveWhileDead.BackColor = Color.Black;
            }
        }

        private void AntiRespawnV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiRespawnV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FD860", "bytes", "0F 85 8C 03 00 00");
                AntiRespawnV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FD860", "bytes", "0F 84 8C 03 00 00");
                AntiRespawnV1.BackColor = Color.Black;
            }
        }

        private void AntiRespawnV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiRespawnV2.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+95E48", "bytes", "EB 10");
                AntiRespawnV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+95E48", "bytes", "75 10");
                AntiRespawnV2.BackColor = Color.Black;
            }
        }

        private void FrogMode_MouseDown(object sender, MouseEventArgs e)
        {
            if (FrogMode.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+400B8C", "bytes", "0F 85 EF 00 00 00");
                FrogMode.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+400B8C", "bytes", "0F 84 EF 00 00 00");
                FrogMode.BackColor = Color.Black;
            }
        }

        private void TeleportUpDown_MouseDown(object sender, MouseEventArgs e)
        {
            if (TeleportUpDown.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40E741", "bytes", "90 90 90 90 90");
                TeleportUpDown.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40E741", "bytes", "F3 44 0F 5C D7");
                TeleportUpDown.BackColor = Color.Black;
            }
        }

        private void TeleportLeftDown_MouseDown(object sender, MouseEventArgs e)
        {
            if (TeleportLeftDown.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40E5A0", "bytes", "90 90 90 90 90");
                TeleportLeftDown.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40E5A0", "bytes", "F3 44 0F 5C CF");
                TeleportLeftDown.BackColor = Color.Black;
            }
        }

        private void AntiLavaDamage_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiLavaDamage.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FADFC", "bytes", "74 07");
                AntiLavaDamage.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FADFC", "bytes", "75 07");
                AntiLavaDamage.BackColor = Color.Black;
            }
        }

        private void AntiCactusDamage_MouseDown(object sender, MouseEventArgs e)
        {
            if (AntiCactusDamage.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FA43C", "bytes", "74 0A");
                AntiCactusDamage.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FA43C", "bytes", "75 0A");
                AntiCactusDamage.BackColor = Color.Black;
            }
        }

        private void WaterZ_MouseDown(object sender, MouseEventArgs e)
        {
            if (WaterZ.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40187B", "bytes", "76 19");
                WaterZ.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40187B", "bytes", "75 19");
                WaterZ.BackColor = Color.Black;
            }
        }

        private void Speedy_MouseDown(object sender, MouseEventArgs e)
        {
            if (Speedy.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40187B", "bytes", "76 19");
                mem.WriteMemory("Growtopia.exe+4018CD", "bytes", "75 26");
                Speedy.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40187B", "bytes", "75 19");
                mem.WriteMemory("Growtopia.exe+4018CD", "bytes", "74 26");
                Speedy.BackColor = Color.Black;
            }
        }

        private void SeeInsideChests_MouseDown(object sender, MouseEventArgs e)
        {
            if (SeeInsideChests.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4706F8", "bytes", "90 90 90 90 90 90");
                SeeInsideChests.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4706F8", "bytes", "0F 82 49 19 00 00");
                SeeInsideChests.BackColor = Color.Black;
            }
        }

        private void SeeLockedDoors_MouseDown(object sender, MouseEventArgs e)
        {
            if (SeeLockedDoors.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+404ABC", "bytes", "75 69");
                SeeLockedDoors.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+404ABC", "bytes", "74 69");
                SeeLockedDoors.BackColor = Color.Black;
            }
        }

        private void SeeGhosts_MouseDown(object sender, MouseEventArgs e)
        {
            if (SeeGhosts.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+431F76", "bytes", "75 0B");
                SeeGhosts.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+431F76", "bytes", "74 0B");
                SeeGhosts.BackColor = Color.Black;
            }
        }

        private void NightVision_MouseDown(object sender, MouseEventArgs e)
        {
            if (NightVision.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+440219", "bytes", "74 06");
                NightVision.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+440219", "bytes", "75 06");
                NightVision.BackColor = Color.Black;
            }
        }

        private void CemeSpeed_MouseDown(object sender, MouseEventArgs e)
        {
            if (CemeSpeed.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+44D640", "bytes", "90 90 90 90");
                CemeSpeed.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+44D640", "bytes", "0F B7 47 04");
                CemeSpeed.BackColor = Color.Black;
            }
        }

        private void SuperPunch_MouseDown(object sender, MouseEventArgs e)
        {
            if (SuperPunch.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+FE44D", "bytes", "75 47");
                mem.WriteMemory("Growtopia.exe+00753168,AA8,198,1A4", "int", "80");
                mem.WriteMemory("Growtopia.exe+00753168,AA8,198,8C", "float", "99999999999999999");
                mem.WriteMemory("Growtopia.exe+00753168,AA8,198,2BE", "2bytes", "5480");
                SuperPunch.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+FE44D", "bytes", "74 47");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,1a4", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,8c", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2be", "2bytes", "0");
                SuperPunch.BackColor = Color.Black;
            }
        }

        private void ExtendedPunch_MouseDown(object sender, MouseEventArgs e)
        {
            if (ExtendedPunch.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+2F8586", "bytes", "83 C0 05");
                ExtendedPunch.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+2F8586", "bytes", "83 C0 02");
                ExtendedPunch.BackColor = Color.Black;
            }
        }

        private void InvisiblePunchV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (InvisiblePunchV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FCD19", "bytes", "75 0F");
                InvisiblePunchV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3FCD19", "bytes", "74 0F");
                InvisiblePunchV1.BackColor = Color.Black;
            }
        }

        private void InvisiblePunchV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (InvisiblePunchV2.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F562F", "bytes", "0F 84 97 00 00 00");
                mem.WriteMemory("Growtopia.exe+3FADFC", "bytes", "90 90 90 90 90");
                InvisiblePunchV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F562F", "bytes", "0F 85 97 00 00 00");
                mem.WriteMemory("Growtopia.exe+3FADFC", "bytes", "E8 9F BF 06 00");
                InvisiblePunchV2.BackColor = Color.Black;
            }
        }

        private void LongPlaceSeed_MouseDown(object sender, MouseEventArgs e)
        {
            if (LongPlaceSeed.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "EB 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "90 90"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "90 90 90 90 90"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "E9 3D FE FF FF 90"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "EB 11"); //HoldDownSeeds
                LongPlaceSeed.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "74 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "74 1F"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "E8 20 6B 13 00"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "0F 85 3C FE FF FF"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "74 11"); //HoldDownSeeds
                LongPlaceSeed.BackColor = Color.Black;
            }
        }

        private void RandomSeeds_MouseDown(object sender, MouseEventArgs e)
        {
            if (RandomSeeds.BackColor == Color.Black)
            {
                FocusText.Focus();
                RandomSeedTimer.Start();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "EB 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "90 90"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "90 90 90 90 90"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "E9 3D FE FF FF 90"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "EB 11"); //HoldDownSeeds
                RandomSeeds.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                RandomSeedTimer.Stop();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "74 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "74 1F"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "E8 20 6B 13 00"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "0F 85 3C FE FF FF"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "74 11"); //HoldDownSeeds
                RandomSeeds.BackColor = Color.Black;
            }
        }

        private void RainbowCrystal_MouseDown(object sender, MouseEventArgs e)
        {
            if (RainbowCrystal.BackColor == Color.Black)
            {
                FocusText.Focus();
                RandomCrystals.Start();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "EB 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "90 90"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "90 90 90 90 90"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "E9 3D FE FF FF 90"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "EB 11"); //HoldDownSeeds
                RainbowCrystal.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                RandomCrystals.Stop();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "74 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "74 1F"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "E8 20 6B 13 00"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "0F 85 3C FE FF FF"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "74 11"); //HoldDownSeeds
                RainbowCrystal.BackColor = Color.Black;
            }
        }

        private void MagicEggSpawn_MouseDown(object sender, MouseEventArgs e)
        {
            if (MagicEggSpawn.BackColor == Color.Black)
            {
                FocusText.Focus();
                EggSpawnTimer.Start();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "EB 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "90 90"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "90 90 90 90 90"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "E9 3D FE FF FF 90"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "EB 11"); //HoldDownSeeds
                MagicEggSpawn.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                EggSpawnTimer.Stop();
                mem.WriteMemory("Growtopia.exe+317855", "bytes", "74 68"); //Dev Place
                mem.WriteMemory("Growtopia.exe+30D201", "bytes", "74 1F"); //Long Place Bypass
                mem.WriteMemory("Growtopia.exe+3179AB", "bytes", "E8 20 6B 13 00"); //PlaceInBlock
                mem.WriteMemory("Growtopia.exe+317D8F", "bytes", "0F 85 3C FE FF FF"); //PlantInAir
                mem.WriteMemory("Growtopia.exe+317445", "bytes", "74 11"); //HoldDownSeeds
                MagicEggSpawn.BackColor = Color.Black;
            }
        }

        private void SpinBot_MouseDown(object sender, MouseEventArgs e)
        {
            if (SpinBot.BackColor == Color.Black)
            {
                FocusText.Focus();
                SpinBotTimer.Start();
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "90 90 90 90"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "90 90 90 90 90"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "90 90 90 90 90"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "90 90 90 90 90 90"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "90 90"); // NoJumpAnimationServer
                //
                mem.WriteMemory("Growtopia.exe+40E0E2", "bytes", "90 90 90"); // nospin
                mem.WriteMemory("Growtopia.exe+40E059", "bytes", "90 90 90"); // nospin
                SpinBot.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                SpinBotTimer.Stop();
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "F3 0F 11 11"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "F3 0F 11 41 04"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "F3 0F 11 53 20"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "88 87 48 1A 00 00"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "74 1D"); // NoJumpAnimationServer
                //
                mem.WriteMemory("Growtopia.exe+40E0E2", "bytes", "88 48 61"); // nospin
                mem.WriteMemory("Growtopia.exe+40E059", "bytes", "88 51 61"); // nospin
                SpinBot.BackColor = Color.Black;
            }
        }

        private void DevMode_MouseDown(object sender, MouseEventArgs e)
        {
            if (DevMode.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+30D251", "bytes", "90 90");
                DevMode.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+30D251", "bytes", "74 5F");
                DevMode.BackColor = Color.Black;
            }
        }

        private void GravityV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (GravityV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D086", "bytes", "0F 85 16 01 00 00");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2");
                GravityV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D086", "bytes", "0F 84 17 01 00 00");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0");
                GravityV1.BackColor = Color.Black;
            }
        }

        private void GravityV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (GravityV2.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D086", "bytes", "0F 85 16 01 00 00");
                mem.WriteMemory("Growtopia.exe+40D060", "bytes", "74 1D");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2");
                GravityV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D086", "bytes", "0F 84 17 01 00 00");
                mem.WriteMemory("Growtopia.exe+40D060", "bytes", "74 1D");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0");
                GravityV2.BackColor = Color.Black;
            }
        }

        private void GravityV3_MouseDown(object sender, MouseEventArgs e)
        {
            if (GravityV3.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D086", "bytes", "0F 85 16 01 00 00");
                mem.WriteMemory("Growtopia.exe+40D060", "bytes", "74 1D");
                mem.WriteMemory("Growtopia.exe+40D026", "bytes", "74 27");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2");
                GravityV3.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D086", "bytes", "0F 84 17 01 00 00");
                mem.WriteMemory("Growtopia.exe+40D060", "bytes", "74 1D");
                mem.WriteMemory("Growtopia.exe+40D026", "bytes", "75 27");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0");
                GravityV3.BackColor = Color.Black;
            }
        }

        private void GravityV4_MouseDown(object sender, MouseEventArgs e)
        {
            if (GravityV4.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D026", "bytes", "74 49");
                mem.WriteMemory("Growtopia.exe+40D1B8", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2");
                GravityV4.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D026", "bytes", "75 49");
                mem.WriteMemory("Growtopia.exe+40D1B8", "bytes", "75 27");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0");
                GravityV4.BackColor = Color.Black;
            }
        }

        private void HigherJump_MouseDown(object sender, MouseEventArgs e)
        {
            if (HigherJump.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D060", "bytes", "74 1D");
                HigherJump.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D060", "bytes", "75 1D");
                HigherJump.BackColor = Color.Black;
            }
        }

        private void Fly_MouseDown(object sender, MouseEventArgs e)
        {
            if (Fly.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D0D2", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 85 88 00 00 00");
                Fly.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40D0D2", "bytes", "74 08");
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 84 88 00 00 00");
                Fly.BackColor = Color.Black;
            }
        }

        private void UnlimitedJump_MouseDown(object sender, MouseEventArgs e)
        {
            if (UnlimitedJump.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "75 5D");
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 83 88 00 00 00");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2");
                UnlimitedJump.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                mem.WriteMemory("Growtopia.exe+40C9F9", "bytes", "0F 84 88 00 00 00");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0");
                UnlimitedJump.BackColor = Color.Black;
            }
        }

        private void MoonWalk_MouseDown(object sender, MouseEventArgs e)
        {
            if (MoonWalk.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40E0D9", "bytes", "75 07");
                mem.WriteMemory("Growtopia.exe+40E050", "bytes", "74 07");
                MoonWalk.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40E0D9", "bytes", "74 07");
                mem.WriteMemory("Growtopia.exe+40E050", "bytes", "75 07");
                MoonWalk.BackColor = Color.Black;
            }
        }

        private void SlowWalkV1_MouseDown(object sender, MouseEventArgs e)
        {
            if (SlowWalkV1.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+401872", "bytes", "90 90");
                SlowWalkV1.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+401872", "bytes", "74 22");
                SlowWalkV1.BackColor = Color.Black;
            }
        }

        private void SlowWalkV2_MouseDown(object sender, MouseEventArgs e)
        {
            if (SlowWalkV2.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40189D", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+40189D", "bytes", "90 90");
                SlowWalkV2.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40189D", "bytes", "74 27");
                mem.WriteMemory("Growtopia.exe+40189D", "bytes", "74 22");
                SlowWalkV2.BackColor = Color.Black;
            }
        }

        private void DanceMove_MouseDown(object sender, MouseEventArgs e)
        {
            if (DanceMove.BackColor == Color.Black)
            {
                FocusText.Focus();
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "90 90 90 90"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "90 90 90 90 90"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "90 90 90 90 90"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "90 90 90 90 90 90"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "90 90"); // NoJumpAnimationServer
                //
                DanceMove.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                //
                mem.WriteMemory("Growtopia.exe+11FEF0", "bytes", "F3 0F 11 11"); // dancemove client
                mem.WriteMemory("Growtopia.exe+11FEF7", "bytes", "F3 0F 11 41 04"); // dancemove server
                mem.WriteMemory("Growtopia.exe+3F881F", "bytes", "F3 0F 11 53 20"); // dancemove down client
                mem.WriteMemory("Growtopia.exe+11FF76", "bytes", "88 87 48 1A 00 00"); // NoJumpAnimationClient
                mem.WriteMemory("Growtopia.exe+3F883A", "bytes", "74 1D"); // NoJumpAnimationServer
                //
                DanceMove.BackColor = Color.Black;
            }
        }

        private void Tractor_MouseDown(object sender, MouseEventArgs e)
        {
            if (Tractor.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+403456", "bytes", "0F 84 B8 00 00 00");
                Tractor.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+403456", "bytes", "0F 85 B8 00 00 00");
                Tractor.BackColor = Color.Black;
            }
        }

        private void AutoPlant_MouseDown(object sender, MouseEventArgs e)
        {
            if (AutoPlant.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+402F96", "bytes", "0F 84 D5 00 00 00");
                AutoPlant.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+402F96", "bytes", "0F 85 D5 00 00 00");
                AutoPlant.BackColor = Color.Black;
            }
        }

        private void AutoPunch_MouseDown(object sender, MouseEventArgs e)
        {
            if (AutoPunch.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40C9D0", "bytes", "90 90 90 90 90 90");
                AutoPunch.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+40C9D0", "bytes", "44 0F 29 44 24 40");
                AutoPunch.BackColor = Color.Black;
            }
        }

        private void AutoJump_MouseDown(object sender, MouseEventArgs e)
        {
            if (AutoJump.BackColor == Color.Black)
            {
                FocusText.Focus();
                ModFlyTimer.Start();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+4029FD", "bytes", "90 90");
                AutoJump.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                ModFlyTimer.Stop();
                mem.WriteMemory("Growtopia.exe+45FF17", "bytes", "74 5D");
                mem.WriteMemory("Growtopia.exe+4029FD", "bytes", "74 09");
                AutoJump.BackColor = Color.Black;
            }
        }

        private void SlowMotion_MouseDown(object sender, MouseEventArgs e)
        {
            if (SlowMotion.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+95DE9", "bytes", "90 90");
                SlowMotion.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+95DE9", "bytes", "72 A7");
                SlowMotion.BackColor = Color.Black;
            }
        }

        private void DoubleJump_MouseDown(object sender, MouseEventArgs e)
        {
            if (DoubleJump.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2");
                DoubleJump.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0");
                DoubleJump.BackColor = Color.Black;
            }
        }

        private void CannotCollect_MouseDown(object sender, MouseEventArgs e)
        {
            if (CannotCollect.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+2FC039", "bytes", "74 0B");
                CannotCollect.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+2FC039", "bytes", "75 0B");
                CannotCollect.BackColor = Color.Black;
            }
        }

        private void CannotMove_MouseDown(object sender, MouseEventArgs e)
        {
            if (CannotMove.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45D190", "bytes", "74 50");
                CannotMove.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45D190", "bytes", "75 50");
                CannotMove.BackColor = Color.Black;
            }
        }

        private void AutoRespawn_MouseDown(object sender, MouseEventArgs e)
        {
            if (AutoRespawn.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "74 0C");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,138", "int", "0");
                mem.WriteMemory("Growtopia.exe+9E5A1", "bytes", "90 90 90 90");
                mem.WriteMemory("Growtopia.exe+9E341", "bytes", "90 90 90 90");
                mem.WriteMemory("Growtopia.exe+9E626", "bytes", "90 90 90 90");
                AutoRespawn.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4508A7", "bytes", "75 0C");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,138", "int", "0");
                mem.WriteMemory("Growtopia.exe+9E5A1", "bytes", "89 54 24 6C");
                mem.WriteMemory("Growtopia.exe+9E341", "bytes", "89 54 24 6C");
                mem.WriteMemory("Growtopia.exe+9E626", "bytes", "48 8B 43 08");
                AutoRespawn.BackColor = Color.Black;
            }
        }

        private void FastPickup_MouseDown(object sender, MouseEventArgs e)
        {
            if (FastPickup.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45BE12", "bytes", "90 90");
                FastPickup.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45BE12", "bytes", "73 19");
                FastPickup.BackColor = Color.Black;
            }
        }

        private void FastDrop_MouseDown(object sender, MouseEventArgs e)
        {
            if (FastDrop.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45BE8D", "bytes", "75 90");
                FastDrop.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+45BE8D", "bytes", "74 90");
                FastDrop.BackColor = Color.Black;
            }
        }

        private void TimeMachine_MouseDown(object sender, MouseEventArgs e)
        {
            if (TimeMachine.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+451CAA", "bytes", "90 90");
                TimeMachine.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+451CAA", "bytes", "74 0A");
                TimeMachine.BackColor = Color.Black;
            }
        }

        private void SteamPipeInv_MouseDown(object sender, MouseEventArgs e)
        {
            if (SteamPipeInv.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3ECA37", "bytes", "0F 84 9F 00 00 00");
                SteamPipeInv.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3ECA37", "bytes", "0F 85 9F 00 00 00");
                SteamPipeInv.BackColor = Color.Black;
            }
        }

        private void RainbowBlocks_MouseDown(object sender, MouseEventArgs e)
        {
            if (RainbowBlocks.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+478776", "bytes", "90 90 90 90 90 90");
                RainbowBlocks.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+478776", "bytes", "0F 85 88 00 00 00");
                RainbowBlocks.BackColor = Color.Black;
            }
        }

        private void RainbowInv_MouseDown(object sender, MouseEventArgs e)
        {
            if (RainbowInv.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3EC5EB", "bytes", "0F 84 BB 00 00 00");
                RainbowInv.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3EC5EB", "bytes", "0F 85 BB 00 00 00");
                RainbowInv.BackColor = Color.Black;
            }
        }

        private void RainbowInvFAST_MouseDown(object sender, MouseEventArgs e)
        {
            if (RainbowInvFAST.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3EC5EB", "bytes", "0F 84 BB 00 00 00");
                mem.WriteMemory("Growtopia.exe+3EC610", "bytes", "74 25");
                mem.WriteMemory("Growtopia.exe+3EC628", "bytes", "90 90 90 90 90 90 90 90");
                RainbowInvFAST.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3EC5EB", "bytes", "0F 85 BB 00 00 00");
                mem.WriteMemory("Growtopia.exe+3EC610", "bytes", "75 25");
                mem.WriteMemory("Growtopia.exe+3EC628", "bytes", "F3 0F 5E 05 68 74 1D 00");
                RainbowInvFAST.BackColor = Color.Black;
            }
        }

        private void TrippyBlocks_MouseDown(object sender, MouseEventArgs e)
        {
            if (TrippyBlocks.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+478541", "bytes", "90 90 90 90 90 90");
                TrippyBlocks.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+478541", "bytes", "0F 85 44 01 00 00");
                TrippyBlocks.BackColor = Color.Black;
            }
        }

        private void WaterVisual_MouseDown(object sender, MouseEventArgs e)
        {
            if (WaterVisual.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4065DC", "bytes", "75 1E");
                WaterVisual.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+4065DC", "bytes", "74 1E");
                WaterVisual.BackColor = Color.Black;
            }
        }

        private void NoItemBorder_MouseDown(object sender, MouseEventArgs e)
        {
            if (NoItemBorder.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+471DA5", "bytes", "90 90 90 90 90 90");
                NoItemBorder.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+471DA5", "bytes", "F3 41 0F 58 45 00");
                NoItemBorder.BackColor = Color.Black;
            }
        }

        private void DragonKnightSet_MouseDown(object sender, MouseEventArgs e)
        {
            if (DragonKnightSet.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c0", "int", "7734");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b4", "int", "7726");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "7728");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b8", "int", "7730");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2ba", "int", "7732");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2bc", "int", "3576");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c2", "int", "290");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,1a4", "int", "56");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2");
                DragonKnightSet.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c0", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b4", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b8", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2ba", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2bc", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c2", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,1a4", "int", "0");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0");
                DragonKnightSet.BackColor = Color.Black;
            }
        }

        private void LegendaryName_MouseDown(object sender, MouseEventArgs e)
        {
            if (LegendaryName.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F5F9F", "bytes", "75 13");
                LegendaryName.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F5F9F", "bytes", "74 13");
                LegendaryName.BackColor = Color.Black;
            }
        }

        private void NoName_MouseDown(object sender, MouseEventArgs e)
        {
            if (NoName.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F6292", "bytes", "90 90");
                NoName.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+3F6292", "bytes", "76 21");
                NoName.BackColor = Color.Black;
            }
        }

        private void OpacityTrackBar_Scroll(object sender, ScrollEventArgs e)
        {
            System.Windows.Forms.Form.ActiveForm.Opacity = (OpacityTrackBar.Value / 100.0);
            TrackbarText.Text = OpacityTrackBar.Value.ToString();
        }

        private void HaveFun_Click(object sender, EventArgs e)
        {

        }

        private void Pages_Click(object sender, EventArgs e)
        {
            FocusText.Focus();
        }

        private void button2_MouseDown(object sender, MouseEventArgs e)
        {
            if (FocusText.BackColor == Color.Black)
            {
                FocusText.Focus();
            }
            else
            {
                FocusText.Focus();
            }
        }

        private void button5_MouseDown_1(object sender, MouseEventArgs e)
        {
            if (FocusText.BackColor == Color.Black)
            {
                FocusText.Focus();
            }
            else
            {
                FocusText.Focus();
            }
        }

        private void SpamEnableDisable_MouseDown(object sender, MouseEventArgs e)
        {
            if (SpamEnableDisable.BackColor == Color.Black)
            {
                FocusText.Focus();
                Start.Enabled = true;
                Stop.Enabled = true;
                SpamEnableDisable.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                TextTimer.Stop();
                Start.Enabled = false;
                Stop.Enabled = false;
                SpamEnableDisable.BackColor = Color.Black;
            }
        }

        private void button11_MouseDown(object sender, MouseEventArgs e)
        {
            if (button11.BackColor == Color.Black)
            {
                FocusText.Focus();
                TextTimer.Interval = 5000;
                button11.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                TextTimer.Stop();
                TextTimer.Interval = 999999999;
                button11.BackColor = Color.Black;
            }
        }

        private void button10_MouseDown(object sender, MouseEventArgs e)
        {
            if (button10.BackColor == Color.Black)
            {
                FocusText.Focus();
                TextTimer.Interval = 6000;
                button10.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                TextTimer.Stop();
                TextTimer.Interval = 999999999;
                button10.BackColor = Color.Black;
            }
        }

        private void button9_MouseDown(object sender, MouseEventArgs e)
        {
            if (button9.BackColor == Color.Black)
            {
                FocusText.Focus();
                TextTimer.Interval = 7000;
                button9.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                TextTimer.Stop();
                TextTimer.Interval = 999999999;
                button9.BackColor = Color.Black;
            }
        }

        private void button8_MouseDown(object sender, MouseEventArgs e)
        {
            if (button8.BackColor == Color.Black)
            {
                FocusText.Focus();
                TextTimer.Interval = 8000;
                button8.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                TextTimer.Stop();
                TextTimer.Interval = 999999999;
                button8.BackColor = Color.Black;
            }
        }

        private void button7_MouseDown(object sender, MouseEventArgs e)
        {
            if (button7.BackColor == Color.Black)
            {
                FocusText.Focus();
                TextTimer.Interval = 9000;
                button7.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                TextTimer.Stop();
                TextTimer.Interval = 999999999;
                button7.BackColor = Color.Black;
            }
        }

        private void UnbannerButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Show();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = true;
            }
            foreach (string subkeyname in Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").GetSubKeyNames())
            {
                if (subkeyname.StartsWith("1") || subkeyname.StartsWith("2") || subkeyname.StartsWith("3") || subkeyname.StartsWith("4") || subkeyname.StartsWith("5") || subkeyname.StartsWith("6") || subkeyname.StartsWith("7") || subkeyname.StartsWith("8") || subkeyname.StartsWith("9"))
                {
                    shortkey.Text = subkeyname;
                    break;
                }
            }
            foreach (string subkeyname2 in Registry.CurrentUser.GetSubKeyNames())
            {
                if (subkeyname2.StartsWith("1") || subkeyname2.StartsWith("2") || subkeyname2.StartsWith("3") || subkeyname2.StartsWith("4") || subkeyname2.StartsWith("5") || subkeyname2.StartsWith("6") || subkeyname2.StartsWith("7") || subkeyname2.StartsWith("8") || subkeyname2.StartsWith("9"))
                {
                    longkey.Text = subkeyname2;
                    break;
                }
            }
            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
            string str = File.ReadAllText(path);
            HostFileEditor.Text = str;

            UpdateAddresses();

            Changers2.Hide();
            AnimatedFire2.Enabled = false;
            AnimatedFire1.Enabled = false;
            HostsFileViewerTimer.Stop();
            RandomMacAdressTimer.Start();
            timer1.Stop();
            Unbanner.BringToFront();
            Visuals.Hide();
            CheatPage1.Hide();
            Changers.Hide();
            Settings.Hide();
            Spammer.Hide();
            Pages.Hide();
            Page1.Hide();
            Page2.Hide();
            Page3.Hide();
            Page4.Hide();
            Unbanner.Show();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Application.Exit();
            System.Diagnostics.Process.Start(Application.ExecutablePath);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            FocusText.Focus();
        }

        private void RandomMacAdressTimer_Tick(object sender, EventArgs e)
        {
            CurrentMacTextBox.Text = Adapter.GetNewMac();
        }

        private void SHOWINFO_MouseDown(object sender, MouseEventArgs e)
        {
            if (SHOWINFO.BackColor == Color.Black)
            {
                foreach (string subkeyname in Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").GetSubKeyNames())
                {
                    if (subkeyname.StartsWith("1") || subkeyname.StartsWith("2") || subkeyname.StartsWith("3") || subkeyname.StartsWith("4") || subkeyname.StartsWith("5") || subkeyname.StartsWith("6") || subkeyname.StartsWith("7") || subkeyname.StartsWith("8") || subkeyname.StartsWith("9"))
                    {
                        shortkey.Text = subkeyname;
                        break;
                    }
                }
                foreach (string subkeyname2 in Registry.CurrentUser.GetSubKeyNames())
                {
                    if (subkeyname2.StartsWith("1") || subkeyname2.StartsWith("2") || subkeyname2.StartsWith("3") || subkeyname2.StartsWith("4") || subkeyname2.StartsWith("5") || subkeyname2.StartsWith("6") || subkeyname2.StartsWith("7") || subkeyname2.StartsWith("8") || subkeyname2.StartsWith("9"))
                    {
                        longkey.Text = subkeyname2;
                        break;
                    }
                }
                FocusText.Focus();
                shortkey.UseSystemPasswordChar = false;
                longkey.UseSystemPasswordChar = false;
                ActualMacLabel.UseSystemPasswordChar = false;
                CurrentMacTextBox.UseSystemPasswordChar = false;
                SHOWINFO.BackColor = Color.Lime;
            }
            else
            {
                foreach (string subkeyname in Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").GetSubKeyNames())
                {
                    if (subkeyname.StartsWith("1") || subkeyname.StartsWith("2") || subkeyname.StartsWith("3") || subkeyname.StartsWith("4") || subkeyname.StartsWith("5") || subkeyname.StartsWith("6") || subkeyname.StartsWith("7") || subkeyname.StartsWith("8") || subkeyname.StartsWith("9"))
                    {
                        shortkey.Text = subkeyname;
                        break;
                    }
                }
                foreach (string subkeyname2 in Registry.CurrentUser.GetSubKeyNames())
                {
                    if (subkeyname2.StartsWith("1") || subkeyname2.StartsWith("2") || subkeyname2.StartsWith("3") || subkeyname2.StartsWith("4") || subkeyname2.StartsWith("5") || subkeyname2.StartsWith("6") || subkeyname2.StartsWith("7") || subkeyname2.StartsWith("8") || subkeyname2.StartsWith("9"))
                    {
                        longkey.Text = subkeyname2;
                        break;
                    }
                }
                FocusText.Focus();
                shortkey.UseSystemPasswordChar = true;
                longkey.UseSystemPasswordChar = true;
                ActualMacLabel.UseSystemPasswordChar = true;
                CurrentMacTextBox.UseSystemPasswordChar = true;
                SHOWINFO.BackColor = Color.Black;
            }
        }

        private void label149_MouseDown(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start("https://mega.nz/file/AEJi2bxS#tkC3HKwp7r3o5yzUt0cN_n_qzUxyDKc9hzCmsNdWKhQ");
        }

        private void guna2Button1_MouseDown(object sender, MouseEventArgs e)
        {
            rtb.Clear();
        }

        private void guna2CirclePictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            string message = "If you can't unban open Growtopia and click 'Connect'. Then Restart the Trainer! ( Restart Button Is Inside The 'Settings' Tab )";
            string title = "NotYetHAX";
            MessageBox.Show(message, title);
        }

        private void Change2_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c2", "int", HairChanger.Text);
        }

        private void Change3_Click(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2bc", "int", EyeChanger.Text);
        }

        private void Reset2_ClientSizeChanged(object sender, EventArgs e)
        {
            //lazy to del, ignore this lmfao
        }

        private void TextColorChanger_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedColor.Text = TextColorChanger.Text;
            if (SelectedColor.Text == "Blue Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`1");
            }
            else
            {

            }
            if (SelectedColor.Text == "Red Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`4");
            }
            else
            {

            }
            if (SelectedColor.Text == "Orange Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`8");
            }
            else
            {

            }
            if (SelectedColor.Text == "Normal Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`7");
            }
            else
            {

            }
            if (SelectedColor.Text == "Green Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`2");
            }
            else
            {

            }
            if (SelectedColor.Text == "Yellow Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`9");
            }
            else
            {

            }
            if (SelectedColor.Text == "Purple Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`#");
            }
            else
            {

            }
            if (SelectedColor.Text == "Black Color")
            {
                mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`b");
            }
            else
            {

            }
        }

        private void RainbowTextTimer_Tick(object sender, EventArgs e)
        {
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`4");
            Thread.Sleep(1000);
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`8");
            Thread.Sleep(1000);
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`9");
            Thread.Sleep(1000);
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`2");
            Thread.Sleep(1000);
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`1");
            Thread.Sleep(1000);
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`1");
            Thread.Sleep(1000);
            mem.WriteMemory("Growtopia.exe+6226D0", "string", "action|input\n|text|`#");
        }

        private void RainbowText_MouseDown(object sender, MouseEventArgs e)
        {
            if (RainbowText.BackColor == Color.Black)
            {
                FocusText.Focus();
                RainbowTextTimer.Start();
                RainbowText.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                RainbowTextTimer.Stop();
                RainbowText.BackColor = Color.Black;
            }
        }

        private void HostFileEditorPanel_MouseDown(object sender, MouseEventArgs e)
        {
            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
            File.WriteAllText(path, HostFileEditor.Text);
        }

        private void HostsFileViewerTimer_Tick(object sender, EventArgs e)
        {
            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
            string str = File.ReadAllText(path);
            HostFileViewer.Text = str;
        }

        private void OpenHostsFile_MouseDown(object sender, MouseEventArgs e)
        {
            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc\hosts");
            Process.Start(path);
        }

        private void CopyToClipboard_MouseDown(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(HostFileViewer.Text);
        }

        private void label115_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Show();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = true;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            Changers2.BringToFront();
            Changers2.Show();
            Changers.Hide();
        }

        private void label153_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Show();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = true;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
            }
            Changers2.Hide();
            Changers.Show();
        }

        private void HackerModePreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModePreview.BackColor == Color.Black)
            {
                FocusText.Focus();
                HackerModePictureBoxPreview.Enabled = true;
                HackerModePreview.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                HackerModePictureBoxPreview.Enabled = false;
                HackerModePreview.BackColor = Color.Black;
            }
        }

        private void HackerModeActivate_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                FocusText.Focus();
                HackerModePictureBox.Show();
                HackerModePictureBox.Enabled = true;
                HackerModeActivate.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                HackerModePictureBox.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModeActivate.BackColor = Color.Black;
            }
        }

        private void RegistryRefresher_MouseDown(object sender, MouseEventArgs e)
        {
            UpdateAddresses();
            if (RegistryRefresher.BackColor == Color.Black)
            {
                foreach (string subkeyname in Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").GetSubKeyNames())
                {
                    if (subkeyname.StartsWith("1") || subkeyname.StartsWith("2") || subkeyname.StartsWith("3") || subkeyname.StartsWith("4") || subkeyname.StartsWith("5") || subkeyname.StartsWith("6") || subkeyname.StartsWith("7") || subkeyname.StartsWith("8") || subkeyname.StartsWith("9"))
                    {
                        shortkey.Text = subkeyname;
                        break;
                    }
                }
                foreach (string subkeyname2 in Registry.CurrentUser.GetSubKeyNames())
                {
                    if (subkeyname2.StartsWith("1") || subkeyname2.StartsWith("2") || subkeyname2.StartsWith("3") || subkeyname2.StartsWith("4") || subkeyname2.StartsWith("5") || subkeyname2.StartsWith("6") || subkeyname2.StartsWith("7") || subkeyname2.StartsWith("8") || subkeyname2.StartsWith("9"))
                    {
                        longkey.Text = subkeyname2;
                        break;
                    }
                }
            }
            else
            {
                foreach (string subkeyname in Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").GetSubKeyNames())
                {
                    if (subkeyname.StartsWith("1") || subkeyname.StartsWith("2") || subkeyname.StartsWith("3") || subkeyname.StartsWith("4") || subkeyname.StartsWith("5") || subkeyname.StartsWith("6") || subkeyname.StartsWith("7") || subkeyname.StartsWith("8") || subkeyname.StartsWith("9"))
                    {
                        shortkey.Text = subkeyname;
                        break;
                    }
                }
                foreach (string subkeyname2 in Registry.CurrentUser.GetSubKeyNames())
                {
                    if (subkeyname2.StartsWith("1") || subkeyname2.StartsWith("2") || subkeyname2.StartsWith("3") || subkeyname2.StartsWith("4") || subkeyname2.StartsWith("5") || subkeyname2.StartsWith("6") || subkeyname2.StartsWith("7") || subkeyname2.StartsWith("8") || subkeyname2.StartsWith("9"))
                    {
                        longkey.Text = subkeyname2;
                        break;
                    }
                }
            }
        }

        private void OpenHostsFilesLocation_MouseDown(object sender, MouseEventArgs e)
        {
            string path = Path.Combine(Environment.SystemDirectory, @"drivers\etc");
            Process.Start(path);
        }

        private void SeeFruits_MouseDown(object sender, MouseEventArgs e)
        {
            if (SeeFruits.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+44E0BA", "bytes", "74 08");
                SeeFruits.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+44E0BA", "bytes", "75 08");
                SeeFruits.BackColor = Color.Black;
            }
        }

        private void InvisibleBlocks_MouseDown(object sender, MouseEventArgs e)
        {
            if (InvisibleBlocks.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+44E50C", "bytes", "90 90 90 90");
                InvisibleBlocks.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+44E50C", "bytes", "0F B7 43 04");
                InvisibleBlocks.BackColor = Color.Black;
            }
        }

        private void OpacityChecker_Tick(object sender, EventArgs e)
        {
            if (Opacity == 0)
            {
                Application.Exit();
            }
            else
            {

            }
        }

        private void FadeOut_Tick(object sender, EventArgs e)
        {
            Opacity -= 0.01;
        }

        private void FadeIn_Tick(object sender, EventArgs e)
        {
            Opacity += 0.01;
        }

        private void Pages_MouseDown(object sender, MouseEventArgs e)
        {
            FocusText.Focus();
        }

        private void button47_MouseDown(object sender, MouseEventArgs e)
        {
            if (button47.BackColor == Color.Black)
            {
                FocusText.Focus();
                float value = mem.ReadFloat("Growtopia.exe + 0073EEA8, aa8, 198, c", "float");
                value += 25;
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,c", "float", value.ToString());
                button47.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                float value = mem.ReadFloat("Growtopia.exe + 0073EEA8, aa8, 198, c", "float");
                value += 25;
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,c", "float", value.ToString());
                button47.BackColor = Color.Black;
            }
        }

        private void CCTV_MouseDown(object sender, MouseEventArgs e)
        {
            if (CCTV.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+309F6B", "bytes", "74 72");
                CCTV.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+309F6B", "bytes", "75 72");
                CCTV.BackColor = Color.Black;
            }
        }

        private void LegendBot_MouseDown(object sender, MouseEventArgs e)
        {
            if (LegendBot.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+1CFF46", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c0", "int", "1780"); //wing
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "1780"); //shirt
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "2"); //dbljump
                LegendBot.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+1CFF46", "bytes", "75 25");
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2c0", "int", "0"); //wing
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,2b6", "int", "0"); //shirt
                mem.WriteMemory("Growtopia.exe+00753168,aa8,198,188", "int", "0"); //dbljump
                LegendBot.BackColor = Color.Black;
            }
        }

        private void ModSpawn_MouseDown(object sender, MouseEventArgs e)
        {
            if (ModSpawn.BackColor == Color.Black)
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+61B50C", "bytes", "90 90");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "73 05");
                ModSpawn.BackColor = Color.Lime;
            }
            else
            {
                FocusText.Focus();
                mem.WriteMemory("Growtopia.exe+61B50C", "bytes", "70 6F");
                mem.WriteMemory("Growtopia.exe+400FD2", "bytes", "74 05");
                ModSpawn.BackColor = Color.Black;
            }
        }

        private void label32_MouseDown(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start("https://windscribe.com/");
        }

        private void Page4_MouseDown(object sender, MouseEventArgs e)
        {
            if (HackerModeActivate.BackColor == Color.Black)
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Hide();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = false;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            else
            {
                HackerModePictureBox.Hide();
                HackerModePictureBoxAbout.Hide();
                HackerModePictureBoxCheatPage1.Hide();
                HackerModePictureBoxCheatPage2.Hide();
                HackerModePictureBoxCheatPage3.Hide();
                HackerModePictureBoxCheatPage4.Show();
                HackerModePictureBoxVisuals.Hide();
                HackerModePictureBoxChangers.Hide();
                HackerModePictureBoxSettings.Hide();
                HackerModePictureBoxUnbanner.Hide();
                HackerModePictureBox.Enabled = false;
                HackerModePictureBoxAbout.Enabled = false;
                HackerModePictureBoxCheatPage1.Enabled = false;
                HackerModePictureBoxCheatPage2.Enabled = false;
                HackerModePictureBoxCheatPage3.Enabled = false;
                HackerModePictureBoxCheatPage4.Enabled = true;
                HackerModePictureBoxVisuals.Enabled = false;
                HackerModePictureBoxChangers.Enabled = false;
                HackerModePictureBoxSettings.Enabled = false;
                HackerModePictureBoxUnbanner.Enabled = false;
                HackerModePictureBoxSpammer.Hide();
                HackerModePictureBoxSpammer.Enabled = false;
            }
            CheatPage4.BringToFront();
            CheatPage1.Hide();
            CheatPage2.Hide();
            CheatPage3.Hide();
            CheatPage4.Show();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            label9.ForeColor = Color.FromArgb(r, g, b);
            HaveFun.ForeColor = Color.FromArgb(r, g, b);
            if (r > 0 && b == 0)
            {
                r--;
                g++;
            }
            if (g > 0 && r == 0)
            {
                g--;
                b++;
            }
            if (b > 0 && g == 0)
            {
                b--;
                r++;
            }
        }
    }
}