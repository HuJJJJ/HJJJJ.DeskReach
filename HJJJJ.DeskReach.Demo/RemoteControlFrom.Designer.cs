namespace HJJJJ.DeskReach.Demo
{
    partial class RemoteControlFrom
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteControlFrom));
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.BrushStatus_Btn = new System.Windows.Forms.ToolStripButton();
            this.BrushColor_Btn = new System.Windows.Forms.ToolStripButton();
            this.BrushSizeComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.ClearDrawingBoard = new System.Windows.Forms.ToolStripButton();
            this.closeDrawingBoard = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.comboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.panel1.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(956, 644);
            this.panel2.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.toolStrip);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 601);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(956, 43);
            this.panel1.TabIndex = 8;
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BrushStatus_Btn,
            this.BrushColor_Btn,
            this.BrushSizeComboBox,
            this.ClearDrawingBoard,
            this.closeDrawingBoard,
            this.toolStripSeparator1,
            this.comboBox1,
            this.toolStripButton3,
            this.toolStripButton4,
            this.toolStripButton5});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(956, 43);
            this.toolStrip.TabIndex = 7;
            this.toolStrip.Text = "toolStrip1";
            // 
            // BrushStatus_Btn
            // 
            this.BrushStatus_Btn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BrushStatus_Btn.Image = ((System.Drawing.Image)(resources.GetObject("BrushStatus_Btn.Image")));
            this.BrushStatus_Btn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BrushStatus_Btn.Name = "BrushStatus_Btn";
            this.BrushStatus_Btn.Size = new System.Drawing.Size(50, 38);
            this.BrushStatus_Btn.Text = "画笔";
            this.BrushStatus_Btn.Click += new System.EventHandler(this.BrushStatus_Btn_Click);
            // 
            // BrushColor_Btn
            // 
            this.BrushColor_Btn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BrushColor_Btn.Image = ((System.Drawing.Image)(resources.GetObject("BrushColor_Btn.Image")));
            this.BrushColor_Btn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.BrushColor_Btn.Name = "BrushColor_Btn";
            this.BrushColor_Btn.Size = new System.Drawing.Size(86, 38);
            this.BrushColor_Btn.Text = "画笔颜色";
            this.BrushColor_Btn.Click += new System.EventHandler(this.BrushColor_Btn_Click);
            // 
            // BrushSizeComboBox
            // 
            this.BrushSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BrushSizeComboBox.Name = "BrushSizeComboBox";
            this.BrushSizeComboBox.Size = new System.Drawing.Size(100, 43);
            this.BrushSizeComboBox.SelectedIndexChanged += new System.EventHandler(this.BrushSizeComboBox_SelectedIndexChanged);
            // 
            // ClearDrawingBoard
            // 
            this.ClearDrawingBoard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ClearDrawingBoard.Image = ((System.Drawing.Image)(resources.GetObject("ClearDrawingBoard.Image")));
            this.ClearDrawingBoard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ClearDrawingBoard.Name = "ClearDrawingBoard";
            this.ClearDrawingBoard.Size = new System.Drawing.Size(86, 38);
            this.ClearDrawingBoard.Text = "清空画板";
            this.ClearDrawingBoard.Click += new System.EventHandler(this.ClearDrawingBoard_Click);
            // 
            // closeDrawingBoard
            // 
            this.closeDrawingBoard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.closeDrawingBoard.Image = ((System.Drawing.Image)(resources.GetObject("closeDrawingBoard.Image")));
            this.closeDrawingBoard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.closeDrawingBoard.Name = "closeDrawingBoard";
            this.closeDrawingBoard.Size = new System.Drawing.Size(86, 38);
            this.closeDrawingBoard.Text = "关闭画板";
            this.closeDrawingBoard.Click += new System.EventHandler(this.CloseDrawingBoard_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 43);
            // 
            // comboBox1
            // 
            this.comboBox1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.comboBox1.AutoSize = false;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(100, 32);
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(0, 38);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton4.Enabled = false;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(82, 38);
            this.toolStripButton4.Text = "视频质量";
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(86, 38);
            this.toolStripButton5.Text = "发送字幕";
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton5_Click);
            // 
            // RemoteControlFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(956, 644);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Name = "RemoteControlFrom";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.RemoteControlFrom_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton BrushStatus_Btn;
        private System.Windows.Forms.ToolStripButton BrushColor_Btn;
        private System.Windows.Forms.ToolStripComboBox BrushSizeComboBox;
        private System.Windows.Forms.ToolStripButton ClearDrawingBoard;
        private System.Windows.Forms.ToolStripButton closeDrawingBoard;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox comboBox1;
        private System.Windows.Forms.ToolStripLabel toolStripButton3;
        private System.Windows.Forms.ToolStripLabel toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}

