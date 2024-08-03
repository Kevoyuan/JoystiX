using SharpDX.XInput;
using System;
using System.Runtime.InteropServices;  // Ensure this using statement is present
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace JoystiX
{
    public partial class MainWindow : Window
    {
        private Controller controller;
        private System.Timers.Timer timer;
        private DispatcherTimer uiUpdateTimer;
        private DispatcherTimer controllerCheckTimer;

        // P/Invoke declaration for XInputGetBatteryInformation
        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetBatteryInformation")]
        private static extern uint XInputGetBatteryInformation(uint dwUserIndex, byte devType, ref XINPUT_BATTERY_INFORMATION pBatteryInformation);

        private const byte BATTERY_DEVTYPE_GAMEPAD = 0x00;
        private const byte BATTERY_TYPE_DISCONNECTED = 0x00;
        private const byte BATTERY_TYPE_WIRED = 0x01;
        private const byte BATTERY_TYPE_ALKALINE = 0x02;
        private const byte BATTERY_TYPE_NIMH = 0x03;
        private const byte BATTERY_TYPE_UNKNOWN = 0xFF;

        private const byte BATTERY_LEVEL_EMPTY = 0x00;
        private const byte BATTERY_LEVEL_LOW = 0x01;
        private const byte BATTERY_LEVEL_MEDIUM = 0x02;
        private const byte BATTERY_LEVEL_FULL = 0x03;

        [StructLayout(LayoutKind.Sequential)]
        private struct XINPUT_BATTERY_INFORMATION
        {
            public byte BatteryType;
            public byte BatteryLevel;
        }

        public MainWindow()
        {
            InitializeComponent();

            controller = new Controller(UserIndex.One);

            timer = new System.Timers.Timer(100);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Interval = TimeSpan.FromMilliseconds(50);
            uiUpdateTimer.Tick += UiUpdateTimer_Tick;
            uiUpdateTimer.Start();

            controllerCheckTimer = new DispatcherTimer();
            controllerCheckTimer.Interval = TimeSpan.FromMilliseconds(1000); // Check every second
            controllerCheckTimer.Tick += CheckControllerStatus;
            controllerCheckTimer.Start();
        }

        private void CheckControllerStatus(object sender, EventArgs e)
        {
            if (controller.IsConnected)
            {
                ConnectionStatus.Text = "Controller Status: Connected";
                BatteryStatus.Text = GetBatteryInformation();
            }
            else
            {
                ConnectionStatus.Text = "Controller Status: Not Connected";
                BatteryStatus.Text = "Battery Status: N/A";
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!controller.IsConnected)
            {
                Dispatcher.Invoke(() =>
                {
                    ConnectionStatus.Text = "Controller Status: Not Connected";
                    BatteryStatus.Text = "Battery Status: N/A";
                });
                return;
            }

            var state = controller.GetState();
            var gamepad = state.Gamepad;

            Dispatcher.Invoke(() =>
            {
                LeftStickXLabel.Text = $"X = {gamepad.LeftThumbX / 32767.0:F6}";
                LeftStickYLabel.Text = $"Y = {gamepad.LeftThumbY / 32767.0:F6}";

                RightStickXLabel.Text = $"X = {gamepad.RightThumbX / 32767.0:F6}";
                RightStickYLabel.Text = $"Y = {gamepad.RightThumbY / 32767.0:F6}";

                LeftTriggerBar.Value = gamepad.LeftTrigger / 255.0 * 100;
                RightTriggerBar.Value = gamepad.RightTrigger / 255.0 * 100;

                MoveStickIndicator(LeftStickIndicator, gamepad.LeftThumbX, gamepad.LeftThumbY);
                MoveStickIndicator(RightStickIndicator, gamepad.RightThumbX, gamepad.RightThumbY);

                UpdateButtonState(gamepad);
                BatteryStatus.Text = GetBatteryInformation();
            });
        }

        private void UpdateButtonState(Gamepad gamepad)
        {
            SetButtonState(AButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.A));
            SetButtonState(BButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.B));
            SetButtonState(XButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.X));
            SetButtonState(YButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.Y));
            SetButtonState(DPadUp, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp));
            SetButtonState(DPadDown, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown));
            SetButtonState(DPadLeft, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft));
            SetButtonState(DPadRight, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight));
            SetButtonState(StartButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.Start));
            SetButtonState(BackButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.Back));
            SetButtonState(LBButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder));
            SetButtonState(RBButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder));
        }

        private void SetButtonState(Label button, bool isPressed)
        {
            button.Background = isPressed ? Brushes.Yellow : Brushes.Transparent;
        }

        private void MoveStickIndicator(Ellipse indicator, short x, short y)
        {
            double normalizedX = x / 32767.0 * 50; // Assuming a radius of 50
            double normalizedY = y / 32767.0 * 50; // Assuming a radius of 50

            Canvas.SetLeft(indicator, normalizedX + 45); // Centered in 100x100 canvas
            Canvas.SetTop(indicator, 45 - normalizedY);  // Centered in 100x100 canvas
        }

        private void UiUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Throttled UI update logic if needed
        }

        private void SaveMapping_Click(object sender, RoutedEventArgs e)
        {
            // Save mapping logic
        }

        private string GetBatteryInformation()
        {
            XINPUT_BATTERY_INFORMATION batteryInfo = new XINPUT_BATTERY_INFORMATION();
            uint result = XInputGetBatteryInformation(0, BATTERY_DEVTYPE_GAMEPAD, ref batteryInfo);

            if (result != 0) // Error
            {
                return "Battery Info: Error";
            }

            string batteryType = batteryInfo.BatteryType switch
            {
                BATTERY_TYPE_DISCONNECTED => "Disconnected",
                BATTERY_TYPE_WIRED => "Wired",
                BATTERY_TYPE_ALKALINE => "Alkaline",
                BATTERY_TYPE_NIMH => "NiMH",
                BATTERY_TYPE_UNKNOWN => "Unknown",
                _ => "Unknown"
            };

            string batteryLevel = batteryInfo.BatteryLevel switch
            {
                BATTERY_LEVEL_EMPTY => "Empty",
                BATTERY_LEVEL_LOW => "Low",
                BATTERY_LEVEL_MEDIUM => "Medium",
                BATTERY_LEVEL_FULL => "Full",
                _ => "Unknown"
            };

            return $"Battery Type: {batteryType}, Level: {batteryLevel}";
        }
    }
}
