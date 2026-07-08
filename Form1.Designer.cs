namespace Script_runner {
    partial class Form1 {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            button1 = new Button();
            resultLabel = new Label();
            createbutton = new Button();
            deletbutton = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new Point(324 , 36);
            textBox1.Margin = new Padding(3 , 2 , 3 , 2);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(154 , 23);
            textBox1.TabIndex = 0;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(324 , 84);
            textBox2.Margin = new Padding(3 , 2 , 3 , 2);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(154 , 23);
            textBox2.TabIndex = 1;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(324 , 134);
            textBox3.Margin = new Padding(3 , 2 , 3 , 2);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(154 , 23);
            textBox3.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(266 , 38);
            label1.Name = "label1";
            label1.Size = new Size(50 , 15);
            label1.TabIndex = 3;
            label1.Text = "Country";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(270 , 86);
            label2.Name = "label2";
            label2.Size = new Size(46 , 15);
            label2.TabIndex = 4;
            label2.Text = "County";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(289 , 136);
            label3.Name = "label3";
            label3.Size = new Size(28 , 15);
            label3.TabIndex = 5;
            label3.Text = "City";
            // 
            // button1
            // 
            button1.Location = new Point(165 , 194);
            button1.Margin = new Padding(3 , 2 , 3 , 2);
            button1.Name = "button1";
            button1.Size = new Size(82 , 22);
            button1.TabIndex = 6;
            button1.Text = "Submit";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // resultLabel
            // 
            resultLabel.AutoSize = true;
            resultLabel.Location = new Point(340 , 231);
            resultLabel.Name = "resultLabel";
            resultLabel.Size = new Size(68 , 15);
            resultLabel.TabIndex = 7;
            resultLabel.Text = "result here: ";
            // 
            // createbutton
            // 
            createbutton.Location = new Point(270 , 194);
            createbutton.Name = "createbutton";
            createbutton.Size = new Size(75 , 23);
            createbutton.TabIndex = 8;
            createbutton.Text = "Create city";
            createbutton.UseVisualStyleBackColor = true;
            createbutton.Click += createbutton_Click;
            // 
            // deletbutton
            // 
            deletbutton.Location = new Point(403 , 193);
            deletbutton.Name = "deletbutton";
            deletbutton.Size = new Size(75 , 23);
            deletbutton.TabIndex = 9;
            deletbutton.Text = "Delete";
            deletbutton.UseVisualStyleBackColor = true;
            deletbutton.Click += deletbutton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F , 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700 , 338);
            Controls.Add(deletbutton);
            Controls.Add(createbutton);
            Controls.Add(resultLabel);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Margin = new Padding(3 , 2 , 3 , 2);
            Name = "Form1";
            Text = "Task1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private TextBox textBox2;
        private TextBox textBox3;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button button1;
        private Button createbutton;
        private Button deletbutton;
        public Label resultLabel;
    }
}
