using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public class FacialRecognition
{
    #region Singleton Implementation
    private static volatile FacialRecognition _instance;

    private static object syncRoot = new Object();

    private FacialRecognition()
    {
        libFace = new AT_Facial_API.Library();
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
        }
        Debug.WriteLine("Face_QualityScore:" + Face_QualityScore + ", Matching_Score:" + Matching_Score);
    }


    public void StartFacialRecognition(Point formLocation, List<byte[]> FaceJpg)
    {
        _failedCount = 0;
        _suceededCount = 0;
        this.FaceJpg = (FaceJpg != null ? FaceJpg : new List<byte[]>()).Where(d => d != null).ToList();
        if (libFace != null && !isStartTracking && this.FaceJpg.Count > 0)
        {
            isStartTracking = true;
            libFace.Init();

            libFace.Show_Window(formLocation, new Size(400, 400));
            OnFacialRecognitionProcessing();
            //Thread.Sleep(1000);
            libFace.FaceDetect += new AT_Facial_API.Library.FaceDetected(lib_FaceDetect);
            libFace.StartTracking();
            
            try
            {
                libFace.Photo_JPG = this.FaceJpg[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (this.FaceJpg.Count > 1)
                {
                    libFace.Photo_JPG = this.FaceJpg[1];
                }
            }
            
            this.FaceJpg.RemoveAt(0);
        }
        else if (!isStartTracking && this.FaceJpg.Count == 0)
        {
            OnFacialRecognitionFailed();
        }

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

