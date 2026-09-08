namespace ProjetosCADLaser.Forms
{
    partial class FormTemporarioCompilacao
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label avisoLabel;

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
            this.avisoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.avisoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.avisoLabel.Location = new System.Drawing.Point(0, 0);
            this.avisoLabel.Name = "avisoLabel";
            this.avisoLabel.Padding = new System.Windows.Forms.Padding(24);
            this.avisoLabel.Size = new System.Drawing.Size(584, 161);
            this.avisoLabel.TabIndex = 0;
            this.avisoLabel.Text = "TELA TEMPORARIA - validacao de compilacao da Fase 1";
            this.avisoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 161);
            this.Controls.Add(this.avisoLabel);
            this.Name = "FormTemporarioCompilacao";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Projetos CAD/LASER - Fase 1 (TEMPORARIO)";
            this.ResumeLayout(false);
        }
    }
}
