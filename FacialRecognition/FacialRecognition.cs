using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using System.Threading;

public class FacialRecognition
{
    #region Singleton Implementation
    private static volatile FacialRecognition _instance;

    private static object syncRoot = new Object();

    private FacialRecognition()
    {
        libFace = new AT_Facial_API.Library();
        
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

    private static int _failedCount = 0;
    private static int _suceededCount = 0;
    private AT_Facial_API.Library libFace = null;
    private bool isStartTracking = false;

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
    public event Action OnFacialRecognitionSucceeded;
    public event Action OnFacialRecognitionFailed;
    public event Action OnFacialRecognitionProcessing;
    private int _matchingScore = 2800;
    private List<byte[]> FaceJpg = new List<byte[]>();


    void lib_FaceDetect(byte[] FaceJpg, int Face_QualityScore, int Matching_Score, int ISO_Quality_ERROR)
    {
        if (isStartTracking && libFace != null && libFace.Photo_JPG != null)
        {
            if (Matching_Score >= _matchingScore)
            {
                _suceededCount++;
            }
            else
            {
                _failedCount++;
            }
            if (_suceededCount > 1)
            {
                Thread.Sleep(1000);
                OnFacialRecognitionSucceeded();
            }
            else
            {
                if (_failedCount > 10)
                {
                    OnFacialRecognitionFailed();
                }
            }
            //if (_faceDetectSucceeded == 2 || _faceDetectFailed == 2)
            //{
            //    if (_faceDetectSucceeded == 2 && FaceDetectSucceeded != null)
            //    {
            //        FaceDetectSucceeded();
            //    }
            //    else if (_faceDetectFailed == 2 && FaceDetectFailed != null)
            //    {
            //        if (this.FaceJpg.Count > 0)
            //        {
            //            libFace.StopTracking();
            //            libFace.Photo_JPG = this.FaceJpg[0];
            //            libFace.StartTracking();
            //            this.FaceJpg.RemoveAt(0);
            //        }
            //        else
            //        {
            //            FaceDetectFailed();
            //        }
            //    }
            //    _faceDetectSucceeded = 0;
            //    _faceDetectFailed = 0;
            //}
        }
        Debug.WriteLine("Face_QualityScore:" + Face_QualityScore + ", Matching_Score:" + Matching_Score);
    }


    public void StartFacialRecognition(List<byte[]> FaceJpg)
    {
        _failedCount = 0;
        _suceededCount = 0;
        this.FaceJpg = (FaceJpg != null ? FaceJpg : new List<byte[]>()).Where(d => d != null).ToList();
        if (libFace != null && !isStartTracking && this.FaceJpg.Count > 0)
        {
            isStartTracking = true;
            libFace.Init();
            //libFace.Show_Window(new System.Drawing.Point(0, 0), System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size);

            libFace.Show_Window(new System.Drawing.Point(100, 100), new Size(400, 400));
            OnFacialRecognitionProcessing();
            Thread.Sleep(1000);
            libFace.FaceDetect += new AT_Facial_API.Library.FaceDetected(lib_FaceDetect);
            libFace.StartTracking();
            libFace.Photo_JPG = this.FaceJpg[0];
            this.FaceJpg.RemoveAt(0);
        }
        else if (!isStartTracking && this.FaceJpg.Count == 0)
        {
            OnFacialRecognitionFailed();
        }

    }
    //public void Compare(byte[] FaceJpg)
    //{
    //    Compare(new List<byte[]>() { FaceJpg });
    //}
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
            libFace.StopTracking();
            libFace.Close_Window();
            libFace.Photo_JPG = null;
            libFace.Deinit();
            _failedCount = 0;
            _failedCount = 0;
        }
        isStartTracking = false;
    }
}

