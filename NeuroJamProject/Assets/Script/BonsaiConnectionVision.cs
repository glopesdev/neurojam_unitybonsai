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

public class BonsaiConnectionVision : MonoBehaviour {

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
    /// Slider that will change the values of the Threshold in Bonsai.
    /// </summary>
    public UnityEngine.UI.Slider ThresholdObj;

    /// <summary>
    /// Dropdown that will change the type of threshold in Bonsai.
    /// </summary>
    public Dropdown ThresholdTypeObj;
 





    /// <summary>
    /// Disposable of the image data subscription.
    /// </summary>
    private IDisposable imgDetectedSubscription;

    // Use this for initialization
    void Start () {

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


            // If we wanted to get a value before we start we used something like this
            // ThresholdObj.value = Convert.ToSingle(Utils.GetWorkflowProperty(workflowBuilder.Workflow, "ThresholdValue"));

            var workflow = workflowBuilder.Workflow.Build();
            var observableFactory = Expression.Lambda<Func<IObservable<IplImage>>>(workflow).Compile();


           

            imgDetectedSubscription = observableFactory().Subscribe(x => updateImage(x));


            workflowBuilderGraph = workflowBuilder.Workflow;



        }
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

    // If the Threshold value is changed will update the value in Bonsai sending the corresponding value.
    public void OnThresholdChange()
    {
        Utils.SetWorkflowProperty(workflowBuilderGraph, "ThresholdValue", ThresholdObj.value);
    }

    // If the ThresholdType value is changed will update the value in Bonsai sending the corresponding value.
    public void OnThresholdTypeChange()
    {


    Utils.SetWorkflowProperty(workflowBuilderGraph, "ThresholdType", ThresholdTypeObj.value);
 
    }


    // Update is called once per frame
    void Update () {

        if (startCam)
        {
            // Update the texter from the data received since the data can't be changed without being in the mainThread of unity.
            tex.LoadRawTextureData(managedArray);
            tex.Apply();
            GetComponent<RawImage>().texture = tex;
        }
   

    }
}
