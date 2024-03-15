namespace HJJJJ.DeskReach.Windows
{
    partial class SlaveForm
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
            this.keyBoardCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.drawingBoardCheckBox = new System.Windows.Forms.CheckBox();
            this.cmdCheckBox = new System.Windows.Forms.CheckBox();
            this.pointerCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // keyBoardCheckBox
            // 
            this.keyBoardCheckBox.AutoSize = true;
            this.keyBoardCheckBox.Checked = true;
            this.keyBoardCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.keyBoardCheckBox.Location = new System.Drawing.Point(6, 23);
            this.keyBoardCheckBox.Name = "keyBoardCheckBox";
            this.keyBoardCheckBox.Size = new System.Drawing.Size(70, 22);
            this.keyBoardCheckBox.TabIndex = 4;
            this.keyBoardCheckBox.Text = "键盘";
            this.keyBoardCheckBox.UseVisualStyleBackColor = true;
            this.keyBoardCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_ChckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdCheckBox);
            this.groupBox1.Controls.Add(this.drawingBoardCheckBox);
            this.groupBox1.Controls.Add(this.pointerCheckBox);
            this.groupBox1.Controls.Add(this.keyBoardCheckBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(326, 51);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "插件列表";
            // 
            // drawingBoardCheckBox
            // 
            this.drawingBoardCheckBox.AutoSize = true;
            this.drawingBoardCheckBox.Checked = true;
            this.drawingBoardCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.drawingBoardCheckBox.Location = new System.Drawing.Point(158, 23);
            this.drawingBoardCheckBox.Name = "drawingBoardCheckBox";
            this.drawingBoardCheckBox.Size = new System.Drawing.Size(70, 22);
            this.drawingBoardCheckBox.TabIndex = 6;
            this.drawingBoardCheckBox.Text = "画板";
            this.drawingBoardCheckBox.UseVisualStyleBackColor = true;
            this.drawingBoardCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_ChckedChanged);
            // 
            // cmdCheckBox
            // 
            this.cmdCheckBox.AutoSize = true;
            this.cmdCheckBox.Checked = true;
            this.cmdCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cmdCheckBox.Location = new System.Drawing.Point(234, 23);
            this.cmdCheckBox.Name = "cmdCheckBox";
            this.cmdCheckBox.Size = new System.Drawing.Size(88, 22);
            this.cmdCheckBox.TabIndex = 7;
            this.cmdCheckBox.Text = "命令行";
            this.cmdCheckBox.UseVisualStyleBackColor = true;
            this.cmdCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_ChckedChanged);
            // 
            // pointerCheckBox
            // 
            this.pointerCheckBox.AutoSize = true;
            this.pointerCheckBox.Checked = true;
            this.pointerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.pointerCheckBox.Location = new System.Drawing.Point(82, 23);
            this.pointerCheckBox.Name = "pointerCheckBox";
            this.pointerCheckBox.Size = new System.Drawing.Size(70, 22);
            this.pointerCheckBox.TabIndex = 5;
            this.pointerCheckBox.Text = "鼠标";
            this.pointerCheckBox.UseVisualStyleBackColor = true;
            this.pointerCheckBox.CheckedChanged += new System.EventHandler(this.CheckBox_ChckedChanged);
            // 
            // SlaveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 67);
            this.Controls.Add(this.groupBox1);
            this.Name = "SlaveForm";
            this.Text = "受控界面";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox keyBoardCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox cmdCheckBox;
        private System.Windows.Forms.CheckBox drawingBoardCheckBox;
        private System.Windows.Forms.CheckBox pointerCheckBox;
    }
}