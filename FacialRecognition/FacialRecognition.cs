using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;

public class FacialRecognition
{
    #region Singleton Implementation
    private static volatile FacialRecognition _instance;

    private static object syncRoot = new Object();

    private FacialRecognition()
    {
        libFace = new AT_Facial_API.Library();
        libFace.FaceDetect += new AT_Facial_API.Library.FaceDetected(lib_FaceDetect);
        //formAvarta = new Form();
        //formAvarta.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        //formAvarta.StartPosition = FormStartPosition.Manual;
    }

    public static FacialRecognition Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                        _instance = new FacialRecognition();
                }
            }
            return _instance;
        }
    }
    #endregion


    private AT_Facial_API.Library libFace = null;
    private bool isStartTracking = false;
    private Form formAvarta = null;

    [DllImport("User32.dll")]
    static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("User32.dll")]
    static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const UInt32 SWP_NOSIZE = 0x0001;
    const UInt32 SWP_NOMOVE = 0x0002;
    const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

    /// <summary>
    /// int Matching Score 
    /// Matching Score > 2800 Face Detect Success
    /// </summary>
    public event Action FaceDetectSucceeded;
    public event Action FaceDetectFailed;
    private int MatchingScore = 2800;
    private int _faceDetectSucceeded = 0;
    private int _faceDetectFailed = 0;
    private List<byte[]> FaceJpg = new List<byte[]>();


    void lib_FaceDetect(byte[] FaceJpg, int Face_QualityScore, int Matching_Score, int ISO_Quality_ERROR)
    {
        if (isStartTracking && libFace != null && libFace.Photo_JPG != null)
        {
            if (MatchingScore >= Matching_Score)
                _faceDetectSucceeded++;
            else
                _faceDetectFailed++;
            if (_faceDetectSucceeded == 2 || _faceDetectFailed == 2)
            {
                if (_faceDetectSucceeded == 2 && FaceDetectSucceeded != null)
                {
                    FaceDetectSucceeded();
                    Dispose();
                }
                else if (_faceDetectFailed == 2 && FaceDetectFailed != null)
                {
                    if (this.FaceJpg.Count>0)
                    {
                        libFace.StopTracking();
                        libFace.Photo_JPG = this.FaceJpg[0];
                        libFace.StartTracking();
                        this.FaceJpg.RemoveAt(0);
                    }
                    else
                    {
                        FaceDetectFailed();
                        Dispose();
                    }
                }
                _faceDetectSucceeded = 0;
                _faceDetectFailed = 0;
            }
        }
    }



    public void Compare(List<byte[]> FaceJpg)
    {
        this.FaceJpg = (FaceJpg != null ? FaceJpg : new List<byte[]>()).Where(d=>d!=null).ToList();
        if (libFace != null && !isStartTracking && this.FaceJpg.Count>0)
        {

            isStartTracking = true;
            libFace.Init();
            //libFace.Show_Window(new System.Drawing.Point(0, 0), System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size);
            libFace.Show_Window(new System.Drawing.Point(0, 0), new Size(200,200));
            libFace.StartTracking();
            libFace.Photo_JPG = this.FaceJpg[0];
            this.FaceJpg.RemoveAt(0);
        }

    }
    public void Compare(byte[] FaceJpg)
    {
        Compare(new List<byte[]>() { FaceJpg });
    }
    private Bitmap CreateBitmapFromByte(byte[] FaceJpg)
    {
        Bitmap bmp;
        using (var ms = new MemoryStream(FaceJpg))
        {
            bmp = new Bitmap(ms);
        }
        return bmp;
    }
    //private void ThreadCompare(object _FaceJpg)
    //{
    //    byte[] FaceJpg = (byte[])_FaceJpg;
    //    if (libFace != null && !isStartTracking)
    //    {
    //        isStartTracking = true;
    //        libFace.Init();
    //        libFace.Show_Window(new System.Drawing.Point(0, 0), System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size);
    //    }

    //    libFace.StartTracking();
    //    libFace.Photo_JPG = FaceJpg;
    //    var img = CreateBitmapFromByte(FaceJpg);
    //    formAvarta.Width = img.Width;
    //    formAvarta.Height = img.Height;
    //    formAvarta.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - formAvarta.Width,
    //                           Screen.PrimaryScreen.WorkingArea.Height - formAvarta.Height);
    //    formAvarta.BackgroundImage = Image.FromHbitmap(img.GetHbitmap());
    //    formAvarta.Refresh();
    //    formAvarta.Show();
    //    System.Threading.Thread.Sleep(100);
    //    SetWindowPos(formAvarta.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
    //}


    public void Dispose()
    {
        if (isStartTracking && libFace != null)
        {
            //formAvarta.Hide();
            libFace.Close_Window();
            libFace.StopTracking();
            libFace.Photo_JPG = null;
            libFace.Deinit();
            _faceDetectSucceeded = 0;
            _faceDetectFailed = 0;
        }
        isStartTracking = false;
    }

}

