using Guna.UI2.WinForms;
using KrishuXmem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace Krishu_X_External
{
    public partial class Main : Form
    {
        private Point PanelLocation = new Point(5, 59);
        private MemoryManager m = new MemoryManager();
        private bool isAimbotActivated = false;
        private bool _isUpdatingCheckbox = false; // Prevent recursive calls

        Krishuu KrishuXmem = new Krishuu();

        public class PatchInfo
        {
            public string Search { get; set; }
            public string Replace { get; set; }
            public List<long> Addresses { get; set; } = new List<long>();
            public bool IsEnabled { get; set; } = false;
        }

        private Dictionary<string, PatchInfo> _patches = new Dictionary<string, PatchInfo>();

        // Keybind tracking
        private Dictionary<Keys, Action> _keybindActions = new Dictionary<Keys, Action>();
        private Dictionary<string, Keys> _featureKeybinds = new Dictionary<string, Keys>();
        private bool _isListeningForKey = false;
        private string _currentListeningFeature = null;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        const uint PROCESS_CREATE_THREAD = 0x2;
        const uint PROCESS_QUERY_INFORMATION = 0x400;
        const uint PROCESS_VM_OPERATION = 0x8;
        const uint PROCESS_VM_WRITE = 0x20;
        const uint PROCESS_VM_READ = 0x10;
        const uint MEM_COMMIT = 0x1000;
        const uint PAGE_READWRITE = 4;

        public Main()
        {
            InitializeComponent();
            LoadAllPatches();
        }

        // ==================== LOAD ALL PATCHES ON STARTUP ====================
        private async void LoadAllPatches()
        {
            if (Process.GetProcessesByName("HD-Player").Length == 0)
            {
                sta.Text = "Open Emulator First";
                sta.ForeColor = Color.Red;
                return;
            }

            m.SetProcess(new string[] { "HD-Player" });
            sta.Text = "Loading Patches...";
            sta.ForeColor = Color.Orange;

            await LoadPatch("GlitchFire", "C0 41 00 00 10 C1 00 00 90 C1 00 00 70 41 01 00 00 00 00 00 C0 3F 00 00 00 3F 00 00 80 3F 00 00 80 3F", "C0 41 00 00 10 C1 00 00 90 C1 00 00 70 41 01 00 00 00 00 00 C0 00 00 00 00 3C 00 00 80 3F 00 00 80 3F");
            await LoadPatch("FastFire_0", "00 00 80 40 33 33 93 40 66 66 06 40 01 01 00", "00 00 80 40 33 33 93 40 8F 0A F7 3D 01 01 00");
            await LoadPatch("FastFire_1", "02 2B 07 3D 02 2B 07 3D 02 2B 07 3D 00 00 00 00 9B 6C F2 41 49 00 00 00", "02 2B 07 3B 02 2B 07 3B 02 2B 07 3D 00 00 00 00 9B 6C F2 41 49 00 00 00");
            await LoadPatch("SniperSwitch", "3F 00 00 80 3E 00 00 00 00 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F", "01 00 00 80 00 00 00 00 00 04 00 00 00 00 00 80 3F 00 00 20 41 00 00 34 42 01 00 00 00 01");
            await LoadPatch("SniperScope", "03 00 01 00 00 00 9A 99 99 3E FF FF FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 33 33 13 40 00 00 B0 3F 00 00 80 3F 01", "03 00 01 00 00 00 9A 99 99 3E FF FF FF FF 08 00 00 00 00 00 60 40 CD CC 8C 3F 8F C2 F5 3C CD CC CC 3D 06 00 00 00 00 00 FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 33 33 13 40 00 00 B0 3F 00 00 80 3F 01");
            await LoadPatch("SniperAim", "41 00 00 00 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 00 00 80 3F 00 00 00 00 00 00 80 3F", "41 00 00 00 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 80 3F");
            await LoadPatch("FastLanding", "00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF", "00 00 00 00 00 00 FF 41 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF");
            await LoadPatch("VisionHack", "00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF", "00 00 80 3F");
            await LoadPatch("CameraLeft", "00 00 00 00 00 00 80 40 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 80 BF", "00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00");
            await LoadPatch("CameraRight", "00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF", "00 00 00 00 00 80 40 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 80 BF 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 80 BF 00 00 80 7F 00 00 80 7F 00 00 80 7F 00 00 80 FF");
            await LoadPatch("NoRecoil", "30 48 2D E9 02 8B 2D ED FC 50 9F E5 00 40 A0 E1 05 50 8F E0 00 00 D5 E5 00 00 50 E3 04 00 00 1A E8 00 9F E5 00 00 9F E7 EC", "30 48 2D E3 1E 8B 2D E1");
            await LoadPatch("SpeedHack", "02 2B 07 3D 02 2B 07 3D 02 2B 07 3D", "E3 A5 9B 3C");
            await LoadPatch("FlyHack", "AC C5 27 37 00 10 A0 E1", "AC C5 A9 3F 00 10 A0 E1");
            await LoadPatch("HighJump", "00 00 C8 42 F0 48 2D E9 10 B0 8D E2 01 50 A0 E1 00 40 A0 E1 00 00 55 E3 02 00 00 0A BC 70 D5 E1", "00 00 7A 43 F0 48 2D E9 10 B0 8D E2 01 50 A0 E1 00 40 A0 E1 00 00 55 E3 02 00 00 0A BC 70 D5 E1");
            await LoadPatch("GuestReset", "10 40 2D E9 D0 40 9F E5 04 40 8F E0 00 00 D4 E5 00 00 50 E3 04 00 00 1A C0 00 9F E5 00 00 9F E7 A9", "01 00 A0 E3 1E FF 2F E1");

            sta.Text = "Hacks Loaded!";
            sta.ForeColor = Color.Green;
        }

        private async Task LoadPatch(string patchName, string search, string replace)
        {
            try
            {
                if (!_patches.ContainsKey(patchName))
                {
                    var addresses = await m.AoBScan(search);
                    if (addresses.Any())
                    {
                        _patches[patchName] = new PatchInfo
                        {
                            Search = search,
                            Replace = replace,
                            Addresses = addresses.ToList(),
                            IsEnabled = false
                        };
                    }
                }
            }
            catch { }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_isListeningForKey)
            {
                if (keyData == Keys.ControlKey || keyData == Keys.ShiftKey || keyData == Keys.Menu)
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }

                if (_currentListeningFeature != null)
                {
                    if (_featureKeybinds.ContainsValue(keyData))
                    {
                        var oldFeature = _featureKeybinds.First(kvp => kvp.Value == keyData).Key;
                        _keybindActions.Remove(keyData);
                        _featureKeybinds.Remove(oldFeature);
                    }

                    Action action = null;
                    switch (_currentListeningFeature)
                    {
                        case "GlitchFire":
                            action = () => ToggleFeature(guna2CustomCheckBox2, "GlitchFire", "Glitch Fire");
                            break;
                        case "FastFire":
                            action = () => ToggleFeatureFastFire();
                            break;
                        case "SniperSwitch":
                            action = () => ToggleFeature(guna2CustomCheckBox4, "SniperSwitch", "Sniper Switch");
                            break;
                        case "SniperScope":
                            action = () => ToggleFeature(guna2CustomCheckBox5, "SniperScope", "Sniper Scope");
                            break;
                        case "SniperAim":
                            action = () => ToggleFeature(guna2CustomCheckBox6, "SniperAim", "Sniper Aim");
                            break;
                        case "FastLanding":
                            action = () => ToggleFeature(guna2CustomCheckBox7, "FastLanding", "Fast Landing");
                            break;
                        case "VisionHack":
                            action = () => ToggleFeature(guna2CustomCheckBox8, "VisionHack", "Vision Hack");
                            break;
                        case "Camera":
                            action = () => ToggleFeature(guna2CustomCheckBox17, "Camera", "Camera");
                            break;
                        case "NoRecoil":
                            action = () => ToggleFeature(guna2CustomCheckBox19, "NoRecoil", "No Recoil");
                            break;
                        case "SpeedHack":
                            action = () => ToggleFeature(guna2CustomCheckBox21, "SpeedHack", "Speed Hack");
                            break;
                        case "FlyHack":
                            action = () => ToggleFeature(guna2CustomCheckBox23, "FlyHack", "Fly Hack");
                            break;
                        case "HighJump":
                            action = () => ToggleFeature(guna2CustomCheckBox24, "HighJump", "High Jump");
                            break;
                        case "GuestReset":
                            action = () => ToggleFeature(guna2CustomCheckBox22, "GuestReset", "Guest Reset");
                            break;
                    }

                    if (action != null)
                    {
                        _featureKeybinds[_currentListeningFeature] = keyData;
                        _keybindActions[keyData] = action;
                        sta.Text = $"Keybind set: {keyData} for {_currentListeningFeature}";
                        sta.ForeColor = Color.Green;
                    }
                }

                _isListeningForKey = false;
                _currentListeningFeature = null;
                return true;
            }

            if (_keybindActions.ContainsKey(keyData))
            {
                _keybindActions[keyData].Invoke();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ==================== TOGGLE FEATURE METHODS ====================

        private async void ToggleFeature(Guna2CustomCheckBox checkbox, string patchName, string displayName)
        {
            if (_isUpdatingCheckbox) return;
            _isUpdatingCheckbox = true;

            try
            {
                if (!_patches.ContainsKey(patchName))
                {
                    sta.Text = $"Patch {displayName} not loaded!";
                    sta.ForeColor = Color.Red;
                    return;
                }

                // Toggle the checkbox state
                checkbox.Checked = !checkbox.Checked;
                bool enable = checkbox.Checked;
                var patch = _patches[patchName];

                if (Process.GetProcessesByName("HD-Player").Length == 0)
                {
                    sta.Text = "Open Emulator First!";
                    sta.ForeColor = Color.Red;
                    checkbox.Checked = !enable;
                    return;
                }

                m.SetProcess(new string[] { "HD-Player" });
                string bytesToWrite = enable ? patch.Replace : patch.Search;

                foreach (var address in patch.Addresses)
                {
                    if (bytesToWrite.Contains(" "))
                        m.AobReplace(address, bytesToWrite);
                    else
                    {
                        try { m.AobReplace(address, Convert.ToInt32(bytesToWrite)); }
                        catch { try { m.AobReplace(address, Convert.ToInt32(bytesToWrite, 16)); } catch { m.AobReplace(address, bytesToWrite); } }
                    }
                }

                patch.IsEnabled = enable;
                sta.Text = enable ? $"{displayName} Activated" : $"{displayName} Deactivated";
                sta.ForeColor = enable ? Color.Green : Color.White;
                Console.Beep(enable ? 400 : 300, 300);
            }
            finally
            {
                _isUpdatingCheckbox = false;
            }
        }

        private async void ToggleFeatureFastFire()
        {
            if (_isUpdatingCheckbox) return;
            _isUpdatingCheckbox = true;

            try
            {
                if (Process.GetProcessesByName("HD-Player").Length == 0)
                {
                    sta.Text = "Open Emulator First!";
                    sta.ForeColor = Color.Red;
                    return;
                }

                m.SetProcess(new string[] { "HD-Player" });
                bool enable = !guna2CustomCheckBox3.Checked;
                guna2CustomCheckBox3.Checked = enable;

                bool allSuccess = true;
                for (int i = 0; i < 2; i++)
                {
                    string patchName = $"FastFire_{i}";
                    if (!_patches.ContainsKey(patchName))
                    {
                        allSuccess = false;
                        continue;
                    }

                    var patch = _patches[patchName];
                    string bytesToWrite = enable ? patch.Replace : patch.Search;

                    foreach (var address in patch.Addresses)
                    {
                        m.AobReplace(address, bytesToWrite);
                    }
                    patch.IsEnabled = enable;
                }

                if (allSuccess)
                {
                    sta.Text = enable ? "Fast Fire Activated" : "Fast Fire Deactivated";
                    sta.ForeColor = enable ? Color.Green : Color.White;
                    Console.Beep(enable ? 400 : 300, 300);
                }
            }
            finally
            {
                _isUpdatingCheckbox = false;
            }
        }

        // ==================== KEYBIND BUTTON EVENTS ====================

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "GlitchFire";
            sta.Text = "Press a key for GlitchFire...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "FastFire";
            sta.Text = "Press a key for FastFire...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "SniperSwitch";
            sta.Text = "Press a key for SniperSwitch...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "SniperScope";
            sta.Text = "Press a key for SniperScope...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "SniperAim";
            sta.Text = "Press a key for SniperAim...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "FastLanding";
            sta.Text = "Press a key for FastLanding...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "VisionHack";
            sta.Text = "Press a key for VisionHack...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button21_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "Camera";
            sta.Text = "Press a key for Camera...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button27_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "NoRecoil";
            sta.Text = "Press a key for NoRecoil...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button25_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "SpeedHack";
            sta.Text = "Press a key for SpeedHack...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button24_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "FlyHack";
            sta.Text = "Press a key for FlyHack...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button23_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "HighJump";
            sta.Text = "Press a key for HighJump...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button22_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "GuestReset";
            sta.Text = "Press a key for GuestReset...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button28_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "FastReload";
            sta.Text = "Press a key for FastReload...";
            sta.ForeColor = Color.Orange;
        }

        private void guna2Button26_Click(object sender, EventArgs e)
        {
            _isListeningForKey = true;
            _currentListeningFeature = "UnlimitedAmmo";
            sta.Text = "Press a key for UnlimitedAmmo...";
            sta.ForeColor = Color.Orange;
        }

        // ==================== GUI EVENTS ====================

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isAimbotActivated)
            {
                isAimbotActivated = false;
                guna2CustomCheckBox1.Checked = false;
                sta.Text = "N/A";
                sta.ForeColor = Color.White;
            }
        }

        private void Hidepanel()
        {
            Aimpanel.Visible = false;
            Esppanel.Visible = false;
            Brutalpanel.Visible = false;
            Extrapanel.Visible = false;
        }

        private void ButtonColour()
        {
            Aimbutton.FillColor = Color.Gray;
            Espbutton.FillColor = Color.Gray;
            Brutalbutton.FillColor = Color.Gray;
            Extrabutton.FillColor = Color.Gray;
        }

        private void Aimbutton_Click(object sender, EventArgs e)
        {
            Hidepanel();
            ButtonColour();
            Aimpanel.Visible = true;
            Aimpanel.BringToFront();
            Aimbutton.FillColor = Color.Red;
            Aimpanel.Location = PanelLocation;
        }

        private void Espbutton_Click(object sender, EventArgs e)
        {
            Hidepanel();
            ButtonColour();
            Esppanel.Visible = true;
            Esppanel.BringToFront();
            Espbutton.FillColor = Color.Red;
            Esppanel.Location = PanelLocation;
        }

        private void Brutalbutton_Click(object sender, EventArgs e)
        {
            Hidepanel();
            ButtonColour();
            Brutalpanel.Visible = true;
            Brutalpanel.BringToFront();
            Brutalbutton.FillColor = Color.Red;
            Brutalpanel.Location = PanelLocation;
        }

        private void Extrabutton_Click(object sender, EventArgs e)
        {
            Hidepanel();
            ButtonColour();
            Extrapanel.Visible = true;
            Extrapanel.BringToFront();
            Extrabutton.FillColor = Color.Red;
            Extrapanel.Location = PanelLocation;
        }

        // ==================== AIMBOT METHODS ====================

        private async Task ActivateAimbot(string mode)
        {
            if (Process.GetProcessesByName("HD-Player").Length == 0)
            {
                sta.Text = "OPEN YOUR EMULATOR";
                sta.ForeColor = Color.Red;
                Console.Beep(2000, 400);
                return;
            }

            m.SetProcess(new string[] { "HD-Player" });
            sta.Text = "Activating-Aimbot";
            sta.ForeColor = Color.Orange;

            string pattern = "";
            long offset1 = 0;
            long offset2 = 0;

            if (mode == "Aimbot")
            {
                pattern = "FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";
                offset1 = 0xB8;
                offset2 = 0xB4;
            }
            else if (mode == "Aimbot AI")
            {
                pattern = "FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A5 43";
                offset1 = 0xB8;
                offset2 = 0xB4;
            }
            else if (mode == "Aimbot Safe")
            {
                pattern = "FF FF FF FF 00 00 00 00 E0 B9 0C B2 00 00 00 00 00 00 00 00 1F 31 43 F2 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 02 02 00 00 00 02 02 00 00 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 1F 31 43 F2 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 09 00 00 09 09 00 00 00 09 09 00 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 A0 BF 43 BA A0 BF 43 BA 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 C0 E8 45 BA 58 37 54 AF 58 37 54 AF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E0 99 C1 84 01 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 58 9E 4E 94 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 04 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 80 3F";
                offset1 = 0x128;
                offset2 = 0x214;
            }

            var result = await m.AoBScan(pattern);
            if (result.Any())
            {
                foreach (var CurrentAddress in result)
                {
                    Int64 dopa = CurrentAddress + offset1;
                    Int64 dopaxd = CurrentAddress + offset2;
                    var Read = m.ReadInt(dopa);
                    m.AobReplace(dopaxd, Read);
                }

                sta.Text = $"Aimbot-{mode}-Activated";
                sta.ForeColor = Color.Green;
                Console.Beep(500, 300);
                isAimbotActivated = true;
            }
            else
            {
                sta.Text = "Failed";
                sta.ForeColor = Color.Red;
                Console.Beep(2000, 400);
                isAimbotActivated = false;
                guna2CustomCheckBox1.Checked = false;
            }
        }

        private async Task DeactivateAimbot()
        {
            sta.Text = "Aimbot-Deactivated";
            sta.ForeColor = Color.White;
            isAimbotActivated = false;
            Console.Beep(300, 200);
        }

        private async void guna2CustomCheckBox1_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox1.Checked)
            {
                string selectedMode = comboBox1.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedMode))
                {
                    sta.Text = "Select Aimbot Mode First";
                    sta.ForeColor = Color.Orange;
                    guna2CustomCheckBox1.Checked = false;
                    return;
                }
                await ActivateAimbot(selectedMode);
            }
            else
            {
                await DeactivateAimbot();
            }
        }

        // ==================== CHECKBOX EVENTS ====================
        // Sab checkbox events yahan hain - manually click karne par chalenge

        private async void guna2CustomCheckBox2_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox2, "GlitchFire", "Glitch Fire");
        }

        private async void guna2CustomCheckBox3_Click(object sender, EventArgs e)
        {
            ToggleFeatureFastFire();
        }

        private async void guna2CustomCheckBox4_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox4, "SniperSwitch", "Sniper Switch");
        }

        private async void guna2CustomCheckBox5_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox5, "SniperScope", "Sniper Scope");
        }

        private async void guna2CustomCheckBox6_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox6, "SniperAim", "Sniper Aim");
        }

        private async void guna2CustomCheckBox7_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox7, "FastLanding", "Fast Landing");
        }

        private async void guna2CustomCheckBox8_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox8, "VisionHack", "Vision Hack");
        }

        private async void guna2CustomCheckBox17_Click(object sender, EventArgs e)
        {
            string selectedSide = comboBox3.SelectedItem?.ToString() ?? "Left";
            string patchName = selectedSide == "Left" ? "CameraLeft" : "CameraRight";
            ToggleFeature(guna2CustomCheckBox17, patchName, $"Camera {selectedSide}");
        }

        private async void guna2CustomCheckBox19_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox19, "NoRecoil", "No Recoil");
        }

        private async void guna2CustomCheckBox21_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox21, "SpeedHack", "Speed Hack");
        }

        private async void guna2CustomCheckBox23_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox23, "FlyHack", "Fly Hack");
        }

        private async void guna2CustomCheckBox24_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox24, "HighJump", "High Jump");
        }

        // Guest Reset
        private async void Guestresetchekbox_Click(object sender, EventArgs e)
        {
            ToggleFeature(guna2CustomCheckBox22, "GuestReset", "Guest Reset");
        }

        // Wall Hack - placeholder
        private async void guna2CustomCheckBox22_Click(object sender, EventArgs e)
        {
            sta.Text = "Wall Hack - Need Values";
            sta.ForeColor = Color.Red;
        }

        // Fast Reload - placeholder
        private async void guna2CustomCheckBox18_Click(object sender, EventArgs e)
        {
            sta.Text = "Fast Reload - Need Values";
            sta.ForeColor = Color.Red;
        }

        // Unlimited Ammo - placeholder
        private async void guna2CustomCheckBox20_Click(object sender, EventArgs e)
        {
            sta.Text = "Unlimited Ammo - Need Values";
            sta.ForeColor = Color.Red;
        }

        // ==================== ESP FEATURES ====================

        private void guna2CustomCheckBox30_Click(object sender, EventArgs e)
        {
            string processName = "HD-Player";
            string dllResourceName = "Krishu_X_External.Krishu Charms.dll";
            string tempDllPath = Path.Combine(Path.GetTempPath(), "Krishu Charms.dll");
            ExtractEmbeddedResource(dllResourceName, tempDllPath);

            Process[] targetProcesses = Process.GetProcessesByName(processName);
            if (targetProcesses.Length == 0)
            {
                sta.Text = "Emulator not found!";
                sta.ForeColor = Color.Red;
                return;
            }

            Process targetProcess = targetProcesses[0];
            IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);
            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr allocMemAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (IntPtr)tempDllPath.Length, MEM_COMMIT, PAGE_READWRITE);
            IntPtr bytesWritten;
            WriteProcessMemory(hProcess, allocMemAddress, System.Text.Encoding.ASCII.GetBytes(tempDllPath), (uint)tempDllPath.Length, out bytesWritten);
            CreateRemoteThread(hProcess, IntPtr.Zero, IntPtr.Zero, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
            sta.Text = "Injected";
            sta.ForeColor = Color.Green;
        }

        private void ExtractEmbeddedResource(string resourceName, string outputPath)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            using (Stream resourceStream = executingAssembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    throw new ArgumentException($"Resource '{resourceName}' not found.");

                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    byte[] buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        // Connect ADB
        private void guna2CustomCheckBox9_Click(object sender, EventArgs e)
        {
            sta.Text = "ADB Connected";
            sta.ForeColor = Color.Green;
        }

        // ESP Features (placeholders)
        private void guna2CustomCheckBox10_Click(object sender, EventArgs e) { sta.Text = "ESP LINE"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox11_Click(object sender, EventArgs e) { sta.Text = "ESP BOX"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox13_Click(object sender, EventArgs e) { sta.Text = "ESP FILL BOX"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox15_Click(object sender, EventArgs e) { sta.Text = "ESP SKELETON"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox16_Click(object sender, EventArgs e) { sta.Text = "ESP NAME"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox14_Click(object sender, EventArgs e) { sta.Text = "ESP Health"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox29_Click(object sender, EventArgs e) { sta.Text = "ESP UP LINE"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox28_Click(object sender, EventArgs e) { sta.Text = "ESP 3D BOX"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox27_Click(object sender, EventArgs e) { sta.Text = "ESP RGB"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox25_Click(object sender, EventArgs e) { sta.Text = "ESP Cursor"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox12_Click(object sender, EventArgs e) { sta.Text = "ESP MINI MAP"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox26_Click(object sender, EventArgs e) { sta.Text = "ESP Moco"; sta.ForeColor = Color.Green; }
        private void guna2CustomCheckBox33_Click(object sender, EventArgs e) { sta.Text = "Stremer Mode"; sta.ForeColor = Color.Green; }

        // ==================== EXTRA FEATURES ====================

        static void ExecuteCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }

        static void DeleteFirewallRule(string programPath)
        {
            ExecuteCommand("netsh advfirewall firewall delete rule name=all program=\"" + programPath + "\"");
        }

        private void guna2CustomCheckBox31_Click(object sender, EventArgs e)
        {
            if (guna2CustomCheckBox31.Checked)
            {
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock0\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock1\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock2\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\Bluestacks.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=in action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");
                ExecuteCommand("netsh advfirewall firewall add rule name=\"TemporaryBlock3\" dir=out action=block profile=any program=\"C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe\"");
                sta.Text = "Internet Blocked";
                sta.ForeColor = Color.Green;
            }
            else
            {
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_nxt\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi5\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks\\HD-Player.exe");
                DeleteFirewallRule("C:\\Program Files\\BlueStacks_msi2\\HD-Player.exe");
                sta.Text = "Internet Unblocked";
                sta.ForeColor = Color.White;
            }
        }

        ColorDialog cd = new ColorDialog();

        private void label39_Click(object sender, EventArgs e)
        {
            if (cd.ShowDialog() == DialogResult.OK)
            {
                guna2Panel1.BorderColor = cd.Color;
                guna2Panel2.BorderColor = cd.Color;
                label39.ForeColor = cd.Color;
                label2.ForeColor = cd.Color;
            }
        }

        private void label16_Click(object sender, EventArgs e) { }

        // ==================== MEMORY MANAGER CLASS ====================

        public class MemoryManager
        {
            private IntPtr _processHandle;
            private int processId;

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesRead);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, IntPtr lpNumberOfBytesWritten);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

            private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
            private const uint MEM_COMMIT = 0x1000;
            private const uint MEM_PRIVATE = 0x20000;
            private const uint PAGE_READWRITE = 0x04;

            [StructLayout(LayoutKind.Sequential)]
            private struct MEMORY_BASIC_INFORMATION
            {
                public IntPtr BaseAddress;
                public IntPtr AllocationBase;
                public uint AllocationProtect;
                public UIntPtr RegionSize;
                public uint State;
                public uint Protect;
                public uint Type;
            }

            public void SetProcess(string[] processNames)
            {
                processId = 0;
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    string processName = process.ProcessName;
                    if (Array.Exists(processNames, name => name.Equals(processName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        processId = process.Id;
                        break;
                    }
                }

                if (processId <= 0)
                    return;

                _processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            }

            public async Task<List<long>> AoBScan(string pattern)
            {
                List<long> addressRet = new List<long>();
                await Task.Run(() =>
                {
                    List<MemoryPage> list = new List<MemoryPage>();
                    IntPtr address = IntPtr.Zero;

                    while (VirtualQueryEx(_processHandle, address, out MEMORY_BASIC_INFORMATION mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) == Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)))
                    {
                        if (mbi.State == MEM_COMMIT && mbi.Type == MEM_PRIVATE && mbi.Protect == PAGE_READWRITE)
                        {
                            list.Add(new MemoryPage(address, (int)mbi.RegionSize.ToUInt64()));
                        }
                        address = (IntPtr)((long)mbi.BaseAddress + (long)mbi.RegionSize);
                    }

                    PatternData patternData = GetPatternData(pattern);
                    byte[] patternBytes = patternData.pattern;
                    byte[] maskBytes = patternData.mask;

                    Parallel.ForEach(list, page =>
                    {
                        byte[] buffer = new byte[page.Size];
                        if (ReadProcessMemory(_processHandle, page.Start, buffer, (IntPtr)page.Size, out IntPtr bytesRead))
                        {
                            int index = -patternBytes.Length;
                            do
                            {
                                index = FindPattern(buffer, patternBytes, maskBytes, index + patternBytes.Length);
                                if (index >= 0)
                                {
                                    lock (addressRet)
                                    {
                                        addressRet.Add((long)page.Start + index);
                                    }
                                }
                            } while (index != -1);
                        }
                    });
                });

                return addressRet.OrderBy(c => c).ToList();
            }

            private int FindPattern(byte[] body, byte[] pattern, byte[] masks, int start = 0)
            {
                if (body.Length == 0 || pattern.Length == 0 || start > body.Length - pattern.Length || pattern.Length > body.Length)
                    return -1;

                for (int i = start; i <= body.Length - pattern.Length; i++)
                {
                    if ((body[i] & masks[0]) != (pattern[0] & masks[0]))
                        continue;

                    bool found = true;
                    for (int j = pattern.Length - 1; j >= 1; j--)
                    {
                        if ((body[i + j] & masks[j]) != (pattern[j] & masks[j]))
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        return i;
                }

                return -1;
            }

            private PatternData GetPatternData(string pattern)
            {
                string[] parts = pattern.Split(' ');
                byte[] patternBytes = new byte[parts.Length];
                byte[] maskBytes = new byte[parts.Length];

                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == "??")
                    {
                        patternBytes[i] = 0x00;
                        maskBytes[i] = 0x00;
                    }
                    else
                    {
                        patternBytes[i] = Convert.ToByte(parts[i], 16);
                        maskBytes[i] = 0xFF;
                    }
                }

                return new PatternData { pattern = patternBytes, mask = maskBytes };
            }

            private struct PatternData
            {
                public byte[] pattern;
                public byte[] mask;
            }

            private struct MemoryPage
            {
                public IntPtr Start;
                public int Size;

                public MemoryPage(IntPtr start, int size)
                {
                    Start = start;
                    Size = size;
                }
            }

            public int ReadInt(long address)
            {
                byte[] buffer = new byte[4];
                if (ReadProcessMemory(_processHandle, (IntPtr)address, buffer, (IntPtr)buffer.Length, out IntPtr bytesRead))
                {
                    return BitConverter.ToInt32(buffer, 0);
                }
                return 0;
            }

            public void AobReplace(long address, int value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                WriteProcessMemory(_processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, IntPtr.Zero);
            }

            public void AobReplace(long address, string hexPattern)
            {
                byte[] bytes = HexStringToByteArray(hexPattern);
                WriteProcessMemory(_processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, IntPtr.Zero);
            }

            private byte[] HexStringToByteArray(string hex)
            {
                if (string.IsNullOrEmpty(hex))
                    return new byte[0];

                string[] hexParts = hex.Split(' ');
                byte[] bytes = new byte[hexParts.Length];

                for (int i = 0; i < hexParts.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hexParts[i], 16);
                }

                return bytes;
            }
        }
    }
}
