namespace CartCollision
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtM1 = new System.Windows.Forms.TextBox();
            this.txtV1 = new System.Windows.Forms.TextBox();
            this.txtM2 = new System.Windows.Forms.TextBox();
            this.txtV2 = new System.Windows.Forms.TextBox();
            this.lblM1 = new System.Windows.Forms.Label();
            this.lblM2 = new System.Windows.Forms.Label();
            this.lblV1 = new System.Windows.Forms.Label();
            this.lblV2 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.timerAnimation = new System.Windows.Forms.Timer(this.components);
            this.lblResult = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtM1
            // 
            this.txtM1.Location = new System.Drawing.Point(29, 26);
            this.txtM1.Name = "txtM1";
            this.txtM1.Size = new System.Drawing.Size(100, 26);
            this.txtM1.TabIndex = 0;
            this.txtM1.Text = "10";
            // 
            // txtV1
            // 
            this.txtV1.Location = new System.Drawing.Point(29, 75);
            this.txtV1.Name = "txtV1";
            this.txtV1.Size = new System.Drawing.Size(100, 26);
            this.txtV1.TabIndex = 1;
            this.txtV1.Text = "20";
            // 
            // txtM2
            // 
            this.txtM2.Location = new System.Drawing.Point(29, 125);
            this.txtM2.Name = "txtM2";
            this.txtM2.Size = new System.Drawing.Size(100, 26);
            this.txtM2.TabIndex = 2;
            this.txtM2.Text = "15";
            // 
            // txtV2
            // 
            this.txtV2.Location = new System.Drawing.Point(29, 172);
            this.txtV2.Name = "txtV2";
            this.txtV2.Size = new System.Drawing.Size(100, 26);
            this.txtV2.TabIndex = 3;
            this.txtV2.Text = "25";
            // 
            // lblM1
            // 
            this.lblM1.AutoSize = true;
            this.lblM1.BackColor = System.Drawing.SystemColors.Control;
            this.lblM1.Location = new System.Drawing.Point(156, 31);
            this.lblM1.Name = "lblM1";
            this.lblM1.Size = new System.Drawing.Size(120, 20);
            this.lblM1.TabIndex = 4;
            this.lblM1.Text = "Масса - m1 (кг)";
            // 
            // lblM2
            // 
            this.lblM2.AutoSize = true;
            this.lblM2.BackColor = System.Drawing.SystemColors.Control;
            this.lblM2.Location = new System.Drawing.Point(156, 131);
            this.lblM2.Name = "lblM2";
            this.lblM2.Size = new System.Drawing.Size(120, 20);
            this.lblM2.TabIndex = 5;
            this.lblM2.Text = "Масса - m2 (кг)";
            // 
            // lblV1
            // 
            this.lblV1.AutoSize = true;
            this.lblV1.BackColor = System.Drawing.SystemColors.Control;
            this.lblV1.Location = new System.Drawing.Point(156, 78);
            this.lblV1.Name = "lblV1";
            this.lblV1.Size = new System.Drawing.Size(114, 20);
            this.lblV1.TabIndex = 6;
            this.lblV1.Text = "Скорость - V1";
            // 
            // lblV2
            // 
            this.lblV2.AutoSize = true;
            this.lblV2.BackColor = System.Drawing.SystemColors.Control;
            this.lblV2.Location = new System.Drawing.Point(156, 178);
            this.lblV2.Name = "lblV2";
            this.lblV2.Size = new System.Drawing.Size(114, 20);
            this.lblV2.TabIndex = 7;
            this.lblV2.Text = "Скорость - V2";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(29, 248);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 40);
            this.btnStart.TabIndex = 8;
            this.btnStart.Text = "Старт";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(29, 308);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 38);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "Сбросить";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // pnlCanvas
            // 
            this.pnlCanvas.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnlCanvas.Location = new System.Drawing.Point(349, 26);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(647, 379);
            this.pnlCanvas.TabIndex = 10;
            this.pnlCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlCanvas_Paint);
            // 
            // timerAnimation
            // 
            this.timerAnimation.Interval = 30;
            this.timerAnimation.Tick += new System.EventHandler(this.timerAnimation_Tick);
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.BackColor = System.Drawing.SystemColors.Control;
            this.lblResult.Location = new System.Drawing.Point(29, 385);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(241, 20);
            this.lblResult.TabIndex = 11;
            this.lblResult.Text = "Скорость после сталкновения";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(1062, 464);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.pnlCanvas);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblV2);
            this.Controls.Add(this.lblV1);
            this.Controls.Add(this.lblM2);
            this.Controls.Add(this.lblM1);
            this.Controls.Add(this.txtV2);
            this.Controls.Add(this.txtM2);
            this.Controls.Add(this.txtV1);
            this.Controls.Add(this.txtM1);
            this.Name = "Form1";
            this.Text = "CartCollision";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtM1;
        private System.Windows.Forms.TextBox txtV1;
        private System.Windows.Forms.TextBox txtM2;
        private System.Windows.Forms.TextBox txtV2;
        private System.Windows.Forms.Label lblM1;
        private System.Windows.Forms.Label lblM2;
        private System.Windows.Forms.Label lblV1;
        private System.Windows.Forms.Label lblV2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.Timer timerAnimation;
        private System.Windows.Forms.Label lblResult;
    }
}

