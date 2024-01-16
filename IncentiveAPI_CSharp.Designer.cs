namespace IncentiveTest
{
    partial class IncentiveAPI_CSharp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_Start = new System.Windows.Forms.Button();
            this.txt_register = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_Axis = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_Outputs = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_Status = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_fpos = new System.Windows.Forms.TextBox();
            this.progressBar_fpos = new System.Windows.Forms.ProgressBar();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chk_Monitor = new System.Windows.Forms.CheckBox();
            this.btn_Stop = new System.Windows.Forms.Button();
            this.txt_Position = new System.Windows.Forms.TextBox();
            this.txt_MaxSpeed = new System.Windows.Forms.TextBox();
            this.txt_Accel = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.btn_UpdatePosition = new System.Windows.Forms.Button();
            this.chk_EnablePosition = new System.Windows.Forms.CheckBox();
            this.txt_Decel = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.chk_Home = new System.Windows.Forms.CheckBox();
            this.chk_AxisOnly = new System.Windows.Forms.CheckBox();
            this.txt_vel = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.btn_Adjustment = new System.Windows.Forms.Button();
            this.txt_Adjustment = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.btnStopIncentive = new System.Windows.Forms.Button();
            this.btnStartIncentive = new System.Windows.Forms.Button();
            this.btnIndicator = new System.Windows.Forms.Button();
            this.btn_onlineSlave = new System.Windows.Forms.Button();
            this.btn_offlineSlave = new System.Windows.Forms.Button();
            this.txt_Slaves = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.chk_QBDownload = new System.Windows.Forms.CheckBox();
            this.txt_output = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.txt_input = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_Start
            // 
            this.btn_Start.Location = new System.Drawing.Point(26, 309);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(118, 38);
            this.btn_Start.TabIndex = 0;
            this.btn_Start.Text = "Start Test";
            this.btn_Start.UseVisualStyleBackColor = true;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // txt_register
            // 
            this.txt_register.Location = new System.Drawing.Point(123, 66);
            this.txt_register.Name = "txt_register";
            this.txt_register.Size = new System.Drawing.Size(100, 20);
            this.txt_register.TabIndex = 1;
            this.txt_register.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "System Timer";
            // 
            // txt_Axis
            // 
            this.txt_Axis.Location = new System.Drawing.Point(123, 153);
            this.txt_Axis.Name = "txt_Axis";
            this.txt_Axis.Size = new System.Drawing.Size(100, 20);
            this.txt_Axis.TabIndex = 3;
            this.txt_Axis.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(64, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Total Axis";
            // 
            // txt_Outputs
            // 
            this.txt_Outputs.Location = new System.Drawing.Point(123, 179);
            this.txt_Outputs.Name = "txt_Outputs";
            this.txt_Outputs.Size = new System.Drawing.Size(100, 20);
            this.txt_Outputs.TabIndex = 5;
            this.txt_Outputs.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 182);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Total Outputs";
            // 
            // txt_Status
            // 
            this.txt_Status.Location = new System.Drawing.Point(67, 286);
            this.txt_Status.Name = "txt_Status";
            this.txt_Status.Size = new System.Drawing.Size(223, 20);
            this.txt_Status.TabIndex = 7;
            this.txt_Status.Text = "Press Start button to run test...";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 289);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Status";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(53, 208);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Axis #1 fpos";
            // 
            // txt_fpos
            // 
            this.txt_fpos.Location = new System.Drawing.Point(123, 205);
            this.txt_fpos.Name = "txt_fpos";
            this.txt_fpos.Size = new System.Drawing.Size(100, 20);
            this.txt_fpos.TabIndex = 10;
            this.txt_fpos.Text = "0";
            // 
            // progressBar_fpos
            // 
            this.progressBar_fpos.Location = new System.Drawing.Point(26, 261);
            this.progressBar_fpos.Maximum = 90;
            this.progressBar_fpos.Name = "progressBar_fpos";
            this.progressBar_fpos.Size = new System.Drawing.Size(268, 23);
            this.progressBar_fpos.Step = 1;
            this.progressBar_fpos.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(286, 271);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(19, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "30";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(23, 271);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "0";
            // 
            // chk_Monitor
            // 
            this.chk_Monitor.AutoSize = true;
            this.chk_Monitor.Checked = true;
            this.chk_Monitor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_Monitor.Location = new System.Drawing.Point(56, 31);
            this.chk_Monitor.Name = "chk_Monitor";
            this.chk_Monitor.Size = new System.Drawing.Size(85, 17);
            this.chk_Monitor.TabIndex = 15;
            this.chk_Monitor.Text = "Monitor Only";
            this.chk_Monitor.UseVisualStyleBackColor = true;
            // 
            // btn_Stop
            // 
            this.btn_Stop.Enabled = false;
            this.btn_Stop.Location = new System.Drawing.Point(176, 309);
            this.btn_Stop.Name = "btn_Stop";
            this.btn_Stop.Size = new System.Drawing.Size(118, 38);
            this.btn_Stop.TabIndex = 16;
            this.btn_Stop.Text = "Stop Test";
            this.btn_Stop.UseVisualStyleBackColor = true;
            this.btn_Stop.Click += new System.EventHandler(this.btn_Stop_Click);
            // 
            // txt_Position
            // 
            this.txt_Position.Location = new System.Drawing.Point(420, 69);
            this.txt_Position.Name = "txt_Position";
            this.txt_Position.Size = new System.Drawing.Size(100, 20);
            this.txt_Position.TabIndex = 17;
            this.txt_Position.Text = "1000";
            // 
            // txt_MaxSpeed
            // 
            this.txt_MaxSpeed.Location = new System.Drawing.Point(420, 105);
            this.txt_MaxSpeed.Name = "txt_MaxSpeed";
            this.txt_MaxSpeed.Size = new System.Drawing.Size(100, 20);
            this.txt_MaxSpeed.TabIndex = 18;
            this.txt_MaxSpeed.Text = "30";
            // 
            // txt_Accel
            // 
            this.txt_Accel.Location = new System.Drawing.Point(420, 142);
            this.txt_Accel.Name = "txt_Accel";
            this.txt_Accel.Size = new System.Drawing.Size(100, 20);
            this.txt_Accel.TabIndex = 19;
            this.txt_Accel.Text = "500";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(345, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "New Position";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(352, 108);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(61, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "Max Speed";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(380, 145);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 13);
            this.label10.TabIndex = 23;
            this.label10.Text = "Accel";
            // 
            // btn_UpdatePosition
            // 
            this.btn_UpdatePosition.Enabled = false;
            this.btn_UpdatePosition.Location = new System.Drawing.Point(402, 213);
            this.btn_UpdatePosition.Name = "btn_UpdatePosition";
            this.btn_UpdatePosition.Size = new System.Drawing.Size(118, 38);
            this.btn_UpdatePosition.TabIndex = 25;
            this.btn_UpdatePosition.Text = "Update Info";
            this.btn_UpdatePosition.UseVisualStyleBackColor = true;
            this.btn_UpdatePosition.Click += new System.EventHandler(this.btn_UpdatePosition_Click);
            // 
            // chk_EnablePosition
            // 
            this.chk_EnablePosition.AutoSize = true;
            this.chk_EnablePosition.Location = new System.Drawing.Point(351, 31);
            this.chk_EnablePosition.Name = "chk_EnablePosition";
            this.chk_EnablePosition.Size = new System.Drawing.Size(185, 17);
            this.chk_EnablePosition.TabIndex = 26;
            this.chk_EnablePosition.Text = "Enable New Position Test (Axis 1)";
            this.chk_EnablePosition.UseVisualStyleBackColor = true;
            this.chk_EnablePosition.CheckedChanged += new System.EventHandler(this.chk_EnablePosition_CheckedChanged);
            // 
            // txt_Decel
            // 
            this.txt_Decel.Location = new System.Drawing.Point(420, 175);
            this.txt_Decel.Name = "txt_Decel";
            this.txt_Decel.Size = new System.Drawing.Size(100, 20);
            this.txt_Decel.TabIndex = 27;
            this.txt_Decel.Text = "500";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(379, 178);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 13);
            this.label11.TabIndex = 28;
            this.label11.Text = "Decel";
            // 
            // chk_Home
            // 
            this.chk_Home.AutoSize = true;
            this.chk_Home.Location = new System.Drawing.Point(168, 31);
            this.chk_Home.Name = "chk_Home";
            this.chk_Home.Size = new System.Drawing.Size(73, 17);
            this.chk_Home.TabIndex = 29;
            this.chk_Home.Text = "Home first";
            this.chk_Home.UseVisualStyleBackColor = true;
            // 
            // chk_AxisOnly
            // 
            this.chk_AxisOnly.AutoSize = true;
            this.chk_AxisOnly.Location = new System.Drawing.Point(17, 452);
            this.chk_AxisOnly.Name = "chk_AxisOnly";
            this.chk_AxisOnly.Size = new System.Drawing.Size(15, 14);
            this.chk_AxisOnly.TabIndex = 30;
            this.chk_AxisOnly.UseVisualStyleBackColor = true;
            this.chk_AxisOnly.CheckedChanged += new System.EventHandler(this.chk_AxisOnly_CheckedChanged);
            // 
            // txt_vel
            // 
            this.txt_vel.Location = new System.Drawing.Point(123, 235);
            this.txt_vel.Name = "txt_vel";
            this.txt_vel.Size = new System.Drawing.Size(100, 20);
            this.txt_vel.TabIndex = 31;
            this.txt_vel.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(53, 238);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(59, 13);
            this.label12.TabIndex = 32;
            this.label12.Text = "Axis #1 vel";
            // 
            // btn_Adjustment
            // 
            this.btn_Adjustment.Location = new System.Drawing.Point(420, 307);
            this.btn_Adjustment.Name = "btn_Adjustment";
            this.btn_Adjustment.Size = new System.Drawing.Size(83, 29);
            this.btn_Adjustment.TabIndex = 33;
            this.btn_Adjustment.Text = "Update Info";
            this.btn_Adjustment.UseVisualStyleBackColor = true;
            this.btn_Adjustment.Click += new System.EventHandler(this.btn_Adjustment_Click);
            // 
            // txt_Adjustment
            // 
            this.txt_Adjustment.Location = new System.Drawing.Point(420, 271);
            this.txt_Adjustment.Name = "txt_Adjustment";
            this.txt_Adjustment.Size = new System.Drawing.Size(100, 20);
            this.txt_Adjustment.TabIndex = 34;
            this.txt_Adjustment.Text = "20";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(39, 453);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(153, 13);
            this.label13.TabIndex = 35;
            this.label13.Text = "EtherCAT Master Process Only";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(39, 468);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(260, 13);
            this.label14.TabIndex = 36;
            this.label14.Text = "(no 5300 PLC Logic running, not normal test scenario)";
            // 
            // btnStopIncentive
            // 
            this.btnStopIncentive.Enabled = false;
            this.btnStopIncentive.Location = new System.Drawing.Point(472, 430);
            this.btnStopIncentive.Name = "btnStopIncentive";
            this.btnStopIncentive.Size = new System.Drawing.Size(118, 38);
            this.btnStopIncentive.TabIndex = 37;
            this.btnStopIncentive.Text = "Stop Incentive";
            this.btnStopIncentive.UseVisualStyleBackColor = true;
            this.btnStopIncentive.Click += new System.EventHandler(this.btnStopIncentive_Click);
            // 
            // btnStartIncentive
            // 
            this.btnStartIncentive.Enabled = false;
            this.btnStartIncentive.Location = new System.Drawing.Point(348, 429);
            this.btnStartIncentive.Name = "btnStartIncentive";
            this.btnStartIncentive.Size = new System.Drawing.Size(118, 38);
            this.btnStartIncentive.TabIndex = 38;
            this.btnStartIncentive.Text = "Start Incentive";
            this.btnStartIncentive.UseVisualStyleBackColor = true;
            this.btnStartIncentive.Click += new System.EventHandler(this.btnStartIncentive_Click);
            // 
            // btnIndicator
            // 
            this.btnIndicator.BackColor = System.Drawing.Color.Yellow;
            this.btnIndicator.Enabled = false;
            this.btnIndicator.Location = new System.Drawing.Point(402, 385);
            this.btnIndicator.Name = "btnIndicator";
            this.btnIndicator.Size = new System.Drawing.Size(118, 38);
            this.btnIndicator.TabIndex = 39;
            this.btnIndicator.Text = "Unknown";
            this.btnIndicator.UseVisualStyleBackColor = false;
            // 
            // btn_onlineSlave
            // 
            this.btn_onlineSlave.Location = new System.Drawing.Point(223, 384);
            this.btn_onlineSlave.Name = "btn_onlineSlave";
            this.btn_onlineSlave.Size = new System.Drawing.Size(67, 34);
            this.btn_onlineSlave.TabIndex = 40;
            this.btn_onlineSlave.Text = "Online";
            this.btn_onlineSlave.UseVisualStyleBackColor = true;
            this.btn_onlineSlave.Click += new System.EventHandler(this.btn_onlineSlave_Click);
            // 
            // btn_offlineSlave
            // 
            this.btn_offlineSlave.Location = new System.Drawing.Point(26, 384);
            this.btn_offlineSlave.Name = "btn_offlineSlave";
            this.btn_offlineSlave.Size = new System.Drawing.Size(67, 34);
            this.btn_offlineSlave.TabIndex = 41;
            this.btn_offlineSlave.Text = "Offline";
            this.btn_offlineSlave.UseVisualStyleBackColor = true;
            this.btn_offlineSlave.Click += new System.EventHandler(this.btn_offlineSlave_Click);
            // 
            // txt_Slaves
            // 
            this.txt_Slaves.Location = new System.Drawing.Point(99, 401);
            this.txt_Slaves.Name = "txt_Slaves";
            this.txt_Slaves.Size = new System.Drawing.Size(118, 20);
            this.txt_Slaves.TabIndex = 42;
            this.txt_Slaves.Text = "2, 3, 4";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(141, 383);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(51, 13);
            this.label15.TabIndex = 43;
            this.label15.Text = "Slave #\'s";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(139, 369);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(53, 13);
            this.label16.TabIndex = 44;
            this.label16.Text = "EtherCAT";
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(26, 358);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(264, 5);
            this.button1.TabIndex = 45;
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(335, 358);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(264, 5);
            this.button2.TabIndex = 46;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(311, 48);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(5, 332);
            this.button3.TabIndex = 47;
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Enabled = false;
            this.button4.Location = new System.Drawing.Point(26, 430);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(264, 5);
            this.button4.TabIndex = 48;
            this.button4.UseVisualStyleBackColor = true;
            // 
            // chk_QBDownload
            // 
            this.chk_QBDownload.AutoSize = true;
            this.chk_QBDownload.Checked = true;
            this.chk_QBDownload.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_QBDownload.Location = new System.Drawing.Point(382, 482);
            this.chk_QBDownload.Name = "chk_QBDownload";
            this.chk_QBDownload.Size = new System.Drawing.Size(169, 17);
            this.chk_QBDownload.TabIndex = 49;
            this.chk_QBDownload.Text = "Enable QB Project Downloads";
            this.chk_QBDownload.UseVisualStyleBackColor = true;
            this.chk_QBDownload.CheckStateChanged += new System.EventHandler(this.chk_QBDownload_CheckedStateChanged);
            // 
            // txt_output
            // 
            this.txt_output.Location = new System.Drawing.Point(123, 92);
            this.txt_output.Name = "txt_output";
            this.txt_output.Size = new System.Drawing.Size(100, 20);
            this.txt_output.TabIndex = 50;
            this.txt_output.Text = "0";
            this.txt_output.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(47, 95);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(67, 13);
            this.label17.TabIndex = 51;
            this.label17.Text = "Output State";
            this.label17.Click += new System.EventHandler(this.label17_Click);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(47, 125);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(59, 13);
            this.label18.TabIndex = 53;
            this.label18.Text = "Input State";
            this.label18.Click += new System.EventHandler(this.label18_Click);
            // 
            // txt_input
            // 
            this.txt_input.Location = new System.Drawing.Point(123, 122);
            this.txt_input.Name = "txt_input";
            this.txt_input.Size = new System.Drawing.Size(100, 20);
            this.txt_input.TabIndex = 52;
            this.txt_input.Text = "0";
            this.txt_input.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            // 
            // IncentiveAPI_CSharp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 511);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.txt_input);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.txt_output);
            this.Controls.Add(this.chk_QBDownload);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.txt_Slaves);
            this.Controls.Add(this.btn_offlineSlave);
            this.Controls.Add(this.btn_onlineSlave);
            this.Controls.Add(this.btnIndicator);
            this.Controls.Add(this.btnStartIncentive);
            this.Controls.Add(this.btnStopIncentive);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txt_Adjustment);
            this.Controls.Add(this.btn_Adjustment);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txt_vel);
            this.Controls.Add(this.chk_AxisOnly);
            this.Controls.Add(this.chk_Home);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txt_Decel);
            this.Controls.Add(this.chk_EnablePosition);
            this.Controls.Add(this.btn_UpdatePosition);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txt_Accel);
            this.Controls.Add(this.txt_MaxSpeed);
            this.Controls.Add(this.txt_Position);
            this.Controls.Add(this.btn_Stop);
            this.Controls.Add(this.chk_Monitor);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.progressBar_fpos);
            this.Controls.Add(this.txt_fpos);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txt_Status);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_Outputs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_Axis);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_register);
            this.Controls.Add(this.btn_Start);
            this.Name = "IncentiveAPI_CSharp";
            this.Text = "Incentive C# API Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.TextBox txt_register;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_Axis;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_Outputs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_Status;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_fpos;
        private System.Windows.Forms.ProgressBar progressBar_fpos;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chk_Monitor;
        private System.Windows.Forms.Button btn_Stop;
        private System.Windows.Forms.TextBox txt_Position;
        private System.Windows.Forms.TextBox txt_MaxSpeed;
        private System.Windows.Forms.TextBox txt_Accel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btn_UpdatePosition;
        private System.Windows.Forms.CheckBox chk_EnablePosition;
        private System.Windows.Forms.TextBox txt_Decel;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chk_Home;
        private System.Windows.Forms.CheckBox chk_AxisOnly;
        private System.Windows.Forms.TextBox txt_vel;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btn_Adjustment;
        private System.Windows.Forms.TextBox txt_Adjustment;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btnStopIncentive;
        private System.Windows.Forms.Button btnStartIncentive;
        private System.Windows.Forms.Button btnIndicator;
        private System.Windows.Forms.Button btn_onlineSlave;
        private System.Windows.Forms.Button btn_offlineSlave;
        private System.Windows.Forms.TextBox txt_Slaves;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.CheckBox chk_QBDownload;
        private System.Windows.Forms.TextBox txt_output;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txt_input;
    }
}

