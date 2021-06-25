
namespace NotYetHAX
{
    partial class Confirmation
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Confirmation));
            this.Exit = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.DragLabelLMFAO = new System.Windows.Forms.Label();
            this.DragButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.CANCEL = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.OpacityChecker = new System.Windows.Forms.Timer(this.components);
            this.FadeOut = new System.Windows.Forms.Timer(this.components);
            this.Drag = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.DragConf = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.LabelDrag = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.ConfPic = new Guna.UI2.WinForms.Guna2PictureBox();
            this.FocusLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ConfPic)).BeginInit();
            this.SuspendLayout();
            // 
            // Exit
            // 
            this.Exit.BackColor = System.Drawing.Color.Black;
            this.Exit.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Exit.ForeColor = System.Drawing.Color.Lime;
            this.Exit.Location = new System.Drawing.Point(261, 2);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(63, 28);
            this.Exit.TabIndex = 164;
            this.Exit.Text = "  X";
            this.Exit.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Exit_MouseDown);
            this.Exit.MouseEnter += new System.EventHandler(this.Exit_MouseEnter);
            this.Exit.MouseLeave += new System.EventHandler(this.Exit_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Lime;
            this.label1.Location = new System.Drawing.Point(3, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 24);
            this.label1.TabIndex = 163;
            this.label1.Text = "Confirmation";
            // 
            // DragLabelLMFAO
            // 
            this.DragLabelLMFAO.BackColor = System.Drawing.Color.Black;
            this.DragLabelLMFAO.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DragLabelLMFAO.ForeColor = System.Drawing.Color.Black;
            this.DragLabelLMFAO.Location = new System.Drawing.Point(2, 4);
            this.DragLabelLMFAO.Name = "DragLabelLMFAO";
            this.DragLabelLMFAO.Size = new System.Drawing.Size(322, 27);
            this.DragLabelLMFAO.TabIndex = 168;
            this.DragLabelLMFAO.Text = "NotYetHAX\'s Free Trainer";
            // 
            // DragButton
            // 
            this.DragButton.BackColor = System.Drawing.Color.Black;
            this.DragButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.DragButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.DragButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Black;
            this.DragButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DragButton.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DragButton.ForeColor = System.Drawing.Color.Lime;
            this.DragButton.Location = new System.Drawing.Point(0, 0);
            this.DragButton.Name = "DragButton";
            this.DragButton.Size = new System.Drawing.Size(325, 32);
            this.DragButton.TabIndex = 165;
            this.DragButton.UseVisualStyleBackColor = false;
            this.DragButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragButton_MouseDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Black;
            this.label3.Font = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.Lime;
            this.label3.Location = new System.Drawing.Point(12, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(302, 24);
            this.label3.TabIndex = 169;
            this.label3.Text = "Are you sure you want to Exit?";
            // 
            // CANCEL
            // 
            this.CANCEL.BackColor = System.Drawing.Color.Black;
            this.CANCEL.FlatAppearance.BorderColor = System.Drawing.Color.Lime;
            this.CANCEL.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.CANCEL.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.CANCEL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CANCEL.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CANCEL.ForeColor = System.Drawing.Color.Lime;
            this.CANCEL.Location = new System.Drawing.Point(213, 77);
            this.CANCEL.Name = "CANCEL";
            this.CANCEL.Size = new System.Drawing.Size(111, 38);
            this.CANCEL.TabIndex = 170;
            this.CANCEL.Text = "Cancel";
            this.CANCEL.UseVisualStyleBackColor = false;
            this.CANCEL.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CANCEL_MouseDown);
            // 
            // OK
            // 
            this.OK.BackColor = System.Drawing.Color.Black;
            this.OK.FlatAppearance.BorderColor = System.Drawing.Color.Lime;
            this.OK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.OK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(24)))));
            this.OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OK.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.OK.ForeColor = System.Drawing.Color.Lime;
            this.OK.Location = new System.Drawing.Point(1, 77);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(111, 38);
            this.OK.TabIndex = 171;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = false;
            this.OK.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OK_MouseDown);
            // 
            // OpacityChecker
            // 
            this.OpacityChecker.Interval = 5;
            this.OpacityChecker.Tick += new System.EventHandler(this.OpacityChecker_Tick);
            // 
            // FadeOut
            // 
            this.FadeOut.Interval = 2;
            this.FadeOut.Tick += new System.EventHandler(this.FadeOut_Tick);
            // 
            // Drag
            // 
            this.Drag.ContainerControl = this;
            this.Drag.TargetControl = this.DragLabelLMFAO;
            // 
            // DragConf
            // 
            this.DragConf.ContainerControl = this;
            this.DragConf.TargetControl = this;
            // 
            // LabelDrag
            // 
            this.LabelDrag.ContainerControl = this;
            this.LabelDrag.TargetControl = this.label1;
            // 
            // ConfPic
            // 
            this.ConfPic.Image = global::NotYetHAX.Properties.Resources.info_512;
            this.ConfPic.Location = new System.Drawing.Point(120, 63);
            this.ConfPic.Name = "ConfPic";
            this.ConfPic.ShadowDecoration.Parent = this.ConfPic;
            this.ConfPic.Size = new System.Drawing.Size(86, 49);
            this.ConfPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ConfPic.TabIndex = 172;
            this.ConfPic.TabStop = false;
            // 
            // FocusLabel
            // 
            this.FocusLabel.AutoSize = true;
            this.FocusLabel.Location = new System.Drawing.Point(334, 63);
            this.FocusLabel.Name = "FocusLabel";
            this.FocusLabel.Size = new System.Drawing.Size(35, 13);
            this.FocusLabel.TabIndex = 173;
            this.FocusLabel.Text = "label2";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Lime;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Lime;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(0, 32);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(1, 90);
            this.button1.TabIndex = 174;
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Lime;
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Lime;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(324, 31);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(1, 90);
            this.button2.TabIndex = 175;
            this.button2.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Lime;
            this.button3.FlatAppearance.BorderColor = System.Drawing.Color.Lime;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(0, 116);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(326, 1);
            this.button3.TabIndex = 179;
            this.button3.UseVisualStyleBackColor = false;
            // 
            // Confirmation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(326, 117);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.FocusLabel);
            this.Controls.Add(this.ConfPic);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.CANCEL);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DragLabelLMFAO);
            this.Controls.Add(this.DragButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Confirmation";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.ConfPic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label Exit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label DragLabelLMFAO;
        private System.Windows.Forms.Button DragButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button CANCEL;
        private System.Windows.Forms.Button OK;
        private Guna.UI2.WinForms.Guna2PictureBox ConfPic;
        private System.Windows.Forms.Timer OpacityChecker;
        private System.Windows.Forms.Timer FadeOut;
        private Guna.UI2.WinForms.Guna2DragControl Drag;
        private Guna.UI2.WinForms.Guna2DragControl DragConf;
        private Guna.UI2.WinForms.Guna2DragControl LabelDrag;
        private System.Windows.Forms.Label FocusLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}