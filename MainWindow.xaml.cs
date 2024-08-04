using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using SharpDX.XInput;

namespace XboxControllerTester
{
    public partial class MainWindow : Window
    {
        private Controller controller;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            
            controller = new Controller(UserIndex.One);
            
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 fps
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!controller.IsConnected)
            {
                // Update UI to show disconnected state
                return;
            }

            var state = controller.GetState();
            var gamepad = state.Gamepad;

            // Update analog sticks
            UpdateStick(LeftStickIndicator, LeftStickXText, LeftStickYText, gamepad.LeftThumbX, gamepad.LeftThumbY);
            UpdateStick(RightStickIndicator, RightStickXText, RightStickYText, gamepad.RightThumbX, gamepad.RightThumbY);

            // Update triggers
            UpdateTrigger(LeftTriggerBar, LeftTriggerText, gamepad.LeftTrigger);
            UpdateTrigger(RightTriggerBar, RightTriggerText, gamepad.RightTrigger);

            // Update stick text
            LeftStickText.Text = $"Left Stick: {gamepad.LeftThumbX}, {gamepad.LeftThumbY}";
            RightStickText.Text = $"Right Stick: {gamepad.RightThumbX}, {gamepad.RightThumbY}";

            // Update buttons
            UpdateButtonState(AButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.A));
            UpdateButtonState(BButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.B));
            UpdateButtonState(XButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.X));
            UpdateButtonState(YButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.Y));
            UpdateButtonState(LeftShoulderButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder));
            UpdateButtonState(RightShoulderButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder));
            UpdateButtonState(StartButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.Start));
            UpdateButtonState(BackButton, gamepad.Buttons.HasFlag(GamepadButtonFlags.Back));

            // Update D-Pad
            UpdateButtonState(DPadUp, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp));
            UpdateButtonState(DPadDown, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown));
            UpdateButtonState(DPadLeft, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft));
            UpdateButtonState(DPadRight, gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight));

            // Update motor states (you'll need to implement the actual motor control)
            UpdateMotorState("Vibration", gamepad.LeftTrigger > 0 || gamepad.RightTrigger > 0);
            UpdateMotorState("Impulse", gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && (gamepad.LeftTrigger > 0 || gamepad.RightTrigger > 0));
        }

        private void UpdateStick(Ellipse indicator, TextBlock xText, TextBlock yText, short x, short y)
        {
            double normalizedX = x / 32767.0;
            double normalizedY = y / 32767.0;

            Canvas.SetLeft(indicator, 90 + normalizedX * 90);
            Canvas.SetTop(indicator, 90 - normalizedY * 90);

            xText.Text = $"X: {normalizedX:P0}";
            yText.Text = $"Y: {normalizedY:P0}";
        }

        private void UpdateTrigger(Rectangle triggerBar, TextBlock triggerText, byte value)
        {
            double normalizedValue = value / 255.0;
            triggerBar.Height = 200 * normalizedValue;
            triggerBar.VerticalAlignment = VerticalAlignment.Bottom;
            triggerText.Text = $"Trigger: {normalizedValue:P0}";
        }

        private void UpdateButtonState(Shape button, bool isPressed)
        {
            button.Fill = isPressed ? Brushes.Yellow : Brushes.Transparent;
        }

        private void UpdateMotorState(string motorName, bool isOn)
        {
            var textBlock = (TextBlock)FindName(motorName + "MotorText");
            if (textBlock != null)
            {
                textBlock.Text = isOn ? "On" : "Off";
                textBlock.Foreground = isOn ? Brushes.Green : Brushes.Red;
            }
        }
    }
}