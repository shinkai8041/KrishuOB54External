using System;
using System.Windows.Forms;

namespace Krishu_X_External
{
    partial class Krishuupdate
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label update;
        private System.Windows.Forms.Label Chekstatuslbl;
        private Guna.UI2.WinForms.Guna2ProgressBar guna2ProgressBar1;
        private Guna.UI2.WinForms.Guna2ProgressIndicator guna2ProgressIndicator1;
        private Guna.UI2.WinForms.Guna2Separator guna2Separator1;
        private Guna.UI2.WinForms.Guna2BorderlessForm guna2BorderlessForm1;
        private System.Windows.Forms.Timer updateTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.guna2BorderlessForm1 = new Guna.UI2.WinForms.Guna2BorderlessForm(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.update = new System.Windows.Forms.Label();
            this.Chekstatuslbl = new System.Windows.Forms.Label();
            this.guna2ProgressBar1 = new Guna.UI2.WinForms.Guna2ProgressBar();
            this.guna2ProgressIndicator1 = new Guna.UI2.WinForms.Guna2ProgressIndicator();
            this.guna2Separator1 = new Guna.UI2.WinForms.Guna2Separator();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);

            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(427, 175);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Krishuupdate";
            this.Text = "Krishuupdate - Checking Updates";
            this.StartPosition = FormStartPosition.CenterScreen;

            // guna2BorderlessForm1
            this.guna2BorderlessForm1.ContainerControl = this;
            this.guna2BorderlessForm1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2BorderlessForm1.TransparentWhileDrag = true;

            // label1 - KRISHU
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Greater Theory", 12F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(102, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "KRISHU";

            // label2 - X
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Greater Theory", 12F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(190, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "X";

            // label3 - Cheats
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Greater Theory", 12F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(214, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 20);
            this.label3.TabIndex = 0;
            this.label3.Text = "Cheats";

            // guna2Separator1
            this.guna2Separator1.Location = new System.Drawing.Point(138, 27);
            this.guna2Separator1.Name = "guna2Separator1";
            this.guna2Separator1.Size = new System.Drawing.Size(123, 10);
            this.guna2Separator1.TabIndex = 1;

            // update label
            this.update.AutoSize = true;
            this.update.Font = new System.Drawing.Font("Avengenz", 12F, System.Drawing.FontStyle.Bold);
            this.update.ForeColor = System.Drawing.Color.White;
            this.update.Location = new System.Drawing.Point(135, 55);
            this.update.Name = "update";
            this.update.Size = new System.Drawing.Size(139, 18);
            this.update.TabIndex = 0;
            this.update.Text = "Checking for updates...";

            // Chekstatuslbl
            this.Chekstatuslbl.AutoSize = true;
            this.Chekstatuslbl.Font = new System.Drawing.Font("Avengenz", 10F, System.Drawing.FontStyle.Bold);
            this.Chekstatuslbl.ForeColor = System.Drawing.Color.LightGray;
            this.Chekstatuslbl.Location = new System.Drawing.Point(61, 104);
            this.Chekstatuslbl.Name = "Chekstatuslbl";
            this.Chekstatuslbl.Size = new System.Drawing.Size(115, 14);
            this.Chekstatuslbl.TabIndex = 0;
            this.Chekstatuslbl.Text = "Initializing...";

            // guna2ProgressBar1
            this.guna2ProgressBar1.BorderColor = System.Drawing.Color.White;
            this.guna2ProgressBar1.BorderRadius = 4;
            this.guna2ProgressBar1.BorderThickness = 2;
            this.guna2ProgressBar1.FillColor = System.Drawing.Color.Black;
            this.guna2ProgressBar1.Location = new System.Drawing.Point(53, 125);
            this.guna2ProgressBar1.Name = "guna2ProgressBar1";
            this.guna2ProgressBar1.ProgressColor = System.Drawing.Color.Red;
            this.guna2ProgressBar1.ProgressColor2 = System.Drawing.Color.Maroon;
            this.guna2ProgressBar1.ShadowDecoration.Color = System.Drawing.Color.Red;
            this.guna2ProgressBar1.Size = new System.Drawing.Size(300, 30);
            this.guna2ProgressBar1.TabIndex = 2;
            this.guna2ProgressBar1.Text = "guna2ProgressBar1";
            this.guna2ProgressBar1.TextMode = Guna.UI2.WinForms.Enums.ProgressBarTextMode.Custom;
            this.guna2ProgressBar1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.guna2ProgressBar1.Value = 0;

            // guna2ProgressIndicator1
            this.guna2ProgressIndicator1.Location = new System.Drawing.Point(370, 119);
            this.guna2ProgressIndicator1.Name = "guna2ProgressIndicator1";
            this.guna2ProgressIndicator1.ProgressColor = System.Drawing.Color.White;
            this.guna2ProgressIndicator1.Size = new System.Drawing.Size(45, 37);
            this.guna2ProgressIndicator1.TabIndex = 3;

            // updateTimer
            this.updateTimer.Interval = 100;
            this.updateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);

            // Add controls
            this.Controls.Add(this.guna2ProgressIndicator1);
            this.Controls.Add(this.guna2ProgressBar1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.update);
            this.Controls.Add(this.Chekstatuslbl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.guna2Separator1);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Timer tick event for smooth progress bar animation
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (guna2ProgressBar1.Value < 100)
            {
                guna2ProgressBar1.Value += 1;
            }
        }
    }
}