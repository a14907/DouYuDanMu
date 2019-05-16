namespace DouYu
{
    partial class Form1
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbCateId = new System.Windows.Forms.TextBox();
            this.btnRoomDetail = new System.Windows.Forms.Button();
            this.btnCateDetail = new System.Windows.Forms.Button();
            this.btnGetCate = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.tbServer = new System.Windows.Forms.TextBox();
            this.tbRoomId = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbGroupId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tblogs = new System.Windows.Forms.TextBox();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tbCateId);
            this.groupBox1.Controls.Add(this.btnRoomDetail);
            this.groupBox1.Controls.Add(this.btnCateDetail);
            this.groupBox1.Controls.Add(this.btnGetCate);
            this.groupBox1.Controls.Add(this.btnClose);
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.tbServer);
            this.groupBox1.Controls.Add(this.tbRoomId);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tbGroupId);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(524, 89);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "操作";
            // 
            // tbCateId
            // 
            this.tbCateId.Location = new System.Drawing.Point(79, 60);
            this.tbCateId.Name = "tbCateId";
            this.tbCateId.Size = new System.Drawing.Size(44, 21);
            this.tbCateId.TabIndex = 11;
            // 
            // btnRoomDetail
            // 
            this.btnRoomDetail.Location = new System.Drawing.Point(244, 60);
            this.btnRoomDetail.Name = "btnRoomDetail";
            this.btnRoomDetail.Size = new System.Drawing.Size(87, 23);
            this.btnRoomDetail.TabIndex = 10;
            this.btnRoomDetail.Text = "获取房间详情";
            this.btnRoomDetail.UseVisualStyleBackColor = true;
            this.btnRoomDetail.Click += new System.EventHandler(this.btnRoomDetail_ClickAsync);
            // 
            // btnCateDetail
            // 
            this.btnCateDetail.Location = new System.Drawing.Point(129, 60);
            this.btnCateDetail.Name = "btnCateDetail";
            this.btnCateDetail.Size = new System.Drawing.Size(110, 23);
            this.btnCateDetail.TabIndex = 9;
            this.btnCateDetail.Text = "获取分类房间列表";
            this.btnCateDetail.UseVisualStyleBackColor = true;
            this.btnCateDetail.Click += new System.EventHandler(this.btnCateDetail_ClickAsync);
            // 
            // btnGetCate
            // 
            this.btnGetCate.Location = new System.Drawing.Point(6, 60);
            this.btnGetCate.Name = "btnGetCate";
            this.btnGetCate.Size = new System.Drawing.Size(67, 23);
            this.btnGetCate.TabIndex = 8;
            this.btnGetCate.Text = "获取分类";
            this.btnGetCate.UseVisualStyleBackColor = true;
            this.btnGetCate.Click += new System.EventHandler(this.btnGetCate_ClickAsync);
            // 
            // btnClose
            // 
            this.btnClose.Enabled = false;
            this.btnClose.Location = new System.Drawing.Point(443, 60);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "断开";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(362, 60);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tbServer
            // 
            this.tbServer.Enabled = false;
            this.tbServer.Location = new System.Drawing.Point(71, 20);
            this.tbServer.Name = "tbServer";
            this.tbServer.Size = new System.Drawing.Size(100, 21);
            this.tbServer.TabIndex = 5;
            this.tbServer.Text = "openbarrage.douyutv.com";
            // 
            // tbRoomId
            // 
            this.tbRoomId.Location = new System.Drawing.Point(244, 20);
            this.tbRoomId.Name = "tbRoomId";
            this.tbRoomId.Size = new System.Drawing.Size(100, 21);
            this.tbRoomId.TabIndex = 1;
            this.tbRoomId.Text = "520";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(192, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "RoomID:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "Server:";
            // 
            // tbGroupId
            // 
            this.tbGroupId.Location = new System.Drawing.Point(418, 21);
            this.tbGroupId.Name = "tbGroupId";
            this.tbGroupId.Size = new System.Drawing.Size(100, 21);
            this.tbGroupId.TabIndex = 3;
            this.tbGroupId.Text = "-9999";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(360, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "GroupId:";
            // 
            // tblogs
            // 
            this.tblogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblogs.Location = new System.Drawing.Point(18, 108);
            this.tblogs.Multiline = true;
            this.tblogs.Name = "tblogs";
            this.tblogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tblogs.Size = new System.Drawing.Size(524, 350);
            this.tblogs.TabIndex = 1;
            // 
            // tbMsg
            // 
            this.tbMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMsg.Location = new System.Drawing.Point(19, 464);
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.Size = new System.Drawing.Size(415, 21);
            this.tbMsg.TabIndex = 13;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(440, 464);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(102, 23);
            this.btnSend.TabIndex = 12;
            this.btnSend.Text = "发送弹幕";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 491);
            this.Controls.Add(this.tbMsg);
            this.Controls.Add(this.tblogs);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "斗鱼弹幕";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox tbServer;
        private System.Windows.Forms.TextBox tbRoomId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbGroupId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tblogs;
        private System.Windows.Forms.TextBox tbCateId;
        private System.Windows.Forms.Button btnRoomDetail;
        private System.Windows.Forms.Button btnCateDetail;
        private System.Windows.Forms.Button btnGetCate;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.Button btnSend;
    }
}

