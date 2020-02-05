using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ANNDrive : MonoBehaviour
{
    ANN ann;
    public float visibleDistance = 50;
    public int epochs = 1000;
    public float speed = 50.0f;
    public float rotationSpeed = 100.0f;

    bool trainingDone = false;
    float trainingProgress = 0;
    // sum of squared errors
    double SSE = 0;
    double lastSEE = 1;

    public float translation;
    public float rotation;

    public bool loadFromFile = true;
    
    // Start is called before the first frame update
    void Start()
    {
        this.ann = new ANN(5, 2, 1, 10, 0.5);
        if(this.loadFromFile)
        {
            LoadWeightsFromFile();
            this.trainingDone = true;
        }
        else
        {
            StartCoroutine(LoadTrainingSet());
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(25, 25, 250, 30), "SSE: " + this.lastSEE);
        GUI.Label(new Rect(25, 40, 250, 30), "Alpha: " + this.ann.alpha);
        GUI.Label(new Rect(25, 55, 250, 30), "Trained: " + this.trainingProgress);
    }

    IEnumerator LoadTrainingSet()
    {
        string path = Application.dataPath + "/trainingData.txt";
        string line;
        if(File.Exists(path))
        {
            int lineCount = File.ReadAllLines(path).Length;
            StreamReader trainingDataFile = File.OpenText(path);
            List<double> calcOutputs = new List<double>();
            List<double> inputs = new List<double>();
            List<double> outputs = new List<double>();

            for(int i = 0; i < epochs; i++)
            {
                // set file pointer to beginning of file
                this.SSE = 0;
                trainingDataFile.BaseStream.Position = 0;
                string currentWeights = this.ann.PrintWeights();

                while((line = trainingDataFile.ReadLine()) != null)
                {
                    string[] data = line.Split(',');
                    // if nothing to be learned ignore this line
                    float thisError = 0;
                    if(System.Convert.ToDouble(data[5]) != 0 && System.Convert.ToDouble(data[6]) != 0)
                    {
                        inputs.Clear();
                        outputs.Clear();
                        inputs.Add(System.Convert.ToDouble(data[0]));
                        inputs.Add(System.Convert.ToDouble(data[1]));
                        inputs.Add(System.Convert.ToDouble(data[2]));
                        inputs.Add(System.Convert.ToDouble(data[3]));
                        inputs.Add(System.Convert.ToDouble(data[4]));

                        double output1 = Map(0, 1, -1, 1, System.Convert.ToSingle(data[5]));
                        outputs.Add(output1);
                        double output2 = Map(0, 1, -1, 1, System.Convert.ToSingle(data[6]));
                        outputs.Add(output2);

                        calcOutputs = this.ann.Train(inputs, outputs);
                        // sum of square errors average
                        thisError = (Mathf.Pow((float)(outputs[0] - calcOutputs[0]), 2) + Mathf.Pow((float)(outputs[1] - calcOutputs[1]), 2)) / 2.0f;
                    }
                    this.SSE += thisError;
                }
                this.trainingProgress = (float)i / (float)epochs;
                this.SSE /= lineCount;

                // if SSE isn't better then reload previous set of weights
                // and decrease alpha
                if(this.lastSEE < this.SSE)
                {
                    this.ann.LoadWeights(currentWeights);
                    this.ann.alpha = Mathf.Clamp((float)this.ann.alpha - 0.001f, 0.01f, 0.9f);
                }
                else
                {
                    this.ann.alpha = Mathf.Clamp((float)this.ann.alpha + 0.001f, 0.01f, 0.9f);
                    this.lastSEE = this.SSE;
                }

                yield return null;
            }
        }
        this.trainingDone = true;
        SaveWeightsToFile();
    }

    void SaveWeightsToFile()
    {
        string path = Application.dataPath + "/weights.txt";
        StreamWriter wf = File.CreateText(path);
        wf.WriteLine(this.ann.PrintWeights());
        wf.Close();
    }

    void LoadWeightsFromFile()
    {
        string path = Application.dataPath + "/weights.txt";
        StreamReader wf = File.OpenText(path);

        if(File.Exists(path))
        {
            string line = wf.ReadLine();
            this.ann.LoadWeights(line);            
        }

        //wf.Close();
    }

    float Map(float newFrom, float newTo, float origFrom, float origTo, float value)
    {
        if (value <= origFrom)
        {
            return newFrom;
        }
        else if (value >= origTo)
        {
            return newTo;
        }
        return (newTo - newFrom) * ((value - origFrom) / (origTo - origFrom)) + newFrom;
    }

    float Round(float x)
    {
        return (float)System.Math.Round(x, System.MidpointRounding.AwayFromZero) / 2.0f;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (!trainingDone) return;

        List<double> calcOutputs = new List<double>();
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();

        // raycasts
        RaycastHit hit;
        float forwardDist = 0;
        float rightDist = 0;
        float leftDist = 0;
        float right45Dist = 0;
        float left45Dist = 0;

        // forward
        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, this.visibleDistance))
        {
            forwardDist = 1 - Round(hit.distance / this.visibleDistance);
            Debug.DrawRay(this.transform.position, this.transform.forward);
        }

        // right
        if (Physics.Raycast(this.transform.position, this.transform.right, out hit, this.visibleDistance))
        {
            rightDist = 1 - Round(hit.distance / this.visibleDistance);
            Debug.DrawRay(this.transform.position, this.transform.right);
        }

        // left
        if (Physics.Raycast(this.transform.position, -this.transform.right, out hit, this.visibleDistance))
        {
            leftDist = 1 - Round(hit.distance / this.visibleDistance);
            Debug.DrawRay(this.transform.position, -this.transform.right);
        }

        // right45
        if (Physics.Raycast(this.transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right, out hit, this.visibleDistance))
        {
            right45Dist = 1 - Round(hit.distance / this.visibleDistance);
            Debug.DrawRay(this.transform.position, Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right);
        }

        // left45
        if (Physics.Raycast(this.transform.position, Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right, out hit, this.visibleDistance))
        {
            left45Dist = 1 - Round(hit.distance / this.visibleDistance);
            Debug.DrawRay(this.transform.position, Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right);
        }

        inputs.Add(forwardDist);
        inputs.Add(rightDist);
        inputs.Add(leftDist);
        inputs.Add(right45Dist);
        inputs.Add(left45Dist);

        outputs.Add(0);
        outputs.Add(0);

        calcOutputs = this.ann.CalcOutput(inputs, outputs);

        float translationInput = Map(-1, 1, 0, 1, (float)calcOutputs[0]);
        float rotationInput = Map(-1, 1, 0, 1, (float)calcOutputs[1]);

        this.translation = translationInput * this.speed * Time.deltaTime;
        this.rotation = rotationInput * this.rotationSpeed * Time.deltaTime;

        print(rotationInput);

        this.transform.Translate(0, 0, this.translation);
        this.transform.Rotate(0, this.rotation, 0);
        */

        if (!trainingDone) return;

        List<double> calcOutputs = new List<double>();
        List<double> inputs = new List<double>();
        List<double> outputs = new List<double>();

        //raycasts
        RaycastHit hit;
        float fDist = 0, rDist = 0, lDist = 0, r45Dist = 0, l45Dist = 0;

        //forward
        if (Physics.Raycast(transform.position, this.transform.forward, out hit, visibleDistance))
        {
            fDist = 1 - Round(hit.distance / visibleDistance);
        }

        //right
        if (Physics.Raycast(transform.position, this.transform.right, out hit, visibleDistance))
        {
            rDist = 1 - Round(hit.distance / visibleDistance);
        }

        //left
        if (Physics.Raycast(transform.position, -this.transform.right, out hit, visibleDistance))
        {
            lDist = 1 - Round(hit.distance / visibleDistance);
        }

        //right 45
        if (Physics.Raycast(transform.position,
                            Quaternion.AngleAxis(-45, Vector3.up) * this.transform.right, out hit, visibleDistance))
        {
            r45Dist = 1 - Round(hit.distance / visibleDistance);
        }

        //left 45
        if (Physics.Raycast(transform.position,
                            Quaternion.AngleAxis(45, Vector3.up) * -this.transform.right, out hit, visibleDistance))
        {
            l45Dist = 1 - Round(hit.distance / visibleDistance);
        }

        inputs.Add(fDist);
        inputs.Add(rDist);
        inputs.Add(lDist);
        inputs.Add(r45Dist);
        inputs.Add(l45Dist);
        outputs.Add(0);
        outputs.Add(0);
        calcOutputs = ann.CalcOutput(inputs, outputs);
        float translationInput = Map(-1, 1, 0, 1, (float)calcOutputs[0]);
        float rotationInput = Map(-1, 1, 0, 1, (float)calcOutputs[1]);
        translation = translationInput * speed * Time.deltaTime;
        rotation = rotationInput * rotationSpeed * Time.deltaTime;
        this.transform.Translate(0, 0, translation);
        this.transform.Rotate(0, rotation, 0);

    }
}
