using Cyotek.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Arduino_Code_Sender {

    public partial class Form1 : Form {
        private Label port = new Label {
            Location = new Point(0, 0),
            AutoSize = true,
            Font = new Font("Arial", 14),
            Text = "",
            ForeColor = Color.White,
        };
        private ComboBox command = new ComboBox {
            Font = new Font("Arial", 14),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(80, 20),
        };
        private Label state = new Label {
            Text = "State",
            AutoSize = true,
            Font = new Font("Arial", 14),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };
        private TextBox led = new TextBox {
            Size = new Size(40, 20),
            Font = new Font("Arial", 14),
            AutoCompleteMode = AutoCompleteMode.Suggest,
            AutoCompleteSource = AutoCompleteSource.CustomSource,
            Anchor = AnchorStyles.None,
            Margin = new Padding(110, 0, 110, 0)
        };
        private TextBox endLed = new TextBox {
            Size = new Size(40, 20),
            Font = new Font("Arial", 14),
            AutoCompleteMode = AutoCompleteMode.Suggest,
            AutoCompleteSource = AutoCompleteSource.CustomSource,
            Anchor = AnchorStyles.None,
            Margin = new Padding(110, 0, 110, 0)
        };
        private Button send = new Button {
            Text = "Send",
            AutoSize = true,
            Font = new Font("Arial", 14),
            BackColor = Color.WhiteSmoke,
            Anchor = AnchorStyles.None
        };
        private Button find = new Button {
            Text = "Port",
            AutoSize = true,
            Font = new Font("Arial", 14),
            BackColor = Color.WhiteSmoke,
            Anchor = AnchorStyles.None
        };
        private Label ledt = new Label {
            Text = "Led N.",
            AutoSize = true,
            Font = new Font("Arial", 14),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };
        private Label lastLedt = new Label {
            Text = "Last led N.",
            AutoSize = true,
            Font = new Font("Arial", 14),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };
        ColorPickerDialog colorPicker = new ColorPickerDialog();
        SerialPort cPort;
        bool portFound;
        String PortName;

        public Form1() {
            InitializeComponent();
            colorPicker.ShowAlphaChannel = false;
            colorPicker.BackColor = Color.FromArgb(23, 23, 23);

            send.Click += Send_Click;
            find.Click += Find_Click;
        }

        private void Find_Click(object sender, EventArgs e) {
            SetComPort();
            return;
        }

        private void Send_Click(object sender, EventArgs e) {
            if (cPort == null) return;
            if (command.SelectedItem == null) command.SelectedIndex = 1;

            send.Enabled = false;
            int x;
            byte red;
            byte green;
            byte blue;
            if (command.SelectedItem.ToString() == "On") {
                x = 1;
                colorPicker.ShowDialog();
                red = colorPicker.Color.R;
                green = colorPicker.Color.G;
                blue = colorPicker.Color.B;
            }
            else {
                x = 2;
                red = 0;
                green = 0;
                blue = 0;
            }
            if (led.Text == "" || led.Text == null) led.Text = "0";
            if (endLed.Text == "" || endLed.Text == null) led.Text = "0";
            Sender(x, Convert.ToInt16(led.Text), Convert.ToInt16(endLed.Text), red, green, blue);
        }

        void SetComPort() {
            try {
                string[] ports = SerialPort.GetPortNames();
                foreach (string name in ports) {
                    cPort = new SerialPort(name, 9600);
                    if (PortFound()) {
                        portFound = true;
                        port.Text = PortName;
                        break;
                    }
                    else {
                        portFound = false;
                    }
                }
            }
            catch (Exception) {

            }
        }
        void Sender(int com, int start, int end, byte red, byte green, byte blue, int stop = 1) {
            byte[] buffer = new byte[7];
            buffer[0] = Convert.ToByte(com); //3 = test, 1 = ledit päälle, 2 = ledit pois
            buffer[1] = Convert.ToByte(start); //for led num
            buffer[2] = Convert.ToByte(end); //for end led num
            buffer[3] = red; //red
            buffer[4] = green; //green
            buffer[5] = blue; //blue
            buffer[6] = Convert.ToByte(stop); //pääte

            cPort.Open();
            cPort.Write(buffer, 0, 7);
            Thread.Sleep(1000);
            send.Enabled = true;
            cPort.Close();
        }

        bool PortFound() {
            try {
                byte[] buffer = new byte[7];
                buffer[0] = Convert.ToByte(3); //3 = test, 1 = ledit päälle, 2 = ledit pois
                buffer[1] = Convert.ToByte(1); //for led num
                buffer[2] = Convert.ToByte(1); //for end led num
                buffer[3] = Convert.ToByte(1); //red
                buffer[4] = Convert.ToByte(1); //green
                buffer[5] = Convert.ToByte(1); //blue
                buffer[6] = Convert.ToByte(1); //pääte

                int retVal = 0;
                char charRetVal = (Char)retVal;

                cPort.Open();
                cPort.Write(buffer, 0, 7);
                Thread.Sleep(1000);

                int count = cPort.BytesToRead;
                string retMsg = "";


                while (count > 0) {
                    retVal = cPort.ReadByte();
                    retMsg = retMsg + Convert.ToChar(retVal);
                    count--;
                }
                PortName = cPort.PortName;
                cPort.Close();
                if (retMsg.Contains("MOI")) {
                    return true;
                }
                else return false;
            }
            catch (Exception) {
                return false;
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            //Window settings ------------------------------------

            Size = new Size(800, 600);
            BackColor = Color.FromArgb(23, 23, 23);
            FormBorderStyle = FormBorderStyle.Fixed3D;

            //Variables ------------------------------------------

            int h = Height;
            int w = Width;
            int lb = 150;

            //Control settings -----------------------------------

            led.Location = new Point(lb, (h / 2) - led.Height / 2);
            endLed.Location = new Point(led.Left + lb, (h / 2) - endLed.Height / 2);
            command.Items.Add("On");
            command.Items.Add("Off");
            command.Location = new Point(endLed.Left + lb, (h / 2) - endLed.Height / 2);
            send.Location = new Point(w / 2 - send.Width / 2, (h / 2) + 40);
            ledt.Location = new Point(led.Left - 10, h / 2 - 40);
            find.Location = new Point(w / 2 - send.Width / 2, (h / 2) + 80);
            state.Location = new Point(command.Left, (h / 2) - 40);
            lastLedt.Location = new Point(endLed.Left - lastLedt.Width / 4, h / 2 - 40);

            //Control adding -------------------------------------

            Controls.Add(send);
            Controls.Add(led);
            Controls.Add(endLed);
            Controls.Add(ledt);
            Controls.Add(port);
            Controls.Add(find);
            Controls.Add(command);
            Controls.Add(state);
            Controls.Add(lastLedt);

        }
    }
}
