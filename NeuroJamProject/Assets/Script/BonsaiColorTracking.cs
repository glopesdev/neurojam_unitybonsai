using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Expression = System.Linq.Expressions.Expression;
using Bonsai;
using Bonsai.Expressions;
using UnityEngine.UI;
using Bonsai.Dag;
using System.Linq;
using OpenCV.Net;
using Bonsai.Vision;
using System.IO;
using Bonsai.Design;
using System.ComponentModel;

public class BonsaiColorTracking : MonoBehaviour
{

    /// <summary>
    /// Path of the workflow we pretend to use.
    /// </summary>
    public string fileName;

    /// <summary>
    /// WorkflowBuilder of the bonsai Workflow being used.
    /// </summary>
    private WorkflowBuilder workflowBuilder;

    /// <summary>
    /// Expression that contains the whole graph of the bonsai workflow.
    /// </summary>
    private ExpressionBuilderGraph workflowBuilderGraph;


    /// <summary>
    /// Data of the image in bonsai.
    /// </summary>
    private IplImage imageData;

    /// <summary>
    /// Texture that will apply the image from bonsai to unity.
    /// </summary>
    private Texture2D tex;

    /// <summary>
    /// Data of the image in bytes to map in unity as texture. Can be used with RenderViewport too.
    /// </summary>
    private byte[] managedArray;

    /// <summary>
    /// Start camera only if we received the first frame.
    /// </summary>
    private bool startCam = false;

    /// <summary>
    /// Texture Format used in unity that is compatible with the same texture format as Bonsai.
    /// </summary>
    public TextureFormat texFormat;


    /// <summary>
    /// Adjust the value if they are too big.
    /// </summary>
    public float valueAdjust = 10f;


    /// <summary>
    /// Invert the sides in X.
    /// </summary>
    public int invertX = -1;

    /// <summary>
    /// Invert the sides in Y.
    /// </summary>
    public int invertY = -1;

    /// <summary>
    /// Image that will receive the Bonsai Image.
    /// </summary>
    public RawImage BonsaiImage;

    //Respective Cubes

    public GameObject RedCube;

    public GameObject GreenCube;

    public GameObject BlueCube;


    //Save the values that from de colors in bonsai

    private Vector3 RedPos;

    private Vector3 GreenPos;

    private Vector3 BluePos;


    /// <summary>
    /// Disposable of the image data subscription.
    /// </summary>
    private IDisposable imgDetectedSubscription;

    /// <summary>
    /// Disposable of the cubes data subscription.
    /// </summary>
    private IDisposable CubesSubscription;


    void Awake()
    {
        // Init unity thread to be called in any thread.
        UnityThread.initUnityThread();
    }

    // Use this for initialization
    void Start()
    {
        // This will load our workflow.
        LoadInputWorkflow(fileName);

        // This will create a texture from the size of the camera used in Bonsai.
        tex = new Texture2D(640, 480, texFormat, false);


    }




    // This will load the the workflow and start the subscription to what ever elements we want.
    private void LoadInputWorkflow(string filename)
    {
        using (var reader = XmlReader.Create(filename))
        {
            var serializer = new XmlSerializer(typeof(WorkflowBuilder));
            workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);


       
            var workflow = workflowBuilder.Workflow.Build();
            var observableFactory = Expression.Lambda<Func<IObservable<Tuple<IplImage, Point2f, Point2f, Point2f>>>>(workflow).Compile();




            imgDetectedSubscription = observableFactory().Subscribe(x => updateImage(x.Item1));

            CubesSubscription = observableFactory().Subscribe(x => updateCubesPositions(x.Item2, x.Item3, x.Item4));

            workflowBuilderGraph = workflowBuilder.Workflow;



        }
    }

    // Update the positions of the cubes and run them in the main thread using UnityThread.
    public void updateCubesPositions(Point2f Gpos, Point2f Rpos, Point2f Bpos)
    {
        if (!float.IsNaN(Gpos.X))
        {

            GreenPos = new Vector3((Gpos.X * invertX) / valueAdjust, (Gpos.Y * invertY) / valueAdjust, 0);
        }

        if (!float.IsNaN(Rpos.X))
        {

            RedPos = new Vector3((Rpos.X * invertX) / valueAdjust, (Rpos.Y * invertY) / valueAdjust, 0);
        }

        if (!float.IsNaN(Bpos.X))
        {

            BluePos = new Vector3((Bpos.X * invertX) / valueAdjust, (Bpos.Y * invertY) / valueAdjust, 0);
        }

        Action PosUpdate = UpdateCubesPos;

        UnityThread.executeInUpdate(PosUpdate);

       
    }

    // Action that will be run in the main thread.
    void UpdateCubesPos()
    {
        GreenCube.transform.position = GreenPos;
        RedCube.transform.position = RedPos;
        BlueCube.transform.position = BluePos;
    }


    // This will change the image data every time it receives a frame from Bonsai since its subscribed.
    private void updateImage(IplImage min)
    {
        ImageArea = min;

        var size = ImageArea.Size.Height * ImageArea.Size.Width * ImageArea.Channels;


         
        managedArray = new byte[size];
        Marshal.Copy(ImageArea.ImageData, managedArray, 0, size);

        if (ImageArea != null)
        {
            startCam = true;
        }

    }


    // Dispose all subscriptions in application quit.
    private void OnApplicationQuit()
    {
        Dispose();
    }


    // Dispose all subscriptions.
    public void Dispose()
    {
        if (imgDetectedSubscription != null)
            imgDetectedSubscription.Dispose();

        if (CubesSubscription != null)
            CubesSubscription.Dispose();
        
    }


    // Update the image value.
    public IplImage ImageArea
    {
        get
        {
            return imageData;
        }
        set
        {
            if (value != imageData)
            {
                imageData = value;


            }
        }
    }



    // Update is called once per frame
    void Update()
    {

        if (startCam)
        {
            // Update the texter from the data received since the data can't be changed without being in the mainThread of unity.
            tex.LoadRawTextureData(managedArray);
            tex.Apply();
            BonsaiImage.texture = tex;
        }


    }
}
