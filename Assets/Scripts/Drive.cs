using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Drive : MonoBehaviour
{
    public float speed = 50.0f;
    public float rotationSpeed = 100.0f;
    public float visibleDistance = 200.0f;

    float translationInput;
    float rotationInput;

    List<string> collectedTrainingData = new List<string>();
    StreamWriter trainingDataFile;

    private void Start()
    {
        string path = Application.dataPath + "/trainingData.txt";
        this.trainingDataFile = File.CreateText(path);
    }

    private void OnApplicationQuit()
    {
        foreach(string td in collectedTrainingData)
        {
            this.trainingDataFile.WriteLine(td);
        }
        this.trainingDataFile.Close();
    }

    void Update()
    {
        ComputeCartMovement();
        ComputeDriverVision();
    }

    void ComputeCartMovement()
    {
        this.translationInput = Input.GetAxis("Vertical");
        this.rotationInput = Input.GetAxis("Horizontal");

        float translation = this.translationInput * speed * Time.deltaTime;
        float rotation = this.rotationInput * rotationSpeed * Time.deltaTime;

        this.transform.Translate(0, 0, translation);
        this.transform.Rotate(0, rotation, 0);

        // Debug.DrawRay(this.transform.position, this.transform.forward * visibleDistance, Color.red);
        // Debug.DrawRay(this.transform.position, this.transform.right * visibleDistance, Color.red);
    }

    float Round(float x)
    {
        return (float) System.Math.Round(x, System.MidpointRounding.AwayFromZero) / 2.0f;
    }

    void ComputeDriverVision()
    {
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

        string trainingData = forwardDist + "," +
                              rightDist + "," +
                              leftDist + "," +
                              right45Dist + "," +
                              left45Dist + "," +
                              Round(this.translationInput) + "," +
                              Round(this.rotationInput);

        if(!this.collectedTrainingData.Contains(trainingData))
        {
            this.collectedTrainingData.Add(trainingData);
        }
    }
}
