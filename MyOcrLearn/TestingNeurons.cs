using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace MyOcrLearn
{
    public partial class TestingNeurons : Form
    {
        string currimg = "";
        string cur_test_path = "";
        string fname = "";
        int currindx = 0;
        bool stopped = true;

        Thread th;

        char[] inpslet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        Pen myPen;
        SolidBrush myBrush;
        Graphics formGraphics;
        int originx = 600;
        int originy = 430;
        int size = 300;
        int scale = 30; //50m to 1 in pixels
        double zoom = 1; //5 m to 1 pixel
        double currentx; //variable used to check max valueof x drawn on x-axis for autozoom
        double currenty; //variable used to check max value of y drawn on y-axis

        int inpctr = 0;
        int ctroo = 0;

        int indx1 = 0;
        int indx2 = 0;
        int indx3 = 0;


        public int mode = 0;
        public string g_path = "C:\\Projects_Segregated\\c#\\newest\\ocr + regression\\MyOcrLearn\\MyOcrLearn\\train_set\\";
        double indx1_val = 0;
        double indx2_val = 0;
        double indx3_val = 0;

        public static int inputlayer = 16*16;
        public static int hiddenlayer = 30;
        public static int outputlayer = 26;

        public static  double learningrate = 0.2;
        public static double momentum = 0;
        public static double mse = 0;

        public int patnr = 0;
        Random r;

        double[] input = new double[inputlayer];
        double[] hidden = new double[hiddenlayer];
        double[] output = new double[outputlayer];

        double[,] weights1 = new double[inputlayer, hiddenlayer];
        double[,] weights2 = new double[hiddenlayer, outputlayer];

        double[] hiddengrad = new double[hiddenlayer];
        double[] outputgrad = new double[outputlayer];

        double[,] weight1delta = new double[inputlayer, hiddenlayer];
        double[,] weight2delta = new double[hiddenlayer, outputlayer];

        double[] bias1 = new double[hiddenlayer];

        double[] bias2 = new double[outputlayer];



        float[] xi;
        float[] yi;
        int xsize = 0;
        int point_ctr = 0;



        double[] outmatx;

        double[] pat = new double[256];

        int ctrr = 0;
        int ctrr2 = 0;

        string name = "";



        public TestingNeurons()
        {

            InitializeComponent();

            r = new Random();
            setNewImage(g_path+"a01616256.bmp");
            cur_test_path = g_path+"a01616256.bmp";
            //pictureBox1.Scale(new SizeF(100, 100));
           // pictureBox1.Show();
          

        
        }

        void TestingNeurons_DoubleClick(object sender, System.EventArgs e)
        {

            Point position = Cursor.Position;
            //1139, 606
            

           

            Point position2 = this.PointToClient(Cursor.Position);

            //MessageBox.Show(((float)(position2.Y-originy)/(scale*10)).ToString());
            formGraphics.DrawEllipse(myPen,position2.X,position2.Y,5,5);
            //(float)originx+x*scale*10,(float)originy-y*scale*10,


            xi[point_ctr] = (float)(position2.X-originx)/(scale*10); 
            yi[point_ctr] = (float)-(position2.Y-originy)/(scale*10);

            point_ctr++;






        }

        private void setNewImage(string name)
        {


            Image bit = new Bitmap(name);

            Image bit2 = new Bitmap(bit, new Size(100, 100));
            cur_test_path = name;

            pictureBox1.Image = bit2;


            richTextBox1.Text = "";

            byte[] mybyt = File.ReadAllBytes(name);

            for (int i = 0; i < 16 * 16; i++)
            {
                if ((int)mybyt[mybyt.Length - i - 1] == 255)
                {

                    mybyt[i] = 0;
                }
                else
                {
                    mybyt[i] = 1;

                }


            }






            for (int i = 0; i < 16*16; i++)
            {



                richTextBox1.Text += mybyt[i].ToString() + mybyt[i].ToString();

                    if (i%16 == 15)
                    {
                        richTextBox1.Text += " \r\n";
                    }
                
            }

          




        }

        private int retIndexOfChar(char[] a, char b)
        {

            return Array.IndexOf(a, b);


        }

        private char retCharOfIndex(int indx)
        {

            return inpslet[indx];
        }

        private char retRandChar(char[] a)
        {
            Random r = new Random();

            return a[r.Next(0, 25)];
        }

        private double[] retOutputArr(char a, int asize)
        {
            int indx1 = retIndexOfChar(inpslet, a);
            double[] outp = new double[asize];
 
            for (int i = 0; i < asize; i++)
            {
                if (i == indx1)
                    outp[i] = 1;
                else
                    outp[i] = 0;

            }
            return outp;
        }

        private double[] retOutputArr2(int indx)
        {

            if (indx == 0)
            {
                return new double[] {0};
            }
            else if (indx == 1)
            {
                return new double[] {1};
            }
            else if (indx == 2)
            {
                return new double[] {1};
            }
            else
            {
                return new double[] {0};
            }


        }

        private double[] retOutputArr4()
        {


            if (name == "one")
            {
                return new double[] { 1, 0, 0 };
            }
            else if (name == "two")
            {
                return new double[] { 0, 1, 0 };
            }
            else
            {

                return new double[] { 0, 0, 1 };
            }




        }

        private double[] getPattern(char a)
        {


            double[] mydoubleimg = new double[256];


            r = new Random();

            int aa = r.Next(0, 4);




            currimg = g_path + a + aa.ToString() + "1616256.bmp";
            currindx = retIndexOfChar(inpslet, a);
            fname = a + aa.ToString()+ "1616256.bmp";

            byte[] mybyt = File.ReadAllBytes(currimg);
            byte[] mybyteimg = new byte[16 * 16];
            byte[,] imgmatrix = new byte[16, 16];

            for (int i = 0; i < 16 * 16; i++)
            {
                if ((int)mybyt[mybyt.Length - i - 1] == 255)
                {

                    mybyteimg[i] = 0;
                }
                else
                {
                    mybyteimg[i] = 1;

                }


            }


            int ctr = 0;
            //mirror(transform) the image into correct form
            for (int i = 0; i < 16; i++)
            {
                for (int j = 16 - 1; j >= 0; j--)
                {

                    imgmatrix[i, j] = mybyteimg[ctr++];
                }

            }

            ctr = 0;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if(imgmatrix[i, j]==0){

                        mydoubleimg[ctr++] = 0;

                    }else{
                        mydoubleimg[ctr++] = (double)imgmatrix[i, j];
                     }

                }

            }

            return mydoubleimg;
        }

        private double[] getPattern2()
        {


            double[] mydoubleimg = new double[256];

            if (ctrr2 == 3)
            {
                ctrr2 = 0;
                ctrr = ctrr + 1;
            }
            if (ctrr == 26)
            {
                ctrr = 0;
            }


            char ch = retCharOfIndex(ctrr);



            currimg = "C:\\Users\\Matt\\Documents\\visual studio 2010\\Projects\\MyOcrLearn\\MyOcrLearn\\" + ch + ctrr2++.ToString()+ "1616256.bmp";
            currindx = retIndexOfChar(inpslet, ch);

            byte[] mybyt = File.ReadAllBytes(currimg);
            byte[] mybyteimg = new byte[16 * 16];
            byte[,] imgmatrix = new byte[16, 16];

            for (int i = 0; i < 16 * 16; i++)
            {
                if ((int)mybyt[mybyt.Length - i - 1] == 255)
                {

                    mybyteimg[i] = 0;
                }
                else
                {
                    mybyteimg[i] = 1;

                }


            }


            int ctr = 0;
            //mirror(transform) the image into correct form
            for (int i = 0; i < 16; i++)
            {
                for (int j = 16 - 1; j >= 0; j--)
                {

                    imgmatrix[i, j] = mybyteimg[ctr++];
                }

            }

            ctr = 0;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if(imgmatrix[i, j]==0){

                        mydoubleimg[ctr++] = 0;

                    }else{
                        mydoubleimg[ctr++] = (double)imgmatrix[i, j];
                     }

                }

            }

            return mydoubleimg;
        }

        private double[] getPattern3()
        {



            if (inpctr == 0)
            {
                inpctr++;
                patnr = 0;
                return new double[] { 0, 0 };
            }
            else if (inpctr == 1)
            {
                inpctr++;
                patnr = 1;
                return new double[] { 0, 1 };
            }
            else if (inpctr == 2)
            {
                inpctr++;
                patnr = 2;
                return new double[] { 1, 0 };
            }
            else
            {
                inpctr = 0;
                patnr = 3;
                return new double[] { 1, 1 };
            }

        }

        private double[] getParretn4()
        {

            double[] mydoubleimg = new double[256];

            currimg = cur_test_path;
     

            byte[] mybyt = File.ReadAllBytes(currimg);

            byte[] mybyteimg = new byte[16 * 16];

            byte[,] imgmatrix = new byte[16, 16];


            for (int i = 0; i < 16 * 16; i++)
            {
                if ((int)mybyt[mybyt.Length - i - 1] == 255)
                {

                    mybyteimg[i] = 0;
                }
                else
                {
                    mybyteimg[i] = 255;

                }


            }


            int ctr = 0;
            //mirror(transform) the image into correct form
            for (int i = 0; i < 16; i++)
            {
                for (int j = 15; j >= 0; j--)
                {

                    imgmatrix[i, j] = mybyteimg[ctr++];
                }

            }

            ctr = 0;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (imgmatrix[i, j] == 0)
                    {

                        mydoubleimg[ctr++] = 0;

                    }
                    else
                    {
                        mydoubleimg[ctr++] = (double)imgmatrix[i, j];
                    }

                }

            }


            return mydoubleimg;


        }

        private double ActivateFunct(double a)
        {

            return 1 / (1 + Math.Exp(-a));
        }

        private int highests(double[] a)
        {
            indx1 = 0;
            indx2 = 0;
            indx3 = 0;
            indx1_val = a[0];
            indx2_val = a[0];
            indx3_val = a[0];

            double val = 0;

            for (int i = 0; i < 26; i++)
            {
                if (a[i] > val)
                {
                    indx3 = indx2;
                    indx3_val = indx2_val;
                    indx2 = indx1;
                    indx2_val = indx1_val;
                    indx1 = i;
                    
                    val = a[i];
                    indx1_val = val;
                }
            }

            return indx1;
        }

        private void train()
        {
            int training_set_size;

            if (mode == 0)
            {
               training_set_size = 26 * 4;
            }
            else
            {

                training_set_size = 4;
            }
     
            while (!stopped || mse<0.001)
            {

                for (int u = 0; u < training_set_size; u++)
                {


                    if (mode == 0)
                    {

                        pat = getPattern(inpslet[u % 26]);
                    }
                    else
                    {
                        pat = getPattern3();

                    }


                    for (int i = 0; i < hiddenlayer; i++)
                    {

                        hidden[i] = 0;

                        for (int j = 0; j < inputlayer; j++)
                        {
                            input[j] = pat[j];

                            hidden[i] += weights1[j, i] * pat[j];

                        }
                       // hidden[i] += bias1[i];

                        hidden[i] = ActivateFunct(hidden[i]);

                    }

                    //hidden->output
                    for (int i = 0; i < outputlayer; i++)
                    {

                        output[i] = 0;
                        for (int j = 0; j < hiddenlayer; j++)
                        {

                            output[i] += weights2[j, i] * hidden[j];

                        }
                       // output[i] += bias2[i];
                        output[i] = ActivateFunct(output[i]);

                    }



                    //backpropagate


                    ctroo++;

                    if (mode == 0)
                    {
                        outmatx = retOutputArr(retCharOfIndex(currindx), 26);
                    }
                    else
                    {

                        outmatx = retOutputArr2(patnr);

                    }



                    for (int i = 0; i < outputlayer; i++)
                    {

                        outputgrad[i] = (output[i] * (1 - output[i])) * (outmatx[i] - output[i]);
                    }


                    for (int i = 0; i < outputlayer; i++)
                    {

                        for (int j = 0; j < hiddenlayer; j++)
                        {

                            weight2delta[j, i] = learningrate * outputgrad[i] * hidden[j];

                            // weight2delta[j, i] = learningrate * (momentum * weight2delta[j, i] + (1.0 - momentum) * outputgrad[i] * hidden[j]);


                        }
                    }

                    //update hidden->output weights


                    for (int i = 0; i < outputlayer; i++)
                    {

                        for (int j = 0; j < hiddenlayer; j++)
                        {

                            weights2[j, i] += weight2delta[j, i];

                        }
                    }

                    //update bias2

                    for (int i = 0; i < outputlayer; i++)
                    {
                        // layerThresholdUpdates[i] = learningRate * (momentum * layerThresholdUpdates[i] + (1.0 - momentum) * error);
                        bias2[i] = learningrate * outputgrad[i] * 1;
                        // bias2[i] = learningrate * (momentum * bias2[i] + (1.0 - momentum) * outputgrad[i]);
                    }



                    //calculate deltas hidden

                    for (int i = 0; i < hiddenlayer; i++)
                    {

                        double weightedSum = 0;

                        for (int k = 0; k < outputlayer; k++)
                            weightedSum += weights2[i, k] * outputgrad[k];

                        //return error gradient
                        hiddengrad[i] = hidden[i] * (1 - hidden[i]) * weightedSum;
                    }


                    for (int i = 0; i < hiddenlayer; i++)
                    {

                        for (int j = 0; j < inputlayer; j++)
                        {

                            weight1delta[j, i] = learningrate * hiddengrad[i] * input[j];
                            // weight1delta[j, i] = learningrate * (momentum * weight1delta[j, i] + (1.0 - momentum) * hiddengrad[i] * input[j]);
                        }
                    }


                    //update hidden->output weights
                    for (int i = 0; i < hiddenlayer; i++)
                    {

                        for (int j = 0; j < inputlayer; j++)
                        {

                            weights1[j, i] += weight1delta[j, i];

                        }
                    }

                    for (int i = 0; i < hiddenlayer; i++)
                    {
                        bias1[i] = learningrate * hiddengrad[i] * 1;
                        // bias1[i] = learningrate * (momentum * bias1[i] + (1.0 - momentum) * hiddengrad[i]);
                    }


                    for (int i = 0; i < outputlayer; i++)
                    {

                        mse += (outmatx[i] - output[i]) * (outmatx[i] - output[i]);
                    }




                }


                mse = mse / training_set_size;
        
                //double a = check10();
                this.Invoke((MethodInvoker)delegate
                {
                    label1.Text = mse.ToString(); // runs on UI thread
                    //label2.Text = a.ToString();

                });

                //label1.Text = mse.ToString();
                //this.Update();
                /*
                if (check10() > 0.9)
                {
                    stopped = true;
                    MessageBox.Show("90%");
                
                }

                 */
            }

   
    

        }

        private double check10(int runs)
        {

            double percent = 0;
            double[] hidden = new double[hiddenlayer];
            double[] output = new double[outputlayer];
            double[] pat;
            char ch;

            for (int ctr = 0; ctr < runs; ctr++)
            {

                //ch = retRandChar(inpslet);
                //pat = getPattern(ch);

                ch = inpslet[ctr % 26];
                pat = getPattern(ch);

                //input->hidden
                for (int i = 0; i < hiddenlayer; i++)
                {

                    hidden[i] = 0;
                    for (int j = 0; j < inputlayer; j++)
                    {

                        hidden[i] += weights1[j, i] * pat[j];

                    }

                    hidden[i] = ActivateFunct(hidden[i] + bias1[i] * 1);

                }
                //hidden->output
                for (int i = 0; i < outputlayer; i++)
                {

                    output[i] = 0;
                    for (int j = 0; j < hiddenlayer; j++)
                    {

                        output[i] += weights2[j, i] * hidden[j];

                    }

                    output[i] = ActivateFunct(output[i] + bias2[i] * 1);

                }

                char w = retCharOfIndex(highests(output));
                if ( w == ch)
                    percent = percent + 1;

            }

            return percent/runs;
        }
        
        private void train_network_click(object sender, EventArgs e)
        {

            stopped = false;
            th = new Thread(train);
            th.Start();

        } 

        private double getHiddenErrorGradient(int i)
        {


            double weightedSum = 0;

            for (int k = 0; k < outputlayer; k++)
                weightedSum += weights2[i,k] * outputgrad[k];

            //return error gradient
            return hidden[i] * (1 - hidden[i]) * weightedSum;


        }

        private void saveWeights(string filename, double[,] weights, int rows, int cols)
        {

            double[,] testv = new double[16, 16];
            string tt = "";



            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    tt += weights[i, j].ToString() + "\n";
                }
            }

          

            File.WriteAllText(filename, tt);
           // "C:\\Users\\Matt\\Desktop\\COUT.txt"

        }

        private double[,] loadWeights(string filename, int rows, int cols)
        {
            double[,] testv = new double[rows, cols];
            string[] load = File.ReadAllLines(filename);

            int ctr = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {

                    testv[i, j] = double.Parse(load[ctr++]);




                }
            }

            return testv;

        }

        private void initialize_network_click(object sender, EventArgs e)
        {
            inputlayer = Int32.Parse(textBox8.Text.ToString());
            hiddenlayer = Int32.Parse(textBox1.Text.ToString());
            outputlayer = Int32.Parse(textBox7.Text.ToString());
            learningrate = Double.Parse(textBox2.Text.ToString());
            momentum = Double.Parse(textBox3.Text.ToString());
            mode = Int32.Parse(textBox6.Text.ToString());


            //init weights1

            for (int i = 0; i < inputlayer; i++)
            {
                for (int j = 0; j < hiddenlayer; j++)
                {
                    weights1[i, j] = r.NextDouble();
                }

            }

            for (int i = 0; i < hiddenlayer; i++)
            {
                for (int j = 0; j < outputlayer; j++)
                {
                    weights2[i, j] = r.NextDouble();
                }

            }



            for (int i = 0; i < inputlayer; i++)
            {
                for (int j = 0; j < hiddenlayer; j++)
                {
                    weight1delta[i, j] = 1;
                }

            }

            //init weights2
            for (int i = 0; i < hiddenlayer; i++)
            {
                for (int j = 0; j < outputlayer; j++)
                {
                    weight2delta[i, j] = 1;
                }

            }

            //init biases

            for (int i = 0; i < hiddenlayer; i++) bias1[i] = 1;

            for (int i = 0; i < outputlayer; i++) bias2[i] = 1;


        } 

        private void save_weights_1_click(object sender, EventArgs e)
        {
            saveWeights("C:\\Users\\Matt\\Desktop\\weights1.txt",weights1,inputlayer,hiddenlayer);
        } 

        private void load_weights_1_click(object sender, EventArgs e)
        {
            weights1 = loadWeights("C:\\Users\\Matt\\Desktop\\weights1.txt",inputlayer,hiddenlayer);
        }

        private void save_weights_2_click(object sender, EventArgs e)
        {
            saveWeights("C:\\Users\\Matt\\Desktop\\weights2.txt", weights2, hiddenlayer, outputlayer);
        }

        private void load_weights_2_click(object sender, EventArgs e)
        {
            weights2 = loadWeights("C:\\Users\\Matt\\Desktop\\weights2.txt", hiddenlayer, outputlayer);
        }

        private void predict_letter_click(object sender, EventArgs e)
        {


            double[] pat = getParretn4();
            // pat = getParretn4();

            double[] hidden = new double[hiddenlayer];
            double[] output = new double[outputlayer];

            //input->hidden
            for (int i = 0; i < hiddenlayer; i++)
            {

                hidden[i] = 0;
                for (int j = 0; j < inputlayer; j++)
                {

                    hidden[i] += weights1[j, i] * pat[j];

                }

                hidden[i] = ActivateFunct(hidden[i] + bias1[i] * 1);

            }
            //hidden->output
            for (int i = 0; i < outputlayer; i++)
            {

                output[i] = 0;
                for (int j = 0; j < hiddenlayer; j++)
                {

                    output[i] += weights2[j, i] * hidden[j];

                }

                output[i] = ActivateFunct(output[i] + bias2[i] * 1);

            }



            highests(output);

            label3.Text = "#1: " + retCharOfIndex(indx1).ToString() + " ("+ indx1_val*100 +"%)\n" +
                          "#2: " + retCharOfIndex(indx2).ToString() + " (" + indx2_val * 100 + "%)\n" +
                          "#3: " + retCharOfIndex(indx3).ToString() + " ("+ indx3_val*100 + "%)\n";
            this.Update();

           // mse = 0;



        } 

        private void load_image_click(object sender, EventArgs e)
        {


            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "C:\\Users\\Matt\\Documents\\visual studio 2010\\Projects\\MyOcrLearn\\MyOcrLearn\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            // Insert code to read the stream here.

                            setNewImage(openFileDialog1.FileName);


                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }


        }

        private void stop_train_click(object sender, EventArgs e)
        {
            stopped = true;
            th.Abort();
        }

        private void check_click(object sender, EventArgs e)
        {
            int runs = Int32.Parse(textBox4.Text.ToString());
            double a = check10(runs);
            label4.Text = Double.Parse(a.ToString())*100+"% accuracy";
        }
       
        private void drawAxix()
        {




            formGraphics = this.CreateGraphics();
            formGraphics.Clear(SystemColors.Control);





            myPen = new System.Drawing.Pen(System.Drawing.Color.Black);

            myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);


            formGraphics.FillRectangle(myBrush, originx-100, originy -scale*13, size+200, size+200);


            formGraphics.DrawLine(myPen, new Point(originx, originy), new Point(originx + size, originy));
            formGraphics.DrawLine(myPen, new Point(originx, originy), new Point(originx, originy - size));

            string drawString = "";
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 12);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);



            for (int i = 1; i <= 10; i++)
            {
                drawString = ((zoom * i)/10).ToString();
                formGraphics.DrawEllipse(myPen, originx + i * scale - 2, originy - 2, 4, 4);
                formGraphics.DrawEllipse(myPen, originx - 2, originy - i * scale - 2, 4, 4);
                formGraphics.DrawString(drawString, drawFont, drawBrush, originx + i * scale - 5, originy + 10);
                formGraphics.DrawString(drawString, drawFont, drawBrush, originx - 2 - 30, originy - i * scale - 10);
                currentx = zoom * i;
                currenty = zoom * i;

            }

    
        }

        //0-1 input
        private void drawPoint(float x, float y)
        {

            //lets say want to draw point at 1,1


            formGraphics.DrawEllipse(myPen, (float)originx+x*scale*10,(float)originy-y*scale*10,(float)10,(float)10);




        }

        private void draw_graph_click(object sender, EventArgs e)
        {

   

            xsize = Int32.Parse(textBox5.Text.ToString());

        

            drawAxix();

            xi = new float[xsize];
            yi = new float[xsize];
        



        }

        private void button6_Click(object sender, EventArgs e)
        {
            string patt = textBox9.Text.ToString();

            double f = Double.Parse(patt[0].ToString());
            double s = Double.Parse(patt[1].ToString());

           // int f = (int)patt[0]; //outs int rep of char (49 for 1) ascii
          //  int s = (int)patt[1];


            double[] pat =  new double[] { f, s };
            // pat = getParretn4();

            double[] hidden = new double[hiddenlayer];
            double[] output = new double[outputlayer];

            //input->hidden
            for (int i = 0; i < hiddenlayer; i++)
            {

                hidden[i] = 0;
                for (int j = 0; j < inputlayer; j++)
                {

                    hidden[i] += weights1[j, i] * pat[j];

                }

                hidden[i] = ActivateFunct(hidden[i] + bias1[i] * 1);

            }
            //hidden->output
            for (int i = 0; i < outputlayer; i++)
            {

                output[i] = 0;
                for (int j = 0; j < hiddenlayer; j++)
                {

                    output[i] += weights2[j, i] * hidden[j];

                }

                output[i] = ActivateFunct(output[i] + bias2[i] * 1);

            }



            MessageBox.Show(output[0].ToString());





        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void linear_regression_click(object sender, EventArgs e)
        {



            point_ctr = 0;

            //float[] xi = { 0.25f, 1f, 0.28f, 0.87f, 0.99f, 0.5f, 0.7f };
            //float[] yi = { 0.5f, 0.5f, 0.8f, 0.24f, 0.2f, 0.6f, 0.45f };

           

            //y=mx+b <=> y = w[0]x + w[1]
            float[] w = new float[2];

            for (int i = 0; i < w.Length; i++) w[i] = 0;

            float learningrate = 0.1f;
            float p = 0;


            //1500 updates
            for (int i = 0; i <Int32.Parse(textBox10.Text.ToString()); i++)
            {

                //if (i % 100 == 0) MessageBox.Show(cost_fn(xi, yi, w).ToString());

                for (int j = 0; j < xi.Length; j++)
                {
                    //per each weight
                    for (int k = 0; k < w.Length; k++)
                    {

                        w[k] = w[k] + learningrate * (yi[j] - eval_hyp(xi[j], w));// *xi[j];


                    }
                }

            }

            //MessageBox.Show(w[0] + "x + " + w[1]);

            for (double i = 0; i < 1; i+=0.01)
            {


                 drawPoint((float)i,eval_hyp((float)i,w));


            }

            MessageBox.Show(cost_fn(xi, yi, w).ToString());


        }

        private float cost_fn(float[] x, float[] y, float[] w)
        {

            float res = 0;

            for (int i = 0; i < x.Length; i++)
            {

                res += (float)Math.Pow((double)(eval_hyp(x[i], w) - y[i]), (double)2);

            }

            return (float)(res*0.5);

        }
     
        private float eval_hyp(float x, float[] w)
        {

            //mx+b <= hypothesis (linear)
            return (w[0] * x) + w[1];


        }


        private float eval_hyp2(float x, float[] w)
        {

            //mx+b <= hypothesis (linear)
            return (w[0] * (x*x)) + (w[1] * x) + w[2];


        }
       
        
        private float cost_fn2(float[] x, float[] y, float[] w)
        {

            float res = 0;

            for (int i = 0; i < x.Length; i++)
            {

                res += (float)Math.Pow((double)(eval_hyp2(x[i], w) - y[i]), (double)2);

            }

            return (float)(res * 0.5);

        }
       

        private void TestingNeurons_Load(object sender, EventArgs e)
        {

        }

        private void classify_click(object sender, EventArgs e)
        {




        }

        private void button15_Click(object sender, EventArgs e)
        {

            point_ctr = 0;
            float[] w = new float[3];

            for (int i = 0; i < w.Length; i++) w[i] = 0;

            float learningrate = 0.1f;
          


            //1500 updates
            for (int i = 0; i < Int32.Parse(textBox10.Text.ToString()); i++)
            {

                //if (i % 100 == 0) MessageBox.Show(cost_fn(xi, yi, w).ToString());

                for (int j = 0; j < xi.Length; j++)
                {
                    //per each weight
                    for (int k = 0; k < w.Length; k++)
                    {

                        w[k] = w[k] + learningrate * (yi[j] - eval_hyp2(xi[j], w));// *xi[j];


                    }
                }

            }

            //MessageBox.Show(w[0] + "x + " + w[1]);

            for (double i = 0; i < 1; i += 0.01)
            {


                drawPoint((float)i, eval_hyp2((float)i, w));


            }

            MessageBox.Show(cost_fn2(xi, yi, w).ToString());


        }
    }
}

