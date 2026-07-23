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
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            label4 = new Label();
            label5 = new Label();
            button5 = new Button();
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
            button1.Location = new Point(324 , 194);
            button1.Margin = new Padding(3 , 2 , 3 , 2);
            button1.Name = "button1";
            button1.Size = new Size(133 , 22);
            button1.TabIndex = 6;
            button1.Text = "Printtoscreen";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // resultLabel
            // 
            resultLabel.AutoSize = true;
            resultLabel.Location = new Point(357 , 169);
            resultLabel.Name = "resultLabel";
            resultLabel.Size = new Size(68 , 15);
            resultLabel.TabIndex = 7;
            resultLabel.Text = "result here: ";
            // 
            // createbutton
            // 
            createbutton.Location = new Point(83 , 288);
            createbutton.Name = "createbutton";
            createbutton.Size = new Size(75 , 23);
            createbutton.TabIndex = 8;
            createbutton.Text = "Create city";
            createbutton.UseVisualStyleBackColor = true;
            createbutton.Click += createbutton_Click;
            // 
            // deletbutton
            // 
            deletbutton.Location = new Point(83 , 317);
            deletbutton.Name = "deletbutton";
            deletbutton.Size = new Size(75 , 23);
            deletbutton.TabIndex = 9;
            deletbutton.Text = "Delete";
            deletbutton.UseVisualStyleBackColor = true;
            deletbutton.Click += deletbutton_Click;
            // 
            // button2
            // 
            button2.Location = new Point(276 , 340);
            button2.Margin = new Padding(3 , 2 , 3 , 2);
            button2.Name = "button2";
            button2.Size = new Size(82 , 22);
            button2.TabIndex = 10;
            button2.Text = "viewBo";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(232 , 288);
            button3.Margin = new Padding(3 , 2 , 3 , 2);
            button3.Name = "button3";
            button3.Size = new Size(172 , 22);
            button3.TabIndex = 11;
            button3.Text = "Create instance in api";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Location = new Point(255 , 314);
            button4.Margin = new Padding(3 , 2 , 3 , 2);
            button4.Name = "button4";
            button4.Size = new Size(118 , 22);
            button4.TabIndex = 12;
            button4.Text = "Clear Api Store";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(270 , 256);
            label4.Name = "label4";
            label4.Size = new Size(90 , 15);
            label4.TabIndex = 13;
            label4.Text = "API interactions";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(59 , 256);
            label5.Name = "label5";
            label5.Size = new Size(120 , 15);
            label5.TabIndex = 14;
            label5.Text = "Database interactions";
            // 
            // button5
            // 
            button5.Location = new Point(39 , 82);
            button5.Name = "button5";
            button5.Size = new Size(119 , 23);
            button5.TabIndex = 15;
            button5.Text = "Internal Editor";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F , 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(626 , 419);
            Controls.Add(button5);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
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
        private Button button2;
        private Button button3;
        private Button button4;
        private Label label4;
        private Label label5;
        private Button button5;
    }
}
