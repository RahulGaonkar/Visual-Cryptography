using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.IO;
using System.Collections;

namespace VCPasswordnew
{
    public partial class VisualCryptography : Form
    {
        private Bitmap bmp = null;

        private string FileCheck_ExtractTxt = string.Empty;


        public struct Coord
        {
            public int x, y;
        }
        public VisualCryptography()
        {
            InitializeComponent();
        }

        private void Work(Bitmap srcb, Bitmap outb, String Data)
        {


            // this is a standard mixing code; this is based on Microsoft demo code to
            // X-Or shift a given array of signal/image
            // only criteris/requirement is that the signal should be 2D array
            // there exist similar function for  3D files like video (frame of X-Y dimension running in time, the third dimension)

            int w = srcb.Width, h = srcb.Height;
            string Msg = Data;

            Graphics gr = Graphics.FromImage(outb);

            // conver the word "Demo" to picture (or any other word)
            Font f = new Font("Arial", 100, FontStyle.Bold);
            var size = gr.MeasureString(Msg, f);
            f = new Font("Arial", w / size.Width * 110, FontStyle.Bold);
            size = gr.MeasureString(Msg, f);
            gr.DrawString(Msg, f, new SolidBrush(Color.White), (w - size.Width) / 2, (h - size.Height) / 2);

            gr.Dispose();



            // below is a double loop to X-OR shift top pixels randomly to bottom
            // and bottom pixels randomly to top.

            Coord[] coord = new Coord[w * h];
            FastBitmap fsb = new FastBitmap(srcb);
            FastBitmap fob = new FastBitmap(outb);
            fsb.LockImage();
            fob.LockImage();
            ulong seed = 1;
            int numpix = h * w;
            int c1 = 0, c2 = numpix;
            int y2 = h / 2;

            int p2 = numpix / 2;

            for (int p = 0; p < p2; p++)
            {
                for (int s = 1; s > -2; s -= 2)
                {
                    int y = (p2 + s * p) / w;
                    int x = (p2 + s * p) % w;

                    uint d = fob.GetPixel(x, y);
                    if (d != 0)
                    {
                        c2--;
                        coord[c2].x = x;
                        coord[c2].y = y;
                    }
                    else
                    {
                        coord[c1].x = x;
                        coord[c1].y = y;
                        c1++;
                    }
                    fob.SetPixel(x, y, fsb.GetPixel(x, y));
                }
            }
            fsb.UnlockImage();
            fob.UnlockImage();
            pbOutput.Refresh();
            Application.DoEvents();





            int half = numpix / 2;
            int limit = half;
            XorShift rng = new XorShift(seed);
            progressBar.Visible = true;
            progressBar.Maximum = limit;

            fob.LockImage();
            while (limit > 0)
            {
                int p = (int)(rng.next() % (uint)limit);
                int q = (int)(rng.next() % (uint)limit);
                uint color = fob.GetPixel(coord[p].x, coord[p].y);
                fob.SetPixel(coord[p].x, coord[p].y, fob.GetPixel(coord[half + q].x, coord[half + q].y));
                fob.SetPixel(coord[half + q].x, coord[half + q].y, color);
                limit--;
                if (p < limit)
                {
                    coord[p] = coord[limit];
                }
                if (q < limit)
                {
                    coord[half + q] = coord[half + limit];
                }
                if ((limit & 0xfff) == 0)
                {
                    progressBar.Value = limit;
                    fob.UnlockImage();
                    pbOutput.Refresh();
                    fob.LockImage();
                }
            }
            fob.UnlockImage();
            pbOutput.Refresh();
            progressBar.Visible = false;
        }

        void DupImage(PictureBox s, PictureBox d)
        {
            if (d.Image != null)
                d.Image.Dispose();
            d.Image = new Bitmap(s.Image.Width, s.Image.Height);
        }

        void GetImagePB(PictureBox pb, string file)
        {
            // get the picture box to memory for further processing
            // images cannot be processed in a container like picturebox
            // they are processed in memory using unsafe bitmap method
            // details here http://davidthomasbernal.com/blog/2008/03/13/c-image-processing-performance-unsafe-vs-safe-code-part-i/
            // more technical details here http://bobpowell.net/lockingbits.aspx

            Bitmap bms = new Bitmap(file, false);
            Bitmap bmp = bms.Clone(new Rectangle(0, 0, bms.Width, bms.Height), PixelFormat.Format32bppArgb);
            bms.Dispose();
            if (pb.Image != null)
                pb.Image.Dispose();
            pb.Image = bmp;
        }

        void Split(string SourceFile, string Target1, string Target2)
        {

            // just a demo function showing a SplitJoiner uses code to split a file.
            // demo is just for two parts. THIS FUNCTION IS NEVER ACTUALLY CALLED. SPLIT-JOINER 
            // IS USED, IT IS SIMILAR BUT FAR MORE COMPLEX.

            // first half
            Bitmap originalImage = new Bitmap(Image.FromFile(SourceFile));
            Rectangle rect = new Rectangle(0, 0, originalImage.Width / 2, originalImage.Height);
            Bitmap firstHalf = originalImage.Clone(rect, originalImage.PixelFormat);
            firstHalf.Save(Target1);

            // second half
            rect = new Rectangle(originalImage.Width / 2, 0, originalImage.Width / 2, originalImage.Height);
            Bitmap secondHalf = originalImage.Clone(rect, originalImage.PixelFormat);
            secondHalf.Save(Target2);
            return;
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            var random = new Random(); // generate a random number, for encryption

            // open a file-dialog box and let user select a file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\temp\\";
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            // if user selects a file ie DialogResult.OK, go ahead; user can cancel using 
            // DialogResult.Cancel
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    string file0, file1;
                    string file2, file3;
                    string file = openFileDialog.FileName;

                    // get the file and put it in picture box.
                    GetImagePB(pbInput, file);

                    // use the picture box bmp (bit map) for further processing.
                    bmp = (Bitmap)pbInput.Image;


                    // SplitJoin.SplitJoinHelper is standard dll for processing 
                    // file; to check file types, split a file and join a file.
                    string FileCheck_ExtractTxt = "";
                    FileCheck_ExtractTxt = SplitJoin.SplitJoinHelper.extractInfo(bmp);


                    // check if its a png file; and already processed
                    // this dll when processing png file puts a "png" header
                    // we use this to our advantage, to conclude we have previously processed this file.
                    if (FileCheck_ExtractTxt.Substring(0, 3) == "png")
                    {
                        MessageBox.Show("Already processed, or invalid file.");
                        return;
                    }


                    // create a duplicate of an image and process it/randomize it.
                    pbInput.Tag = file; //origional file
                    DupImage(pbInput, pbOutput); // create duplicate (same as holding a integer variable data in to a temp variable
                    Work(pbInput.Image as Bitmap, pbOutput.Image as Bitmap, txtEncryptedText.Text); // process the file (randomize)

                    bmp = (Bitmap)pbOutput.Image;
                    bmp = SplitJoin.SplitJoinHelper.embedInfo("png" + random.Next(1, 1000).ToString(), bmp);


                    ////////////////// below code is redundant; but dont delete  ////////////
                    file1 = file;

                    pbInput.Tag = file;
                    DupImage(pbInput, pbOutput);
                    Work(pbInput.Image as Bitmap, pbOutput.Image as Bitmap, txtEncryptedText.Text);

                    bmp = (Bitmap)pbOutput.Image;
                    bmp = SplitJoin.SplitJoinHelper.embedInfo("png" + random.Next(1, 1000).ToString(), bmp);

                    /////////////////  end of redundant code /////////////////////////////////



                    // use the standard file to just split the file in to multiple parts.
                    // first is input, others are output.
                    // this is essential; unlike a simple text file; which can be easily broken into 'n; parts
                    // a media (video/audio) file has header, metadata and other info; hence you cant use a 
                    // simple loop to split a file. Even if you do it; image will be permanently broken.
                    // to test, open a small jpg file in notepad; edit; save and re-open. :)
                    // THIS FUNCTION IS MODIFIED TO SPLIT IN TO EQUAL PART SO AS EACH PART RETAINS ORIGIONAL DATA
                    // THIS ENSURE HACKER DOES NOT KNOW IF PROCESSED FILE IS 1/4 ORIGIONAL

                    SplitJoin.SplitJoinHelper.GetProcessedFile(file, out file0, out file2, out file3, out file);


                    // save to hard-disk.
                    pbOutput.Image.Save(file0);
                    pbOutput.Image.Save(file2);
                    pbOutput.Image.Save(file3);
                    pbOutput.Image.Save(file);

                    MessageBox.Show("Processing successful.");


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }

            }

        }

        private void btnMerge_Click_1(object sender, EventArgs e)
        {

            var random = new Random();



            // open the FileOpen dialog box
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\temp\\";
            openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            // allow multi-select this time
            openFileDialog.Multiselect = true;


            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // user has to select exactly four file
                    if (openFileDialog.SafeFileNames.Count() != 4)
                    {
                        MessageBox.Show("Only four file can be selected.");
                        return;

                    }

                    // variable to hold data
                    string FileCheck_ExtractTxt0 = "";
                    string FileCheck_ExtractTxt1 = "";
                    string FileCheck_ExtractTxt2 = "";
                    string FileCheck_ExtractTxt3 = "";

                    string file0, file1, file2, file3;

                    // get all the four files that user has selected
                    file0 = openFileDialog.FileNames[0];
                    file1 = openFileDialog.FileNames[1];
                    file2 = openFileDialog.FileNames[2];
                    file3 = openFileDialog.FileNames[3];


                    // get all the files
                    GetImagePB(pbInput, file0);
                    bmp = (Bitmap)pbInput.Image;
                    FileCheck_ExtractTxt0 = SplitJoin.SplitJoinHelper.extractInfo(bmp);

                    GetImagePB(pbInput, file1);
                    bmp = (Bitmap)pbInput.Image;
                    FileCheck_ExtractTxt1 = SplitJoin.SplitJoinHelper.extractInfo(bmp);

                    GetImagePB(pbInput, file2);
                    bmp = (Bitmap)pbInput.Image;
                    FileCheck_ExtractTxt2 = SplitJoin.SplitJoinHelper.extractInfo(bmp);

                    GetImagePB(pbInput, file3);
                    bmp = (Bitmap)pbInput.Image;
                    FileCheck_ExtractTxt3 = SplitJoin.SplitJoinHelper.extractInfo(bmp);

                    // ensure they are correct file type
                    if (FileCheck_ExtractTxt0.Substring(0, 3) != "png")
                    {
                        MessageBox.Show("Never processed, or invalid file.");
                        return;
                    }
                    if (FileCheck_ExtractTxt1.Substring(0, 3) != "png")
                    {
                        MessageBox.Show("Never processed, or invalid file.");
                        return;
                    }
                    if (FileCheck_ExtractTxt2.Substring(0, 3) != "png")
                    {
                        MessageBox.Show("Never processed, or invalid file.");
                        return;
                    }
                    if (FileCheck_ExtractTxt3.Substring(0, 3) != "png")
                    {
                        MessageBox.Show("Never processed, or invalid file.");
                        return;
                    }


                    // check if they are correct Quad-pair
                    if (FileCheck_ExtractTxt0 != FileCheck_ExtractTxt1)
                    {
                        MessageBox.Show("Invalid pair file.");
                        return;
                    }
                    if (FileCheck_ExtractTxt1 != FileCheck_ExtractTxt2)
                    {
                        MessageBox.Show("Invalid pair file.");
                        return;
                    }

                    if (FileCheck_ExtractTxt2 != FileCheck_ExtractTxt3)
                    {
                        MessageBox.Show("Invalid pair file.");
                        return;
                    }

                    // process in reverse
                    pbInput.Tag = file0;
                    DupImage(pbInput, pbOutput);
                    Work(pbInput.Image as Bitmap, pbOutput.Image as Bitmap, txtEncryptedText.Text);

                    // save and close
                    file1 = file0;
                    file0 = Path.GetDirectoryName(file0) + Path.DirectorySeparatorChar + (Path.GetFileNameWithoutExtension(file0).Replace(".scr0", "")).Replace(".scr1", "") + ".join.png";
                    pbOutput.Image.Save(file0);

                    MessageBox.Show("Processing successful.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }

            }
        }
     
      
        private void btnMerge_Click(object sender, EventArgs e)
        {


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            About ss = new About();
            ss.Show();
        }


       
    }
    // Standard Fast bitmap calls and
    // X-OR functions
    unsafe public class FastBitmap
    {
        private Bitmap workingBitmap = null;
        private int width = 0;
        private BitmapData bitmapData = null;
        private Byte* pBase = null;

        public FastBitmap(Bitmap inputBitmap)
        {
            workingBitmap = inputBitmap;
        }

        public BitmapData LockImage()
        {
            Rectangle bounds = new Rectangle(Point.Empty, workingBitmap.Size);

            width = (int)(bounds.Width * 4 + 3) & ~3;

            //Lock Image
            bitmapData = workingBitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            pBase = (Byte*)bitmapData.Scan0.ToPointer();
            return bitmapData;
        }

        private uint* pixelData = null;

        public uint GetPixel(int x, int y)
        {
            pixelData = (uint*)(pBase + y * width + x * 4);
            return *pixelData;
        }

        public uint GetNextPixel()
        {
            return *++pixelData;
        }

        public void GetPixelArray(int x, int y, uint[] Values, int offset, int count)
        {
            pixelData = (uint*)(pBase + y * width + x * 4);
            while (count-- > 0)
            {
                Values[offset++] = *pixelData++;
            }
        }

        public void SetPixel(int x, int y, uint color)
        {
            pixelData = (uint*)(pBase + y * width + x * 4);
            *pixelData = color;
        }

        public void SetNextPixel(uint color)
        {
            *++pixelData = color;
        }

        public void UnlockImage()
        {
            workingBitmap.UnlockBits(bitmapData);
            bitmapData = null;
            pBase = null;
        }
    }

    public class XorShift
    {
        private ulong x; /* The state must be seeded with a nonzero value. */

        public XorShift(ulong seed)
        {
            x = seed;
        }

        public ulong next()
        {
            x ^= x >> 12; // a
            x ^= x << 25; // b
            x ^= x >> 27; // c
            return x * 2685821657736338717L;
        }



    }

}
